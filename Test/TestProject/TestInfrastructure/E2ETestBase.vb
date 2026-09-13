Imports System
Imports System.Collections.Generic
Imports System.IO

Namespace TestInfrastructure
    Public MustInherit Class E2ETestBase
        Protected Function NewDbRoot(caseName As String) As String
            Dim dbPath = System.IO.Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "runtime_db",
                caseName)

            If Directory.Exists(dbPath) Then Directory.Delete(dbPath, True)
            Directory.CreateDirectory(dbPath)
            ClearExternalMocks()
            Environment.SetEnvironmentVariable("E2E_DB_ROOT", dbPath)
            Return dbPath
        End Function


        Private Sub ClearExternalMocks()
            For Each kind In New String() {"API", "HTTP", "MQ", "KAFKA", "NATS", "GRPC", "WSDL"}
                Environment.SetEnvironmentVariable("E2E_MOCK_" & kind & "_FILE", Nothing)
            Next
        End Sub

        Protected Function UseExternalMock(
            kind As String,
            blockName As String,
            sopName As String,
            fileName As String) As String

            If String.IsNullOrWhiteSpace(kind) Then Throw New ArgumentException(NameOf(kind))
            Dim fixture = SopFixturePath(blockName, sopName, fileName)
            Dim envName = "E2E_MOCK_" & kind.Trim().ToUpperInvariant().Replace("-", "_") & "_FILE"
            Environment.SetEnvironmentVariable(envName, fixture)
            Return fixture
        End Function

        Protected Function UseApiMock(blockName As String, sopName As String, fileName As String) As String
            Return UseExternalMock("API", blockName, sopName, fileName)
        End Function

        Protected Function UseMqMock(blockName As String, sopName As String, fileName As String) As String
            Return UseExternalMock("MQ", blockName, sopName, fileName)
        End Function

        Protected Function UseKafkaMock(blockName As String, sopName As String, fileName As String) As String
            Return UseExternalMock("KAFKA", blockName, sopName, fileName)
        End Function

        Protected Function UseNatsMock(blockName As String, sopName As String, fileName As String) As String
            Return UseExternalMock("NATS", blockName, sopName, fileName)
        End Function

        Protected Function UseGrpcMock(blockName As String, sopName As String, fileName As String) As String
            Return UseExternalMock("GRPC", blockName, sopName, fileName)
        End Function

        Protected Function UseWsdlMock(blockName As String, sopName As String, fileName As String) As String
            Return UseExternalMock("WSDL", blockName, sopName, fileName)
        End Function

        Protected Function SqlParams(
            ParamArray nameValuePairs() As Object) As IDictionary(Of String, Object)

            If nameValuePairs Is Nothing OrElse nameValuePairs.Length = 0 Then
                Return New Dictionary(Of String, Object)(
                    StringComparer.OrdinalIgnoreCase)
            End If

            If nameValuePairs.Length Mod 2 <> 0 Then
                Throw New ArgumentException(
                    "SqlParams requires name/value pairs.")
            End If

            Dim result As New Dictionary(Of String, Object)(
                StringComparer.OrdinalIgnoreCase)

            For i = 0 To nameValuePairs.Length - 1 Step 2
                Dim name = Convert.ToString(nameValuePairs(i))

                If String.IsNullOrWhiteSpace(name) Then
                    Throw New ArgumentException(
                        "SQL parameter name may not be empty.")
                End If

                If result.ContainsKey(name) Then
                    Throw New ArgumentException(
                        "Duplicate SQL parameter: " & name)
                End If

                result(name) = nameValuePairs(i + 1)
            Next

            Return result
        End Function

        Protected Sub RunPrepareSql(
            relativePath As String,
            dbRoot As String,
            Optional parameters As IDictionary(Of String, Object) = Nothing)

            Dim projectRoot = System.IO.Path.GetFullPath(
                System.IO.Path.Combine(
                    AppDomain.CurrentDomain.BaseDirectory,
                    "..\..\"))

            Dim sqlPath = System.IO.Path.Combine(projectRoot, relativePath)
            Dim fixture As New SqlFixtureRunner()
            fixture.ExecutePrepareSql(sqlPath, dbRoot, parameters)
        End Sub

        Protected Function SopFixturePath(
            blockName As String,
            sopName As String,
            fileName As String) As String

            If String.IsNullOrWhiteSpace(blockName) Then Throw New ArgumentException(NameOf(blockName))
            If String.IsNullOrWhiteSpace(sopName) Then Throw New ArgumentException(NameOf(sopName))
            If String.IsNullOrWhiteSpace(fileName) Then Throw New ArgumentException(NameOf(fileName))

            Dim expectedPrefix = blockName & "-SOP-"
            If Not sopName.StartsWith(expectedPrefix, StringComparison.OrdinalIgnoreCase) Then
                Throw New ArgumentException(
                    "SOP must belong to the requested block: " & sopName)
            End If

            If fileName.IndexOfAny(New Char() {"\"c, "/"c}) >= 0 Then
                Throw New ArgumentException(
                    "Fixture file must be a file in the owning SOP folder, not another path.")
            End If

            Dim projectRoot = System.IO.Path.GetFullPath(
                System.IO.Path.Combine(
                    AppDomain.CurrentDomain.BaseDirectory,
                    "..\..\"))

            Dim sopRoot = System.IO.Path.GetFullPath(
                System.IO.Path.Combine(projectRoot, blockName, sopName))

            Dim candidate = System.IO.Path.GetFullPath(
                System.IO.Path.Combine(sopRoot, fileName))

            Dim prefix = sopRoot.TrimEnd(
                System.IO.Path.DirectorySeparatorChar,
                System.IO.Path.AltDirectorySeparatorChar) &
                System.IO.Path.DirectorySeparatorChar

            If Not candidate.StartsWith(prefix, StringComparison.OrdinalIgnoreCase) Then
                Throw New InvalidOperationException(
                    "Cross-SOP fixture access is forbidden.")
            End If

            If Not System.IO.File.Exists(candidate) Then
                Throw New System.IO.FileNotFoundException(candidate)
            End If

            Return candidate
        End Function

        Protected Function ReadSopFixture(
            blockName As String,
            sopName As String,
            fileName As String) As String

            Return System.IO.File.ReadAllText(
                SopFixturePath(blockName, sopName, fileName))
        End Function

        Protected Function CallMain(blockName As String) As Integer
            Return SchedulerApp.Program.Main(New String() {blockName})
        End Function
    End Class
End Namespace
