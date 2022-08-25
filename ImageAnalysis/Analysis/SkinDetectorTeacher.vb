Imports System.IO
Imports System.Net
Imports System.Net.Sockets
Imports System.Text

Imports UvARescue.Agent
Imports UvARescue.Math

Public Enum SkinDetectorTeacherMode
    SkinOnly
    NonSkinOnly
    SkinAndNonSkin
End Enum


Public Class SkinDetectorTeacher
    Implements IImageAnalysis

#Region "Constructor"

    Public Sub New(ByVal load As Boolean, ByVal mode As String)

        Me._Mode = mode

        Me._VictimInView = False
        Me._SkinModel = New ColorHistogram()
        Me._NonSkinModel = New ColorHistogram()

        If load Then
            Dim filename As String
            Select Case Me.Mode

                Case SkinDetectorTeacherMode.NonSkinOnly.ToString
                    filename = Path.Combine(My.Application.Info.DirectoryPath, "NonSkinModel.hist")
                    Me.NonSkinModel.LoadFromFile(filename, False)

                Case SkinDetectorTeacherMode.SkinAndNonSkin.ToString
                    filename = Path.Combine(My.Application.Info.DirectoryPath, "SkinModel.hist")
                    Me.SkinModel.LoadFromFile(filename, False)
                    filename = Path.Combine(My.Application.Info.DirectoryPath, "NonSkinModel.hist")
                    Me.NonSkinModel.LoadFromFile(filename, False)

                Case SkinDetectorTeacherMode.SkinOnly.ToString
                    filename = Path.Combine(My.Application.Info.DirectoryPath, "SkinModel.hist")
                    Me.SkinModel.LoadFromFile(filename, False)

            End Select
        End If

    End Sub

#End Region

#Region "Properties"

    Dim _Mode As String
    Public ReadOnly Property Mode() As String
        Get
            Return Me._Mode
        End Get
    End Property

    Dim _VictimInView As Boolean
    Public ReadOnly Property VictimInView() As Boolean
        Get
            Return Me._VictimInView
        End Get
    End Property

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

        If Me.VictimInView Then
            Me._SkinModel.AddCameraData(camData)
        Else
            Me._NonSkinModel.AddCameraData(camData)
        End If

    End Sub

    Dim count As Integer = 0

    Protected Overridable Sub ProcessVictData(ByVal agent As Agent.Agent, ByVal victim As VictimData)

        If victim.PartCount > 0 Then
            Me._VictimInView = True
            Console.WriteLine("SkinMode")
        Else
            Me._VictimInView = False
            Console.WriteLine("NonSkinMode")
        End If

    End Sub

#End Region

#Region "Report"

    Public Sub Report() Implements IImageAnalysis.Report

        Dim pathName, sFileName, nSFileName As String
        pathName = Path.Combine(My.Application.Info.DirectoryPath, "Histograms\")
        sFileName = Path.Combine(pathName, "SkinModel.hist")
        nSFileName = Path.Combine(pathName, "NonSkinModel.hist")

        Dim fileInfo As New FileInfo(sFileName)
        If Not fileInfo.Directory.Exists Then
            fileInfo.Directory.Create()
        End If

        Select Case Me.Mode

            Case "NonSkinOnly"
                Me.NonSkinModel.SaveToFile(nSFileName)

            Case "SkinAndNonSkin"
                Me.SkinModel.SaveToFile(sFileName)
                Me.NonSkinModel.SaveToFile(nSFileName)

            Case "SkinOnly"
                Me.SkinModel.SaveToFile(sFileName)

        End Select

        ' For debugging or graphical display purposes
        Dim toText As Boolean = False
        If toText Then
            Using sw As StreamWriter = New StreamWriter(Path.Combine(pathName, "SkinModelHist.txt"))
                sw.Write(Me._SkinModel.ToString())
            End Using
            Using sw As StreamWriter = New StreamWriter(Path.Combine(pathName, "NonSkinModelHist.txt"))
                sw.Write(Me._NonSkinModel.ToString())
            End Using
        End If

    End Sub

#End Region

End Class
