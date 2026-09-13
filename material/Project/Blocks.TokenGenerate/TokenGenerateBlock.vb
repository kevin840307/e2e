Imports Workflow
Imports CommandBusiness
Imports CommandRepo

Namespace TokenGenerate
    Public Class TokenGenerateBlock
        Inherits WorkflowBlockBase

        Public Overrides ReadOnly Property Name As String
            Get
                Return "TokenGenerate"
            End Get
        End Property

        Public Overrides Sub Execute(context As WorkflowContext)
            Dim dbRoot = Param(Of String)("dbRoot","runtime_db")
            Dim correlationId = Param(Of String)("correlationId","")
            Dim svc As New CommandService(New CommandRepository(dbRoot))
            Dim token = svc.GenerateToken(correlationId)
            context.SetValue("Command.Token", token)
        End Sub
    End Class
End Namespace
