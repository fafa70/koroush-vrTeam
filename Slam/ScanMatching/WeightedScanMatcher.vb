Imports UvARescue.Agent
Imports UvARescue.Math
Imports UvARescue.Tools

Imports System.Math

''' <summary>
''' Implementaton fot the Weighted scanmatcher by Pfister et al.
''' </summary>
''' <remarks></remarks>
Public Class WeightedScanMatcher
    Inherits IcpScanMatcher

    Private _manifold As Manifold
    Private Const IANGLE_THRESHOLD As Double = PI / 8

    'for tuning the noise error covariance
    Private Const DEFAULT_SIGMAANG As Double = 0.002
    Private Const DEFAULT_SIGMADIST As Double = 10

    'for tuning the corresponde error covariance
    Private Const SIGMA_CORRELATED As Double = 10


    Protected Friend Overrides Function MatchPoints(ByVal manifold As Manifold, ByVal points1() As Vector2, ByVal points2() As Vector2, ByVal odometry As Pose2D, ByVal dangle As Double, ByVal filter1() As Boolean, ByVal filter2() As Boolean) As MatchResult

        Dim startTime As DateTime = Now

        Me._manifold = manifold

        Dim J As New Matrix2(0, -1, 1, 0)
        Dim Jt As Matrix2 = J.Transpose


        Dim iangles1() As Double = Me.ComputeIncidenceAngles(points1)
        Dim iangles2() As Double = Me.ComputeIncidenceAngles(points2)


        'covariance matrixes
        Dim ecovs1(points1.Length - 1) As Matrix2
        Dim ecovs2(points2.Length - 1) As Matrix2

        For i As Integer = 0 To ecovs1.Length - 1
            ecovs1(i) = Me.ComputeErrorCovariance(points1(i), iangles1(i), dangle)
        Next


        Dim rawdpose As Pose2D = odometry
        Dim curdpose As Pose2D = rawdpose
        Dim curpoints2() As Vector2 = Nothing
        Dim curpairs() As Correlation(Of Vector2) = Nothing

        'indexers, for fast access to covariance matrices
        Dim index As Integer = 0
        Dim index1() As Integer = Nothing
        Dim index2() As Integer = Nothing


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
            For i As Integer = 0 To ecovs2.Length - 1
                ecovs2(i) = Me.ComputeErrorCovariance(curpoints2(i), iangles2(i), dangle)
            Next


            'find all correlated point-pairs
            curpairs = Me.CorrelatePointsQT(manifold, points1, curpoints2, filter1, filter2)

            If curpairs.Length > 0 Then

                'recompute indexes only once per iteration
                ReDim index1(curpairs.Length - 1)
                ReDim index2(curpairs.Length - 1)
                index = 0
                For Each pair As Correlation(Of Vector2) In curpairs
                    index1(index) = Array.IndexOf(points1, pair.Point1)
                    index2(index) = Array.IndexOf(curpoints2, pair.Point2)
                    index += 1
                Next

                'these vars will hold summaries for a single iteration
                Dim sumipoint1 As New Vector2
                Dim sumipoint2 As New Vector2
                Dim sumirotmid As New Matrix2
                Dim sumrotmid As New Vector2
                Dim sumerror As Double = 0.0
                suminvsumcov = New Matrix2

                index = 0
                For Each pair As Correlation(Of Vector2) In curpairs

                    'get corresponding points and error covariances ...
                    Dim point1 As Vector2 = pair.Point1
                    Dim point2 As Vector2 = pair.Point2
                    Dim cov1 As Matrix2 = ecovs1(index1(index))
                    Dim cov2 As Matrix2 = ecovs2(index2(index))

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

                    index += 1
                Next

                'get rotational midpoint and translation estimate
                midpoint = sumirotmid.Invert * sumrotmid
                sumsumcov = suminvsumcov.Invert
                transest = sumsumcov * (sumipoint2 - sumipoint1)



                Dim sumnval As Double = 0
                Dim sumdval As Double = 0

                index = 0
                For Each pair As Correlation(Of Vector2) In curpairs

                    'get corresponding points and error covariances ...
                    Dim point1 As Vector2 = pair.Point1
                    Dim point2 As Vector2 = pair.Point2
                    Dim cov1 As Matrix2 = ecovs1(index1(index))
                    Dim cov2 As Matrix2 = ecovs2(index2(index))

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

                    index += 1
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

            If Not IsNothing(curpoints2) Then

                suminvsumcov = New Matrix2
                Dim sumrtval As Double = 0
                Dim sumjqval As New Vector2

                index = 0
                For Each pair As Correlation(Of Vector2) In curpairs

                    'get corresponding points and error covariances ...
                    Dim point1 As Vector2 = pair.Point1
                    Dim point2 As Vector2 = pair.Point2
                    Dim cov1 As Matrix2 = ecovs1(index1(index))
                    Dim cov2 As Matrix2 = ecovs2(index2(index))

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



    Protected Overridable Function ComputeIncidenceAngles(ByVal points As Vector2()) As Double()

        Dim dpoints(points.Length - 2) As Vector2
        Dim dangles(points.Length - 2) As Double

        Dim dpoint As Vector2

        For i As Integer = 0 To dangles.Length - 1
            dpoint = points(i + 1) - points(i)
            dpoints(i) = dpoint
            dangles(i) = Atan2(dpoint.Y, dpoint.X)
        Next

        Dim cmpangles(dpoints.Length - 2) As Double
        Dim avgangles(dpoints.Length - 2) As Double
        Dim dangle As Double
        Dim iwhole As Integer

        For i As Integer = 0 To cmpangles.Length - 1
            dangle = dangles(i) - dangles(i + 1)
            iwhole = CInt(Floor((dangle + PI) / (2 * PI)))
            dangle = dangle - 2 * PI * iwhole
            cmpangles(i) = dangle
            avgangles(i) = dangles(i) - 0.5 * dangle
        Next

        Dim iangles(points.Length - 1) As Double
        Dim iangle As Double

        Dim point As Vector2
        Dim angle As Double

        For i As Integer = 1 To iangles.Length - 2
            If Abs(cmpangles(i - 1)) < IANGLE_THRESHOLD Then
                point = points(i)
                angle = Atan2(point.Y, point.X)
                iangle = angle - avgangles(i - 1)
                iangle = iangle - 2 * PI * Floor((iangle + PI) / (2 * PI))
                iangle = iangle - PI * Floor((iangle + PI / 2) / (PI))
            Else
                iangle = 0
            End If

            iangles(i) = iangle

        Next

        Return iangles
    End Function

    Protected Overridable Function ComputeErrorCovariance(ByVal point As Vector2, ByVal iangle As Double, ByVal dangle As Double) As Matrix2
        Dim nCov As Matrix2 = Me.ComputeNoiseErrorCovariance(point)
        Dim cCov As Matrix2 = Me.ComputeCorrespondenceErrorCovariance(point, iangle, dangle)
        Dim eCov As Matrix2 = nCov + cCov
        Return eCov
    End Function

    Protected Overridable Function ComputeNoiseErrorCovariance(ByVal point As Vector2) As Matrix2

        Dim angle As Double = Atan2(point.Y, point.X)
        Dim dist As Double = Sqrt(Pow(point.X, 2) + Pow(point.Y, 2))

        Dim c As Double = Cos(angle)
        Dim s As Double = Sin(angle)
        Dim a As Double = Pow(DEFAULT_SIGMADIST, 2)
        Dim b As Double = Pow(DEFAULT_SIGMAANG, 2) * Pow(dist, 2)

        Dim cov As New Matrix2
        cov(0, 0) = CType(Pow(c, 2) * a + Pow(s, 2) * b, Single)
        cov(0, 1) = CType(c * s * a - c * s * b, Single)
        cov(1, 0) = CType(cov(0, 1), Single)
        cov(1, 1) = CType(Pow(s, 2) * a + Pow(c, 2) * b, Single)
        Return cov

    End Function

    Protected Overridable Function ComputeCorrespondenceErrorCovariance(ByVal point As Vector2, ByVal iangle As Double, ByVal dangle As Double) As Matrix2

        Dim wasZero As Boolean = False
        If iangle = 0 Then
            wasZero = True
            iangle = IANGLE_THRESHOLD
        End If

        Dim angle As Double = Atan2(point.Y, point.X)
        Dim dist As Double = Sqrt(Pow(point.X, 2) + Pow(point.Y, 2))
        Dim dmin As Double = (dist * Sin(dangle)) / Sin(iangle + dangle)
        Dim dmax As Double = (dist * Sin(dangle)) / Sin(iangle - dangle)
        Dim dsum As Double = dmin + dmax
        Dim cerr As Double = SIGMA_CORRELATED * (Pow(dmin, 3) + Pow(dmax, 3)) / (3 * dsum)

        Dim cov As New Matrix2
        If wasZero Then
            cov(0, 0) = CType(cerr, Single)
            cov(0, 1) = 0
            cov(1, 0) = 0
            cov(1, 1) = CType(cerr, Single)

        Else
            Dim dang As Double = angle - iangle
            Dim c As Double = Cos(dang)
            Dim s As Double = Sin(dang)

            cov(0, 0) = CType(Pow(c, 2) * cerr, Single)
            cov(0, 1) = CType(c * s * cerr, Single)
            cov(1, 0) = CType(cov(0, 1), Single)
            cov(1, 1) = CType(Pow(s, 2) * cerr, Single)

        End If

        Return cov

    End Function

End Class
