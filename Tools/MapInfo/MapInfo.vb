Imports OSGeo
Imports OSGeo.OGR

Imports UvARescue.Math



''' <summary>
''' Reads and writes MapInfo Interchange Files (mif/mid) using the OGR library.
''' </summary>
''' <remarks></remarks>
Public Class MapInfo
    Implements IDisposable

#Region " Constructor "

    Public Sub New() 
        'should also contain the data, a list of drawable objects

        Me._Points = New PointQueue("Points")
        Me._LineStrings = New LineStringQueue("LineStrings")

    End Sub

    ''' <summary>
    ''' Private constructor.
    ''' </summary>
    ''' <param name="transformation"></param>
    ''' <remarks></remarks>
    Private Sub New(ByVal transformation() As Double) ' from the MIF-heather TRANSFORM and DELIMITER 
        Me.New()

        'read numbers into more descriptive properties
        Me._OffsetX = CType(transformation(0), Single)
        Me._OffsetY = CType(transformation(3), Single)
        Me._PixelWidth = CType(transformation(1), Single)
        Me._PixelHeight = CType(transformation(5), Single)
        Me._ShearX = CType(transformation(2), Single)
        Me._ShearY = CType(transformation(4), Single)

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

#End Region

#Region " DataElements "

    Private _Points As PointQueue
    Public ReadOnly Property Points() As PointQueue
        Get
            Return _Points
        End Get
    End Property

    Private _LineStrings As LineStringQueue
    Public ReadOnly Property LineStrings() As LineStringQueue
        Get
            Return _LineStrings
        End Get
    End Property

#End Region

    Private Shared mutex As New Object
    Private Shared ogrInitialized As Boolean

    ''' <summary>
    ''' Because initializing GDAL can take several seconds, this
    ''' method was implemented such that gdal initialization will
    ''' occur only once for the whole application lifetime.
    ''' </summary>
    ''' <remarks></remarks>
    Private Shared Sub InitializeOgr() 'Is this needed

        'quick, non-threadsafe check
        If Not ogrInitialized Then

            'enforce thread-safety
            SyncLock mutex

                'now perform the check again but in a thread-safe context
                If Not ogrInitialized Then
                    'ok, we are sure: register driver(s).  
                    Ogr.RegisterAll()
                    ogrInitialized = True
                End If

            End SyncLock

        End If

    End Sub

    Public Shared Function Load(ByVal filename As String) As MapInfo

        If IsNothing(filename) Then
            Return Nothing
        End If

        Try

            'See http://www.gdal.org/ogr/ogr_apitut.html

            InitializeOgr()



            'load metadata first
            Using meta As DataSource = Ogr.Open(filename, 0) 'update = FALSE (I hope)

                Dim name As String = meta.GetName
                Dim layers As Integer = meta.GetLayerCount
                Dim references As Integer = meta.GetRefCount
                Dim summary As String = meta.ToString

                For i As Integer = 0 To layers - 1

                    'acquire geo-referencing (=coordinate transformation) parameters

                    Using layer As Layer = meta.GetLayerByIndex(i)
                        Dim num(7) As Double
                        Dim success As Integer

                        layer.ResetReading()

                        name = layer.GetName 'Is FileName without extension

                        Dim features As Integer = layer.GetFeatureCount(1) 'force
                        Dim filter As Geometry = layer.GetSpatialFilter
                        If Not IsNothing(filter) Then
                            Dim boundary As Geometry = filter.GetBoundary
                            Dim X As Double = boundary.GetX(0)
                        End If

                        'Dim envelop As Envelope

                        'Dim success As Integer = layer.GetExtent(envelop, 0) 'this call crashes , due to memory problems

                        'If Not IsNothing(envelop(0)) Then
                        '    Dim maxX As Double = envelop(0).MaxX
                        '    Dim minX As Double = envelop(0).MinX
                        '    Dim maxY As Double = envelop(0).MaxY
                        '    Dim minY As Double = envelop(0).MinY

                        'End If

                        Dim attribute As String
                        Dim spatialReference As SpatialReference = layer.GetSpatialRef

                        Dim proj As String = Nothing
                        success = spatialReference.ExportToWkt(proj) 'the name in OpenGIS Well Known Text format


                        Dim projected As Integer = spatialReference.IsProjected() ' projected on a flat earth
                        Dim local As Integer = spatialReference.IsLocal()
                        Dim geographic As Integer = spatialReference.IsGeographic()

                        ' See http://www.gdal.org/ogr/osr_tutorial.html

                        If projected = 1 OrElse local = 1 Then

                            Dim scale As Double = spatialReference.GetLinearUnits() ' such as 1.0
                            Dim unit As String = spatialReference.GetLinearUnitsName() 'such as "Meters"

                            If local = 0 Then 'Non-local, more information available

                                attribute = spatialReference.GetAttrValue("PROJCS", 0)

                                attribute = spatialReference.GetAttrValue("PROJECTION", 0)

                                'This are valid question for the Transverse_Mercator projection

                                Dim parameter As Double = spatialReference.GetProjParm("latitude_of_origin", -0.123)
                                parameter = spatialReference.GetProjParm("central_meridian", -0.1234)
                                parameter = spatialReference.GetProjParm("scale_factor", -0.12345)
                                parameter = spatialReference.GetProjParm("false_easting", -0.123456)
                                parameter = spatialReference.GetProjParm("false_northing", -0.1234567)


                                'the following exports code works, but not needed

                                'success = spatialReference.ExportToPrettyWkt(proj, 1) 'simplify is true

                                'success = spatialReference.ExportToPrettyWkt(proj, 0) 'simplify is false

                                'success = spatialReference.ExportToProj4(proj)

                                'success = spatialReference.ExportToPCI(proj, unit)

                                'Dim code(), zone(), datum() As Integer

                                'success = spatialReference.ExportToUSGS(code, zone, datum) 'crashes due to memory problems


                                'This works too, but do the result is still zeros

                                'success = spatialReference.SetProjection("Orthographic")
                                'success = spatialReference.SetWellKnownGeogCS("WGS84")

                                'scale = spatialReference.GetLinearUnits() ' still the same? yes
                                'unit = spatialReference.GetLinearUnitsName() 'still the same? no, now degrees

                            End If


                            'A Projected coordinate system measured in meters

                        End If

                        If geographic = 1 OrElse projected = 1 Then
                            ' A geographic coordinate system measured in long/lat

                            ' Or a non-local projection, which always has an underlying Geometric representation
                            attribute = spatialReference.GetAttrValue("GEOGCS", 0)

                            Dim angularscale As Double = spatialReference.GetAngularUnits() ' such as 1.0
                            Dim angularunit As String = spatialReference.GetAttrValue("UNIT", 0) 'such as "Degrees" (no GetAngularUnitsName)

                            attribute = spatialReference.GetAttrValue("DATUM", 0)

                            attribute = spatialReference.GetAttrValue("SPHEROID", 0) 'name
                            attribute = spatialReference.GetAttrValue("SPHEROID", 1) 'number
                            attribute = spatialReference.GetAttrValue("SPHEROID", 2) 'number

                            attribute = spatialReference.GetAttrValue("PRIMEM", 0)

                            success = spatialReference.GetTOWGS84(num) 'only first three could be non-zeros
                            'first three numbers also zero if datum = WGS84


                        End If

                        Dim mapinfo As MapInfo = New MapInfo(num) 'num contains the transformation


                        'Get all features from this layer (see http://www.gdal.org/ogr/ogr_apitut.html)

                        Dim featureCount As Integer = layer.GetFeatureCount(1) 'force = true
                        Dim feature As OSGeo.OGR.Feature
                        Dim geometry As OSGeo.OGR.Geometry
                        Dim type As OSGeo.OGR.wkbGeometryType
                        Dim field As OSGeo.OGR.FieldDefn

                        For j As Integer = 1 To featureCount 'no bug, features start at 1


                            feature = layer.GetFeature(j) 'alternative is While loop with GetNextFeature

                            If IsNothing(feature) Then
                                Continue For
                            End If

                            geometry = feature.GetGeometryRef()
                            If IsNothing(geometry) Then
                                Continue For
                            End If

                            type = geometry.GetGeometryType
                            If IsNothing(type) Then
                                Continue For
                            End If

                            Select Case type
                                Case wkbGeometryType.wkbPoint


                                    'Dim y As Double = geometry.GetX(0) 'switched in Jacobs.mif
                                    'Dim x As Double = geometry.GetY(0)
                                    Dim pointName As String = feature.GetFieldAsString("ID")
                                    Dim comment As String = feature.GetFieldAsString(2)

                                    'generic alternative:
                                    'GetFieldCount()
                                    field = feature.GetFieldDefnRef(2)
                                    If Not IsNothing(field) Then
                                        Dim fieldName As String = field.GetNameRef()
                                        Dim fieldValue As String = field.ToString()
                                        fieldValue = feature.GetFieldAsString(2)
                                    End If

                                    Dim style As String = feature.GetStyleString

                                    mapinfo.Points.EnqueueData(New PointData(geometry.GetY(0), geometry.GetX(0), 0.0, 0.0)) 'Could be used for GetFieldAsDoubles

                                Case wkbGeometryType.wkbLineString

                                    Dim lineID As Integer = feature.GetFieldAsInteger("ID")
                                    Dim lineName As String = feature.GetFieldAsString("Name")

                                    Dim pointCount As Integer = geometry.GetPointCount

                                    If pointCount > 0 Then


                                        Dim line As LineStringData = New LineStringData(pointCount)


                                        For p As Integer = 0 To pointCount - 1 'point start at 0

                                            line.X(p) = geometry.GetX(p)
                                            line.Y(p) = geometry.GetY(p) 'due to coordinate system of UsarSim

                                        Next

                                        'store last points as target point


                                        ' mapinfo._TargetPoints.EnqueueData(New PointData(line.X(pointCount - 1), line.Y(pointCount - 1)))


                                        mapinfo.LineStrings.EnqueueData(line)


                                    End If




                                Case Else

                                    Console.WriteLine("[MapInfo:Load] WARNING, to be implemented GeometryType")

                            End Select








                        Next




                        Return mapinfo




                    End Using
                Next




                ' How do I get OSGeo.OGR.SpatialReference



                'acquire geo-referencing (=coordinate transformation) parameters
                ' Dim num(5) As Double

                'return new MapInfo object

            End Using

        Catch ex As Exception
            Console.WriteLine("Error occurred while trying to load MapInfo Interchange File.")
            Console.WriteLine(ex.Message & vbNewLine & ex.StackTrace)

        End Try

        Return Nothing

    End Function

    Public Shared Sub Save(ByVal filename As String, ByVal mapInfo As MapInfo)

        If IsNothing(filename) Then
            Return
        End If

        Try

            'See http://www.gdal.org/ogr/ogr_apitut.html

            InitializeOgr()

            Dim driver As Driver = Ogr.GetDriverByName("MapInfo File")

            If IsNothing(driver) Then
                Console.WriteLine(String.Format("{0} driver not available.\n", "MapInfo File"))
                Return
            End If

            Dim options As String() = New String() {"Format = MIF"}
            'see http://www.gdal.org/ogr/drv_mitab.html

            Using meta As DataSource = driver.CreateDataSource(filename, options) 'MIF is the interchange format, TAB is the default

                If IsNothing(meta) Then
                    Console.WriteLine(String.Format("{0} could not be created in format MIF", filename))
                    Return
                End If

                Dim wkt As String = "LOCAL_CS[""Nonearth"",UNIT[""Meter"",1]]"

                Dim spatialReference As SpatialReference = New SpatialReference(Nothing) 'CS NonEarth
                spatialReference.SetLocalCS("NonEarth")
                spatialReference.SetLinearUnits("Meter", 1.0)


                'check if result is as expected
                Dim proj As String = Nothing
                Dim success As Integer = spatialReference.ExportToWkt(proj)

                options = New String() {"SPATIAL_INDEX_MODE=QUICK"}

                If mapInfo.LineStrings.DataCount > 0 Then

                    Using layer As Layer = meta.CreateLayer("Paths", spatialReference, wkbGeometryType.wkbLineString, options)

                        If IsNothing(layer) Then
                            Console.WriteLine(String.Format("Layer creation failed"))
                            Return
                        End If

                        Dim fieldDefn As FieldDefn = New FieldDefn("Tag", FieldType.OFTString)

                        success = layer.CreateField(fieldDefn, 1) 'approx_ok = true


                        Dim featureDefn As FeatureDefn = New FeatureDefn("Path")

                        Dim feature As Feature = Nothing
                        Dim path As LineStringData = Nothing
                        Dim geometry As Geometry = Nothing


                        For i As Integer = 0 To mapInfo.LineStrings.DataCount - 1

                            feature = New Feature(featureDefn)


                            ' feature.SetField("TargetName", String.Format("Target{0}", i))

                            geometry = New Geometry(wkbGeometryType.wkbLineString) 'also possible with pointlist


                            path = mapInfo.LineStrings.PopData

                            Dim pointCount As Integer = path.PointCount

                            ' Dim line As LineStringData = New LineStringData(pointCount)

                            For p As Integer = 0 To pointCount - 1 'point start at 0

                                geometry.SetPoint(p, path.X(p), path.Y(p), 0.0)
                                'x and y not longer swapped

                            Next

                            feature.SetGeometry(geometry)

                            success = layer.CreateFeature(feature)
                            'feature.Dispose()

                        Next

                        'layer.Dispose()

                    End Using 'layer
                End If

                If mapInfo.Points.DataCount > 0 Then

                    Using layer As Layer = meta.CreateLayer("Points", spatialReference, wkbGeometryType.wkbLineString, options)

                        If IsNothing(layer) Then
                            Console.WriteLine(String.Format("Layer creation failed"))
                            Return
                        End If

                        Dim xFieldDefn As FieldDefn = New FieldDefn("X", FieldType.OFTReal)
                        success = layer.CreateField(xFieldDefn, 1) 'approx_ok = true

                        Dim yFieldDefn As FieldDefn = New FieldDefn("Y", FieldType.OFTReal)
                        success = layer.CreateField(yFieldDefn, 1) 'approx_ok = true

                        Dim thFieldDefn As FieldDefn = New FieldDefn("rotation", FieldType.OFTReal)
                        success = layer.CreateField(thFieldDefn, 1) 'approx_ok = true

                        Dim secFieldDefn As FieldDefn = New FieldDefn("timestamp", FieldType.OFTReal)
                        success = layer.CreateField(secFieldDefn, 1) 'approx_ok = true

                        'Dim featureDefn As FeatureDefn = New FeatureDefn("Point")

                        Dim feature As Feature = Nothing
                        Dim point As PointData = Nothing
                        Dim geometry As Geometry = Nothing


                        For i As Integer = 0 To mapInfo.Points.DataCount - 1

                            feature = New Feature(layer.GetLayerDefn)

                            point = mapInfo.Points.PopData

                            feature.SetField("X", point.X)
                            feature.SetField("Y", point.Y)
                            feature.SetField("rotation", point.Th)
                            feature.SetField("timestamp", point.Sec)

                            'should also add th and measuredtime

                            geometry = New Geometry(wkbGeometryType.wkbPoint) 'also possible with pointlist


                            geometry.AddPoint(point.X, point.Y, 0)

                            feature.SetGeometry(geometry)

                            success = layer.CreateFeature(feature)
                            'feature.Dispose()

                        Next

                        'layer.Dispose()

                    End Using 'layer
                End If

                'meta.Dispose()
            End Using 'meta datasource


        Catch ex As Exception
            Console.WriteLine("Error occurred while trying to save  MapInfo Interchange File.")
            Console.WriteLine(ex.Message & vbNewLine & ex.StackTrace)

        End Try

        Return

    End Sub

#Region " IDisposable Support "


    Private disposedValue As Boolean = False        ' To detect redundant calls

    ' IDisposable
    Protected Overridable Sub Dispose(ByVal disposing As Boolean)
        If Not Me.disposedValue Then
            If disposing Then
                ' TODO: free unmanaged resources when explicitly called
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
