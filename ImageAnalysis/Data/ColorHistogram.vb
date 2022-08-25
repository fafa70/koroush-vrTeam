Imports System.IO

Imports UvARescue.Agent

Public Class ColorHistogram

#Region "constructor"

    Public Sub New()
        Me.New(8) 'default binsize
    End Sub

    Public Sub New(ByVal binSize As Integer)
        Me._totalCount = 0
        Me._binSize = binSize
        Me._numOfBins = CInt(256 / binSize)
        ReDim _bins(CInt(Me._numOfBins ^ 3))
        ReDim _normalizedBins(CInt(Me._numOfBins ^ 3))
    End Sub

    Public Sub New(ByVal camDat As CameraData)
        Me.New()
        Me.AddCameraData(camDat)
    End Sub

    Public Sub New(ByVal camDat As CameraData, ByVal binSize As Integer)
        Me.New(binSize)
        Me.AddCameraData(camDat)
    End Sub

#End Region

#Region "properties"

    Dim _numOfBins As Integer
    Public ReadOnly Property NumOfBins() As Integer
        Get
            Return _numOfBins
        End Get
    End Property

    Dim _binSize As Integer
    Public ReadOnly Property BinSize() As Integer
        Get
            Return _binSize
        End Get
    End Property

    Dim _totalCount As UInt64
    Public ReadOnly Property totalCount() As UInt64
        Get
            Return (_totalCount)
        End Get
    End Property

    Dim _bins As Integer()
    Public ReadOnly Property Bins() As Integer()
        Get
            Return _bins
        End Get
    End Property

    Dim _normalizedBins As Double()
    Public ReadOnly Property NormalizedBins() As Double()
        Get
            Return Me._normalizedBins
        End Get
    End Property

#End Region

#Region "AddCameraData"

    Public Function AddCameraData(ByVal camDat As CameraData) As Boolean

        Dim success As Boolean = False

        Dim R As Byte
        Dim G As Byte
        Dim B As Byte

        For index As Integer = 0 To camDat.RawData.Length - 3 Step 3
            R = camDat.RawData(index)
            G = camDat.RawData(index + 1)
            B = camDat.RawData(index + 2)

            Dim i As Integer = RGBToBinIndex(R, G, B)
            _bins(i) += 1

            Me._totalCount += CULng(1)
        Next

        ReDim Me._normalizedBins(_bins.Length)
        For i As Integer = 0 To _bins.Length - 1
            Me._normalizedBins(i) = _bins(i) / totalCount
        Next

        Return success

    End Function

#End Region

#Region "LoadFromFile"

    Public Sub LoadFromFile(ByVal filename As String, ByVal noBlack As Boolean)

        Try
            FileOpen(1, filename, OpenMode.Binary)
            FileGet(1, Me._bins(0))
            FileClose(1)

        Catch ex As Exception
            Console.WriteLine("[ColorHistogram] Error while loading from file '" + filename + "'.")
            Console.WriteLine(ex.Message & vbNewLine & ex.StackTrace)
            Return

        End Try
        

        Me._numOfBins = CInt(_bins.Length ^ (1 / 3))
        Me._binSize = CInt(256 / Me.NumOfBins)

        If noBlack Then
            Me._bins(0) = 0
        End If


        For i As Integer = 0 To Me._bins.Length - 1
            Me._totalCount += CULng(Me._bins(i))
        Next

        ReDim Me._normalizedBins(_bins.Length)
        For i As Integer = 0 To _bins.Length - 1
            Me._normalizedBins(i) = _bins(i) / totalCount
        Next

    End Sub

#End Region

#Region "SaveToFile"

    Public Sub SaveToFile(ByVal filename As String)

        FileOpen(1, filename, OpenMode.Binary)
        FilePut(1, Me._bins)
        FileClose(1)

    End Sub

#End Region

#Region "ToString"

    Public Overrides Function ToString() As String

        Dim returnString As String = ""

        For i As Integer = 0 To _bins.Length - 1
            returnString = String.Concat(returnString, CStr(_bins(i)), " ")
        Next

        Return returnString

    End Function

#End Region

#Region "GetProbability"

    Public Function GetProbability(ByVal r As Byte, ByVal g As Byte, ByVal b As Byte) As Double
        Return Me.NormalizedBins(RGBToBinIndex(r, g, b))
    End Function

    Private Function RGBToBinIndex(ByVal R As Byte, ByVal G As Byte, ByVal B As Byte) As Integer
        Dim returnval As Integer = ((CInt(R) \ Me.BinSize) * NumOfBins * NumOfBins) + ((CInt(G) \ Me.BinSize) * NumOfBins) + (CInt(B) \ Me.BinSize)
        Dim testval As Integer = (CInt(R) \ Me.BinSize)
        If returnval > _bins.Length Then
            Return _bins.Length
        End If
        Return returnval
    End Function
#End Region

End Class
