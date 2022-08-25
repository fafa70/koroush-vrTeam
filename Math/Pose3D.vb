Imports System.Math

<Serializable()> _
Public Class Pose3D

#Region " Constructor and Properties "

    Private _Position As Vector3
    Private _Rotation As Vector3

    Public Sub New()
        Me.New(New Vector3(0, 0, 0), New Vector3(0, 0, 0))
    End Sub

    Public Sub New(ByVal position As Vector3, ByVal rotation As Vector3)
        Me.Position = position
        Me.Rotation = rotation
    End Sub

    Public Sub New(ByVal startLocation As String, ByVal startRotation As String)
        Dim parts() As String = Strings.Split(startLocation, ",")

        If parts.Length = 3 Then
            Me.Position.X = Single.Parse(parts(0))
            Me.Position.Y = Single.Parse(parts(1))
            Me.Position.Z = Single.Parse(parts(2))
        Else
            Me.Position = New Vector3(0, 0, 0)
        End If


        parts = Strings.Split(startRotation, ",")
        If parts.Length = 3 Then
            Me.Rotation.X = Single.Parse(parts(0))
            Me.Rotation.Y = Single.Parse(parts(1))
            Me.Rotation.Z = Single.Parse(parts(2))
        Else
            Me.Rotation = New Vector3(0, 0, 0)
        End If

    End Sub


    Public Sub New(ByVal X As Double, ByVal Y As Double, ByVal Z As Double, ByVal Roll As Double, ByVal Pitch As Double, ByVal Yaw As Double)
        Me.Position.X = X
        Me.Position.Y = Y
        Me.Position.Z = Z

        Me.Rotation.X = Roll
        Me.Rotation.Y = Pitch
        Me.Rotation.Z = Yaw

    End Sub

    Public Property Position() As Vector3
        Get
            Return _Position
        End Get
        Set(ByVal value As Vector3)
            Me._Position = value
        End Set
    End Property

    Public Property X() As Double
        Get
            Return Me._Position.X
        End Get
        Set(ByVal value As Double)
            Me._Position.X = value
        End Set
    End Property

    Public Property Y() As Double
        Get
            Return Me._Position.Y
        End Get
        Set(ByVal value As Double)
            Me._Position.Y = value
        End Set
    End Property

    Public Property Z() As Double
        Get
            Return Me._Position.Z
        End Get
        Set(ByVal value As Double)
            Me._Position.Z = value
        End Set
    End Property

    Public Property Rotation() As Vector3
        Get
            Return _Rotation
        End Get
        Set(ByVal value As Vector3)
            Me._Rotation = value
        End Set
    End Property

    Public Property Roll() As Double
        Get
            Return Me._Rotation.X
        End Get
        Set(ByVal value As Double)
            Me._Rotation.X = value
        End Set
    End Property

    Public Property Pitch() As Double
        Get
            Return Me._Rotation.Y
        End Get
        Set(ByVal value As Double)
            Me._Rotation.Y = value
        End Set
    End Property

    Private _Yaw As Double
    Public Property Yaw() As Double
        Get
            Return Me._Rotation.Z
        End Get
        Set(ByVal value As Double)
            Me._Rotation.Z = value
        End Set
    End Property

#End Region


End Class

