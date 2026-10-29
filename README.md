public class JugadorRepository
{
    private readonly string _connectionString;

    public JugadorRepository(string connectionString)
        => _connectionString = connectionString;

    public bool Desactivar(int idJugador)
        => EjecutarSp("sp_DesactivarJugador", idJugador);

    public bool Reactivar(int idJugador)
        => EjecutarSp("sp_ReactivarJugador", idJugador);

    private bool EjecutarSp(string nombreSp, int idJugador)
    {
        using var conn = new SqlConnection(_connectionString);
        using var cmd = new SqlCommand(nombreSp, conn)
        {
            CommandType = CommandType.StoredProcedure
        };
        cmd.Parameters.AddWithValue("@id_jugador", idJugador);

        conn.Open();
        int filas = Convert.ToInt32(cmd.ExecuteScalar());
        return filas > 0;
    }

    public bool Login(string usuario, string passwordHash)
    {
        const string sql = @"SELECT COUNT(1) FROM Jugadores
                             WHERE nombre_usuario = @u
                               AND password_hash = @p
                               AND activo = 1";
        using var conn = new SqlConnection(_connectionString);
        using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@u", usuario);
        cmd.Parameters.AddWithValue("@p", passwordHash);

        conn.Open();
        return Convert.ToInt32(cmd.ExecuteScalar()) == 1;
    }
}
