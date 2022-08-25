Imports System.Math

<Serializable()> _
Public Class Pose2D

#Region " Constructor and Properties "

    Private _Position As Vector2
    Private _Rotation As Double

    Public Sub New()
        Me.New(New Vector2(0, 0), 0)
    End Sub
    Public Sub New(ByVal rotation As Double)
        Me.New(New Vector2(0, 0), rotation)
    End Sub

    Public Sub New(ByVal x As Double, ByVal y As Double, ByVal rotation As Double)
        Me.New(New Vector2(x, y), rotation)
    End Sub

    Public Sub New(ByVal position As Vector2, ByVal rotation As Double)
        Me.Position = position
        Me.Rotation = rotation
    End Sub

    Public Sub New(ByVal startLocation As String, ByVal startRotation As String)
        Dim parts() As String = Strings.Split(startLocation, ",")

        If parts.Length > 2 Then
            Me.Position.X = Single.Parse(parts(0))
            Me.Position.Y = Single.Parse(parts(1))
        Else
            Me.Position = New Vector2(0, 0)
        End If


        parts = Strings.Split(startRotation, ",")
        If parts.Length = 3 Then
            Me.Rotation = Single.Parse(parts(2))
        Else
            Me.Rotation = 0.0
        End If
    End Sub



    Public Property Position() As Vector2
        Get
            Return _Position
        End Get
        Set(ByVal value As Vector2)
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

    Public Property Rotation() As Double
        Get
            Return Me._Rotation
        End Get
        Set(ByVal value As Double)
            Me._Rotation = value
        End Set
    End Property

    Public Overrides Function ToString() As String
        Return Me.ToString(5, 5)
    End Function

    Public Overloads Function ToString(ByVal decimalsInPosition As Integer) As String
        Return String.Format(String.Format("{{0:f{0}}} , {{1:f{0}}}", decimalsInPosition), Me.X / 1000, Me.Y / 1000)
    End Function

    Public Overloads Function ToString(ByVal decimalsInPosition As Integer, ByVal decimalsInRotation As Integer) As String
        Return String.Format(String.Format("{{0:f{0}}} , {{1:f{0}}} / {{2:f{1}}}", decimalsInPosition, decimalsInRotation), Me.X / 1000, Me.Y / 1000, Me.Rotation)
    End Function

#End Region

#Region " Normalize Rotation "

    Public Function GetNormalizedRotation() As Double
        Dim radians As Double = Me.Rotation
        While radians > PI
            radians -= 2 * PI
        End While
        While radians <= -PI
            radians += 2 * PI
        End While
        Return radians
    End Function

#End Region

#Region " Smart Functions "

    Public Function ToGlobalMatrix() As TMatrix2D
        Return New TMatrix2D(Me.Position, Me.Rotation)
    End Function

    Public Function ToLocalMatrix() As TMatrix2D
        Dim mx As New TMatrix2D(-Me.Rotation)
        Dim merotated As Pose2D = mx * Me
        mx.Translation = -merotated.Position
        Return mx
    End Function

    Public Function ToGlobal(ByVal currentOrigin As Pose2D) As Pose2D
        Dim rotmx As TMatrix2D = currentOrigin.ToGlobalMatrix
        Dim pglobal As Pose2D = rotmx * Me
        Return pglobal
    End Function

    Public Function ToLocal(ByVal targetOrigin As Pose2D) As Pose2D
        Dim rotmx As TMatrix2D = targetOrigin.ToLocalMatrix
        Dim plocal As Pose2D = rotmx * Me
        Return plocal
    End Function

#End Region

End Class

