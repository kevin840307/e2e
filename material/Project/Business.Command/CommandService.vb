Imports System
Imports CommandRepo

Namespace CommandBusiness
    Public Class CommandService
        Private ReadOnly _repo As CommandRepository
        Public Sub New(repo As CommandRepository)
            _repo = repo
        End Sub

        Public Function GenerateToken(correlationId As String) As String
            If String.Equals(correlationId, "INVALID", StringComparison.OrdinalIgnoreCase) Then Return ""
            Dim token = correlationId & "-" & DateTime.Now.Ticks.ToString()
            _repo.SaveToken(correlationId, token)
            Return token
        End Function

        Public Function ValidateToken(token As String) As Boolean
            Return _repo.TokenExists(token)
        End Function

        Public Sub SaveCommand(token As String, command As String)
            _repo.SaveCommand(token, command)
        End Sub
    End Class
End Namespace
