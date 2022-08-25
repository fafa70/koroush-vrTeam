Imports System.IO
Imports System.Net
Imports System.Net.Sockets
Imports System.Text
Imports System.Math

Imports UvARescue.Agent
Imports UvARescue.Math
Imports UvARescue.Slam

Imports MathNet.Numerics
Imports MathNet.SignalProcessing.DataSources

Public Class SkinDetector
    Implements IImageAnalysis

#Region "Constructor"

    Public Sub New()

        Me._VictimPartsInView = 0

        Dim imageAnalysisPath As String
        Dim fileName As String

        Me._SkinModel = New ColorHistogram()

        imageAnalysisPath = My.Application.Info.DirectoryPath
        For i As Integer = 1 To 4
            imageAnalysisPath = Directory.GetParent(imageAnalysisPath).ToString
        Next
        imageAnalysisPath += "\ImageAnalysis"
        fileName = Path.Combine(imageAnalysisPath, "SkinModel.hist")
        Me.SkinModel.LoadFromFile(fileName, False)

        Me._NonSkinModel = New ColorHistogram()
        fileName = Path.Combine(imageAnalysisPath, "NonSkinModel.hist")
        Me.NonSkinModel.LoadFromFile(fileName, False)

        Me._Theta = 0.5 'default value

        Me._avgVictPartLoc = New Vector2

        Me._BirdEyeImageWidthHeight = 400
        Me._OmniViewRadiusPixels = 170
        Me._CameraImageSize = New Size(480, 360)

        ReDim Me._BirdEyeConversionArray(Me._BirdEyeImageWidthHeight ^ 2)
        'Me.ConstructBirdEyeConvArr(Me._OmniViewRadiusPixels, Me._BirdEyeImageWidthHeight)

    End Sub

    Public Sub New(ByVal slam As ManifoldSlam, ByVal theta As Integer)
        Me.New()
        Me._slam = slam
        Me._Theta = theta / 100
    End Sub

#End Region

#Region "Properties"

    Dim _SkinModel As ColorHistogram
    Public ReadOnly Property SkinModel() As ColorHistogram
        Get
            Return Me._SkinModel
        End Get
    End Property

    Dim _NonSkinModel As ColorHistogram
    Public ReadOnly Property NonSkinModel() As ColorHistogram
        Get
            Return Me._NonSkinModel
        End Get
    End Property

    Dim _Theta As Double
    Public ReadOnly Property Theta() As Double
        Get
            Return Me._Theta
        End Get
    End Property

    Dim _ConnCompMap() As Integer
    Public ReadOnly Property ConnCompMap() As Integer()
        Get
            Return Me._ConnCompMap
        End Get
    End Property

    Dim _VictimPartsInView As Integer
    Public ReadOnly Property VictimPartsInView() As Integer
        Get
            Return Me._VictimPartsInView
        End Get
    End Property

    Dim _skinPixelPercentage As Double
    Public ReadOnly Property SkinPixelPercentage() As Double
        Get
            Return Me._skinPixelPercentage
        End Get
    End Property

    Dim _avgSkinPixelProbability As Double
    Public ReadOnly Property AvgSkinPixelProbability() As Double
        Get
            Return Me._avgSkinPixelProbability
        End Get
    End Property

    Dim _avgVictPartLoc As Vector2
    Public ReadOnly Property AvgVictPartLoc() As Vector2
        Get
            Return Me._avgVictPartLoc
        End Get
    End Property

    Dim _slam As ManifoldSlam
    Public ReadOnly Property Slam() As ManifoldSlam
        Get
            Return Me._slam
        End Get
    End Property

    Dim _CameraImageSize As System.Drawing.Size
    Public ReadOnly Property CameraImageSize() As System.Drawing.Size
        Get
            Return Me._CameraImageSize
        End Get
    End Property


    Dim _OmniViewRadiusPixels As Integer
    Public ReadOnly Property OmniViewRadiusPixels() As Integer
        Get
            Return Me._OmniViewRadiusPixels
        End Get
    End Property

    Dim _BirdEyeImageWidthHeight As Integer
    Public ReadOnly Property BirdEyeImageWidthHeight() As Integer
        Get
            Return Me._BirdEyeImageWidthHeight
        End Get
    End Property

    Dim _BirdEyeConversionArray As Integer()
    Public ReadOnly Property BirdEyeConversionArray() As Integer()
        Get
            Return Me._BirdEyeConversionArray
        End Get
    End Property

#End Region

#Region "Process functions"

    Public Sub ProcessSensorUpdate(ByVal sensor As Agent.Sensor) Implements IImageAnalysis.ProcessSensorUpdate
        Select Case sensor.SensorType

            Case CameraSensor.SENSORTYPE_CAMERA
                Me.ProcessCameraData(sensor.Agent, DirectCast(sensor, CameraSensor).PopData)

            Case VictimSensor.SENSORTYPE_VICTIM ' 2007 Competition
                Me.ProcessVictData(sensor.Agent, DirectCast(sensor, VictimSensor).CurrentData)

        End Select
    End Sub

    Private Sub ProcessCameraData(ByVal agent As Agent.Agent, ByVal camData As CameraData)

        ' process one out of five images
        If camData.Sequence Mod 5 = 0 Then
            Dim victDetected As Boolean = False
            Dim method As String = ""
            Dim histIm As Bitmap

            Try
                histIm = Me.HistImage(camData)
            Catch ex As Exception
                Console.WriteLine("Error occurred while trying to create Histogram.")
                Console.WriteLine(ex.Message & vbNewLine & ex.StackTrace)
                Return
            End Try

            If Me.SkinPixelPercentage > 0.01 And Me.VictimPartsInView > 0 Then
                victDetected = True
                method = String.Format("_SPP{0:f4}", Me.SkinPixelPercentage)
            End If

            If victDetected Then
                'Console.WriteLine("[SkinDetector]-- Vict Detected")
                If Not Me._slam Is Nothing Then
                    Me.Slam.ProcessVictimPicture(agent, Me.AvgVictPartLoc, histIm, Me.SkinPixelPercentage)
                End If
                SaveHistImAsJpeg(histIm, agent.Name, camData.Sequence, method)
            Else
                'Console.WriteLine("[SkinDetector]-- No Vict")
            End If
        End If

    End Sub

    Protected Sub SaveHistImAsJpeg(ByVal histIm As Bitmap, ByVal agentName As String, ByVal sequence As Integer, ByVal method As String)
        Dim imFile As String

        Dim location As String = "_NoLocData_"
        If AvgVictPartLoc.X > Single.MinValue Then
            location = String.Format("_{0:F2}-{1:F2}_", Me.AvgVictPartLoc.X, Me.AvgVictPartLoc.Y)
        End If

        If sequence < 10 Then
            imFile = String.Concat("00", sequence, location, method, ".jpg")
        ElseIf sequence < 100 Then
            imFile = String.Concat("0", sequence, location, method, ".jpg")
        Else
            imFile = String.Concat(sequence, location, method, ".jpg")
        End If

        Dim fileName As String
        fileName = Path.Combine(My.Application.Info.DirectoryPath, "Images\")
        fileName = Path.Combine(fileName, agentName)
        fileName = Path.Combine(fileName, imFile)
        Dim fileInfo As New FileInfo(fileName)
        If Not fileInfo.Directory.Exists Then
            fileInfo.Directory.Create()
        End If
        histIm.Save(fileName, System.Drawing.Imaging.ImageFormat.Jpeg)

    End Sub

    Protected Overridable Sub ProcessVictData(ByVal agent As Agent.Agent, ByVal victim As VictimData)

        If victim.PartCount > 0 Then

            Me._avgVictPartLoc.X = 0
            Me._avgVictPartLoc.X = 0

            Dim sumX As Double = 0.0
            Dim sumY As Double = 0.0

            Me._VictimPartsInView = victim.PartCount

            For Each part As VictimData.VictimPart In victim.Parts
                sumX += part.X
                sumY += part.Y
            Next

            Me._avgVictPartLoc.X = sumX / victim.PartCount
            Me._avgVictPartLoc.Y = sumY / victim.PartCount

        Else

            Me._avgVictPartLoc.X = Single.MinValue
            Me._avgVictPartLoc.Y = Single.MinValue
            Me._VictimPartsInView = 0

        End If

    End Sub

#End Region

#Region "HistImage en ConnComp"

    Public Function HistImage(ByVal camData As CameraData) As Bitmap
        Dim imLength As Integer = CInt(camData.RawData.Length / 3)

        ReDim Me._ConnCompMap(imLength)

        Dim pSkin, pNonSkin, likelyhood As Double
        Dim red, green, blue As Byte
        Dim r(imLength), g(imLength), b(imLength) As Complex

        Dim numSkinPixels As Integer = 0
        Dim sumSkinProbs As Double = 0.0

        Dim skinFound As Boolean
        For index As Integer = 0 To imLength - 1

            red = camData.RawData(index * 3)
            green = camData.RawData(index * 3 + 1)
            blue = camData.RawData(index * 3 + 2)

            pSkin = Me.SkinModel.GetProbability(red, green, blue)
            pNonSkin = Me.NonSkinModel.GetProbability(red, green, blue)

            skinFound = False

            likelyhood = 0
            If pSkin > 0 Then
                If pNonSkin > 0 Then

                    likelyhood = pSkin / pNonSkin
                    If likelyhood > Me.Theta Then
                        skinFound = True
                    End If

                Else 'pSkin > 0 && pNonSkin = 0

                    skinFound = True

                End If
            End If

            If skinFound Then

                r(index).Real = 1
                g(index).Real = 0
                b(index).Real = 0

                numSkinPixels += 1
                sumSkinProbs += pSkin
                Me._ConnCompMap(index) = 1

            Else

                r(index).Real = red / Byte.MaxValue / 1.5
                g(index).Real = green / Byte.MaxValue / 1.5
                b(index).Real = blue / Byte.MaxValue / 1.5

            End If
        Next

        TwoPassConnCompMaker(camData)

        Me._skinPixelPercentage = (numSkinPixels / imLength)
        Me._avgSkinPixelProbability = sumSkinProbs / numSkinPixels

        Dim cp As ComplexPart = ComplexPart.Real
        Return BitmapConverter.WriteChannel(camData.Bitmap.Size, r, cp, g, cp, b, cp)

    End Function

    Private Sub TwoPassConnCompMaker(ByVal camData As CameraData)

        Dim curlabel As Integer = 0
        For index As Integer = camData.Width To Me._ConnCompMap.Length - camData.Width - 1

            If Not (Me.ConnCompMap(index) = 0) Then
            ElseIf Not (index - camData.Width - 1) < 0 AndAlso Not (Me.ConnCompMap(index - camData.Width - 1) = 0) Then
                Me.ConnCompMap(index) = Me.ConnCompMap(index - camData.Width - 1)
            ElseIf Not (index - 1) < 0 AndAlso Not (Me.ConnCompMap(index - 1) = 0) AndAlso Not (index - camData.Width) < 0 AndAlso Not (Me.ConnCompMap(index - camData.Width) = 0) Then
                curlabel += 1
                Me.ConnCompMap(index) = curlabel
            ElseIf Not (index - camData.Width) < 0 AndAlso Not (Me.ConnCompMap(index - camData.Width) = 0) Then
                Me.ConnCompMap(index) = Me.ConnCompMap(index - camData.Width)
            ElseIf Not (index - 1) < 0 AndAlso Not (Me.ConnCompMap(index - 1) = 0) Then
                Me.ConnCompMap(index) = Me.ConnCompMap(index - 1)
            ElseIf Not (index - camData.Width) < 0 Then
                Me.ConnCompMap(index) = Me.ConnCompMap(index - camData.Width)
            End If
        Next

    End Sub

#End Region

#Region "OmniCam Image Transformation functions"

    Private Sub ConstructBirdEyeConvArr(ByVal radius As Integer, ByVal BEImSize As Integer)

        Dim corrBirdEyeX, corrBirdEyeY, omniImX, omniImY, index As Integer
        Dim d, pitch, yaw, multiplier As Double

        index = 0
        For birdEyeX As Integer = 0 To BEImSize - 1
            For birdEyeY As Integer = 0 To BEImSize - 1

                corrBirdEyeX = birdEyeX - BEImSize / 2
                corrBirdEyeY = birdEyeY - BEImSize / 2

                d = (corrBirdEyeX ^ 2 + corrBirdEyeY ^ 2) ^ 0.5
                pitch = Atan(d / 30)

                yaw = Atan2(corrBirdEyeX, corrBirdEyeY)

                multiplier = (radius / 2) * pitch / (0.5 * 3.141572)

                omniImX = multiplier * Cos(yaw) + Me._CameraImageSize.Width / 2
                omniImY = multiplier * Sin(yaw) + Me._CameraImageSize.Height / 2

                Me._BirdEyeConversionArray(index) = omniImY * Me._CameraImageSize.Width + omniImX
                index = index + 1
            Next
        Next

    End Sub

    Protected Function Omni2BirdEyeTransform(ByVal camdata As CameraData)

        Dim imLength As Integer = camdata.RawData.Length / 3
        Dim r(imLength), g(imLength), b(imLength) As Complex

        For index As Integer = 0 To Me._BirdEyeConversionArray.Length - 1
            r(index).Real = camdata.RawData(Me._BirdEyeConversionArray(index) * 3) / Byte.MaxValue
            g(index).Real = camdata.RawData(Me._BirdEyeConversionArray(index) * 3 + 1) / Byte.MaxValue
            b(index).Real = camdata.RawData(Me._BirdEyeConversionArray(index) * 3 + 2) / Byte.MaxValue
        Next

        Dim cp As ComplexPart = ComplexPart.Real
        Dim size As System.Drawing.Size
        size.Height = Me._BirdEyeImageWidthHeight
        size.Width = Me._BirdEyeImageWidthHeight
        Return BitmapConverter.WriteChannel(size, r, cp, g, cp, b, cp)
    End Function

#End Region

#Region "Report"

    Public Sub Report() Implements IImageAnalysis.Report
    End Sub

#End Region

End Class
