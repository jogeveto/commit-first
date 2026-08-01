using Npgsql;

namespace Empleabilidad.Api.Auth;

/// Repositorio de `users` sobre PostgreSQL (design D3). El alta es idempotente por
/// `linkedin_sub UNIQUE`: INSERT ... ON CONFLICT DO NOTHING resuelve la carrera
/// entre el Find y el Create de AccountProvisioningService; el SELECT de respaldo
/// garantiza devolver el `User_ID` exista o no conflicto. Un fallo de conexión/SQL
/// se propaga como excepción → la orquestación lo traduce a ProvisioningFailed sin
/// dejar cuenta parcial (HU-002 AC2).
public class PostgresUserRepository : IUserRepository
{
    private readonly string _connectionString;

    public PostgresUserRepository(string connectionString) => _connectionString = connectionString;

    public async Task<Guid?> FindByLinkedinSubAsync(string linkedinSub)
    {
        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync();
        await using var cmd = new NpgsqlCommand(
            "SELECT id FROM users WHERE linkedin_sub = @sub", conn);
        cmd.Parameters.AddWithValue("sub", linkedinSub);
        var result = await cmd.ExecuteScalarAsync();
        return result is Guid id ? id : null;
    }

    public async Task<Guid> CreateAsync(string linkedinSub, string? name)
    {
        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync();
        await using var cmd = new NpgsqlCommand(@"
            INSERT INTO users (linkedin_sub, display_name)
            VALUES (@sub, @name)
            ON CONFLICT (linkedin_sub) DO NOTHING;
            SELECT id FROM users WHERE linkedin_sub = @sub;", conn);
        cmd.Parameters.AddWithValue("sub", linkedinSub);
        cmd.Parameters.AddWithValue("name", (object?)name ?? DBNull.Value);
        var result = await cmd.ExecuteScalarAsync();
        return (Guid)result!;
    }
}
