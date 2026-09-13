Imports System.Collections.Generic
Imports EquipmentRepo

Namespace EquipmentBusiness
    Public Class EquipmentService
        Private ReadOnly _repo As EquipmentRepository
        Public Sub New(repo As EquipmentRepository)
            _repo = repo
        End Sub

        Public Function Evaluate(eqId As String, requiredMode As String) As String
            If IsUnderMaintenance(eqId) Then Return "MAINTENANCE"

            Dim row = _repo.QueryEquipment(eqId)
            If row Is Nothing Then Return "NOT_FOUND"
            If row("STATE") <> "RUN" Then Return "NOT_RUN"
            If IsBlockedByRule(eqId, requiredMode) Then Return "MODE_BLOCKED"
            If requiredMode <> "" AndAlso row("MODE") <> requiredMode Then Return "MODE_MISMATCH"
            Return "READY"
        End Function

        Private Function IsUnderMaintenance(eqId As String) As Boolean
            Return _repo.QueryMaintenanceWindow(eqId)
        End Function

        Private Function IsBlockedByRule(eqId As String, requiredMode As String) As Boolean
            If requiredMode = "" Then Return False
            Dim rule = _repo.QueryEquipmentRule(eqId, requiredMode)
            If rule Is Nothing Then Return False
            Return rule.ContainsKey("ALLOW_YN") AndAlso rule("ALLOW_YN") <> "Y"
        End Function
    End Class
End Namespace
