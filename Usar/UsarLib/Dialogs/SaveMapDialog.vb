Imports System.Drawing
Imports System.Drawing.Imaging
Imports System.IO
Imports System.Windows.Forms

Imports UvARescue.Agent
Imports UvARescue.Tools
Imports UvARescue.Math


Public Class SaveMapDialog

    ' A Range defines the range of patches that are used to create the final map.
    Public Structure Range
        Dim first As Integer
        Dim last As Integer
        Public Sub New(ByVal _first As Integer, ByVal _last As Integer)
            first = _first
            last = _last
        End Sub
    End Structure

    Private _GeoRef As GeoReference
    Private _Image As ManifoldImage
    Private _ManifoldView As ManifoldView
    Private _TeamMembers As List(Of String)
    Private _PatchIndex As Dictionary(Of String, List(Of Patch))
    Private _PatchesToBeRendered As Dictionary(Of String, List(Of Range))
    Private _SaveMapPath As String

#Region " Constructor "
    Public Sub New(ByVal teamconfig As TeamConfig, ByRef view As ManifoldView)
        ' This call is required by the Windows Form Designer.
        InitializeComponent()

        ' Initialize Tabs
        With tabMenu
            .TabPages.Item(0).Text = "Process"
            .TabPages.Item(1).Text = "Save"
        End With

        ' Save image locally for processing
        Me._Image = view.Image()

        ' Save ManifoldView locally.  NOTE: passed by reference so changes are permanent!!
        Me._ManifoldView = view

        ' Initialize list of team members (excluding Operator)
        Me.ResetTeammembers(teamconfig)

        ' Initialize Processing Tab
        Me.InitializeProcessTab(view.Image.Manifold.PatchIndex)

        ' Initialize Saving Tab
        Me.InitializeSavingTab()

        Me.RedrawMap()
    End Sub
#End Region

#Region " Constructor helper functions "
    Private Sub InitializeSavingTab()
        'try to load last used pathname
        Me._SaveMapPath = My.Settings.LastMapSavePath
        If Not My.Computer.FileSystem.DirectoryExists(Me._SaveMapPath) Then
            Me._SaveMapPath = My.Computer.FileSystem.Drives(0).ToString
        End If
        Me.lblPath.Text = Me._SaveMapPath

        'check for Geo Reference Data
        If Me._Image.GetGeoReferenceData(Me._GeoRef) Then
            With Me._GeoRef
                Me.txtOffsetX.Text = CStr(.OffsetX)
                Me.txtOffsetY.Text = CStr(.OffsetY)
                Me.txtPixelWidth.Text = CStr(.ScaleX)
                Me.txtPixelHeight.Text = CStr(.ScaleY)
                Me.txtShearingX.Text = CStr(.ShearX)
                Me.txtShearingY.Text = CStr(.ShearY)
            End With

        Else
            MsgBox("Could not load GeoReference data for Saving from ManifoldImage", MsgBoxStyle.OkOnly Or MsgBoxStyle.Exclamation)
            Me.DialogResult = Windows.Forms.DialogResult.Cancel
            Me.Close()

        End If
    End Sub

    Private Sub InitializeProcessTab(ByVal patchIndex As Dictionary(Of String, List(Of Patch)))
        ' Save the Patch index locally
        Me._PatchIndex = patchIndex

        ' Initialize dictionary of how many patches should be rendered for each agent
        Me._PatchesToBeRendered = New Dictionary(Of String, List(Of Range))

        ' Initialize labels and patches to be rendered;
        ' Add Patch Track Bar for each agent
        For i As Integer = 0 To Me._TeamMembers.Count - 1
            Dim currName As String = Me._TeamMembers(i)
            Me._PatchesToBeRendered(currName) = New List(Of Range)
            ' Dim currPatchTotal As Integer

            'If (patchIndex.ContainsKey(currName)) Then
            ' currPatchTotal = patchIndex.Item(currName).Count
            ' Else
            'currPatchTotal = 0
            'End If


            'Me.SetPatchTotal(currName, currPatchTotal)

            With Me.pnlAgentPatches.Controls
                Dim patchView As AgentPatchController = Me.CreateAgentPatchView(currName)
                If Not IsNothing(patchView) Then
                    patchView.Dock = DockStyle.Top
                    .Add(patchView)
                    .SetChildIndex(patchView, 0)
                Else
                    MsgBox("Could Initialize Patch (No patches yet?)", MsgBoxStyle.OkOnly)
                End If
            End With
        Next
    End Sub

    Public Sub ResetTeammembers(ByVal teamconfig As TeamConfig)
        Me._TeamMembers = New List(Of String)
        If Not IsNothing(teamconfig) Then
            Dim members() As String = Strings.Split(teamconfig.TeamMembers, ",")
            For Each member As String In members
                Dim parts() As String = Strings.Split(member, "-")
                Dim memberName As String = parts(1)

                If Not (memberName = teamconfig.OperatorName) Then
                    Me._TeamMembers.Add(memberName)
                End If
            Next
        End If
    End Sub
#End Region

#Region " Save image "
    Private Sub Save()
        'get all supported save options
        Dim optvalues As Array = System.Enum.GetValues(GetType(ManifoldImage.SaveOption))
        Dim optnames() As String = System.Enum.GetNames(GetType(ManifoldImage.SaveOption))

        'inspect selected image formats
        Dim formats As New List(Of ImageFormat)
        For Each item As ListViewItem In Me.lvwFormats.Items
            If item.Checked Then
                formats.Add(Me.GetImageFormat(CStr(item.Tag)))
            End If
        Next

        'to display progress, count the number of tasks
        Dim tasks As Integer = formats.Count
        tasks *= optvalues.Length 'each option for every format
        tasks *= 2 'world file generation for each format+save option
        If Me.chkGeoTiff.Checked Then
            tasks += 1 'GeoTIFF generation
        End If

        'setup progressbar
        With Me.barProgress
            .Value = 0
            .Minimum = 0
            .Maximum = tasks
        End With

        'construct base filename
        Dim basefilename As String = Me.txtName.Text.Trim
        If String.IsNullOrEmpty(basefilename) Then
            MsgBox("No filename specified", MsgBoxStyle.OkOnly Or MsgBoxStyle.Exclamation)
            Exit Sub
        Else
            'combine with selected folder
            basefilename = Path.Combine(Me._SaveMapPath, Path.GetFileNameWithoutExtension(basefilename))
        End If



        'now save, loop through formats
        For Each format As ImageFormat In formats

            'for each save option
            For i As Integer = 0 To optvalues.Length - 1
                'get save option
                Dim optvalue As ManifoldImage.SaveOption = CType(optvalues.GetValue(i), ManifoldImage.SaveOption)

                'construct filenames for this layer
                Dim imgFilename As String = String.Format("{0}-{1}-[{2}].{3}", basefilename, CInt(optvalue), optnames(i).ToLower, Me.GetImageFileExtension(format))
                Dim wldFilename As String = String.Format("{0}-{1}-[{2}].{3}", basefilename, CInt(optvalue), optnames(i).ToLower, Me.GetWorldFileExtension(format))

                'save the world file
                Me._GeoRef.SaveWorldFile(wldFilename)
                Me.barProgress.Value += 1

                Dim bitmap As Bitmap = Nothing
                If Me._Image.GetImageFileSaveData(bitmap, optvalue) Then
                    Try
                        bitmap.Save(imgFilename, format)
                    Catch ex As Exception
                        MsgBox("Could not Save as " & format.ToString.ToUpper & " to " & imgFilename & vbNewLine & ex.Message, MsgBoxStyle.OkOnly Or MsgBoxStyle.Exclamation)
                    Finally
                        If Not IsNothing(bitmap) Then
                            bitmap.Dispose()
                        End If
                    End Try
                Else
                    MsgBox("Could not get data from Manifold for layer " & optnames(i), MsgBoxStyle.OkOnly Or MsgBoxStyle.Exclamation)
                End If
                Me.barProgress.Value += 1
            Next
        Next

        If Me.chkGeoTiff.Checked Then
            Dim bitmap As Bitmap = Nothing
            If Me._Image.GetImageFileSaveData(bitmap, ManifoldImage.SaveOption.Jury) Then
                Try
                    GeoTiff.Save(basefilename & Me.GetImageFileExtension(ImageFormat.Tiff), bitmap, Me._GeoRef)
                Catch ex As Exception
                    MsgBox("Could not Save as GeoTIFF" & vbNewLine & ex.Message, MsgBoxStyle.OkOnly, MsgBoxStyle.Exclamation)
                End Try
            Else
                MsgBox("Could not get data from Manifold for GeoTIFF", MsgBoxStyle.OkOnly, MsgBoxStyle.Exclamation)
            End If
            Me.barProgress.Value += 1
        End If

        MsgBox("Map was saved successfully", MsgBoxStyle.OkOnly Or MsgBoxStyle.Exclamation)
        Me.barProgress.Value = 0


        Dim mapInfoData As MapInfo = New MapInfo
        Dim currPatch As Patch

        Dim pathFileName As String

        For Each currAgent As String In Me._TeamMembers
            pathFileName = basefilename + currAgent + "_Path.mif"

            For Each patches As Range In Me._PatchesToBeRendered(currAgent)
                For i As Integer = patches.first To patches.last
                    currPatch = Me._PatchIndex.Item(currAgent).Item(i)

                    Dim origin As Pose2D = currPatch.EstimatedOrigin
                    Dim lpose As New Pose2D(0, 0, 0)
                    Dim gpose As Pose2D = origin.ToGlobal(lpose)

                    mapInfoData.Points.EnqueueData(New PointData(gpose.X / 1000, gpose.Y / 1000, gpose.Rotation, currPatch.Scan.MeasuredTime))
                Next

            Next

            Tools.MapInfo.Save(pathFileName, mapInfoData)
        Next
    End Sub
#End Region

#Region " Image saving helper functions "

    Private Function GetImageFormat(ByVal extension As String) As ImageFormat
        Select Case extension.ToLower
            Case "tif", "tiff"
                Return ImageFormat.Tiff
            Case "gif"
                Return ImageFormat.Gif
            Case "jpg", "jpe", "jpeg"
                Return ImageFormat.Jpeg
            Case "png"
                Return ImageFormat.Png
            Case Else
                Return ImageFormat.Bmp
        End Select
    End Function

    Private Function GetImageFileExtension(ByVal format As ImageFormat) As String
        If format.Equals(ImageFormat.Tiff) Then
            Return "tif"
        ElseIf format.Equals(ImageFormat.Gif) Then
            Return "gif"
        ElseIf format.Equals(ImageFormat.Jpeg) Then
            Return "jpg"
        ElseIf format.Equals(ImageFormat.Png) Then
            Return "png"
        Else
            Return "bmp"
        End If
    End Function

    Private Function GetWorldFileExtension(ByVal format As ImageFormat) As String
        Dim imExt As String = Me.GetImageFileExtension(format)
        Dim wfExt As String = ""
        If imExt.Length > 2 Then
            'first char of imgext + last char of imgext + w 
            wfExt &= imExt.Substring(0, 1)
            wfExt &= imExt.Substring(imExt.Length - 1, 1)
            wfExt &= "w"
        Else
            'default to TFW
            wfExt = "tfw"
        End If
        Return wfExt
    End Function
#End Region

#Region " Map processing helper functions "
    Public Function GetPatchesToBeRendered(ByVal name As String) As List(Of Range)
        Return Me._PatchesToBeRendered(name)
    End Function

    ' Add a patch. It can overlap in three ways.
    ' 1. overlap like segments 1-3 + 2-5 results in 1-5. (The start of the new patch is extended and old patch removed)
    ' 2. overlap like 2-3 + 1-5 results in 1-5. (New patch overlaps completely, so remove old one)
    ' 3. overlap like 2-5 + 1-3 results in 1-5. (The end of the new patch is extended and the old patch removed)
    Public Sub AddToPatchesToBeRendered(ByVal name As String, ByVal first As Integer, ByVal last As Integer)
        ' first should always be smaller or equal to last.
        If first > last Then
            Dim swap As Integer = first
            first = last
            last = swap
        End If

        Dim RenderRanges As List(Of SaveMapDialog.Range) = New List(Of Range)
        For Each r As Range In GetPatchesToBeRendered(name)
            If r.first < first AndAlso first <= r.last AndAlso r.last <= last Then
                ' Case 1: overlap at left of new patch
                first = r.first
            ElseIf first <= r.first AndAlso r.first <= last AndAlso first <= r.last AndAlso r.last <= last Then
                ' Case 2: complete overlap
            ElseIf first <= r.first AndAlso r.first <= last AndAlso last < r.last Then
                ' Case 3: overlap at right of new patch
                last = r.last
            Else
                ' Case 4: No overlap
                RenderRanges.Add(r)
            End If
        Next
        RenderRanges.Add(New Range(first, last))
        Me._PatchesToBeRendered(name) = RenderRanges
        Me.RedrawMap()
    End Sub

    ' Delete all sections that contain this patch
    Public Sub DelPatchesToBeRendered(ByVal name As String, ByVal patch As Integer)
        Dim RenderRanges As List(Of SaveMapDialog.Range) = New List(Of Range)
        For Each r As Range In GetPatchesToBeRendered(name)
            If Not (r.first <= patch AndAlso patch <= r.last) Then 'patch should not be deleted
                RenderRanges.Add(r)
            End If
        Next
        Me._PatchesToBeRendered(name) = RenderRanges
        Me.RedrawMap()
    End Sub

    Protected Overridable Function CreateAgentPatchView(ByVal name As String) As AgentPatchController
        'If Me._PatchIndex.Count > 0 Then
        Try
            Dim view As New AgentPatchController(Me, name, Me._PatchIndex.Item(name))
            Return view
        Catch ex As Exception
            Return Nothing
        End Try
        'End If
    End Function
#End Region

#Region " Actions "

    Private Sub btnSave_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnSave.Click
        Try
            Me.Save()
        Catch ex As Exception
            MsgBox("Error occurred during Save" & vbNewLine & ex.Message, MsgBoxStyle.OkOnly, MsgBoxStyle.Exclamation)
        End Try
    End Sub

    Private Sub btnOK_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnOK.Click
        Me.DialogResult = Windows.Forms.DialogResult.Abort
        Me.Close()
    End Sub

    Private Sub btnPath_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnPath.Click
        With Me.dlgFolderBrowser
            Dim folder As String = My.Settings.LastMapSavePath
            If Not String.IsNullOrEmpty(folder) Then
                .SelectedPath = folder
            End If
            Select Case .ShowDialog
                Case Windows.Forms.DialogResult.OK
                    Me.lblPath.Text = .SelectedPath
                    Me._SaveMapPath = .SelectedPath
                    My.Settings.LastMapSavePath = .SelectedPath
                    My.Settings.Save()
                Case Else
                    Exit Sub
            End Select
        End With
    End Sub

#End Region

#Region " Redraw Map "
    Public Sub RedrawMap()
        Dim tempManifold As Manifold = New Manifold()
        Dim tempManifoldImage As ManifoldImage = New ManifoldImage(tempManifold, UvARescue.Tools.Constants.SAVE_MAP_RESOLUTION, False)
        Dim currPatch As Patch

        For Each currAgent As String In Me._TeamMembers
            For Each patches As Range In Me._PatchesToBeRendered(currAgent)
                For i As Integer = patches.first To patches.last
                    currPatch = Me._PatchIndex.Item(currAgent).Item(i)
                    tempManifold.Extend(currPatch)
                    tempManifoldImage.RenderPatch(currPatch)
                Next
            Next
        Next


        Me._Image = tempManifoldImage
        Me._ManifoldView.Image = tempManifoldImage
        Me._ManifoldView.Invalidate()
    End Sub
#End Region

End Class