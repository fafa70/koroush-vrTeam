Imports UvARescue.Math
Imports System.Math

Public Class SonarSensor
    Inherits SingleStateSensor(Of SonarData)

    Private Const FRONTKEY As String = "F"
    Private Const REARKEY As String = "R"
    Private Const SKEY As String = "S" 'ATRVJr
    Private Const SONARKEY As String = "Sonar" 'Nomad

    Public Shared ReadOnly SENSORTYPE_SONAR As String = "Sonar"

    Public Sub New()
        MyBase.New(New SonarData, SENSORTYPE_SONAR, Sensor.MATCH_ANYNAME)

        Me._frontX = New Dictionary(Of Integer, Double)
        Me._frontY = New Dictionary(Of Integer, Double)
        Me._frontZ = New Dictionary(Of Integer, Double)
        Me._frontRoll = New Dictionary(Of Integer, Double)
        Me._frontPitch = New Dictionary(Of Integer, Double)
        Me._frontYaw = New Dictionary(Of Integer, Double)

        Me._frontBeamAngle = New Dictionary(Of Integer, Double)
        Me._frontMinRange = New Dictionary(Of Integer, Double)
        Me._frontMaxRange = New Dictionary(Of Integer, Double)

        Me._rearX = New Dictionary(Of Integer, Double)
        Me._rearY = New Dictionary(Of Integer, Double)
        Me._rearZ = New Dictionary(Of Integer, Double)
        Me._rearRoll = New Dictionary(Of Integer, Double)
        Me._rearPitch = New Dictionary(Of Integer, Double)
        Me._rearYaw = New Dictionary(Of Integer, Double)
    End Sub

    Public Sub DefaultGeometry(ByVal robotModel As String)


        ' he GetGEO message is not directly loaded. To avoid access to empty dictionary, load the default values

        If IsNothing(robotModel) Then
            Console.WriteLine("[SonarSensor]: No RobotModel")
            'Do not load wrong model, because the correct number of front and rear sonars are important
        Else
            If robotModel.ToLower = "nomad" Then

                'The Nomad has a circular shape (values not updated yet)
                'GEO {Type Sonar} {Name Sonar0 Location 0.19,-0,-0.26 Orientation 0,0,-0 Mount HARD}
                '                 {Name Sonar1 Location 0.175537,-0.0727099,-0.26 Orientation 0,0,-0.392699 Mount HARD}
                '                 {Name Sonar2 Location 0.13435,-0.13435,-0.26 Orientation 0,0,-0.785398 Mount HARD} 
                '                 {Name Sonar3 Location 0.0727099,-0.175537,-0.26 Orientation 0,0,-1.1781 Mount HARD}
                '                 {Name Sonar4 Location 1.16338e-17,-0.19,-0.26 Orientation 0,0,-1.5708 Mount HARD}
                '                 {Name Sonar5 Location -0.0727099,-0.175537,-0.26 Orientation 0,0,-1.9635 Mount HARD}
                '                 {Name Sonar6 Location -0.13435,-0.13435,-0.26 Orientation 0,0,-2.35619 Mount HARD}
                '                 {Name Sonar7 Location -0.175537,-0.0727099,-0.26 Orientation 0,0,-2.74889 Mount HARD}
                '                 {Name Sonar8 Location -0.19,-2.32675e-17,-0.26 Orientation 0,0,-3.14159 Mount HARD}
                '                 {Name Sonar9 Location -0.175537,0.0727099,-0.26 Orientation 0,0,2.74889 Mount HARD}
                '                 {Name Sonar10 Location -0.13435,0.13435,-0.26 Orientation 0,0,2.35619 Mount HARD}
                '                 {Name Sonar11 Location -0.0727099,0.175537,-0.26 Orientation 0,0,1.9635 Mount HARD}
                '                 {Name Sonar12 Location 1.16338e-17,0.19,-0.26 Orientation 0,0,1.5708 Mount HARD}
                '                 {Name Sonar13 Location 0.0727099,0.175537,-0.26 Orientation 0,0,1.1781 Mount HARD}
                '                 {Name Sonar14 Location 0.13435,0.13435,-0.26 Orientation 0,0,0.785398 Mount HARD} 
                '                 {Name Sonar15 Location 0.175537,0.0727099,-0.26 Orientation 0,0,0.392699 Mount HARD}

                Me._frontX(1) = 0.0 'Sonar4
                Me._frontX(2) = 0.0727099 'Sonar3
                Me._frontX(3) = 0.13435 'Sonar2
                Me._frontX(4) = 0.175537 'Sonar1

                Me._frontX(5) = 0.19 'Sonar0

                Me._frontX(6) = 0.175537 'Sonar15
                Me._frontX(7) = 0.13435 'Sonar14
                Me._frontX(8) = 0.0727099 'Sonar13
                Me._frontX(9) = 0.0 'Sonar12

                Me._rearX(1) = -0.0727099 'Sonar11
                Me._rearX(2) = -0.13435 'Sonar10
                Me._rearX(3) = -0.175537 'Sonar9

                Me._rearX(4) = -0.19 'Sonar8

                Me._rearX(5) = -0.175537 'Sonar7
                Me._rearX(6) = -0.13435 'Sonar6
                Me._rearX(7) = -0.0727099 'Sonar5

                '----------------------------------------------------------

                Me._frontY(1) = -0.19 'Sonar4
                Me._frontY(2) = -0.175537 'Sonar3
                Me._frontY(3) = -0.13435 'Sonar2
                Me._frontY(4) = -0.0727099 'Sonar1

                Me._frontY(5) = 0.0 'The central one

                Me._frontY(6) = 0.0727099 'Sonar15
                Me._frontY(7) = 0.13435 'Sonar14
                Me._frontY(8) = 0.175537 'Sonar13
                Me._frontY(9) = 0.19 'Sonar12


                Me._rearY(1) = 0.175537 'Sonar11
                Me._rearY(2) = 0.13435 'Sonar10
                Me._rearY(3) = 0.0727099 'Sonar9

                Me._rearY(4) = -0.0 'The central one

                Me._rearY(5) = -0.0727099 'Sonar7
                Me._rearY(6) = -0.13435 'Sonar6
                Me._rearY(7) = -0.175537 'Sonar5

                '----------------------------------------------------------

                Me._frontZ(1) = -0.26
                Me._frontZ(2) = -0.26
                Me._frontZ(3) = -0.26
                Me._frontZ(4) = -0.26

                Me._frontZ(5) = -0.26 'Sonar0

                Me._frontZ(6) = -0.26
                Me._frontZ(7) = -0.26
                Me._frontZ(8) = -0.26
                Me._frontZ(9) = -0.26

                Me._rearZ(1) = -0.26
                Me._rearZ(2) = -0.26
                Me._rearZ(3) = -0.26

                Me._rearZ(4) = -0.26 'Sonar8

                Me._rearZ(5) = -0.26
                Me._rearZ(6) = -0.26
                Me._rearZ(7) = -0.26

                '----------------------------------------------------------

                Me._frontRoll(1) = 0.0 'rotation around X-axis
                Me._frontRoll(2) = 0.0
                Me._frontRoll(3) = 0.0
                Me._frontRoll(4) = 0.0

                Me._frontRoll(5) = 0.0

                Me._frontRoll(6) = 0.0
                Me._frontRoll(7) = 0.0
                Me._frontRoll(8) = 0.0
                Me._frontRoll(9) = 0.0

                Me._rearRoll(1) = 0.0
                Me._rearRoll(2) = 0.0
                Me._rearRoll(3) = 0.0

                Me._rearRoll(4) = 0.0

                Me._rearRoll(5) = 0.0
                Me._rearRoll(6) = 0.0
                Me._rearRoll(7) = 0.0

                '----------------------------------------------------------

                Me._frontPitch(1) = 0.0 'rotation around Y-axis
                Me._frontPitch(2) = 0.0
                Me._frontPitch(3) = 0.0
                Me._frontPitch(4) = 0.0

                Me._frontPitch(5) = 0.0

                Me._frontPitch(6) = 0.0
                Me._frontPitch(7) = 0.0
                Me._frontPitch(8) = 0.0
                Me._frontPitch(9) = 0.0

                Me._rearPitch(1) = 0.0
                Me._rearPitch(2) = 0.0
                Me._rearPitch(3) = 0.0

                Me._rearPitch(4) = 0.0

                Me._rearPitch(5) = 0.0
                Me._rearPitch(6) = 0.0
                Me._rearPitch(7) = 0.0

                '----------------------------------------------------------

                Me._frontYaw(1) = -1.5708 'rotation around Z-axis (pointing downwards)
                Me._frontYaw(2) = -1.1781 'Sonar3
                Me._frontYaw(3) = -0.785398 'Sonar2
                Me._frontYaw(4) = -0.392699 'Sonar1

                Me._frontYaw(5) = 0.0 'Sonar0

                Me._frontYaw(6) = +0.392699 'Sonar15
                Me._frontYaw(7) = +0.785398 'Sonar14
                Me._frontYaw(8) = +1.1781 'Sonar13
                Me._frontYaw(9) = +1.5707964 'Sonar12

                Me._rearYaw(1) = +1.9635 'Sonar11
                Me._rearYaw(2) = +2.35619 'Sonar10
                Me._rearYaw(3) = +2.74889 'Sonar9

                Me._rearYaw(4) = 3.1415927 'Sonar8

                Me._rearYaw(5) = -2.74889 'Sonar7
                Me._rearYaw(6) = -2.35619 'Sonar6
                Me._rearYaw(7) = -1.9635 'Sonar5

            ElseIf robotModel.ToLower.StartsWith("atrvjr") Then

                'The ATVRJr has a rectangular shape

                Me._frontX(1) = 0.07226088 'S14
                Me._frontX(2) = 0.117199875 'S15
                Me._frontX(3) = 0.17248991 'S16
                Me._frontX(4) = 0.23023024 'S17

                Me._frontX(5) = 0.33495012 'S1
                Me._frontX(6) = 0.34040916 'S2
                Me._frontX(7) = 0.3470606 'S3
                Me._frontX(8) = 0.34040916 'S4
                Me._frontX(9) = 0.33495012 'S5

                Me._frontX(10) = 0.23023024 'S6
                Me._frontX(11) = 0.17248991 'S7
                Me._frontX(12) = 0.117199875 'S8
                Me._frontX(13) = 0.07226088 'S9

                Me._rearX(1) = -0.2951654 'S10
                Me._rearX(2) = -0.3470606 'S11
                Me._rearX(3) = -0.3470606 'S12
                Me._rearX(4) = -0.2951654 'S13

                '----------------------------------------------------------

                Me._frontY(1) = -0.1810998 'S14
                Me._frontY(2) = -0.1810998 'S15
                Me._frontY(3) = -0.17859982 'S16
                Me._frontY(4) = -0.17499982 'S17

                Me._frontY(5) = -0.104390375 'S1
                Me._frontY(6) = -0.049910426
                Me._frontY(7) = 0.0 'The central one
                Me._frontY(8) = 0.049910426
                Me._frontY(9) = 0.104390375 'S5

                Me._frontY(10) = 0.17499982 'S6
                Me._frontY(11) = 0.17859982
                Me._frontY(12) = 0.1810998
                Me._frontY(13) = 0.1810998 'S9

                Me._rearY(1) = 0.1810998 'S10
                Me._rearY(2) = 0.15035984
                Me._rearY(3) = -0.15035984
                Me._rearY(4) = -0.1810998 'S12

                '----------------------------------------------------------

                Me._frontZ(1) = -0.0 'S14
                Me._frontZ(2) = -0.0
                Me._frontZ(3) = -0.0
                Me._frontZ(4) = -0.0 'S17

                Me._frontZ(5) = -0.0 'S1
                Me._frontZ(6) = -0.0
                Me._frontZ(7) = -0.0
                Me._frontZ(8) = -0.0
                Me._frontZ(9) = -0.0 'S5

                Me._frontZ(10) = -0.0 'S6
                Me._frontZ(11) = -0.0
                Me._frontZ(12) = -0.0
                Me._frontZ(13) = -0.0 'S9

                Me._rearZ(1) = -0.0 'S10
                Me._rearZ(2) = -0.0
                Me._rearZ(3) = -0.0
                Me._rearZ(4) = -0.0 'S12

                '----------------------------------------------------------

                Me._frontRoll(1) = 0.0 'rotation around X-axis
                Me._frontRoll(2) = 0.0
                Me._frontRoll(3) = 0.0
                Me._frontRoll(4) = 0.0

                Me._frontRoll(5) = 0.0
                Me._frontRoll(6) = 0.0
                Me._frontRoll(7) = 0.0
                Me._frontRoll(8) = 0.0
                Me._frontRoll(9) = 0.0

                Me._frontRoll(10) = 0.0
                Me._frontRoll(11) = 0.0
                Me._frontRoll(12) = 0.0
                Me._frontRoll(13) = 0.0

                Me._rearRoll(1) = 0.0
                Me._rearRoll(2) = 0.0
                Me._rearRoll(3) = 0.0
                Me._rearRoll(4) = 0.0

                '----------------------------------------------------------

                Me._frontPitch(1) = 0.0 'rotation around Y-axis
                Me._frontPitch(2) = 0.0
                Me._frontPitch(3) = 0.0
                Me._frontPitch(4) = 0.0

                Me._frontPitch(5) = 0.0
                Me._frontPitch(6) = 0.0
                Me._frontPitch(7) = 0.0
                Me._frontPitch(8) = 0.0
                Me._frontPitch(9) = 0.0

                Me._frontPitch(10) = 0.0
                Me._frontPitch(11) = 0.0
                Me._frontPitch(12) = 0.0
                Me._frontPitch(13) = 0.0

                Me._rearPitch(1) = 0.0
                Me._rearPitch(2) = 0.0
                Me._rearPitch(3) = 0.0
                Me._rearPitch(4) = 0.0

                '----------------------------------------------------------

                Me._frontYaw(1) = -1.5707964 'rotation around Z-axis (pointing downwards)
                Me._frontYaw(2) = -1.308965 'S15
                Me._frontYaw(3) = -1.0472295
                Me._frontYaw(4) = -0.7853982 'S17

                Me._frontYaw(5) = -0.52356684 'S1
                Me._frontYaw(6) = -0.26183134
                Me._frontYaw(7) = 0.0         'S3
                Me._frontYaw(8) = +0.26183134
                Me._frontYaw(9) = +0.52356684 'S5

                Me._frontYaw(10) = 0.7853982 'S6
                Me._frontYaw(11) = 1.0472295
                Me._frontYaw(12) = 1.308965
                Me._frontYaw(13) = 1.5707964 'S9 and S10 are really looking to the right

                Me._rearYaw(1) = 1.5707964 'S10
                Me._rearYaw(2) = 3.1415927 'S11 and S12 are really looking backwards
                Me._rearYaw(3) = -3.1415927 'S12
                Me._rearYaw(4) = -1.5707964 'S13 and S14 are really looking to the left

                'All values from USARBot.ini Revision 1.97 - Wed Sep 10 13:36:03 2008 UTC ----------------------------

            ElseIf robotModel.ToLower.StartsWith("airrobot") Then

                'Load the default values of the AirRobot (as used by SPQR in semi-finals 2008)
                ' Sensors=(ItemClass=class'USARBot.SonarSensor',ItemName="F1",Position=(X=0.0,Y=0.0,Z=-0.0),Direction=(Y=0.0,Z=-1.5707964,X=0.0)) 
                ' Sensors=(ItemClass=class'USARBot.SonarSensor',ItemName="F2",Position=(X=0.0,Y=0.0,Z=-0.0),Direction=(Y=0.0,Z=-1.0477,X=0.0)) '
                ' Sensors=(ItemClass=class'USARBot.SonarSensor',ItemName="F3",Position=(X=0.0,Y=0.0,Z=-0.0),Direction=(Y=0.0,Z=0.0000,X=0.0)) '
                ' Sensors=(ItemClass=class'USARBot.SonarSensor',ItemName="F4",Position=(X=0.0,Y=0.0,Z=-0.0),Direction=(Y=0.0,Z=1.0477,X=0.0)) 
                ' Sensors=(ItemClass=class'USARBot.SonarSensor',ItemName="F5",Position=(X=0.0,Y=0.0,Z=-0.0),Direction=(Y=0.0,Z=1.5707964,X=0.0)) 

                ' Sensors=(ItemClass=class'USARBot.SonarSensor',ItemName="F6",Position=(X=0.0,Y=0.0,Z=-0.15),Direction=(Y=1.5707964,Z=0.0,X=0.0))

                Me._frontX(1) = -0.1912 'just below rotors
                Me._frontX(2) = +0.1912
                Me._frontX(3) = +0.1912
                Me._frontX(4) = -0.1912

                Me._frontX(5) = 0.0
                Me._frontX(6) = 0.0

                '----------------------------------------------------------

                Me._frontY(1) = -0.1912
                Me._frontY(2) = -0.1912
                Me._frontY(3) = +0.1912
                Me._frontY(4) = +0.1912

                Me._frontY(5) = 0.0
                Me._frontY(6) = 0.0

                '----------------------------------------------------------

                Me._frontZ(1) = -0.0
                Me._frontZ(2) = -0.0
                Me._frontZ(3) = -0.0
                Me._frontZ(4) = -0.0

                Me._frontZ(5) = -0.15
                Me._frontZ(6) = +0.15

                '----------------------------------------------------------

                Me._frontRoll(1) = 0.0 'rotation around X-axis
                Me._frontRoll(2) = 0.0
                Me._frontRoll(3) = 0.0
                Me._frontRoll(4) = 0.0

                Me._frontRoll(5) = 0.0
                Me._frontRoll(6) = 0.0

                '----------------------------------------------------------

                Me._frontPitch(1) = 0.0 'rotation around Y-axis
                Me._frontPitch(2) = 0.0
                Me._frontPitch(3) = 0.0
                Me._frontPitch(4) = 0.0

                Me._frontPitch(5) = 1.5707964 'looking up
                Me._frontPitch(6) = -1.5707964 'looking down

                '----------------------------------------------------------

                Me._frontYaw(1) = -1.4 'rotation around Z-axis (pointing downwards)
                Me._frontYaw(2) = -0.4
                Me._frontYaw(3) = +0.4
                Me._frontYaw(4) = +1.4

                Me._frontYaw(5) = 0.0
                Me._frontYaw(6) = 0.0

            ElseIf robotModel.ToLower.StartsWith("p2at") Then

                'Load the default values of the P2AT (nearly round sonar ring)

                Me._frontX(1) = 0.14499985
                Me._frontX(2) = 0.1849998
                Me._frontX(3) = 0.21999978
                Me._frontX(4) = 0.23999976
                Me._frontX(5) = 0.23999976
                Me._frontX(6) = 0.21999978
                Me._frontX(7) = 0.1849998
                Me._frontX(8) = 0.14499985

                '----------------------------------------------------------

                Me._frontY(1) = -0.12999986
                Me._frontY(2) = -0.114999875
                Me._frontY(3) = -0.079999916
                Me._frontY(4) = -0.024999974
                Me._frontY(5) = 0.024999974
                Me._frontY(6) = 0.079999916
                Me._frontY(7) = 0.114999875
                Me._frontY(8) = 0.12999986

                '----------------------------------------------------------

                Me._frontZ(1) = -0.0
                Me._frontZ(2) = -0.0
                Me._frontZ(3) = -0.0
                Me._frontZ(4) = -0.0
                Me._frontZ(5) = -0.0
                Me._frontZ(6) = -0.0
                Me._frontZ(7) = -0.0
                Me._frontZ(8) = -0.0

                '----------------------------------------------------------

                Me._frontRoll(1) = 0.0 'rotation around X-axis
                Me._frontRoll(2) = 0.0
                Me._frontRoll(3) = 0.0
                Me._frontRoll(4) = 0.0
                Me._frontRoll(5) = 0.0
                Me._frontRoll(6) = 0.0
                Me._frontRoll(7) = 0.0
                Me._frontRoll(8) = 0.0

                '----------------------------------------------------------

                Me._frontPitch(1) = 0.0 'rotation around Y-axis
                Me._frontPitch(2) = 0.0
                Me._frontPitch(3) = 0.0
                Me._frontPitch(4) = 0.0
                Me._frontPitch(5) = 0.0
                Me._frontPitch(6) = 0.0
                Me._frontPitch(7) = 0.0
                Me._frontPitch(8) = 0.0

                '----------------------------------------------------------

                Me._frontYaw(1) = -1.5707964 'rotation around Z-axis (pointing downwards)
                Me._frontYaw(2) = -0.87264335
                Me._frontYaw(3) = -0.52356684
                Me._frontYaw(4) = -0.17449032
                Me._frontYaw(5) = +0.17449032
                Me._frontYaw(6) = +0.52356684
                Me._frontYaw(7) = +0.8743691 '=/ -frontYaw(2)
                Me._frontYaw(8) = +1.5707964

                'All values from USARBot.ini Revision 1.97 - Wed Sep 10 13:36:03 2008 UTC ----------------------------

                Me._rearX(1) = -0.14499985
                Me._rearX(2) = -0.1849998
                Me._rearX(3) = -0.21999978
                Me._rearX(4) = -0.23999976
                Me._rearX(5) = -0.23999976
                Me._rearX(6) = -0.21999978
                Me._rearX(7) = -0.1849998
                Me._rearX(8) = -0.14499985

                '----------------------------------------------------------

                Me._rearY(1) = 0.12999986
                Me._rearY(2) = 0.114999875
                Me._rearY(3) = 0.079999916
                Me._rearY(4) = 0.024999974
                Me._rearY(5) = -0.024999974
                Me._rearY(6) = -0.079999916
                Me._rearY(7) = -0.114999875
                Me._rearY(8) = -0.12999986

                '----------------------------------------------------------

                Me._rearZ(1) = -0.0
                Me._rearZ(2) = -0.0
                Me._rearZ(3) = -0.0
                Me._rearZ(4) = -0.0
                Me._rearZ(5) = -0.0
                Me._rearZ(6) = -0.0
                Me._rearZ(7) = -0.0
                Me._rearZ(8) = -0.0

                '----------------------------------------------------------

                Me._rearRoll(1) = 0.0 'rotation around X-axis
                Me._rearRoll(2) = 0.0
                Me._rearRoll(3) = 0.0
                Me._rearRoll(4) = 0.0
                Me._rearRoll(5) = 0.0
                Me._rearRoll(6) = 0.0
                Me._rearRoll(7) = 0.0
                Me._rearRoll(8) = 0.0

                '----------------------------------------------------------

                Me._rearPitch(1) = 0.0 'rotation around Y-axis
                Me._rearPitch(2) = 0.0
                Me._rearPitch(3) = 0.0
                Me._rearPitch(4) = 0.0
                Me._rearPitch(5) = 0.0
                Me._rearPitch(6) = 0.0
                Me._rearPitch(7) = 0.0
                Me._rearPitch(8) = 0.0

                '----------------------------------------------------------

                Me._rearYaw(1) = +1.5707964 'rotation around Z-axis (pointing downwards)
                Me._rearYaw(2) = +2.2689495
                Me._rearYaw(3) = +2.618026
                Me._rearYaw(4) = +2.9671025
                Me._rearYaw(5) = -2.9671025
                Me._rearYaw(6) = -2.618026
                Me._rearYaw(7) = -2.2689495
                Me._rearYaw(8) = -1.5707964
            ElseIf robotModel.ToLower = "p2dx" OrElse robotModel.ToLower = "omnip2dx" Then

                'Load the default values of the P2DX(only sonars at the front)

                Me._frontX(1) = 0.114999875 'different x-positions as P2AT
                Me._frontX(2) = 0.15499984
                Me._frontX(3) = 0.1899998
                Me._frontX(4) = 0.20999977
                Me._frontX(5) = 0.20999977
                Me._frontX(6) = 0.1899998
                Me._frontX(7) = 0.15499984
                Me._frontX(8) = 0.114999875

                '----------------------------------------------------------

                Me._frontY(1) = -0.12999986 'same y-positions as P2AT
                Me._frontY(2) = -0.114999875
                Me._frontY(3) = -0.079999916
                Me._frontY(4) = -0.024999974
                Me._frontY(5) = 0.024999974
                Me._frontY(6) = 0.079999916
                Me._frontY(7) = 0.114999875
                Me._frontY(8) = 0.12999986

                '----------------------------------------------------------

                Me._frontZ(1) = -0.0
                Me._frontZ(2) = -0.0
                Me._frontZ(3) = -0.0
                Me._frontZ(4) = -0.0
                Me._frontZ(5) = -0.0
                Me._frontZ(6) = -0.0
                Me._frontZ(7) = -0.0
                Me._frontZ(8) = -0.0

                '----------------------------------------------------------

                Me._frontRoll(1) = 0.0 'rotation around X-axis
                Me._frontRoll(2) = 0.0
                Me._frontRoll(3) = 0.0
                Me._frontRoll(4) = 0.0
                Me._frontRoll(5) = 0.0
                Me._frontRoll(6) = 0.0
                Me._frontRoll(7) = 0.0
                Me._frontRoll(8) = 0.0

                '----------------------------------------------------------

                Me._frontPitch(1) = 0.0 'rotation around Y-axis
                Me._frontPitch(2) = 0.0
                Me._frontPitch(3) = 0.0
                Me._frontPitch(4) = 0.0
                Me._frontPitch(5) = 0.0
                Me._frontPitch(6) = 0.0
                Me._frontPitch(7) = 0.0
                Me._frontPitch(8) = 0.0

                '----------------------------------------------------------

                Me._frontYaw(1) = -1.5707964 'rotation around Z-axis (pointing downwards)
                Me._frontYaw(2) = -0.87264335
                Me._frontYaw(3) = -0.52356684
                Me._frontYaw(4) = -0.17449032
                Me._frontYaw(5) = +0.17449032
                Me._frontYaw(6) = +0.52356684
                Me._frontYaw(7) = +0.87264335 '== -frontYaw(2)
                Me._frontYaw(8) = +1.5707964

                'All values from USARBot.ini Suzhou 2008 RoboCup competition default version: Tuesday July 15, 2008; Arnoud Visser (Version 0.3) 22:50
 
            Else

                Console.WriteLine("[SonarSensor]: Unknown RobotModel")

                'Do not load wrong model, because the correct number of front and rear sonars are important
            End If
        End If

        'Values can be later overwritten, when a GetGEO message arrives

    End Sub
    ' Typical reading P2AT robot:
    'SEN {Time 4015.8388} {Type Sonar} {Name F1 Range 1.9991} {Name F2 Range 5.0000} {Name F3 Range 4.8843} {Name F4 Range 0.9377} {Name F5 Range 2.3472} {Name F6 Range 2.1501} {Name F7 Range 0.7115} {Name F8 Range 0.6085} {Name R1 Range 0.4477} {Name R2 Range 0.9995} {Name R3 Range 0.9862} {Name R4 Range 2.5638} {Name R5 Range 2.0131} {Name R6 Range 1.8465} {Name R7 Range 1.9408} {Name R8 Range 3.4782}

    Protected Overrides Function ToKeyValuePair(ByVal part As String) As System.Collections.Generic.KeyValuePair(Of String, String)
        Dim parts() As String = Strings.Split(part, " ")
        If parts.Length >= 3 Then
            If parts(0).ToUpper = "NAME" Then
                Return New KeyValuePair(Of String, String)(parts(1), parts(3).Trim)
            End If
        End If

        Return MyBase.ToKeyValuePair(part)

    End Function

    Protected Overrides Sub ProcessMessage(ByVal msgtype As MessageType, ByVal msg As System.Collections.Specialized.StringDictionary)
        MyBase.ProcessMessage(msgtype, msg)

        If msgtype = MessageType.Sensor Then
            Me.CurrentData.Load(msg)
            Me.Agent.NotifySensorUpdate(Me)
        End If

    End Sub

    Public Overrides Sub ProcessGeoMessage(ByVal msgtype As MessageType, ByVal msg As System.Collections.Specialized.StringDictionary)
        MyBase.ProcessGeoMessage(msgtype, msg)
        'GEO {Type string} {Name string Location x,y,z Orientation x,y,z Mount string} {Name string Location x,y,z Orientation x,y,z Mount string} 

        If Not IsNothing(Me.Agent) Then
            'This statement is not always reached, because Me.Agent is not assigned yet
            Console.WriteLine(String.Format("SonarSensor: geometry values for {0} updated with GETGEO message", Me.Agent.RobotModel))
        Else
            Console.WriteLine("SonarSensor: geometry values updated with GETGEO message")
        End If

        Dim parts() As String

        If msgtype = MessageType.Geometry Then

            'For sensors with multiple instances ANYNAME is used. The names and GEOs of the all instances are returned. Handle the parts

            'Console.WriteLine(String.Format("{0} received GeometryMessage for  sensor with name {1}", Me.SensorName, msg("Name")))

            'msg("Name") = "F1 Location 0.1450,-0.1300,0.0000 Orientation 0.0002,0.0002,-1.5707 Mount HARD"


            For i As Integer = 1 To 8

                Dim instance_name As String = FRONTKEY
                instance_name &= i.ToString

                Dim key As String = "Name "
                key &= instance_name

                parts = Strings.Split(msg(key), " ")
                If parts.Length = 6 Then
                    Dim locations() As String = Strings.Split(parts(1), ",")
                    Dim orientations() As String = Strings.Split(parts(3), ",")
                    'To be done,  the Mount (If not HARD, the pose of the mount should be queried.

                    Me._frontX(CInt(instance_name.Substring(FRONTKEY.Length))) = Double.Parse(locations(0))
                    Me._frontY(CInt(instance_name.Substring(FRONTKEY.Length))) = Double.Parse(locations(1))
                    Me._frontZ(CInt(instance_name.Substring(FRONTKEY.Length))) = Double.Parse(locations(2))

                    Me._frontRoll(CInt(instance_name.Substring(FRONTKEY.Length))) = Double.Parse(orientations(0))
                    Me._frontPitch(CInt(instance_name.Substring(FRONTKEY.Length))) = Double.Parse(orientations(1))
                    Me._frontYaw(CInt(instance_name.Substring(FRONTKEY.Length))) = Double.Parse(orientations(2))
                End If


                instance_name = REARKEY
                instance_name &= i.ToString

                key = "Name "
                key &= instance_name

                parts = Strings.Split(msg(key), " ")
                If parts.Length = 6 Then
                    Dim locations() As String = Strings.Split(parts(1), ",")
                    Dim orientations() As String = Strings.Split(parts(3), ",")
                    'To be done,  the Mount (If not HARD, the pose of the mount should be queried.

                    Me._rearX(CInt(instance_name.Substring(REARKEY.Length))) = Double.Parse(locations(0))
                    Me._rearY(CInt(instance_name.Substring(REARKEY.Length))) = Double.Parse(locations(1))
                    Me._rearZ(CInt(instance_name.Substring(REARKEY.Length))) = Double.Parse(locations(2))

                    Me._rearRoll(CInt(instance_name.Substring(REARKEY.Length))) = Double.Parse(orientations(0))
                    Me._rearPitch(CInt(instance_name.Substring(REARKEY.Length))) = Double.Parse(orientations(1))
                    Me._rearYaw(CInt(instance_name.Substring(REARKEY.Length))) = Double.Parse(orientations(2))
                End If


            Next

            If Me.Agent.RobotModel.ToLower = "nomad" Then

                For i As Integer = 0 To 15

                    Dim instance_name As String = SONARKEY
                    instance_name &= i.ToString

                    Dim key As String = "Name "
                    key &= instance_name

                    parts = Strings.Split(msg(key), " ")
                    If parts.Length = 6 Then
                        Dim locations() As String = Strings.Split(parts(1), ",")
                        Dim orientations() As String = Strings.Split(parts(3), ",")

                        If i < 5 Then
                            'Sonar0 = F5 .. Sonar4 = F1
                            Me._frontX(5 - i) = Double.Parse(locations(0))
                            Me._frontY(5 - i) = Double.Parse(locations(1))
                            Me._frontZ(5 - i) = Double.Parse(locations(2))

                            Me._frontRoll(5 - i) = Double.Parse(orientations(0))
                            Me._frontPitch(5 - i) = Double.Parse(orientations(1))
                            Me._frontYaw(5 - i) = Double.Parse(orientations(2))
                        ElseIf i > 11 Then
                            'Sonar12 = F9 .. Sonar15 = F6
                            Me._frontX(21 - i) = Double.Parse(locations(0))
                            Me._frontY(21 - i) = Double.Parse(locations(1))
                            Me._frontZ(21 - i) = Double.Parse(locations(2))

                            Me._frontRoll(21 - i) = Double.Parse(orientations(0))
                            Me._frontPitch(21 - i) = Double.Parse(orientations(1))
                            Me._frontYaw(21 - i) = Double.Parse(orientations(2))
                        Else
                            'Sonar11 = R1 .. Sonar5 = R7
                            Me._rearX(12 - i) = Double.Parse(locations(0))
                            Me._rearY(12 - i) = Double.Parse(locations(1))
                            Me._rearZ(12 - i) = Double.Parse(locations(2))

                            Me._rearRoll(12 - i) = Double.Parse(orientations(0))
                            Me._rearPitch(12 - i) = Double.Parse(orientations(1))
                            Me._rearYaw(12 - i) = Double.Parse(orientations(2))

                        End If
                    End If

                Next
            ElseIf Me.Agent.RobotModel.ToLower.StartsWith("atrvjr") Then
                For i As Integer = 1 To 17

                    Dim instance_name As String = SKEY
                    instance_name &= i.ToString

                    Dim key As String = "Name "
                    key &= instance_name

                    parts = Strings.Split(msg(key), " ")
                    If parts.Length = 6 Then
                        Dim locations() As String = Strings.Split(parts(1), ",")
                        Dim orientations() As String = Strings.Split(parts(3), ",")
                        'To be done,  the Mount (If not HARD, the pose of the mount should be queried.

                        If i < 10 Then
                            'S1 = F5 .. S9 = F13
                            Me._frontX(i + 4) = Double.Parse(locations(0))
                            Me._frontY(i + 4) = Double.Parse(locations(1))
                            Me._frontZ(i + 4) = Double.Parse(locations(2))

                            Me._frontRoll(i + 4) = Double.Parse(orientations(0))
                            Me._frontPitch(i + 4) = Double.Parse(orientations(1))
                            Me._frontYaw(i + 4) = Double.Parse(orientations(2))
                        ElseIf i > 13 Then
                            'S14 = F1 .. S17 = F4
                            Me._frontX(i - 13) = Double.Parse(locations(0))
                            Me._frontY(i - 13) = Double.Parse(locations(1))
                            Me._frontZ(i - 13) = Double.Parse(locations(2))

                            Me._frontRoll(i - 13) = Double.Parse(orientations(0))
                            Me._frontPitch(i - 13) = Double.Parse(orientations(1))
                            Me._frontYaw(i - 13) = Double.Parse(orientations(2))
                        Else
                            'S10 = R1 .. S13 = R4
                            Me._rearX(i - 9) = Double.Parse(locations(0))
                            Me._rearY(i - 9) = Double.Parse(locations(1))
                            Me._rearZ(i - 9) = Double.Parse(locations(2))

                            Me._rearRoll(i - 9) = Double.Parse(orientations(0))
                            Me._rearPitch(i - 9) = Double.Parse(orientations(1))
                            Me._rearYaw(i - 9) = Double.Parse(orientations(2))

                        End If
                    End If
                Next
            End If
        End If
    End Sub

    Public Overrides Sub ProcessConfMessage(ByVal msgtype As MessageType, ByVal msg As System.Collections.Specialized.StringDictionary)
        MyBase.ProcessConfMessage(msgtype, msg)
        'CONF {Type Sonar}  {Name Sonar0} {MaxRange 10.668} {MinRange 0.1524} {BeamAngle 0.349066}
        '                   {Name Sonar1} {MaxRange 10.668} {MinRange 0.1524} {BeamAngle 0.349066}

        For i As Integer = 1 To Me._frontX.Count

            Dim instance_name As String = FRONTKEY
            instance_name &= i.ToString

            Dim key As String = "Name "

            'If msg("Name") = instance_name Then 'msg contains only 5 keys (Type, Name, MaxRange, MinRange, BeamAngle). Rest of line is ignored
        If msg.ContainsKey("BeamAngle") Then
                Me._frontBeamAngle(CInt(instance_name.Substring(FRONTKEY.Length))) = Double.Parse(msg("BeamAngle"))
        End If

        If msg.ContainsKey("MinRange") Then
                If msg.ContainsKey("MinRange") Then
                    Me._frontMinRange(CInt(instance_name.Substring(FRONTKEY.Length))) = Double.Parse(msg("MinRange"))
        End If
            End If

        If msg.ContainsKey("MaxRange") Then
                If msg.ContainsKey("MaxRange") Then
                    Me._frontMaxRange(CInt(instance_name.Substring(FRONTKEY.Length))) = Double.Parse(msg("MaxRange"))
        End If
            End If
            'End If

        Next

        
    End Sub


    Private _frontX As Dictionary(Of Integer, Double)
    Public ReadOnly Property FrontX(ByVal index As Integer) As Double
        Get
            If index > Me._frontX.Count OrElse index < 0 Then
                Console.WriteLine(String.Format("[SonarSensor]: WARNING 'X of Front({0}) of this RobotModel not known (yet)'", index))
                Return 0.0123
            Else
                Return Me._frontX(index)
            End If
        End Get
        'This protection is needed, because Sensor can be constructed before the Agent
    End Property

    Private _frontY As Dictionary(Of Integer, Double)
    Public ReadOnly Property FrontY(ByVal index As Integer) As Double
        Get
            If index > Me._frontY.Count OrElse index < 0 Then
                Console.WriteLine(String.Format("[SonarSensor]: WARNING 'Y of Front({0}) of this RobotModel not known (yet)'", index))
                Return 0.0123
            Else
                Return Me._frontY(index)
            End If
        End Get
    End Property
    Private _frontZ As Dictionary(Of Integer, Double)
    Public ReadOnly Property FrontZ(ByVal index As Integer) As Double
        Get
            If index > Me._frontZ.Count OrElse index < 0 Then
                Console.WriteLine(String.Format("[SonarSensor]: WARNING 'Z of Front({0}) of this RobotModel not known (yet)'", index))
                Return 0.0123
            Else
                Return (Me._frontZ(index))
            End If
        End Get
    End Property

    Private _frontRoll As Dictionary(Of Integer, Double)
    Public ReadOnly Property FrontRoll(ByVal index As Integer) As Double
        Get
            If index > Me._frontRoll.Count OrElse index < 0 Then
                Console.WriteLine(String.Format("[SonarSensor]: WARNING 'Roll of Front({0}) of this RobotModel not known (yet)'", index))
                Return 2 * PI
            Else
                Return Me._frontRoll(index)
            End If

        End Get
    End Property

    Private _frontPitch As Dictionary(Of Integer, Double)
    Public ReadOnly Property FrontPitch(ByVal index As Integer) As Double
        Get
            If index > Me._frontPitch.Count OrElse index < 0 Then
                Console.WriteLine(String.Format("[SonarSensor]: WARNING 'Pitch of Front({0}) of this RobotModel not known (yet)'", index))
                Return 2 * PI
            Else
                Return Me._frontPitch(index)
            End If
        End Get
    End Property

    Private _frontYaw As Dictionary(Of Integer, Double)
    Public ReadOnly Property FrontYaw(ByVal index As Integer) As Double
        Get
            'If this crashes, the specific names of the Sonar of that RobotModel should be included in the Constructor and ProcessGeo chain
            If index > Me._frontYaw.Count OrElse index < 0 Then
                Console.WriteLine(String.Format("[SonarSensor]: WARNING 'Yaw of Front({0}) of this RobotModel not known (yet)'", index))
                Return 2 * PI
            Else
                Return Me._frontYaw(index)
            End If
            'This protection is needed, because Sensor can be constructed before the Agent
        End Get
    End Property

    Private _frontBeamAngle As Dictionary(Of Integer, Double)
    Public ReadOnly Property FrontBeamAngle(ByVal index As Integer) As Double
        Get
            'If this crashes, the specific names of the Sonar of that RobotModel should be included in the Constructor and ProcessGeo chain
            If index > Me._frontBeamAngle.Count OrElse index < 0 Then
                Console.WriteLine(String.Format("[SonarSensor]: WARNING 'BeamAngle of Front({0}) of this RobotModel not known (yet)'", index))
                Return PI / 4
            Else
                Return Me._frontBeamAngle(index)
            End If
            'This protection is needed, because Sensor can be constructed before the Agent
        End Get
    End Property

    Private _frontMinRange As Dictionary(Of Integer, Double)
    Public ReadOnly Property FrontMinRange(ByVal index As Integer) As Double
        Get
            'If this crashes, the specific names of the Sonar of that RobotModel should be included in the Constructor and ProcessGeo chain
            If index > Me._frontMinRange.Count OrElse index < 0 Then
                Console.WriteLine(String.Format("[SonarSensor]: WARNING 'MinRange of Front({0}) of this RobotModel not known (yet)'", index))
                Return 0.1
            Else
                Return Me._frontMinRange(index)
            End If
            'This protection is needed, because Sensor can be constructed before the Agent
        End Get
    End Property

    Private _frontMaxRange As Dictionary(Of Integer, Double)
    Public ReadOnly Property FrontMaxRange(ByVal index As Integer) As Double
        Get
            'If this crashes, the specific names of the Sonar of that RobotModel should be included in the Constructor and ProcessGeo chain
            If index > Me._frontMaxRange.Count OrElse index < 0 Then
                Console.WriteLine(String.Format("[SonarSensor]: WARNING 'MaxRange of Front({0}) of this RobotModel not known (yet)'", index))
                Return 10.0
            Else
                Return Me._frontMaxRange(index)
            End If
            'This protection is needed, because Sensor can be constructed before the Agent
        End Get
    End Property

    Private _rearX As Dictionary(Of Integer, Double)
    Public ReadOnly Property RearX(ByVal index As Integer) As Double
        Get
            If index > Me._rearX.Count OrElse index < 0 Then
                Console.WriteLine(String.Format("[SonarSensor]: WARNING 'X of Rear({0}) of this RobotModel not known (yet)'", index))
                Return 0.0123
            Else
                Return Me._rearX(index)
            End If
        End Get
    End Property

    Private _rearY As Dictionary(Of Integer, Double)
    Public ReadOnly Property RearY(ByVal index As Integer) As Double
        Get
            If index > Me._rearY.Count OrElse index < 0 Then
                Console.WriteLine(String.Format("[SonarSensor]: WARNING 'Y of Rear({0}) of this RobotModel not known (yet)'", index))
                Return 0.0123
            Else
                Return Me._rearY(index)
            End If
        End Get
    End Property
    Private _rearZ As Dictionary(Of Integer, Double)
    Public ReadOnly Property RearZ(ByVal index As Integer) As Double
        Get
            If index > Me._rearZ.Count OrElse index < 0 Then
                Console.WriteLine(String.Format("[SonarSensor]: WARNING 'Z of Rear({0}) of this RobotModel not known (yet)'", index))
                Return 0.0123
            Else
                Return (Me._rearZ(index))
            End If
        End Get
    End Property

    Private _rearRoll As Dictionary(Of Integer, Double)
    Public ReadOnly Property RearRoll(ByVal index As Integer) As Double
        Get
            If index > Me._rearRoll.Count OrElse index < 0 Then
                Console.WriteLine(String.Format("[SonarSensor]: WARNING 'Roll of Rear({0}) of this RobotModel not known (yet)'", index))
                Return 2 * PI
            Else
                Return (Me._rearRoll(index))
            End If
        End Get
    End Property

    Private _rearPitch As Dictionary(Of Integer, Double)
    Public ReadOnly Property RearPitch(ByVal index As Integer) As Double
        Get
            If index > Me._rearPitch.Count OrElse index < 0 Then
                Console.WriteLine(String.Format("[SonarSensor]: WARNING 'Pitch of Rear({0}) of this RobotModel not known (yet)'", index))
                Return 2 * PI
            Else
                Return (Me._rearPitch(index))
            End If

        End Get
    End Property

    Private _rearYaw As Dictionary(Of Integer, Double)
    Public ReadOnly Property RearYaw(ByVal index As Integer) As Double
        Get
            If index > Me._rearYaw.Count OrElse index < 0 Then
                Console.WriteLine(String.Format("[SonarSensor]: WARNING 'Yaw of Rear({0}) of this RobotModel not known (yet)'", index))
                Return 2 * PI
            Else
                Return Me._rearYaw(index)
            End If
        End Get
    End Property
End Class
