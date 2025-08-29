Imports System.Data.SqlClient
Imports System.Security.Cryptography
Imports System.Text
Imports System.ComponentModel.DataAnnotations

Public Class Registro
    Inherits System.Web.UI.Page

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        If Not IsPostBack Then
            lblError.Visible = False
        End If
    End Sub

    'Método seguro para registrar paciente
    Private Function RegistrarPaciente(nombre As String, apellidos As String,
                                      email As String, password As String,
                                      direccion As String, telefono As String) As Boolean

        Using conn As SqlConnection = New DatabaseHelper().GetConnection()
            conn.Open()
            Using transaction As SqlTransaction = conn.BeginTransaction()
                Try
                    Dim helper As New DatabaseHelper()

                    ' 1. Validar que el email no exista
                    If EmailExiste(email, transaction) Then
                        Throw New Exception("El email ya está registrado")
                    End If

                    ' 2. Hashear la contraseña
                    Dim hashedPassword = HashPassword(password)

                    ' 3. Insertar en Personas
                    Dim sqlPersona As String =
                        "INSERT INTO Personas (Nombre, Apellidos, Email, Password, FechaCreacion)
                         OUTPUT INSERTED.Id
                         VALUES (@Nombre, @Apellidos, @Email, @Password, GETDATE())"

                    Dim personaParams As New List(Of SqlParameter) From {
                        helper.CreateParameter("@Nombre", nombre),
                        helper.CreateParameter("@Apellidos", apellidos),
                        helper.CreateParameter("@Email", email),
                        helper.CreateParameter("@Password", hashedPassword)
                    }

                    Dim personaId As Integer = Convert.ToInt32(
                        helper.ExecuteScalar(transaction, sqlPersona, personaParams))

                    ' 4. Insertar en Pacientes
                    Dim sqlPaciente As String =
                        "INSERT INTO Pacientes (PersonaId, Direccion, Telefono, FechaRegistro)
                         VALUES (@PersonaId, @Direccion, @Telefono, GETDATE())"

                    Dim pacienteParams As New List(Of SqlParameter) From {
                        helper.CreateParameter("@PersonaId", personaId),
                        helper.CreateParameter("@Direccion", direccion),
                        helper.CreateParameter("@Telefono", telefono)
                    }

                    helper.ExecuteNonQuery(transaction, sqlPaciente, pacienteParams)

                    ' 5. Asignar rol de Paciente
                    Dim sqlRol As String =
                        "INSERT INTO PersonaRoles (PersonaId, RolId, Activo)
                         VALUES (@PersonaId, (SELECT Id FROM Roles WHERE Nombre = 'Paciente'), 1)"

                    Dim rolParams As New List(Of SqlParameter) From {
                        helper.CreateParameter("@PersonaId", personaId)
                    }

                    helper.ExecuteNonQuery(transaction, sqlRol, rolParams)

                    transaction.Commit()
                    Return True

                Catch ex As Exception
                    transaction.Rollback()
                    Throw New Exception("Error al registrar paciente: " & ex.Message)
                End Try
            End Using
        End Using
    End Function

    'Verificar si email ya existe
    Private Function EmailExiste(email As String, transaction As SqlTransaction) As Boolean
        Dim helper As New DatabaseHelper()
        Dim sql As String = "SELECT COUNT(*) FROM Personas WHERE Email = @Email"
        Dim parameters As New List(Of SqlParameter) From {
            helper.CreateParameter("@Email", email)
        }

        Dim count As Integer = Convert.ToInt32(
            helper.ExecuteScalar(transaction, sql, parameters))

        Return count > 0
    End Function

    'Función para hashear contraseñas
    Private Function HashPassword(password As String) As String
        Using sha256 As SHA256 = SHA256.Create()
            Dim bytes As Byte() = Encoding.UTF8.GetBytes(password)
            Dim hash As Byte() = sha256.ComputeHash(bytes)
            Return Convert.ToBase64String(hash)
        End Using
    End Function

    'Validar datos del formulario
    Private Function ValidarFormulario() As String
        If String.IsNullOrEmpty(txtNombre.Text.Trim()) Then
            Return "El nombre es requerido"
        End If

        If String.IsNullOrEmpty(txtApellido.Text.Trim()) Then
            Return "Los apellidos son requeridos"
        End If

        If String.IsNullOrEmpty(txtEmail.Text.Trim()) Then
            Return "El email es requerido"
        End If

        ' Validar formato de email
        Dim emailAttribute As New EmailAddressAttribute()
        If Not emailAttribute.IsValid(txtEmail.Text.Trim()) Then
            Return "El formato del email no es válido"
        End If

        If String.IsNullOrEmpty(txtPass.Text) Then
            Return "La contraseña es requerida"
        End If

        If txtPass.Text.Length < 6 Then
            Return "La contraseña debe tener al menos 6 caracteres"
        End If



        Return Nothing ' Sin errores
    End Function

    Protected Sub btnRegistrar_Click(sender As Object, e As EventArgs) Handles btnRegistrar.Click
        Try
            ' 1. Validar formulario
            Dim mensajeError As String = ValidarFormulario()
            If mensajeError IsNot Nothing Then
                MostrarError(mensajeError)
                Return
            End If

            ' 2. Obtener valores del formulario
            Dim nombre As String = txtNombre.Text.Trim()
            Dim apellidos As String = txtApellido.Text.Trim()
            Dim email As String = txtEmail.Text.Trim().ToLower()
            Dim password As String = txtPass.Text

            ' 3. Valores por defecto para paciente 
            Dim direccion As String = txtDireccion.Text.Trim()
            Dim telefono As String = txtTelefono.Text.Trim()

            ' Validar teléfono
            If String.IsNullOrEmpty(telefono) Then
                MostrarError("El teléfono es requerido")
                Return
            End If

            ' 4. Registrar paciente
            If RegistrarPaciente(nombre, apellidos, email, password, direccion, telefono) Then
                MostrarExito("Usuario registrado exitosamente. Será redirigido al login.")

                ' Redirigir después de 3 segundos
                ScriptManager.RegisterStartupScript(Me, Me.GetType(),
                    "Redireccionar", "setTimeout(function() { window.location.href = 'Login.aspx'; }, 3000);", True)
            Else
                MostrarError("Error al registrar el usuario. Inténtalo de nuevo.")
            End If

        Catch ex As Exception
            MostrarError("Error: " & ex.Message)
        End Try
    End Sub


    'mostrar mensajes
    Private Sub MostrarError(mensaje As String)
        lblError.Text = mensaje
        lblError.CssClass = "alert alert-danger"
        lblError.Visible = True

        ScriptManager.RegisterStartupScript(Me, Me.GetType(),
            "MostrarError", $"alert('{mensaje.Replace("'", "\'")}');", True)
    End Sub

    Private Sub MostrarExito(mensaje As String)
        lblError.Text = mensaje
        lblError.CssClass = "alert alert-success"
        lblError.Visible = True

        ScriptManager.RegisterStartupScript(Me, Me.GetType(),
            "MostrarExito", $"alert('{mensaje.Replace("'", "\'")}');", True)
    End Sub

    'limpiar formulario
    Private Sub LimpiarFormulario()
        txtNombre.Text = String.Empty
        txtApellido.Text = String.Empty
        txtEmail.Text = String.Empty
        txtPass.Text = String.Empty
        lblError.Visible = False
    End Sub



End Class