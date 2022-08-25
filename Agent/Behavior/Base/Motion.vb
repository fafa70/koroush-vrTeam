Public MustInherit Class Motion

    Private _x_pose As Double
    Private _y_pose As Double

    Public Sub New(ByVal control As MotionControl)
        'Console.WriteLine("[RIKNOTE] Agent\Behavior\Base\Motion.vb::New() called")
        Me._Active = False
        Me._Control = control
    End Sub

    Private _Active As Boolean

    Protected Sub New(ByVal x_pose As Double, ByVal y_pose As Double)
        ' TODO: Complete member initialization 
        _x_pose = x_pose
        _y_pose = y_pose
    End Sub

    Public ReadOnly Property Active() As Boolean
        Get
            Return Me._Active
        End Get
    End Property

    Private _Control As MotionControl
    Public ReadOnly Property Control() As MotionControl
        Get
            Return Me._Control
        End Get
    End Property

    Public Sub Activate()
        Console.WriteLine("[RIKNOTE] Agent\Behavior\Base\Motion.vb::Activate() called")
        If Not Me._Active Then
            Me._Active = True
            Me.OnActivated()
        End If
    End Sub
    Public Sub DeActivate()
        If Me._Active Then
            Me._Active = False
            Me.OnDeActivated()
        End If
    End Sub

    Protected Overridable Sub OnActivated()
        Console.WriteLine(String.Format("[Motion] - {0} Activated", Me.ToString))
    End Sub
    Protected Overridable Sub OnDeActivated()
        Console.WriteLine(String.Format("[Motion] - {0} Deactivated", Me.ToString))
    End Sub

    Public Sub NotifySensorUpdate(ByVal sensor As Sensor)
        ' RIKNOTE: does this call Motion.vb::ProcessSensorUpdate()
        ' below or a derived-type ProcessSensorUpdate() function?
        Me.ProcessSensorUpdate(sensor)
    End Sub
    Protected Overridable Sub ProcessSensorUpdate(ByVal sensor As Sensor)
    End Sub



    Public Sub NotifyPoseEstimateUpdated(ByVal pose As Math.Pose2D)
        Me.ProcessPoseEstimateUpdated(pose)
    End Sub
    Protected Overridable Sub ProcessPoseEstimateUpdated(ByVal pose As Math.Pose2D)
    End Sub

End Class
