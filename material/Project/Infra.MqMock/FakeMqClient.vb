Imports System
Imports System.IO
Imports System.Text
Imports System.Text.RegularExpressions

Namespace MqMock
    Public Class FakeMqClient
        Public Function Publish(topic As String, payload As String) As Boolean
            Dim fixture = Environment.GetEnvironmentVariable("E2E_MOCK_MQ_FILE")
            Dim body As String = ""
            If Not String.IsNullOrWhiteSpace(fixture) AndAlso File.Exists(fixture) Then
                body = File.ReadAllText(fixture, Encoding.UTF8)
            End If

            Dim ack = Not Regex.IsMatch(
                body,
                """ack""\s*:\s*false|NACK",
                RegexOptions.IgnoreCase)

            Dim dbRoot = Environment.GetEnvironmentVariable("E2E_DB_ROOT")
            If Not String.IsNullOrWhiteSpace(dbRoot) Then
                Dim logRoot = Path.Combine(dbRoot, "_external_log")
                Directory.CreateDirectory(logRoot)
                File.AppendAllLines(
                    Path.Combine(logRoot, "mq.log"),
                    New String() {String.Join("|", New String() {topic, payload, If(ack, "ACK", "NACK")})},
                    Encoding.UTF8)
            End If

            Return ack
        End Function
    End Class
End Namespace
