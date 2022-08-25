Imports System.IO

Public Class LogFileWriter
    Inherits StreamWriter

    Public Sub New(ByVal logfile As String)
        MyBase.New(Path.GetFullPath(logfile), False, Text.Encoding.ASCII)
        Me._LogFile = logfile
        Me.PrintHeader()
    End Sub

    Private _LogFile As String
    Public ReadOnly Property LogFile() As String
        Get
            Return Me._LogFile
        End Get
    End Property

    Private Sub PrintHeader()

        Dim builder As New Text.StringBuilder
        Dim c As String = My.Settings.CommentChar
        builder.AppendLine(c & " UvA Rescue Team Logfile")
        builder.AppendLine(c)
        builder.AppendLine(c & " Created Date: " & String.Format("{0:dd}-{0:MM}-{0:yyyy}", Now))
        builder.AppendLine(c & " Created Time: " & String.Format("{0:HH}:{0:mm}", Now))
        builder.AppendLine(c)
        builder.AppendLine(c & " Machine: " & My.Computer.Name)
        builder.AppendLine(c)
        builder.AppendLine(c & " Original Filename: " & Me._LogFile)

        Me.WriteLine(builder.ToString)

    End Sub

End Class
