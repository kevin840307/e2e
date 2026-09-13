Imports Workflow
Imports CommandBusiness
Imports CommandRepo
Imports MqMock

Namespace CommandSubmit
    Public Class CommandSubmitBlock
        Inherits WorkflowBlockBase

        Public Overrides ReadOnly Property Name As String
            Get
                Return "CommandSubmit"
            End Get
        End Property

        Public Overrides Sub Execute(context As WorkflowContext)
            Dim dbRoot = Param(Of String)("dbRoot", "runtime_db")
            Dim command = Param(Of String)("command", "")
            Dim mqTopic = Param(Of String)("mqTopic", "command.submit")
            Dim token = context.GetValue(Of String)("Command.Token", "")

            Dim svc As New CommandService(New CommandRepository(dbRoot))
            If token = "" OrElse Not svc.ValidateToken(token) Then
                context.SetValue("CommandSubmit.Result", "INVALID_TOKEN")
                Return
            End If

            svc.SaveCommand(token, command)

            Dim mq As New FakeMqClient()
            Dim ack = mq.Publish(mqTopic, command)
            context.SetValue("CommandSubmit.MqAck", ack)
            context.SetValue("CommandSubmit.Result", If(ack, "SUBMITTED", "MQ_FAILED"))
        End Sub
    End Class
End Namespace
