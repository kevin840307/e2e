Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports TestInfrastructure

<TestClass>
Public Class XXXTests
    Inherits E2ETestBase

    <TestMethod("條件：描述 before-state 與 external boundary；預期：正式 workflow 產生可觀察結果")>
    Public Sub XXX_SOP_001()
        Dim dbRoot = NewDbRoot("XXX-SOP-001")

        ' Optional external boundary only. Fixture must belong to this SOP.
        ' UseApiMock("XXX", "XXX-SOP-001", "mock_api_response.json")
        ' UseMqMock("XXX", "XXX-SOP-001", "mock_mq_response.json")

        RunPrepareSql(
            "XXX\XXX-SOP-001\prepare.sql",
            dbRoot,
            SqlParams("INPUT_ID", "CASE001")
        )

        Dim rc = CallMain()
        Assert.AreEqual(0, rc)
    End Sub
End Class
