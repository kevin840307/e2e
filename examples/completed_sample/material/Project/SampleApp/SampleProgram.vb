Imports System.IO

Public Class SampleProgram
    Public Shared Function Main(blockName As String, dbRoot As String) As Integer
        If blockName <> "SampleBlock" Then
            Return 2
        End If

        Dim inputPath As String =
            System.IO.Path.Combine(dbRoot, "input.txt")

        Dim inputValue As Integer =
            Integer.Parse(File.ReadAllText(inputPath).Trim())

        Dim engine As New SampleBlockEngine()
        Dim resultValue As Integer = engine.Execute(inputValue)

        Dim resultPath As String =
            System.IO.Path.Combine(dbRoot, "result.txt")

        File.WriteAllText(
            resultPath,
            resultValue.ToString())

        Return 0
    End Function
End Class
