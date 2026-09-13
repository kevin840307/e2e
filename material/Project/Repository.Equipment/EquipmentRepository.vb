Imports System.Collections.Generic
Imports FileDb

Namespace EquipmentRepo
    Public Class EquipmentRepository
        Private ReadOnly _db As FileDbSession
        Private ReadOnly _paramDb As FileDbSession

        Public Sub New(dbRoot As String, Optional paramDbRoot As String = Nothing)
            _db = New FileDbSession(dbRoot)
            _paramDb = New FileDbSession(If(String.IsNullOrWhiteSpace(paramDbRoot), dbRoot, paramDbRoot))
        End Sub

        Public Function QueryEquipment(eqId As String) As Dictionary(Of String, String)
            Dim sql = "SELECT * FROM EQUIPMENT WHERE EQ_ID=@EQ_ID"
            Dim rows = _db.Query("EQUIPMENT",
                Function(r) GetV(r,"EQ_ID")=eqId, sql)
            If rows.Count=0 Then Return Nothing
            Return rows(0)
        End Function

        Public Function QueryEquipmentRule(eqId As String, requiredMode As String) As Dictionary(Of String, String)
            Dim sql = "SELECT * FROM EQUIPMENT_RULE WHERE EQ_ID IN (@EQ_ID,'DEFAULT') AND REQUIRED_MODE=@MODE AND ACTIVE_YN='Y'"
            Dim rows = _paramDb.Query("EQUIPMENT_RULE",
                Function(r)
                    Dim ruleEq = GetV(r, "EQ_ID")
                    Return (ruleEq = eqId OrElse ruleEq = "DEFAULT") AndAlso
                        GetV(r, "REQUIRED_MODE") = requiredMode AndAlso
                        GetV(r, "ACTIVE_YN") = "Y"
                End Function,
                sql)

            For Each row In rows
                If GetV(row, "EQ_ID") = eqId Then Return row
            Next

            If rows.Count = 0 Then Return Nothing
            Return rows(0)
        End Function

        Public Function QueryMaintenanceWindow(eqId As String) As Boolean
            Dim sql = "SELECT COUNT(1) FROM EQUIPMENT_MAINTENANCE WHERE EQ_ID=@EQ_ID AND ACTIVE_YN='Y'"
            Return _paramDb.Query("EQUIPMENT_MAINTENANCE",
                Function(r) GetV(r, "EQ_ID") = eqId AndAlso GetV(r, "ACTIVE_YN") = "Y",
                sql).Count > 0
        End Function

        Private Function GetV(row As IDictionary(Of String,String), key As String) As String
            Dim v As String = Nothing
            If row.TryGetValue(key,v) Then Return v
            Return ""
        End Function
    End Class
End Namespace
