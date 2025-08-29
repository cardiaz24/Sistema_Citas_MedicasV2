Imports System.Data.SqlClient
Imports System.Web.UI.WebControls

Public Class Dashboard
    Inherits System.Web.UI.Page

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        If Not IsPostBack Then
            ' 1. Verificar autenticación y permisos
            If Not VerificarPermisosAdministrador() Then
                Response.Redirect("~/Login.aspx")
                Return
            End If

            ' 2. Cargar datos iniciales
            CargarDoctores()
            lblMsg.Visible = False
        End If
    End Sub

    'Verificar si usuario es administrador
    Private Function VerificarPermisosAdministrador() As Boolean
        If Session("UsuarioId") Is Nothing OrElse Session("UsuarioRol") Is Nothing Then
            Return False
        End If

        Dim rol As String = Session("UsuarioRol").ToString()
        Return rol.Equals("Administrador", StringComparison.OrdinalIgnoreCase)
    End Function

    'Cargar lista de doctores
    Private Sub CargarDoctores()
        Try
            Dim helper As New DatabaseHelper()
            Dim sql As String =
                "SELECT p.Id, p.Nombre, p.Apellidos, p.Email, 
                        d.Especialidad, d.Activo,
                        p.FechaCreacion
                 FROM Personas p
                 INNER JOIN Doctores d ON p.Id = d.PersonaId
                 ORDER BY p.Apellidos, p.Nombre"

            gvDoctores.DataSource = helper.ExecuteQuery(sql)
            gvDoctores.DataBind()

        Catch ex As Exception
            MostrarError("Error al cargar doctores: " & ex.Message)
        End Try
    End Sub

    'Asignar rol de doctor
    Protected Sub btnAsignarDoctor_Click(sender As Object, e As EventArgs) Handles btnAsignarDoctor.Click
        Try
            ' 1. Validar permisos
            If Not VerificarPermisosAdministrador() Then
                MostrarError("No tiene permisos para realizar esta acción")
                Return
            End If

            ' 2. Validar campos
            If String.IsNullOrEmpty(txtPersonaId.Text.Trim()) Then
                MostrarError("El ID de persona es requerido")
                Return
            End If

            If String.IsNullOrEmpty(txtEspecialidad.Text.Trim()) Then
                MostrarError("La especialidad es requerida")
                Return
            End If

            Dim personaId As Integer
            If Not Integer.TryParse(txtPersonaId.Text.Trim(), personaId) Then
                MostrarError("El ID de persona debe ser un número válido")
                Return
            End If

            ' 3. Verificar que la persona existe
            If Not PersonaExiste(personaId) Then
                MostrarError("La persona con ID " & personaId & " no existe")
                Return
            End If

            ' 4. Verificar que no sea ya doctor
            If EsDoctor(personaId) Then
                MostrarError("Esta persona ya tiene rol de doctor")
                Return
            End If

            ' 5. Asignar rol de doctor
            If AsignarRolDoctor(personaId, txtEspecialidad.Text.Trim()) Then
                MostrarExito("Rol de doctor asignado exitosamente")
                LimpiarFormulario()
                CargarDoctores() ' Recargar grid
            Else
                MostrarError("Error al asignar rol de doctor")
            End If

        Catch ex As Exception
            MostrarError("Error: " & ex.Message)
        End Try
    End Sub

    ' Verificar si persona existe
    Private Function PersonaExiste(personaId As Integer) As Boolean
        Dim helper As New DatabaseHelper()
        Dim sql As String = "SELECT COUNT(*) FROM Personas WHERE Id = @PersonaId"
        Dim parameters As New List(Of SqlParameter) From {
            helper.CreateParameter("@PersonaId", personaId)
        }

        Dim count As Integer = Convert.ToInt32(helper.ExecuteScalar(sql, parameters))
        Return count > 0
    End Function

    ' Verificar si ya es doctor
    Private Function EsDoctor(personaId As Integer) As Boolean
        Dim helper As New DatabaseHelper()
        Dim sql As String = "SELECT COUNT(*) FROM Doctores WHERE PersonaId = @PersonaId"
        Dim parameters As New List(Of SqlParameter) From {
            helper.CreateParameter("@PersonaId", personaId)
        }

        Dim count As Integer = Convert.ToInt32(helper.ExecuteScalar(sql, parameters))
        Return count > 0
    End Function

    'Asignar rol de doctor (transaccional)
    Private Function AsignarRolDoctor(personaId As Integer, especialidad As String) As Boolean
        Dim db As New DatabaseHelper()

        Using conn As SqlConnection = db.GetConnection()
            conn.Open()
            Using tx As SqlTransaction = conn.BeginTransaction()
                Try
                    ' 1) Insertar en Doctores
                    Dim sqlDoctor As String =
                    "INSERT INTO Doctores (PersonaId, Especialidad, Activo) " &
                    "VALUES (@PersonaId, @Especialidad, 1)"

                    Dim doctorParams As New List(Of SqlParameter) From {
                    db.CreateParameter("@PersonaId", personaId),
                    db.CreateParameter("@Especialidad", especialidad)
                }
                    db.ExecuteNonQuery(tx, sqlDoctor, doctorParams)

                    ' 2) Asignar rol en PersonaRoles
                    Dim sqlRol As String =
                    "INSERT INTO PersonaRoles (PersonaId, RolId, Activo) " &
                    "VALUES (@PersonaId, (SELECT Id FROM Roles WHERE Nombre = 'Doctor'), 1)"

                    Dim rolParams As New List(Of SqlParameter) From {
                    db.CreateParameter("@PersonaId", personaId)
                }
                    db.ExecuteNonQuery(tx, sqlRol, rolParams)

                    tx.Commit()
                    Return True

                Catch ex As Exception
                    tx.Rollback()
                    Throw New Exception("Error en asignación de rol: " & ex.Message)
                End Try
            End Using
        End Using
    End Function

    'Métodos auxiliares para mensajes
    Private Sub MostrarError(mensaje As String)
        lblMsg.Text = mensaje
        lblMsg.CssClass = "alert alert-danger"
        lblMsg.Visible = True
    End Sub

    Private Sub MostrarExito(mensaje As String)
        lblMsg.Text = mensaje
        lblMsg.CssClass = "alert alert-success"
        lblMsg.Visible = True
    End Sub

    Private Sub LimpiarFormulario()
        txtPersonaId.Text = String.Empty
        txtEspecialidad.Text = String.Empty
    End Sub

    'Recargar grid desde fuera
    Protected Sub btnRecargar_Click(sender As Object, e As EventArgs)
        CargarDoctores()
    End Sub


    Protected Sub gvDoctores_RowDataBound(sender As Object, e As GridViewRowEventArgs) Handles gvDoctores.RowDataBound
        If e.Row.RowType = DataControlRowType.DataRow Then
            ' Resaltar doctores inactivos
            Dim activo As Boolean = Convert.ToBoolean(DataBinder.Eval(e.Row.DataItem, "Activo"))
            If Not activo Then
                e.Row.CssClass = "table-warning"
            End If

            ' Formatear fecha
            Dim fechaCell As TableCell = e.Row.Cells(6) ' Ajustar índice según tus columnas
            If fechaCell.Text <> String.Empty Then
                Dim fecha As DateTime = Convert.ToDateTime(fechaCell.Text)
                fechaCell.Text = fecha.ToString("dd/MM/yyyy")
            End If
        End If
    End Sub

    'Buscar persona por email
    Protected Sub btnBuscarPersona_Click(sender As Object, e As EventArgs)
    Try
        Dim email As String = txtEmailBusqueda.Text.Trim()
        If String.IsNullOrEmpty(email) Then
            MostrarError("Ingrese un email para buscar")
            Return
        End If

        Dim helper As New DatabaseHelper()
        Dim sql As String = 
            "SELECT Id, Nombre, Apellidos, Email FROM Personas WHERE Email = @Email"
        Dim parameters As New List(Of SqlParameter) From {
            helper.CreateParameter("@Email", email)
        }

        Dim dt As DataTable = helper.ExecuteQuery(sql, parameters)
        
        If dt.Rows.Count > 0 Then
            Dim row As DataRow = dt.Rows(0)
            lblPersonaInfo.Text = $"{row("Nombre")} {row("Apellidos")} (ID: {row("Id")})"
            hdnPersonaId.Value = row("Id").ToString()
            txtPersonaId.Text = row("Id").ToString()
            pnlResultadoBusqueda.Visible = True
        Else
            MostrarError("No se encontró persona con ese email")
            pnlResultadoBusqueda.Visible = False
        End If

    Catch ex As Exception
        MostrarError("Error en búsqueda: " & ex.Message)
    End Try
End Sub










End Class