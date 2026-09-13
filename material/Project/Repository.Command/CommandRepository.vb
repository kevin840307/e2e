Imports System.Collections.Generic
Imports FileDb

Namespace CommandRepo
    Public Class CommandRepository
        Private ReadOnly _db As FileDbSession
        Private ReadOnly _paramDb As FileDbSession
        Private ReadOnly _masterDb As FileDbSession

        Public Sub New(
            dbRoot As String,
            Optional paramDbRoot As String = Nothing,
            Optional masterDbRoot As String = Nothing)

            _db = New FileDbSession(dbRoot)
            _paramDb = New FileDbSession(If(String.IsNullOrWhiteSpace(paramDbRoot), dbRoot, paramDbRoot))
            _masterDb = New FileDbSession(If(String.IsNullOrWhiteSpace(masterDbRoot), dbRoot, masterDbRoot))
        End Sub

        Public Function QueryTokenPolicy(correlationId As String) As Dictionary(Of String, String)
            Dim sql = "SELECT * FROM COMMAND_TOKEN_POLICY WHERE CORRELATION_ID IN (@ID,'DEFAULT') AND ACTIVE_YN='Y'"
            Dim rows = _paramDb.Query("COMMAND_TOKEN_POLICY",
                Function(r)
                    Dim id = V(r, "CORRELATION_ID")
                    Return (id = correlationId OrElse id = "DEFAULT") AndAlso V(r, "ACTIVE_YN") = "Y"
                End Function,
                sql)

            For Each row In rows
                If V(row, "CORRELATION_ID") = correlationId Then Return row
            Next

            If rows.Count = 0 Then Return Nothing
            Return rows(0)
        End Function

        Public Function QueryCommandRule(command As String) As Boolean
            Dim sql = "SELECT COUNT(1) FROM COMMAND_RULE WHERE COMMAND=@COMMAND AND ENABLED_YN='Y'"
            Return _paramDb.Query("COMMAND_RULE",
                Function(r) V(r, "COMMAND") = command AndAlso V(r, "ENABLED_YN") = "Y",
                sql).Count > 0
        End Function

        Public Function QueryMqEndpoint(endpointName As String) As String
            Dim sql = "SELECT TOP 1 TOPIC FROM MQ_ENDPOINT WHERE ENDPOINT_NAME=@ENDPOINT AND ACTIVE_YN='Y'"
            Dim rows = _masterDb.Query("MQ_ENDPOINT",
                Function(r) V(r, "ENDPOINT_NAME") = endpointName AndAlso V(r, "ACTIVE_YN") = "Y",
                sql)
            If rows.Count = 0 Then Return ""
            Return V(rows(0), "TOPIC")
        End Function

        Public Sub SaveToken(correlationId As String, token As String)
            _db.Insert("COMMAND_TOKEN",
                New Dictionary(Of String,String) From {{"CORRELATION_ID",correlationId},{"TOKEN",token}},
                "INSERT INTO COMMAND_TOKEN(CORRELATION_ID,TOKEN) VALUES(@ID,@TOKEN)")
        End Sub

        Public Function TokenExists(token As String) As Boolean
            Return _db.Query("COMMAND_TOKEN",
                Function(r)
                    Dim v As String = Nothing
                    Return r.TryGetValue("TOKEN",v) AndAlso v=token
                End Function,
                "SELECT COUNT(1) FROM COMMAND_TOKEN WHERE TOKEN=@TOKEN").Count > 0
        End Function

        Public Sub SaveCommand(token As String, command As String)
            _db.Insert("COMMAND_AUDIT",
                New Dictionary(Of String,String) From {{"TOKEN",token},{"COMMAND",command}},
                "INSERT INTO COMMAND_AUDIT(TOKEN,COMMAND) VALUES(@TOKEN,@COMMAND)")
        End Sub

        Private Function V(row As IDictionary(Of String, String), key As String) As String
            Dim value As String = Nothing
            If row IsNot Nothing AndAlso row.TryGetValue(key, value) Then Return value
            Return ""
        End Function
    End Class
End Namespace
