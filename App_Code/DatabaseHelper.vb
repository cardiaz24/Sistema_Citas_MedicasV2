Imports System
Imports System.Collections.Generic
Imports System.Configuration
Imports System.Data
Imports System.Data.SqlClient


Public Class DatabaseHelper
    Private ReadOnly connectionString As String = ConfigurationManager.ConnectionStrings("ConexionSt").ConnectionString



    Public Sub New()
        EnsureErrorLogTableExists()
    End Sub

    'Método para obtener la conexión (SIN abrirla automáticamente)
    Public Function GetConnection() As SqlConnection
        Return New SqlConnection(connectionString)
    End Function

    'MÉTODOS SOBRECARGADOS CON SOPORTE PARA TRANSACCIONES 

    ' ExecuteNonQuery con transacción
    Public Function ExecuteNonQuery(transaction As SqlTransaction, query As String,
                                   Optional parameters As List(Of SqlParameter) = Nothing,
                                   Optional isStoredProcedure As Boolean = False) As Integer
        If String.IsNullOrWhiteSpace(query) Then
            Throw New ArgumentException("La consulta no puede estar vacía.")
        End If

        Using cmd As New SqlCommand(query, transaction.Connection, transaction)
            If parameters IsNot Nothing Then
                cmd.Parameters.AddRange(parameters.ToArray())
            End If
            If isStoredProcedure Then
                cmd.CommandType = CommandType.StoredProcedure
            End If

            Try
                Return cmd.ExecuteNonQuery()
            Catch ex As Exception
                LogError(ex, query)
                Throw New Exception("Error al ejecutar el comando: " & ex.Message)
            End Try
        End Using
    End Function

    'ExecuteNonQuery sin transacción (abre su propia conexión)
    Public Function ExecuteNonQuery(query As String,
                                   Optional parameters As List(Of SqlParameter) = Nothing,
                                   Optional isStoredProcedure As Boolean = False) As Integer
        Using conn As SqlConnection = GetConnection()
            conn.Open()
            Using cmd As New SqlCommand(query, conn)
                If parameters IsNot Nothing Then
                    cmd.Parameters.AddRange(parameters.ToArray())
                End If
                If isStoredProcedure Then
                    cmd.CommandType = CommandType.StoredProcedure
                End If

                Try
                    Return cmd.ExecuteNonQuery()
                Catch ex As Exception
                    LogError(ex, query)
                    Throw New Exception("Error al ejecutar el comando: " & ex.Message)
                End Try
            End Using
        End Using
    End Function

    'ExecuteQuery con transacción
    Public Overloads Function ExecuteQuery(transaction As SqlTransaction,
                                       query As String,
                                       Optional parameters As List(Of SqlParameter) = Nothing,
                                       Optional isStoredProcedure As Boolean = False) As DataTable
        If String.IsNullOrWhiteSpace(query) Then
            Throw New ArgumentException("La consulta no puede estar vacía.")
        End If

        Dim dt As New DataTable()
        Using cmd As New SqlCommand(query, transaction.Connection, transaction)
            If parameters IsNot Nothing Then cmd.Parameters.AddRange(parameters.ToArray())
            If isStoredProcedure Then cmd.CommandType = CommandType.StoredProcedure
            Using adapter As New SqlDataAdapter(cmd)
                adapter.Fill(dt)
            End Using
        End Using
        Return dt
    End Function

    'ExecuteScalar con transacción
    Public Function ExecuteScalar(transaction As SqlTransaction, query As String,
                                 Optional parameters As List(Of SqlParameter) = Nothing,
                                 Optional isStoredProcedure As Boolean = False) As Object
        If String.IsNullOrWhiteSpace(query) Then
            Throw New ArgumentException("La consulta no puede estar vacía.")
        End If

        Using cmd As New SqlCommand(query, transaction.Connection, transaction)
            If parameters IsNot Nothing Then
                cmd.Parameters.AddRange(parameters.ToArray())
            End If
            If isStoredProcedure Then
                cmd.CommandType = CommandType.StoredProcedure
            End If

            Try
                Return cmd.ExecuteScalar()
            Catch ex As Exception
                LogError(ex, query)
                Throw New Exception("Error al ejecutar ExecuteScalar: " & ex.Message)
            End Try
        End Using
    End Function

    'ExecuteScalar sin transacción
    Public Function ExecuteScalar(query As String,
                                 Optional parameters As List(Of SqlParameter) = Nothing,
                                 Optional isStoredProcedure As Boolean = False) As Object
        Using conn As SqlConnection = GetConnection()
            conn.Open()
            Using cmd As New SqlCommand(query, conn)
                If parameters IsNot Nothing Then
                    cmd.Parameters.AddRange(parameters.ToArray())
                End If
                If isStoredProcedure Then
                    cmd.CommandType = CommandType.StoredProcedure
                End If

                Try
                    Return cmd.ExecuteScalar()
                Catch ex As Exception
                    LogError(ex, query)
                    Throw New Exception("Error al ejecutar ExecuteScalar: " & ex.Message)
                End Try
            End Using
        End Using
    End Function

    'MÉTODOS AUXILIARES.

    Private Sub EnsureErrorLogTableExists()
        Dim query As String = "
            IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'ErrorLog')
            BEGIN
                CREATE TABLE ErrorLog (
                    Id INT IDENTITY(1,1) PRIMARY KEY,
                    ErrorMessage NVARCHAR(4000),
                    ErrorSeverity INT,
                    ErrorState INT,
                    ErrorProcedure NVARCHAR(200),
                    ErrorLine INT,
                    ErrorDateTime DATETIME DEFAULT GETDATE()
                )
            END"

        ExecuteNonQuery(query)
    End Sub
    'ExecuteQuery sin transacción
    Public Overloads Function ExecuteQuery(query As String,
                                      Optional parameters As List(Of SqlParameter) = Nothing,
                                      Optional isStoredProcedure As Boolean = False) As DataTable
        Using conn As SqlConnection = GetConnection()
            conn.Open()
            Return ExecuteQueryInternal(conn, query, parameters, isStoredProcedure)
        End Using
    End Function


    Private Function ExecuteQueryInternal(conn As SqlConnection,
                                      query As String,
                                      Optional parameters As List(Of SqlParameter) = Nothing,
                                      Optional isStoredProcedure As Boolean = False) As DataTable
        Dim dt As New DataTable()
        Using cmd As New SqlCommand(query, conn)
            If parameters IsNot Nothing Then cmd.Parameters.AddRange(parameters.ToArray())
            If isStoredProcedure Then cmd.CommandType = CommandType.StoredProcedure
            Using da As New SqlDataAdapter(cmd)
                da.Fill(dt)
            End Using
        End Using
        Return dt
    End Function







    'LogError usa parámetros parametrizados
    Private Sub LogError(ex As Exception, Optional query As String = "")
        Dim fullMessage As String = $"Message: {ex.Message}" & Environment.NewLine &
                                    $"StackTrace: {ex.StackTrace}" & Environment.NewLine &
                                    $"InnerException: {If(ex.InnerException IsNot Nothing, ex.InnerException.Message, "N/A")}" & Environment.NewLine &
                                    $"Query: {query}"

        Dim severity As Integer = 16
        Dim state As Integer = 1
        Dim procedureName As String = If(ex.TargetSite IsNot Nothing, ex.TargetSite.Name, DBNull.Value.ToString())
        Dim lineNumber As Integer = 0

        If ex.StackTrace IsNot Nothing AndAlso ex.StackTrace.Contains(":line ") Then
            Dim partes = ex.StackTrace.Split(":line ")
            Dim ultimaParte = partes(partes.Length - 1)
            Integer.TryParse(ultimaParte.Split(" "c)(0), lineNumber)
        End If

        Dim logQuery As String = "
        INSERT INTO ErrorLog (ErrorMessage, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine) 
        VALUES (@ErrorMessage, @ErrorSeverity, @ErrorState, @ErrorProcedure, @ErrorLine)"

        Dim parameters As New List(Of SqlParameter) From {
            CreateParameter("@ErrorMessage", fullMessage),
            CreateParameter("@ErrorSeverity", severity),
            CreateParameter("@ErrorState", state),
            CreateParameter("@ErrorProcedure", procedureName),
            CreateParameter("@ErrorLine", lineNumber)
        }

        Try
            ExecuteNonQuery(logQuery, parameters)
        Catch logEx As Exception
            LogErrorToFile(fullMessage)
        End Try
    End Sub

    Private Sub LogErrorToFile(message As String)
        Try
            Dim path As String = "C:\Logs\error_log.txt"
            ' Asegurar que el directorio existe
            System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(path))
            System.IO.File.AppendAllText(path, $"{DateTime.Now}: {message}{Environment.NewLine}")
        Catch
            ' Si falla el log a archivo, no hay mucho más que hacer
        End Try
    End Sub

    Public Function CreateParameter(name As String, value As Object) As SqlParameter
        Return New SqlParameter(name, If(value IsNot Nothing, value, DBNull.Value))
    End Function


End Class