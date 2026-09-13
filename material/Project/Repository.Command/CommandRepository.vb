Imports System.Collections.Generic
Imports FileDb

Namespace CommandRepo
    Public Class CommandRepository
        Private ReadOnly _db As FileDbSession
        Public Sub New(dbRoot As String)
            _db = New FileDbSession(dbRoot)
        End Sub

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
    End Class
End Namespace
