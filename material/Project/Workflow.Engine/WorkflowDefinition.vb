Imports System.Collections.Generic
Imports Workflow

Namespace Engine
    Public Class WorkflowDefinition
        Public Property Name As String
        Public Property Blocks As New List(Of IWorkflowBlock)()
    End Class
End Namespace
