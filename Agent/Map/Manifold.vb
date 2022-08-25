Imports System.IO
Imports System.Drawing
Imports System.Threading
Imports System.Math

Imports UvARescue.Math
Imports UvARescue.Tools

Imports MathNet.Numerics
Imports MathNet.SignalProcessing.DataSources

''' <summary>
''' Extends graph with locking capabilities and specific Manifold-related 
''' functionality.
''' 
''' Please note that although locking functionality is provided, it is NOT
''' used in the Manifold's members. So the Manifold (like the Graph from which
''' it inherits) is NOT thread-safe UNLESS this is enforced by all consuming 
''' parties.
''' </summary>
''' <remarks></remarks>
Public Class Manifold
    Inherits Graph(Of Patch, Relation)

#Region " Constructor "
    Public Sub New()
        Me._BirdEyeImageSize = New Size(479, 479) 'should be smaller than CameraImageSize
        Me._CameraImageSize = New Size(640, 480)
        Me._HRadius = 0.185625
        Console.WriteLine("manifold is constructed")
        ReDim Me._BirdEyeConversionArray(_BirdEyeImageSize.Width * _BirdEyeImageSize.Height)
        'Me.ConstructBirdEyeConvArr()
    End Sub

#End Region

#Region " Observers "


    Public accurateAngle As Double


    Public currNumber As Integer
    Public ReadOnly Property showcurrNumber() As Integer
        Get
            Return Me.currNumber
        End Get
    End Property

    Public _targetDataBase As New Dictionary(Of Integer, Pose2D)

    Public _targetDatabase2 As New Dictionary(Of Integer, Pose2D)


    Public _changeMotion As New Dictionary(Of Integer, Boolean)

    Public _Astar As Astar

    Public _finished As Boolean
    Public _moving As Boolean

    Public startArea As New Dictionary(Of Integer, Vector2)

    Public endArea As New Dictionary(Of Integer, Vector2)

    Private observers As New List(Of IManifoldObserver)

    Public _robotsAngle As Double

    Public Sub AddObserver(ByVal observer As IManifoldObserver)
        SyncLock Me.observers
            Me.observers.Add(observer)
        End SyncLock
    End Sub
    Public Sub RemoveObserver(ByVal observer As IManifoldObserver)
        SyncLock Me.observers
            Me.observers.Remove(observer)
        End SyncLock
    End Sub

#End Region

#Region " Graph Hooks "

    Protected Overrides Sub OnNodeInserted(ByVal node As Patch)
        MyBase.OnNodeInserted(node)

        'notify observers
        SyncLock Me.observers
            For Each observer As IManifoldObserver In Me.observers
                observer.NotifyPatchInserted(node)
            Next
        End SyncLock

    End Sub

    Protected Overrides Sub OnLinkInserted(ByVal link As Relation)
        MyBase.OnLinkInserted(link)

        'notify observers
        SyncLock Me.observers
            For Each observer As IManifoldObserver In Me.observers
                observer.NotifyRelationInserted(link)
            Next
        End SyncLock

        'code below only used for debugging, and counting can slow us down

        'Dim nodes As Integer
        'Dim links As Integer

        'Me.AcquireReaderLock()
        'nodes = Me.NodeCount
        'links = Me.LinkCount
        'Me.ReleaseReaderLock()

        ' Console.WriteLine(String.Format("[Manifold]-- Extended, currently has  {0} patches / {1} relations", nodes, links))

    End Sub

    Public Overrides Sub Clear()
        MyBase.Clear()

        'notify observers
        SyncLock Me.observers
            For Each observer As IManifoldObserver In Me.observers
                observer.NotifyCleared()
            Next
        End SyncLock

    End Sub

#End Region

#Region " Locking "

    'reader-writer locks allow either a single writer (x)or multiple simultaneous
    'readers. Since heavy (read-only) use of the manifold is anticipated
    '(especially for Gui rendering) this made reader-writer locks the preferred
    'choice over mutexes or other mutual exclusive synchronization instruments 
    'like wait-handles.


    Private lock As New ReaderWriterLock

    Public Sub AcquireReaderLock()
        Me.lock.AcquireReaderLock(Timeout.Infinite)
        'Console.WriteLine(Thread.CurrentThread.Name & " acquired Read lock")
    End Sub
    Public Sub ReleaseReaderLock()
        'Console.WriteLine(Thread.CurrentThread.Name & " released Read lock")
        Me.lock.ReleaseReaderLock()
    End Sub

    Public Sub AcquireWriterLock()
        Me.lock.AcquireWriterLock(Timeout.Infinite)
        'Console.WriteLine(Thread.CurrentThread.Name & " acquired Write lock")
    End Sub
    Public Sub ReleaseWriterLock()
        'Console.WriteLine(Thread.CurrentThread.Name & " released Write lock")
        Me.lock.ReleaseWriterLock()
    End Sub

    Public Sub ReleaseLock()
        Me.lock.ReleaseLock()
    End Sub

#End Region

#Region " ManifoldIndex "

    Private _ManifoldIndex As New ManifoldIndex(Me)

    Public ReadOnly Property ManifoldIndex() As ManifoldIndex
        Get
            Return Me._ManifoldIndex
        End Get
    End Property

    Public Function FindNearestPatch(ByVal target As Vector2, ByVal maxDistance As Single) As Patch
        Return Me._ManifoldIndex.FindNearestPatch(target, maxDistance)
    End Function




    Private _PatchIndex As New Dictionary(Of String, List(Of Patch))
    'Private _LogIndex As New Dictionary(Of String, LogFileWriter)
    Public ReadOnly Property PatchIndex() As Dictionary(Of String, List(Of Patch))
        Get
            Return Me._PatchIndex
        End Get
    End Property
#End Region

#Region " Mapping Functionality "

    Public Overridable Sub Extend(ByVal atPatch As Patch, ByVal newPatch As Patch, ByVal odometry As Pose2D, ByVal covariance As Matrix3)
        Me.Extend(newPatch)
        Me.Extend(New Relation(Me, atPatch, newPatch, odometry, covariance))
    End Sub
    Public Overridable Sub Extend(ByVal newPatch As Patch, ByVal newRelation As Relation)
        Me.Extend(newPatch)
        Me.Extend(newRelation)
    End Sub


    Public Overridable Sub Extend(ByVal newPatch As Patch)
        Me.InsertNode(newPatch)
        Me._ManifoldIndex.Add(newPatch)

        'Add Patch to PatchIndex
        If Me._PatchIndex.ContainsKey(newPatch.AgentName) Then
            Me._PatchIndex.Item(newPatch.AgentName).Add(newPatch)
        Else
            Dim templist As New List(Of Patch)
            templist.Add(newPatch)
            Me._PatchIndex.Add(newPatch.AgentName, templist)
        End If
    End Sub

    Public Overridable Sub Extend(ByVal newRelation As Relation)
        Me.InsertLink(newRelation)
    End Sub

#End Region

#Region " Localization Functionality "

    Private TrackedAgents As New Dictionary(Of Guid, Agent)
    Private CurrentPatches As New Dictionary(Of Guid, Guid)
    Private CurrentPoses As New Dictionary(Of Guid, Pose2D)


    Public Function GetTrackedAgents() As IEnumerable(Of Agent)
        Return Me.TrackedAgents.Values
    End Function


    Public Function GetCurrentPatch(ByVal agent As Agent) As Patch
        If Not Me.CurrentPatches.ContainsKey(agent.UniqueID) Then
            Return Nothing
        End If
        Return Me.GetNode(Me.CurrentPatches(agent.UniqueID))
    End Function


    Public Function GetCurrentPose(ByVal agent As Agent) As Pose2D
        If Not Me.CurrentPoses.ContainsKey(agent.UniqueID) Then
            Return Nothing
        End If
        Return Me.CurrentPoses(agent.UniqueID)
    End Function

    Public Overridable Sub LocalizeAgent(ByVal agent As Agent, ByVal patch As Patch, ByVal pose As Pose2D, ByVal commMatrix As Double(,))

        If Not Me.TrackedAgents.ContainsKey(agent.UniqueID) Then
            Me.TrackedAgents.Add(agent.UniqueID, agent)
        End If

        Me.CurrentPoses(agent.UniqueID) = pose
        If Not IsNothing(patch) Then
            Me.CurrentPatches(agent.UniqueID) = patch.ID
        Else
            Me.CurrentPatches(agent.UniqueID) = Guid.Empty
        End If

        agent.NotifyPoseEstimateUpdated(pose)

        SyncLock Me.observers
            For Each observer As IManifoldObserver In Me.observers
                observer.NotifyAgentMoved(agent, pose, commMatrix)
            Next
        End SyncLock

    End Sub

#End Region

#Region " Victim Observations "

    Private _Victims As New Dictionary(Of String, VictimObservation)
    Public ReadOnly Property Victims() As IEnumerable(Of VictimObservation)
        Get
            Return Me._Victims.Values
        End Get
    End Property
    Public ReadOnly Property VictimCount() As Integer
        Get
            Return Me._Victims.Count
        End Get
    End Property

    Public Function FindNearestVictim(ByVal x As Double, ByVal y As Double, ByVal maxDistance As Double) As VictimObservation

        Dim curDistance As Double = Double.MaxValue
        Dim curVictim As VictimObservation = Nothing

        For Each victim As VictimObservation In Me._Victims.Values
            Dim distance As Double = ((x - victim.AverageX) ^ 2 + (y - victim.AverageY) ^ 2) ^ 0.5
            If distance < curDistance Then
                curDistance = distance
                curVictim = victim
            End If
        Next

        'check if this observation is within the specified maximum distance
        If curDistance <= maxDistance Then
            Return curVictim
        Else
            Return Nothing
        End If

    End Function

    Public Function GetVictim(ByVal victimID As String) As VictimObservation
        If Me._Victims.ContainsKey(victimID) Then
            Return Me._Victims(victimID)
        End If
        Return Nothing
    End Function

    Public Function HasVictim(ByVal victimID As String) As Boolean
        Return Me._Victims.ContainsKey(victimID)
    End Function

    Public Function InsertVictim(ByVal victim As VictimObservation) As VictimObservation
        Me._Victims.Add(victim.VictimID, victim)
        Me.OnVictimInserted(victim)
        Return victim
    End Function

    Public Function RemoveVictim(ByVal victimID As String) As VictimObservation
        Dim victim As VictimObservation = Me.GetVictim(victimID)
        If Not IsNothing(victim) Then
            Me._Victims.Remove(victimID)
            Me.OnVictimRemoved(victim)
        End If
        Return victim
    End Function


    ''' <summary>
    ''' Invoked by VictimObservations to notify the manifold of updates.
    ''' </summary>
    ''' <param name="victim"></param>
    ''' <remarks></remarks>
    Friend Sub NotifyVictimUpdated(ByVal victim As VictimObservation)
        Me.OnVictimUpdated(victim)
    End Sub

    Protected Overridable Sub OnVictimInserted(ByVal victim As VictimObservation)
        SyncLock Me.observers
            For Each observer As IManifoldObserver In Me.observers
                observer.NotifyVictimUpdated(victim)
            Next
        End SyncLock
    End Sub
    Protected Overridable Sub OnVictimUpdated(ByVal victim As VictimObservation)
        SyncLock Me.observers
            For Each observer As IManifoldObserver In Me.observers
                observer.NotifyVictimUpdated(victim)
            Next
        End SyncLock
    End Sub
    Protected Overridable Sub OnVictimRemoved(ByVal victim As VictimObservation)
        SyncLock Me.observers
            For Each observer As IManifoldObserver In Me.observers
                observer.NotifyVictimUpdated(victim)
            Next
        End SyncLock
    End Sub

#End Region

#Region " Omnicam Observations "

    'Process new omnicam data
    Public Sub ProcessOmnicam(ByVal patch As Patch, ByVal camera As CameraData, ByVal poseEstimate As Pose2D)

        'SyncLock Me.observers
        '    'If camera.Bitmap.Size() = Me._CameraImageSize Then 'This test doesn't work, because camera.Bitmap is used elsewhere
        '    'Create a new observation
        '    Dim omnicam As OmnicamObservation = New OmnicamObservation(DateTime.Now, camera.Bitmap, Omni2BirdEyeTransform(camera))

        '    'Add the observation to the patch
        '    patch.SetOmnicam(omnicam, poseEstimate)

        '    'Update the layers
        '    'OnOmnicamUpdated(patch)
        '    'End If
        'End SyncLock

    End Sub

    ''Update the layers when new omnicam data has been retrieved
    'Protected Overridable Sub OnOmnicamUpdated(ByVal patch As Patch)
    '    SyncLock Me.observers
    '        For Each observer As IManifoldObserver In Me.observers
    '            observer.NotifyOmnicamUpdated(patch)
    '        Next
    '    End SyncLock
    'End Sub

#Region " OmniCamera Bird-Eye View "

    Dim _CameraImageSize As System.Drawing.Size
    Public ReadOnly Property CameraImageSize() As System.Drawing.Size
        Get
            Return Me._CameraImageSize
        End Get
    End Property

    Dim _BirdEyeImageSize As Size
    Public ReadOnly Property BirdEyeImageSize() As Size
        Get
            Return Me._BirdEyeImageSize
        End Get
    End Property

    Dim _HRadius As Double
    Public ReadOnly Property HRadius() As Double
        Get
            Return Me._HRadius
        End Get
    End Property

    Dim _BirdEyeConversionArray As Integer()
    Public ReadOnly Property BirdEyeConversionArray() As Integer()
        Get
            Return Me._BirdEyeConversionArray
        End Get
    End Property

    Private _BirdEyeViewBitmap As Bitmap = Nothing
    Public ReadOnly Property BirdEyeViewBitmap() As Bitmap
        Get
            Return Me._BirdEyeViewBitmap
        End Get
    End Property

    Private Sub ConstructBirdEyeConvArr()

        Dim x, y, x0, y0, index, z As Integer
        Dim xxbe, yybe, h, theta, phi, rho As Double

        x0 = CInt(Me._CameraImageSize.Width / 2)
        y0 = CInt(Me._CameraImageSize.Height / 2)
        z = 40
        h = HRadius * Me._CameraImageSize.Width

        index = 0
        For xbe As Integer = 0 To _BirdEyeImageSize.Width - 1
            For ybe As Integer = 0 To _BirdEyeImageSize.Height - 1

                xxbe = CInt(xbe - _BirdEyeImageSize.Width / 2)
                yybe = CInt(ybe - _BirdEyeImageSize.Height / 2)

                theta = Acos(z / Sqrt((xxbe ^ 2) + (yybe ^ 2) + (z ^ 2)))
                phi = Atan2(xxbe, yybe)

                If (1 + Cos(theta) <> 0) Then
                    rho = h / (1 + Cos(theta))
                    x = CInt(Round(rho * Sin(theta) * Cos(phi) + x0))
                    y = CInt(Round(rho * Sin(theta) * Sin(phi) + y0))
                Else
                    x = 1
                    y = 1
                End If

                x = CInt(Floor((x Mod (_CameraImageSize.Width - 1))) + 1)
                y = CInt(Floor((y Mod (_CameraImageSize.Height - 1))) + 1)

                Me._BirdEyeConversionArray(index) = y * Me._CameraImageSize.Width + x
                index = index + 1
            Next
        Next

    End Sub

    Protected Function Omni2BirdEyeTransform(ByVal camdata As CameraData) As Bitmap

        'Check this function later
        Return Nothing
        SyncLock Me.observers

            Dim imLength As Integer = CInt(camdata.RawData.Length / 3)
            Dim r(imLength), g(imLength), b(imLength) As Complex

            If imLength * 3 < Me._BirdEyeConversionArray.Length - 1 Then
                Return Nothing
            End If

            For index As Integer = 0 To imLength - 1
                r(index).Real = camdata.RawData(Me._BirdEyeConversionArray(index) * 3) / Byte.MaxValue
                g(index).Real = camdata.RawData(Me._BirdEyeConversionArray(index) * 3 + 1) / Byte.MaxValue
                b(index).Real = camdata.RawData(Me._BirdEyeConversionArray(index) * 3 + 2) / Byte.MaxValue
            Next

            Dim cp As ComplexPart = ComplexPart.Real
            Dim size As System.Drawing.Size
            size.Height = Me._BirdEyeImageSize.Height
            size.Width = Me._BirdEyeImageSize.Width
            Return BitmapConverter.WriteChannel(size, r, cp, g, cp, b, cp)
        End SyncLock

    End Function

#End Region

#End Region

#Region " WriteVictFile "

    Public Sub WriteVictFile()

        ' define filename and check existence directory
        Dim pathName As String
        pathName = Path.Combine(My.Application.Info.DirectoryPath, "Logs\")
        WriteVictFile(pathName.ToString)
    End Sub

    Public Sub WriteVictFile(ByVal pathname As String)
        Dim victFileName, imFileName, logFileName As String
        victFileName = Path.Combine(pathname, "Victim.txt")
        Dim fileInfo As New FileInfo(pathname)
        If Not fileInfo.Directory.Exists Then
            fileInfo.Directory.Create()
        End If

        ' write victims to files
        Using sw As StreamWriter = New StreamWriter(victFileName)
            For Each victim As String In Me._Victims.Keys
                sw.Write(String.Format("{0}, ", victim))
                sw.Write(String.Format("{0:f2}, ", Me._Victims(victim).AverageX / 1000))
                sw.Write(String.Format("{0:f2}, ", Me._Victims(victim).AverageY / 1000))
                sw.Write(String.Format("{0}, ", 0))
                sw.WriteLine(String.Format("{0}", Me._Victims(victim).Method))

                Dim pic As Bitmap = Me._Victims(victim).Picture
                If Not pic Is Nothing Then
                    imFileName = Path.Combine(pathname, String.Format("{0}.jpg", victim))
                    pic.Save(imFileName, System.Drawing.Imaging.ImageFormat.Jpeg)
                End If
            Next
            sw.WriteLine("END")
        End Using

        logFileName = Path.Combine(pathname, "VictLogFile.txt")
        Dim i As Integer
        Using sw As StreamWriter = New StreamWriter(logFileName)
            For Each victim As String In Me._Victims.Keys
                i = 0
                sw.WriteLine(String.Format("[{0}] Log Entries:", victim))
                While i < Me._Victims(victim).LogEntries.Count
                    sw.WriteLine(Me._Victims(victim).LogEntries(i))
                    i += 1
                End While

            Next
        End Using
    End Sub

    Public Sub WritePathFile(ByVal pathname As String)
        Dim fileName, pathFileName As String

        For Each agentName As String In PatchIndex.Keys
            fileName = agentName + "_Path.mif"
            pathFileName = Path.Combine(pathname, fileName)

            Dim fileInfo As New FileInfo(pathname)
            If Not fileInfo.Directory.Exists Then
                fileInfo.Directory.Create()
            End If

            Dim currPatchTotal As Integer = PatchIndex.Item(agentName).Count

            Dim mapInfoData As MapInfo = New MapInfo
            Dim currPatch As Patch


            For p As Integer = 0 To currPatchTotal - 1 'point start at 0
                currPatch = Me._PatchIndex.Item(agentName).Item(p)
                Dim origin As Pose2D = currPatch.EstimatedOrigin
                Dim lpose As New Pose2D(0, 0, 0)
                Dim gpose As Pose2D = origin.ToGlobal(lpose)


                mapInfoData.Points.EnqueueData(New PointData(gpose.X / 1000, gpose.Y / 1000, gpose.Rotation, currPatch.Scan.MeasuredTime))

                'should add features as x, y, th, time

            Next

            Tools.MapInfo.Save(pathFileName, mapInfoData)


        Next




    End Sub

#End Region

End Class
