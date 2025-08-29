Imports System.Data
Imports System.Data.SqlClient
Imports System.Linq 'permite trabajar con consultas sobre colecciones de datos


Public Class MisHorarios
    Inherits System.Web.UI.Page

    ' Verificar autenticación y rol
    Private Sub VerificarAutenticacion()
        If Session("UsuarioId") Is Nothing OrElse Session("UsuarioRol") Is Nothing Then
            Response.Redirect("~/Login.aspx")
        End If

        Dim rol As String = Session("UsuarioRol").ToString()
        If Not rol.Equals("Doctor", StringComparison.OrdinalIgnoreCase) Then
            Response.Redirect("~/Default.aspx")
        End If
    End Sub

    ' Obtener ID del doctor actual
    Private Function ObtenerDoctorId() As Integer
        Dim usuarioId As String = Session("UsuarioId")?.ToString()
        Dim id As Integer
        If Not String.IsNullOrEmpty(usuarioId) AndAlso Integer.TryParse(usuarioId, id) Then
            Return id
        End If
        Throw New Exception("No se pudo obtener el ID del doctor")
    End Function

    Protected Sub Page_Load(sender As Object, e As EventArgs) Handles Me.Load
        Try
            VerificarAutenticacion()

            If Not IsPostBack Then
                CargarHorarios()
                txtFecha.Text = DateTime.Now.AddDays(1).ToString("yyyy-MM-ddTHH:mm") ' Formato HTML5 datetime-local
            End If

        Catch ex As Exception
            MostrarError("Error: " & ex.Message)
        End Try
    End Sub

    ' Cargar horarios del doctor
    Private Sub CargarHorarios()
        Try
            Dim doctorId As Integer = ObtenerDoctorId()
            Dim repo As New HorarioRepository()

            Dim horarios As DataTable = repo.GetByDoctor(doctorId)

            ' Filtrar solo horarios futuros para la visualización
            Dim horariosFuturos = horarios.Clone()
            For Each row As DataRow In horarios.Rows
                Dim fechaHorario As DateTime = Convert.ToDateTime(row("Fecha"))
                If fechaHorario > DateTime.Now Then
                    horariosFuturos.ImportRow(row)
                End If
            Next

            gvHorarios.DataSource = horariosFuturos
            gvHorarios.DataBind()

        Catch ex As Exception
            Throw New Exception("Error al cargar horarios: " & ex.Message)
        End Try
    End Sub

    ' Agregar nuevo horario
    Protected Sub btnAgregar_Click(sender As Object, e As EventArgs) Handles btnAgregar.Click
        Try
            ' 1. Validar fecha
            Dim fecha As DateTime
            If Not DateTime.TryParse(txtFecha.Text, fecha) Then
                MostrarError("Formato de fecha inválido. Use: dd/MM/yyyy HH:mm")
                Return
            End If

            ' 2. Validar que la fecha sea futura
            If fecha <= DateTime.Now Then
                MostrarError("La fecha debe ser futura")
                Return
            End If

            ' 3. Validar que no sea un horario duplicado
            Dim doctorId As Integer = ObtenerDoctorId()
            If HorarioExiste(doctorId, fecha) Then
                MostrarError("Ya existe un horario para esta fecha y hora")
                Return
            End If

            ' 4. Insertar horario
            Dim repo As New HorarioRepository()
            Dim horario As New Horario With {
                .DoctorId = doctorId,
                .Fecha = fecha,
                .Disponible = True
            }

            If repo.Insert(horario) Then
                MostrarExito("Horario agregado exitosamente")
                LimpiarFormulario()
                CargarHorarios()
            Else
                MostrarError("Error al agregar horario")
            End If

        Catch ex As Exception
            MostrarError("Error: " & ex.Message)
        End Try
    End Sub

    'Verificar si el horario ya existe
    Private Function HorarioExiste(doctorId As Integer, fecha As DateTime) As Boolean
        Try
            Dim repo As New HorarioRepository()
            Dim horarios As DataTable = repo.GetByDoctor(doctorId)

            For Each row As DataRow In horarios.Rows
                Dim fechaExistente As DateTime = Convert.ToDateTime(row("Fecha"))
                If fechaExistente.ToString("yyyyMMddHHmm") = fecha.ToString("yyyyMMddHHmm") Then
                    Return True
                End If
            Next

            Return False
        Catch ex As Exception
            Throw New Exception("Error al verificar horarios existentes: " & ex.Message)
        End Try
    End Function

    'Eliminar horario (desde GridView)
    Protected Sub gvHorarios_RowDeleting(sender As Object, e As GridViewDeleteEventArgs) Handles gvHorarios.RowDeleting
        Try
            Dim horarioId As Integer = Convert.ToInt32(gvHorarios.DataKeys(e.RowIndex).Value)
            Dim repo As New HorarioRepository()

            ' Verificar si el horario tiene citas
            If TieneCitas(horarioId) Then
                MostrarError("No se puede eliminar: el horario tiene citas asignadas")
                Return
            End If

            If repo.MarcarComoNoDisponible(horarioId) Then
                MostrarExito("Horario eliminado exitosamente")
                CargarHorarios()
            Else
                MostrarError("Error al eliminar horario")
            End If

        Catch ex As Exception
            MostrarError("Error: " & ex.Message)
        End Try
    End Sub

    ' Verificar si el horario tiene citas
    Private Function TieneCitas(horarioId As Integer) As Boolean
        Try
            Dim helper As New DatabaseHelper()
            Dim sql As String = "SELECT COUNT(*) FROM Citas WHERE HorarioId = @HorarioId"
            Dim parameters As New List(Of SqlParameter) From {
                helper.CreateParameter("@HorarioId", horarioId)
            }

            Dim count As Integer = Convert.ToInt32(helper.ExecuteScalar(sql, parameters))
            Return count > 0

        Catch ex As Exception
            Throw New Exception("Error al verificar citas: " & ex.Message)
        End Try
    End Function

    ' Métodos auxiliares
    Private Sub MostrarError(mensaje As String)
        ScriptManager.RegisterStartupScript(Me, Me.GetType(),
            "MostrarError", $"alert('{mensaje.Replace("'", "\'")}');", True)
    End Sub

    Private Sub MostrarExito(mensaje As String)
        ScriptManager.RegisterStartupScript(Me, Me.GetType(),
            "MostrarExito", $"alert('{mensaje.Replace("'", "\'")}');", True)
    End Sub

    Private Sub LimpiarFormulario()
        txtFecha.Text = DateTime.Now.AddDays(1).ToString("yyyy-MM-ddTHH:mm")
    End Sub

    ' Mejorar la visualización del GridView
    Protected Sub gvHorarios_RowDataBound(sender As Object, e As GridViewRowEventArgs) Handles gvHorarios.RowDataBound
        If e.Row.RowType = DataControlRowType.DataRow Then
            ' Resaltar horarios no disponibles
            Dim disponible As Boolean = Convert.ToBoolean(DataBinder.Eval(e.Row.DataItem, "Disponible"))
            If Not disponible Then
                e.Row.CssClass = "table-warning"
            End If

            ' Agregar botón de eliminar
            Dim btnEliminar As Button = New Button()
            btnEliminar.Text = "Eliminar"
            btnEliminar.CssClass = "btn btn-danger btn-sm"
            btnEliminar.CommandName = "Delete"
            btnEliminar.OnClientClick = "return confirm('¿Está seguro de eliminar este horario?');"

            e.Row.Cells(3).Controls.Add(btnEliminar)
        End If
    End Sub

    ' Recargar horarios
    Protected Sub btnRecargar_Click(sender As Object, e As EventArgs)
        CargarHorarios()
    End Sub

    'Cargar horarios por rango de fechas
    Private Sub CargarHorariosPorRango(fechaInicio As DateTime, fechaFin As DateTime)
        Try
            Dim doctorId As Integer = ObtenerDoctorId()
            Dim repo As New HorarioRepository()

            Dim horarios As DataTable = repo.GetByDoctorYFecha(doctorId, fechaInicio, fechaFin)

            gvHorarios.DataSource = horarios
            gvHorarios.DataBind()

        Catch ex As Exception
            Throw New Exception("Error al cargar horarios por rango: " & ex.Message)
        End Try
    End Sub





End Class