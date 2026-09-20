Imports System
Imports System.Collections.Generic
Imports System.Data
Imports System.Text
Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports TestInfrastructure

<TestClass>
Public Class RouteDispatch

    <TestMethod>
    Public Sub RouteDispatch_SOP_001()
        Dim dbRoot As String = NewDbRoot("RouteDispatch")

        Try
            PrepareSql(dbRoot)

            Dim result As Integer = CallMain("RouteDispatch")

            Assert.IsTrue(result = 0, "CallMain should return 0")
            AssertPostState(dbRoot)
        Finally
            Cleanup(dbRoot)
        End Try
    End Sub

    Private Sub PrepareSql(dbRoot As String)
        RunPrepareSql("RouteDispatch\RouteDispatch-SOP-001\prepare.sql", dbRoot)
    End Sub

    Private Sub AssertPostState(dbRoot As String)
        ' Assert observable production post-state here
        ' Example: Verify E2E_SOP_CONTROL status or other produced data
    End Sub

    Private Sub Cleanup(dbRoot As String)
        ' Cleanup logic if needed
    End Sub
End Class