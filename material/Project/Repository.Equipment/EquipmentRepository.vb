Imports System.Collections.Generic
Imports FileDb

Namespace EquipmentRepo
    Public Class EquipmentRepository
        Private ReadOnly _db As FileDbSession
        Public Sub New(dbRoot As String)
            _db = New FileDbSession(dbRoot)
        End Sub

        Public Function QueryEquipment(eqId As String) As Dictionary(Of String, String)
            Dim sql = "SELECT * FROM EQUIPMENT WHERE EQ_ID=@EQ_ID"
            Dim rows = _db.Query("EQUIPMENT",
                Function(r) GetV(r,"EQ_ID")=eqId, sql)
            If rows.Count=0 Then Return Nothing
            Return rows(0)
        End Function

        Private Function GetV(row As IDictionary(Of String,String), key As String) As String
            Dim v As String = Nothing
            If row.TryGetValue(key,v) Then Return v
            Return ""
        End Function
    End Class
End Namespace
