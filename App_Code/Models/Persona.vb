Imports System.Data

Public Class Persona

    Public Property Id As Integer
    Public Property Nombre As String
    Public Property Apellidos As String
    Public Property Email As String
    Public Property Password As String
    Public Property FechaCreacion As DateTime = DateTime.Now
    Public Sub New()
    End Sub

    Public Overrides Function Equals(obj As Object) As Boolean
        Dim persona = TryCast(obj, Persona)
        Return persona IsNot Nothing AndAlso
               Id = persona.Id AndAlso
               Nombre = persona.Nombre AndAlso
               Apellidos = persona.Apellidos AndAlso
               Email = persona.Email AndAlso
               Password = persona.Password
        FechaCreacion = persona.FechaCreacion
    End Function


    Public Overrides Function GetHashCode() As Integer
        Return (Id, Nombre, Apellidos, Email, Password).GetHashCode()
    End Function
End Class
