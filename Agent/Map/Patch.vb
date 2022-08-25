Imports System.Drawing
Imports System.Math
Imports System.IO

Imports UvARescue.Agent
Imports UvARescue.Math
Imports UvARescue.Tools

''' <summary>
''' A patch stores a single laser range scan with a pose estimate.
''' TODO: calculate data to enhance patch for constructing a mobility map. (MAARTEN)
''' </summary>
''' <remarks></remarks>
Public Class Patch
    Inherits GraphNode(Of Patch, Relation)

#Region " Constructor "

    ''' <summary>
    ''' Will construct a new Patch with a newly assigned globally unique ID (Guid)
    ''' </summary>
    ''' <param name="manifold"></param>
    ''' <param name="scan"></param>
    ''' <param name="rawOrigin"></param>
    ''' <remarks></remarks>
    Public Sub New(ByVal manifold As Manifold, ByVal scan As ScanObservation, ByVal rawOrigin As Pose2D, ByVal agentName As String)
        MyBase.New(manifold, Guid.NewGuid)

        Me._Timestamp = Now
        Me._OldTime = oldTime 'get the timestamp from the last patch(MRT)
        Me._Scan = scan
        Me._RawOrigin = rawOrigin
        Me._EstimatedOrigin = rawOrigin
        Me._AgentSize = agentSize 'get the size of the agent that created the patch (MRT)
        Me._AgentName = agentName
    End Sub

    ''' <summary>
    ''' Will construct a patch with the specified id
    ''' </summary>
    ''' <param name="manifold"></param>
    ''' <param name="patchID"></param>
    ''' <param name="scan"></param>
    ''' <param name="rawOrigin"></param>
    ''' <remarks></remarks>
    Protected Sub New(ByVal manifold As Manifold, ByVal patchID As Guid, ByVal scan As ScanObservation, ByVal rawOrigin As Pose2D, ByVal oldTime As DateTime, ByVal agentSize As Size, ByVal agentName As String)
        MyBase.New(manifold, patchID)
        Me._Timestamp = Now
        Me._Scan = scan
        Me._RawOrigin = rawOrigin
        Me._EstimatedOrigin = rawOrigin
        Me._AgentSize = agentSize 'get size of agent that creates the patch(MRT)
        Me._AgentName = agentName
    End Sub


#End Region

#Region " Memento "

    ''' <summary>
    ''' This class is to hold a serializable summary of the Patch that captures
    ''' all relevant information usually stored on the patch. The memento will be sent
    ''' to other team-members which can copy the information by restoring this memento
    ''' in their own manifold.
    ''' </summary>
    ''' <remarks></remarks>
    <Serializable()> _
    Protected Class PatchMemento
        Implements IMemento

        ''' <summary>
        ''' The sender constructs a summary of the particular patch.
        ''' </summary>
        ''' <param name="patch"></param>
        ''' <remarks></remarks>
        Public Sub New(ByVal patch As Patch)
            Me._PatchID = patch.ID
            Me._Scan = patch._Scan
            Me._RawOrigin = patch._RawOrigin
            Me._EstimatedOrigin = patch._EstimatedOrigin
            Me._AgentName = patch._AgentName
            'Me._OmniCamOrigin = patch._OmniCamOrigin
            'Me._Omnicam = patch.Omnicam

            'copy all victim data ever processed
            Me._VictimPoses.AddRange(patch._VictimPoses)
            Me._VictimData.AddRange(patch._VictimData)
            Me._VictimTimestamps.AddRange(patch._VictimTimestamps)

            Me._Timestamp = patch.Timestamp
            Me._OldTime = patch.OldTime   ' Add OldTime (MRT)
            Me._AgentSize = patch.AgentSize 'Add Agent Size(MRT)

            Me._PicturePoses.AddRange(patch._PicturePoses)
            Me._PictureX.AddRange(patch._PictureXs)
            Me._PictureY.AddRange(patch._PictureYs)
            Me._HistIms.AddRange(patch._HistIms)
            Me._SkinPercentage.AddRange(patch._SkinPercentages)
            Me._ImageTimestamps.AddRange(patch._VictimTimestamps)

        End Sub

        Private _PatchID As Guid
        Private _Scan As ScanObservation
        Private _RawOrigin As Pose2D
        Private _EstimatedOrigin As Pose2D
        Private _OmniCamOrigin As Pose2D
        Private _Omnicam As OmnicamObservation
        Private _AgentSize As Size 'Size of agent 
        Private _AgentName As String


        Private _VictimPoses As New List(Of Pose2D)
        Private _VictimData As New List(Of VictimData)
        Private _VictimTimestamps As New List(Of DateTime)

        Private _PicturePoses As New List(Of Pose2D)
        Private _PictureX As New List(Of Double)
        Private _PictureY As New List(Of Double)
        Private _HistIms As New List(Of Bitmap)
        Private _SkinPercentage As New List(Of Double)
        Private _ImageTimestamps As New List(Of DateTime)

        Private _Timestamp As DateTime
        Private _OldTime As DateTime 'OldTime in Memento 

        ''' <summary>
        ''' Invoked by the receiver to create an exact copy of the patch in his own
        ''' manifold.
        ''' </summary>
        ''' <param name="manifold"></param>
        ''' <remarks></remarks>
        Public Sub Restore(ByVal manifold As Manifold) Implements IMemento.Restore
            If Not manifold.HasNode(Me._PatchID) Then
                Dim patch As New Patch(manifold, Me._PatchID, Me._Scan, Me._RawOrigin, Me._OldTime, Me._AgentSize, Me._AgentName) ' Pass OldTime & AgentSize along (Be aware of mistakes in sharing!?!) [MRT]
                patch._EstimatedOrigin = Me._EstimatedOrigin
                manifold.Extend(patch)

                're-process all omnicam data?

                're-process all victim data 
                Dim i As Integer = 0
                While i < Me._VictimData.Count
                    patch.ProcessVictimData(Me._VictimPoses(i), Me._VictimData(i))
                    i += 1
                End While

                i = 0
                While i < Me._PicturePoses.Count
                    patch.ProcessVictimPicture(Me._PicturePoses(i), Me._PictureX(i), Me._PictureY(i), Me._HistIms(i), Me._SkinPercentage(i))
                    i += 1
                End While

                'IMPORTANT
                'do not update timestamp, every manifold must assign its own
                'timestamps to patches in order to make manifold sharing
                'between more than 2 robots work!
                'patch._Timestamp = Me._Timestamp


            Else

                Console.WriteLine(String.Format("[Map] - patch {0} already exist)", Me._PatchID))

            End If
        End Sub

    End Class

'Sub New()
' TODO: Complete member initialization 
'End Sub

    Public Function CreateMemento() As IMemento
        Return New PatchMemento(Me)
    End Function

#End Region

#Region " Core Patch Properties "

    Public ReadOnly Property Manifold() As Manifold
        Get
            Return DirectCast(Me.Graph, Manifold)
        End Get
    End Property

    Private _Timestamp As DateTime
    Public ReadOnly Property Timestamp() As DateTime
        Get
            Return Me._Timestamp
        End Get
    End Property

    ' Oldtime = timestamp from last patch (MRT)
    Private _OldTime As DateTime
    Public ReadOnly Property OldTime() As DateTime
        Get
            Return Me._OldTime
        End Get
    End Property

    Private _Scan As ScanObservation
    Public ReadOnly Property Scan() As ScanObservation
        Get
            Return _Scan
        End Get
    End Property

    Private _RawOrigin As Pose2D
    Public ReadOnly Property RawOrigin() As Pose2D
        Get
            Return _RawOrigin
        End Get
    End Property

    Private _AgentName As String
    Public ReadOnly Property AgentName() As String
        Get
            Return _AgentName
        End Get
    End Property
    'Private _OmnicamOrigin As Pose2D
    'Public ReadOnly Property OmnicamOrigin() As Pose2D
    '    Get
    '        Return _OmnicamOrigin
    '    End Get
    'End Property

    'Private _Omnicam As OmnicamObservation
    'Public ReadOnly Property Omnicam() As OmnicamObservation

    '    Get
    '        Return _Omnicam
    '    End Get
    'End Property
    'AgentSize = size of agent from which patch is created (MRT)
    Private _AgentSize As Size
    Public ReadOnly Property AgentSize() As Size
        Get
            Return _AgentSize
        End Get
    End Property

#End Region

#Region " Invariant (Local) Derived Properties "

    Private _Angles() As Double = Nothing
    Public ReadOnly Property Angles() As Double()
        Get
            If IsNothing(Me._Angles) Then
                'compute the angles for the first time
                With Me.Scan
                    Dim fromAngle As Double = -.FieldOfView / 2
                    ReDim Me._Angles(.Length - 1)
                    For i As Integer = 0 To .Length - 1
                        Me._Angles(i) = fromAngle + .Resolution * i
                    Next
                End With
            End If
            Return Me._Angles
        End Get
    End Property

    Private _LocalPoints() As Vector2 = Nothing
    Public ReadOnly Property LocalPoints() As Vector2()
        Get
            'since these points are in local coords they are independent of the current
            'pose estimate. Hence, once computed they can be cached.

            If IsNothing(Me._LocalPoints) Then
                With Me.Scan

                    'compute the points for the first time.
                    Dim angles() As Double = Me.Angles
                    ReDim Me._LocalPoints(.Length - 1)

                    Dim dist As Double, angle As Double
                    For i As Integer = 0 To .Length - 1
                        Try
                            dist = .Range(i)
                        Catch ' HANNE: for fake scans (DOAS 2009)
                            dist = 10
                        End Try
                        angle = angles(i)
                        Me._LocalPoints(i) = New Vector2(Cos(angle) * (dist * .Factor), Sin(angle) * (dist * .Factor))
                    Next

                End With
            End If

            Return Me._LocalPoints

        End Get
    End Property

    Private _Filter() As Boolean = Nothing
    Public ReadOnly Property Filter() As Boolean()
        Get
            If IsNothing(Me._Filter) Then
                'compute the filter for the first time
                With Me.Scan
                    Try
                        ReDim Me._Filter(.Range.Length - 1)
                        For i As Integer = 0 To .Range.Length - 1
                            Me._Filter(i) = .Range(i) < .MinRange OrElse .Range(i) > .MaxRange
                        Next
                    Catch ' HANNE: for fake scans (DOAS 2009)
                        ReDim Me._Filter(9)
                        For i As Integer = 0 To 9
                            Me._Filter(i) = True
                        Next
                    End Try
                End With
            End If
            Return Me._Filter
        End Get
    End Property

    Private _EstimatedSpeed As Double = Nothing
    Public Property EstimatedSpeed() As Double
        Get
            If IsNothing(Me._EstimatedSpeed) Then
                'Compute the speed for the first time
                With Me.Scan
                    _EstimatedSpeed = Sqrt(Pow(.OffsetX, 2) + Pow(.OffsetY, 2)) / (_Timestamp - _OldTime).TotalSeconds() 'Get Timespan between patches to calculate speed!(MRT)
                End With

            End If
            Return Me._EstimatedSpeed
        End Get
        Set(ByVal value As Double)

        End Set
    End Property


#End Region

#Region " Covariances - For Weigthed Scan Matching "

    Private Const IANGLE_THRESHOLD As Double = PI / 8

    'for tuning the noise error covariance
    Private Const DEFAULT_SIGMAANG As Double = 0.002
    Private Const DEFAULT_SIGMADIST As Double = 1

    'for tuning the corresponde error covariance
    Private Const SIGMA_CORRELATED As Double = 1


    Private _Covariances() As Matrix2 = Nothing
    Public ReadOnly Property Covariances() As Matrix2()
        Get
            If IsNothing(Me._Covariances) Then
                Dim points() As Vector2 = Me.LocalPoints
                Dim iangles() As Double = Me.IncidenceAngles
                Dim dangle As Double = Me.Scan.FieldOfView / Me.Scan.Length

                ReDim Me._Covariances(points.Length - 1)
                For i As Integer = 0 To Me._Covariances.Length - 1
                    Me._Covariances(i) = Me.ComputeErrorCovariance(points(i), iangles(i), dangle)
                Next

            End If
            Return Me._Covariances
        End Get
    End Property

    Public ReadOnly Property AvgCovarianceDeterminant() As Double
        Get
            Dim sum As Double = 0
            If Me.Covariances.Length = 0 Then
                Return 0.5 'what is a good value 0 or 1
            End If
            For Each Covariance As Matrix2 In Me.Covariances
                sum += Covariance.Determinant
            Next
            Return sum / Me.Covariances.Length
        End Get
    End Property

    Private _IncidenceAngles As Double() = Nothing
    Public ReadOnly Property IncidenceAngles() As Double()
        Get
            If IsNothing(Me._IncidenceAngles) Then
                Me._IncidenceAngles = Me.ComputeIncidenceAngles
            End If
            Return Me._IncidenceAngles
        End Get
    End Property


    Protected Overridable Function ComputeIncidenceAngles() As Double()

        Dim points() As Vector2 = Me.LocalPoints
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

#End Region

#Region " EstimatedOrigin-dependent (Global) Derived Properties "

    Private Mutex As New Object

    Private _EstimatedOrigin As Pose2D

    ''' <summary>
    ''' This property is publicly writable, however it is typically solely
    ''' up to a SLAM implementation to maintain the estimate.
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property EstimatedOrigin() As Pose2D
        Get
            Return _EstimatedOrigin
        End Get
        Set(ByVal value As Pose2D)
            If Not value Is Me._EstimatedOrigin Then

                SyncLock Me.Mutex

                    Me._EstimatedOrigin = value

                    'make sure the global points and -polygon are recomputed next time
                    Me._GlobalPoints = Nothing
                    Me._GlobalPolygon = Nothing

                End SyncLock

            End If
        End Set
    End Property

    Private _GlobalPoints() As Vector2 = Nothing
    Public ReadOnly Property GlobalPoints() As Vector2()
        Get
            Dim points() As Vector2
            SyncLock Me.Mutex
                If IsNothing(Me._GlobalPoints) Then

                    'first get the local points
                    Dim plocal() As Vector2 = Me.LocalPoints

                    'construct projection matrix
                    Dim rotmx As TMatrix2D = Me.EstimatedOrigin.ToGlobalMatrix

                    'project local points 
                    ReDim Me._GlobalPoints(plocal.Length - 1)
                    For i As Integer = 0 To plocal.Length - 1
                        Me._GlobalPoints(i) = rotmx * plocal(i)
                    Next

                End If

                points = DirectCast(Me._GlobalPoints.Clone(), Vector2())

            End SyncLock

            Return points

        End Get
    End Property

    Private _GlobalPolygon() As PointF
    Public ReadOnly Property GlobalPolygon() As PointF()
        Get
            If IsNothing(Me._GlobalPolygon) Then
                SyncLock Me.Mutex
                    If IsNothing(Me._GlobalPolygon) Then

                        Dim points() As Vector2 = Me.GlobalPoints

                        'construct the polyline from all the points.
                        ReDim Me._GlobalPolygon(points.Length) 'the origin will be appended too
                        For i As Integer = 0 To points.Length - 1
                            Me._GlobalPolygon(i).X = CType(points(i).X, Single)
                            Me._GlobalPolygon(i).Y = CType(points(i).Y, Single)
                        Next

                        'add origin
                        Dim origin As Vector2 = Me.EstimatedOrigin.Position
                        Me._GlobalPolygon(Me._GlobalPolygon.Length - 1) = New PointF(CType(origin.X, Single), CType(origin.Y, Single))

                    End If
                End SyncLock
            End If

            Return Me._GlobalPolygon

        End Get
    End Property

#End Region

#Region " Victim Observations "

    'to keep track of all the victim-related data that we have seen
    Private _VictimPoses As New List(Of Pose2D)
    Private _VictimData As New List(Of VictimData)
    Private _VictimTimestamps As New List(Of DateTime)

    Private _VictimParts As New Dictionary(Of String, List(Of VictimData.VictimPart))

    Private _histIm As Bitmap

    Public Sub ProcessVictimData(ByVal pose As Pose2D, ByVal victimData As VictimData)

        Me._VictimPoses.Add(pose)
        Me._VictimData.Add(victimData)
        Me._VictimTimestamps.Add(Now)

        Dim factor As Single = 1000 'to convert from m to mm

        For Each part As VictimData.VictimPart In victimData.Parts

            If part.PartName = "chest" Or part.PartName = "head" Or part.PartName = "leg" Or part.PartName = "hand" Then

                'note that the part is observed in local coords wrt the current pose of the robot
                'transform to global coordinate wrt pose using a Pose2D object with dummy rotation
                Dim pglobal As Vector2 = New Pose2D(CType(part.X * factor, Single), CType(part.Y * factor, Single), 0).ToGlobal(pose).Position

                'find observation nearest to this global coord
                Dim observation As VictimObservation = Me.Manifold.FindNearestVictim(pglobal.X, pglobal.Y, 2000)
                Dim victimID As String

                If Not IsNothing(observation) Then
                    'a nearby observation was found, contribute to it
                    victimID = observation.VictimID
                Else
                    'no nearby observation was found, create a new one
                    victimID = String.Format("VictimZone-{0}", Me.Manifold.VictimCount)
                    observation = Me.Manifold.InsertVictim(New VictimObservation(Me.Manifold, victimID, "VSENSOR"))
                End If

                Dim count As Integer = victimData.PartCount(part.PartName)

                observation.AcquireWriterLock()
                observation.AddVictimPart(part, count, pglobal.X, pglobal.Y)
                observation.AddLogEntry(String.Format("[{0}]-- Agent at {1}: seen part {2} of {3} at location {4:F2},{5:F2}", Now.ToString, Me.RawOrigin.ToString, part.PartName, victimID, pglobal.X / 1000, pglobal.Y / 1000))
                observation.ReleaseWriterLock()

                'also store relevant information in local dictionary

                Dim list As List(Of VictimData.VictimPart)
                If Me._VictimParts.ContainsKey(victimID) Then
                    list = Me._VictimParts(victimID)
                Else
                    list = New List(Of VictimData.VictimPart)
                    Me._VictimParts(victimID) = list
                End If

                list.Add(part)

            End If

        Next

        'update timestamp
        Me._Timestamp = Now

    End Sub

    Private _PicturePoses As New List(Of Pose2D)
    Private _PictureXs As New List(Of Double)
    Private _PictureYs As New List(Of Double)
    Private _HistIms As New List(Of Bitmap)
    Private _SkinPercentages As New List(Of Double)

    Public Sub ProcessVictimPicture(ByVal pose As Pose2D, ByVal x As Double, ByVal y As Double, ByVal histIm As Bitmap, ByVal skinPercentage As Double)

        Me._PicturePoses.Add(pose)
        Me._PictureXs.Add(x)
        Me._PictureYs.Add(y)
        Me._HistIms.Add(histIm)
        Me._SkinPercentages.Add(skinPercentage)

        Me._VictimTimestamps.Add(Now)

        Dim factor As Single = 1000 'to convert from m to mm

        'note that the part is observed in local coords wrt the current pose of the robot
        'transform to global coordinate wrt pose using a Pose2D object with dummy rotation
        Dim pglobal As Vector2 = New Pose2D(CType(x * factor, Single), CType(y * factor, Single), 0).ToGlobal(pose).Position

        'find observation nearest to this global coord
        Dim observation As VictimObservation = Me.Manifold.FindNearestVictim(pglobal.X, pglobal.Y, 2000)
        Dim victimID As String

        If Not IsNothing(observation) Then
            'a nearby observation was found, contribute to it
            victimID = observation.VictimID
        Else
            'no nearby observation was found, create a new one
            victimID = String.Format("VictimZone-{0}", Me.Manifold.VictimCount)
            observation = Me.Manifold.InsertVictim(New VictimObservation(Me.Manifold, victimID, "VISUAL"))
        End If

        observation.AcquireWriterLock()
        If observation.UpdatePicture(histIm, skinPercentage) Then
            'add "visual" body part
            Dim avgLoc As String = String.Format("{0},{1},0", x, y)
            Dim part As VictimData.VictimPart = New VictimData.VictimPart("Visual", avgLoc)
            observation.AddVictimPart(part, 1, pglobal.X, pglobal.Y)
            observation.AddLogEntry(String.Format("[{0}]-- Agent at {1}: Seen image of {2} having {3:F2} amount of skin", Now.ToString, Me.RawOrigin.ToString, victimID, skinPercentage))
            'add histIm to patch
            Me._histIm = histIm
            'update timestamp
            Me._Timestamp = Now
        End If
        observation.ReleaseWriterLock()

    End Sub

#End Region

#Region " Omnicam Observations "

    Private Const CamProcessTimeStep As Integer = 750
    Private Shared LastCameraProcess As DateTime = DateTime.Now

    Public Sub ProcessCameraData(ByVal camera As CameraData, ByVal poseEstimate As Pose2D)
        'If the last omnicam update was longer than CamProcessTimeStep ago, update it
        If (DateTime.Now - LastCameraProcess).Ticks >= CamProcessTimeStep Then
            LastCameraProcess = DateTime.Now
            Me.Manifold.ProcessOmnicam(Me, camera, poseEstimate)
        End If
    End Sub

    'Friend Sub SetOmnicam(ByVal omnicam As OmnicamObservation, ByVal poseEstimate As Pose2D)
    '    _Omnicam = omnicam
    '    _OmnicamOrigin = poseEstimate
    'End Sub

#End Region

End Class
