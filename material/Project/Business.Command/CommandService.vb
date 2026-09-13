Imports System
Imports System.Collections.Generic
Imports CommandRepo

Namespace CommandBusiness
    Public Class CommandService
        Private ReadOnly _repo As CommandRepository
        Public Sub New(repo As CommandRepository)
            _repo = repo
        End Sub

        Public Function GenerateToken(correlationId As String) As String
            If String.Equals(correlationId, "INVALID", StringComparison.OrdinalIgnoreCase) Then Return ""
            Dim policy = _repo.QueryTokenPolicy(correlationId)
            If policy Is Nothing OrElse Not IsEnabled(policy, "ALLOW_YN") Then Return ""

            Dim prefix = V(policy, "TOKEN_PREFIX")
            If String.IsNullOrWhiteSpace(prefix) Then prefix = "CMD"

            Dim token = prefix & "-" & correlationId & "-" & DateTime.Now.Ticks.ToString()
            _repo.SaveToken(correlationId, token)
            Return token
        End Function

        Public Function ValidateToken(token As String) As Boolean
            Return _repo.TokenExists(token)
        End Function

        Public Function ValidateCommand(command As String) As Boolean
            Return _repo.QueryCommandRule(command)
        End Function

        Public Function ResolveMqTopic(endpointName As String, fallbackTopic As String) As String
            Dim topic = _repo.QueryMqEndpoint(endpointName)
            If String.IsNullOrWhiteSpace(topic) Then Return fallbackTopic
            Return topic
        End Function

        Public Sub SaveCommand(token As String, command As String)
            _repo.SaveCommand(token, command)
        End Sub

        Private Function IsEnabled(row As IDictionary(Of String, String), key As String) As Boolean
            Return String.Equals(V(row, key), "Y", StringComparison.OrdinalIgnoreCase)
        End Function

        Private Function V(row As IDictionary(Of String, String), key As String) As String
            Dim value As String = Nothing
            If row IsNot Nothing AndAlso row.TryGetValue(key, value) Then Return value
            Return ""
        End Function
    End Class
End Namespace
