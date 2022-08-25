Imports UvARescue.Agent
Imports UvARescue.Math
Imports UvARescue.Tools

Imports System.Math

Public Class QuadIdcScanMatcher
    Inherits IdcScanMatcher

    Public Overrides Function Match(ByVal manifold As Manifold, ByVal patch1 As Patch, ByVal patch2 As Patch, ByVal seed As Pose2D) As MatchResult

        Dim startTime As DateTime = Now

        Dim rawdpose As Pose2D = seed
        Dim curdpose As Pose2D = rawdpose

        Dim curpairs() As Correlation(Of Vector2) = Nothing
        Dim points1() As Vector2 = Nothing
        Dim points2() As Vector2 = Nothing

        Dim results As New Queue(Of Double)
        Dim nconverged As Integer = 0

        Dim uavg As New Vector2
        Dim sumerror As Double = 0.0
        Dim sumangcov As Double = 0.0


        Dim n As Integer = 0
        For n = 1 To MAX_ITERATIONS

            'find all correlated point-pairs
            curpairs = Me.CorrelatePointsGlobally(manifold, patch2)

            If curpairs.Length > 0 Then

                Dim tmx As TMatrix2D = patch1.EstimatedOrigin.ToLocalMatrix
                ReDim points1(curpairs.Length - 1)
                ReDim points2(curpairs.Length - 1)
                For i As Integer = 0 To curpairs.Length - 1
                    With curpairs(i)
                        points1(i) = tmx * .Point1
                        points2(i) = tmx * .Point2
                    End With
                Next


                'these vars will hold summaries for a single iteration
                Dim sumpoint1 As New Vector2
                Dim sumpoint2 As New Vector2

                For i As Integer = 0 To curpairs.Length - 1
                    sumpoint1 += points1(i)
                    sumpoint2 += points2(i)
                Next

                Dim u1 As Vector2 = sumpoint1 / curpairs.GetLength(0)
                Dim u2 As Vector2 = sumpoint2 / curpairs.GetLength(0)
                uavg = (u1 + u2) / 2

                sumpoint1 = New Vector2
                sumpoint2 = New Vector2
                sumerror = 0.0
                sumangcov = 0.0

                For i As Integer = 0 To curpairs.Length - 1

                    Dim point1 As Vector2 = points1(i)
                    Dim point2 As Vector2 = points2(i)

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

                patch2.EstimatedOrigin = curdpose.ToGlobal(patch1.EstimatedOrigin)

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

End Class
