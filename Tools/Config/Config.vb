Imports System.Collections.Specialized
Imports System.IO

Public Class Config

   

    Public Sub New()
        Dim cfgfile As String = "newmap"
        If File.Exists(cfgfile) Then
            Try
                Me.Load(cfgfile)
                Console.WriteLine("i found that file")
            Catch ex As Exception
                Console.WriteLine("Error occurred loading configuration file.")
                Console.WriteLine(ex.Message & vbNewLine & ex.StackTrace)
            End Try
        End If
    End Sub

#Region " Load / Save "

    Private _FileName As String = String.Empty
    Public ReadOnly Property FileName() As String
        Get
            Return Me._FileName
        End Get
    End Property

    Public Sub Reload()
        Me.Load(Me._FileName)
    End Sub

    Public Sub Load(ByVal filename As String)


        Me._Sections.Clear()

        If String.IsNullOrEmpty(filename) OrElse Not File.Exists(filename) Then
            Exit Sub
        End If

        filename = Path.GetFullPath(filename)

        Using reader As StreamReader = File.OpenText(filename)

            Dim section As String = ""
            Dim configs As Dictionary(Of String, String) = Nothing

            Dim line As String
            While Not reader.EndOfStream
                line = reader.ReadLine.Trim

                If line.StartsWith("#") Or line.StartsWith(";") Or line.StartsWith("//") Then
                    'ignore comments
                    Continue While
                End If

                If line.StartsWith("[") Then
                    'a new section starts here, store current one 
                    If Not String.IsNullOrEmpty(section) Then
                        Me._Sections.Add(Config.Section.Load(section, configs))
                    End If

                    'extract next section name
                    section = line.Substring(1)
                    If section.EndsWith("]") Then
                        'prune ']'
                        section = section.Substring(0, section.Length - 1)
                    End If

                    'reset dictionary 
                    configs = New Dictionary(Of String, String)


                ElseIf Not IsNothing(configs) Then

                    'extract config setting if present
                    If line.Contains("=") Then
                        Dim idx As Integer = line.IndexOf("=")
                        If idx < line.Length Then
                            Dim key As String = line.Substring(0, idx).Trim
                            Dim value As String = line.Substring(idx + 1).Trim()

                            If Not String.IsNullOrEmpty(key) Then
                                If configs.ContainsKey(key) Then
                                    configs(key) = value 'overwrite
                                Else
                                    configs.Add(key, value)
                                End If
                            End If

                        End If

                    End If

                End If

            End While

            'make sure the last config section is also included
            If Not String.IsNullOrEmpty(section) Then
                Me._Sections.Add(Config.Section.Load(section, configs))
            End If

            reader.Close()

        End Using

        'loaded successfully
        Me._FileName = filename

    End Sub

    Public Sub Save(ByVal filename As String)
        Try

            filename = Directory.GetCurrentDirectory + "\newmap"
            Using writer As New StreamWriter(filename, False)
                For Each section As Section In Me._Sections
                    section.Save(writer)
                Next

                writer.Flush()
                writer.Close()

            End Using

            'saved successfully
            Me._FileName = filename
        Catch ex As Exception
            Console.Out.WriteLine("Could not save to '" + filename + "'.")
        End Try
    End Sub

#End Region


#Region " Sections and Get/Set/Has Configs "

    Private _Sections As New List(Of Section)

    Private Function HasSection(ByVal name As String) As Boolean
        For Each section As Section In Me._Sections
            If section.Name = name.ToLower Then
                Return True
            End If
        Next
        Return False
    End Function
    Private Function GetSection(ByVal name As String) As Section
        For Each section As Section In Me._Sections
            If section.Name = name.ToLower Then
                Return section
            End If
        Next
        Return Nothing
    End Function

    Public Function GetConfig(ByVal section As String, ByVal key As String) As String
        Dim oSection As Section = Me.GetSection(section)
        If Not IsNothing(oSection) Then
            Return oSection.GetConfig(key)
        End If
        Return Nothing
    End Function

    Public Function GetConfig(ByVal section As String, ByVal key As String, ByVal valueIfNotExists As String) As String
        Dim oSection As Section = Me.GetSection(section)
        If Not IsNothing(oSection) Then
            Return oSection.GetConfig(key, valueIfNotExists)
        End If
        Return valueIfNotExists
    End Function

    Public Sub SetConfig(ByVal section As String, ByVal key As String, ByVal value As String)
        Dim oSection As Section = Me.GetSection(section)
        If IsNothing(oSection) Then
            oSection = Config.Section.Create(section)
            Me._Sections.Add(oSection)
        End If
        oSection.SetConfig(key, value)
    End Sub

    Public Function HasConfig(ByVal section As String, ByVal key As String) As Boolean
        Dim oSection As Section = Me.GetSection(section)
        If Not IsNothing(oSection) Then
            Return oSection.HasConfig(key)
        End If
        Return False
    End Function

#End Region


#Region " Section Class "

    Private Class Section

#Region " Create / Load / Save ConfigSection "

        Friend Shared Function Create(ByVal name As String) As Section
            Return New Section(name, New Dictionary(Of String, String))
        End Function

        Friend Shared Function Load(ByVal name As String, ByVal configs As Dictionary(Of String, String)) As Section
            Return New Section(name, configs)
        End Function

        Friend Sub Save(ByVal writer As StreamWriter)

            writer.WriteLine(String.Format("[{0}]", Me.Name))

            Dim sorted As New SortedDictionary(Of String, String)(Me._Configs)
            For Each pair As KeyValuePair(Of String, String) In sorted
                writer.WriteLine(String.Format("{0} = {1}", pair.Key, pair.Value))
            Next

            writer.WriteLine()
            writer.WriteLine()

        End Sub

        Protected Sub New(ByVal name As String, ByVal configs As Dictionary(Of String, String))
            Me._Name = name.ToLower
            Me._Configs = configs
        End Sub

#End Region

#Region " Name and Get/Set/Has Config "

        Private _Name As String
        Private _Configs As Dictionary(Of String, String)

        Public ReadOnly Property Name() As String
            Get
                Return Me._Name
            End Get
        End Property


        Public Function GetConfig(ByVal key As String) As String
            If Me._Configs.ContainsKey(key.ToLower) Then
                Return Me._Configs(key.ToLower)
            End If
            Return Nothing
        End Function

        Public Function GetConfig(ByVal key As String, ByVal valueIfNotExists As String) As String
            Dim value As String = Me.GetConfig(key)
            If String.IsNullOrEmpty(value) Then
                value = valueIfNotExists
            End If
            Return value
        End Function

        Public Sub SetConfig(ByVal key As String, ByVal value As String)
            Me._Configs(key.ToLower) = value
        End Sub

        Public Function HasConfig(ByVal key As String) As Boolean
            Return Me._Configs.ContainsKey(key.ToLower)
        End Function

#End Region

    End Class

#End Region


End Class
