Public Class SampleBlockEngine
    Public Function Execute(value As Integer) As Integer
        Dim service As New SampleService()
        Return service.Resolve(value) + 1
    End Function
End Class
