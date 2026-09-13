Imports System
Imports System.Collections.Generic
Imports System.IO
Imports System.Linq
Imports System.Text

Namespace FileDb
    Public Class FileDbSession
        Private ReadOnly _root As String

        Public Sub New(rootFolder As String)
            _root = System.IO.Path.GetFullPath(rootFolder)
            Directory.CreateDirectory(_root)
            Directory.CreateDirectory(System.IO.Path.Combine(_root, "_sql_log"))
        End Sub

        Public Sub Insert(tableName As String, row As IDictionary(Of String, String), sqlText As String)
            LogSql("INSERT", tableName, sqlText)
            Dim rows As List(Of Dictionary(Of String, String)) = LoadTable(tableName)
            rows.Add(New Dictionary(Of String, String)(row, StringComparer.OrdinalIgnoreCase))
            SaveTable(tableName, rows)
        End Sub

        Public Function Query(tableName As String,
                              predicate As Func(Of IDictionary(Of String, String), Boolean),
                              sqlText As String) As List(Of Dictionary(Of String, String))
            LogSql("QUERY", tableName, sqlText)
            Return LoadTable(tableName).
                Where(Function(x) predicate(x)).
                Select(Function(x) New Dictionary(Of String, String)(x, StringComparer.OrdinalIgnoreCase)).
                ToList()
        End Function

        Public Function Update(tableName As String,
                               predicate As Func(Of IDictionary(Of String, String), Boolean),
                               updater As Action(Of IDictionary(Of String, String)),
                               sqlText As String) As Integer
            LogSql("UPDATE", tableName, sqlText)
            Dim rows As List(Of Dictionary(Of String, String)) = LoadTable(tableName)
            Dim count As Integer = 0
            For Each row In rows
                If predicate(row) Then
                    updater(row)
                    count += 1
                End If
            Next
            SaveTable(tableName, rows)
            Return count
        End Function

        Public Function Delete(tableName As String,
                               predicate As Func(Of IDictionary(Of String, String), Boolean),
                               sqlText As String) As Integer
            LogSql("DELETE", tableName, sqlText)
            Dim rows As List(Of Dictionary(Of String, String)) = LoadTable(tableName)
            Dim before As Integer = rows.Count
            rows = rows.Where(Function(x) Not predicate(x)).ToList()
            SaveTable(tableName, rows)
            Return before - rows.Count
        End Function

        Public Function LoadTable(tableName As String) As List(Of Dictionary(Of String, String))
            Dim filePath As String = TablePath(tableName)
            Dim result As New List(Of Dictionary(Of String, String))()
            If Not File.Exists(filePath) Then Return result

            For Each line As String In File.ReadAllLines(filePath, Encoding.UTF8)
                If String.IsNullOrWhiteSpace(line) Then Continue For
                Dim row As New Dictionary(Of String, String)(StringComparer.OrdinalIgnoreCase)
                For Each pair In line.Split(ControlChars.Tab)
                    Dim parts = pair.Split(New Char() {"="c}, 2)
                    If parts.Length = 2 Then row(Unescape(parts(0))) = Unescape(parts(1))
                Next
                result.Add(row)
            Next
            Return result
        End Function

        Private Sub SaveTable(tableName As String, rows As List(Of Dictionary(Of String, String)))
            Dim lines As String() = rows.Select(
                Function(row) String.Join(
                    ControlChars.Tab,
                    row.OrderBy(Function(x) x.Key).
                        Select(Function(x) Escape(x.Key) & "=" & Escape(If(x.Value, ""))))).ToArray()
            File.WriteAllLines(TablePath(tableName), lines, Encoding.UTF8)
        End Sub

        Private Sub LogSql(operation As String, tableName As String, sqlText As String)
            Dim logPath As String = System.IO.Path.Combine(_root, "_sql_log", DateTime.Now.ToString("yyyyMMdd") & ".sql.log")
            Dim logLine As String = String.Format("{0:O}|{1}|{2}|{3}", DateTime.Now, operation, tableName, sqlText)
            File.AppendAllLines(logPath, New String() {logLine}, Encoding.UTF8)
        End Sub

        Private Function TablePath(tableName As String) As String
            Return System.IO.Path.Combine(_root, tableName & ".table.txt")
        End Function

        Private Function Escape(value As String) As String
            If value Is Nothing Then Return ""
            Return value.Replace("\", "\\").Replace("=", "\e").Replace(ControlChars.Tab, "\t")
        End Function

        Private Function Unescape(value As String) As String
            Return value.Replace("\t", ControlChars.Tab).Replace("\e", "=").Replace("\\", "\")
        End Function
    End Class
End Namespace
