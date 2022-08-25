Imports System.IO

Public Class GeoReference

    Public Sub New()
        Me.OffsetX = 0
        Me.OffsetY = 0
        Me.ScaleX = 0
        Me.ScaleY = 0
        Me.ShearX = 0
        Me.ShearY = 0
    End Sub

    Public Sub SaveWorldFile(ByVal filename As String)
        Using writer As New StreamWriter(File.Open(filename, FileMode.Create, FileAccess.Write))

            'WorldFile spec from wikipedia (http://en.wikipedia.org/wiki/World_file)
            'map unit is in meter, the default for WGS84 - Orthographic projections

            '* Line 1: A, pixel size in the x-direction in map units/pixel
            writer.WriteLine(Me.ScaleX)

            '* Line 2: D: rotation about y-axis
            writer.WriteLine(Me.ShearY) 'usually 0 

            '* Line 3: B: rotation about x-axis
            writer.WriteLine(Me.ShearX) 'usually 0

            '* Line 4: E: pixel size in the y-direction in map units, almost always negative[3]
            writer.WriteLine(Me.ScaleY) 'usually negative value of A (=-scalex)

            '* Line 5: C: x-coordinate of the center of the upper left pixel
            writer.WriteLine(Me.OffsetX)

            '* Line 6: F: y-coordinate of the center of the upper left pixel
            writer.WriteLine(Me.OffsetY)


            writer.Flush()
            writer.Close()

        End Using
    End Sub

    Public OffsetX As Double
    Public OffsetY As Double
    Public ShearX As Double
    Public ShearY As Double
    Public ScaleX As Double
    Public ScaleY As Double

End Class
