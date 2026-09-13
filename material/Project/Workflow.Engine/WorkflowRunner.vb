Imports System
Imports Workflow

Namespace Engine
    Public Class WorkflowRunner
        Public Sub Run(definition As WorkflowDefinition, context As WorkflowContext)
            If definition Is Nothing Then Throw New ArgumentNullException("definition")
            If context Is Nothing Then Throw New ArgumentNullException("context")

            For Each block In definition.Blocks
                If context.IsStopped Then Exit For
                block.Execute(context)
            Next
        End Sub
    End Class
End Namespace
