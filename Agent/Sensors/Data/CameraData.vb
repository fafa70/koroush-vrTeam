Imports System.Drawing
'Imports System.Drawing.Drawing2D
Imports System.Drawing.Imaging
Imports System.Runtime.InteropServices
Imports System.IO
Imports MathNet.Numerics 'Defines Complex
Imports MathNet.SignalProcessing.DataSources


Public Enum CameraImageType As Byte
    Raw
    Jpeg
End Enum

Public Class CameraData
    Implements ISensorData

    Private PostponeConvertion As Boolean = True



#Region " Constructor "

    Public Sub New()

    End Sub

#End Region

#Region " Properties "

    Private _CameraImageType As CameraImageType
    Public ReadOnly Property CameraImageType() As Byte
        Get
            Return _CameraImageType
        End Get
    End Property

    Private _Width As Integer = 0
    Public ReadOnly Property Width() As Integer
        Get
            Return _Width
        End Get
    End Property

    Private _Height As Integer = 0
    Public ReadOnly Property Height() As Integer
        Get
            Return _Height
        End Get
    End Property

    Private window_x As Integer = 0
    Public ReadOnly Property _window_x() As Integer
        Get
            Return window_x
        End Get
    End Property

    Private window_y As Integer = 0
    Public ReadOnly Property _window_y() As Integer
        Get
            Return window_y
        End Get
    End Property

    Private _RawData As Byte() = Nothing
    Public ReadOnly Property RawData() As Byte()
        Get
            If Not IsNothing(Me._Bitmap) AndAlso PostponeConvertion = True Then
                Me.ConvertFromBitmap()
            End If
            Return _RawData
        End Get
    End Property

    Private _Bitmap As Bitmap = Nothing
    Public Property Bitmap() As Bitmap
        Get
            Return Me._Bitmap
        End Get
        Set(ByVal value As Bitmap)
            Me._Bitmap = value
        End Set
    End Property

    Private _Sequence As Integer = 0
    Public ReadOnly Property Sequence() As Integer
        Get
            Return _Sequence
        End Get
    End Property


#End Region

#Region " Load "

    Protected Mutex As New Object


    Public Sub Load(ByVal bytes As Byte(), ByVal viewNumber As Integer, ByVal multiviewMode As Byte, ByVal sequenceNumber As Integer)

        Me._Sequence = sequenceNumber

        If bytes(0) = 0 Then
            Me._CameraImageType = UvARescue.Agent.CameraImageType.Raw
        Else
            Me._CameraImageType = UvARescue.Agent.CameraImageType.Jpeg
        End If

        ' convert 4 bytes to Integer
        Dim length As Integer = CInt(bytes(1) * 2 ^ 24) + CInt(bytes(2) * 2 ^ 16) + CInt(bytes(3) * 2 ^ 8) + CInt(bytes(4) * 2 ^ 0)

        Select Case Me._CameraImageType

            Case UvARescue.Agent.CameraImageType.Raw
                Console.WriteLine("raw is correct")
                SyncLock Me.Mutex

                    Me._Width = CInt(bytes(5) * 2 ^ 8) + CInt(bytes(6))
                    Me._Height = CInt(bytes(7) * 2 ^ 8) + CInt(bytes(8)) 

        

                    'copy raw data RGB(1 byte + 1 byte + 1 byte )
                    ReDim Me._RawData(length - 5)
                    Dim raw(length - 5) As Byte 'A clone which can be discarded

                    For i As Integer = 0 To Me._RawData.Length - 1
                        Me._RawData(i) = bytes(i + 9)
                        raw(i) = bytes(i + 9)
                    Next

                    If Not IsNothing(Me._Bitmap) Then
                        Me._Bitmap.Dispose() 'clean up, to prevent memory problems
                        Me._Bitmap = Nothing
                    End If

                    If multiviewMode < 2 AndAlso True Then 'SwitchOff for UsarSkinAgent

                        Dim bmp As Bitmap = Nothing

                        If IsNothing(Me._Bitmap) Then

                            bmp = New Bitmap(Me.Width, Me.Height, PixelFormat.Format24bppRgb)
                        Else
                            bmp = Me._Bitmap
                        End If

                        ' Lock the bitmap's bits.  
                        Dim rect As New Rectangle(0, 0, bmp.Width, bmp.Height)
                        Dim bmpData As System.Drawing.Imaging.BitmapData = bmp.LockBits(rect, _
                            Drawing.Imaging.ImageLockMode.ReadWrite, bmp.PixelFormat)




                        ' Get the address of the first line.
                        Dim ptr As IntPtr = bmpData.Scan0

                        ' Declare an array to hold the bytes of the bitmap.
                        ' This code is specific to a bitmap with 24 bits per pixels.

                        ' Copy the RGB values back to the bitmap
                        System.Runtime.InteropServices.Marshal.Copy(raw, 0, ptr, raw.Length)

                        ' Unlock the bits.
                        bmp.UnlockBits(bmpData)


                        Me._Bitmap = DirectCast(bmp.Clone, Bitmap)
                        bmp.Dispose()

                        'Dim imLength As Integer = CInt(Me.RawData.Length / 3)
                        'Dim red, green, blue As Byte
                        'Dim r(imLength), g(imLength), b(imLength) As Complex
                        'Dim cp As ComplexPart = ComplexPart.Real
                        'Dim size As System.Drawing.Size = New Size(Me.Width, Me.Height) 'If camdata = raw, no bitmap exist

                        'For index As Integer = 0 To imLength - 1

                        '    red = Me.RawData(index * 3)
                        '    green = Me.RawData(index * 3 + 1)
                        '    blue = Me.RawData(index * 3 + 2)

                        '    r(index).Real = red / Byte.MaxValue
                        '    g(index).Real = green / Byte.MaxValue
                        '    b(index).Real = blue / Byte.MaxValue

                        'Next

                        'Me._Bitmap = BitmapConverter.WriteChannel(size, r, cp, g, cp, b, cp) 'MainForm hangs

                    Else
                        'get Multiview subview 
                        Me._Bitmap = GetSubView(Me._Bitmap, viewNumber, multiviewMode)
                        'Me._RawData = GetSubView(Me._RawData, viewNumber) Not implemented yet

                    End If


                End SyncLock

            Case Else
                'jpeg
                SyncLock Me.Mutex
                    Console.WriteLine("jpeg is correct")
                    If Not IsNothing(Me._Bitmap) Then
                        Me._Bitmap.Dispose() 'clean up, to prevent memory problems
                    End If

                    Dim jpeg(length - 1) As Byte
                    For i As Integer = 0 To jpeg.Length - 1
                        jpeg(i) = bytes(i + 5)
                    Next

                    'if Multiview is not in use
                    'If multiviewMode < 2 Then
                    'Me._Bitmap = DirectCast(New Bitmap(New System.IO.MemoryStream(jpeg)).Clone, Bitmap)

                    'Else
                    'get Multiview subview 
                    Console.WriteLine("jpeg is correct")
                    Dim subView As Bitmap
                    subView = DirectCast(New Bitmap(New System.IO.MemoryStream(jpeg)).Clone, Bitmap)

                    Me._Bitmap = GetSubView(subView, viewNumber, multiviewMode)


                    'End If

                    Me._Width = Me._Bitmap.Width
                    Me._Height = Me._Bitmap.Height

                End SyncLock

        End Select

    End Sub

    'subview getter function for Raw data
    Private Function GetSubView(ByVal rawData As Byte(), ByVal spawnNumber As Integer) As Byte()

        'TODO: Raw subview getter function should be implemented here

        Dim subViewRaw As Byte() = Nothing
        Return subViewRaw
    End Function


    'subview getter function for Jpeg data
    Private Function GetSubView(ByVal bMap As Bitmap, ByVal spawnNumber As Integer, ByVal multiviewMode As Byte) As Bitmap

        Try
            Dim fullView As Bitmap = New Bitmap(bMap)

            'saving cameradata to file
            'Dim pathName, imageFile As String
            'pathName = Path.Combine(My.Application.Info.DirectoryPath, "Images")
            'imageFile = Path.Combine(pathName, String.Concat(CStr(spawnNumber), "_FULL.jpg"))
            'Dim fileInfo As New FileInfo(imageFile)
            'If Not fileInfo.Directory.Exists Then
            '   FileInfo.Directory.Create()
            'End If
            'fullView.Save(imageFile, System.Drawing.Imaging.ImageFormat.Jpeg)

            Dim row, column, width, height As Integer

            'row = (spawnNumber - 1) Mod multiviewMode
            'column = (spawnNumber - 1) \ multiviewMode

            row = (spawnNumber) Mod multiviewMode
            column = (spawnNumber) \ multiviewMode

            If row < 0 Then
                Console.WriteLine("[GetSubView]: Warning row < 0, set to 0")
                row = 0
            End If
            If column < 0 Then
                Console.WriteLine("[GetSubView]: Warning column < 0, set to 0")
                column = 0
            End If

            'width = fullView.Width \ multiviewMode
            'height = fullView.Height \ multiviewMode - 50
            width = 320
            height = 240
            'Console.WriteLine("full: {0},{1}, cropped: {2},{3} and sequence : {4}", fullView.Width, fullView.Height, width, height, spawnNumber)

            Dim subView As Bitmap = Nothing

            
            
            If (spawnNumber = 1) Then
                Dim cloneRect As New Drawing.Rectangle(0, 0, width, height)

                Dim format As Imaging.PixelFormat = fullView.PixelFormat
                subView = fullView.Clone(cloneRect, format)


            ElseIf (spawnNumber = 2) Then
                Dim cloneRect As New Drawing.Rectangle(width, 0, 2 * width - 320, height)
                Console.WriteLine("{0} ,{1} ,{2}.", spawnNumber, 2 * width - 320, height)
                Dim format As Imaging.PixelFormat = fullView.PixelFormat
                subView = fullView.Clone(cloneRect, format)


            ElseIf (spawnNumber = 3) Then
                Dim cloneRect As New Drawing.Rectangle(0, height, width, 2 * height - 240)

                Dim format As Imaging.PixelFormat = fullView.PixelFormat
                subView = fullView.Clone(cloneRect, format)

            ElseIf (spawnNumber = 4) Then
                Dim cloneRect As New Drawing.Rectangle(width, height, width, height)

                Dim format As Imaging.PixelFormat = fullView.PixelFormat
                subView = fullView.Clone(cloneRect, format)


            End If

            'saving cameradata to file
            'imageFile = Path.Combine(pathName, String.Concat(CStr(spawnNumber), "_SUBVIEW.jpg"))
            'subView.Save(imageFile, System.Drawing.Imaging.ImageFormat.Jpeg)

            Return subView
            Console.WriteLine("fafa")
        Catch ex As Exception
            Console.WriteLine("Error occurred while trying to create a subview.")
            Console.WriteLine(ex.Message & vbNewLine & ex.StackTrace)
            Return bMap

        End Try

    End Function

    Public Sub Load(ByVal msg As System.Collections.Specialized.StringDictionary) Implements ISensorData.Load
    End Sub

    Private SlowConvertMethod As Boolean = False

    Private Sub ConvertFromBitmap()

        'copy data from Me._Bitmap to Me._RawData

        ReDim Me._RawData(Me._Width * Me._Height * 3)

        If SlowConvertMethod = True AndAlso Me._Bitmap.PixelFormat = PixelFormat.Format24bppRgb Then

            Dim bits As BitmapData = Me._Bitmap.LockBits(New Rectangle(0, 0, Me._Bitmap.Width, Me._Bitmap.Height), ImageLockMode.ReadOnly, Me._Bitmap.PixelFormat)

            'From example http://www.bobpowell.net/mapfill.aspx

            Dim offset As Integer = 0
            Dim index As Integer = 0
            For x As Integer = 0 To Me.Width - 1
                For y As Integer = 0 To Me.Height - 1

                    offset = y * bits.Stride + (3 * x) '3 colors x 8 bits = 24bbp
                    _RawData(index) = Marshal.ReadByte(bits.Scan0, offset) 'red
                    _RawData(index + 1) = Marshal.ReadByte(bits.Scan0, offset + 1) 'green
                    _RawData(index + 2) = Marshal.ReadByte(bits.Scan0, offset + 2) 'blue

                    index += 3
                Next 'y
            Next 'x

            Me._Bitmap.UnlockBits(bits)

        ElseIf SlowConvertMethod = True AndAlso Me._Bitmap.PixelFormat = PixelFormat.Format32bppArgb Then

            Dim bits As BitmapData = Me._Bitmap.LockBits(New Rectangle(0, 0, Me._Bitmap.Width, Me._Bitmap.Height), ImageLockMode.ReadOnly, Me._Bitmap.PixelFormat)

            'From example http://www.bobpowell.net/mapfill.aspx

            Dim offset As Integer = 0
            Dim index As Integer = 0
            For x As Integer = 0 To Me.Width - 1
                For y As Integer = 0 To Me.Height - 1

                    offset = y * bits.Stride + (4 * x) 'skip alpha

                    _RawData(index) = Marshal.ReadByte(bits.Scan0, offset) 'red
                    _RawData(index + 1) = Marshal.ReadByte(bits.Scan0, offset + 1) 'green
                    _RawData(index + 2) = Marshal.ReadByte(bits.Scan0, offset + 2) 'blue

                    index += 3
                Next 'y
            Next 'x

            Me._Bitmap.UnlockBits(bits)

        Else
            'Fast, but memory problems for larger resolutions

            Dim R(Me._Width * Me._Height) As Complex
            Dim G(Me._Width * Me._Height) As Complex
            Dim B(Me._Width * Me._Height) As Complex

            Try
                MathNet.SignalProcessing.DataSources.BitmapConverter.ReadChannel(Me._Bitmap, R, G, B)
            Catch ex As Exception
                Console.WriteLine("Error occurred while trying to read RGB channels.")
                Console.WriteLine(ex.Message & vbNewLine & ex.StackTrace)
            End Try

            'ReDim Me._RawData(R.Length * 3) ' already done above

            Dim index As Integer = 0

            Try
                For i As Integer = 0 To (R.Length) - 1
                    _RawData(index) = CByte(R(i).Real * 256)
                    _RawData(index + 1) = CByte(G(i).Real * 256)
                    _RawData(index + 2) = CByte(B(i).Real * 256)
                    index += 3
                Next
            Catch ex As Exception
                Console.WriteLine("Error converting RGB data to RAW image")
                Console.WriteLine(ex.Message & vbNewLine & ex.StackTrace)
            End Try


        End If
    End Sub

#End Region

End Class
