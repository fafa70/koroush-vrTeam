Imports UvARescue.Agent
Imports UvARescue.Math

Imports System.Math

Public Class FastWeightedScanMatcher
    Inherits WeightedScanMatcher


    Private Const IANGLE_THRESHOLD As Double = PI / 8

    'for tuning the noise error covariance
    Private Const DEFAULT_SIGMAANG As Double = 0.002
    Private Const DEFAULT_SIGMADIST As Double = 10

    'for tuning the corresponde error covariance
    Private Const SIGMA_CORRELATED As Double = 10

    Public Sub New()
    End Sub

    Protected Overrides Function MatchPoints(ByVal points1() As Vector2, ByVal points2() As Vector2, ByVal odometry As Pose2D, ByVal dangle As Double, ByVal filter1() As Boolean, ByVal filter2() As Boolean) As MatchResult

        Dim startTime As DateTime = Now

        Dim J As New Matrix2(0, -1, 1, 0)
        Dim Jt As Matrix2 = J.Transpose


        Dim iangles1() As Double = Me.ComputeIncidenceAngles(points1)
        Dim iangles2() As Double = Me.ComputeIncidenceAngles(points2)


        'setup covariance matrixes
        Dim covs1(points1.Length - 1) As Matrix2
        Dim covs2(points2.Length - 1) As Matrix2
        For i As Integer = 0 To covs1.Length - 1
            covs1(i) = Me.ComputeErrorCovariance(points1(i), iangles1(i), dangle)
        Next


        Dim rawdpose As Pose2D = odometry
        Dim curdpose As Pose2D = rawdpose
        Dim curpoints2() As Vector2 = Nothing
        Dim curpairs(,) As Integer = Nothing

        Dim results As New Queue(Of Double)
        Dim nconverged As Integer = 0

        Dim suminvsumcov As New Matrix2
        Dim midpoint As New Vector2
        Dim sumsumcov As New Matrix2
        Dim transest As New Vector2
        Dim rotest As New Double




        Dim n As Integer = 0
        For n = 1 To MAX_ITERATIONS

            'recompute relative coords for range-scan 2 given the new dpose estimate
            curpoints2 = Me.ToLocal(points2, curdpose)

            'recompute covariances for curpoints2 (= covs2)
            For i As Integer = 0 To covs2.Length - 1
                covs2(i) = Me.ComputeErrorCovariance(curpoints2(i), iangles2(i), dangle)
            Next


            'find all correlated point-pairs
            curpairs = Me.CorrelatePoints(points1, curpoints2, filter1, filter2)

            If curpairs.Length > 0 Then

                'these vars will hold summaries for a single iteration
                Dim sumipoint1 As New Vector2
                Dim sumipoint2 As New Vector2
                Dim sumirotmid As New Matrix2
                Dim sumrotmid As New Vector2
                Dim sumerror As Double = 0.0
                suminvsumcov = New Matrix2

                For p As Integer = 0 To curpairs.GetLength(0) - 1

                    'get corresponding points and error covariances ...
                    Dim point1 As Vector2 = points1(curpairs(p, 0))
                    Dim point2 As Vector2 = curpoints2(curpairs(p, 1))
                    Dim cov1 As Matrix2 = covs1(curpairs(p, 0))
                    Dim cov2 As Matrix2 = covs2(curpairs(p, 1))

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

                For p As Integer = 0 To curpairs.GetLength(0) - 1

                    'get corresponding points and error covariances ...
                    Dim point1 As Vector2 = points1(curpairs(p, 0))
                    Dim point2 As Vector2 = curpoints2(curpairs(p, 1))
                    Dim cov1 As Matrix2 = covs1(curpairs(p, 0))
                    Dim cov2 As Matrix2 = covs2(curpairs(p, 1))

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

                'check for convergence
                Dim converged As Boolean = False
                For Each result As Double In results
                    If sumerror = 0 Then
                        converged = converged OrElse (result < ERROR_THRESHOLD)
                    Else
                        converged = converged OrElse (Abs(result / sumerror - 1) < ERROR_THRESHOLD)
                    End If
                Next

                results.Enqueue(sumerror)
                While results.Count > RESULT_MAXCONSIDERED
                    results.Dequeue()
                End While

                If converged Then
                    nconverged += 1
                End If

            End If

            If nconverged >= RESULT_CONVERGED Then
                Exit For
            End If

        Next



        'compute the matching covariance
        Dim mcov As New Matrix3
        Dim num As Integer = 0

        If Not IsNothing(curpairs) Then

            num = curpairs.GetLength(0)

            If Not IsNothing(curpoints2) Then

                suminvsumcov = New Matrix2
                Dim sumrtval As Double = 0
                Dim sumjqval As New Vector2

                For p As Integer = 0 To curpairs.GetLength(0) - 1

                    'get corresponding points and error covariances ...
                    Dim point1 As Vector2 = points1(curpairs(p, 0))
                    Dim point2 As Vector2 = curpoints2(curpairs(p, 1))
                    Dim cov1 As Matrix2 = covs1(curpairs(p, 0))
                    Dim cov2 As Matrix2 = covs2(curpairs(p, 1))

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

        Return New MatchResult(rawdpose, curdpose, mcov, n, num, CInt(duration.TotalMilliseconds), Me.HasConverged(points1.Length, num, n))

    End Function

End Class
