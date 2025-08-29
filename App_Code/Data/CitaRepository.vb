Imports System.Collections.Generic
Imports System.Data
Imports System.Data.SqlClient


Public Class CitaRepository
    Private ReadOnly db As New DatabaseHelper()

    Public Function GetCitasPorPaciente(pacienteId As Integer) As DataTable
        Try
            Dim sql As String = "sp_VerCitasPorPaciente"
            Return db.ExecuteQuery(
    query:=sql,
    parameters:=New List(Of SqlParameter) From {db.CreateParameter("@PacienteId", pacienteId)},
    isStoredProcedure:=True)
        Catch ex As Exception
            Throw New Exception("Error al obtener citas del paciente: " & ex.Message)
        End Try
    End Function

    Public Function ReservarCita(pacienteId As Integer, horarioId As Integer) As Boolean
        Using conn As SqlConnection = db.GetConnection()
            conn.Open()
            Using transaction As SqlTransaction = conn.BeginTransaction()
                Try
                    ' 1. Verificar si el horario existe y está disponible
                    Dim verificarSql As String =
                        "SELECT COUNT(*) FROM Horarios WHERE Id = @HorarioId AND Disponible = 1"

                    Dim count As Integer = Convert.ToInt32(db.ExecuteScalar(transaction, verificarSql,
                        New List(Of SqlParameter) From {
                            db.CreateParameter("@HorarioId", horarioId)
                        }))

                    If count = 0 Then
                        Throw New Exception("El horario no está disponible o no existe")
                    End If

                    ' 2. Reservar la cita usando transacción
                    Dim sql As String = "sp_ReservarCita"

                        db.ExecuteNonQuery(transaction, sql, New List(Of SqlParameter) From {
                            db.CreateParameter("@PacienteId", pacienteId),
                            db.CreateParameter("@HorarioId", horarioId)
                        }, True) ' isStoredProcedure = True

                        transaction.Commit()
                        Return True


                Catch ex As Exception
                    transaction.Rollback()
                    Throw New Exception("Error al reservar cita: " & ex.Message)
                End Try
            End Using
        End Using
    End Function

    Public Sub CambiarEstadoCita(citaId As Integer, nuevoEstado As String)
        Try
            ' Validar estado válido
            Dim estadosValidos As String() = {"Pendiente", "Confirmada", "Completada", "Cancelada", "NoPresentado"}
            If Not Array.Exists(estadosValidos, Function(e) e.Equals(nuevoEstado, StringComparison.OrdinalIgnoreCase)) Then
                Throw New ArgumentException("Estado de cita no válido")
            End If

            Dim sql As String = "sp_ActualizarEstadoCita"

            db.ExecuteNonQuery(sql, New List(Of SqlParameter) From {
                db.CreateParameter("@CitaId", citaId),
                db.CreateParameter("@NuevoEstado", nuevoEstado)
            }, True) ' isStoredProcedure = True

        Catch ex As Exception
            Throw New Exception("Error al cambiar estado de cita: " & ex.Message)
        End Try
    End Sub

    Public Function EliminarCitaSiEsFutura(citaId As Integer) As Boolean
        Try
            ' sp en lugar de lógica en código
            Dim sql As String = "sp_EliminarCita"

            Dim affectedRows As Integer = db.ExecuteNonQuery(sql, New List(Of SqlParameter) From {
                db.CreateParameter("@CitaId", citaId)
            }, True) ' isStoredProcedure = True

            Return affectedRows > 0

        Catch ex As Exception
            Throw New Exception("Error al eliminar cita: " & ex.Message)
        End Try
    End Function

    'Verificar disponibilidad de horario
    Public Function VerificarDisponibilidadHorario(horarioId As Integer) As Boolean
        Try
            Dim sql As String =
                "SELECT Disponible FROM Horarios WHERE Id = @HorarioId AND Fecha > GETDATE()"

            Dim result As Object = db.ExecuteScalar(sql, New List(Of SqlParameter) From {
                db.CreateParameter("@HorarioId", horarioId)
            })

            Return result IsNot Nothing AndAlso Convert.ToBoolean(result)

        Catch ex As Exception
            Throw New Exception("Error al verificar disponibilidad: " & ex.Message)
        End Try
    End Function

    'Obtener horarios disponibles por doctor
    Public Function GetHorariosDisponiblesPorDoctor(doctorId As Integer) As DataTable
        Try
            Dim sql As String =
                "SELECT h.Id, h.Fecha, p.Nombre + ' ' + p.Apellidos AS DoctorNombre
                 FROM Horarios h
                 INNER JOIN Doctores d ON h.DoctorId = d.PersonaId
                 INNER JOIN Personas p ON d.PersonaId = p.Id
                 WHERE h.DoctorId = @DoctorId 
                 AND h.Disponible = 1 
                 AND h.Fecha > GETDATE()
                 ORDER BY h.Fecha ASC"

            Return db.ExecuteQuery(sql, New List(Of SqlParameter) From {db.CreateParameter("@DoctorId", doctorId)})
        Catch ex As Exception
            Throw New Exception("Error al obtener horarios disponibles: " & ex.Message)
        End Try
    End Function



    Public Function GetCitaById(citaId As Integer) As DataTable
        Try
            Dim sql As String =
                "SELECT c.Id, c.Estado, c.FechaCreacion,
                    h.Fecha
             FROM Citas c
             INNER JOIN Horarios h ON c.HorarioId = h.Id
             WHERE c.Id = @Id"
            Return db.ExecuteQuery(sql, New List(Of SqlParameter) From {db.CreateParameter("@Id", citaId)})
        Catch ex As Exception
            Throw New Exception("Error al obtener cita por ID: " & ex.Message)
        End Try

    End Function









End Class