Imports UvARescue.Agent
Imports UvARescue.Math
Imports UvARescue.Tools

Imports System.Math

Public Class IcpScanMatcher
    Implements IScanMatcher


    'for convenience 
    Public Const PI_2 As Double = PI / 2


    'break/convergence thresholds
    Protected MAX_ITERATIONS As Integer = 80
    Protected RESULTS_CONSIDERED As Integer = 10 'max num of results to consider 
    Protected RESULTS_CONVERGED As Integer = 8 'how many good results before converged
    Protected RESULTS_MAXERROR As Double = 0.01 'consider an iteration converged when rms below this


    'maximum distance within which to detect correlations
    Protected MAX_CORRELATIONDISTANCE As Single = 2000 / 4 'mm


    Protected Overridable Sub ApplyConfig(ByVal config As Tools.Config) Implements IScanMatcher.ApplyConfig
        Me.RESULTS_MAXERROR = Double.Parse(config.GetConfig("slam", "error-threshold", CStr(Me.RESULTS_MAXERROR)))
        Me.MAX_CORRELATIONDISTANCE = Single.Parse(config.GetConfig("slam", "correlation-maxdistance", CStr(Me.MAX_CORRELATIONDISTANCE)))
    End Sub



    Public Overridable Function Match(ByVal manifold As Manifold, ByVal patch1 As Patch, ByVal patch2 As Patch, ByVal seed As Math.Pose2D) As MatchResult Implements IScanMatcher.Match
        Return Me.Match(manifold, patch1.Scan, patch2.Scan, seed)
    End Function



    Public Function Match(ByVal manifold As Manifold, ByVal scan1 As ScanObservation, ByVal scan2 As ScanObservation, ByVal seed As Pose2D) As MatchResult

        If scan1.Length < 2 OrElse scan2.Length < 2 Then _
            Throw New InvalidOperationException("Cannot match scans with fewer than 2 scanpoints")

        Dim points1() As Vector2
        Dim points2() As Vector2

        If scan1.FieldOfView = scan2.FieldOfView AndAlso scan1.Resolution = scan2.Resolution Then
            'same scanner setup, reuse angles to save redundant computation
            Dim angles() As Double = Me.ComputeScanAngles(scan1)
            points1 = Me.ToPoints(scan1, angles)
            points2 = Me.ToPoints(scan2, angles)
        Else
            'differing scanner setup, compute angles for each scan individually
            points1 = Me.ToPoints(scan1, Me.ComputeScanAngles(scan1))
            points2 = Me.ToPoints(scan2, Me.ComputeScanAngles(scan2))
        End If


        Dim filter1() As Boolean = Me.ToFilter(scan1)
        Dim filter2() As Boolean = Me.ToFilter(scan2)
        Dim dangle As Double = scan1.FieldOfView / scan1.Length

        Return Me.MatchPoints(manifold, points1, points2, seed, dangle, filter1, filter2)

    End Function


    Protected Friend Overridable Function MatchPoints(ByVal manifold As Manifold, ByVal points1() As Vector2, ByVal points2() As Vector2, ByVal seed As Pose2D, ByVal dangle As Double, ByVal filter1() As Boolean, ByVal filter2() As Boolean) As MatchResult

        'this is the simplified QWSM implementation, with I-matrix as ecov
        Dim startTime As DateTime = Now

        Dim J As New Matrix2(0, -1, 1, 0)
        Dim Jt As Matrix2 = J.Transpose


        'covariance matrixes
        Dim ecovs1(points1.Length - 1) As Matrix2
        Dim ecovs2(points2.Length - 1) As Matrix2

        For i As Integer = 0 To ecovs1.Length - 1
            'this is the modification compared to WSM
            ecovs1(i) = New Matrix2(1, 0, 0, 1)
        Next


        Dim rawdpose As Pose2D = seed
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
                'this is the modification compared to QWSM
                ecovs2(i) = New Matrix2(0, 0, 0, 0)
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


    Protected Overridable Function HasConverged(ByVal numPoints As Integer, ByVal numCorrespondencies As Integer, ByVal numIterations As Integer) As Boolean
        Return numIterations < MAX_ITERATIONS
    End Function



    Protected Overridable Function ToPoints(ByVal scan As ScanObservation, ByVal angles() As Double) As Vector2()
        With scan
            Dim points(.Range.Length - 1) As Vector2
            'Dim xoffset As Double = scan.OffsetX * .Factor
            'Dim yoffset As Double = scan.OffsetY * .Factor

            Dim dist As Double, angle As Double
            For i As Integer = 0 To .Range.Length - 1
                dist = .Range(i) * .Factor
                angle = angles(i)
                points(i) = New Vector2(Cos(angle) * dist, Sin(angle) * dist)
            Next

            Return points
        End With
    End Function

    Protected Overridable Function ToFilter(ByVal scan As ScanObservation) As Boolean()
        With scan
            Dim filter(.Range.Length - 1) As Boolean
            For i As Integer = 0 To .Range.Length - 1
                filter(i) = .Range(i) < .MinRange OrElse .Range(i) > .MaxRange
            Next
            Return filter
        End With
    End Function

    Protected Overridable Function ToLocal(ByVal points() As Vector2, ByVal target As Pose2D) As Vector2()
        Dim rotmx As TMatrix2D = target.ToGlobalMatrix

        Dim local(points.Length - 1) As Vector2
        For i As Integer = 0 To points.Length - 1
            local(i) = rotmx * points(i)
        Next
        Return local

    End Function



    Protected Overridable Function ComputeScanAngles(ByVal scan As ScanObservation) As Double()

        Dim fov As Double = scan.FieldOfView
        Dim res As Double = scan.Resolution

        Dim length As Integer = CInt(fov / res) + 1
        Dim fromAngle As Double = -fov / 2

        Dim angles(length - 1) As Double
        For i As Integer = 0 To length - 1
            angles(i) = fromAngle + res * i
        Next
        Return angles

    End Function





    Protected Function CorrelatePointsDM(ByVal manifold As Manifold, ByVal points1() As Vector2, ByVal points2() As Vector2, ByVal filter1() As Boolean, ByVal filter2() As Boolean) As Correlation(Of Vector2)()

        Dim maxdist As Double = Pow(MAX_CORRELATIONDISTANCE, 2)

        'dists will hold the full distance matrix
        Dim dists(points1.Length - 1, points2.Length - 1) As Double

        '- collect preferences for points1
        '- and simultaneously construct full distance matrix dists
        Dim marks1to2(points1.Length - 1) As Integer
        For i As Integer = 0 To points1.Length - 1

            Dim curdist As Double = maxdist + 1
            Dim curmark As Integer = -1

            For j As Integer = 0 To points2.Length - 1

                'compute distance and store in full distance matrix
                Dim dist As Double = Me.ComputeSquaredDistance(points1(i), points2(j))
                dists(i, j) = dist

                'check preference
                If Not filter1(i) AndAlso Not filter2(j) _
                    AndAlso dist < maxdist AndAlso dist < curdist Then

                    curdist = dists(i, j)
                    curmark = j

                End If

            Next

            'store preference
            marks1to2(i) = curmark

        Next

        'matches will get to store the number of pairs that prefer each other
        Dim matches As Integer = 0

        Dim correlations As New List(Of Correlation(Of Vector2))

        'collect preferences for points2
        Dim marks2to1(points2.Length - 1) As Integer
        For j As Integer = 0 To points2.Length - 1

            Dim curdist As Double = maxdist + 1
            Dim curmark As Integer = -1

            'find nearest point of those that prefer me
            For i As Integer = 0 To points1.Length - 1
                If marks1to2(i) = j Then
                    Dim dist As Double = dists(i, j)
                    If dist < maxdist AndAlso dist < curdist Then
                        curdist = dist
                        curmark = i
                    End If
                End If
            Next

            marks2to1(j) = curmark

            If curdist < maxdist Then
                'we have a match!
                correlations.Add(New Correlation(Of Vector2)(points1(curmark), points2(j)))
                matches += 1
            End If

        Next

        'collect all pairs
        Dim pairs(matches - 1, 1) As Integer
        Dim p As Integer = 0
        For j As Integer = 0 To points2.Length - 1

            If marks2to1(j) < 0 Then
                Continue For
            End If

            pairs(p, 0) = marks2to1(j)
            pairs(p, 1) = j

            p += 1

        Next

        Return correlations.ToArray

    End Function

    Protected Function CorrelatePointsQT(ByVal manifold As Agent.Manifold, ByVal points1() As Math.Vector2, ByVal points2() As Math.Vector2, ByVal filter1() As Boolean, ByVal filter2() As Boolean) As Correlation(Of Math.Vector2)()

        Dim qtree1 As New QuadTree(Of Vector2)
        For i As Integer = 0 To points1.Length - 1
            If Not filter1(i) Then
                qtree1.Insert(points1(i))
            End If
        Next

        Dim qtree2 As New QuadTree(Of Vector2)
        For i As Integer = 0 To points2.Length - 1
            If Not filter2(i) Then
                qtree2.Insert(points2(i))
            End If
        Next

        Dim correlations As New List(Of Correlation(Of Vector2))

        Dim point1 As Vector2, point2 As Vector2
        For Each point As Vector2 In points2

            point1 = qtree1.FindNearestNeighbour(point, MAX_CORRELATIONDISTANCE)
            If IsNothing(point1) Then Continue For

            point2 = qtree2.FindNearestNeighbour(point1, MAX_CORRELATIONDISTANCE)
            If IsNothing(point2) Then Continue For

            If point2 Is point Then
                'we found a pair
                correlations.Add(New Correlation(Of Vector2)(point1, point2))
            End If
        Next

        Return correlations.ToArray

    End Function

    Protected Function CorrelatePointsGlobally(ByVal manifold As Manifold, ByVal patch As Patch) As Correlation(Of Vector2)()

        Dim maxdist As Double = MAX_CORRELATIONDISTANCE

        Dim qtree1 As ManifoldIndex = manifold.ManifoldIndex

        Dim qtree2 As New QuadTree(Of Vector2)
        Dim points() As Vector2 = patch.GlobalPoints
        Dim filter() As Boolean = patch.Filter
        For i As Integer = 0 To points.Length - 1
            If Not filter(i) Then
                qtree2.Insert(points(i))
            End If
        Next


        Dim correlations As New List(Of Correlation(Of Vector2))

        Dim point1 As Vector2, point2 As Vector2
        For Each point As Vector2 In points

            'find nearest point in manifold
            point1 = qtree1.FindNearestNeighbour(point, MAX_CORRELATIONDISTANCE)
            If IsNothing(point1) Then Continue For

            'find nearest point in patch for the point found in the manifold
            point2 = qtree2.FindNearestNeighbour(point1, MAX_CORRELATIONDISTANCE)
            If IsNothing(point2) Then Continue For

            If point2 Is point Then
                'we found a pair
                correlations.Add(New Correlation(Of Vector2)(point1, point2))
            End If
        Next

        Return correlations.ToArray

    End Function




    Protected Overridable Function ComputeSquaredDistance(ByVal point1 As Vector2, ByVal point2 As Vector2) As Double
        Return Pow(point2.X - point1.X, 2) + Pow(point2.Y - point1.Y, 2)
    End Function

End Class
