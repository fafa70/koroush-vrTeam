Imports System.Text
Imports System.Drawing


Public Class TcpCameraConnection
    Inherits TcpConnection

    Public Sub SendAcknowledgement()
        Me.Send("OK") '  & Environment.NewLine()
    End Sub


    Private imgdata() As Byte = Nothing

    Public Function ReceiveImageData(ByVal maxLength As Integer) As Byte()

        If Not Me.IsConnected Then Throw New InvalidOperationException("You should connect first")

        'receive any inbound messages
        If stream.DataAvailable Then

            'load data into buffer
            Dim buffer(maxLength - 1) As Byte

            'length will tell how many bytes were actually retrieved
            Dim length As Integer = stream.Read(buffer, 0, buffer.Length)

            Dim offset As Integer
            If IsNothing(Me.imgdata) Then
                'no data available from previous read, start new array
                offset = 0
                ReDim imgdata(length - 1)
            Else
                'expand previously retrieved data and append buffer into it
                offset = imgdata.Length
                ReDim Preserve imgdata(imgdata.Length + length - 1)
            End If

            'copy buffer into imgdata
            For i As Integer = 0 To length - 1
                imgdata(offset + i) = buffer(i)

            Next

            'at this point imgdata may contain a complete or a partial image
            'the first 5 bytes store the type (1 byte) and length (4 bytes)
            'so we need at least that to proceed
            If imgdata.Length > 5 Then

                'read the image length from bytes 2 to 5
                Dim imglength As Integer = CInt(imgdata(1) * 2 ^ 24) + CInt(imgdata(2) * 2 ^ 16) + CInt(imgdata(3) * 2 ^ 8) + CInt(imgdata(4) * 2 ^ 0)

                'check if we have this image in full in imgdata
                If imgdata.Length >= imglength + 5 Then

                    'ok we have it, so extract the relevant part
                    Console.WriteLine("full image")
                    Dim curimg() As Byte = Nothing
                    Dim nxtimg() As Byte = Nothing
                    '=

                    If imgdata.Length = imglength + 5 Then
                        curimg = imgdata
                        Console.WriteLine("equal")
                        nxtimg = Nothing

                    ElseIf imgdata.Length > imglength + 5 Then
                        ReDim curimg(imglength + 5 - 1)
                        Console.WriteLine("bigger")
                        For i As Integer = 0 To curimg.Length - 1
                            curimg(i) = imgdata(i)
                        Next

                        ReDim nxtimg(imgdata.Length - curimg.Length - 1)
                        For i As Integer = 0 To nxtimg.Length - 1
                            nxtimg(i) = imgdata(curimg.Length + i)
                        Next

                    End If

                    Me.imgdata = nxtimg




                    Return curimg

                End If

            End If
        End If

        Return Nothing

    End Function


End Class
