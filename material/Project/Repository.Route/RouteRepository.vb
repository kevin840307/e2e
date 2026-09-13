Imports System
Imports System.Collections.Generic
Imports FileDb

Namespace RouteRepo
    Public Class RouteRepository
        Private ReadOnly _db As FileDbSession

        Public Sub New(dbRoot As String)
            _db = New FileDbSession(dbRoot)
        End Sub

        Public Function QueryRoutePolicy(fab As String, product As String) As Dictionary(Of String, String)
            Dim sql = "SELECT * FROM ROUTE_POLICY WHERE FAB=@FAB AND PRODUCT=@PRODUCT AND ENABLED='Y'"
            Dim rows = _db.Query("ROUTE_POLICY",
                Function(r) V(r,"FAB")=fab AndAlso V(r,"PRODUCT")=product AndAlso V(r,"ENABLED")="Y", sql)
            If rows.Count = 0 Then Return Nothing
            Return rows(0)
        End Function

        Public Function QueryCondition(fab As String, product As String, name As String) As Boolean
            Dim sql = "SELECT COUNT(1) FROM ROUTE_CONDITION WHERE FAB=@FAB AND PRODUCT=@PRODUCT AND CONDITION_NAME=@NAME AND ACTIVE_YN='Y'"
            Dim rows = _db.Query("ROUTE_CONDITION",
                Function(r) V(r,"FAB")=fab AndAlso V(r,"PRODUCT")=product AndAlso V(r,"CONDITION_NAME")=name AndAlso V(r,"ACTIVE_YN")="Y", sql)
            Return rows.Count > 0
        End Function

        Public Function QueryRecipe(fab As String, product As String, route As String) As Boolean
            Dim sql = "SELECT COUNT(1) FROM RECIPE_MAPPING WHERE FAB=@FAB AND PRODUCT=@PRODUCT AND ROUTE=@ROUTE AND ACTIVE_YN='Y'"
            Dim rows = _db.Query("RECIPE_MAPPING",
                Function(r) V(r,"FAB")=fab AndAlso V(r,"PRODUCT")=product AndAlso V(r,"ROUTE")=route AndAlso V(r,"ACTIVE_YN")="Y", sql)
            Return rows.Count > 0
        End Function

        Public Function QueryQueue(fab As String, route As String) As Integer
            Dim sql = "SELECT COUNT(1) FROM LOT_QUEUE WHERE FAB=@FAB AND ROUTE=@ROUTE AND STATUS='WAIT'"
            Return _db.Query("LOT_QUEUE",
                Function(r) V(r,"FAB")=fab AndAlso V(r,"ROUTE")=route AndAlso V(r,"STATUS")="WAIT", sql).Count
        End Function

        Public Sub InsertAudit(lotId As String, action As String, route As String, reason As String)
            _db.Insert("DISPATCH_AUDIT",
                New Dictionary(Of String, String) From {
                    {"LOT_ID", lotId}, {"ACTION", action}, {"ROUTE", route}, {"REASON", reason}
                },
                "INSERT INTO DISPATCH_AUDIT(...) VALUES(...)")
        End Sub

        Public Sub InsertHold(lotId As String, reason As String)
            _db.Insert("LOT_HOLD",
                New Dictionary(Of String, String) From {{"LOT_ID", lotId},{"REASON", reason}},
                "INSERT INTO LOT_HOLD(LOT_ID,REASON) VALUES(@LOT_ID,@REASON)")
        End Sub

        Private Function V(row As IDictionary(Of String, String), key As String) As String
            Dim value As String = Nothing
            If row.TryGetValue(key, value) Then Return value
            Return ""
        End Function
    End Class
End Namespace
