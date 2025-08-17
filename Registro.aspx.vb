Imports System.Data.SqlClient

Public Class Registro
    Inherits System.Web.UI.Page

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load

    End Sub
    Protected Sub btnRegistrar_Click(sender As Object, e As EventArgs) Handles btnRegistrar.Click
        Dim nombre As String = txtNombre.Text
        Dim apellido As String = txtApellido.Text
        Dim email As String = txtEmail.Text
        Dim pass As String = txtPass.Text

        Dim usuario As New Usuario() With {
            .Nombre = txtNombre.Text,
            .Apellidos = txtApellido.Text,
            .Email = txtEmail.Text,
            .Password = txtPass.Text
        }
        If RegistrarUsuario(usuario) Then
            ' Redirigir al login o a la página de inicio

            ScriptManager.RegisterStartupScript(
                Me, Me.GetType(),
                "ServerControlScript",
                "Swal.fire('Usuario Registrado').then((result) => {
                    if (result.isConfirmed) {
                        window.location.href = 'Login.aspx';
                    }
                });",
                True)

        Else
            ScriptManager.RegisterStartupScript(
                Me, Me.GetType(),
                "ServerControlScript",
                "Swal.fire('Error al registrar el usuario. Inténtalo de nuevo.');",
                True)
            lblError.Text = "Error al registrar el usuario. Inténtalo de nuevo."
            lblError.Visible = True
        End If

    End Sub

    Private Function RegistrarUsuario(usuario As Usuario) As Boolean
        Dim helper As New DatabaseHelper()
        Dim sql As String = "INSERT INTO Personas (Nombre, Apellidos, Email, Password ) VALUES (@Nombre,@Apellido, @Email, @Password )"
        Dim parameters As New List(Of SqlParameter) From {
            New SqlParameter("@Nombre", usuario.Nombre),
            New SqlParameter("@Apellido", usuario.Apellidos),
            New SqlParameter("@Email", usuario.Email),
            New SqlParameter("@Password", usuario.Password)
        }
        helper.ExecuteNonQuery(sql, parameters)
        Return True
    End Function
End Class