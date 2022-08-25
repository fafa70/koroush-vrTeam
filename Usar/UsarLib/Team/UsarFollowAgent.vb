Imports UvARescue.Agent
Imports UvARescue.Math
Imports UvARescue.Slam
Imports System.Drawing
Imports System.IO
Imports System.Net
Imports System.Net.Sockets
Imports System.Text
Imports OpenCVDotNet

'set Region Of Interst
'repeat
'   receive image
'   mean-shift on the ROI
'   update ROI
'   


Public Class UsarFollowAgent
    Inherits UsarAgent

    Private image1 As OpenCVDotNet.CVImage
    Private image2 As Bitmap
    Private initialHistogram As OpenCVDotNet.CVHistogram
    Private roi As New Rectangle(240, 180, 100, 100)
    Private oldROI As New Rectangle
    Private i As Integer
    Private tracker As Boolean
    Private callback As Image.GetThumbnailImageAbort
    Private thumbnail As Image


    Public Sub New(ByVal manifold As Manifold, ByVal name As String, ByVal agentConfig As AgentConfig, ByVal teamConfig As TeamConfig)
        MyBase.New(manifold, name, agentConfig, teamConfig)
        i = 0
        UpdateHistogram()
        tracker = False
    End Sub

    Protected Overrides Sub NotifySensorUpdate(ByVal sensor As Agent.Sensor)
        'fafa comments it out 
        MyBase.NotifySensorUpdate(sensor)


        If sensor.SensorType = CameraSensor.SENSORTYPE_CAMERA And tracker = True Then
            i += 1
            Dim image As Bitmap = DirectCast(sensor, CameraSensor).PopData.Bitmap


            Dim pathName, imageFile As String
            pathName = Path.Combine(My.Application.Info.DirectoryPath, "Notify\")
            Console.WriteLine("fafa:" + pathName)
            Dim fileInfo As New FileInfo(pathName)
            If Not fileInfo.Directory.Exists Then
                fileInfo.Directory.Create()
            End If
            ' define filename and check existence directory
            Dim imgFile As String = "image" + i.ToString + ".jpg"
            imageFile = Path.Combine(pathName, imgFile)


            'Convert to CVImage
            image.Save(imageFile, System.Drawing.Imaging.ImageFormat.Jpeg)
            'image.Dispose()
            Dim image1 As New OpenCVDotNet.CVImage(imageFile, True)
            Dim bp As OpenCVDotNet.CVImage
            bp = image1.CalcBackProject(Me.initialHistogram)
            Dim conn As OpenCVDotNet.CVConnectedComp = Nothing
            Try
                conn = bp.MeanShift(roi)
            Catch ex As Exception
                MsgBox(ex.Message)
            End Try
            image2 = image1.ToBitmap
            'image1.Dispose()
            Dim objGraphics As Graphics
            objGraphics = Graphics.FromImage(image2)
            objGraphics.DrawRectangle(New Pen(Color.Red), conn.Rect)

            Dim imageFile2 As String
            pathName = Path.Combine(My.Application.Info.DirectoryPath, "Tracker\")
            Dim fileInfo2 As New FileInfo(pathName)
            If Not fileInfo2.Directory.Exists Then
                fileInfo2.Directory.Create()
            End If
            imageFile2 = Path.Combine(pathName, imgFile)

            image2.Save(imageFile2, System.Drawing.Imaging.ImageFormat.Jpeg)
            thumbnail = image2.GetThumbnailImage(132, 97, callback, System.IntPtr.Zero)
            Me.NotifyTrackerImage(thumbnail)
            'thumbnail.Dispose()
            'image2.Dispose()
            roi = conn.Rect()
            'Eigen view in USARCommanderen daarin de nieuwe image projecteren


        End If
    End Sub

#Region "Histogram function"
    Public Sub UpdateHistogram()
        Dim bpRanges() As OpenCVDotNet.CVPair = {New OpenCVDotNet.CVPair(0, 255), New OpenCVDotNet.CVPair(0, 255), New OpenCVDotNet.CVPair(0, 255)}
        Dim bpBins() As Integer = {30, 30, 30}
        ' Load image to be used as histogram

        Dim histImage As New OpenCVDotNet.CVImage("..\..\..\UsarLib\histoPic2.JPG", True)
        If Not IsNothing(histImage) Then
            roi.Width = histImage.Width
            roi.Height = histImage.Height
            Me.initialHistogram = histImage.CalcHistogram(bpBins, bpRanges)
        Else
            Console.WriteLine("[UpdateHistogram] Couldn't load UsarLib\histoPic2.JPG")
        End If
    End Sub
#End Region

#Region "Auxillary functions"
    Public Sub activateTracker()
        Me.tracker = True
    End Sub

    Public Overrides Sub toggleTracker()
        If Me.tracker = False Then
            Me.tracker = True
        Else
            Me.tracker = False
        End If
    End Sub
#End Region

End Class


