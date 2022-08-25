Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports System.Drawing.Imaging
Imports System.Math
Imports System.IO

Imports OSGeo
Imports OSGeo.GDAL



''' <summary>
''' Reads and writes GeoTIFF images using the GDAL library.
''' </summary>
''' <remarks></remarks>
Public Class GeoTiff
    Implements IDisposable

#Region " Shared Load and Save Functionality "

    Private Shared mutex As New Object
    Private Shared gdalInitialized As Boolean

    ''' <summary>
    ''' Because initializing GDAL can take several seconds, this
    ''' method was implemented such that gdal initialization will
    ''' occur only once for the whole application lifetime.
    ''' </summary>
    ''' <remarks></remarks>
    Private Shared Sub InitializeGdal()

        'quick, non-threadsafe check
        If Not gdalInitialized Then

            'enforce thread-safety
            SyncLock mutex

                'now perform the check again but in a thread-safe context
                If Not gdalInitialized Then
                    'ok, we are sure: register driver(s).                                             */
                    Gdal.AllRegister()
                    gdalInitialized = True
                End If

            End SyncLock

        End If

    End Sub

    ''' <summary>
    ''' Opens the specified GeoTiFF file Neodym's BitmapConverter.
    ''' </summary>
    ''' <param name="filename"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Shared Function Load(ByVal filename As String) As GeoTiff

        

        Try
            If String.IsNullOrEmpty(filename) OrElse Not File.Exists(filename) Then
                Return Nothing
            End If

            InitializeGdal()

            'load metadata first
            Using meta As Dataset = Gdal.Open(filename, Access.GA_ReadOnly)

                'get image dimensions
                Dim w As Integer = meta.RasterXSize
                Dim h As Integer = meta.RasterYSize

                Dim img As Image
                Dim useNeodym As Boolean = False

                If Not useNeodym Then
                    'the dotnet native way, really fast
                    img = Image.FromFile(filename)

                Else
                    'using Neodym's BitmapConverter works too, but is rather slow
                    'when compared to .Net default image loading.

                    Dim bmp As New Bitmap(w, h, PixelFormat.Format32bppRgb)
                    Dim length As Integer = w * h
                    Dim r(length - 1) As Byte
                    Dim g(length - 1) As Byte
                    Dim b(length - 1) As Byte
                    meta.GetRasterBand(1).ReadRaster(0, 0, w, h, r, w, h, 0, 0)
                    meta.GetRasterBand(2).ReadRaster(0, 0, w, h, g, w, h, 0, 0)
                    meta.GetRasterBand(3).ReadRaster(0, 0, w, h, b, w, h, 0, 0)

                    For x As Integer = 0 To w - 1
                        For y As Integer = 0 To h - 1
                            Dim idx As Integer = y * w + x
                            bmp.SetPixel(x, y, Color.FromArgb(CInt(r(idx)), CInt(g(idx)), CInt(b(idx))))
                        Next
                    Next

                    img = bmp

                End If

                Dim thumb As Image = img.GetThumbnailImage(img.Width, img.Height, Nothing, Nothing)
                img.Dispose()

                'acquire geo-referencing (=coordinate transformation) parameters
                Dim num(5) As Double
                meta.GetGeoTransform(num)

                'return new GeoTiff image
                Return New GeoTiff(num, thumb)

            End Using

        Catch ex As Exception
            Console.Error.WriteLine(String.Format("{0} [ERROR] - {1}", "GeoTiff:Load", "Catched"))
            'Console.Error.WriteLine(ex.Message & vbNewLine & ex.StackTrace)
        End Try

        Return Nothing

    End Function


    ''' <summary>
    ''' Saves the supplied image as GeoTiff in the specified filename.
    ''' </summary>
    ''' <param name="filename"></param>
    ''' <param name="image"></param>
    ''' <param name="geo"></param>
    ''' <remarks></remarks>
    Public Shared Sub Save(ByVal filename As String, ByVal image As Bitmap, ByVal geo As GeoReference)

        InitializeGdal()

        'get individual color channels
        'Dim size As Size
        'Dim rChannel() As Complex = Nothing
        'Dim gChannel() As Complex = Nothing
        'Dim bChannel() As Complex = Nothing
        'Dim aChannel() As Complex = Nothing

        ''use MathNet's superfast sequential reading
        'BitmapConverter.ReadChannel(image, size, rChannel, gChannel, bChannel, aChannel)

        'load the driver for the GeoTIFF format
        Using driver As Driver = Gdal.GetDriverByName("GTIFF")

            'create new metadata 
            Using meta As Dataset = driver.Create(filename, image.Width, image.Height, 4, DataType.GDT_Byte, GetSaveOptions)

                'construct geo-reference params 
                Dim num() As Double = {geo.OffsetY, geo.ScaleX, geo.ShearX, geo.OffsetY, geo.ShearY, geo.ScaleY}
                meta.SetGeoTransform(num)

                'construct WGS84 Orthographic projection reference
                Using osrs As New OSGeo.OGR.SpatialReference("")
                    'Using osrs As New OSR.SpatialReference("")
                    osrs.SetProjection("Orthographic")
                    osrs.SetWellKnownGeogCS("WGS84")
                    osrs.SetProjParm("latitude_of_origin", 0)
                    osrs.SetProjParm("central_meridian", 0)
                    osrs.SetProjParm("false_easting", 0)
                    osrs.SetProjParm("false_northing", 0)

                    Dim proj As String = Nothing
                    osrs.ExportToWkt(proj)
                    meta.SetProjection(proj)

                End Using

                ''we need to convert the Complex number to bytes
                Dim length As Integer = image.Height * image.Width
                Dim w As Integer = image.Width
                Dim h As Integer = image.Height
                Dim r(length - 1) As Byte
                Dim g(length - 1) As Byte
                Dim b(length - 1) As Byte
                Dim a(length - 1) As Byte

                'For i As Integer = 0 To length - 1
                '    r(i) = CByte(rChannel(i).Real * 256)
                '    g(i) = CByte(gChannel(i).Real * 256)
                '    b(i) = CByte(bChannel(i).Real * 256)
                '    a(i) = CByte(aChannel(i).Real * 256)
                'Next


                'old style, very slow pixel-by-pixel copying
                For x As Integer = 0 To w - 1
                    For y As Integer = 0 To h - 1
                        Dim pixel As Color = image.GetPixel(x, y)
                        Dim idx As Integer = y * w + x
                        r(idx) = pixel.R
                        g(idx) = pixel.G
                        b(idx) = pixel.B
                    Next
                Next

                'flush the individual channels to their specific rasters
                With meta.GetRasterBand(1)
                    .SetRasterColorInterpretation(ColorInterp.GCI_RedBand)
                    .WriteRaster(0, 0, w, h, r, w, h, 0, 0)
                    .FlushCache()
                End With
                With meta.GetRasterBand(2)
                    .SetRasterColorInterpretation(ColorInterp.GCI_GreenBand)
                    .WriteRaster(0, 0, w, h, g, w, h, 0, 0)
                    .FlushCache()
                End With
                With meta.GetRasterBand(3)
                    .SetRasterColorInterpretation(ColorInterp.GCI_BlueBand)
                    .WriteRaster(0, 0, w, h, b, w, h, 0, 0)
                    .FlushCache()
                End With
                'With meta.GetRasterBand(4)
                '    .SetRasterColorInterpretation(ColorInterp.GCI_AlphaBand)
                '    .WriteRaster(0, 0, w, h, a, w, h, 0, 0)
                '    .FlushCache()
                'End With

                'flush all
                meta.FlushCache()

            End Using
        End Using
    End Sub

    ''' <summary>
    ''' Return "key=value" pairs compatible with WGS84 Orthographic projections
    ''' </summary>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Private Shared Function GetSaveOptions() As String()

        'TFW=YES: Force the generation of an associated ESRI world file (.tfw).See a World Files section for details.
        'INTERLEAVE=[BAND,PIXEL]: By default TIFF files with band interleaving (PLANARCONFIG_SEPARATE in TIFF terminology) are created. These are slightly more efficient for some purposes, but some applications only support pixel interleaved TIFF files. In these cases pass INTERLEAVE=PIXEL to produce pixel interleaved TIFF files (PLANARCONFIG_CONTIG in TIFF terminology).
        'TILED=YES: By default stripped TIFF files are created. This option can be used to force creation of tiled TIFF files.
        'BLOCKXSIZE=n: Sets tile width, defaults to 256.
        'BLOCKYSIZE=n: Set tile or strip height. Tile height defaults to 256, strip height defaults to a value such that one strip is 8K or less.
        'NBITS=n: Create a file with less than 8 bits per sample by passing a value from 1 to 7. The apparent pixel type should be Byte.
        'COMPRESS=[JPEG/LZW/PACKBITS/DEFLATE/CCITTRLE/CCITTFAX3/CCITTFAX4/NONE]: Set the compression to use. JPEG should only be used with Byte data. The CCITT compression should only be used with 1bit (NBITS=1) data. None is the default.
        'PREDICTOR=[1/2/3]: Set the predictor for LZW or DEFLATE compression. The default is 1 (no predictor), 2 is horizontal differencing and 3 is floating point prediction.
        'JPEG_QUALITY=[1-100]: Set the JPEG quality when using JPEG compression. A value of 100 is best quality (least compression), and 1 is worst quality (best compression). The default is 75.
        'PROFILE=[GDALGeoTIFF/GeoTIFF/BASELINE]: Control what non-baseline tags are emitted by GDAL. With GDALGeoTIFF (the default) various GDAL custom tags may be written. With GeoTIFF only GeoTIFF tags will be added to the baseline. With BASELINE no GDAL or GeoTIFF tags will be written. BASELINE is occationally useful when writing files to be read by applications intolerant of unrecognised tags.
        'PHOTOMETRIC=[MINISBLACK/MINISWHITE/RGB/CMYK/YCBCR/CIELAB/ICCLAB/ITULAB]: Set the photometric interpretation tag. Default is MINISBLACK, but if the input image has 3 or 4 bands of Byte type, then RGB will be selected. You can override default photometric using this option.
        'ALPHA=YES: The first "extrasample" is marked as being alpha if there are any extra samples. This is necessary if you want to produce a greyscale TIFF file with an alpha band (for instance).

        Dim dict As New Specialized.StringDictionary
        dict.Add("BLOCKXSIZE", "256")
        dict.Add("BLOCKYSIZE", "1")
        dict.Add("PROFILE", "GDALGeoTIFF")

        'convert dict to list
        Dim list As New List(Of String)
        For Each key As String In dict.Keys
            list.Add(String.Format("{0}={1}", key, dict(key)))
        Next

        'convert list to array
        Return list.ToArray

    End Function

#End Region




#Region " Constructor "

    ''' <summary>
    ''' Private constructor, new GeoTiffs should be created using the shared Open
    ''' member.
    ''' </summary>
    ''' <param name="num"></param>
    ''' <param name="img"></param>
    ''' <remarks></remarks>
    Private Sub New(ByVal num() As Double, ByVal img As Image)

        If IsNothing(img) Then Throw New ArgumentNullException("img")

        'read numbers into more descriptive properties
        Me._OffsetX = CType(num(0), Single)
        Me._OffsetY = CType(num(3), Single)
        Me._PixelWidth = CType(num(1), Single)
        Me._PixelHeight = CType(num(5), Single)
        Me._ShearX = CType(num(2), Single)
        Me._ShearY = CType(num(4), Single)

        Me._Image = img

    End Sub

#End Region

#Region " Properties "

    Private _OffsetX As Single
    Public ReadOnly Property OffsetX() As Single
        Get
            Return _OffsetX
        End Get
    End Property

    Private _OffsetY As Single
    Public ReadOnly Property OffsetY() As Single
        Get
            Return _OffsetY
        End Get
    End Property

    Private _PixelWidth As Single
    Public ReadOnly Property PixelWidth() As Single
        Get
            Return _PixelWidth
        End Get
    End Property

    Private _PixelHeight As Single
    Public ReadOnly Property PixelHeight() As Single
        Get
            Return _PixelHeight
        End Get
    End Property

    Private _ShearX As Single
    Public ReadOnly Property ShearX() As Single
        Get
            Return _ShearX
        End Get
    End Property

    Private _ShearY As Single
    Public ReadOnly Property ShearY() As Single
        Get
            Return _ShearY
        End Get
    End Property



    Private _Image As Image
    Public ReadOnly Property Image() As Image
        Get
            Return _Image
        End Get
    End Property

#End Region

#Region " IDisposable "

    Private disposedValue As Boolean = False        ' To detect redundant calls

    ' IDisposable
    Protected Overridable Sub Dispose(ByVal disposing As Boolean)
        If Not Me.disposedValue Then
            If disposing Then

                If Not IsNothing(Me.Image) Then
                    Me.Image.Dispose()
                End If

            End If

            ' TODO: free shared unmanaged resources
        End If
        Me.disposedValue = True
    End Sub

    ' This code added by Visual Basic to correctly implement the disposable pattern.
    Public Sub Dispose() Implements IDisposable.Dispose
        ' Do not change this code.  Put cleanup code in Dispose(ByVal disposing As Boolean) above.
        Dispose(True)
        GC.SuppressFinalize(Me)
    End Sub

#End Region

End Class
