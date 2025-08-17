Imports System
Imports System.Collections.Generic
Imports System.Data
Imports System.Data.SqlClient

Public Class DoctorRepository
    Private ReadOnly db As New DatabaseHelper()

    Public Function GetAll() As DataTable
        Dim sql As String =
            "SELECT p.Id, p.Nombre, p.Apellidos, p.Email, d.Especialidad, d.Activo
             FROM Personas p
             INNER JOIN Doctores d ON p.Id = d.PersonaId"
        Return db.ExecuteQuery(sql)
    End Function

    Public Sub Insert(doctor As Doctor)
        ' Insertar en Personas
        Dim personaSql As String =
            "INSERT INTO Personas (Nombre, Apellidos, Email, Password)
             VALUES (@Nombre, @Apellidos, @Email, @Password);
             SELECT SCOPE_IDENTITY();"

        Dim personaParams = New List(Of SqlParameter) From {
            db.CreateParameter("@Nombre", doctor.Nombre),
            db.CreateParameter("@Apellidos", doctor.Apellidos),
            db.CreateParameter("@Email", doctor.Email),
            db.CreateParameter("@Password", doctor.Password)
        }

        Dim dt = db.ExecuteQuery(personaSql, personaParams)
        Dim personaId = Convert.ToInt32(dt.Rows(0)(0))

        ' Insertar en Doctores
        Dim doctorSql As String =
            "INSERT INTO Doctores (PersonaId, Especialidad, Activo)
             VALUES (@PersonaId, @Especialidad, @Activo)"

        Dim doctorParams = New List(Of SqlParameter) From {
            db.CreateParameter("@PersonaId", personaId),
            db.CreateParameter("@Especialidad", doctor.Especialidad),
            db.CreateParameter("@Activo", doctor.Activo)
        }

        db.ExecuteNonQuery(doctorSql, doctorParams)
    End Sub
End Class
