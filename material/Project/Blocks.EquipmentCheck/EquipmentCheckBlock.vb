Imports Workflow
Imports EquipmentRepo
Imports EquipmentBusiness

Namespace EquipmentCheck
    Public Class EquipmentCheckBlock
        Inherits WorkflowBlockBase

        Public Overrides ReadOnly Property Name As String
            Get
                Return "EquipmentCheck"
            End Get
        End Property

        Public Overrides Sub Execute(context As WorkflowContext)
            Dim dbRoot = Param(Of String)("dbRoot","runtime_db")
            Dim eqId = Param(Of String)("eqId","")
            Dim requiredMode = Param(Of String)("requiredMode","")

            Dim svc As New EquipmentService(New EquipmentRepository(dbRoot))
            context.SetValue("EquipmentCheck.Result", svc.Evaluate(eqId, requiredMode))
        End Sub
    End Class
End Namespace
