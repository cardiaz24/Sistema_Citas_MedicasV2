Imports System
Imports System.Collections.Generic
Imports System.Data
Imports System.Data.SqlClient

Public Class AdministradorRepository
    Private ReadOnly db As New DatabaseHelper()

    Public Function GetAll() As DataTable
        Dim sql As String =
            "SELECT p.Id, p.Nombre, p.Apellidos, p.Email, a.FechaRegistro, a.NivelAcceso, a.Activo
             FROM Personas p
             INNER JOIN Administradores a ON p.Id = a.PersonaId"
        Return db.ExecuteQuery(sql)
    End Function

    Public Sub Insert(admin As Administrador)
        ' Insertar en Personas
        Dim personaSql As String =
            "INSERT INTO Personas (Nombre, Apellidos, Email, Password)
             VALUES (@Nombre, @Apellidos, @Email, @Password);
             SELECT SCOPE_IDENTITY();"

        Dim personaParams = New List(Of SqlParameter) From {
            db.CreateParameter("@Nombre", admin.Nombre),
            db.CreateParameter("@Apellidos", admin.Apellidos),
            db.CreateParameter("@Email", admin.Email),
            db.CreateParameter("@Password", admin.Password)
        }

        Dim dt = db.ExecuteQuery(personaSql, personaParams)
        Dim personaId = Convert.ToInt32(dt.Rows(0)(0))

        ' Insertar en Administradores
        Dim adminSql As String =
            "INSERT INTO Administradores (PersonaId, FechaRegistro, NivelAcceso, Activo)
             VALUES (@PersonaId, GETDATE(), @NivelAcceso, @Activo)"

        Dim adminParams = New List(Of SqlParameter) From {
            db.CreateParameter("@PersonaId", personaId),
            db.CreateParameter("@NivelAcceso", admin.NivelAcceso),
            db.CreateParameter("@Activo", admin.Activo)
        }

        db.ExecuteNonQuery(adminSql, adminParams)
    End Sub
End Class
