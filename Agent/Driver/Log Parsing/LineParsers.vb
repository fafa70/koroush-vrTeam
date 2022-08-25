Imports System.Math
Imports System.IO
Imports System.Threading
Imports System.Globalization

''' <summary>
''' Utility class shich can parse several standard logfile formats.
''' </summary>
''' <remarks></remarks>
Public Class LineParsers

    Private Shared Function GetMessageType(ByVal key As String) As MessageType
        Dim msgtype As MessageType

        Select Case key
            Case "SEN"
                msgtype = MessageType.Sensor
            Case "NFO"
                msgtype = MessageType.Info
            Case "STA"
                msgtype = MessageType.Status
            Case "MISSTA"
                msgtype = MessageType.MissionPackageStatus
            Case "GEO"
                msgtype = MessageType.Geometry
            Case "CONF"
                msgtype = MessageType.Configuration
            Case "RES"
                msgtype = MessageType.Response
            Case Else
                Return MessageType.Unknown
        End Select

        Return msgtype
    End Function

    Public Shared Function ParseUsarSimLine(ByVal receiver As Agent, ByVal line As String) As Boolean

        Dim msgtype As MessageType
        Dim type As String = String.Empty
        Dim name As String = String.Empty
        Dim parts As New Specialized.StringCollection

        Dim success As Boolean = False
        If Not String.IsNullOrEmpty(line) Then

            'extract msgtype first
            Dim i As Integer = 0
            Dim j As Integer = line.IndexOf(" ")
            Dim part As String = Nothing
            If j > 0 Then
                part = line.Substring(i, j - i)
            End If
            msgtype = GetMessageType(part)

            If msgtype = MessageType.Unknown Then
                Console.WriteLine("Received message of unknown type: " & line)
            End If

            'then extract all curly-braced parts
            i = line.IndexOf("{", j)
            While i >= 0
                j = line.IndexOf("}", i)
                If j >= 0 Then
                    part = line.Substring(i + 1, j - i - 1)
                    parts.Add(part)

                    'we need type and possibly also the name later on in order to know 
                    'which sensor to forward the message to.
                    If String.IsNullOrEmpty(type) AndAlso part.ToUpper.StartsWith("TYPE ") Then
                        type = part.Substring(5).Trim
                    ElseIf String.IsNullOrEmpty(name) AndAlso part.ToUpper.StartsWith("NAME ") Then
                        name = part.Substring(5).Trim

                        'some messages have a different format, then the name is embedded within a part:
                        'e.g.: GEO {Type RangeScanner} {Name Scanner1 Location 0.1440,0.0000,-0.0918 Orientation 0.0000,-0.0000,0.0000 Mount HARD}
                        If name.Contains(" ") Then
                            name = name.Substring(0, name.IndexOf(" "))
                        End If
                    End If

                    i = line.IndexOf("{", j)

                Else
                    'done
                    Exit While

                End If
            End While

            receiver.ReceiveMessage(msgtype, type, name, parts)
            parts = Nothing

            success = True

        End If

        Return success

    End Function

    Public Shared Function ParseMatlabDataLine(ByVal receiver As Agent, ByVal line As String) As Boolean

        'a single line hold a pose estimate and a range scan
        '- the first 3 doubles are the ground-truth pose estimate
        '- the remaining double are the scan which is assumed to be collected over a 180 degree field-of-view



        Dim success As Boolean = False
        If Not String.IsNullOrEmpty(line) Then

            line = line.Replace(vbTab, " ")

            Dim nums() As String = Strings.Split(line, " ")
            Dim pose(3 - 1) As String
            Dim scan(nums.Length - pose.Length - 1) As String

            Array.ConstrainedCopy(nums, 0, pose, 0, 3)
            Array.ConstrainedCopy(nums, pose.Length, scan, 0, scan.Length)
            Array.Reverse(scan)


            'first construct INS message
            'SEN {Type INS} {Name INS} {Location 0.80,-7.28,-0.45} {Orientation 0.00,6.28,0.00}
            Dim parts As New Specialized.StringCollection
            parts.Add("Type " & InsSensor.SENSORTYPE_INS)
            parts.Add("Name INS")
            parts.Add(String.Format("Location {0},{1},0.00", pose(0), pose(1)))
            parts.Add(String.Format("Orientation 0.00,0.00,{0}", pose(2)))
            receiver.ReceiveMessage(MessageType.Sensor, InsSensor.SENSORTYPE_INS, "INS", parts)


            'construct similar GroundTruth message
            'SEN {Type GroundTruth} {Name GroundTruth} {Location 0.80,-7.28,-0.45} {Orientation 0.00,6.28,0.00}
            parts.Clear()
            parts.Add("Type " & GroundTruthSensor.SENSORTYPE_GROUNDTRUTH)
            parts.Add("Name GroundTruth")
            parts.Add(String.Format("Location {0},{1},0.00", pose(0), pose(1)))
            parts.Add(String.Format("Orientation 0.00,0.00,{0}", pose(2)))
            receiver.ReceiveMessage(MessageType.Sensor, GroundTruthSensor.SENSORTYPE_GROUNDTRUTH, "GroundTruth", parts)


            'then construct range scan message
            'SEN {Type RangeScanner} {Name Scanner1} {Resolution 0.0174} {FOV 3.1415} {Range <doubles>}
            parts.Clear()
            parts.Add("Type " & LaserRangeSensor.SENSORTYPE_RANGESCANNER)
            parts.Add("Name Scanner1")
            parts.Add("Resolution " + CStr(PI / scan.Length))
            parts.Add("FOV " + CStr(PI))
            parts.Add("Range " + Strings.Join(scan, ","))
            receiver.ReceiveMessage(MessageType.Sensor, LaserRangeSensor.SENSORTYPE_RANGESCANNER, "Scanner1", parts)

            success = True

        End If

        Return success

    End Function

    Public Shared Function ParsePlayerLine(ByVal receiver As Agent, ByVal line As String) As Boolean


        Dim success As Boolean = False
        If Not String.IsNullOrEmpty(line) Then

            line = line.Replace(vbTab, " ").Trim

            Dim nums() As String = Strings.Split(line, " ")

            If nums.Length > 1 Then

                Dim parts As New Specialized.StringCollection

                Dim type As String = nums(3).ToLower
                Select Case type
                    Case "power" 'ignore, could be translated to STA message

                    Case "fiducial" 'ignore, could be translated to SONAR message

                    Case "lmap" 'ignore, could be translated to GROUNDTRUTH message

                    Case "sync" 'ignore, just time

                    Case "position"
                        '#  Position: pose_x pose_y pose_theta vel_x vel_y vel_theta
                        'format 1075329039.144 intel0 7000 position 01 1075329039.034 +00.000 +00.000 -0.000 +0.000 +0.000 +0.000

                        Dim pose(3 - 1) As String
                        Array.ConstrainedCopy(nums, 6, pose, 0, pose.Length)
                        'pose(1) = CStr(-Double.Parse(pose(1)))
                        'pose(2) = CStr(-Double.Parse(pose(2)))

                        'construct INS message
                        'SEN {Type INS} {Name INS} {Location 0.80,-7.28,-0.45} {Orientation 0.00,6.28,0.00}
                        parts.Add("Type " & InsSensor.SENSORTYPE_INS)
                        parts.Add("Name INS")
                        parts.Add(String.Format("Location {0},{1},0.00", pose(0), pose(1)))
                        parts.Add(String.Format("Orientation 0.00,0.00,{0}", pose(2)))
                        receiver.ReceiveMessage(MessageType.Sensor, InsSensor.SENSORTYPE_INS, "INS", parts)

                        'construct similar GroundTruth message
                        'SEN {Type GroundTruth} {Name GroundTruth} {Location 0.80,-7.28,-0.45} {Orientation 0.00,6.28,0.00}
                        parts.Clear()
                        parts.Add("Type " & GroundTruthSensor.SENSORTYPE_GROUNDTRUTH)
                        parts.Add("Name GroundTruth")
                        parts.Add(String.Format("Location {0},{1},0.00", pose(0), pose(1)))
                        parts.Add(String.Format("Orientation 0.00,0.00,{0}", pose(2)))
                        receiver.ReceiveMessage(MessageType.Sensor, GroundTruthSensor.SENSORTYPE_GROUNDTRUTH, "GroundTruth", parts)


                    Case "laser" 'front laser range scan
                        '#  Laser: range bearing intensity range bearing intensity...[repeats] 
                        'format 1075329039.148 intel0 7000 laser 00 1075329038.984 0.620 -1.571 0 0.630 -1.553 0 ...

                        Dim length As Integer = nums.Length() - 7
                        Dim ranges As New List(Of String)
                        For element As Integer = 3 To length - 1 Step 3
                            ranges.Add(nums(7 + element))
                        Next

                        length = ranges.Count
                        Dim scan(length - 1) As String

                        Array.ConstrainedCopy(ranges.ToArray, 0, scan, 0, scan.Length) 'skip 2 elements
                        Array.Reverse(scan)

                        'then construct range scan message
                        'SEN {Type RangeScanner} {Name Scanner1} {Resolution 0.0174} {FOV 3.1415} {Range <doubles>}
                        parts.Add("Type " & LaserRangeSensor.SENSORTYPE_RANGESCANNER)
                        parts.Add("Name Scanner1")
                        parts.Add("Range " + Strings.Join(scan, ","))
                        parts.Add("Resolution " + CStr(PI / scan.Length))
                        parts.Add("FOV " + CStr(PI))
                        receiver.ReceiveMessage(MessageType.Sensor, LaserRangeSensor.SENSORTYPE_RANGESCANNER, "Scanner1", parts)

                End Select


                success = True
            End If

        End If

        Return success
    End Function

    Public Shared Function ParseCarmenLine(ByVal receiver As Agent, ByVal line As String) As Boolean

        Dim success As Boolean = False
        If Not String.IsNullOrEmpty(line) Then

            line = line.Replace(vbTab, " ").Trim

            Dim nums() As String = Strings.Split(line, " ")

            If nums.Length > 1 Then

                Dim parts As New Specialized.StringCollection

                Dim type As String = nums(0).ToLower
                Select Case type
                    Case "param" 'parameter settings, ignore
                        'PARAM param_name param_value

                    Case "odom" 'odometry message
                        'ODOM x y theta tv rv accel
                        Dim pose(3 - 1) As String
                        Array.ConstrainedCopy(nums, 1, pose, 0, pose.Length)
                        pose(1) = CStr(-Double.Parse(pose(1)))
                        pose(2) = CStr(-Double.Parse(pose(2)))

                        'construct INS message
                        'SEN {Type INS} {Name INS} {Location 0.80,-7.28,-0.45} {Orientation 0.00,6.28,0.00}
                        parts.Add("Type " & InsSensor.SENSORTYPE_INS)
                        parts.Add("Name INS")
                        parts.Add(String.Format("Location {0},{1},0.00", pose(0), pose(1)))
                        parts.Add(String.Format("Orientation 0.00,0.00,{0}", pose(2)))
                        receiver.ReceiveMessage(MessageType.Sensor, InsSensor.SENSORTYPE_INS, "INS", parts)

                        'construct Odometry message
                        'SEN {Type Odometry} {Name Odometry} {Pose -0.1451,0.0000,0.0000}
                        'UsarSim expects pose-difference, Carmen gives absolute position 

                        'parts.Add("Type " & OdometrySensor.SENSORTYPE_ODOMETRY)
                        'parts.Add("Name Odometry")
                        'parts.Add(String.Format("Pose {0},{1},{2}", pose(0), pose(1), pose(2)))
                        'receiver.ReceiveMessage(MessageType.Sensor, OdometrySensor.SENSORTYPE_ODOMETRY, "Odometry", parts)

                        'Pose update based on Odometry is not drawn (To Be Checked)

                    Case "truepos" 'Ground Truth message (only use first three values)
                        'TRUEPOS true_x true_y true_theta odom_x odom_y odom_theta
                        Dim pose(3 - 1) As String
                        Array.ConstrainedCopy(nums, 1, pose, 0, pose.Length)
                        pose(1) = CStr(-Double.Parse(pose(1)))
                        pose(2) = CStr(-Double.Parse(pose(2)))

                        'construct similar GroundTruth message
                        'SEN {Type GroundTruth} {Name GroundTruth} {Location 0.80,-7.28,-0.45} {Orientation 0.00,6.28,0.00}
                        parts.Clear()
                        parts.Add("Type " & GroundTruthSensor.SENSORTYPE_GROUNDTRUTH)
                        parts.Add("Name GroundTruth")
                        parts.Add(String.Format("Location {0},{1},0.00", pose(0), pose(1)))
                        parts.Add(String.Format("Orientation 0.00,0.00,{0}", pose(2)))
                        receiver.ReceiveMessage(MessageType.Sensor, GroundTruthSensor.SENSORTYPE_GROUNDTRUTH, "GroundTruth", parts)


                    Case "flaser" 'front laser range scan
                        'FLASER num_readings [range_readings] x y theta odom_x odom_y odom_theta
                        Dim length As Integer = CInt(nums(1))
                        Dim scan(length - 1) As String
                        Array.ConstrainedCopy(nums, 2, scan, 0, scan.Length)
                        'Array.Reverse(scan)

                        'then construct range scan message
                        'SEN {Type RangeScanner} {Name Scanner1} {Resolution 0.0174} {FOV 3.1415} {Range <doubles>}
                        parts.Add("Type " & LaserRangeSensor.SENSORTYPE_RANGESCANNER)
                        parts.Add("Name Scanner1")
                        parts.Add("Range " + Strings.Join(scan, ","))
                        parts.Add("Resolution " + CStr(PI / scan.Length))
                        parts.Add("FOV " + CStr(PI))
                        receiver.ReceiveMessage(MessageType.Sensor, LaserRangeSensor.SENSORTYPE_RANGESCANNER, "Scanner1", parts)

                    Case "robotlaser1" 'same laser, new format
                        '# ROBOTLASER1 laser_type start_angle field_of_view angular_resolution maximum_range accuracy remission_mode num_readings [range_readings] num_remissions [remission values] laser_pose_x laser_pose_y laser_pose_theta robot_pose_x robot_pose_y robot_pose_theta laser_tv laser_rv forward_safety_dist side_safty_dist turn_axis

                        Dim length As Integer = CInt(nums(8))
                        Dim scan(length - 1) As String
                        Array.ConstrainedCopy(nums, 9, scan, 0, scan.Length)
                        'Array.Reverse(scan)

                        'then construct range scan message
                        'SEN {Type RangeScanner} {Name Scanner1} {Resolution 0.0174} {FOV 3.1415} {Range <doubles>}
                        parts.Add("Type " & LaserRangeSensor.SENSORTYPE_RANGESCANNER)
                        parts.Add("Name Scanner1")
                        parts.Add("Range " + Strings.Join(scan, ","))
                        parts.Add("Resolution " + CStr(CInt(nums(3)) / scan.Length))
                        parts.Add("FOV " + CStr(Int(nums(3))))
                        receiver.ReceiveMessage(MessageType.Sensor, LaserRangeSensor.SENSORTYPE_RANGESCANNER, "Scanner1", parts)

                End Select


                success = True
            End If

        End If

        Return success
    End Function

    Public Shared Function ParseCognironLine(ByVal receiver As Agent, ByVal line As String) As Boolean

        'timestamp + 181 or 361 beams over field of view op 180 degrees

        Dim success As Boolean = False
        If Not String.IsNullOrEmpty(line) Then

            line = line.Replace(vbTab, " ").Trim

            Dim nums() As String = Strings.Split(line, " ")


            If nums.Length > 1 Then

                Dim time(0) As String 'first number is time
                Array.ConstrainedCopy(nums, 0, time, 0, time.Length)

                Dim scan(nums.Length - time.Length - 1) As String

                Array.ConstrainedCopy(nums, time.Length, scan, 0, scan.Length)
                Array.Reverse(scan) 'negative z-axis


                Dim parts As New Specialized.StringCollection

                'then construct range scan message
                'SEN {Type RangeScanner} {Name Scanner1} {Resolution 0.0174} {FOV 3.1415} {Range <doubles>}
                parts.Clear()
                parts.Add("Type " & LaserRangeSensor.SENSORTYPE_RANGESCANNER)
                parts.Add("Name Scanner1")
                parts.Add("Range " + Strings.Join(scan, ","))

                parts.Add("Resolution " + CStr(PI / scan.Length))
                parts.Add("FOV " + CStr(PI))

                receiver.ReceiveMessage(MessageType.Sensor, LaserRangeSensor.SENSORTYPE_RANGESCANNER, "Scanner1", parts)

                success = True

            Else
                Console.WriteLine("Received short line: " & line)

            End If
        End If

        Return success

    End Function

    Public Shared Function ParseNomadHokuyoLine(ByVal receiver As Agent, ByVal line As String) As Boolean

        'Can be in two formats:
        '- Hokuyo URG04-LX scan-measurements = timestamp + 769 range measurements in meters (769 x 0.36=276.84 deg) 
        '- Some other scanner = timestamp + 361 beams over field of view op 180 degrees

        Dim success As Boolean = False
        If Not String.IsNullOrEmpty(line) Then

            line = line.Replace(vbTab, " ").Trim

            Dim nums() As String = Strings.Split(line, " ")

            If nums.Length > 1 Then

                Dim time(0) As String 'first number is time
                Array.ConstrainedCopy(nums, 0, time, 0, time.Length)

                'the hokuyo emits status info with the range scan
                '- the first 43 and last 43 numbers should be irnored
                Dim scan(nums.Length - 2 * 43 - time.Length - 1) As String

                Array.ConstrainedCopy(nums, time.Length + 43, scan, 0, scan.Length)
                'Array.Reverse(scan) 'negative z-axis


                Dim useSubset As Boolean = True
                If useSubset Then

                    Dim nbeams As Integer = CInt(((scan.Length) / 2))
                    Dim subscan(nbeams - 1) As String
                    For beam As Integer = 0 To subscan.Length - 1
                        subscan(beam) = scan(beam * 2)
                    Next

                    scan = subscan

                End If


                Dim parts As New Specialized.StringCollection
                Dim ScannerName As String = "AnyName"
                If receiver.IsMounted(LaserRangeSensor.SENSORTYPE_RANGESCANNER, "Scanner1") Then
                    ScannerName = "Scanner1"
                ElseIf receiver.IsMounted(LaserRangeSensor.SENSORTYPE_RANGESCANNER, "LaserScannerURG") Then
                    ScannerName = "LaserScannerURG"
                ElseIf receiver.IsMounted(LaserRangeSensor.SENSORTYPE_RANGESCANNER, "SICK") Then
                    ScannerName = "SICK"
                End If

                'then construct range scan message
                'SEN {Time 123.45} {Type RangeScanner} {Name Scanner1} {Resolution 0.0174} {FOV 3.1415} {Range <doubles>}
                parts.Clear()
                parts.Add("Time " & time(0))
                parts.Add("Type " & LaserRangeSensor.SENSORTYPE_RANGESCANNER)
                parts.Add("Name " + ScannerName)
                parts.Add("Range " + Strings.Join(scan, ","))

                Dim fov As Single = System.Math.PI 'default fov 180 deg
                If ScannerName = "LaserScannerURG" Then
                'the hokuyo has a field-of-view from -2.0944 rad to 2.0944 rad

                    fov = 2 * 2.0944 'radians" Then
                End If
                parts.Add("Resolution " + CStr(fov / scan.Length))
                parts.Add("FOV " + CStr(fov))

                receiver.ReceiveMessage(MessageType.Sensor, LaserRangeSensor.SENSORTYPE_RANGESCANNER, ScannerName, parts)

                success = True
                'Console.WriteLine("Parse hokuyo-line @ time " & time(0))

            Else
                Console.WriteLine("Received short line: " & line)

            End If
        End If

        Return success

    End Function

    '    The columns of sonar.txt hold the following information:

    'column		description
    '1		timestamp of time of measurement, milliseconds since 1970
    '2 	    sonar range information at 0/16 * 2Pi radians in meters 
    '3  	sonar range information at 1/16 * 2Pi radians in meters 
    '4	 	sonar range information at 2/16 * 2Pi radians in meters 
    '5		sonar range information at 3/16 * 2Pi radians in meters 
    '6		sonar range information at 4/16 * 2Pi radians in meters 
    '7		sonar range information at 5/16 * 2Pi radians in meters 
    '8		sonar range information at 6/16 * 2Pi radians in meters 
    '9		sonar range information at 7/16 * 2Pi radians in meters 
    '10	    sonar range information at 8/16 * 2Pi radians in meters 
    '11	    sonar range information at 9/16 * 2Pi radians in meters 
    '12	    sonar range information at 10/16 * 2Pi radians in meters 
    '13	    sonar range information at 11/16 * 2Pi radians in meters 	
    '14	    sonar range information at 12/16 * 2Pi radians in meters 
    '15	    sonar range information at 13/16 * 2Pi radians in meters 
    '16	    sonar range information at 14/16 * 2Pi radians in meters 
    '17	    sonar range information at 15/16 * 2Pi radians in meters 

    Private Const SONARKEY As String = "SONAR" 'Nomad

    Public Shared Function ParseSonarLine(ByVal receiver As Agent, ByVal line As String) As Boolean

        Dim success As Boolean = False

        line = line.Replace(vbTab, " ").Trim

        Dim nums() As String = Strings.Split(line, " ")
        If nums.Length = 17 Then 'sonar.txt

            Dim time(0) As String 'first number is time
            Dim range(16 - 1) As String 'then comes the pose in 2D

            Array.ConstrainedCopy(nums, 0, time, 0, time.Length)
            Array.ConstrainedCopy(nums, time.Length, range, 0, range.Length)

            'construct SONAR message
            'SEN {Time 45.14} {Type Sonar} {Name F1 Range 4.4690} {Name F2 Range 1.9387} 

            Dim parts As New Specialized.StringCollection
            parts.Add("Type " & SonarSensor.SENSORTYPE_SONAR)
            For i As Integer = 0 To range.Length - 1
                parts.Add(String.Format("Name {0}{1} Range {2}", SONARKEY, i, range(i)))
            Next
            receiver.ReceiveMessage(MessageType.Sensor, SonarSensor.SENSORTYPE_SONAR, Sensor.MATCH_ANYNAME, parts)

            success = True
            'Console.WriteLine("Parse sonar-line @ time " & time(0))

        ElseIf nums.Length > 17 Then 'OdometryAndSonar.txt

            Dim time(0) As String 'first number is time
            Dim pose(3 - 1) As String 'then comes the pose in 2D
            Dim range(16 - 1) As String 'then comes the pose in 2D

            Array.ConstrainedCopy(nums, 0, time, 0, time.Length)
            Array.ConstrainedCopy(nums, time.Length, pose, 0, pose.Length)
            Array.ConstrainedCopy(nums, time.Length + pose.Length + 2, range, 0, range.Length) 'skipping trans-speed(m/s) angle-speed(rad/s)

            'construct INS message
            'SEN {Type INS} {Name INS} {Location 0.80,-7.28,-0.45} {Orientation 0.00,6.28,0.00}
            Dim parts As New Specialized.StringCollection
            parts.Add("Type " & InsSensor.SENSORTYPE_INS)
            parts.Add("Name INS")
            parts.Add(String.Format("Location {0},{1},0.00", pose(0), pose(1)))
            parts.Add(String.Format("Orientation 0.00,0.00,{0}", pose(2)))
            receiver.ReceiveMessage(MessageType.Sensor, InsSensor.SENSORTYPE_INS, "INS", parts)

            'construct SONAR message
            'SEN {Time 45.14} {Type Sonar} {Name F1 Range 4.4690} {Name F2 Range 1.9387} 
            parts.Clear()
            parts.Add("Type " & SonarSensor.SENSORTYPE_SONAR)
            For i As Integer = 0 To range.Length - 1
                parts.Add(String.Format("Name {0}{1} Range {2}", SONARKEY, i, range(i)))
            Next
            receiver.ReceiveMessage(MessageType.Sensor, SonarSensor.SENSORTYPE_SONAR, Sensor.MATCH_ANYNAME, parts)

            success = True
            'Console.WriteLine("Parse sonar-line @ time " & time(0))

        Else

            Console.WriteLine("Received short line: " & line)

        End If

        Return success
    End Function


    Public Shared Function ParseInsLine(ByVal receiver As Agent, ByVal line As String) As Boolean

        Dim success As Boolean = False


        line = line.Replace(vbTab, " ").Trim

        Dim nums() As String = Strings.Split(line, " ")
        If nums.Length = 4 Then 'OdometrySLAM.txt

            Dim time(0) As String 'first number is time
            Dim pose(3 - 1) As String 'then comes the pose in 2D

            Array.ConstrainedCopy(nums, 0, time, 0, time.Length)
            Array.ConstrainedCopy(nums, time.Length, pose, 0, pose.Length)

            'construct similar GroundTruth message
            'SEN {Type GroundTruth} {Name GroundTruth} {Location 0.80,-7.28,-0.45} {Orientation 0.00,6.28,0.00}
            Dim parts As New Specialized.StringCollection

            parts.Add("Type " & GroundTruthSensor.SENSORTYPE_GROUNDTRUTH)
            parts.Add("Name GroundTruth")
            parts.Add(String.Format("Location {0},{1},0.00", pose(0), pose(1)))
            parts.Add(String.Format("Orientation 0.00,0.00,{0}", pose(2)))
            receiver.ReceiveMessage(MessageType.Sensor, GroundTruthSensor.SENSORTYPE_GROUNDTRUTH, "GroundTruth", parts)

            success = True
            'Console.WriteLine("Parse ins-line @ time " & time(0))
        ElseIf nums.Length > 4 Then 'odometry.txt or OdometryAndSonar.txt

            'typical line format:
            '#time(ms) x(m) y(m) phi(rad) trans-speed(m/s) angle-speed(rad/s) sonar(rad:0.000000)-distance(m) sonar(rad:0.392699)-distance(m) sonar(rad:0.785398)-distance(m) sonar(rad:1.178097)-distance(m) sonar(rad:1.570796)-distance(m) sonar(rad:1.963495)-distance(m) sonar(rad:2.356194)-distance(m) sonar(rad:2.748894)-distance(m) sonar(rad:3.141593)-distance(m) sonar(rad:-2.748894)-distance(m) sonar(rad:-2.356194)-distance(m) sonar(rad:-1.963495)-distance(m) sonar(rad:-1.570796)-distance(m) sonar(rad:-1.178097)-distance(m) sonar(rad:-0.785398)-distance(m) sonar(rad:-0.392699)-distance(m)


            Dim time(0) As String 'first number is time
            Dim pose(3 - 1) As String 'then comes the pose in 2D

            Array.ConstrainedCopy(nums, 0, time, 0, time.Length)
            Array.ConstrainedCopy(nums, time.Length, pose, 0, pose.Length)

            'construct INS message
            'SEN {Type INS} {Name INS} {Location 0.80,-7.28,-0.45} {Orientation 0.00,6.28,0.00}
            Dim parts As New Specialized.StringCollection
            parts.Add("Type " & InsSensor.SENSORTYPE_INS)
            parts.Add("Name INS")
            parts.Add(String.Format("Location {0},{1},0.00", pose(0), pose(1)))
            parts.Add(String.Format("Orientation 0.00,0.00,{0}", pose(2)))
            receiver.ReceiveMessage(MessageType.Sensor, InsSensor.SENSORTYPE_INS, "INS", parts)


            'construct similar GroundTruth message
            'SEN {Type GroundTruth} {Name GroundTruth} {Location 0.80,-7.28,-0.45} {Orientation 0.00,6.28,0.00}
            parts.Clear()
            parts.Add("Type " & GroundTruthSensor.SENSORTYPE_GROUNDTRUTH)
            parts.Add("Name GroundTruth")
            parts.Add(String.Format("Location {0},{1},0.00", pose(0), pose(1)))
            parts.Add(String.Format("Orientation 0.00,0.00,{0}", pose(2)))
            receiver.ReceiveMessage(MessageType.Sensor, GroundTruthSensor.SENSORTYPE_GROUNDTRUTH, "GroundTruth", parts)

            success = True
            'Console.WriteLine("Parse ins-line @ time " & time(0))
        Else
            Console.WriteLine("Received short line: " & line)

        End If

        Return success

    End Function


    Public Shared Function ParseCameraLine(ByVal receiver As Agent, ByVal line As String, ByVal path As String, ByVal counter As Integer) As Boolean

        Dim success As Boolean = False

        'typical line format:
        '1192018128.789519 images/image0000000.jpg

        line = line.Replace(vbTab, " ").Trim

        Dim nums() As String = Strings.Split(line, " ")
        If nums.Length >= 1 Then

            Dim time(0) As String 'first number is time
            Dim image(0) As String 'then comes the pose in 2D

            Array.ConstrainedCopy(nums, 0, time, 0, time.Length)

            If nums.Length >= 2 Then
                Array.ConstrainedCopy(nums, time.Length, image, 0, image.Length)
            Else
                image(0) = "ImagesOmnicam\" + counter.ToString("0000000") + ".jpg"
            End If

            If File.Exists(path + "\" + image(0)) Then

                'Is this thread-safe? Should tbis protected with a SyncLock?

                Dim jpeg() As Byte = File.ReadAllBytes(path + "\" + image(0))
                Dim header(5) As Byte ' see http://athiri.cimds.ri.cmu.edu/foswiki/UsarSim/UpisInterface
                header(0) = 3 'jpeg - normal quality 
                'Dim length As Integer = CInt(bytes(1) * 2 ^ 24) + CInt(bytes(2) * 2 ^ 16) + CInt(bytes(3) * 2 ^ 8) + CInt(bytes(4) * 2 ^ 0)
                header(1) = CByte(System.Math.Floor((jpeg.Length / (2 ^ 24))))
                header(2) = CByte(System.Math.Floor((jpeg.Length - (header(1) * (2 ^ 24))) / (2 ^ 16)))
                header(3) = CByte(System.Math.Floor((jpeg.Length - (header(1) * (2 ^ 24)) - (header(2) * (2 ^ 16))) / (2 ^ 8)))
                header(4) = CByte(System.Math.Floor((jpeg.Length - (header(1) * (2 ^ 24)) - (header(2) * (2 ^ 16)) - (header(3) * (2 ^ 8))) / (2 ^ 0)))

                Dim bytes(5 + jpeg.Length - 1) As Byte

                Array.ConstrainedCopy(header, 0, bytes, 0, 5)
                header = Nothing
                Array.ConstrainedCopy(jpeg, 0, bytes, 5, jpeg.Length)
                jpeg = Nothing

                If Not ImageParser.ParseImageData(receiver, bytes) Then
                    Console.Error.WriteLine("Could not parse image data." & image(0))
                End If
                bytes = Nothing

            Else
                Console.WriteLine("ParseCameraLine: Could not find " & line)
            End If

            success = True
            'Console.WriteLine("Parse cam-line @ time " & time(0))

        Else
            Console.WriteLine("Received short line: " & line)

        End If

        Return success

    End Function

    Public Shared Function ParseLongwoodLine(ByVal receiver As Agent, ByVal line As String) As Boolean

        'The Longwood at Oakmont nursing home. This map was constructed out of two separate data runs, where the data was collected using an old robot control software package called beeSoft. Two separate maps were created from the data, and then merged. The map resolution is 10cm.

        'The log file format has been lost in the mists of time, but is relatively easy to understand. 

        '@SENS 12-12-00 20:46:56.958857
        '#LASER 180 0: 127 116 106 96 86 86 81 76 66 66 61 61 61 56 56 51 51 46 46 46 41 46 41 41 41 41 41 46 56 321 209 204 254 341 159 159 164 336 161 149 149 401 221 221 231 286 269 276 441 436 376 119 120 139 179 186 471 229 229 274 486 142 142 691 666 621 601 179 179 561 551 539 536 546 551 519 551 511 496 516 516 274 281 351 521 256 254 311 361 551 541 581 336 274 151 94 94 94 151 556 301 294 227 227 531 331 329 521 249 516 511 511 126 129 129 501 501 501 501 76 69 69 74 326 486 356 491 254 101 99 99 111 351 491 249 246 319 381 286 276 71 66 66 66 61 66 61 61 61 61 71 51 51 51 56 51 51 51 46 51 51 51 46 46 46 41 46 46 49 49 49 44 49 49 44 49 44 44 49 49
        'params: 286250.312500 286895.031250 -144.859161 0    0.000000 0.000000
        'position: 0.849990 0.013153 342.746490
        'end(pattern)
        'arnoud: params seems to be a copy of the odometry, position seems to be a estimate of SLAM algorithm

        '@SENS 12-12-00 20:46:57.65362
        '#ROBOT 286249.812500 286894.531250 -165.985916
        'arnoud: odometry x,y, orientation?!

        '@SENS 12-12-00 20:46:57.260489
        '#SONAR 24: 647.700012 647.700012 647.700012 647.700012 647.700012 591.799988 647.700012 647.700012 647.700012 647.700012 647.700012 571.500000 647.700012 647.700012 647.700012 647.700012 0.000000 0.000000 0.000000 0.000000 0.000000 0.000000 0.000000 0.000000



        Dim success As Boolean = False
        If Not String.IsNullOrEmpty(line) Then

            line = line.Replace(vbTab, " ").Trim

            Dim nums() As String = Strings.Split(line, " ")


            If nums.Length > 0 Then

                Dim parts As New Specialized.StringCollection

                Dim type As String = nums(0).ToLower

                Select Case type
                    Case "params:" 'additional parameters, ignore

                        'arnoud: params seems to be a copy of the odometry

                    Case "end(pattern)" 'ready with laserscan

                    Case "@open" 'the start time of the logfile

                    Case "@sens" 'the time of the measurement

                    Case "#sonar" 'ignore for the moment

                    Case "#robot" 'odometry, not initiated with start position

                        Dim pose(3 - 1) As String
                        Array.ConstrainedCopy(nums, 1, pose, 0, pose.Length)
                        pose(0) = CStr(-Double.Parse(pose(0)) / 100) 'milimeters instead of meters
                        pose(1) = CStr(-Double.Parse(pose(1)) / 100)
                        pose(2) = CStr(-Double.Parse(pose(2)) / 180 * PI) 'degrees, no radians

                        'construct INS message
                        'SEN {Type INS} {Name INS} {Location 0.80,-7.28,-0.45} {Orientation 0.00,6.28,0.00}
                        parts.Add("Type " & InsSensor.SENSORTYPE_INS)
                        parts.Add("Name INS")
                        parts.Add(String.Format("Location {0},{1},0.00", pose(0), pose(1)))
                        parts.Add(String.Format("Orientation 0.00,0.00,{0}", pose(2)))
                        receiver.ReceiveMessage(MessageType.Sensor, InsSensor.SENSORTYPE_INS, "INS", parts)

                        'parts.Add("Type " & OdometrySensor.SENSORTYPE_ODOMETRY)
                        'parts.Add("Name Odometry")
                        'parts.Add(String.Format("Pose {0},{1},{2}", pose(0), pose(1), pose(2)))
                        'receiver.ReceiveMessage(MessageType.Sensor, OdometrySensor.SENSORTYPE_ODOMETRY, "Odometry", parts)


                    Case "position:" 'position estimate in absolute coordinates

                        'arnoud: position seems to be a estimate of SLAM algorithm (USE to generate INS message?!)
                        'position: 0.849990 0.013153 342.746490
                        Dim pose(3 - 1) As String
                        Array.ConstrainedCopy(nums, 1, pose, 0, pose.Length)
                        pose(0) = CStr(-Double.Parse(pose(0)) / 100) 'milimeters instead of meters
                        pose(1) = CStr(-Double.Parse(pose(1)) / 100)
                        pose(2) = CStr(-Double.Parse(pose(2)) / 180 * PI) 'degrees, no radians


                        'construct similar GroundTruth message
                        'SEN {Type GroundTruth} {Name GroundTruth} {Location 0.80,-7.28,-0.45} {Orientation 0.00,6.28,0.00}
                        parts.Clear()
                        parts.Add("Type " & GroundTruthSensor.SENSORTYPE_GROUNDTRUTH)
                        parts.Add("Name GroundTruth")
                        parts.Add(String.Format("Location {0},{1},0.00", pose(0), pose(1)))
                        parts.Add(String.Format("Orientation 0.00,0.00,{0}", pose(2)))
                        receiver.ReceiveMessage(MessageType.Sensor, GroundTruthSensor.SENSORTYPE_GROUNDTRUTH, "GroundTruth", parts)


                        success = True

                    Case "#laser"

                        '#LASER 180 0: 127 116 106 96 86 86 81 76 66 66 61 61 61 56 56 51 51 46 46 46 41 46 41 41 41 41 41 46 56 321 209 204 254 341 159 159 164 336 161 149 149 401 221 221 231 286 269 276 441 436 376 119 120 139 179 186 471 229 229 274 486 142 142 691 666 621 601 179 179 561 551 539 536 546 551 519 551 511 496 516 516 274 281 351 521 256 254 311 361 551 541 581 336 274 151 94 94 94 151 556 301 294 227 227 531 331 329 521 249 516 511 511 126 129 129 501 501 501 501 76 69 69 74 326 486 356 491 254 101 99 99 111 351 491 249 246 319 381 286 276 71 66 66 66 61 66 61 61 61 61 71 51 51 51 56 51 51 51 46 51 51 51 46 46 46 41 46 46 49 49 49 44 49 49 44 49 44 44 49 49

                        Dim length As Integer = CInt(nums(1))
                        If length = nums.Length - 3 Then

                            Dim scan(length - 1) As String
                            Dim range(length - 1) As Double
                            Array.ConstrainedCopy(nums, 3, scan, 0, scan.Length)
                            Array.Reverse(scan)

                            For i As Integer = 0 To length - 1
                                range(i) = Double.Parse(scan(i)) / 100 'values seem to be in milimeters
                                scan(i) = String.Format("{0}", range(i)) 'make sure your Regional settings are OK
                            Next

                            'then construct range scan message
                            'SEN {Type RangeScanner} {Name Scanner1} {Resolution 0.0174} {FOV 3.1415} {Range <doubles>}
                            parts.Add("Type " & LaserRangeSensor.SENSORTYPE_RANGESCANNER)
                            parts.Add("Name Scanner1")
                            parts.Add("Range " + Strings.Join(scan, ","))
                            parts.Add("Resolution " + CStr(PI / scan.Length))
                            parts.Add("FOV " + CStr(PI))
                            receiver.ReceiveMessage(MessageType.Sensor, LaserRangeSensor.SENSORTYPE_RANGESCANNER, "Scanner1", parts)
                        Else
                            Console.WriteLine(String.Format("Laser scan with {0} points", nums.Length - 3))


                        End If
                End Select

            Else
                Console.WriteLine("Received short line: " & line)

            End If


            success = True
        End If


   
        Return success

    End Function

    Public Sub New()
        
    End Sub
End Class

