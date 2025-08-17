Imports System
Imports System.Collections.Generic
Imports System.Data
Imports System.Data.SqlClient

Public Class PacienteRepository
    Private ReadOnly db As New DatabaseHelper()

    ' Obtener todos los pacientes
    Public Function GetAll() As DataTable
        Dim sql As String =
            "SELECT p.Id, p.Nombre, p.Apellidos, p.Email, pa.Direccion, pa.Telefono, pa.FechaRegistro
             FROM Personas p
             INNER JOIN Pacientes pa ON p.Id = pa.PersonaId
             ORDER BY p.Id DESC"

        Return db.ExecuteQuery(sql)
    End Function

    ' Obtener un paciente por ID
    Public Function GetById(id As Integer) As DataTable
        Dim sql As String =
            "SELECT TOP 1 p.Id, p.Nombre, p.Apellidos, p.Email, pa.Direccion, pa.Telefono, pa.FechaRegistro
             FROM Personas p
             INNER JOIN Pacientes pa ON p.Id = pa.PersonaId
             WHERE p.Id = @Id"

        Return db.ExecuteQuery(sql, New List(Of SqlParameter) From {
            db.CreateParameter("@Id", id)
        })
    End Function

    ' Insertar nuevo paciente (en Personas y Pacientes)
    Public Sub Insert(paciente As Paciente)
        ' Insertar en Personas
        Dim sqlPersona As String =
            "INSERT INTO Personas (Nombre, Apellidos, Email, Password)
             VALUES (@Nombre, @Apellidos, @Email, @Password);
             SELECT SCOPE_IDENTITY();"

        Dim personaParams As New List(Of SqlParameter) From {
            db.CreateParameter("@Nombre", paciente.Nombre),
            db.CreateParameter("@Apellidos", paciente.Apellidos),
            db.CreateParameter("@Email", paciente.Email),
            db.CreateParameter("@Password", paciente.Password)
        }

        Dim dt = db.ExecuteQuery(sqlPersona, personaParams)
        Dim personaId As Integer = Convert.ToInt32(dt.Rows(0)(0))

        ' Insertar en Pacientes
        Dim sqlPaciente As String =
            "INSERT INTO Pacientes (PersonaId, Direccion, Telefono, FechaRegistro)
             VALUES (@PersonaId, @Direccion, @Telefono, GETDATE())"

        Dim pacienteParams As New List(Of SqlParameter) From {
            db.CreateParameter("@PersonaId", personaId),
            db.CreateParameter("@Direccion", paciente.Direccion),
            db.CreateParameter("@Telefono", paciente.Telefono)
        }

        db.ExecuteNonQuery(sqlPaciente, pacienteParams)
    End Sub

    ' Actualizar paciente (Persona y Pacientes)
    Public Sub Update(paciente As Paciente)
        ' Actualizar tabla Personas
        Dim sqlPersona As String =
            "UPDATE Personas
             SET Nombre = @Nombre,
                 Apellidos = @Apellidos,
                 Email = @Email,
                 Password = @Password
             WHERE Id = @Id"

        Dim personaParams As New List(Of SqlParameter) From {
            db.CreateParameter("@Id", paciente.Id),
            db.CreateParameter("@Nombre", paciente.Nombre),
            db.CreateParameter("@Apellidos", paciente.Apellidos),
            db.CreateParameter("@Email", paciente.Email),
            db.CreateParameter("@Password", paciente.Password)
        }

        db.ExecuteNonQuery(sqlPersona, personaParams)

        ' Actualizar tabla Pacientes (sin tocar FechaRegistro)
        Dim sqlPaciente As String =
            "UPDATE Pacientes
             SET Direccion = @Direccion,
                 Telefono = @Telefono
             WHERE PersonaId = @Id"

        Dim pacienteParams As New List(Of SqlParameter) From {
            db.CreateParameter("@Id", paciente.Id),
            db.CreateParameter("@Direccion", paciente.Direccion),
            db.CreateParameter("@Telefono", paciente.Telefono)
        }

        db.ExecuteNonQuery(sqlPaciente, pacienteParams)
    End Sub

    ' Eliminar paciente de Pacientes y Personas
    Public Sub Delete(id As Integer)
        ' Primero eliminar de Pacientes
        Dim sqlPaciente As String = "DELETE FROM Pacientes WHERE PersonaId = @Id"
        db.ExecuteNonQuery(sqlPaciente, New List(Of SqlParameter) From {
            db.CreateParameter("@Id", id)
        })

        ' Luego eliminar de Personas
        Dim sqlPersona As String = "DELETE FROM Personas WHERE Id = @Id"
        db.ExecuteNonQuery(sqlPersona, New List(Of SqlParameter) From {
            db.CreateParameter("@Id", id)
        })
    End Sub
End Class
