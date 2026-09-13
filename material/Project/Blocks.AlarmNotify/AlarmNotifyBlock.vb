Imports Workflow
Imports HttpMock
Imports Logging

Namespace AlarmNotify
    Public Class AlarmNotifyBlock
        Inherits WorkflowBlockBase

        Public Overrides ReadOnly Property Name As String
            Get
                Return "AlarmNotify"
            End Get
        End Property

        Public Overrides Sub Execute(context As WorkflowContext)
            Dim url = Param(Of String)("url","http://mock/alarm")
            Dim message = Param(Of String)("message","")
            Dim client As New FakeHttpClient()
            Dim status = client.Post(url, message)
            context.SetValue("AlarmNotify.HttpStatus", status)
            Dim logger As New AppLogger()
            logger.Info("Alarm HTTP status=" & status.ToString())
        End Sub
    End Class
End Namespace
