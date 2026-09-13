Imports System

Namespace Logging
    Public Class AppLogger
        Public Sub Info(message As String)
            Console.WriteLine("[INFO] " & message)
        End Sub

        Public Sub Warn(message As String)
            Console.WriteLine("[WARN] " & message)
        End Sub

        Public Sub [Error](message As String)
            Console.WriteLine("[ERROR] " & message)
        End Sub
    End Class
End Namespace
