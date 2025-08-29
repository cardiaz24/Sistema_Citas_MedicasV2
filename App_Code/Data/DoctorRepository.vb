Imports System
Imports System.Collections.Generic
Imports System.Data
Imports System.Data.SqlClient
Imports System.Security.Cryptography
Imports System.Text


Public Class DoctorRepository
    Private ReadOnly db As New DatabaseHelper()

    Public Function GetAll() As DataTable
        Try
            Dim sql As String =
            "SELECT p.Id, p.Nombre + ' ' + p.Apellidos AS NombreCompleto, 
                    d.Especialidad, d.Activo
             FROM Personas p
             INNER JOIN Doctores d ON p.Id = d.PersonaId
             WHERE d.Activo = 1
             ORDER BY p.Apellidos, p.Nombre"

            Return db.ExecuteQuery(sql)
        Catch ex As Exception
            Throw New Exception("Error al obtener doctores: " & ex.Message)
        End Try
    End Function


    Public Function Insert(doctor As Doctor) As Boolean
        Using conn As SqlConnection = db.GetConnection()
            conn.Open()
            Using transaction As SqlTransaction = conn.BeginTransaction()
                Try
                    ' 1. Hashear la contraseña
                    Dim hashedPassword = HashPassword(doctor.Password)

                    ' 2. Insertar en Personas
                    Dim personaSql As String =
                        "INSERT INTO Personas (Nombre, Apellidos, Email, Password, FechaCreacion)
                         OUTPUT INSERTED.Id
                         VALUES (@Nombre, @Apellidos, @Email, @Password, GETDATE())"

                    Dim personaParams = New List(Of SqlParameter) From {
                        db.CreateParameter("@Nombre", doctor.Nombre),
                        db.CreateParameter("@Apellidos", doctor.Apellidos),
                        db.CreateParameter("@Email", doctor.Email),
                        db.CreateParameter("@Password", hashedPassword)
                    }

                    Dim personaId As Integer = Convert.ToInt32(
                        db.ExecuteScalar(transaction, personaSql, personaParams))

                    ' 3. Insertar en Doctores
                    Dim doctorSql As String =
                        "INSERT INTO Doctores (PersonaId, Especialidad, Activo)
                         VALUES (@PersonaId, @Especialidad, @Activo)"

                    Dim doctorParams = New List(Of SqlParameter) From {
                        db.CreateParameter("@PersonaId", personaId),
                        db.CreateParameter("@Especialidad", doctor.Especialidad),
                        db.CreateParameter("@Activo", doctor.Activo)
                    }

                    db.ExecuteNonQuery(transaction, doctorSql, doctorParams)

                    ' 4. Asignar rol de Doctor
                    Dim rolSql As String =
                        "INSERT INTO PersonaRoles (PersonaId, RolId, Activo)
                         VALUES (@PersonaId, (SELECT Id FROM Roles WHERE Nombre = 'Doctor'), 1)"

                    Dim rolParams = New List(Of SqlParameter) From {
                        db.CreateParameter("@PersonaId", personaId)
                    }

                    db.ExecuteNonQuery(transaction, rolSql, rolParams)

                    transaction.Commit()
                    Return True

                Catch ex As Exception
                    transaction.Rollback()
                    Throw New Exception("Error al insertar doctor: " & ex.Message)
                End Try
            End Using
        End Using
    End Function

    'Función para hashear contraseñas
    Private Function HashPassword(password As String) As String
        Using sha256 As SHA256 = SHA256.Create()
            Dim bytes = Encoding.UTF8.GetBytes(password)
            Dim hash = sha256.ComputeHash(bytes)
            Return Convert.ToBase64String(hash)
        End Using
    End Function



    Public Function GetByEspecialidad(especialidad As String) As DataTable
        Try
            Dim sql As String =
                "SELECT p.Id, p.Nombre, p.Apellidos, p.Email, d.Especialidad
                 FROM Personas p
                 INNER JOIN Doctores d ON p.Id = d.PersonaId
                 WHERE d.Especialidad = @Especialidad AND d.Activo = 1
                 ORDER BY p.Nombre, p.Apellidos"

            Return db.ExecuteQuery(
    query:=sql,
    parameters:=New List(Of SqlParameter) From {
                db.CreateParameter("@Especialidad", especialidad)
            })
        Catch ex As Exception
            Throw New Exception("Error al obtener doctores por especialidad: " & ex.Message)
        End Try
    End Function

    Public Function ActivarDesactivarDoctor(personaId As Integer, activo As Boolean) As Boolean
        Using conn As SqlConnection = db.GetConnection()
            conn.Open()
            Using transaction As SqlTransaction = conn.BeginTransaction()
                Try
                    ' Actualizar estado en Doctores
                    Dim sql As String =
                        "UPDATE Doctores SET Activo = @Activo WHERE PersonaId = @PersonaId"

                    Dim params = New List(Of SqlParameter) From {
                        db.CreateParameter("@Activo", activo),
                        db.CreateParameter("@PersonaId", personaId)
                    }

                    db.ExecuteNonQuery(transaction, sql, params)

                    ' También actualizar el rol si es necesario
                    Dim rolSql As String =
                        "UPDATE PersonaRoles SET Activo = @Activo 
                         WHERE PersonaId = @PersonaId 
                         AND RolId = (SELECT Id FROM Roles WHERE Nombre = 'Doctor')"

                    db.ExecuteNonQuery(transaction, rolSql, params)

                    transaction.Commit()
                    Return True

                Catch ex As Exception
                    transaction.Rollback()
                    Throw New Exception("Error al actualizar estado del doctor: " & ex.Message)
                End Try
            End Using
        End Using
    End Function

    Public Function GetDoctorById(personaId As Integer) As DataTable
        Try
            Dim sql As String =
                "SELECT p.Id, p.Nombre, p.Apellidos, p.Email, d.Especialidad, d.Activo
                 FROM Personas p
                 INNER JOIN Doctores d ON p.Id = d.PersonaId
                 WHERE p.Id = @PersonaId"

            Return db.ExecuteQuery(sql, New List(Of SqlParameter) From {
                db.CreateParameter("@PersonaId", personaId)
            })
        Catch ex As Exception
            Throw New Exception("Error al obtener doctor por ID: " & ex.Message)
        End Try
    End Function
End Class