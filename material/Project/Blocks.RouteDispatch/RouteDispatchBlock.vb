Imports System
Imports Workflow
Imports RouteBusiness
Imports RouteRepo
Imports Logging

Namespace RouteDispatch
    Public Class RouteDispatchBlock
        Inherits WorkflowBlockBase

        Public Overrides ReadOnly Property Name As String
            Get
                Return "RouteDispatch"
            End Get
        End Property

        Public Overrides Sub Execute(context As WorkflowContext)
            Dim dbRoot = Param(Of String)("dbRoot","runtime_db")
            Dim paramDbRoot = Param(Of String)("paramDbRoot", dbRoot)
            Dim fab = Param(Of String)("fab","FAB1")
            Dim lotId = Param(Of String)("lotId","LOT-UNKNOWN")
            Dim product = Param(Of String)("product","")
            Dim mode = Param(Of String)("conditionMode","AUTO")
            Dim allowSub = Param(Of Boolean)("allowSubRoute", True)
            Dim holdIfNoRecipe = Param(Of Boolean)("holdIfNoRecipe", True)
            Dim maxQueue = Param(Of Integer)("maxQueue", 10)

            Dim logger As New AppLogger()
            Dim repo As New RouteRepository(dbRoot, paramDbRoot)
            Dim svc As New RouteDecisionService(repo)

            Dim route = svc.ResolveRoute(fab, product, mode, allowSub)
            If route = "" Then
                Finish(context, repo, lotId, "ERROR", "", "NO_ROUTE_POLICY")
                Return
            End If

            If svc.ShouldHoldForRecipe(fab, product, route, holdIfNoRecipe) Then
                Finish(context, repo, lotId, "HOLD", route, "RECIPE_NOT_FOUND")
                Return
            End If

            If svc.IsQueueFull(fab, route, maxQueue) Then
                Finish(context, repo, lotId, "HOLD", route, "QUEUE_LIMIT")
                Return
            End If

            logger.Info("Dispatch " & lotId & " -> " & route)
            Finish(context, repo, lotId, "DISPATCH", route, "OK")
        End Sub

        Private Sub Finish(context As WorkflowContext, repo As RouteRepository, lotId As String,
                           action As String, route As String, reason As String)
            context.SetValue("RouteDispatch.Action", action)
            context.SetValue("RouteDispatch.Route", route)
            context.SetValue("RouteDispatch.Reason", reason)
            repo.InsertAudit(lotId, action, route, reason)
        End Sub
    End Class
End Namespace
