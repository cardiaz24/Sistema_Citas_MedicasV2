Imports System.Data.SqlClient
Imports System.Security.Cryptography
Imports System.Text

Public Class Login
    Inherits System.Web.UI.Page

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        ' Limpiar sesión al cargar la página de login
        Session.Clear()
    End Sub

    ' Método seguro para verificar usuario
    Private Function VerificarUsuario(email As String, password As String) As DataTable
        Try
            Dim helper As New DatabaseHelper()

            ' 1. Primero obtener el usuario por email
            Dim query As String = "SELECT * FROM Personas WHERE Email = @Email"
            Dim parametros As New List(Of SqlParameter) From {
                helper.CreateParameter("@Email", email)
            }

            Dim dataTable As DataTable = helper.ExecuteQuery(query, parametros)

            If dataTable.Rows.Count = 0 Then
                Return Nothing ' Usuario no existe
            End If

            ' 2. Verificar contraseña hasheada
            Dim storedPassword As String = dataTable.Rows(0)("Password").ToString()
            Dim inputPasswordHash = HashPassword(password)

            If storedPassword = inputPasswordHash Then
                ' 3. Obtener información completa con rol
                Dim usuarioCompletoSql As String =
                    "SELECT p.*, r.Nombre as Rol, pr.Activo as RolActivo
                     FROM Personas p
                     INNER JOIN PersonaRoles pr ON p.Id = pr.PersonaId
                     INNER JOIN Roles r ON pr.RolId = r.Id
                     WHERE p.Email = @Email AND pr.Activo = 1"

                Return helper.ExecuteQuery(usuarioCompletoSql, parametros)
            Else
                Return Nothing ' Contraseña incorrecta
            End If

        Catch ex As Exception
            ' Log del error
            Throw New Exception("Error en verificación de usuario: " & ex.Message)
        End Try
    End Function

    'Función para hashear contraseñas
    Private Function HashPassword(password As String) As String
        Using sha256 As SHA256 = SHA256.Create()
            Dim bytes As Byte() = Encoding.UTF8.GetBytes(password)
            Dim hash As Byte() = sha256.ComputeHash(bytes)
            Return Convert.ToBase64String(hash)
        End Using
    End Function

    'Método para establecer sesión segura
    Private Sub EstablecerSesionSegura(dtUsuario As DataTable)
        If dtUsuario Is Nothing OrElse dtUsuario.Rows.Count = 0 Then
            Throw New Exception("Datos de usuario inválidos para establecer sesión")
        End If

        Dim row As DataRow = dtUsuario.Rows(0)

        'Almacena solo información necesaria
        Session("UsuarioId") = row("Id").ToString()
        Session("UsuarioNombre") = row("Nombre").ToString()
        Session("UsuarioApellidos") = row("Apellidos").ToString()
        Session("UsuarioEmail") = row("Email").ToString()
        Session("UsuarioRol") = row("Rol").ToString()
        Session("RolActivo") = row("RolActivo").ToString()

        'Establecer timeout de sesión (10 minutos)
        Session.Timeout = 10
    End Sub

    ' Segun el rol se va a redirigir a la pagina que le corresponda.
    Private Sub RedirigirSegunRol()
        Dim rol As String = Session("UsuarioRol")?.ToString()

        Select Case rol?.ToLower()
            Case "administrador"
                Response.Redirect("~/Pages/Admin/Dashboard.aspx")
            Case "doctor"
                Response.Redirect("~/Pages/Doctor/MisHorarios.aspx")
            Case "paciente"
                Response.Redirect("~/Pages/Paciente/Pacientes.aspx")
            Case Else
                Throw New Exception("Rol no reconocido: " & rol)
        End Select
    End Sub

    Protected Sub btnLogin_Click(sender As Object, e As EventArgs) Handles btnLogin.Click
        Try
            ' Validaciones básicas
            If String.IsNullOrEmpty(txtEmail.Text.Trim()) OrElse
               String.IsNullOrEmpty(txtPass.Text) Then
                lblError.Text = "Por favor, complete todos los campos."
                lblError.Visible = True
                Return
            End If

            ' Se verifican credenciales
            Dim dtUsuario As DataTable = VerificarUsuario(txtEmail.Text.Trim(), txtPass.Text)

            If dtUsuario IsNot Nothing AndAlso dtUsuario.Rows.Count > 0 Then
                ' Credenciales válidas
                EstablecerSesionSegura(dtUsuario)
                RedirigirSegunRol()
            Else
                ' Credenciales inválidas
                lblError.Text = "Credenciales inválidas. Por favor, intente nuevamente."
                lblError.Visible = True
            End If

        Catch ex As Exception

            lblError.Text = "Error en el sistema. Por favor, contacte al administrador."
            lblError.Visible = True
        End Try
    End Sub

    'Método para cerrar sesión
    Public Shared Sub CerrarSesion()
        HttpContext.Current.Session.Clear()
        HttpContext.Current.Session.Abandon()
        HttpContext.Current.Response.Redirect("~/Login.aspx")
    End Sub

    'Verifica si usuario está autenticado
    Public Shared Function EstaAutenticado() As Boolean
        Return HttpContext.Current.Session("UsuarioId") IsNot Nothing
    End Function

    'Verifica si usuario tiene rol específico
    Public Shared Function TieneRol(rol As String) As Boolean
        Dim rolActual As String = HttpContext.Current.Session("UsuarioRol")?.ToString()
        Return Not String.IsNullOrEmpty(rolActual) AndAlso
               rolActual.Equals(rol, StringComparison.OrdinalIgnoreCase)
    End Function

    'Obtiene el ID de usuario actual
    Public Shared Function ObtenerUsuarioId() As Integer
        Dim idStr As String = HttpContext.Current.Session("UsuarioId")?.ToString()
        If Integer.TryParse(idStr, Nothing) Then
            Return Convert.ToInt32(idStr)
        End If
        Return 0
    End Function
End Class