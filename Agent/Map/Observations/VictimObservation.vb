Imports System.Drawing
Imports System.Threading
Imports System.Math

Imports UvARescue.Math


Public Class VictimObservation

#Region " Constructor "

    Public Sub New(ByVal manifold As Manifold, ByVal victimID As String, ByVal method As String)

        If IsNothing(manifold) Then Throw New ArgumentNullException("manifold")
        Me._Manifold = manifold

        Me._VictimID = victimID
        Me._Parts = New Dictionary(Of String, Integer)

        Me._AverageX = 0
        Me._AverageY = 0
        Me._NumObservations = 0

        Me._Picture = Nothing
        Me._SkinPercentage = 0

        Me._Method = method

        Me._logEntries = New List(Of String)

    End Sub

#End Region

#Region " Properties "

    Private _Manifold As Manifold
    Public ReadOnly Property Manifold() As Manifold
        Get
            Return Me._Manifold
        End Get
    End Property

    Private _VictimID As String
    Public ReadOnly Property VictimID() As String
        Get
            Return Me._VictimID
        End Get
    End Property

    Private _Parts As Dictionary(Of String, Integer)
    Public ReadOnly Property PartNames() As IEnumerable(Of String)
        Get
            Return Me._Parts.Keys
        End Get
    End Property
    Public ReadOnly Property PartCount(ByVal partName As String) As Integer
        Get
            If Me._Parts.ContainsKey(partName) Then
                Return Me._Parts(partName)
            Else
                Return 0
            End If
        End Get
    End Property


    Private _AverageX As Double
    Public ReadOnly Property AverageX() As Double
        Get
            Return Me._AverageX
        End Get
    End Property

    Private _AverageY As Double
    Public ReadOnly Property AverageY() As Double
        Get
            Return Me._AverageY
        End Get
    End Property

    Private _NumObservations As Integer
    Public ReadOnly Property NumObservations() As Integer
        Get
            Return Me._NumObservations
        End Get
    End Property

    Private _Picture As Bitmap
    Public ReadOnly Property Picture() As Bitmap
        Get
            Return Me._Picture
        End Get
    End Property

    Private _SkinPercentage As Double
    Public ReadOnly Property SkinPercentage() As Double
        Get
            Return Me._SkinPercentage
        End Get
    End Property

    Private _Method As String
    Public Property Method() As String
        Get
            Return Me._Method
        End Get
        Set(ByVal value As String)
            Me._Method = value
        End Set
    End Property

    Private _logEntries As List(Of String)
    Public ReadOnly Property LogEntries() As List(Of String)
        Get
            Return Me._logEntries
        End Get
    End Property
    Public Sub AddLogEntry(ByVal entry As String)
        Me._logEntries.Add(entry)
    End Sub

#End Region

#Region " Update Observation "

    Public Sub AddVictimPart(ByVal part As VictimData.VictimPart, ByVal count As Integer, ByVal x As Double, ByVal y As Double)

        'add to parts dictionary
        If Me._Parts.ContainsKey(part.PartName) Then
            Me._Parts(part.PartName) = Max(Me._Parts(part.PartName), count)
        Else
            Me._Parts.Add(part.PartName, count)
        End If

        'update averages
        If Me._NumObservations = 0 Then
            'shortcut
            Me._AverageX = x
            Me._AverageY = y
        Else
            Me._AverageX = ((Me._NumObservations * Me._AverageX) + x) / (Me._NumObservations + 1)
            Me._AverageY = ((Me._NumObservations * Me._AverageY) + y) / (Me._NumObservations + 1)
        End If

        'increment counter
        Me._NumObservations += 1

        Me.Manifold.NotifyVictimUpdated(Me)

    End Sub

    Public Sub RemoveVictimPart(ByVal part As VictimData.VictimPart)

        'update averages
        If Me._NumObservations <= 0 Then
            'should be impossible!
            Exit Sub
        ElseIf Me._NumObservations = 1 Then
            'avoid divide-by-zero
            Me._AverageX = 0
            Me._AverageY = 0
        Else
            Me._AverageX = ((Me._NumObservations * Me._AverageX) - part.X) / (Me._NumObservations - 1)
            Me._AverageY = ((Me._NumObservations * Me._AverageY) - part.Y) / (Me._NumObservations - 1)
        End If

        'decrement counter
        Me._NumObservations -= 1

        Me.Manifold.NotifyVictimUpdated(Me)

    End Sub

    Public Function UpdatePicture(ByVal picture As Bitmap, ByVal skinPercentage As Double) As Boolean

        If IsNothing(Me._Picture) OrElse skinPercentage > Me._SkinPercentage Then

            Me._SkinPercentage = skinPercentage
            Me._Picture = picture

            If Me._Method = "VSENSOR" Then
                Me._Method = "VSENSOR_VISUAL" ' set method to proper value
            End If

            Me.Manifold.NotifyVictimUpdated(Me)

            Return True

        End If

        'no changes
        Return False

    End Function

#End Region

#Region " Locking "

    'reader-writer locks allow either a single writer (x)or multiple simultaneous
    'readers. (See manifold class)

    Private lock As New ReaderWriterLock

    Public Sub AcquireReaderLock()
        Me.lock.AcquireReaderLock(Timeout.Infinite)
        'Console.WriteLine(Thread.CurrentThread.Name & " acquired Read lock")
    End Sub
    Public Sub ReleaseReaderLock()
        'Console.WriteLine(Thread.CurrentThread.Name & " released Read lock")
        Me.lock.ReleaseReaderLock()
    End Sub

    Public Sub AcquireWriterLock()
        Me.lock.AcquireWriterLock(Timeout.Infinite)
        'Console.WriteLine(Thread.CurrentThread.Name & " acquired Write lock")
    End Sub
    Public Sub ReleaseWriterLock()
        'Console.WriteLine(Thread.CurrentThread.Name & " released Write lock")
        Me.lock.ReleaseWriterLock()
    End Sub

    Public Sub ReleaseLock()
        Me.lock.ReleaseLock()
    End Sub

#End Region

End Class
