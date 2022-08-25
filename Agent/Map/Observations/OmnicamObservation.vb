Imports System.Drawing
Imports System.Threading

Imports UvARescue.Agent
Imports UvARescue.Math

<Serializable()> _
Public Class OmnicamObservation

#Region " Constructor "

    Public Sub New(ByVal timestamp As DateTime, ByVal bitmap As Bitmap, ByVal birdview As Bitmap)
        Me._TimeStamp = timestamp
        Me._Bitmap = bitmap
        Me._Birdview = birdview
    End Sub

#End Region

#Region " Properties "

    Private _Bitmap As Bitmap
    Public ReadOnly Property Bitmap() As Bitmap
        Get
            Return _Bitmap
        End Get
    End Property

    Private _Birdview As Bitmap
    Public ReadOnly Property BirdView() As Bitmap
        Get
            Return _Birdview
        End Get
    End Property

    Private _TimeStamp As DateTime
    Public ReadOnly Property TimeStamp() As DateTime
        Get
            Return _TimeStamp
        End Get
    End Property

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
