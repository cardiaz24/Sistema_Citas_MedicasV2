Imports System.Collections.Generic
Imports System.Data
Imports System.Data.SqlClient
Imports System

Public Class HorarioRepository
    Friend ReadOnly Property GetByDoctor(v As Integer) As Object
        Get
            Throw New NotImplementedException()
        End Get
    End Property

    Friend Sub Insert(horario As Horario)
        Throw New NotImplementedException()
    End Sub

    Public Class HorarioRepository
        Private ReadOnly db As New DatabaseHelper()

        Public Function GetByDoctor(doctorId As Integer) As DataTable
            Dim sql As String =
            "SELECT * FROM Horarios WHERE DoctorId = @DoctorId ORDER BY Fecha ASC"
            Return db.ExecuteQuery(sql, New List(Of SqlParameter) From {
            db.CreateParameter("@DoctorId", doctorId)
        })
        End Function

        Public Function GetDisponiblesPorDoctor(doctorId As Integer) As DataTable
            Dim sql As String =
            "SELECT * FROM Horarios WHERE DoctorId = @DoctorId AND Disponible = 1 ORDER BY Fecha ASC"
            Return db.ExecuteQuery(sql, New List(Of SqlParameter) From {
            db.CreateParameter("@DoctorId", doctorId)
        })
        End Function

        Public Sub Insert(horario As Horario)
            Dim sql As String =
            "INSERT INTO Horarios (DoctorId, Fecha, Disponible)
             VALUES (@DoctorId, @Fecha, @Disponible)"

            Dim p = New List(Of SqlParameter) From {
            db.CreateParameter("@DoctorId", horario.DoctorId),
            db.CreateParameter("@Fecha", horario.Fecha),
            db.CreateParameter("@Disponible", horario.Disponible)
        }

            db.ExecuteNonQuery(sql, p)
        End Sub

        Public Sub MarcarComoNoDisponible(horarioId As Integer)
            Dim sql As String =
            "UPDATE Horarios SET Disponible = 0 WHERE Id = @Id"
            db.ExecuteNonQuery(sql, New List(Of SqlParameter) From {
            db.CreateParameter("@Id", horarioId)
        })
        End Sub
    End Class

End Class
