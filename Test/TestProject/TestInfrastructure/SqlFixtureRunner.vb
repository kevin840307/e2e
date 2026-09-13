Imports System
Imports System.Collections.Generic
Imports System.Globalization
Imports System.IO
Imports System.Linq
Imports System.Text.RegularExpressions
Imports FileDb

Namespace TestInfrastructure
    Public Class SqlFixtureRunner
        Private Shared ReadOnly PlaceholderPattern As New Regex(
            "\{\{([A-Za-z_][A-Za-z0-9_]*)\}\}",
            RegexOptions.Compiled Or RegexOptions.CultureInvariant)

        Public Sub ExecutePrepareSql(
            sqlPath As String,
            dbRoot As String,
            Optional parameters As IDictionary(Of String, Object) = Nothing)

            If Not File.Exists(sqlPath) Then Throw New FileNotFoundException(sqlPath)

            Dim template = File.ReadAllText(sqlPath)
            Dim rendered = RenderSql(template, parameters)

            Dim db As New FileDbSession(dbRoot)
            For Each raw As String In SplitStatements(rendered)
                Dim line = raw.Trim()
                If line = "" OrElse line.StartsWith("--") Then Continue For

                If line.StartsWith("INSERT INTO ", StringComparison.OrdinalIgnoreCase) Then
                    ExecuteInsert(db, line)
                Else
                    Throw New InvalidOperationException(
                        "prepare.sql only supports INSERT preconditions in this sample: " & line)
                End If
            Next
        End Sub

        Public Function RenderSql(
            template As String,
            Optional parameters As IDictionary(Of String, Object) = Nothing) As String

            If template Is Nothing Then Throw New ArgumentNullException(NameOf(template))

            Dim supplied As IDictionary(Of String, Object) =
                If(parameters,
                   New Dictionary(Of String, Object)(StringComparer.OrdinalIgnoreCase))

            Dim used As New HashSet(Of String)(StringComparer.OrdinalIgnoreCase)

            Dim rendered = PlaceholderPattern.Replace(
                template,
                Function(m As Match)
                    Dim name = m.Groups(1).Value
                    Dim value As Object = Nothing

                    If Not supplied.TryGetValue(name, value) Then
                        Throw New InvalidOperationException(
                            "Missing SQL parameter {{" & name & "}}.")
                    End If

                    used.Add(name)
                    Return ToSqlLiteral(value)
                End Function)

            Dim unresolved = PlaceholderPattern.Match(rendered)
            If unresolved.Success Then
                Throw New InvalidOperationException(
                    "Unresolved SQL parameter: " & unresolved.Value)
            End If

            For Each key In supplied.Keys
                If Not used.Contains(key) Then
                    Throw New InvalidOperationException(
                        "Unused SQL parameter: " & key)
                End If
            Next

            Return rendered
        End Function

        Private Function ToSqlLiteral(value As Object) As String
            If value Is Nothing OrElse value Is DBNull.Value Then Return "NULL"

            If TypeOf value Is Boolean Then
                Return If(CBool(value), "1", "0")
            End If

            If TypeOf value Is Byte OrElse
               TypeOf value Is SByte OrElse
               TypeOf value Is Short OrElse
               TypeOf value Is UShort OrElse
               TypeOf value Is Integer OrElse
               TypeOf value Is UInteger OrElse
               TypeOf value Is Long OrElse
               TypeOf value Is ULong OrElse
               TypeOf value Is Single OrElse
               TypeOf value Is Double OrElse
               TypeOf value Is Decimal Then

                Return Convert.ToString(value, CultureInfo.InvariantCulture)
            End If

            If TypeOf value Is DateTime Then
                Return "'" & DirectCast(value, DateTime).ToString(
                    "yyyy-MM-dd HH:mm:ss.fff",
                    CultureInfo.InvariantCulture).Replace("'", "''") & "'"
            End If

            Dim text = Convert.ToString(value, CultureInfo.InvariantCulture)
            Return "'" & text.Replace("'", "''") & "'"
        End Function

        Private Function SplitStatements(sql As String) As IEnumerable(Of String)
            ' The sample fixture SQL is intentionally simple: one INSERT per line
            ' or statements separated by semicolon. Placeholder rendering happens first.
            Return sql.Replace(vbCrLf, vbLf).
                Split(New Char() {";"c, ControlChars.Lf}).
                Select(Function(x) x.Trim()).
                Where(Function(x) x <> "")
        End Function

        Private Sub ExecuteInsert(db As FileDbSession, sql As String)
            Dim rest = sql.Substring("INSERT INTO ".Length)
            Dim openCols = rest.IndexOf("("c)
            Dim closeCols = rest.IndexOf(")"c)
            Dim valuesIndex = rest.IndexOf("VALUES", StringComparison.OrdinalIgnoreCase)

            If openCols < 1 OrElse closeCols < openCols OrElse valuesIndex < closeCols Then
                Throw New InvalidOperationException("Invalid INSERT syntax: " & sql)
            End If

            Dim tableName = rest.Substring(0, openCols).Trim()
            Dim columns = rest.Substring(
                openCols + 1,
                closeCols - openCols - 1).
                Split(","c).
                Select(Function(x) x.Trim()).
                ToArray()

            Dim openValues = rest.IndexOf("("c, valuesIndex)
            Dim closeValues = rest.LastIndexOf(")"c)
            Dim values = rest.Substring(
                openValues + 1,
                closeValues - openValues - 1).
                Split(","c).
                Select(Function(x) NormalizeStoredValue(x.Trim())).
                ToArray()

            If columns.Length <> values.Length Then
                Throw New InvalidOperationException(
                    "Column/value count mismatch: " & sql)
            End If

            Dim row As New Dictionary(Of String, String)(
                StringComparer.OrdinalIgnoreCase)

            For i = 0 To columns.Length - 1
                row(columns(i)) = values(i)
            Next

            db.Insert(tableName, row, sql)
        End Sub

        Private Function NormalizeStoredValue(value As String) As String
            If String.Equals(value, "NULL", StringComparison.OrdinalIgnoreCase) Then
                Return ""
            End If

            If value.Length >= 2 AndAlso
               value.StartsWith("'") AndAlso
               value.EndsWith("'") Then

                Return value.Substring(1, value.Length - 2).Replace("''", "'")
            End If

            Return value
        End Function
    End Class
End Namespace
