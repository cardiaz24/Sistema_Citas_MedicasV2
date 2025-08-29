Imports System
Imports System.Collections.Generic
Imports System.Data
Imports System.Data.SqlClient
Imports System.Security.Cryptography
Imports System.Text

Public Class PacienteRepository
    Private ReadOnly db As New DatabaseHelper()

    ' Obtener todos los pacientes
    Public Function GetAll() As DataTable
        Try
            Dim sql As String =
                "SELECT p.Id, p.Nombre, p.Apellidos, p.Email, 
                        pa.Direccion, pa.Telefono, pa.FechaRegistro
                 FROM Personas p
                 INNER JOIN Pacientes pa ON p.Id = pa.PersonaId
                 ORDER BY p.Apellidos, p.Nombre"

            Return db.ExecuteQuery(sql)
        Catch ex As Exception
            Throw New Exception("Error al obtener pacientes: " & ex.Message)
        End Try
    End Function

    ' Obtener un paciente por ID
    Public Function GetById(id As Integer) As DataTable
        Try
            Dim sql As String =
                "SELECT p.Id, p.Nombre, p.Apellidos, p.Email, 
                        pa.Direccion, pa.Telefono, pa.FechaRegistro
                 FROM Personas p
                 INNER JOIN Pacientes pa ON p.Id = pa.PersonaId
                 WHERE p.Id = @Id"

            Return db.ExecuteQuery(sql, New List(Of SqlParameter) From {
                db.CreateParameter("@Id", id)
            })
        Catch ex As Exception
            Throw New Exception("Error al obtener paciente por ID: " & ex.Message)
        End Try
    End Function

    ' Insertar nuevo paciente
    Public Function Insert(paciente As Paciente) As Boolean
        Using conn As SqlConnection = db.GetConnection()
            conn.Open()
            Using transaction As SqlTransaction = conn.BeginTransaction()
                Try
                    ' 1. Hashear la contraseña
                    Dim hashedPassword = HashPassword(paciente.Password)

                    ' 2. Insertar en Personas
                    Dim sqlPersona As String =
                        "INSERT INTO Personas (Nombre, Apellidos, Email, Password, FechaCreacion)
                         OUTPUT INSERTED.Id
                         VALUES (@Nombre, @Apellidos, @Email, @Password, GETDATE())"

                    Dim personaParams As New List(Of SqlParameter) From {
                        db.CreateParameter("@Nombre", paciente.Nombre),
                        db.CreateParameter("@Apellidos", paciente.Apellidos),
                        db.CreateParameter("@Email", paciente.Email),
                        db.CreateParameter("@Password", hashedPassword)
                    }

                    Dim personaId As Integer = Convert.ToInt32(
                        db.ExecuteScalar(transaction, sqlPersona, personaParams))

                    ' 3. Insertar en Pacientes
                    Dim sqlPaciente As String =
                        "INSERT INTO Pacientes (PersonaId, Direccion, Telefono, FechaRegistro)
                         VALUES (@PersonaId, @Direccion, @Telefono, GETDATE())"

                    Dim pacienteParams As New List(Of SqlParameter) From {
                        db.CreateParameter("@PersonaId", personaId),
                        db.CreateParameter("@Direccion", paciente.Direccion),
                        db.CreateParameter("@Telefono", paciente.Telefono)
                    }

                    db.ExecuteNonQuery(transaction, sqlPaciente, pacienteParams)

                    ' 4. Asignar rol de Paciente
                    Dim rolSql As String =
                        "INSERT INTO PersonaRoles (PersonaId, RolId, Activo)
                         VALUES (@PersonaId, (SELECT Id FROM Roles WHERE Nombre = 'Paciente'), 1)"

                    Dim rolParams As New List(Of SqlParameter) From {
                        db.CreateParameter("@PersonaId", personaId)
                    }

                    db.ExecuteNonQuery(transaction, rolSql, rolParams)

                    transaction.Commit()
                    Return True

                Catch ex As Exception
                    transaction.Rollback()
                    Throw New Exception("Error al insertar paciente: " & ex.Message)
                End Try
            End Using
        End Using
    End Function

    ' Actualiza paciente (Persona y Pacientes)
    Public Function Update(paciente As Paciente) As Boolean
        Using conn As SqlConnection = db.GetConnection()
            conn.Open()
            Using transaction As SqlTransaction = conn.BeginTransaction()
                Try
                    ' 1. Verificar si se está actualizando la contraseña
                    Dim currentPassword As String = GetCurrentPassword(paciente.Id, transaction)
                    Dim passwordToUse As String = If(
                        String.IsNullOrEmpty(paciente.Password),
                        currentPassword,
                        HashPassword(paciente.Password)
                    )

                    ' 2. Actualiza tabla Personas
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
                        db.CreateParameter("@Password", passwordToUse)
                    }

                    db.ExecuteNonQuery(transaction, sqlPersona, personaParams)

                    ' 3. Actualiza tabla Pacientes
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

                    db.ExecuteNonQuery(transaction, sqlPaciente, pacienteParams)

                    transaction.Commit()
                    Return True

                Catch ex As Exception
                    transaction.Rollback()
                    Throw New Exception("Error al actualizar paciente: " & ex.Message)
                End Try
            End Using
        End Using
    End Function

    ' Eliminar paciente de Pacientes y Personas
    Public Function Delete(id As Integer) As Boolean
        Using conn As SqlConnection = db.GetConnection()
            conn.Open()
            Using transaction As SqlTransaction = conn.BeginTransaction()
                Try
                    ' 1. Verificar si tiene citas activas
                    Dim citasSql As String =
                        "SELECT COUNT(*) FROM Citas WHERE PacienteId = @Id AND Estado IN ('Pendiente', 'Confirmada')"

                    Dim citasCount As Integer = Convert.ToInt32(
                        db.ExecuteScalar(transaction, citasSql,
                            New List(Of SqlParameter) From {
                                db.CreateParameter("@Id", id)
                            }))

                    If citasCount > 0 Then
                        Throw New Exception("No se puede eliminar el paciente porque tiene citas activas")
                    End If

                    ' 2. Eliminar de PersonaRoles
                    Dim rolesSql As String = "DELETE FROM PersonaRoles WHERE PersonaId = @Id"
                    db.ExecuteNonQuery(transaction, rolesSql,
                        New List(Of SqlParameter) From {
                            db.CreateParameter("@Id", id)
                        })

                    ' 3. Eliminar de Pacientes
                    Dim pacienteSql As String = "DELETE FROM Pacientes WHERE PersonaId = @Id"
                    db.ExecuteNonQuery(transaction, pacienteSql,
                        New List(Of SqlParameter) From {
                            db.CreateParameter("@Id", id)
                        })

                    ' 4. Eliminar de Personas
                    Dim personaSql As String = "DELETE FROM Personas WHERE Id = @Id"
                    db.ExecuteNonQuery(transaction, personaSql,
                        New List(Of SqlParameter) From {
                            db.CreateParameter("@Id", id)
                        })

                    transaction.Commit()
                    Return True

                Catch ex As Exception
                    transaction.Rollback()
                    Throw New Exception("Error al eliminar paciente: " & ex.Message)
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

    'Obtener contraseña actual para no sobreescribir si no se proporciona nueva
    Private Function GetCurrentPassword(personaId As Integer, transaction As SqlTransaction) As String
        Dim sql As String = "SELECT Password FROM Personas WHERE Id = @Id"

        Dim result As Object = db.ExecuteScalar(transaction, sql,
            New List(Of SqlParameter) From {
                db.CreateParameter("@Id", personaId)
            })

        Return If(result IsNot Nothing, result.ToString(), String.Empty)
    End Function


    '
    Public Function GetPacientesConCitas() As DataTable
        Try
            Dim sql As String =
                "SELECT DISTINCT p.Id, p.Nombre, p.Apellidos, p.Email,
                        pa.Direccion, pa.Telefono, COUNT(c.Id) AS TotalCitas
                 FROM Personas p
                 INNER JOIN Pacientes pa ON p.Id = pa.PersonaId
                 LEFT JOIN Citas c ON p.Id = c.PacienteId
                 GROUP BY p.Id, p.Nombre, p.Apellidos, p.Email, pa.Direccion, pa.Telefono
                 HAVING COUNT(c.Id) > 0
                 ORDER BY p.Apellidos, p.Nombre"

            Return db.ExecuteQuery(sql)
        Catch ex As Exception
            Throw New Exception("Error al obtener pacientes con citas: " & ex.Message)
        End Try
    End Function

    Public Function BuscarPacientes(terminoBusqueda As String) As DataTable
        Try
            Dim sql As String =
                "SELECT p.Id, p.Nombre, p.Apellidos, p.Email,
                        pa.Direccion, pa.Telefono, pa.FechaRegistro
                 FROM Personas p
                 INNER JOIN Pacientes pa ON p.Id = pa.PersonaId
                 WHERE p.Nombre LIKE @Termino OR p.Apellidos LIKE @Termino OR p.Email LIKE @Termino
                 ORDER BY p.Apellidos, p.Nombre"

            Return db.ExecuteQuery(sql, New List(Of SqlParameter) From {
                db.CreateParameter("@Termino", "%" & terminoBusqueda & "%")
            })
        Catch ex As Exception
            Throw New Exception("Error al buscar pacientes: " & ex.Message)
        End Try
    End Function
End Class