public int ContarAlcanzablesIterativo(int filaInicio, int colInicio)
{
    if (!EnRango(filaInicio, colInicio) || Obtener(filaInicio, colInicio) == Tile.Pared)
        return 0;

    var visitado = new bool[Filas, Columnas];
    var cola = new ColaEnlazada<(int fila, int col)>();

    int[] df = { 1, -1, 0, 0 };
    int[] dc = { 0, 0, 1, -1 };

    visitado[filaInicio, colInicio] = true;
    cola.Encolar((filaInicio, colInicio));
    int contador = 0;

    while (!cola.EstaVacia)
    {
        var (f, c) = cola.Desencolar();
        contador++;

        for (int i = 0; i < 4; i++)
        {
            int nf = f + df[i];
            int nc = c + dc[i];

            if (EnRango(nf, nc) && !visitado[nf, nc] && Obtener(nf, nc) != Tile.Pared)
            {
                visitado[nf, nc] = true;
                cola.Encolar((nf, nc));
            }
        }
    }

    return contador;
}

// Verifica que desde el inicio se pueda llegar a la meta
public bool HayCaminoAMeta(int filaInicio, int colInicio)
{
    if (!EnRango(filaInicio, colInicio) || Obtener(filaInicio, colInicio) == Tile.Pared)
        return false;

    var visitado = new bool[Filas, Columnas];
    var cola = new ColaEnlazada<(int fila, int col)>();

    int[] df = { 1, -1, 0, 0 };
    int[] dc = { 0, 0, 1, -1 };

    visitado[filaInicio, colInicio] = true;
    cola.Encolar((filaInicio, colInicio));

    while (!cola.EstaVacia)
    {
        var (f, c) = cola.Desencolar();

        if (Obtener(f, c) == Tile.Meta) return true;

        for (int i = 0; i < 4; i++)
        {
            int nf = f + df[i];
            int nc = c + dc[i];

            if (EnRango(nf, nc) && !visitado[nf, nc] && Obtener(nf, nc) != Tile.Pared)
            {
                visitado[nf, nc] = true;
                cola.Encolar((nf, nc));
            }
        }
    }

    return false;
}