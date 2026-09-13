Imports Workflow
Imports RouteRepo
Imports Logging

Namespace HoldLot
    Public Class HoldLotBlock
        Inherits WorkflowBlockBase

        Public Overrides ReadOnly Property Name As String
            Get
                Return "HoldLot"
            End Get
        End Property

        Public Overrides Sub Execute(context As WorkflowContext)
            Dim dbRoot = Param(Of String)("dbRoot","runtime_db")
            Dim lotId = Param(Of String)("lotId","")
            Dim reason = Param(Of String)("reason","MANUAL_HOLD")

            Dim repo As New RouteRepository(dbRoot)
            repo.InsertHold(lotId, reason)

            Dim logger As New AppLogger()
            logger.Warn("Hold lot " & lotId & ": " & reason)
            context.SetValue("HoldLot.Result","HELD")
        End Sub
    End Class
End Namespace
