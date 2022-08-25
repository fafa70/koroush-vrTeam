Imports System.IO
Imports System.Net
Imports System.Net.Sockets
Imports System.Text
Imports System.Drawing
Imports UvARescue.Agent
Imports UvARescue.Math
Imports UvARescue.Tools

Public Class CameraSensor
    Inherits MultiStateSensor(Of CameraData)

    Public Shared ReadOnly SENSORTYPE_CAMERA As String = "Camera"

#Region " Constructor "

    Public Sub New(ByVal name As String)
        MyBase.New(SENSORTYPE_CAMERA, Name, 100)
        Me._sequence = 0
    End Sub

    Public Sub New(ByVal agentConfig As AgentConfig)
        Me.New(agentConfig, Sensor.MATCH_ANYNAME)
    End Sub
    Public Sub New(ByVal agentConfig As AgentConfig, ByVal name As String)
        Me.New(name)
        With agentConfig
            Me._spawnNumber = .AgentNumber
            If String.Equals(.MultiViewPanels, "") OrElse .UseMultiView = False Then
                Me._multiviewMode = 0
            Else
                Me._multiviewMode = CByte(.MultiViewPanels)
            End If
        End With
    End Sub

#End Region

#Region " Properties "

    Dim _sequence As Integer
    Public ReadOnly Property Sequence() As Integer
        Get
            Return _sequence
        End Get
    End Property

    Private Function NextSequence() As Integer
        If Me._sequence < Integer.MaxValue Then
            Me._sequence += 1
        Else
            Me._sequence = 0
        End If
        Return _sequence
    End Function

    Dim _spawnNumber As Integer
    Public ReadOnly Property SpawnNumber() As Integer
        Get
            Return _spawnNumber
        End Get
    End Property

    Dim _multiviewMode As Byte
    Public ReadOnly Property MultiviewMode() As Byte
        Get
            Return Me._multiviewMode
        End Get
    End Property

    Private _latestCamIm As Drawing.Bitmap
    Public Property LatestCamIm() As Drawing.Bitmap
        Get
            Return Me._latestCamIm
        End Get
        Set(ByVal value As Drawing.Bitmap)
            Me._latestCamIm = value
        End Set
    End Property

#End Region

#Region "ProcessImage"
    Protected Overrides Sub ProcessBytes(ByVal bytes() As Byte)

        Console.WriteLine("CameraSensor: receiving bytes")
        Dim data As New CameraData
        Dim viewNumber As Integer = Me.Agent.Number
        If Not IsNothing(Me.Agent) AndAlso Not IsNothing(Me.Agent.Status) Then
            viewNumber = Me.Agent.Status.View() + 1
            If viewNumber <> Me.SpawnNumber Then
                Console.WriteLine(String.Format("[CameraSensor:ProcessBytes] The viewNumber {0} is not equal to the spawnNumber {1}", viewNumber, Me.SpawnNumber))
                viewNumber = Me.SpawnNumber
            End If
        End If

        data.Load(bytes, viewNumber, Me.MultiviewMode, Me.NextSequence)
        Me.LatestCamIm = data.Bitmap
        Me.EnqueueData(data)
        Me.Agent.NotifySensorUpdate(Me)
    End Sub


    'Calculate whether two locations are within a certain range
    Public Function inRange(ByVal newX As Double, ByVal newY As Double, ByVal oldX As Double, ByVal oldY As Double, ByVal range As Double) As Boolean
        If newX >= oldX - range And newX <= oldX + range And newY >= oldY - range And newY <= oldY + range Then
            Return True
        End If

        Return False

    End Function

    Public Overrides Sub ProcessGeoMessage(ByVal msgtype As MessageType, ByVal msg As System.Collections.Specialized.StringDictionary)
        MyBase.ProcessGeoMessage(msgtype, msg)
        'GEO {Type string} {Name string Location x,y,z Orientation x,y,z Mount string} {Name string Location x,y,z Orientation x,y,z Mount string} 

        If Not IsNothing(Me.Agent) Then
            'This statement is not always reached, because Me.Agent is not assigned yet
            Console.WriteLine(String.Format("{0}: geometry values for {1} on {2} updated with GETGEO message", Me.SensorType, Me.SensorName, Me.Agent.RobotModel))
        Else
            Console.WriteLine("{0}:  geometry values updated with GETGEO message", Me.SensorType)
        End If

        Dim parts() As String

        If msgtype = MessageType.Geometry Then

            'For sensors with multiple instances ANYNAME is used. The names and GEOs of the all instances are returned. Handle the parts

            'Console.WriteLine(String.Format("{0} received GeometryMessage for  sensor with name {1}", Me.SensorName, msg("Name")))


            Dim key As String = "Name"


            parts = Strings.Split(msg(key), " ")


            If parts.Length = 7 Then
                Dim locations() As String = Strings.Split(parts(2), ",")
                Dim orientations() As String = Strings.Split(parts(4), ",")
                
                If Me.SensorName = Sensor.MATCH_ANYNAME Then
                    ' Me.SensorName = parts(0) 'readonly
                End If

                _X = Single.Parse(locations(0))
                _Y = Single.Parse(locations(1))
                _Z = Single.Parse(locations(2))

                _Roll = Single.Parse(orientations(0))
                _Pitch = Single.Parse(orientations(1))
                _Yaw = Single.Parse(orientations(2))

                'To be done,  the Mount is not HARD,  the mount is CameraPanTilt_Link2, and should be queried).

                'Should add a complete MisPkg-class


                'GEO {Type MisPkg} {Name string} {Link int} {ParentLink int} {Location x,y,z} {Orientation x,y,z} {Link int} {ParentLink int} {Location x,y,z} {Orientation x,y,z}
                'GEO {Type MisPkg} {Name CameraPanTilt} {Link 1} {ParentLink -1} {Location 0.1239,0.0000,-0.2036} {Orientation 3.1415,0.0000,0.0000} {Link 2} {ParentLink 1} {Location 0.0000,0.0000,0.0599} {Orientation 1.5707,0.0000,0.0000}

                'CONF {Type MisPkg} {Name CameraPantilt} {Link 1} {JointType Revolute} {MaxSpeed 0.17} {MaxTorque 20.00} {MinRange 1} {MaxRange 0} {Link 2} {JointType Revolute} {MaxSpeed 0.17} {MaxTorque 20} {MinRange -0.16} {MaxRange 0.40}
                'MISSTA {Time 102.09} {Name CameraPanTilt} {Link 1} {Value 0.0000} {Torque -20.0000} {Link 2} {Value -0.1600} {Torque -20.0000}
                'MISPKG {Name CameraPanTilt} {Link 1} {Value 1.5} {Link 2} {Value 0}
            End If

        End If
    End Sub

#End Region

#Region "Geometry Properties"
    Private _X As Double
    Public ReadOnly Property X() As Double
        Get
            Return Me._X
        End Get
    End Property

    Private _Y As Double
    Public ReadOnly Property Y() As Double
        Get
            Return Me._Y
        End Get
    End Property

    Private _Z As Double
    Public ReadOnly Property Z() As Double
        Get
            Return Me._Z
        End Get
    End Property

    Private _Roll As Double
    Public ReadOnly Property Roll() As Double
        Get
            Return Me._Roll
        End Get
    End Property

    Private _Pitch As Double
    Public ReadOnly Property Pitch() As Double
        Get
            Return Me._Pitch
        End Get
    End Property

    Private _Yaw As Double
    Public ReadOnly Property Yaw() As Double
        Get
            Return Me._Yaw
        End Get
    End Property

#End Region

End Class
