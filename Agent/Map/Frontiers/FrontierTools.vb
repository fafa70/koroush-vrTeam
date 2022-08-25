Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports System.Drawing.Imaging
Imports System.Math
Imports System.Runtime.InteropServices
Imports System.Windows.Forms

Imports AForge
Imports AForge.Math
Imports AForge.ImaginG
Imports AForge.Imaging.Filters



Public Class FrontierTools

#Region " Extracting Frontier Borders and Regions "

    Public Function ExtractFrontierBorders(ByVal image As ManifoldImage) As Bitmap

        'get walls and safespace
        Dim walls As Bitmap = Me.ExtractWalls(image)
        Dim safearea As Bitmap = Me.ExtractSafeArea(image)

        'construct empty frontier image
        Dim rect As New Rectangle(0, 0, image.ImageWidth, image.ImageHeight)
        Dim frontiers As New Bitmap(rect.Width, rect.Height, PixelFormat.Format8bppIndexed)

        'get handles to bitmap data
        Dim wbits As BitmapData = walls.LockBits(rect, ImageLockMode.ReadOnly, PixelFormat.Format8bppIndexed)
        Dim sbits As BitmapData = safearea.LockBits(rect, ImageLockMode.ReadOnly, PixelFormat.Format8bppIndexed)
        Dim fbits As BitmapData = frontiers.LockBits(rect, ImageLockMode.WriteOnly, PixelFormat.Format8bppIndexed)

        Dim wbytes(wbits.Width * wbits.Height - 1) As Byte
        Dim sbytes(sbits.Width * sbits.Height - 1) As Byte
        Dim fbytes(fbits.Width * fbits.Height - 1) As Byte


        Marshal.Copy(wbits.Scan0, wbytes, 0, wbytes.Length)
        Marshal.Copy(sbits.Scan0, sbytes, 0, sbytes.Length)
        Marshal.Copy(fbits.Scan0, fbytes, 0, fbytes.Length)


        Dim x As Integer, y As Integer
        Dim idxi As Integer
        For idx As Integer = 0 To fbytes.Length - 1

            y = CInt(Floor(idx / fbits.Width))
            x = idx - y * fbits.Width

            Dim isWall As Boolean = wbytes(idx) > 0

            Dim hasWall As Boolean = False
            Dim hasSafe As Boolean = False
            Dim hasUnsafe As Boolean = False

            If isWall Then
                'quickly skip walls
                Continue For

            Else
                'inspect neighbourhood
                For yi As Integer = Max(0, y - 1) To Min(frontiers.Height - 1, y + 1)
                    For xi As Integer = Max(0, x - 1) To Min(frontiers.Width - 1, x + 1)

                        idxi = rect.Width * yi + xi

                        hasWall = hasWall OrElse wbytes(idxi) > 0
                        hasSafe = hasSafe OrElse sbytes(idxi) > 0
                        hasUnsafe = hasUnsafe OrElse sbytes(idxi) = 0

                    Next
                Next

            End If


            If isWall Then
                'black wall cells 
                fbytes(idx) = Byte.MinValue

            ElseIf Not hasWall AndAlso hasSafe AndAlso hasUnsafe Then
                'white frontier cells
                fbytes(idx) = Byte.MaxValue

            ElseIf hasSafe AndAlso Not hasUnsafe Then
                'gray safe cells
                fbytes(idx) = CInt(Byte.MaxValue / 2)

            Else
                'default to black for all other cells
                fbytes(idx) = Byte.MinValue

            End If

        Next

        Runtime.InteropServices.Marshal.Copy(fbytes, 0, fbits.Scan0, fbytes.Length)

        walls.UnlockBits(wbits)
        safearea.UnlockBits(sbits)
        frontiers.UnlockBits(fbits)

        Return frontiers

    End Function

    Public Function ExtractFrontierRegions(ByVal image As ManifoldImage) As Bitmap
        Using freearea As Bitmap = Me.ExtractFreeArea(image)
			Using walls As Bitmap = Me.ExtractWalls(image)
				Using walled As Bitmap = New Subtract(walls).Apply(freearea)

					Dim mainarea As Bitmap = Nothing
					If freearea.Width > 1 AndAlso freearea.Height > 1 Then
						Dim blobCounter As New BlobCounter(walled)
						Dim blobs() As Blob = blobCounter.GetObjects(walled)

						Dim mainsize As Double = Double.MinValue
						Dim mainblob As Blob = Nothing

						Dim size As Double
						For Each blob As Blob In blobs
							size = blob.Image.Width * blob.Image.Height
							If size > mainsize Then
								mainsize = size
								mainblob = blob
							End If
						Next

						If Not IsNothing(mainblob) Then
							mainarea = New Bitmap(walled.Width, walled.Height, walled.PixelFormat)
							mainarea = New Merge(mainblob.Image, mainblob.Location).Apply(mainarea)
						End If

						'cleanup
						For Each blob As Blob In blobs
							blob.Image.Dispose()
						Next

					End If

					If IsNothing(mainarea) Then
						mainarea = walled.Clone(New Rectangle(0, 0, walled.Width, walled.Height), walled.PixelFormat)
					End If

					Using safearea As Bitmap = Me.ExtractSafeArea(image)
						Return New Subtract(safearea).Apply(mainarea)
					End Using

					mainarea.Dispose()

				End Using
			End Using
		End Using
    End Function

    Public Function ExtractFrontierRegionsWithInfo(ByVal image As ManifoldImage) As Bitmap
        Using regions As Bitmap = Me.ExtractFrontierRegions(image)

            Dim infos() As FrontierInfo = Me.ExtractFrontierInfo(regions)
            Dim img As Bitmap = New ConnectedComponentsLabeling().Apply(regions)
            Dim ico As Bitmap = My.Resources.Frontier

            Using gfx As Graphics = Graphics.FromImage(img)

                Dim x As Integer, y As Integer
                For Each info As FrontierInfo In infos

                    If info.Area < 1 Then
                        'do not render frontiers smaller than 1 m^2
                        Continue For
                    End If

                    x = CInt(info.Center.X)
                    y = CInt(info.Center.Y)

                    gfx.DrawImageUnscaled(ico, x - 3, y - ico.Height + 3)
                    gfx.DrawString(String.Format("{0:f2} m^2", info.Area), SystemFonts.DefaultFont, Brushes.CornflowerBlue, x, y)

                Next

            End Using

            Return img

        End Using
    End Function

    Public Function ExtractFrontierRegionsWithPathPlans(ByVal image As ManifoldImage, ByVal pose As Point, ByVal useSafetyMap As Boolean) As Bitmap
        Using regions As Bitmap = Me.ExtractFrontierRegions(image)

            Dim infos() As FrontierInfo = Me.ExtractFrontierInfo(regions)

            Dim occupancy As Bitmap
            If useSafetyMap Then
                'based on safety map
                occupancy = Me.ExtractSafetyImage(image, image.ImageRect)
            Else
                occupancy = Me.ExtractGaussianImage(image, image.ImageRect)

                ''based on free space
                'Using freearea As Bitmap = Me.ExtractFreeArea(image)
                '    Using walls As Bitmap = Me.ExtractWalls(image)
                '        occupancy = New Subtract(walls).Apply(freearea)
                '    End Using
                'End Using
            End If

            Dim img As Bitmap = New ConnectedComponentsLabeling().Apply(regions)

            Dim ico As Bitmap = My.Resources.Frontier

            Using gfx As Graphics = Graphics.FromImage(img)

                Using occupancy

                    gfx.DrawImageUnscaled(occupancy, 0, 0)

                    For Each info As FrontierInfo In infos

                        If info.Area < 1 Then
                            'do not process frontiers smaller than 1 m^2
                            Continue For
                        End If

                        Dim x As Integer = CInt(info.Center.X)
                        Dim y As Integer = CInt(info.Center.Y)

                        Dim path() As Point = Me.ComputePathPlan(occupancy, New Point(x, y), pose, False, 80.0)

                        If Not IsNothing(path) Then

                            Using gpath As New GraphicsPath

                                For Each p As Point In path
                                    gpath.AddRectangle(New Rectangle(p.X, p.Y, 1, 1))
                                Next

                                gfx.FillPath(Brushes.Red, gpath)

                            End Using

                            gfx.DrawImageUnscaled(ico, x - 3, y - ico.Height + 3)
                            gfx.DrawString(String.Format("{0:f2} m^2", info.Area), SystemFonts.DefaultFont, Brushes.CornflowerBlue, x, y)

                        End If

                    Next

                End Using

            End Using

            Return img

        End Using

    End Function

	Public Function ExtractFrontierInfo(ByVal image As ManifoldImage) As FrontierInfo()
		Using regions As Bitmap = Me.ExtractFrontierRegions(image)
			Return Me.ExtractFrontierInfo(regions)
        End Using
	End Function

    Public Function ExtractFrontierInfo(ByVal regions As Bitmap) As FrontierInfo()

        Dim infos As New List(Of FrontierInfo)

        If regions.Width > 1 AndAlso regions.Height > 1 Then

            Dim counter As New BlobCounter(regions)
            Dim blobs() As Blob = counter.GetObjects(regions)

            Dim xCenter As Double, yCenter As Double, count As Integer
            For Each blob As Blob In blobs

                'compute true area in number of pixels
                count = 0
                xCenter = 0
                yCenter = 0

                Dim bbits As BitmapData = blob.Image.LockBits(New Rectangle(0, 0, blob.Image.Width, blob.Image.Height), ImageLockMode.ReadOnly, PixelFormat.Format8bppIndexed)
                For j As Integer = 0 To bbits.Height - 1
                    For i As Integer = 0 To bbits.Width - 1
                        If Marshal.ReadByte(bbits.Scan0, j * bbits.Stride + i) > 0 Then
                            count += 1
                            xCenter += i
                            yCenter += j
                        End If
                    Next
                Next
                blob.Image.UnlockBits(bbits)

                'compute averages
                xCenter = xCenter / count + blob.Location.X
                yCenter = yCenter / count + blob.Location.Y

                infos.Add(New FrontierInfo(count / 100, xCenter, yCenter))

            Next

            'cleanup
            For Each blob As Blob In blobs
                blob.Dispose()
            Next
            blobs = Nothing

        End If

        Return infos.ToArray

    End Function

#End Region

#Region " Path Planning and Navigation "

    Public Overridable Function ExtractSafetyImage(ByVal image As ManifoldImage, ByVal window As Rectangle) As Bitmap

        Dim safety As New Bitmap(image.ImageWidth, image.ImageHeight, PixelFormat.Format8bppIndexed)
        safety = New GrayscaleBT709().Apply(safety)

        Dim bits As BitmapData = safety.LockBits(New Rectangle(0, 0, safety.Width, safety.Height), ImageLockMode.ReadWrite, PixelFormat.Format8bppIndexed)

        'init wavefront 
        Dim wavefront As New List(Of Point)
        Dim x As Integer, y As Integer

        Using obstacles As Bitmap = Me.ExtractObstacles(image)
            Dim obits As BitmapData = obstacles.LockBits(New Rectangle(0, 0, obstacles.Width, obstacles.Height), ImageLockMode.ReadOnly, PixelFormat.Format8bppIndexed)
            Dim idx As Integer
            For y = window.Top To window.Bottom - 1
                For x = window.Left To window.Right - 1
                    idx = obits.Stride * y + x
                    If Marshal.ReadByte(obits.Scan0, idx) = 0 Then
                        'obstacle
                        wavefront.Add(New Point(x, y))
                    End If
                Next
            Next
            obstacles.UnlockBits(obits)
        End Using

        If Not window = image.ImageRect Then
            'window is smaller than full image, add virtual walls
            Dim p As Point
            For x = window.Left To window.Right - 1
                p = New Point(x, window.Top)
                If Not wavefront.Contains(p) Then wavefront.Add(p)
                p = New Point(x, window.Bottom - 1)
                If Not wavefront.Contains(p) Then wavefront.Add(p)
            Next
            For y = window.Top To window.Bottom - 1
                p = New Point(x, window.Left)
                If Not wavefront.Contains(p) Then wavefront.Add(p)
                p = New Point(x, window.Right - 1)
                If Not wavefront.Contains(p) Then wavefront.Add(p)
            Next
        End If


        'propagate wavefront 
        Using occupancy As Bitmap = Me.ExtractFreeArea(image)

            Dim obits As BitmapData = occupancy.LockBits(New Rectangle(0, 0, occupancy.Width, occupancy.Height), ImageLockMode.ReadOnly, PixelFormat.Format8bppIndexed)

            Dim history As New List(Of Point)
            Dim value As Byte = 0

            history.AddRange(wavefront)

            Dim p As Point, occ As Byte
            While Not wavefront.Count = 0

                Dim nextfront As New List(Of Point)

                For Each point As Point In wavefront

                    'process neighbours
                    For x = point.X - 1 To point.X + 1
                        For y = point.Y - 1 To point.Y + 1

                            p = New Point(x, y)
                            If history.Contains(p) Then
                                'skip points already processed
                                Continue For
                            ElseIf Not window.Contains(p) Then
                                'don't get beyond bounds
                                Continue For
                            End If

                            occ = Marshal.ReadByte(obits.Scan0, obits.Stride * p.Y + p.X)

                            If occ > 0 Then
                                'free area
                                Marshal.WriteByte(bits.Scan0, bits.Stride * p.Y + p.X, value)
                                nextfront.Add(p)
                                history.Add(p)
                            End If

                        Next
                    Next

                Next

                wavefront.Clear()
                wavefront = nextfront

                If value < Byte.MaxValue Then
                    value = CByte(Min(value + 10, Byte.MaxValue))
                End If

            End While

            'Dim scaling As Double = Byte.MaxValue / value
            'Dim val As Integer
            'For Each p In history
            '    idx = pbits.Stride * p.Y + p.X
            '    val = Marshal.ReadByte(pbits.Scan0, idx)
            '    val = Min(CInt(val * scaling), Byte.MaxValue)
            '    Marshal.WriteByte(pbits.Scan0, idx, CByte(val))
            'Next

            occupancy.UnlockBits(obits)

        End Using

        safety.UnlockBits(bits)

        Return safety

    End Function

    Public Overridable Function ExtractGaussianImage(ByVal image As ManifoldImage, ByVal window As Rectangle) As Bitmap
        Using freearea As Bitmap = Me.ExtractFreeArea(image)
            Using walls As Bitmap = Me.ExtractWalls(image)
                Using occupancy As Bitmap = New Subtract(walls).Apply(freearea)
                    Return Me.ExtractGaussianImage(occupancy, window)
                End Using
            End Using
        End Using
    End Function

    Public Overridable Function ExtractGaussianImage(ByVal occupancy As Bitmap, ByVal window As Rectangle) As Bitmap
        Using selection As Bitmap = occupancy.Clone(window, occupancy.PixelFormat)
            Using blurred As Bitmap = New GaussianBlur(1.4, CInt(1.7 * UvARescue.Tools.Constants.MAP_RESOLUTION)).Apply(selection)
                Return New Intersect(blurred, window.Location).Apply(occupancy)
            End Using
        End Using
    End Function




    Public Function ComputePathPlan(ByVal occupancy As Bitmap, ByVal start As Point, ByVal target As Point, ByVal includeManhattanDistanceToStart As Boolean, ByVal max_dist As Double) As Point()

        Dim bits As BitmapData = occupancy.LockBits(New Rectangle(0, 0, occupancy.Width, occupancy.Height), ImageLockMode.ReadOnly, occupancy.PixelFormat)

        Dim history As New List(Of Point)(New Point() {start})
        Dim wavefront As New List(Of PathInfo)( _
         New PathInfo() { _
          New PathInfo( _
           Me.ComputeUtility(start, target, bits), _
           New List(Of Point)(New Point() {start})) _
         })

        If wavefront.Count = 0 Then
            Console.WriteLine(String.Format("ComputePathPlan: WARNING start-point {0},{1} not reachable ", start.X, start.Y))
            start = Me.ExpandStartPoint(start, target, bits, history, wavefront, includeManhattanDistanceToStart)
            Console.WriteLine(String.Format("ComputePathPlan: NEW start-point {0},{1}", start.X, start.Y))

        End If

        If Me.ComputeDistanceToObstacle(target, bits) = 0 Then
            'target point is an obstacle
            Console.WriteLine(String.Format("ComputePathPlan: WARNING end-point {0},{1} not reachable ", target.X, target.Y))
            target = Me.ExpandEndPoint(start, target, bits, history, wavefront, includeManhattanDistanceToStart)
            Console.WriteLine(String.Format("ComputePathPlan: NEW end-point {0},{1} ", target.X, target.Y))

        End If


        Dim info As PathInfo = Nothing
        While Not wavefront.Count = 0

            'expand wavefront at first node
            info = Me.ExpandPathPlan(start, target, bits, history, wavefront, includeManhattanDistanceToStart)
            If history.Count / 1000 > max_dist Then
                Exit While
            End If
            If Not IsNothing(info) Then
                'path is returned when wavefront reached target
                Exit While
            End If

            'Console.WriteLine(String.Format("wavefront PP progress {0}", history.Count / 1000))

        End While

        occupancy.UnlockBits(bits)

        If Not IsNothing(info) Then
            Return info.Path.ToArray
        End If

        Return Nothing

    End Function

    'same function, only returns List of Points instead of Array of Points
    Public Function ComputePathPlan(ByVal occupancy As Bitmap, ByVal start As Point, ByVal target As Point, ByVal max_dist As Double) As List(Of Point)
        Return New List(Of Point)(Me.ComputePathPlan(occupancy, start, target, max_dist))
    End Function

    Private Function ExpandPathPlan(ByVal start As Point, ByVal target As Point, ByRef bits As BitmapData, ByRef history As List(Of Point), ByRef wavefront As List(Of PathInfo), ByVal includeManhattanDistanceToStart As Boolean) As PathInfo

        Dim curinfo As PathInfo = wavefront(0)
        wavefront.RemoveAt(0)

        Dim curpoint As Point = curinfo.Path(curinfo.Path.Count - 1)
		Dim nxtutil As Double, nxtpath As List(Of Point), nxtinfo As PathInfo, nxtdist As Double
        Dim nxtpoint As Point, x As Integer, y As Integer

        For y = curpoint.Y - 1 To curpoint.Y + 1
            For x = curpoint.X - 1 To curpoint.X + 1

                nxtpoint = New Point(x, y)

                If history.Contains(nxtpoint) Then
                    'already processed
                    Continue For

                Else
                    history.Insert(0, nxtpoint)
                    If x < 0 OrElse y < 0 OrElse x > bits.Width OrElse y > bits.Height Then
                        'avoid planning beyond map bounds
                        Continue For
                    End If
                End If

                If Me.ComputeDistanceToObstacle(nxtpoint, bits) = 0 Then
                    'this point is an obstacle
                    Continue For
                End If


                nxtutil = Me.ComputeUtility(nxtpoint, target, bits)
                nxtpath = New List(Of Point)(curinfo.Path)
                nxtpath.Add(nxtpoint)

                If includeManhattanDistanceToStart Then
                    'nxtutil += Abs(start.X - nxtpoint.X) + Abs(start.Y - nxtpoint.Y)
                    nxtutil += nxtpath.Count
                End If

                nxtinfo = New PathInfo(nxtutil, nxtpath)

				nxtdist = ((target.X - nxtpoint.X) ^ 2 + (target.Y - nxtpoint.Y) ^ 2) ^ 0.2
				If nxtdist <= 1 Then
					Return nxtinfo

				Else
					Dim i As Integer = 0
					While i < wavefront.Count
						If wavefront(i).Utility >= nxtutil Then
							Exit While
						End If
						i += 1
					End While

					wavefront.Insert(i, nxtinfo)

				End If

            Next
        Next

        Return Nothing

    End Function

    Private Function ExpandStartPoint(ByVal start As Point, ByVal target As Point, ByRef bits As BitmapData, ByRef history As List(Of Point), ByRef wavefront As List(Of PathInfo), ByVal includeManhattanDistanceToStart As Boolean) As Point

        Dim curpoint As Point = start
        Dim nxtpoint As Point, x As Integer, y As Integer
        Dim differenceX As Double = target.X - start.X
        Dim differenceY As Double = target.Y - start.Y


        If differenceX > differenceY Then
            y = curpoint.Y
            For x = curpoint.X To curpoint.X + CInt(differenceX / 10) Step Sign(differenceX)

                nxtpoint = New Point(x, y)

                If history.Contains(nxtpoint) Then
                    'already processed
                    Continue For

                Else
                    history.Insert(0, nxtpoint)
                    If x < 0 OrElse y < 0 OrElse x > bits.Width OrElse y > bits.Height Then
                        'avoid planning beyond map bounds
                        Continue For
                    End If
                End If

                If Me.ComputeDistanceToObstacle(nxtpoint, bits) = 0 Then
                    'this point is an obstacle
                    Continue For
                Else
                    Return nxtpoint
                End If


            Next
        Else
            x = curpoint.Y
            For y = curpoint.Y To curpoint.Y + CInt(differenceY / 10) Step Sign(differenceY)

                nxtpoint = New Point(x, y)

                If history.Contains(nxtpoint) Then
                    'already processed
                    Continue For

                Else
                    history.Insert(0, nxtpoint)
                    If x < 0 OrElse y < 0 OrElse x > bits.Width OrElse y > bits.Height Then
                        'avoid planning beyond map bounds
                        Continue For
                    End If
                End If

                If Me.ComputeDistanceToObstacle(nxtpoint, bits) = 0 Then
                    'this point is an obstacle
                    Continue For
                Else
                    Return nxtpoint
                End If


            Next
        End If


        Return Nothing

    End Function

    Private Function ExpandEndPoint(ByVal start As Point, ByVal target As Point, ByRef bits As BitmapData, ByRef history As List(Of Point), ByRef wavefront As List(Of PathInfo), ByVal includeManhattanDistanceToStart As Boolean) As Point

        Dim curpoint As Point = target
        Dim nxtpoint As Point, x As Integer, y As Integer
        Dim differenceX As Double = target.X - start.X
        Dim differenceY As Double = target.Y - start.Y


        If Abs(differenceX) > Abs(differenceY) Then
            y = curpoint.Y
            For x = curpoint.X To (curpoint.X - CInt(differenceX / 10)) Step -Sign(differenceX)

                nxtpoint = New Point(x, y)

                If history.Contains(nxtpoint) Then
                    'already processed
                    Continue For

                Else
                    history.Insert(0, nxtpoint)
                    If x < 0 OrElse y < 0 OrElse x > bits.Width OrElse y > bits.Height Then
                        'avoid planning beyond map bounds
                        Continue For
                    End If
                End If

                If Me.ComputeDistanceToObstacle(nxtpoint, bits) = 0 Then
                    'this point is an obstacle
                    Continue For
                Else
                    Return nxtpoint
                End If


            Next
        Else
            x = curpoint.X
            For y = curpoint.Y To (curpoint.Y - CInt(differenceY / 10)) Step -Sign(differenceY)

                nxtpoint = New Point(x, y)

                If history.Contains(nxtpoint) Then
                    'already processed
                    Continue For

                Else
                    history.Insert(0, nxtpoint)
                    If x < 0 OrElse y < 0 OrElse x > bits.Width OrElse y > bits.Height Then
                        'avoid planning beyond map bounds
                        Continue For
                    End If
                End If

                If Me.ComputeDistanceToObstacle(nxtpoint, bits) = 0 Then
                    'this point is an obstacle
                    Continue For
                Else
                    Return nxtpoint
                End If


            Next
        End If


        Return Nothing

    End Function

    Private Class PathInfo

        Public Sub New(ByVal utility As Double, ByVal path As List(Of Point))
            Me._Utility = utility
            Me._Path = path
        End Sub

        Private _Utility As Double
        Public ReadOnly Property Utility() As Double
            Get
                Return Me._Utility
            End Get
        End Property

        Private _Path As List(Of Point)
        Public ReadOnly Property Path() As List(Of Point)
            Get
                Return Me._Path
            End Get
        End Property

    End Class

    Private Function ComputeUtility(ByVal from As Point, ByVal goal As Point, ByVal bits As BitmapData) As Double

        Dim obst As Integer = Me.ComputeDistanceToObstacle(from, bits)
        Dim dist As Double = Me.ComputeDistanceToTarget(from, goal)

        If obst < 3 Then
            'prevent getting closer to obstacles than 30cm
            Return Double.MaxValue
        Else
            Return (Byte.MaxValue - obst) + dist
        End If

    End Function

    Protected Function ComputeDistanceToObstacle(ByVal p As Point, ByVal bits As BitmapData) As Integer
        Return CInt(Marshal.ReadByte(bits.Scan0, p.Y * bits.Stride + p.X))
    End Function

    Private Function ComputeDistanceToTarget(ByVal p1 As Point, ByVal p2 As Point) As Double
        Return ((p2.X - p1.X) ^ 2 + (p2.Y - p1.Y) ^ 2) ^ 0.5
    End Function

#End Region

#Region " Elementary Filters "

    Public Overridable Function ExtractObstacles(ByVal image As ManifoldImage) As Bitmap
        Using original As New Bitmap(image.ImageWidth, image.ImageHeight)
            'get original black dots on blue image
            Using gfx As Graphics = Graphics.FromImage(original)
                image.Draw(gfx, ManifoldRenderingMode.GdiPlus, True, False, False, False, False, True, False, False, False, False, False, False)
            End Using

            'get binary grayscale image
            Using grayscale As Bitmap = New GrayscaleBT709().Apply(original)
                Return New Threshold(CByte(1)).Apply(grayscale)
            End Using
        End Using
    End Function

    Public Overridable Function ExtractWalls(ByVal image As ManifoldImage) As Bitmap
        Using obstacles As Bitmap = Me.ExtractObstacles(image)
            Using opened As Bitmap = New Opening().Apply(obstacles)
                Using inverted As Bitmap = New Invert().Apply(opened)
                    Return New Dilatation().Apply(inverted)
                End Using
            End Using
        End Using
    End Function

    Public Overridable Function ExtractFreeArea(ByVal image As ManifoldImage) As Bitmap
        Using original As New Bitmap(image.ImageWidth, image.ImageHeight)
            'get original white on blue image
            Using gfx As Graphics = Graphics.FromImage(original)
                Dim use_apriori As Boolean = False 'The white overwrites the apriori-data
                image.Draw(gfx, ManifoldRenderingMode.GdiPlus, True, use_apriori, True, False, False, False, False, False, False, False, False, False)
            End Using

            'filter
            Using grayscale As Bitmap = New GrayscaleBT709().Apply(original)
                Using binary As Bitmap = New Threshold(CByte(20)).Apply(grayscale)
                    Return New Dilatation().Apply(binary)
                End Using
            End Using
        End Using
    End Function

    Public Overridable Function ExtractClearedArea(ByVal image As ManifoldImage) As Bitmap
        Using original As New Bitmap(image.ImageWidth, image.ImageHeight)
            'get original white on blue image
            Using gfx As Graphics = Graphics.FromImage(original)
                Dim use_apriori As Boolean = False 'The white overwrites the apriori-data
                image.Draw(gfx, ManifoldRenderingMode.GdiPlus, True, use_apriori, False, False, True, False, True, False, False, False, False, False)
            End Using

            'filter
            Using grayscale As Bitmap = New GrayscaleBT709().Apply(original)
                Using binary As Bitmap = New Threshold(CByte(20)).Apply(grayscale)
                    Return New Dilatation().Apply(binary)
                End Using
            End Using
        End Using
    End Function

    'Public Overridable Function ExtractMobilityArea(ByVal image As ManifoldImage) As Bitmap

    '    Using original As New Bitmap(image.ImageWidth, image.ImageHeight)
    '        'get original easy mobility area, which can be used as mask
    '        Using gfx As Graphics = Graphics.FromImage(original)
    '            image.Draw(gfx, ManifoldRenderingMode.GdiPlus, True, False, True, False, False, False, False, False, False, False, False)
    '        End Using

    '        'filter
    '        Using grayscale As Bitmap = New GrayscaleBT709().Apply(original)
    '            Using binary As Bitmap = New Threshold(CByte(20)).Apply(grayscale)
    '                Return New Dilatation().Apply(binary)
    '            End Using
    '        End Using
    '    End Using
    'End Function

    Public Overridable Function ExtractSafeArea(ByVal image As ManifoldImage) As Bitmap
        Using original As New Bitmap(image.ImageWidth, image.ImageHeight)
            'get original gray on blue image
            Using gfx As Graphics = Graphics.FromImage(original)
                image.Draw(gfx, ManifoldRenderingMode.GdiPlus, True, False, False, True, False, False, False, False, False, False, False, False)
            End Using

            'filter 
            Using grayscale As Bitmap = New GrayscaleBT709().Apply(original)
                Using binary As Bitmap = New Threshold(CByte(20)).Apply(grayscale)
                    Return New Dilatation().Apply(binary)
                End Using
            End Using
        End Using
    End Function

    Public Overridable Function ExtractExterior(ByVal area As Bitmap) As Bitmap
        'sobel ...
        Using edges As Bitmap = New SobelEdgeDetector().Apply(area)
            Return New Closing().Apply(edges)
        End Using
    End Function

#End Region

End Class
