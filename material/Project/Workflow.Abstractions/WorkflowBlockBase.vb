Imports System
Imports System.Collections.Generic

Namespace Workflow
    Public MustInherit Class WorkflowBlockBase
        Implements IWorkflowBlock

        Public MustOverride ReadOnly Property Name As String Implements IWorkflowBlock.Name

        Public Property Parameters As IDictionary(Of String, Object) =
            New Dictionary(Of String, Object)(StringComparer.OrdinalIgnoreCase) _
            Implements IWorkflowBlock.Parameters

        Public MustOverride Sub Execute(context As WorkflowContext) Implements IWorkflowBlock.Execute

        Protected Function Param(Of T)(name As String, Optional defaultValue As T = Nothing) As T
            Dim value As Object = Nothing
            If Not Parameters.TryGetValue(name, value) OrElse value Is Nothing Then Return defaultValue
            If GetType(T) Is GetType(String) Then Return CType(CType(value.ToString(), Object), T)
            Return CType(Convert.ChangeType(value, GetType(T)), T)
        End Function
    End Class
End Namespace
