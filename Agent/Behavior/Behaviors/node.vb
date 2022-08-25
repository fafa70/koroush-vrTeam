''' <summary>
''' this is the class that is used by A* algorithm and it consists an area that is about 10 meter and two bool variable.
''' </summary>
''' <remarks></remarks>



Class node

    Public Sub New(ByVal area As Drawing.Point, ByVal free As Boolean, ByVal checked As Boolean)
        Me.TraverseArea = area
        Me.IsWalkable = free
        Me.IsTraversed = checked
        'Me.x = x_index
        'Me.y = y_index
    End Sub

    Public Sub traversed()
        Me.IsTraversed = True
    End Sub

    Public Function IsRepeated() As Boolean
        If (Me.IsTraversed) Then
            Return True
        Else
            Return False
        End If
    End Function


    Public Function showX() As Integer
        Return Me.x
    End Function


    Public Function showY() As Integer
        Return Me.y
    End Function


    Public Function canWalk() As Boolean
        If (Me.IsWalkable) Then
            Return True
        Else
            Return False
        End If
    End Function

    Public Sub IsObstacle()
        Me.IsWalkable = False
    End Sub


    Private IsWalkable As Boolean
    Private IsTraversed As Boolean
    Private x As Integer
    Private y As Integer
    Private TraverseArea As Drawing.Point

End Class