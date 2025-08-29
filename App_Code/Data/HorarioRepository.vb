Imports System.Collections.Generic
Imports System.Data
Imports System.Data.SqlClient

Public Class HorarioRepository
    Private ReadOnly db As New DatabaseHelper()




    Public Function GetByDoctor(doctorId As Integer) As DataTable
        Try
            Dim sql As String =
                "SELECT h.Id, h.DoctorId, h.Fecha, h.Disponible, 
                        p.Nombre + ' ' + p.Apellidos AS DoctorNombre
                 FROM Horarios h
                 INNER JOIN Doctores d ON h.DoctorId = d.PersonaId
                 INNER JOIN Personas p ON d.PersonaId = p.Id
                 WHERE h.DoctorId = @DoctorId 
                 AND h.Fecha >= DATEADD(DAY, -1, GETDATE()) -- Incluir hoy y futuros
                 ORDER BY h.Fecha ASC"

            Return db.ExecuteQuery(
    query:=sql,
    parameters:=New List(Of SqlParameter) From {db.CreateParameter("@DoctorId", doctorId)})
        Catch ex As Exception
            Throw New Exception("Error al obtener horarios del doctor: " & ex.Message)
        End Try
    End Function



    Public Function GetDisponiblesPorDoctor(doctorId As Integer) As DataTable
        Try
            Dim sql As String =
            "SELECT h.Id, h.Fecha, 
                    CONVERT(VARCHAR, h.Fecha, 103) + ' ' + 
                    CONVERT(VARCHAR, h.Fecha, 108) AS FechaFormateada,
                    p.Nombre + ' ' + p.Apellidos AS DoctorNombre
             FROM Horarios h
             INNER JOIN Doctores d ON h.DoctorId = d.PersonaId
             INNER JOIN Personas p ON d.PersonaId = p.Id
             WHERE h.DoctorId = @DoctorId 
             AND h.Disponible = 1 
             AND h.Fecha > DATEADD(HOUR, 24, GETDATE()) -- Solo horarios con 24h de anticipación
             ORDER BY h.Fecha ASC"

            Return db.ExecuteQuery(
    query:=sql,
    parameters:=New List(Of SqlParameter) From {db.CreateParameter("@DoctorId", doctorId)})
        Catch ex As Exception
            Throw New Exception("Error al obtener horarios disponibles: " & ex.Message)
        End Try
    End Function


    Public Function Insert(horario As Horario) As Boolean
        Try
            ' Valida que el doctor existe y está activo
            Dim validarSql As String =
                "SELECT COUNT(*) FROM Doctores 
                 WHERE PersonaId = @DoctorId AND Activo = 1"

            Dim count As Integer = Convert.ToInt32(db.ExecuteScalar(validarSql,
                New List(Of SqlParameter) From {
                    db.CreateParameter("@DoctorId", horario.DoctorId)
                }))

            If count = 0 Then
                Throw New Exception("El doctor no existe o no está activo")
            End If

            ' Valida que no exista un horario duplicado
            Dim duplicadoSql As String =
                "SELECT COUNT(*) FROM Horarios 
                 WHERE DoctorId = @DoctorId AND Fecha = @Fecha"

            Dim duplicados As Integer = Convert.ToInt32(db.ExecuteScalar(duplicadoSql,
                New List(Of SqlParameter) From {
                    db.CreateParameter("@DoctorId", horario.DoctorId),
                    db.CreateParameter("@Fecha", horario.Fecha)
                }))

            If duplicados > 0 Then
                Throw New Exception("Ya existe un horario para esta fecha y doctor")
            End If

            ' Insertar el horario
            Dim sql As String =
                "INSERT INTO Horarios (DoctorId, Fecha, Disponible)
                 VALUES (@DoctorId, @Fecha, @Disponible)"

            Dim affectedRows As Integer = db.ExecuteNonQuery(sql,
                New List(Of SqlParameter) From {
                    db.CreateParameter("@DoctorId", horario.DoctorId),
                    db.CreateParameter("@Fecha", horario.Fecha),
                    db.CreateParameter("@Disponible", horario.Disponible)
                })

            Return affectedRows > 0

        Catch ex As Exception
            Throw New Exception("Error al insertar horario: " & ex.Message)
        End Try
    End Function

    Public Function MarcarComoNoDisponible(horarioId As Integer) As Boolean
        Try
            Dim sql As String =
                "UPDATE Horarios SET Disponible = 0 WHERE Id = @Id"

            Dim affectedRows As Integer = db.ExecuteNonQuery(sql,
                New List(Of SqlParameter) From {
                    db.CreateParameter("@Id", horarioId)
                })

            Return affectedRows > 0

        Catch ex As Exception
            Throw New Exception("Error al marcar horario como no disponible: " & ex.Message)
        End Try
    End Function

    Public Function MarcarComoDisponible(horarioId As Integer) As Boolean
        Try
            Dim sql As String =
                "UPDATE Horarios SET Disponible = 1 WHERE Id = @Id"

            Dim affectedRows As Integer = db.ExecuteNonQuery(sql,
                New List(Of SqlParameter) From {
                    db.CreateParameter("@Id", horarioId)
                })

            Return affectedRows > 0

        Catch ex As Exception
            Throw New Exception("Error al marcar horario como disponible: " & ex.Message)
        End Try
    End Function

    Public Function GetHorariosProximos(dias As Integer) As DataTable
        Try
            Dim sql As String =
                "SELECT h.Id, h.Fecha, h.Disponible,
                        p.Nombre + ' ' + p.Apellidos AS DoctorNombre,
                        d.Especialidad
                 FROM Horarios h
                 INNER JOIN Doctores d ON h.DoctorId = d.PersonaId
                 INNER JOIN Personas p ON d.PersonaId = p.Id
                 WHERE h.Fecha BETWEEN GETDATE() AND DATEADD(DAY, @Dias, GETDATE())
                 ORDER BY h.Fecha ASC"

            Return db.ExecuteQuery(sql, New List(Of SqlParameter) From {
                db.CreateParameter("@Dias", dias)
            })
        Catch ex As Exception
            Throw New Exception("Error al obtener horarios próximos: " & ex.Message)
        End Try
    End Function

    Public Function EliminarHorariosPasados() As Integer
        Try
            Dim sql As String =
                "DELETE FROM Horarios WHERE Fecha < DATEADD(DAY, -1, GETDATE())"

            Return db.ExecuteNonQuery(sql)

        Catch ex As Exception
            Throw New Exception("Error al eliminar horarios pasados: " & ex.Message)
        End Try
    End Function

    Public Function ExisteCitaEnHorario(horarioId As Integer) As Boolean
        Try
            Dim sql As String =
                "SELECT COUNT(*) FROM Citas WHERE HorarioId = @HorarioId"

            Dim count As Integer = Convert.ToInt32(db.ExecuteScalar(sql,
                New List(Of SqlParameter) From {
                    db.CreateParameter("@HorarioId", horarioId)
                }))

            Return count > 0

        Catch ex As Exception
            Throw New Exception("Error al verificar citas en horario: " & ex.Message)
        End Try
    End Function

    Public Function GetByDoctorYFecha(doctorId As Integer, fechaInicio As DateTime, fechaFin As DateTime) As DataTable
        Try
            Dim sql As String =
                "SELECT h.Id, h.DoctorId, h.Fecha, h.Disponible,
                    p.Nombre + ' ' + p.Apellidos AS DoctorNombre
             FROM Horarios h
             INNER JOIN Doctores d ON h.DoctorId = d.PersonaId
             INNER JOIN Personas p ON d.PersonaId = p.Id
             WHERE h.DoctorId = @DoctorId
               AND h.Fecha BETWEEN @Inicio AND @Fin
             ORDER BY h.Fecha ASC"
            Return db.ExecuteQuery(sql, New List(Of SqlParameter) From {
                db.CreateParameter("@DoctorId", doctorId),
                db.CreateParameter("@Inicio", fechaInicio),
                db.CreateParameter("@Fin", fechaFin)
            })
        Catch ex As Exception
            Throw New Exception("Error al obtener horarios por rango: " & ex.Message)
        End Try
    End Function

End Class