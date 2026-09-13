Imports Workflow
Imports HttpMock
Imports Logging
Imports FileDb

Namespace AlarmNotify
    Public Class AlarmNotifyBlock
        Inherits WorkflowBlockBase

        Public Overrides ReadOnly Property Name As String
            Get
                Return "AlarmNotify"
            End Get
        End Property

        Public Overrides Sub Execute(context As WorkflowContext)
            Dim dbRoot = Param(Of String)("dbRoot", "runtime_db")
            Dim configDbRoot = Param(Of String)("configDbRoot", dbRoot)
            Dim channel = Param(Of String)("channel", "DEFAULT")
            Dim url = Param(Of String)("url", "http://mock/alarm")
            Dim message = Param(Of String)("message", "")

            ResolveChannel(configDbRoot, channel, url, message)

            Dim client As New FakeHttpClient()
            Dim status = client.Post(url, message)
            context.SetValue("AlarmNotify.HttpStatus", status)
            Dim logger As New AppLogger()
            logger.Info("Alarm HTTP status=" & status.ToString())
        End Sub

        Private Sub ResolveChannel(configDbRoot As String, channel As String, ByRef url As String, ByRef message As String)
            Dim db As New FileDbSession(configDbRoot)
            ApplyChannel(db, channel, url)
            ApplyTemplate(db, channel, message)
        End Sub

        Private Sub ApplyChannel(db As FileDbSession, channel As String, ByRef url As String)
            Dim rows = db.Query("ALARM_CHANNEL",
                Function(r) V(r, "CHANNEL") = channel AndAlso V(r, "ACTIVE_YN") = "Y",
                "SELECT * FROM ALARM_CHANNEL WHERE CHANNEL=@CHANNEL AND ACTIVE_YN='Y'")
            If rows.Count > 0 AndAlso V(rows(0), "URL") <> "" Then url = V(rows(0), "URL")
        End Sub

        Private Sub ApplyTemplate(db As FileDbSession, channel As String, ByRef message As String)
            Dim rows = db.Query("ALARM_TEMPLATE",
                Function(r) V(r, "CHANNEL") = channel AndAlso V(r, "ACTIVE_YN") = "Y",
                "SELECT * FROM ALARM_TEMPLATE WHERE CHANNEL=@CHANNEL AND ACTIVE_YN='Y'")
            If rows.Count > 0 AndAlso V(rows(0), "BODY_TEMPLATE") <> "" Then
                message = V(rows(0), "BODY_TEMPLATE").Replace("{MESSAGE}", message)
            End If
        End Sub

        Private Function V(row As IDictionary(Of String, String), key As String) As String
            Dim value As String = Nothing
            If row IsNot Nothing AndAlso row.TryGetValue(key, value) Then Return value
            Return ""
        End Function
    End Class
End Namespace
