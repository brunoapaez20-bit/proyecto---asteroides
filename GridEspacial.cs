using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace juego_def
{
    // ARRAY 2D: la pantalla se divide en celdas de 80x80 px y cada celda
    // guarda los asteroides que estan adentro. Para detectar choques o cercania
    // solo se revisan las celdas vecinas a la nave (3x3) en vez de todos los asteroides.
    public class GridEspacial
    {
        public const int Celda = 80;
        public readonly int Filas, Columnas;
        readonly List<Asteroide>[,] _celdas;

        public GridEspacial(int ancho, int alto)
        {
            Columnas = ancho / Celda;
            Filas = alto / Celda;
            _celdas = new List<Asteroide>[Filas, Columnas];
            for (int f = 0; f < Filas; f++)
                for (int c = 0; c < Columnas; c++)
                    _celdas[f, c] = new List<Asteroide>();
        }

        public void Limpiar()
        {
            for (int f = 0; f < Filas; f++)
                for (int c = 0; c < Columnas; c++)
                    _celdas[f, c].Clear();
        }

        public void ObtenerCelda(Vector2 p, out int fila, out int col)
        {
            fila = MathHelper.Clamp((int)(p.Y / Celda), 0, Filas - 1);
            col = MathHelper.Clamp((int)(p.X / Celda), 0, Columnas - 1);
        }

        public void Insertar(Asteroide a)
        {
            int f, c;
            ObtenerCelda(a.Pos, out f, out c);
            _celdas[f, c].Add(a);
        }

        // Asteroides de la celda de "p" y sus 8 vecinas
        public List<Asteroide> Cercanos(Vector2 p)
        {
            List<Asteroide> res = new List<Asteroide>();
            int f, c;
            ObtenerCelda(p, out f, out c);
            for (int df = -1; df <= 1; df++)
                for (int dc = -1; dc <= 1; dc++)
                {
                    int ff = f + df, cc = c + dc;
                    if (ff >= 0 && ff < Filas && cc >= 0 && cc < Columnas)
                        res.AddRange(_celdas[ff, cc]);
                }
            return res;
        }

        // Mapa de ocupacion (true = hay al menos un asteroide en la celda)
        public bool[,] Ocupacion()
        {
            bool[,] oc = new bool[Filas, Columnas];
            for (int f = 0; f < Filas; f++)
                for (int c = 0; c < Columnas; c++)
                    oc[f, c] = _celdas[f, c].Count > 0;
            return oc;
        }
    }
}
