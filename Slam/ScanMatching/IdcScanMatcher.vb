Imports UvARescue.Agent
Imports UvARescue.Math
Imports UvARescue.Tools

Imports System.Math


Public Class IdcScanMatcher
    Inherits IcpScanMatcher

    Protected Friend Overrides Function MatchPoints(ByVal manifold As Agent.Manifold, ByVal points1() As Math.Vector2, ByVal points2() As Math.Vector2, ByVal seed As Math.Pose2D, ByVal dangle As Double, ByVal filter1() As Boolean, ByVal filter2() As Boolean) As MatchResult

        Dim startTime As DateTime = Now

        Dim rawdpose As Pose2D = seed
        Dim curdpose As Pose2D = rawdpose
        Dim curpoints2() As Vector2 = Nothing
        Dim curpairs() As Correlation(Of Vector2) = Nothing

        Dim results As New Queue(Of Double)
        Dim nconverged As Integer = 0

        Dim uavg As New Vector2
        Dim sumerror As Double = 0.0
        Dim sumangcov As Double = 0.0


        Dim n As Integer = 0
        For n = 1 To MAX_ITERATIONS

            'recompute relative coords for range-scan 2 given the new dpose estimate
            curpoints2 = Me.ToLocal(points2, curdpose)
            'find all correlated point-pairs
            curpairs = Me.CorrelatePointsQT(manifold, points1, curpoints2, filter1, filter2)

            If curpairs.Length > 0 Then

                'these vars will hold summaries for a single iteration
                Dim sumpoint1 As New Vector2
                Dim sumpoint2 As New Vector2

                For Each pair As Correlation(Of Vector2) In curpairs
                    sumpoint1 += pair.Point1
                    sumpoint2 += pair.Point2
                Next

                Dim u1 As Vector2 = sumpoint1 / curpairs.GetLength(0)
                Dim u2 As Vector2 = sumpoint2 / curpairs.GetLength(0)
                uavg = (u1 + u2) / 2

                sumpoint1 = New Vector2
                sumpoint2 = New Vector2
                sumerror = 0.0
                sumangcov = 0.0

                For Each pair As Correlation(Of Vector2) In curpairs

                    Dim point1 As Vector2 = pair.Point1
                    Dim point2 As Vector2 = pair.Point2

                    sumpoint1 += New Vector2( _
                        (point1(0) - u1(0)) * (point2(0) - u2(0)), _
                        (point1(1) - u1(1)) * (point2(1) - u2(1)))
                    sumpoint2 += New Vector2( _
                        (point1(0) - u1(0)) * (point2(1) - u2(0)), _
                        (point1(1) - u1(1)) * (point2(0) - u2(1)))

                    Dim dpoint As Vector2 = point1 - point2
                    Dim mpoint As Vector2 = (point1 + point2) / 2

                    sumerror += dpoint * dpoint
                    sumangcov += mpoint * mpoint

                Next

                'update odometry estimate
                Dim rotest As Double = Atan2(sumpoint2(0) - sumpoint2(1), sumpoint1(0) + sumpoint1(1))
                Dim rotmx As New TMatrix2D(rotest)
                Dim transest As Vector2 = u2 - rotmx * u1

                curdpose = New Pose2D(curdpose.Position - transest, curdpose.Rotation - rotest)
                rotmx = New TMatrix2D(-rotest)
                curdpose = rotmx * curdpose


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

            End If

            If nconverged >= RESULTS_CONVERGED Then
                Exit For
            End If

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

            Try
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
            Catch ex As Exception
                Console.WriteLine("Error occurred while creating covariance matrix.")
                Console.WriteLine(ex.Message & vbNewLine & ex.StackTrace)
            End Try

        End If

        Dim untilTime As DateTime = Now
        Dim duration As TimeSpan = untilTime - startTime

        Return New MatchResult(rawdpose, curdpose, mcov, mdist, n, num, CInt(duration.TotalMilliseconds), Me.HasConverged(points1.Length, num, n))

    End Function

End Class
