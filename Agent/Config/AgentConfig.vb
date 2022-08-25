Imports UvARescue.Tools

Public Class AgentConfig
    Inherits Config

    Public Sub New(ByVal agentName As String)
        Me._AgentName = agentName
    End Sub

    Private _AgentName As String
    Public Property AgentName() As String
        Get
            Return Me._AgentName
        End Get
        Set(ByVal value As String)
            Me._AgentName = value
        End Set
    End Property

    Public Property AgentType() As String
        Get
            Return Me.GetConfig(Me.AgentName, "AgentType")
        End Get
        Set(ByVal value As String)
            Me.SetConfig(Me.AgentName, "AgentType", value)
        End Set
    End Property

    Public Property RobotModel() As String
        Get
            Return Me.GetConfig(Me.AgentName, "RobotModel")
        End Get
        Set(ByVal value As String)
            Me.SetConfig(Me.AgentName, "RobotModel", value)
        End Set
    End Property
    Public Property StartLocation() As String
        Get
            Dim value As String = Me.GetConfig(Me.AgentName, "StartLocation")
            Return Me.CleanStartPositionValue(value)
        End Get
        Set(ByVal value As String)
            value = Me.CleanStartPositionValue(value)
            Me.SetConfig(Me.AgentName, "StartLocation", value)
        End Set
    End Property
    Public Property StartRotation() As String
        Get
            Dim value As String = Me.GetConfig(Me.AgentName, "StartRotation")
            Return Me.CleanStartPositionValue(value)
        End Get
        Set(ByVal value As String)
            value = Me.CleanStartPositionValue(value)
            Me.SetConfig(Me.AgentName, "StartRotation", value)
        End Set
    End Property

    ' Public Property PathPlanGoal() As String
    '    Get
    'Dim value As String = Me.GetConfig(Me.AgentName, "PathPlanGoal")
    '       Return Me.CleanStartPositionValue(value)
    '  End Get
    ' Set(ByVal value As String)
    '    value = Me.CleanStartPositionValue(value)
    '   Me.SetConfig(Me.AgentName, "PathPlanGoal", value)
    'End Set
    'End Property

    ''' <summary>
    ''' UsarSim does not like spaces in startlocation and -rotation.
    ''' </summary>
    ''' <param name="value"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Protected Function CleanStartPositionValue(ByVal value As String) As String
        Dim clean As String = value
        If Not String.IsNullOrEmpty(clean) Then
            Dim parts() As String = Strings.Split(clean, ",")
            For i As Integer = 0 To parts.Length - 1
                parts(i) = parts(i).Trim
            Next
            clean = Strings.Join(parts, ",")
        End If
        Return clean
    End Function


    Public Property SpawnFromCommander() As Boolean
        Get
            Return Me.GetConfig(Me.AgentName, "SpawnFromCommander") = Boolean.TrueString
        End Get
        Set(ByVal value As Boolean)
            Me.SetConfig(Me.AgentName, "SpawnFromCommander", value.ToString)
        End Set
    End Property
    Public Property AgentNumber() As Integer
        Get
            Dim nr As String = Me.GetConfig(Me.AgentName, "AgentNumber")
            If IsNumeric(nr) Then
                Return CInt(Double.Parse(nr))
            Else
                Return 0
            End If
        End Get
        Set(ByVal value As Integer)
            Me.SetConfig(Me.AgentName, "AgentNumber", CStr(value))
        End Set
    End Property

    Public Property LogPlayback() As Boolean
        Get
            Return Me.GetConfig(Me.AgentName, "LogPlayback") = Boolean.TrueString
        End Get
        Set(ByVal value As Boolean)
            Me.SetConfig(Me.AgentName, "LogPlayback", value.ToString)
        End Set
    End Property
    Public Property LogFile() As String
        Get
            Return Me.GetConfig(Me.AgentName, "LogFile")
        End Get
        Set(ByVal value As String)
            Me.SetConfig(Me.AgentName, "LogFile", value)
        End Set
    End Property
    Public Property LogFileFormat() As String
        Get
            Return Me.GetConfig(Me.AgentName, "LogFileFormat")
        End Get
        Set(ByVal value As String)
            Me.SetConfig(Me.AgentName, "LogFileFormat", value)
        End Set
    End Property

    Public Property UseLogger() As Boolean
        Get
            Return Me.GetConfig(Me.AgentName, "UseLogger") = Boolean.TrueString
        End Get
        Set(ByVal value As Boolean)
            Me.SetConfig(Me.AgentName, "UseLogger", value.ToString)
        End Set
    End Property
    Public Property UseImageServer() As Boolean
        Get
            Return Me.GetConfig(Me.AgentName, "UseImageServer") = Boolean.TrueString
        End Get
        Set(ByVal value As Boolean)
            Me.SetConfig(Me.AgentName, "UseImageServer", value.ToString)
        End Set
    End Property
    Public Property UseMultiView() As Boolean
        Get
            Return Me.GetConfig(Me.AgentName, "UseMultiView") = Boolean.TrueString
        End Get
        Set(ByVal value As Boolean)
            Me.SetConfig(Me.AgentName, "UseMultiView", value.ToString)
        End Set
    End Property
    Public Property MultiViewPanels() As String
        Get
            Return Me.GetConfig(Me.AgentName, "MultiViewPanels")
        End Get
        Set(ByVal value As String)
            Me.SetConfig(Me.AgentName, "MultiViewPanels", value)
        End Set
    End Property

    ' RIKNOTE: this can return "FollowCorridorBehavior" now too
    Public Property BehaviorMode() As String
        Get
            Return Me.GetConfig(Me.AgentName, "BehaviorMode")
        End Get
        Set(ByVal value As String)
            Me.SetConfig(Me.AgentName, "BehaviorMode", value)
        End Set
    End Property
    Public Property BehaviorBalance() As Double
        Get
            Dim sigma As String = Me.GetConfig(Me.AgentName, "BehaviorBalance")
            If IsNumeric(sigma) Then
                Return Double.Parse(sigma)
            Else
                Return 1.0
            End If
        End Get
        Set(ByVal value As Double)
            Me.SetConfig(Me.AgentName, "BehaviorBalance", CStr(value))
        End Set
    End Property

    Public Property MappingMode() As String
        Get
            Return Me.GetConfig(Me.AgentName, "SLAM-MappingMode")
        End Get
        Set(ByVal value As String)
            Me.SetConfig(Me.AgentName, "SLAM-MappingMode", value)
        End Set
    End Property
    Public Property SeedMode() As String
        Get
            Return Me.GetConfig(Me.AgentName, "SLAM-SeedMode")
        End Get
        Set(ByVal value As String)
            Me.SetConfig(Me.AgentName, "SLAM-SeedMode", value)
        End Set
    End Property
    Public Property ScanMatcher() As String
        Get
            Return Me.GetConfig(Me.AgentName, "SLAM-ScanMatcher")
        End Get
        Set(ByVal value As String)
            Me.SetConfig(Me.AgentName, "SLAM-ScanMatcher", value)
        End Set
    End Property
    Public Property UseNoise() As Boolean
        Get
            Return Me.GetConfig(Me.AgentName, "UseNoise") = Boolean.TrueString
        End Get
        Set(ByVal value As Boolean)
            Me.SetConfig(Me.AgentName, "UseNoise", value.ToString)
        End Set
    End Property
    Public Property NoiseSigma() As Double
        Get
            Dim sigma As String = Me.GetConfig(Me.AgentName, "NoiseSigma")
            If IsNumeric(sigma) Then
                Return Double.Parse(sigma)
            Else
                Return 0
            End If
        End Get
        Set(ByVal value As Double)
            Me.SetConfig(Me.AgentName, "NoiseSigma", CStr(value))
        End Set
    End Property


    Public Property SkinDetectorMode() As String
        Get
            Return Me.GetConfig(Me.AgentName, "SkinDetectotMode")
        End Get
        Set(ByVal value As String)
            Me.SetConfig(Me.AgentName, "SkinDetectotMode", value)
        End Set
    End Property

    Public Property TeacherMode() As String
        Get
            Return Me.GetConfig(Me.AgentName, "TeacherMode")
        End Get
        Set(ByVal value As String)
            Me.SetConfig(Me.AgentName, "TeacherMode", value)
        End Set
    End Property

    'End Property

    Public Property DetectorTheta() As String
        Get
            Return Me.GetConfig(Me.AgentName, "DetectorTheta")
        End Get
        Set(ByVal value As String)
            Me.SetConfig(Me.AgentName, "DetectorTheta", value)
        End Set
    End Property

End Class
