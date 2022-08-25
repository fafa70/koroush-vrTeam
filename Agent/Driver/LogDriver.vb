Imports System.IO
Imports System.Threading
Imports System.Globalization

''' <summary>
''' Will use a logfile to obtain sensor readings.
''' </summary>
''' <remarks></remarks>
Public Class LogDriver
    Inherits Driver


    Public Delegate Function ParseDelegate(ByVal receiver As Agent, ByVal line As String) As Boolean


    Public Sub New(ByVal agent As Agent, ByVal logfile As String, ByVal parseHandler As ParseDelegate)
        MyBase.New(agent)

        If File.Exists(logfile) Then
            Me.LogFile = logfile
        Else
            Me.LogFile = ""
        End If


        Me._Parse = parseHandler

        Dim logext As String = Path.GetExtension(logfile)
        Dim insext As String = ".ins"
        Dim logpath As String = Path.GetDirectoryName(logfile)
        Dim insfile As String = logfile.Substring(0, logfile.Length - logext.Length) & insext 'The old way
        Dim sonarfile As String = logfile.Substring(0, logfile.Length - logext.Length) & ".sonar" 'The old way
        Dim imtimesfile As String = logfile.Substring(0, logfile.Length - logext.Length) & ".camera" 'The old way

        If logpath.EndsWith("set10000") Then
            insfile = String.Concat(logpath, "\odometry.txt")
        Else
            insfile = String.Concat(logpath, "\OdometrySLAM.txt")
        End If


        If File.Exists(insfile) Then
            Me._InsFile = insfile
        Else
            Me._InsFile = ""
        End If

        If logpath.EndsWith("set10000") Then
            sonarfile = String.Concat(logpath, "\sonar.txt")
        Else
            sonarfile = String.Concat(logpath, "\OdometryAndSonar.txt")
        End If

        If File.Exists(sonarfile) Then
            Me._SonarFile = sonarfile
        Else
            Me._SonarFile = ""
        End If

        If logpath.EndsWith("set10000") Then
            imtimesfile = String.Concat(logpath, "\imtimes.txt")
        Else
            imtimesfile = String.Concat(logpath, "\ImagesOmnicamT.txt")
        End If


        If File.Exists(imtimesfile) Then
            Me._CamFile = imtimesfile
        Else
            Me._CamFile = ""
        End If

    End Sub


    Private _InsFile As String
    Public ReadOnly Property InsFile() As String
        Get
            Return Me._InsFile
        End Get
    End Property

    Private _SonarFile As String
    Public ReadOnly Property SonarFile() As String
        Get
            Return Me._SonarFile
        End Get
    End Property

    Private _CamFile As String
    Public ReadOnly Property CamFile() As String
        Get
            Return Me._CamFile
        End Get
    End Property


    Private _Parse As ParseDelegate
    Protected ReadOnly Property Parse() As ParseDelegate
        Get
            Return Me._Parse
        End Get
    End Property

    'Private CameraMutex As New Object

    Protected Overrides Sub Run()

        System.Threading.Thread.CurrentThread.Name = Me.Agent.Name

        'See http://msdn.microsoft.com/en-us/library/system.globalization.cultureinfo.currentculture(VS.71).aspx

        If Not CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator = NumberFormatInfo.InvariantInfo.NumberDecimalSeparator Then
            'This prevents BufferedLayer overflow, due to huge laser ranges if ',' and '.' are interchanged
            Thread.CurrentThread.CurrentCulture = New CultureInfo("en-US", False)
            Console.WriteLine("[LogDriver:Run]: CurrentCulture is now {0}.", CultureInfo.CurrentCulture.Name)
        End If

        'open a reader to the logfile and optionally the odofile and sonarfile
        Dim logreader As StreamReader = Nothing
        If Not String.IsNullOrEmpty(Me.LogFile) Then
            logreader = File.OpenText(Path.GetFullPath(Me.LogFile))
        End If
        Dim insreader As StreamReader = Nothing
        If Not String.IsNullOrEmpty(Me.InsFile) Then
            insreader = File.OpenText(Path.GetFullPath(Me.InsFile))
        End If
        Dim sonarreader As StreamReader = Nothing
        If Not String.IsNullOrEmpty(Me.SonarFile) Then
            sonarreader = File.OpenText(Path.GetFullPath(Me.SonarFile))
        End If
        Dim camreader As StreamReader = Nothing
        If Not String.IsNullOrEmpty(Me.CamFile) Then
            camreader = File.OpenText(Path.GetFullPath(Me.CamFile))
        End If

        Dim logline As String = ""
        Dim logtime As Double = Double.MinValue

        Dim insline As String = ""
        Dim instime As Double = Double.MinValue

        Dim sonarline As String = ""
        Dim sonartime As Double = Double.MinValue

        Dim camline As String = ""
        Dim camtime As Double = Double.MinValue
        Dim camcounter As Integer = 0


        'a single line will hold a single server message or sensor reading
        While Me.IsRunning

            Me.CheckForPause()

            If Not IsNothing(logreader) AndAlso logreader.EndOfStream Then
                Exit While
            End If

            While Not IsNothing(logreader) AndAlso Not logreader.EndOfStream AndAlso String.IsNullOrEmpty(logline)

            logline = logreader.ReadLine

            'skip commented lines
                'If logline.StartsWith(My.Settings.CommentChar) AndAlso Not Me.Agent.AgentConfig.LogFileFormat = "Longwood" Then
                logline = ""
                Continue While
                'End If

                'skip INFO message, otherwise the Agent code will think that
                'the agent was actually spawned in UsarSim, would cause numerous
                'exceptions
                If logline.StartsWith("NFO") Then
                    logline = ""
                    Continue While
                End If
            End While

            'get timestamp
            'If Not String.IsNullOrEmpty(logline) AndAlso Not logline.StartsWith("STA") Then
            '    'assume timestamps are in msecs and are the first double in the line
            '    logtime = Double.Parse(logline.Substring(0, logline.IndexOf(" ")))
            'Else
            logtime = Double.MaxValue '
            'End If


            'ok, we found a line with laser readings for the agent, now check if we need
            'to get other sensor data first
            While (logtime > sonartime OrElse logtime > instime OrElse logtime > camtime)

                'older sonar, ins or camera-data is available, so synchronize with the laser-data
                'see if we need to move forward in log-file

                While Not IsNothing(sonarreader) AndAlso Not sonarreader.EndOfStream AndAlso String.IsNullOrEmpty(sonarline)

                    sonarline = sonarreader.ReadLine

                    'skip commented lines
                    If sonarline.StartsWith(My.Settings.CommentChar) Then
                        sonarline = ""
                        Continue While
                    End If
                End While

                'get timestamp
                If Not String.IsNullOrEmpty(sonarline) Then
                    sonarline = sonarline.Replace(vbTab, " ").Trim
                    sonartime = Double.Parse(sonarline.Substring(0, sonarline.IndexOf(" ")))
                Else
                    sonartime = Double.MaxValue
                End If


                While Not IsNothing(insreader) AndAlso Not insreader.EndOfStream AndAlso String.IsNullOrEmpty(insline)

                    insline = insreader.ReadLine

                    'skip commented lines
                    If insline.StartsWith(My.Settings.CommentChar) Then
                        insline = ""
                        Continue While
                    End If
                End While

                'get timestamp
                If Not String.IsNullOrEmpty(insline) Then
                    insline = insline.Replace(vbTab, " ").Trim
                    instime = Double.Parse(insline.Substring(0, insline.IndexOf(" ")))
                Else
                    instime = Double.MaxValue
                End If

                While Not IsNothing(camreader) AndAlso Not camreader.EndOfStream AndAlso String.IsNullOrEmpty(camline)

                    camline = camreader.ReadLine

                    'skip commented lines
                    If camline.StartsWith(My.Settings.CommentChar) Then
                        camline = ""
                        Continue While
                    End If
                End While

                'get timestamp
                If Not String.IsNullOrEmpty(camline) Then
                    camline = camline.Replace(vbTab, " ").Trim
                    If camline.Contains(" ") Then
                        camtime = Double.Parse(camline.Substring(0, camline.IndexOf(" ")))
                    Else
                        camtime = Double.Parse(camline)
                    End If

                Else
                    camtime = Double.MaxValue
                End If


                If sonartime = Double.MaxValue AndAlso instime = Double.MaxValue AndAlso camtime = Double.MaxValue Then
                    insline = ""
                    sonarline = ""
                    camline = "" 'read three new lines
                    Exit While
                ElseIf sonartime <= instime AndAlso sonartime <= camtime AndAlso sonartime <= logtime Then
                    If Not LineParsers.ParseSonarLine(Me.Agent, sonarline) Then
                        Console.Error.WriteLine("Could not parse sonar-line: " & vbNewLine & vbTab & sonarline)
                    End If
                    sonarline = "" 'so, read new line

                ElseIf instime <= sonartime AndAlso instime <= camtime AndAlso instime <= logtime Then
                    If Not LineParsers.ParseInsLine(Me.Agent, insline) Then
                        Console.Error.WriteLine("Could not parse ins-line: " & vbNewLine & vbTab & insline)
                    End If
                    insline = "" 'so, read new line
                ElseIf camtime <= sonartime AndAlso camtime <= instime AndAlso camtime <= logtime Then
                    camcounter += 1
                    'SyncLock CameraMutex
                    If Not LineParsers.ParseCameraLine(Me.Agent, camline, Path.GetDirectoryName(Me.LogFile), camcounter) Then
                        Console.Error.WriteLine("Could not parse cam-line: " & vbNewLine & vbTab & camline)
                    End If
                    camline = "" 'so, read new line
                    'End SyncLock
                Else
                    'logtime> sonartime AndAlso logtime > instime -> So the break comes 
                    'Console.Error.WriteLine("LogDriver: Strange combination of times " & logtime & sonartime & instime)
                End If

                'If sonartime = Double.MaxValue Then
                '    sonartime = Double.MinValue
                'End If
                'If instime = Double.MaxValue Then
                '    instime = Double.MinValue
                'End If
                'If camtime = Double.MaxValue Then
                '    camtime = Double.MinValue
                'End If
            End While 'logtime > sonartime AndAlso logtime > instime


            'forward line the the referenced parsing function
            If Not Me.Parse(Me.Agent, logline) Then
                Console.Error.WriteLine("Could not parse log-line: " & vbNewLine & vbTab & logline)
            End If
            logline = "" 'so, read new line

        End While

    End Sub

End Class
