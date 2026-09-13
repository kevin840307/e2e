Imports System
Imports System.IO
Imports System.Text
Imports System.Text.RegularExpressions

Namespace HttpMock
    Public Class FakeHttpClient
        Public Function Post(url As String, body As String) As Integer
            Dim fixture = Environment.GetEnvironmentVariable("E2E_MOCK_API_FILE")
            If String.IsNullOrWhiteSpace(fixture) Then
                fixture = Environment.GetEnvironmentVariable("E2E_MOCK_HTTP_FILE")
            End If

            Dim content As String = ""
            If Not String.IsNullOrWhiteSpace(fixture) AndAlso File.Exists(fixture) Then
                content = File.ReadAllText(fixture, Encoding.UTF8)
            End If

            Dim match = Regex.Match(content, """statusCode""\s*:\s*(\d+)", RegexOptions.IgnoreCase)
            Dim fallback = If(
                String.IsNullOrWhiteSpace(url),
                400,
                If(url.IndexOf("fail", StringComparison.OrdinalIgnoreCase) >= 0, 500, 200))

            Return If(match.Success, Integer.Parse(match.Groups(1).Value), fallback)
        End Function
    End Class
End Namespace
