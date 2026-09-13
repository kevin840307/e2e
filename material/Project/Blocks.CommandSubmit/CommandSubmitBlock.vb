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
            Dim paramDbRoot = Param(Of String)("paramDbRoot", dbRoot)
            Dim masterDbRoot = Param(Of String)("masterDbRoot", dbRoot)
            Dim command = Param(Of String)("command", "")
            Dim mqTopic = Param(Of String)("mqTopic", "command.submit")
            Dim mqEndpoint = Param(Of String)("mqEndpoint", "COMMAND_SUBMIT")
            Dim token = context.GetValue(Of String)("Command.Token", "")

            Dim svc As New CommandService(New CommandRepository(dbRoot, paramDbRoot, masterDbRoot))
            If token = "" OrElse Not svc.ValidateToken(token) Then
                context.SetValue("CommandSubmit.Result", "INVALID_TOKEN")
                Return
            End If

            If Not svc.ValidateCommand(command) Then
                context.SetValue("CommandSubmit.Result", "COMMAND_NOT_ALLOWED")
                Return
            End If

            Dim topic = svc.ResolveMqTopic(mqEndpoint, mqTopic)
            svc.SaveCommand(token, command)

            Dim mq As New FakeMqClient()
            Dim ack = mq.Publish(topic, command)
            context.SetValue("CommandSubmit.MqAck", ack)
            context.SetValue("CommandSubmit.Result", If(ack, "SUBMITTED", "MQ_FAILED"))
        End Sub
    End Class
End Namespace
