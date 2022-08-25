Imports UvARescue.Agent
Imports UvARescue.Math
Imports UvARescue.Tools

' according to the Generalized-ICP paper, standard ICP is like the WSM with the sumcov set to the I-matrix
' href=http://www.roboticsproceedings.org/rss05/p21.pdf
Imports System.Math

Public Class QuadIcpScanMatcher
    Inherits IcpScanMatcher

    Public Overrides Function Match(ByVal manifold As Manifold, ByVal patch1 As Patch, ByVal patch2 As Patch, ByVal seed As Pose2D) As MatchResult

        Dim startTime As DateTime = Now


        Dim J As New Matrix2(0, -1, 1, 0)
        Dim Jt As Matrix2 = J.Transpose

        Dim rawdpose As Pose2D = seed
        Dim curdpose As Pose2D = rawdpose

        Dim curpairs() As Correlation(Of Vector2) = Nothing
        Dim points1() As Vector2 = Nothing
        Dim points2() As Vector2 = Nothing
        Dim covs1() As Matrix2 = Nothing
        Dim covs2() As Matrix2 = Nothing



        Dim results As New Queue(Of Double)
        Dim nconverged As Integer = 0

        Dim suminvsumcov As New Matrix2
        Dim midpoint As New Vector2
        Dim sumsumcov As New Matrix2
        Dim transest As New Vector2
        Dim rotest As New Double


        Dim n As Integer = 0
        For n = 1 To MAX_ITERATIONS

            'find all correlated point-pairs
            curpairs = Me.CorrelatePointsGlobally(manifold, patch2)

            If curpairs.Length > 0 Then

                Dim tmx As TMatrix2D = patch1.EstimatedOrigin.ToLocalMatrix
                ReDim points1(curpairs.Length - 1)
                ReDim points2(curpairs.Length - 1)
                ReDim covs1(curpairs.Length - 1)
                ReDim covs2(curpairs.Length - 1)

                'Dim c As Double = Cos(curdpose.Rotation)
                'Dim s As Double = Sin(curdpose.Rotation)

                For i As Integer = 0 To curpairs.Length - 1
                    With curpairs(i)
                        points1(i) = tmx * .Point1
                        points2(i) = tmx * .Point2

                        'this is the only modification compared to WSM
                        Dim cov1 As New Matrix2(1, 0, 0, 1)
                        Dim cov2 As New Matrix2(0, 0, 0, 0)

                        covs1(i) = cov1
                        covs2(i) = cov2

                    End With
                Next


                'these vars will hold summaries for a single iteration
                Dim sumipoint1 As New Vector2
                Dim sumipoint2 As New Vector2
                Dim sumirotmid As New Matrix2
                Dim sumrotmid As New Vector2
                Dim sumerror As Double = 0.0
                suminvsumcov = New Matrix2

                For i As Integer = 0 To curpairs.Length - 1

                    'get corresponding points and error covariances ...
                    Dim point1 As Vector2 = points1(i)
                    Dim point2 As Vector2 = points2(i)
                    Dim cov1 As Matrix2 = covs1(i)
                    Dim cov2 As Matrix2 = covs2(i)

                    'translation estimate ...
                    Dim sumcov As Matrix2 = cov1 + cov2
                    Dim invsumcov As Matrix2 = sumcov.Invert

                    suminvsumcov = suminvsumcov + invsumcov
                    sumipoint1 = sumipoint1 + invsumcov * point1
                    sumipoint2 = sumipoint2 + invsumcov * point2

                    'error metric ...
                    Dim dpoint As Vector2 = point1 - point2
                    sumerror = sumerror + (dpoint * invsumcov * dpoint)

                    'rotation estimate ...
                    Dim irotmid As Matrix2 = Jt * invsumcov * J
                    sumirotmid = sumirotmid + 2 * irotmid
                    sumrotmid = sumrotmid + irotmid * point1 + irotmid * point2

                Next

                'get rotational midpoint and translation estimate
                midpoint = sumirotmid.Invert * sumrotmid
                sumsumcov = suminvsumcov.Invert
                transest = sumsumcov * (sumipoint2 - sumipoint1)



                Dim sumnval As Double = 0
                Dim sumdval As Double = 0

                For i As Integer = 0 To curpairs.Length - 1

                    'get corresponding points and error covariances ...
                    Dim point1 As Vector2 = points1(i)
                    Dim point2 As Vector2 = points2(i)
                    Dim cov1 As Matrix2 = covs1(i)
                    Dim cov2 As Matrix2 = covs2(i)

                    'apply estimated midpoint and translation
                    point1 = point1 - midpoint
                    point2 = point2 - transest - midpoint

                    Dim sumcov As Matrix2 = cov1 + cov2
                    Dim invsumcov As Matrix2 = sumcov.Invert

                    'update numerator and denominator summaries
                    Dim dpoint As Vector2 = point2 - point1
                    Dim nval As Double = dpoint * invsumcov * J * point2
                    Dim dval As Double = dpoint * invsumcov * point2 + point2 * J * invsumcov * J * point2

                    sumnval = sumnval + nval
                    sumdval = sumdval + dval

                Next

                'get rotational estimate
                rotest = -sumnval / sumdval

                'update odometry estimate
                Dim vdpose As Vector2 = curdpose.Position - transest - midpoint
                Dim rotmx As New TMatrix2D(-rotest)
                vdpose = rotmx * vdpose
                vdpose = vdpose + midpoint
                curdpose = New Pose2D(vdpose, curdpose.Rotation - rotest)

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





        'compute the matching covariance
        Dim mcov As New Matrix3
        Dim mdist As Double = 0
        Dim num As Integer = 0

        If Not IsNothing(curpairs) Then

            num = curpairs.GetLength(0)

            If Not IsNothing(points2) Then

                suminvsumcov = New Matrix2
                Dim sumrtval As Double = 0
                Dim sumjqval As New Vector2

                For i As Integer = 0 To curpairs.Length - 1

                    'get corresponding points and error covariances ...
                    Dim point1 As Vector2 = points1(i)
                    Dim point2 As Vector2 = points2(i)
                    Dim cov1 As Matrix2 = covs1(i)
                    Dim cov2 As Matrix2 = covs2(i)

                    'compute distance
                    Dim dpoint As Vector2 = point2 - point1
                    mdist += dpoint.X ^ 2 + dpoint.Y ^ 2


                    'apply estimated midpoint and translation
                    point1 = point1 - midpoint
                    point2 = point2 - transest - midpoint


                    Dim sumcov As Matrix2 = cov1 + cov2
                    Dim invsumcov As Matrix2 = sumcov.Invert
                    suminvsumcov = suminvsumcov + invsumcov

                    'update rtval and jqval summaries
                    Dim rtval As Double = point2 * J * invsumcov * J * point2
                    Dim jqval As Vector2 = invsumcov * J * point2
                    sumrtval = sumrtval + rtval
                    sumjqval = sumjqval + jqval

                Next

                mdist = mdist ^ 0.5

                sumrtval = -(1 / sumrtval)

                Dim prt As Vector2 = -sumrtval * suminvsumcov.Invert * sumjqval

                mcov(0, 0) = sumsumcov(0, 0)
                mcov(0, 1) = sumsumcov(0, 1)
                mcov(1, 0) = sumsumcov(1, 0)
                mcov(1, 1) = sumsumcov(1, 1)
                mcov(0, 2) = prt.X
                mcov(2, 0) = prt.X
                mcov(2, 1) = prt.Y
                mcov(1, 2) = prt.Y
                mcov(2, 2) = sumrtval

                Dim S As New Matrix3
                S.LoadIdentity()
                S(0, 2) = midpoint.Y
                S(1, 2) = -midpoint.X

                mcov = New Matrix3(S * mcov * S.Transpose)

            End If

        End If

        Dim untilTime As DateTime = Now
        Dim duration As TimeSpan = untilTime - startTime

        Return New MatchResult(rawdpose, curdpose, mcov, mdist, n, num, CInt(duration.TotalMilliseconds), Me.HasConverged(points1.Length, num, n))

    End Function


End Class
