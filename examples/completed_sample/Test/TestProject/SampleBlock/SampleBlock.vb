Imports System.IO
Imports Microsoft.VisualStudio.TestTools.UnitTesting

<TestClass()>
Public Class SampleBlockTests
    Inherits E2ETestBase

    <TestMethod("條件：輸入值為 10；預期：主流程寫入結果 21")>
    Public Sub NormalFlow()
        Dim dbRoot As String = NewDbRoot("NormalFlow")

        RunPrepareSql(
            "SampleBlock\SampleBlock-SOP-001\prepare.sql",
            dbRoot,
            SqlParams("INPUT_VALUE", 10))

        Dim rc As Integer = CallMain("SampleBlock")

        Assert.AreEqual(0, rc)

        Dim resultPath As String =
            System.IO.Path.Combine(dbRoot, "result.txt")

        Assert.AreEqual(
            "21",
            File.ReadAllText(resultPath).Trim())
    End Sub
End Class
