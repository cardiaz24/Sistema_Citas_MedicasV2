Imports System.Data
Imports System.Linq
Imports System.Data.SqlClient


Public Class Pacientes
    Inherits System.Web.UI.Page

    'Verificar autenticación y rol
    Private Sub VerificarAutenticacion()
        If Session("UsuarioId") Is Nothing OrElse Session("UsuarioRol") Is Nothing Then
            Response.Redirect("~/Login.aspx")
        End If

        Dim rol As String = Session("UsuarioRol").ToString()
        If Not rol.Equals("Paciente", StringComparison.OrdinalIgnoreCase) Then
            Response.Redirect("~/Default.aspx")
        End If
    End Sub

    ' Obtener ID del paciente actual
    Private Function ObtenerPacienteId() As Integer
        Dim usuarioId As String = Session("UsuarioId")?.ToString()
        If Integer.TryParse(usuarioId, Nothing) Then
            Return Convert.ToInt32(usuarioId)
        End If
        Throw New Exception("No se pudo obtener el ID del paciente")
    End Function

    Protected Sub Page_Load(sender As Object, e As EventArgs) Handles Me.Load
        Try
            VerificarAutenticacion()

            If Not IsPostBack Then
                CargarCitas()
                CargarDoctores() ' Para el dropdown de doctores
                CargarHorariosDisponibles() ' Para el dropdown de horarios
            End If

        Catch ex As Exception
            MostrarError("Error: " & ex.Message)
        End Try
    End Sub

    ' Cargar citas del paciente
    Private Sub CargarCitas()
        Try
            Dim pacienteId As Integer = ObtenerPacienteId()
            Dim repo As New CitaRepository()

            Dim citas As DataTable = repo.GetCitasPorPaciente(pacienteId)

            gvCitas.DataSource = citas
            gvCitas.DataBind()

        Catch ex As Exception
            Throw New Exception("Error al cargar citas: " & ex.Message)
        End Try
    End Sub

    'Cargar lista de doctores para reserva
    Private Sub CargarDoctores()
        Try
            Dim repo As New DoctorRepository()
            Dim doctores As DataTable = repo.GetAll()

            If doctores IsNot Nothing AndAlso doctores.Rows.Count > 0 Then
                ddlDoctores.DataSource = doctores
                ddlDoctores.DataTextField = "NombreCompleto"
                ddlDoctores.DataValueField = "Id"
                ddlDoctores.DataBind()
            Else
                ' No hay doctores disponibles
                ddlDoctores.Items.Clear()
            End If

            ddlDoctores.Items.Insert(0, New ListItem("Seleccione un doctor", ""))

        Catch ex As Exception
            ' Mejor usar logging en lugar de throw en un evento de UI
            ddlDoctores.Items.Clear()
            ddlDoctores.Items.Insert(0, New ListItem("Error al cargar doctores", ""))
            ' Opcional: Registrar el error en un log
            ' Logger.Error("Error en CargarDoctores: " & ex.Message)
        End Try
    End Sub

    'Cargar horarios disponibles según doctor seleccionado
    Private Sub CargarHorariosDisponibles()
        Try
            Dim doctorId As Integer
            If Integer.TryParse(ddlDoctores.SelectedValue, doctorId) AndAlso doctorId > 0 Then
                Dim repo As New HorarioRepository()
                Dim horarios As DataTable = repo.GetDisponiblesPorDoctor(doctorId)

                ddlHorarios.DataSource = horarios
                ddlHorarios.DataTextField = "FechaFormateada"
                ddlHorarios.DataValueField = "Id"
                ddlHorarios.DataBind()
                ddlHorarios.Items.Insert(0, New ListItem("Seleccione un horario", ""))
            Else
                ddlHorarios.Items.Clear()
                ddlHorarios.Items.Insert(0, New ListItem("Primero seleccione un doctor", ""))
            End If

        Catch ex As Exception
            Throw New Exception("Error al cargar horarios: " & ex.Message)
        End Try
    End Sub

    'Reservar nueva cita
    Protected Sub btnReservar_Click(sender As Object, e As EventArgs) Handles btnReservar.Click
        Try
            ' 1. Validar selecciones
            If String.IsNullOrEmpty(ddlDoctores.SelectedValue) Then
                MostrarError("Seleccione un doctor")
                Return
            End If

            If String.IsNullOrEmpty(ddlHorarios.SelectedValue) Then
                MostrarError("Seleccione un horario")
                Return
            End If

            Dim horarioId As Integer = Convert.ToInt32(ddlHorarios.SelectedValue)
            Dim pacienteId As Integer = ObtenerPacienteId()

            ' 2. Reservar cita
            Dim repo As New CitaRepository()
            If repo.ReservarCita(pacienteId, horarioId) Then
                MostrarExito("Cita reservada exitosamente")
                LimpiarFormularioReserva()
                CargarCitas() ' Recargar grid
                CargarHorariosDisponibles() ' Actualizar horarios
            Else
                MostrarError("Error al reservar cita. El horario puede no estar disponible.")
            End If

        Catch ex As Exception
            MostrarError("Error: " & ex.Message)
        End Try
    End Sub

    ' cancelar cita
    Protected Sub gvCitas_RowDeleting(sender As Object, e As GridViewDeleteEventArgs) Handles gvCitas.RowDeleting
        Try
            Dim citaId As Integer = Convert.ToInt32(gvCitas.DataKeys(e.RowIndex).Value)
            Dim repo As New CitaRepository()

            ' Verificar si la cita se puede cancelar
            If Not PuedeCancelarCita(citaId) Then
                MostrarError("No se puede cancelar esta cita. Puede que ya haya pasado o esté en estado no cancelable.")
                Return
            End If

            If repo.EliminarCitaSiEsFutura(citaId) Then
                MostrarExito("Cita cancelada exitosamente")
                CargarCitas()
            Else
                MostrarError("Error al cancelar cita")
            End If

        Catch ex As Exception
            MostrarError("Error: " & ex.Message)
        End Try
    End Sub

    'Verificar si la cita se puede cancelar
    Private Function PuedeCancelarCita(citaId As Integer) As Boolean
        Try
            Dim repo As New CitaRepository()
            Dim cita As DataTable = repo.GetCitaById(citaId)

            If cita.Rows.Count = 0 Then Return False

            Dim estado As String = cita.Rows(0)("Estado").ToString()
            Dim fechaCita As DateTime = Convert.ToDateTime(cita.Rows(0)("Fecha"))

            ' Solo se pueden cancelar citas pendientes/confirmadas y futuras
            Return (estado = "Pendiente" OrElse estado = "Confirmada") AndAlso
                   fechaCita > DateTime.Now

        Catch ex As Exception
            Throw New Exception("Error al verificar cita: " & ex.Message)
        End Try
    End Function

    'Actualizar horarios cuando cambia el doctor
    Protected Sub ddlDoctores_SelectedIndexChanged(sender As Object, e As EventArgs) Handles ddlDoctores.SelectedIndexChanged
        Try
            CargarHorariosDisponibles()
        Catch ex As Exception
            MostrarError("Error: " & ex.Message)
        End Try
    End Sub

    'Métodos auxiliares
    Private Sub MostrarError(mensaje As String)
        lblMensaje.Text = mensaje
        lblMensaje.CssClass = "alert alert-danger"
        lblMensaje.Visible = True
    End Sub

    Private Sub MostrarExito(mensaje As String)
        lblMensaje.Text = mensaje
        lblMensaje.CssClass = "alert alert-success"
        lblMensaje.Visible = True
    End Sub

    Private Sub LimpiarFormularioReserva()
        ddlDoctores.SelectedIndex = 0
        ddlHorarios.Items.Clear()
        ddlHorarios.Items.Insert(0, New ListItem("Seleccione un doctor primero", ""))
    End Sub

    'Mejorar la visualización del GridView
    Protected Sub gvCitas_RowDataBound(sender As Object, e As GridViewRowEventArgs) Handles gvCitas.RowDataBound
        If e.Row.RowType = DataControlRowType.DataRow Then
            ' Resaltar citas según estado
            Dim estado As String = DataBinder.Eval(e.Row.DataItem, "Estado").ToString()
            Select Case estado.ToLower()
                Case "confirmada"
                    e.Row.CssClass = "table-success"
                Case "cancelada", "nopresentado"
                    e.Row.CssClass = "table-danger"
                Case "completada"
                    e.Row.CssClass = "table-info"
            End Select

            ' Ocultar botón de eliminar para citas no cancelables
            Dim fechaCita As DateTime = Convert.ToDateTime(DataBinder.Eval(e.Row.DataItem, "Fecha"))
            Dim puedeCancelar As Boolean = (estado = "Pendiente" OrElse estado = "Confirmada") AndAlso
                                          fechaCita > DateTime.Now

            Dim btnEliminar As Button = TryCast(e.Row.Cells(4).Controls(0), Button)
            If btnEliminar IsNot Nothing Then
                btnEliminar.Visible = puedeCancelar
                If puedeCancelar Then
                    btnEliminar.OnClientClick = "return confirm('¿Está seguro de cancelar esta cita?');"
                End If
            End If
        End If
    End Sub
End Class