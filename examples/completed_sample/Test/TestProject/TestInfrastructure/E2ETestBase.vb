Imports System
Imports System.Collections.Generic
Imports System.IO
Imports System.Text.RegularExpressions

Public MustInherit Class E2ETestBase
    Private _dbRoot As String

    Protected Function NewDbRoot(caseName As String) As String
        _dbRoot = System.IO.Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory,
            "db",
            caseName)

        If Directory.Exists(_dbRoot) Then
            Directory.Delete(_dbRoot, True)
        End If

        Directory.CreateDirectory(_dbRoot)
        Return _dbRoot
    End Function

    Protected Function SqlParams(
        ParamArray pairs() As Object
    ) As IDictionary(Of String, Object)

        Dim result As New Dictionary(Of String, Object)()

        For i As Integer = 0 To pairs.Length - 1 Step 2
            result(CStr(pairs(i))) = pairs(i + 1)
        Next

        Return result
    End Function

    Protected Sub RunPrepareSql(
        filePath As String,
        dbRoot As String,
        parameters As IDictionary(Of String, Object))

        Dim projectRoot As String =
            System.IO.Path.GetFullPath(
                System.IO.Path.Combine(
                    AppDomain.CurrentDomain.BaseDirectory,
                    "..\..\"))

        Dim sql As String =
            File.ReadAllText(
                System.IO.Path.Combine(projectRoot, filePath))

        For Each pair As KeyValuePair(Of String, Object) In parameters
            sql = sql.Replace(
                "{{" & pair.Key & "}}",
                CStr(pair.Value))
        Next

        Dim match As Match =
            Regex.Match(
                sql,
                "VALUES\s*\(\s*(\d+)\s*\)",
                RegexOptions.IgnoreCase)

        If Not match.Success Then
            Throw New Exception("Invalid prepare.sql")
        End If

        File.WriteAllText(
            System.IO.Path.Combine(dbRoot, "input.txt"),
            match.Groups(1).Value)
    End Sub

    Protected Function CallMain(blockName As String) As Integer
        Return SampleProgram.Main(blockName, _dbRoot)
    End Function
End Class
