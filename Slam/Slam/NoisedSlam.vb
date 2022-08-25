Imports UvARescue.Agent
Imports UvARescue.Math
Imports UvARescue.Tools

Imports System.Math

Imports MathNet.Numerics.Distributions 'Defines IContinuousGenerator and NormalDistribution

Public Class NoisedSlam
    Inherits ManifoldSlam

    Private _NoiseGenerator As IContinuousGenerator

    Public Sub New(ByVal manifold As Manifold, ByVal matcher As IScanMatcher, ByVal mappingMode As MappingMode, ByVal seedMode As PoseEstimationSeedMode, ByVal sigma As Double)
        MyBase.New(manifold, matcher, mappingMode, seedMode)

        'zero-mean Gaussian noise
        Me._NoiseGenerator = New NormalDistribution(0, sigma)

    End Sub

    Protected Overrides Sub ProcessLaserRangeData(ByVal sensorName As String, ByVal agent As Agent.Agent, ByVal laser As Agent.LaserRangeData)
        'It is possible that a sensor is mounted on a tilt.  Not interested in that here.
        If (sensorName = "TiltedScanner") Then
            Exit Sub
        End If


        If laser.Range.Length > 0 Then
            'apply noise before forwarding the laser data to the base class
            For i As Integer = 0 To laser.Range.Length - 1
                laser.Range(i) += Me._NoiseGenerator.NextDouble
            Next
        End If

        MyBase.ProcessLaserRangeData(sensorName, agent, laser)

    End Sub

End Class
