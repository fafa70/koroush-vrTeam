Imports System.Text

Public Class TcpMessagingConnection
    Inherits TcpConnection


    Protected Const CHARCODE_CARRIAGERETURN As Integer = 13
    Protected Const CHARCODE_LINEFEED As Integer = 10

    Private buffer() As Byte = Nothing

    Public Function ReceiveMessages(ByVal maxLength As Integer) As Specialized.StringCollection

        If Not Me.IsConnected Then Throw New InvalidOperationException("You should connect first")

        Dim messages As New Specialized.StringCollection

        'receive any inbound messages
        If stream.DataAvailable Then

            'load data into buffer, max 4K per iteration
            Dim bytes(maxLength - 1) As Byte

            'nread will tell how many bytes were actually retrieved
            Dim nread As Integer = stream.Read(bytes, 0, bytes.Length)

            Dim offset As Integer
            If IsNothing(Me.buffer) Then
                'no data available from previous read, start new array
                offset = 0
                ReDim buffer(nread - 1)
            Else
                'expand previously retrieved data 
                offset = buffer.Length
                ReDim Preserve buffer(buffer.Length + nread - 1)
            End If

            'copy bytes into buffer
            For i As Integer = 0 To nread - 1
                buffer(offset + i) = bytes(i)
            Next


            'search fo a line-feeds 
            Dim start As Integer = 0 'to store offset of current line
            Dim until As Integer = 0 'to store until which byte was processed

            For i As Integer = offset To buffer.Length - 1

                If buffer(i) = CHARCODE_LINEFEED Then

                    'set until var
                    If i > 0 AndAlso buffer(i - 1) = CHARCODE_CARRIAGERETURN Then
                        'make sure also the carriage return is stripped
                        until = i - 1
                    Else
                        until = i 'was 1 (seems to be a bug, arnoud, 15 Feb 09)
                    End If

                    'found linefeed, extract the string until this byte 
                    Dim line(until - start - 1) As Byte
                    For k As Integer = 0 To line.Length - 1
                        line(k) = buffer(start + k)
                    Next

                    'process the line as ascii text and add it to the list
                    messages.Add(Encoding.ASCII.GetString(line))

                    'next line starts at i + 1
                    start = i + 1

                End If
            Next

            'no more linefeeds found, remove processed bytes from buffer
            If start = buffer.Length Then
                'whole buffer has been processed, so nothing to keep 
                Me.buffer = Nothing

            ElseIf until > 0 Then
                'the buffer was partially processed, keep residu 
                Dim nxtbuffer(buffer.Length - start - 1) As Byte
                For i As Integer = 0 To nxtbuffer.Length - 1
                    nxtbuffer(i) = buffer(start + i)
                Next

                Me.buffer = nxtbuffer

            End If
        End If

        Return messages

    End Function

End Class
