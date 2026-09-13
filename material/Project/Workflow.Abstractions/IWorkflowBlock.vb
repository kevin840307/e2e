Imports System.Collections.Generic

Namespace Workflow
    Public Interface IWorkflowBlock
        ReadOnly Property Name As String
        Property Parameters As IDictionary(Of String, Object)
        Sub Execute(context As WorkflowContext)
    End Interface
End Namespace
