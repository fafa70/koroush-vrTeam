Imports UvARescue.Agent
Imports UvARescue.ImageAnalysis
Imports UvARescue.Math
Imports UvARescue.Slam

Public Class UsarSkinDetAgent
    Inherits UsarSlamAgent

    Public Sub New(ByVal slam As ISlamStrategy, ByVal manifold As Manifold, ByVal imAn As IImageAnalysis, ByVal name As String, ByVal agentConfig As AgentConfig, ByVal teamConfig As TeamConfig)
        MyBase.New(slam, manifold, name, agentConfig, teamConfig)

        If IsNothing(imAn) Then Throw New ArgumentNullException("imAn")

        Me._ImAn = imAn

    End Sub

    Protected _ImAn As IImageAnalysis

    ''' <summary>
    ''' Forwards sensor readings to Image Analyser.
    ''' </summary>
    ''' <param name="sensor"></param>
    ''' <remarks></remarks>
    Protected Overrides Sub NotifySensorUpdate(ByVal sensor As Sensor)
        MyBase.NotifySensorUpdate(sensor)
        Me._ImAn.ProcessSensorUpdate(sensor)
    End Sub

    Public Overrides Sub Finish()
        Me._ImAn.Report()
    End Sub

    Public Sub activateTracker()
    End Sub

End Class


