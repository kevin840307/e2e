Imports System
Imports System.Collections.Generic

Namespace Workflow
    Public Class WorkflowContext
        Private ReadOnly _values As New Dictionary(Of String, Object)(StringComparer.OrdinalIgnoreCase)

        Public Property IsStopped As Boolean
        Public Property StopReason As String

        Public Sub SetValue(key As String, value As Object)
            _values(key) = value
        End Sub

        Public Function GetValue(key As String) As Object
            Dim value As Object = Nothing
            If _values.TryGetValue(key, value) Then Return value
            Return Nothing
        End Function

        Public Function GetValue(Of T)(key As String, Optional defaultValue As T = Nothing) As T
            Dim value = GetValue(key)
            If value Is Nothing Then Return defaultValue
            If GetType(T) Is GetType(String) Then Return CType(CType(value.ToString(), Object), T)
            Return CType(Convert.ChangeType(value, GetType(T)), T)
        End Function

        Public Function Contains(key As String) As Boolean
            Return _values.ContainsKey(key)
        End Function
    End Class
End Namespace
