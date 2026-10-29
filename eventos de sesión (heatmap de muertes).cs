using MySqlConnector;
public readonly record struct EventoSesion(
    int IdSesion,
    int Fila,
    int Columna,
    string TipoEvento);   // ej: "muerte"

using MySqlConnector;

public class ColaEventos
{
    private readonly ColaEnlazada<EventoSesion> _cola = new();
    private readonly string _connectionString;

    public ColaEventos(string connectionString =
        "Server=localhost;Port=3306;Database=videojuego;Uid=root;Pwd=;")
    {
        _connectionString = connectionString;
    }

    public int Pendientes => _cola.Cantidad;

    // Se llama durante el juego (por ejemplo, cuando el jugador muere)
    public void Registrar(int idSesion, int fila, int columna, string tipoEvento)
        => _cola.Encolar(new EventoSesion(idSesion, fila, columna, tipoEvento));

    // Se llama al cerrar la sesión: vacía la cola en orden FIFO hacia la BD
    public int GuardarEnBaseDeDatos()
    {
        if (_cola.EstaVacia) return 0;

        using var conn = new MySqlConnection(_connectionString);
        conn.Open();
        using var tx = conn.BeginTransaction();

        const string sql = @"INSERT INTO EventosSesion (id_sesion, fila, columna, tipo_evento)
                             VALUES (@s, @f, @c, @t)";
        int guardados = 0;

        try
        {
            while (!_cola.EstaVacia)
            {
                var ev = _cola.Desencolar();

                using var cmd = new MySqlCommand(sql, conn, tx);
                cmd.Parameters.AddWithValue("@s", ev.IdSesion);
                cmd.Parameters.AddWithValue("@f", ev.Fila);
                cmd.Parameters.AddWithValue("@c", ev.Columna);
                cmd.Parameters.AddWithValue("@t", ev.TipoEvento);
                cmd.ExecuteNonQuery();
                guardados++;
            }

            tx.Commit();
        }
        catch
        {
            tx.Rollback();
            throw;
        }

        return guardados;
    }
}

// Al iniciar la partida
var eventos = new ColaEventos();

// Cuando el jugador muere en una celda
int fila = (int)(jugador.Centro.Y / Mapa.TAM_TILE);
int col = (int)(jugador.Centro.X / Mapa.TAM_TILE);
eventos.Registrar(idSesion, fila, col, "muerte");

// Al cerrar la sesión
int total = eventos.GuardarEnBaseDeDatos();