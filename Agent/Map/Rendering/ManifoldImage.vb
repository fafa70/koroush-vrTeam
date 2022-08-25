Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports System.Drawing.Imaging
Imports System.Math
Imports System.Runtime.InteropServices

Imports AForge
Imports AForge.ImaginG
Imports AForge.Imaging.Filters


Imports UvARescue.Math
Imports UvARescue.Tools


''' <summary>
''' Manages a multi-layer, double-buffered image that depicts the
''' manifold. The image is maintained in real-time, as new data is 
''' retrieved from manifold.
''' 
''' The image does not render anything itself, all painting logic is encapsulated
''' by the Layers.
''' 
''' As the Image handles all threading and locking issues, the layers can 
''' assume a single-threaded environment.
''' </summary>
''' <remarks></remarks>
Public Class ManifoldImage
    Implements IManifoldObserver, IDisposable

    'the following threads access this object:
    '- UI-thread:
    '   - to construct or dispose the object
    '   - to render the image on screen
    '- agent thread
    '   - to notify of new manifold data
    '- background thread
    '   - to render new manifold data

    Private Mutex As New Object


#Region " Constructor / Destructor "

    Public Sub New(ByVal manifold As Manifold, ByVal resolution As Integer, ByVal debugArea As Boolean)
        If IsNothing(manifold) Then Throw New ArgumentNullException("manifold")

        Me._Manifold = manifold
        Me._Resolution = resolution
        Me._DebugArea = debugArea

        Me.InitializeLayers()
        Me.ResetImage()

        Me._Manifold.AddObserver(Me)

    End Sub

    Private disposedValue As Boolean = False ' To detect redundant calls
    Protected Overridable Sub Dispose(ByVal disposing As Boolean)
        If disposing Then

            SyncLock Me.Mutex
                Try
                    If Not IsNothing(Me._Manifold) Then
                        Me._Manifold.RemoveObserver(Me)
                    End If

                    For Each view As IManifoldView In Me.views
                        DetachView(view)
                    Next

                    For Each layer As ManifoldLayer In Me.layers
                        layer.Dispose()
                    Next
                    Me.layers.Clear()

                Catch ex As Exception
                    Console.Error.WriteLine(ex.ToString)

                End Try

            End SyncLock

        End If
    End Sub

    Public Sub Dispose() Implements IDisposable.Dispose
        ' Do not change this code.  Put cleanup code in Dispose(ByVal disposing As Boolean) above.
        Dispose(True)
        GC.SuppressFinalize(Me)
    End Sub

#End Region

#Region " Maintain Image Bounds "

    Private _ImageNotYetSized As Boolean = True
    Private _ImageWidth As Integer = 1
    Private _ImageHeight As Integer = 1

    Public ReadOnly Property ImageWidth() As Integer
        Get
            Return Me._ImageWidth
        End Get
    End Property
    Public ReadOnly Property ImageHeight() As Integer
        Get
            Return Me._ImageHeight
        End Get
    End Property
    Public ReadOnly Property ImageSize() As Size
        Get
            Return New Size(Me._ImageWidth, Me._ImageHeight)
        End Get
    End Property
    Public ReadOnly Property ImageRect() As Rectangle
        Get
            Return New Rectangle(0, 0, Me._ImageWidth, Me._ImageHeight)
        End Get
    End Property

    ''' <summary>
    ''' Invoked by layers to make sure the image is sizable enough
    ''' </summary>
    ''' <param name="bounds"></param>
    ''' <remarks></remarks>
    Friend Sub EnsureMinimumImageBounds(ByVal bounds As RectangleF)

        'add some padding
        bounds.Inflate(10, 10)

        Dim imgRect As RectangleF = Me.ImageRect
        If Not imgRect.Contains(bounds) Then

            'img needs to be enlarged to accomodate rect

            'construct a new rectangle that will be large enough
            Dim newRect As RectangleF
            If Me._ImageNotYetSized Then
                'don't include the dummy rect currently set
                newRect = Me.RoundedUpRect(bounds, 10)
            Else
                'take the union of the current and new size
                newRect = Me.RoundedUpRect(RectangleF.Union(imgRect, bounds), 10)
            End If

            Me._ImageNotYetSized = False
            Me._ImageWidth = CInt(newRect.Width)
            Me._ImageHeight = CInt(newRect.Height)

            'compute new image offset, corrected for scaling
            Dim newOffset As New PointF()
            newOffset.X = Me.TransformationOffset.X - newRect.Left / Me.TransformationScale
            newOffset.Y = Me.TransformationOffset.Y - newRect.Top / Me.TransformationScale

            'reset transformation members to match the new offset
            Me.ResetTransformations(newOffset)

            For Each layer As ManifoldLayer In Me.layers
                layer.NotifyImageResize()
            Next

        End If

    End Sub

    ''' <summary>
    ''' Small helper function that will inflate the rectangle
    ''' in all dimensions until the first divisor of the specified
    ''' unit is found.
    ''' </summary>
    ''' <param name="minrect"></param>
    ''' <param name="unit"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Private Function RoundedUpRect(ByVal minrect As RectangleF, ByVal unit As Integer) As RectangleF

        'round up in each dimension 
        Dim left As Integer = CInt(Floor(minrect.Left / unit)) * unit
        Dim top As Integer = CInt(Floor(minrect.Top / unit)) * unit
        Dim right As Integer = CInt(Ceiling(minrect.Right / unit)) * unit
        Dim bottom As Integer = CInt(Ceiling(minrect.Bottom / unit)) * unit

        Return New RectangleF(left, top, right - left, bottom - top)

    End Function

    Private Sub ResetImage()

        'reset to small bitmap
        Me._ImageNotYetSized = True
        Me._ImageWidth = 1
        Me._ImageHeight = 1

        'reset transformations to (0,0)
        Me.ResetTransformations(New PointF(0, 0))

        'have layers reset themselves too
        For Each layer As ManifoldLayer In Me.layers
            layer.NotifyImageReset()
        Next

    End Sub

#End Region

#Region " Image to Manifold Coordinate Transformation "
    Public Function PixelToCoordinate(ByVal pixel As Vector2) As Vector2
        Dim v As New Vector2( _
            (pixel.X / Me.Resolution) * 1000 + Me.ManifoldLeft, _
            (pixel.Y / Me.Resolution) * 1000 + Me.ManifoldTop _
        )
        Return v
    End Function

#End Region


#Region " Manifold <-> Image Coordinate Transformation "

    Private _Resolution As Integer

    ''' <summary>
    ''' Image resolution in pixels/meter
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property Resolution() As Integer
        Get
            Return Me._Resolution
        End Get
        Set(ByVal value As Integer)
            Me._Resolution = value
        End Set
    End Property

    ''' <summary>
    ''' Is invoked every time the image is resized.
    ''' This routine resets all transformation-related properties
    ''' to match the current world-to-image transformation.
    ''' </summary>
    ''' <param name="offset"></param>
    ''' <remarks></remarks>
    Private Sub ResetTransformations(ByVal offset As PointF)

        'note that the manifold (i.e. the world) is in millimeters
        'and the resolution is in meters. 
        Dim scale As Single = CType(1 / 1000 * Me._Resolution, Single)

        Dim matrix As New Drawing2D.Matrix
        matrix.Scale(scale, scale)
        matrix.Translate(offset.X, offset.Y)

        Me._TransformationScale = scale
        Me._TransformationOffset = offset
        Me._TransformationMatrix = matrix

    End Sub


    'disclose transformation information through friend readonly properties
    'mostly for the purpose of informing layers about currently applicable
    'coordinate transformations

    Private _TransformationScale As Single
    Private _TransformationOffset As PointF
    Private _TransformationMatrix As Drawing2D.Matrix

    Friend ReadOnly Property TransformationScale() As Single
        Get
            Return Me._TransformationScale
        End Get
    End Property
    Friend ReadOnly Property TransformationOffset() As PointF
        Get
            Return Me._TransformationOffset
        End Get
    End Property
    Friend ReadOnly Property TransformationMatrix() As Drawing2D.Matrix
        Get
            Return Me._TransformationMatrix
        End Get
    End Property

#Region " Manifold Bounds "

    'a bunch of convenience properties 

    Public ReadOnly Property ManifoldLeft() As Integer
        Get
            Return CInt(-Me._TransformationOffset.X)
        End Get
    End Property
    Public ReadOnly Property ManifoldTop() As Integer
        Get
            Return CInt(-Me._TransformationOffset.Y)
        End Get
    End Property
    Public ReadOnly Property ManifoldWidth() As Integer
        Get
            Return CInt(Me.ImageWidth / Me.TransformationScale)
        End Get
    End Property
    Public ReadOnly Property ManifoldHeight() As Integer
        Get
            Return CInt(Me.ImageHeight / Me.TransformationScale)
        End Get
    End Property

    Public ReadOnly Property ManifoldOffset() As Point
        Get
            Return New Point(Me.ManifoldLeft, Me.ManifoldTop)
        End Get
    End Property
    Public ReadOnly Property ManifoldSize() As Size
        Get
            Return New Size(Me.ManifoldWidth, Me.ManifoldHeight)
        End Get
    End Property
    Public ReadOnly Property ManifoldRect() As Rectangle
        Get
            Return New Rectangle(Me.ManifoldLeft, Me.ManifoldTop, Me.ManifoldWidth, Me.ManifoldHeight)
        End Get
    End Property

#End Region

#End Region

#Region " Maintain Layer Stack "

    Private layers As New Stack(Of ManifoldLayer)

    Private _AprioriLayer As New GeoTiffLayer(Me)


    'Private _NotesLayer As New NotesLayer(Me)
    Private _CommsLayer As New CommsLayer(Me)
    Private _AxesLayer As New AxesLayer(Me)
    Private _AgentsLayer As New AgentsLayer(Me)
    'Private _VictimsLayer As New VictimsLayer(Me)
    'Private _FrontierLayer As New FrontierLayer(Me)
    Private _ObstaclesLayer As New ObstaclesLayer(Me)
    'Private _ObstaclesLayer As New NavigationLayer(Me)
    'Private _ClearSpaceLayer As New ClearSpaceLayer(Me, 40, 4000)
    Private _SafeSpaceLayer As New SafeSpaceLayer(Me)
    Private _FreeSpaceLayer As New FreeSpaceLayer(Me)
    Private _BackgroundLayer As New BackgroundLayer(Me)

    Public Sub AddBaseStationPoseToCommLayer(ByVal uniqueID As Guid, ByVal pose As Pose2D)
        Me._CommsLayer.AddBaseStationPose(uniqueID, pose)
    End Sub

    Protected Sub InitializeLayers()
        'add layers, top to bottom
        'Me.layers.Push(Me._NotesLayer)
        Me.layers.Push(Me._AxesLayer)
        Me.layers.Push(Me._AgentsLayer)
        Me.layers.Push(Me._CommsLayer)
        'Me.layers.Push(Me._VictimsLayer)
        'Me.layers.Push(Me._FrontierLayer)
        Me.layers.Push(Me._ObstaclesLayer)
        'Me.layers.Push(Me._ClearSpaceLayer)
        Me.layers.Push(Me._SafeSpaceLayer)
        Me.layers.Push(Me._FreeSpaceLayer)
        Me.layers.Push(Me._AprioriLayer)
        Me.layers.Push(Me._BackgroundLayer)
    End Sub

#End Region

#Region " Wrapper Properties for AprioriLayer "

    Public Property AprioriFileName() As String
        Get
            SyncLock Me.Mutex
                Return Me._AprioriLayer.FileName
            End SyncLock
        End Get
        Set(ByVal value As String)
            SyncLock Me.Mutex
                Me._AprioriLayer.FileName = value
            End SyncLock
        End Set
    End Property
    Public Property ShowAprioriMobility() As Boolean
        Get
            Return Not Me._AprioriLayer.FilterRed
        End Get
        Set(ByVal value As Boolean)
            Me._AprioriLayer.FilterRed = Not value
        End Set
    End Property
    Public Property ShowAprioriVictims() As Boolean
        Get
            Return Not Me._AprioriLayer.FilterGreen
        End Get
        Set(ByVal value As Boolean)
            Me._AprioriLayer.FilterGreen = Not value
        End Set
    End Property
    Public Property ShowAprioriComm() As Boolean
        Get
            Return Not Me._AprioriLayer.FilterBlue
        End Get
        Set(ByVal value As Boolean)
            Me._AprioriLayer.FilterBlue = Not value
        End Set
    End Property

#End Region

#Region " Frontier Extraction "

    'Public Sub RecomputeFrontiers()
    '    If Not IsNothing(Me._FrontierLayer) Then
    '        Me._FrontierLayer.RecomputeFrontiers()
    '    End If
    'End Sub

#End Region

#Region " Compute Area "

    Public Function ComputeFreeArea() As Double

        Dim pixels As Integer

        Dim tools As New FrontierTools
        Using freespace As Bitmap = tools.ExtractFreeArea(Me)

            If freespace.Width <= 1 AndAlso freespace.Height <= 1 Then
                'blobbounter does not like 1-pixel images
                pixels = 1

            Else

                Dim counter As New BlobCounter(freespace)
                Dim blobs() As Blob = counter.GetObjects(freespace)

                For Each blob As Blob In blobs

                    Dim bbits As BitmapData = blob.Image.LockBits(New Rectangle(0, 0, blob.Image.Width, blob.Image.Height), ImageLockMode.ReadOnly, PixelFormat.Format8bppIndexed)
                    For j As Integer = 0 To bbits.Height - 1
                        For i As Integer = 0 To bbits.Width - 1
                            If Marshal.ReadByte(bbits.Scan0, j * bbits.Stride + i) > 0 Then
                                pixels += 1
                            End If
                        Next
                    Next
                    blob.Image.UnlockBits(bbits)

                Next

                'cleanup
                For Each blob As Blob In blobs
                    blob.Dispose()
                Next
                blobs = Nothing

            End If
        End Using

        'transform to squared meters
        Return pixels / Me.Resolution ^ 2

    End Function

    Public Function ComputeClearedArea() As Double

        Dim pixels As Integer

        Dim tools As New FrontierTools
        Using freespace As Bitmap = tools.ExtractClearedArea(Me)

            If freespace.Width <= 1 AndAlso freespace.Height <= 1 Then
                'blobbounter does not like 1-pixel images
                pixels = 1

            Else

                Dim counter As New BlobCounter(freespace)
                Dim blobs() As Blob = counter.GetObjects(freespace)

                For Each blob As Blob In blobs

                    Dim bbits As BitmapData = blob.Image.LockBits(New Rectangle(0, 0, blob.Image.Width, blob.Image.Height), ImageLockMode.ReadOnly, PixelFormat.Format8bppIndexed)
                    For j As Integer = 0 To bbits.Height - 1
                        For i As Integer = 0 To bbits.Width - 1
                            If Marshal.ReadByte(bbits.Scan0, j * bbits.Stride + i) > 0 Then
                                pixels += 1
                            End If
                        Next
                    Next
                    blob.Image.UnlockBits(bbits)

                Next

                'cleanup
                For Each blob As Blob In blobs
                    blob.Dispose()
                Next
                blobs = Nothing

            End If
        End Using

        'transform to squared meters
        Return pixels / Me.Resolution ^ 2

    End Function
#End Region

#Region " Observe Manifold "

    Private _Manifold As Manifold = Nothing
    Public ReadOnly Property Manifold() As Manifold
        Get
            Return Me._Manifold
        End Get
    End Property


    'call rendering methods asynchronously (using BeginInvoke) in order 
    'to enable the agent to continue processing imcoming rangescans, 
    'instead of having to wait for the rendering methods to return.
    'In effect this transfer control to a background thread.

    Private Delegate Sub AgentHandler(ByVal agent As Agent, ByVal pose As Pose2D, ByVal commMatrix As Double(,))
    Private Delegate Sub PatchHandler(ByVal patch As Patch)
    Private Delegate Sub RelationHandler(ByVal relation As Relation)
    Private Delegate Sub VictimHandler(ByVal victim As VictimObservation)

    Private Sub Manifold_AgentMoved(ByVal agent As Agent, ByVal pose As Pose2D, ByVal commMatrix As Double(,)) Implements IManifoldObserver.NotifyAgentMoved
        Dim method As New AgentHandler(AddressOf Me.RenderAgent)

        'BeginInvoke delegates work to a background thread, do not use anymore
        'method.BeginInvoke(agent, pose, New AsyncCallback(AddressOf NotifyManifoldUpdatedCallback), Nothing)

        'use Invoke now to have the new pose drawn immediately
        method.Invoke(agent, pose, commMatrix)

        'notify views
        Me.NotifyManifoldUpdated()

    End Sub

    Private Sub Manifold_PatchInserted(ByVal patch As Patch) Implements IManifoldObserver.NotifyPatchInserted
        Dim method As New PatchHandler(AddressOf Me.RenderPatch)

        'don't use callback, only notify views when agents moved
        'method.BeginInvoke(patch, New AsyncCallback(AddressOf Me.NotifyManifoldUpdatedCallback), Nothing)
        method.BeginInvoke(patch, Nothing, Nothing)

    End Sub

    Private Sub Manifold_RelationInserted(ByVal relation As Relation) Implements IManifoldObserver.NotifyRelationInserted
        Dim method As New RelationHandler(AddressOf Me.RenderRelation)

        'don't use callback, only notify views when agents moved
        'method.BeginInvoke(relation, New AsyncCallback(AddressOf Me.NotifyManifoldUpdatedCallback), Nothing)
        method.BeginInvoke(relation, Nothing, Nothing)

    End Sub

    Private Sub Manifold_VictimUpdated(ByVal victim As VictimObservation) Implements IManifoldObserver.NotifyVictimUpdated
        Dim method As New VictimHandler(AddressOf Me.RenderVictim)

        'don't use callback, only notify views when agents moved
        'method.BeginInvoke(victim, New AsyncCallback(AddressOf Me.NotifyManifoldUpdatedCallback), Nothing)
        method.BeginInvoke(victim, Nothing, Nothing)

    End Sub

    Private Sub Manifold_Cleared() Implements IManifoldObserver.NotifyCleared
        Me.ResetImage()
        Me.NotifyManifoldUpdated()
    End Sub

#End Region

#Region " Attached Views "

    Private views As New List(Of IManifoldView)

    Public Sub AttachView(ByVal view As IManifoldView)
        SyncLock Me.views
            Me.views.Add(view)
        End SyncLock
    End Sub
    Public Sub DetachView(ByVal view As IManifoldView)
        SyncLock Me.views
            Me.views.Remove(view)
        End SyncLock
    End Sub


    Private Sub NotifyManifoldUpdatedCallback(ByVal result As IAsyncResult)
        Me.NotifyManifoldUpdated()
    End Sub

    Private Sub NotifyManifoldUpdated()
        SyncLock Me.views
            For Each view As IManifoldView In Me.views
                view.NotifyManifoldImageUpdated()
            Next
        End SyncLock
    End Sub

#End Region

#Region " Rendering "

    ''' <summary>
    ''' Invoked by background thread to render new patch data
    ''' </summary>
    ''' <remarks></remarks>
    Protected Overridable Sub RenderAgent(ByVal agent As Agent, ByVal pose As Pose2D, ByVal commMatrix As Double(,))
        SyncLock Me.Mutex
            For Each layer As ManifoldLayer In Me.layers
                layer.RenderAgent(agent, pose, agent._newTarget)
                If (TypeOf (layer) Is CommsLayer) Then
                    DirectCast(layer, CommsLayer).RenderCommLinks(agent, pose, commMatrix)
                End If
            Next
        End SyncLock
    End Sub



    Private _DebugArea As Boolean = False
    Private _TimeOffset As DateTime = Now
    Private _TimeLast As DateTime = Now

    ''' <summary>
    ''' Invoked by background thread to render new patch data
    ''' </summary>
    ''' <param name="patch"></param>
    ''' <remarks></remarks>
    Public Overridable Sub RenderPatch(ByVal patch As Patch)
        SyncLock Me.Mutex
            For Each layer As ManifoldLayer In Me.layers
                layer.RenderPatch(patch)
            Next

        End SyncLock
    End Sub

    ''' <summary>
    ''' Invoked by background thread to render new relation data
    ''' </summary>
    ''' <param name="relation"></param>
    ''' <remarks></remarks>
    Protected Overridable Sub RenderRelation(ByVal relation As Relation)
    End Sub


    Protected Overridable Sub RenderVictim(ByVal victim As VictimObservation)
        SyncLock Me.Mutex
            For Each layer As ManifoldLayer In Me.layers
                layer.RenderVictim(victim)
            Next
        End SyncLock
    End Sub



    Public Function GetRotationOffset(ByVal renderingMode As ManifoldRenderingMode) As Single
        Select Case renderingMode
            Case ManifoldRenderingMode.GdiPlus
                'default rendering, no offset
                Return 0.0F
            Case ManifoldRenderingMode.GeoReferenced
                'georeferenced data is rotated -90 degrees (= 90 degrees CCW) wrt GdiPlus
                Return -90.0F
            Case Else
                'should not happen ...
                Return 0.0F
        End Select
    End Function


    ''' <summary>
    ''' Invoked by UI-thread to render the manifold on the supplied 
    ''' graphics canvas.
    ''' </summary>
    ''' <param name="g"></param>
    ''' <remarks></remarks>
    Public Sub Draw(ByVal g As Graphics)
        Me.Draw(g, ManifoldRenderingMode.GdiPlus, True, True, True, False, True, True, True, True, True, True, True, True)
    End Sub

    Public Sub Draw(ByVal g As Graphics, ByVal renderingMode As ManifoldRenderingMode, ByVal background As Boolean, ByVal apriori As Boolean, ByVal freeSpace As Boolean, ByVal safeSpace As Boolean, ByVal clearSpace As Boolean, ByVal obstacles As Boolean, ByVal victims As Boolean, ByVal frontiers As Boolean, ByVal notes As Boolean, ByVal comms As Boolean, ByVal agents As Boolean, ByVal axes As Boolean)
        SyncLock Me.Mutex

            g.TextRenderingHint = Text.TextRenderingHint.AntiAlias

            'clear with transparancy
            g.Clear(Color.Transparent)

            For Each layer As ManifoldLayer In Me.layers

                If Not background AndAlso layer Is Me._BackgroundLayer Then Continue For
                If Not apriori AndAlso layer Is Me._AprioriLayer Then Continue For
                If Not freeSpace AndAlso layer Is Me._FreeSpaceLayer Then Continue For
                If Not safeSpace AndAlso layer Is Me._SafeSpaceLayer Then Continue For
                'If Not clearSpace AndAlso layer Is Me._ClearSpaceLayer Then Continue For
                If Not obstacles AndAlso layer Is Me._ObstaclesLayer Then Continue For
                'If Not frontiers AndAlso layer Is Me._FrontierLayer Then Continue For
                'If Not notes AndAlso layer Is Me._NotesLayer Then Continue For
                'If Not victims AndAlso layer Is Me._VictimsLayer Then Continue For
                If Not agents AndAlso layer Is Me._AgentsLayer Then Continue For
                If Not comms AndAlso layer Is Me._CommsLayer Then Continue For
                If Not axes AndAlso layer Is Me._AxesLayer Then Continue For

                layer.Draw(g, renderingMode)

            Next

        End SyncLock
    End Sub

#End Region

#Region " Saving "

    Public Function GetGeoReferenceData(ByRef geo As GeoReference) As Boolean

        SyncLock Me.Mutex

            Try
                geo = New GeoReference
                With geo

                    'offsets have to be in meters
                    .OffsetX = CType(Me.ManifoldTop / 1000, Single)
                    .OffsetY = CType((Me.ManifoldLeft + Me.ManifoldWidth) / 1000, Single)

                    .ScaleX = CType(1 / Me.Resolution, Single)
                    .ScaleY = -.ScaleX

                    .ShearX = 0
                    .ShearY = 0

                End With

            Catch ex As Exception
                Console.WriteLine(ex)
                Return False
            End Try

            Return True

        End SyncLock

    End Function


    Public Enum SaveOption

        Map 'excludes cleared area
        Jury 'includes cleared area
        Axes
        Notes

    End Enum

    ''' <summary>
    ''' Creates a temporary image and calls Draw to have the manifold
    ''' painted on it's graphics canvas. The manifold will be saved
    ''' in GeoTIFF format.
    ''' </summary>
    ''' <param name="bmp"></param>
    ''' <remarks></remarks>
    Public Function GetImageFileSaveData(ByRef bmp As Bitmap, ByVal saveOption As SaveOption) As Boolean

        SyncLock Me.Mutex

            Try
                'Arnoud: German Open 2008: Save with a higher resolution
                'Me.Resolution = 100

                ''reset transformations to (0,0)
                'Me.ResetTransformations(New PointF(0, 0))

                ''have layers reset themselves too
                'For Each layer As ManifoldLayer In Me.layers
                '    layer.NotifyImageReset()
                'Next

                bmp = New Bitmap(Me.ImageHeight, Me.ImageWidth)

                'draw the image 
                Using gfx As Graphics = Graphics.FromImage(bmp)

                    'draw the image 90 degrees rotated counter-clockwise
                    'this is to comply with the geotiff convention:
                    '- positive x-values go north
                    '- positive y-values go east
                    gfx.TranslateTransform(0, Me.ImageWidth)
                    gfx.RotateTransform(-90)


                    Select Case saveOption
                        Case ManifoldImage.SaveOption.Jury
                            'Me.Draw(gfx, ManifoldRenderingMode.GeoReferenced, True, False, True, False, True, True, True, False, False, False, False)
                            'Removed the ClearLayer for teleop @ robocup2010 (Okke)
                            'Added NotesLayer for teleop
                            Me.Draw(gfx, ManifoldRenderingMode.GeoReferenced, True, False, True, False, False, True, True, False, True, False, False, False)

                        Case ManifoldImage.SaveOption.Map
                            Me.Draw(gfx, ManifoldRenderingMode.GeoReferenced, True, False, True, True, False, True, True, False, False, False, True, False)

                        Case ManifoldImage.SaveOption.Axes
                            Me.Draw(gfx, ManifoldRenderingMode.GeoReferenced, False, False, False, False, False, False, False, False, False, False, False, True)

                        Case ManifoldImage.SaveOption.Notes
                            Me.Draw(gfx, ManifoldRenderingMode.GeoReferenced, False, False, False, False, False, False, False, False, True, False, False, False)

                        Case Else
                            Return False

                    End Select
                End Using

            Catch ex As Exception
                Console.WriteLine(ex)
                Return False
            End Try

            Return True

        End SyncLock

    End Function


    Public Function Save(ByVal formats As List(Of ImageFormat), ByVal basefilename As String, ByRef barProgress As System.Windows.Forms.ProgressBar) As Boolean
        'get Georeference data
        Dim geoRef As New GeoReference
        Me.GetGeoReferenceData(geoRef)

        'get all supported save options
        Dim optvalues As Array = System.Enum.GetValues(GetType(SaveOption))
        Dim optnames() As String = System.Enum.GetNames(GetType(SaveOption))

        'loop through formats
        For Each format As ImageFormat In formats

            'for each save option
            For i As Integer = 0 To optvalues.Length - 1
                'get save option
                Dim optvalue As SaveOption = CType(optvalues.GetValue(i), SaveOption)

                'construct filenames for this layer
                Dim imgFilename As String = String.Format("{0}-{1}-[{2}].{3}", basefilename, CInt(optvalue), optnames(i).ToLower, Me.GetImageFileExtension(format))
                Dim wldFilename As String = String.Format("{0}-{1}-[{2}].{3}", basefilename, CInt(optvalue), optnames(i).ToLower, Me.GetWorldFileExtension(format))

                'save the world file
                geoRef.SaveWorldFile(wldFilename)
                If (Not barProgress Is Nothing) Then
                    barProgress.Value += 1
                End If

                Dim bitmap As Bitmap = Nothing
                If GetImageFileSaveData(bitmap, optvalue) Then
                    Try
                        bitmap.Save(imgFilename, format)
                        If (Not barProgress Is Nothing) Then
                            barProgress.Value += 1
                        End If
                    Catch ex As Exception
                        Return False
                    Finally
                        If Not IsNothing(bitmap) Then
                            bitmap.Dispose()
                        End If
                    End Try
                Else
                    Return False
                End If
            Next
        Next

        Return True
    End Function

#End Region

#Region " Image saving helper functions "

    Public Function GetImageFormat(ByVal extension As String) As ImageFormat
        Select Case extension.ToLower
            Case "tif", "tiff"
                Return ImageFormat.Tiff
            Case "gif"
                Return ImageFormat.Gif
            Case "jpg", "jpe", "jpeg"
                Return ImageFormat.Jpeg
            Case "png"
                Return ImageFormat.Png
            Case Else
                Return ImageFormat.Bmp
        End Select
    End Function

    Public Function GetImageFileExtension(ByVal format As ImageFormat) As String
        If format.Equals(ImageFormat.Tiff) Then
            Return "tif"
        ElseIf format.Equals(ImageFormat.Gif) Then
            Return "gif"
        ElseIf format.Equals(ImageFormat.Jpeg) Then
            Return "jpg"
        ElseIf format.Equals(ImageFormat.Png) Then
            Return "png"
        Else
            Return "bmp"
        End If
    End Function

    Public Function GetWorldFileExtension(ByVal format As ImageFormat) As String
        Dim imExt As String = Me.GetImageFileExtension(format)
        Dim wfExt As String = ""
        If imExt.Length > 2 Then
            'first char of imgext + last char of imgext + w 
            wfExt &= imExt.Substring(0, 1)
            wfExt &= imExt.Substring(imExt.Length - 1, 1)
            wfExt &= "w"
        Else
            'default to TFW
            wfExt = "tfw"
        End If
        Return wfExt
    End Function
#End Region


End Class
