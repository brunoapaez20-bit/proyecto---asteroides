using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace juego_def
{
    // Partida interrumpida (se guarda en respaldo.bin cada 3 segundos)
    public class Respaldo
    {
        public float T, Puntos, NaveX, NaveY;
        public int Esquivados, Cerca;
        public float[] Fx = new float[4];   // tiene que estar inicializado, si no da NullReferenceException
    }
}
