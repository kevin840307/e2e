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
            ApplyDispatchParameters(fab, product, mode, allowSub)

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
            holdIfMissing = ResolveHoldIfMissing(fab, product, holdIfMissing)
            If Not holdIfMissing Then Return False
            Return Not _repo.QueryRecipe(fab, product, route)
        End Function

        Public Function IsQueueFull(fab As String, route As String, maxQueue As Integer) As Boolean
            maxQueue = ResolveMaxQueue(fab, maxQueue)
            If maxQueue <= 0 Then Return False
            Return _repo.QueryQueue(fab, route) >= maxQueue
        End Function

        Private Sub ApplyDispatchParameters(fab As String, product As String, ByRef mode As String, ByRef allowSub As Boolean)
            Dim row = _repo.QueryDispatchParam(fab, product)
            If row Is Nothing Then Return

            If V(row, "CONDITION_MODE") <> "" Then mode = V(row, "CONDITION_MODE")
            If V(row, "ALLOW_SUB_ROUTE") <> "" Then
                allowSub = String.Equals(V(row, "ALLOW_SUB_ROUTE"), "Y", StringComparison.OrdinalIgnoreCase)
            End If
        End Sub

        Private Function ResolveHoldIfMissing(fab As String, product As String, fallback As Boolean) As Boolean
            Dim row = _repo.QueryDispatchParam(fab, product)
            If row Is Nothing OrElse V(row, "HOLD_IF_NO_RECIPE") = "" Then Return fallback
            Return String.Equals(V(row, "HOLD_IF_NO_RECIPE"), "Y", StringComparison.OrdinalIgnoreCase)
        End Function

        Private Function ResolveMaxQueue(fab As String, fallback As Integer) As Integer
            Dim row = _repo.QueryDispatchParam(fab, "")
            If row Is Nothing Then Return fallback
            Dim parsed As Integer
            If Integer.TryParse(V(row, "MAX_QUEUE"), parsed) Then Return parsed
            Return fallback
        End Function

        Private Function V(row As IDictionary(Of String, String), key As String) As String
            Dim value As String = Nothing
            If row IsNot Nothing AndAlso row.TryGetValue(key, value) Then Return value
            Return ""
        End Function
    End Class
End Namespace
