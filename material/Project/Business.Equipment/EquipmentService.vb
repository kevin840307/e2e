Imports System.Collections.Generic
Imports EquipmentRepo

Namespace EquipmentBusiness
    Public Class EquipmentService
        Private ReadOnly _repo As EquipmentRepository
        Public Sub New(repo As EquipmentRepository)
            _repo = repo
        End Sub

        Public Function Evaluate(eqId As String, requiredMode As String) As String
            Dim row = _repo.QueryEquipment(eqId)
            If row Is Nothing Then Return "NOT_FOUND"
            If row("STATE") <> "RUN" Then Return "NOT_RUN"
            If requiredMode <> "" AndAlso row("MODE") <> requiredMode Then Return "MODE_MISMATCH"
            Return "READY"
        End Function
    End Class
End Namespace
