Imports UvARescue.Agent
Imports UvARescue.Math

Imports System.Math


Public Class MbIcpScanMatcher
    Inherits IcpScanMatcher

    'In Minguez' code the 'magical' L is set to 3 meters, perhaps because this value is close to PI ..?
    Protected Const LSQ_M As Single = 3.0 * 3.0
    Protected Const LSQ_MM As Single = 3000.0 * 3000.0

    Protected Friend Overrides Function MatchPoints(ByVal manifold As Manifold, ByVal points1() As Vector2, ByVal points2() As Vector2, ByVal odometry As Pose2D, ByVal dangle As Double, ByVal filter1() As Boolean, ByVal filter2() As Boolean) As MatchResult

        Dim startTime As DateTime = Now

        Dim rawdpose As Pose2D = odometry
        Dim curdpose As Pose2D = rawdpose
        Dim curpoints2() As Vector2 = Nothing
        Dim curpairs() As Correlation(Of Vector2) = Nothing

        Dim uavg As New Vector2
        Dim sumerror As Double = 0.0
        Dim sumangcov As Double = 0.0

        Dim results As New Queue(Of Double)
        Dim nconverged As Integer = 0



        Dim n As Integer = 0
        For n = 1 To MAX_ITERATIONS

            'recompute relative coords for range-scan 2 given the new dpose estimate
            curpoints2 = Me.ToLocal(points2, curdpose)
            'find all correlated point-pairs
            curpairs = Me.CorrelatePointsDM(manifold, points1, curpoints2, filter1, filter2)

            If curpairs.Length > 0 Then

                'these vars will hold summaries for a single iteration
                Dim sumpoint1 As New Vector2
                Dim sumpoint2 As New Vector2


                Dim A As New MathNet.Numerics.LinearAlgebra.Matrix(3, 3)
                Dim b As New MathNet.Numerics.LinearAlgebra.Matrix(3, 1)

                sumerror = 0
                sumangcov = 0.0

                For Each pair As Correlation(Of Vector2) In curpairs

                    Dim point1 As Vector2 = pair.Point1
                    Dim point2 As Vector2 = pair.Point2
                    Dim dpoint As Vector2 = point2 - point1
                    Dim mpoint As Vector2 = (point1 + point2) / 2

                    sumpoint1 += point1
                    sumpoint2 += point2
                    sumerror += dpoint * dpoint
                    sumangcov += mpoint * mpoint

                    'MbICP needs the points in meters
                    Dim pix As Double = point1.X / 1000
                    Dim piy As Double = point1.Y / 1000
                    Dim cix As Double = point2.X / 1000
                    Dim ciy As Double = point2.Y / 1000

                    Dim ki As Double = pix * pix + piy * piy + LSQ_M
                    Dim cxpxPcypy As Double = cix * pix + ciy * piy
                    Dim cxpyMcypx As Double = cix * piy - ciy * pix

                    A(0, 0) += 1.0 - piy * piy / ki
                    A(0, 1) += pix * piy / ki
                    A(0, 2) += -ciy + piy / ki * cxpxPcypy
                    A(1, 1) += 1.0 - pix * pix / ki
                    A(1, 2) += cix - pix / ki * cxpxPcypy
                    A(2, 2) += cix * cix + ciy * ciy - cxpxPcypy * cxpxPcypy / ki

                    b(0, 0) += cix - pix - piy / ki * cxpyMcypx
                    b(1, 0) += ciy - piy + pix / ki * cxpyMcypx
                    b(2, 0) += (cxpxPcypy / ki - 1.0) * cxpyMcypx

                Next

                'Complete the A-matrix by assigning the symmetric portions of it
                A(1, 0) = A(0, 1)
                A(2, 0) = A(0, 2)
                A(2, 1) = A(1, 2)

                'compute the transformation that minimizes 
                A = A.Inverse
                A.Multiply(-1)

                'qmin will be in meters
                Dim qmin As MathNet.Numerics.LinearAlgebra.Matrix = A * b

                'construct pose in millimeters
                Dim qminpose As New Pose2D(CType(qmin(0, 0) * 1000, Single), CType(qmin(1, 0) * 1000, Single), CType(qmin(2, 0), Single))

                'check for convergence
                Dim converged As Boolean = False
                For Each result As Double In results
                    If sumerror = 0 Then
                        converged = converged OrElse (result < RESULTS_MAXERROR)
                    Else
                        converged = converged OrElse (Abs(result / sumerror - 1) < RESULTS_MAXERROR)
                    End If
                Next

                results.Enqueue(sumerror)
                While results.Count > RESULTS_CONSIDERED
                    results.Dequeue()
                End While

                If converged Then
                    nconverged += 1
                End If


                'BELOW IS THE ORIGINAL CONVERGENCY CHECK THAT MINGUEZ USED
                'Dim threshold As Double = 0.0001 'threshold copied from Minguez' matlab code
                'converged = True
                'converged = converged AndAlso qmin(0, 0) < threshold
                'converged = converged AndAlso qmin(1, 0) < threshold
                'converged = converged AndAlso qmin(2, 0) < threshold

                'construct new curdpose by 'adding' qminpose to current curdpose
                curdpose = qminpose.ToGlobal(curdpose)

            End If

            If nconverged >= RESULTS_CONVERGED Then
                Exit For
            End If


            'BELOW IS THE ORIGINAL CONVERGENCY CHECK THAT MINGUEZ USED
            'If converged Then
            '    Exit For
            'End If

        Next

        Dim mcov As New Matrix3
        Dim mdist As Double = 0
        Dim num As Integer = 0

        If Not IsNothing(curpairs) Then

            'compute distance
            For Each pair As Correlation(Of Vector2) In curpairs
                Dim dpoint As Vector2 = pair.Point2 - pair.Point1
                mdist += dpoint.X ^ 2 + dpoint.Y ^ 2
            Next
            mdist = mdist ^ 0.5

            num = curpairs.GetLength(0)

            Dim cov As New MathNet.Numerics.LinearAlgebra.Matrix(3, 3)
            cov(0, 0) = num
            cov(0, 1) = 0
            cov(0, 2) = -num * uavg(1)
            cov(1, 0) = 0
            cov(1, 1) = num
            cov(1, 2) = num * uavg(0)
            cov(2, 0) = -num * uavg(1)
            cov(2, 1) = num * uavg(0)
            cov(2, 2) = sumangcov

            cov = (sumerror / (2 * num - 3)) * cov.Inverse

            mcov(0, 0) = CType(cov(0, 0), Single)
            mcov(0, 1) = CType(cov(0, 1), Single)
            mcov(0, 2) = CType(cov(0, 2), Single)
            mcov(1, 0) = CType(cov(1, 0), Single)
            mcov(1, 1) = CType(cov(1, 1), Single)
            mcov(1, 2) = CType(cov(1, 2), Single)
            mcov(2, 0) = CType(cov(2, 0), Single)
            mcov(2, 1) = CType(cov(2, 1), Single)
            mcov(2, 2) = CType(cov(2, 2), Single)

        End If

        Dim untilTime As DateTime = Now
        Dim duration As TimeSpan = untilTime - startTime

        Return New MatchResult(rawdpose, curdpose, mcov, mdist, n, num, CInt(duration.TotalMilliseconds), Me.HasConverged(points1.Length, num, n))

    End Function

    Protected Overrides Function ComputeSquaredDistance(ByVal point1 As Math.Vector2, ByVal point2 As Math.Vector2) As Double
        Dim dx As Double = point2.X - point1.X
        Dim dy As Double = point2.Y - point1.Y
        Dim tmp As Double = dx * point1.Y - dy * point1.X
        Return dx ^ 2 + dy ^ 2 - (tmp ^ 2 / (point1.Y ^ 2 + point1.X ^ 2 + LSQ_MM))
    End Function

End Class
