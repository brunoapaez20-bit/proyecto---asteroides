using System.Data;
using MySqlConnector;

public class JugadorRepository
{
    private readonly string _connectionString;

    // XAMPP por defecto: usuario root, sin contraseña, puerto 3306
    public JugadorRepository(string connectionString =
        "Server=localhost;Port=3306;Database=videojuego;Uid=root;Pwd=;")
    {
        _connectionString = connectionString;
    }

    public bool Desactivar(int idJugador) => EjecutarSp("sp_DesactivarJugador", idJugador);
    public bool Reactivar(int idJugador)  => EjecutarSp("sp_ReactivarJugador", idJugador);

    private bool EjecutarSp(string nombreSp, int idJugador)
    {
        using var conn = new MySqlConnection(_connectionString);
        using var cmd = new MySqlCommand(nombreSp, conn)
        {
            CommandType = CommandType.StoredProcedure
        };
        cmd.Parameters.AddWithValue("p_id_jugador", idJugador);

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
        using var conn = new MySqlConnection(_connectionString);
        using var cmd = new MySqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@u", usuario);
        cmd.Parameters.AddWithValue("@p", passwordHash);

        conn.Open();
        return Convert.ToInt32(cmd.ExecuteScalar()) == 1;
    }

    public int IniciarSesion(int idJugador)
    {
        const string sql = "INSERT INTO Sesiones (id_jugador) VALUES (@id); SELECT LAST_INSERT_ID();";
        using var conn = new MySqlConnection(_connectionString);
        using var cmd = new MySqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@id", idJugador);

        conn.Open();
        try
        {
            return Convert.ToInt32(cmd.ExecuteScalar());
        }
        catch (MySqlException ex) when (ex.SqlState == "45000")
        {
            // Disparado por el trigger: jugador desactivado
            throw new InvalidOperationException(ex.Message);
        }
    }
}
