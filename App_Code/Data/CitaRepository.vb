Imports System.Collections.Generic
Imports System.Data
Imports System.Data.SqlClient

Public Class CitaRepository
    Private ReadOnly db As New DatabaseHelper()

    Public Function GetCitasPorPaciente(pacienteId As Integer) As DataTable
        Dim sql As String =
            "SELECT c.Id, c.Estado, h.Fecha, d.PersonaId AS DoctorId, p.Nombre + ' ' + p.Apellidos AS DoctorNombre
             FROM Citas c
             INNER JOIN Horarios h ON c.HorarioId = h.Id
             INNER JOIN Doctores d ON h.DoctorId = d.PersonaId
             INNER JOIN Personas p ON d.PersonaId = p.Id
             WHERE c.PacienteId = @PacienteId
             ORDER BY h.Fecha ASC"

        Return db.ExecuteQuery(sql, New List(Of SqlParameter) From {
            db.CreateParameter("@PacienteId", pacienteId)
        })
    End Function

    Public Sub ReservarCita(pacienteId As Integer, horarioId As Integer)
        Dim sql As String =
            "INSERT INTO Citas (PacienteId, HorarioId, Estado)
             VALUES (@PacienteId, @HorarioId, 'Pendiente');
             UPDATE Horarios SET Disponible = 0 WHERE Id = @HorarioId"

        Dim p = New List(Of SqlParameter) From {
            db.CreateParameter("@PacienteId", pacienteId),
            db.CreateParameter("@HorarioId", horarioId)
        }

        db.ExecuteNonQuery(sql, p)
    End Sub

    Public Sub CambiarEstadoCita(citaId As Integer, nuevoEstado As String)
        Dim sql As String =
            "UPDATE Citas SET Estado = @Estado WHERE Id = @CitaId"

        db.ExecuteNonQuery(sql, New List(Of SqlParameter) From {
            db.CreateParameter("@Estado", nuevoEstado),
            db.CreateParameter("@CitaId", citaId)
        })
    End Sub

    Public Sub EliminarCitaSiEsFutura(citaId As Integer)
        Dim sql As String = "
            DECLARE @HorarioId INT, @Fecha DATETIME;

            SELECT @HorarioId = HorarioId FROM Citas WHERE Id = @CitaId;
            SELECT @Fecha = Fecha FROM Horarios WHERE Id = @HorarioId;

            IF @Fecha > GETDATE()
            BEGIN
                UPDATE Horarios SET Disponible = 1 WHERE Id = @HorarioId;
                DELETE FROM Citas WHERE Id = @CitaId;
            END"

        db.ExecuteNonQuery(sql, New List(Of SqlParameter) From {
            db.CreateParameter("@CitaId", citaId)
        })
    End Sub
End Class
