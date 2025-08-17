Imports System
Imports System.Data
Public Class Pacientes
    Inherits System.Web.UI.Page

    Private Sub RequireRole(ParamArray roles() As String)
        If Session("UsuarioId") Is Nothing Then Response.Redirect("~/Login.aspx")
        Dim rol As String = Convert.ToString(Session("UsuarioRol"))
        If Not roles.Any(Function(r) r.Equals(rol, StringComparison.OrdinalIgnoreCase)) Then
            Response.Redirect("~/Default.aspx")
        End If
    End Sub

    Protected Sub Page_Load(sender As Object, e As EventArgs) Handles Me.Load
        RequireRole("Paciente")
        If Not IsPostBack Then
            Dim repo As New CitaRepository()
            Dim dt As DataTable = repo.GetCitasPorPaciente(Convert.ToInt32(Session("UsuarioId")))
            gvCitas.DataSource = dt
            gvCitas.DataBind()
        End If
    End Sub
End Class