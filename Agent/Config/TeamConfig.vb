Imports UvARescue.Tools

Public Class TeamConfig
    Inherits Config

    Public Sub New(ByVal cfgfile As String)
        MyBase.New()
    End Sub

    Public Property OperatorName() As String
        Get
            Return Me.GetConfig("Team", "OperatorName")
        End Get
        Set(ByVal value As String)
            Me.SetConfig("Team", "OperatorName", value)
        End Set
    End Property

    Public Property TeamMembers() As String
        Get
            Return Me.GetConfig("Team", "TeamMembers")
        End Get
        Set(ByVal value As String)
            Me.SetConfig("Team", "TeamMembers", value)
        End Set
    End Property



    Public Property LocalHost() As String
        Get
            Return Me.GetConfig("Local", "LocalHost")
        End Get
        Set(ByVal value As String)
            Me.SetConfig("Local", "LocalHost", value)
        End Set
    End Property


    Public Property UsarSimHost() As String
        Get
            Return Me.GetConfig("Network", "UsarSimHost")
        End Get
        Set(ByVal value As String)
            Me.SetConfig("Network", "UsarSimHost", value)
        End Set
    End Property
    Public Property UsarSimPort() As Integer
        Get
            Dim port As String = Me.GetConfig("Network", "UsarSimPort")
            If IsNumeric(port) Then
                Return CInt(Double.Parse(port))
            Else
                Return My.Settings.DefaultUsarSimPort
            End If
        End Get
        Set(ByVal value As Integer)
            Me.SetConfig("Network", "UsarSimPort", CStr(value))
        End Set
    End Property

    Public Property ImageServerHost() As String
        Get
            Return Me.GetConfig("Network", "ImageServerHost")
        End Get
        Set(ByVal value As String)
            Me.SetConfig("Network", "ImageServerHost", value)
        End Set
    End Property
    Public Property ImageServerPort() As Integer
        Get
            Dim port As String = Me.GetConfig("Network", "ImageServerPort")
            If IsNumeric(port) Then
                Return CInt(Double.Parse(port))
            Else
                Return My.Settings.DefaultImageServerPort
            End If
        End Get
        Set(ByVal value As Integer)
            Me.SetConfig("Network", "ImageServerPort", CStr(value))
        End Set
    End Property

    Public Property WirelessServerHost() As String
        Get
            Return Me.GetConfig("Network", "WirelessServerHost")
        End Get
        Set(ByVal value As String)
            Me.SetConfig("Network", "WirelessServerHost", value)
        End Set
    End Property
    Public Property WirelessServerPort() As Integer
        Get
            Dim port As String = Me.GetConfig("Network", "WirelessServerPort")
            If IsNumeric(port) Then
                Return CInt(Double.Parse(port))
            Else
                Return My.Settings.DefaultWirelessServerPort
            End If
        End Get
        Set(ByVal value As Integer)
            Me.SetConfig("Network", "WirelessServerPort", CStr(value))
        End Set
    End Property

    Public Property StartPoses() As String
        Get
            Return Me.GetConfig("Map", "StartPoses")
        End Get
        Set(ByVal value As String)
            Me.SetConfig("Map", "StartPoses", value)
        End Set
    End Property

    Public Property LevelName() As String
        Get
            Return Me.GetConfig("Map", "LevelName")
        End Get
        Set(ByVal value As String)
            Me.SetConfig("Map", "LevelName", value)
        End Set
    End Property

    Public Property CreateBackups() As Boolean
        Get
            Dim createbackup As String = Me.GetConfig("Backup", "CreateBackups")
            If Not IsNothing(createbackup) AndAlso createbackup.Equals("False") Then
                Return False
            Else
                Return True
            End If
        End Get
        Set(ByVal value As Boolean)
            If value Then
                Me.SetConfig("Backup", "CreateBackups", "True")
            Else
                Me.SetConfig("Backup", "CreateBackups", "False")
            End If
        End Set
    End Property

    Public Property BackupFrequency() As Integer
        Get
            Dim backupfreq As String = Me.GetConfig("Backup", "BackupFrequency")
            If IsNumeric(backupfreq) Then
                Return CInt(Double.Parse(backupfreq))
            Else
                Return CInt(My.Settings.DefaultBackupFrequency)
            End If
        End Get
        Set(ByVal value As Integer)
            Me.SetConfig("Backup", "BackupFrequency", CStr(value))
        End Set
    End Property

End Class
