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
            Dim paramDbRoot = Param(Of String)("paramDbRoot", dbRoot)
            Dim lotId = Param(Of String)("lotId","")
            Dim reason = Param(Of String)("reason","MANUAL_HOLD")
            Dim fab = Param(Of String)("fab", "FAB1")
            Dim product = Param(Of String)("product", "")

            Dim repo As New RouteRepository(dbRoot, paramDbRoot)
            Dim lot = repo.QueryLotState(lotId)
            If lot Is Nothing Then
                context.SetValue("HoldLot.Result","LOT_NOT_FOUND")
                Return
            End If

            If product = "" AndAlso lot.ContainsKey("PRODUCT") Then product = lot("PRODUCT")

            Dim policy = repo.QueryHoldPolicy(fab, product, reason)
            If policy Is Nothing OrElse Not String.Equals(policy("ALLOW_YN"), "Y", System.StringComparison.OrdinalIgnoreCase) Then
                context.SetValue("HoldLot.Result","HOLD_NOT_ALLOWED")
                Return
            End If

            repo.InsertHold(lotId, reason)

            Dim logger As New AppLogger()
            logger.Warn("Hold lot " & lotId & ": " & reason)
            context.SetValue("HoldLot.Result","HELD")
        End Sub
    End Class
End Namespace
