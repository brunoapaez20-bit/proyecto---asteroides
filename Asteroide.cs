using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace juego_def
{
    public class Asteroide
    {
        public Vector2 Pos;
        public float VelX, Angulo, VelAng;
        public int TypeId;
        public bool Apunta;                      // true = sale dirigido hacia la nave
        public bool EnZonaCerca;                 // entro a la zona de peligro (se cuenta al esquivarlo)
        public float DistMin = float.MaxValue;   // menor distancia a la nave
        public bool Activo = true;               // false = se elimina de la lista al final del frame
    }
}
