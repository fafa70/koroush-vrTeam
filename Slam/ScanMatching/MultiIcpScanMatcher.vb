Imports UvARescue.Agent

Public Class MultiIcpScanMatcher
    Inherits IcpScanMatcher

    Private wsmCounter As Integer = 0
    Private idcCounter As Integer = 0
    Private insCounter As Integer = 0


    Private wsm As New WeightedScanMatcher
    Private idc As New IdcScanMatcher

    Protected Friend Overrides Function MatchPoints(ByVal manifold As Manifold, ByVal points1() As Math.Vector2, ByVal points2() As Math.Vector2, ByVal odometry As Math.Pose2D, ByVal dangle As Double, ByVal filter1() As Boolean, ByVal filter2() As Boolean) As MatchResult

        Dim threshold As Single = 1000.0F

        Dim wsmResult As MatchResult = Me.wsm.MatchPoints(manifold, points1, points2, odometry, dangle, filter1, filter2)
        If wsmResult.Covariance(0, 0) > threshold OrElse wsmResult.Covariance(1, 1) > threshold Then
            Dim idcResult As MatchResult = Me.idc.MatchPoints(manifold, points1, points2, odometry, dangle, filter1, filter2)
            If idcResult.Covariance(0, 0) > threshold OrElse idcResult.Covariance(1, 1) > threshold Then
                Me.insCounter += 1
                Me.Debug("INS")

                'create result with just raw odometry
                With idcResult
                    Return New MatchResult(.RawOdometryPose, .RawOdometryPose, New Math.Matrix3, .Distance, 0, 0, 0, True)
                End With


            Else
                Me.idcCounter += 1
                Me.Debug("IDC")
                Return idcResult

            End If

        Else
            wsmCounter += 1
            Me.Debug("WSM")
            Return wsmResult

        End If

    End Function

    Private Sub Debug(ByVal used As String)
        Console.WriteLine(String.Format("[Multi-ICP] - Used {0}, counters (WSM/IDC/INS): {1} / {2} / {3}", used, Me.wsmCounter, Me.idcCounter, Me.insCounter))
    End Sub

End Class
