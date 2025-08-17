Imports System.Data
Imports System.Configuration
Imports System


Public Class Usuario
    Inherits Persona


    Public Sub New()

    End Sub

    ' Método para validar el usuario
    Public Function Validar() As Boolean
        Return Not String.IsNullOrEmpty(Email) AndAlso Not String.IsNullOrEmpty(Password)
    End Function

    ' Método para convertir un DataTable en un objeto Usuario
    Public Function DtToUsuario(dataTable As DataTable) As Usuario
        If dataTable IsNot Nothing AndAlso dataTable.Rows.Count > 0 Then
            Dim row As DataRow = dataTable.Rows(0)
            Return New Usuario() With {
                .Id = Convert.ToInt32(row("Id")),
                .Nombre = Convert.ToString(row("Nombre")),
                .Apellidos = Convert.ToString(row("Apellidos")),
                .Email = Convert.ToString(row("Email")),
                .Password = Convert.ToString(row("Password")),
                .FechaCreacion = If(row("FechaCreacion") IsNot DBNull.Value, Convert.ToDateTime(row("FechaCreacion")), DateTime.Now)
            }
        End If
        Return Nothing
    End Function

End Class
