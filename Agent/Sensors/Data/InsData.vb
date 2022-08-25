
'SEN {Type INS} {Name INS} {Location 0.80,-7.28,-0.45} {Orientation 0.00,6.28,0.00}
Imports System.Math


<Serializable()> _
Public Class InsData
    Implements ISensorData

    Private _IsLoadedOnce As Boolean = False

    Private _OffsetX As Double
    Private _OffsetY As Double
    Private _OffsetYaw As Double

    Public Sub New()
        Me._X = 0
        Me._Y = 0
        Me._Z = 0
        Me._Roll = 0
        Me._Pitch = 0
        Me._Yaw = 0
    End Sub

    Public Sub Load(ByVal msg As System.Collections.Specialized.StringDictionary) Implements ISensorData.Load
        Dim parts() As String

        parts = Strings.Split(msg("Location"), ",")
        Me._X = Double.Parse(parts(0))
        Me._Y = Double.Parse(parts(1))
        Me._Z = Double.Parse(parts(2))

        parts = Strings.Split(msg("Orientation"), ",")
        Me._Roll = Double.Parse(parts(0))
        Me._Pitch = Double.Parse(parts(1))
        Me._Yaw = Double.Parse(parts(2))

        If Not Me.X = 0.0 And Not Me.Y = 0.0 Then
            'To protect the Theseus bug in the competition. 
            'After the competition a Z-check can be added. 
            'Spawning an agent on 0,0,0 cannot occur because this spawns a UnrealBOt
            Me._IsLoadedOnce = True
        End If

    End Sub

    Public Sub ResetOffsets(ByVal x As Double, ByVal y As Double, ByVal yaw As Double)
        If Me._IsLoadedOnce Then
            Me._OffsetX = x - Me._X
            Me._OffsetY = y - Me._Y
            Me._OffsetYaw = yaw - Me._Yaw
        End If
    End Sub

    Private _X As Double
    Public ReadOnly Property X() As Double
        Get
            Return _OffsetX + _X
        End Get
    End Property

    Private _Y As Double
    Public ReadOnly Property Y() As Double
        Get
            Return _OffsetY + _Y
        End Get
    End Property

    Private _Z As Double
    Public ReadOnly Property Z() As Double
        Get
            Return _Z
        End Get
    End Property

    Private _Roll As Double
    Public ReadOnly Property Roll() As Double
        Get
            Return _Roll
        End Get
    End Property

    Private _Pitch As Double
    Public ReadOnly Property Pitch() As Double
        Get
            Return _Pitch
        End Get
    End Property

    Private _Yaw As Double
    Public ReadOnly Property Yaw() As Double
        Get
            Return _OffsetYaw + _Yaw
        End Get
    End Property

    Public ReadOnly Property OffsetX() As Double
        Get
            Return _OffsetX
        End Get
    End Property

    Public ReadOnly Property OffsetY() As Double
        Get
            Return _OffsetY
        End Get
    End Property

    Public ReadOnly Property Offset() As Double
        Get
            Return Sqrt(Pow(_OffsetX, 2) + Pow(_OffsetY, 2))

        End Get
    End Property

End Class
