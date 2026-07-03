using Melon.Models;
using Microsoft.Data.Sqlite;

namespace Melon.Services;

public class Sqlite
{
    private static readonly string _path = Path.Combine("/Data", "refresh.db");
    private readonly string _connectUri = $"Data Source={_path}";
    
    private async Task<SqliteConnection> EstablishConnection()
    {
        Console.WriteLine($"BaseDirectory: {AppContext.BaseDirectory}");
        Console.WriteLine($"Database path: {_path}");
        Console.WriteLine($"Directory: {Path.GetDirectoryName(_path)}");
        Console.WriteLine($"Directory exists: {Directory.Exists(Path.GetDirectoryName(_path)!)}");
        string? directory = Path.GetDirectoryName(_path);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }
        SqliteConnection connection = new(_connectUri);
        await connection.OpenAsync();
        return connection;
    }

    public async Task CreateTable()
    {
        await using SqliteConnection connection = await EstablishConnection();
        await using SqliteCommand command = connection.CreateCommand();
        command.CommandText = """
             CREATE TABLE IF NOT EXISTS tasks (
              UserId TEXT NOT NULL, 
              discordId TEXT NOT NULL
            );
            """;
        await command.ExecuteNonQueryAsync();
    }

    public async Task AddToTask(string userId, ulong discordId)
    {
        await RemoveFromTask(discordId);
        await using SqliteConnection connection = await EstablishConnection();
        await using SqliteCommand command = connection.CreateCommand();
        command.CommandText = """
                INSERT INTO  tasks (UserId, discordId)
                VALUES ($UserId, $discordId)
            """;
        command.Parameters.AddWithValue("$UserId", userId);
        command.Parameters.AddWithValue("$discordId", discordId);
        await command.ExecuteNonQueryAsync();
    }

    public async Task RemoveFromTask(ulong discordId)
    {
        await using SqliteConnection connection = await EstablishConnection();
        await using SqliteCommand command = connection.CreateCommand();
        command.CommandText = """
              DELETE FROM tasks
              WHERE discordId = $discordId
            """;
        command.Parameters.AddWithValue("$discordId", discordId);
        await command.ExecuteNonQueryAsync();
    }

    public async Task<List<UserTasks>> GetTasks()
    {
        await using SqliteConnection connection = await EstablishConnection();
        await using SqliteCommand command = connection.CreateCommand();
        List<UserTasks> users = new();
        command.CommandText = """
            SELECT UserId, discordId
            FROM tasks
            """;
        await using var reader = await command.ExecuteReaderAsync();
        while (reader.Read())
        {
            users.Add(
                new UserTasks()
                {
                    UserId = reader.GetString(0),
                    DiscordId = (ulong)reader.GetInt64(1),
                }
            );
        }

        return users;
    }
}
