Imports System
Imports System.Data
Imports System.Configuration

Public Class MisHorarios
    Inherits System.Web.UI.Page

    Private Sub RequireRole(ParamArray roles() As String)
        If Session("UsuarioId") Is Nothing Then Response.Redirect("~/Login.aspx")
        Dim rol As String = Convert.ToString(Session("UsuarioRol"))
        If Not roles.Any(Function(r) r.Equals(rol, StringComparison.OrdinalIgnoreCase)) Then
            Response.Redirect("~/Default.aspx")
        End If
    End Sub

    Protected Sub Page_Load(sender As Object, e As EventArgs) Handles Me.Load
        RequireRole("Doctor")
        If Not IsPostBack Then BindGrid()
    End Sub

    Private Sub BindGrid()
        Dim repo As New HorarioRepository()
        gvHorarios.DataSource = repo.GetByDoctor(Convert.ToInt32(Session("UsuarioId")))
        gvHorarios.DataBind()
    End Sub

    Protected Sub btnAgregar_Click(sender As Object, e As EventArgs) Handles btnAgregar.Click
        Dim fecha As DateTime
        If DateTime.TryParse(txtFecha.Text, fecha) Then
            Dim repo As New HorarioRepository()
            repo.Insert(New Horario With {
                .DoctorId = Convert.ToInt32(Session("UsuarioId")),
                .fecha = fecha,
                .Disponible = True
            })
            BindGrid()
        End If
    End Sub
End Class
