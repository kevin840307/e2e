Imports System
Imports System.Collections.Generic
Imports RouteRepo

Namespace RouteBusiness
    Public Class RouteDecisionService
        Private ReadOnly _repo As RouteRepository

        Public Sub New(repo As RouteRepository)
            _repo = repo
        End Sub

        Public Function ResolveRoute(fab As String, product As String, mode As String, allowSub As Boolean) As String
            Dim policy = _repo.QueryRoutePolicy(fab, product)
            If policy Is Nothing Then Return ""

            Dim mainRoute = policy("MAIN_ROUTE")
            Dim subRoute = policy("SUB_ROUTE")
            Dim selected = mainRoute

            If allowSub AndAlso _repo.QueryCondition(fab, product, "USE_SUB_ROUTE") Then
                selected = subRoute
            End If

            If String.Equals(mode, "FORCE_MAIN", StringComparison.OrdinalIgnoreCase) Then
                selected = mainRoute
            ElseIf String.Equals(mode, "FORCE_SUB", StringComparison.OrdinalIgnoreCase) AndAlso allowSub Then
                selected = subRoute
            End If

            Return selected
        End Function

        Public Function ShouldHoldForRecipe(fab As String, product As String, route As String, holdIfMissing As Boolean) As Boolean
            If Not holdIfMissing Then Return False
            Return Not _repo.QueryRecipe(fab, product, route)
        End Function

        Public Function IsQueueFull(fab As String, route As String, maxQueue As Integer) As Boolean
            If maxQueue <= 0 Then Return False
            Return _repo.QueryQueue(fab, route) >= maxQueue
        End Function
    End Class
End Namespace
