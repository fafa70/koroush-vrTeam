
Imports System.Drawing
Imports System.IO
Imports System.Math
Imports System.Windows.Forms
Imports System.Text
Imports UvARescue.Agent
Imports UvARescue.Slam
Imports UvARescue.Tools
Imports UvARescue.Math
Imports UvARescue.ImageAnalysis

Public Class mainController
    Inherits Windows.Forms.Form

    Private AgentControllers As New List(Of AgentController)
    Private listOfPanels As New List(Of Panel)
    Private _agentNumber As Integer
    Private counter As Integer = 0

    Sub New()
        Me.Size = New Drawing.Size(750, 300)
        Show()

    End Sub


    Public Sub createAgent(ByVal agenthandler As AgentController,ByVal agentnumber As integer)
        Me._agentNumber = agentnumber
        'Console.WriteLine("[mainController]:agent number {0} fafafafafafa", agentnumber)
        Me.AgentControllers.Insert(counter, agenthandler)
        Me.listOfPanels.Insert(counter, New Panel With {.Size = New System.Drawing.Size(750, 300)})
        Me.listOfPanels(counter).Controls.Add(Me.AgentControllers(counter))
        counter = counter + 1
    End Sub


    Public Sub showCurrController(ByVal agentnumber As Integer)

        Me.Controls.Add(Me.AgentControllers(agentnumber - 1))
        '        Next


        'If (agentnumber = 1) Then
        'Me.Controls.Add(Me.listOfPanels(0))
        'ElseIf (agentnumber = 2) Then
        'Me.Controls.Add(Me.listOfPanels(1))
        'Else
        'Me.Controls.Add(Me.listOfPanels(1))
        'End If


    End Sub



End Class
