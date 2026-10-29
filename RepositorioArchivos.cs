using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Microsoft.Xna.Framework;

namespace juego_def
{
    // El juego solo habla con esta interfaz. Hoy la implementan los archivos;
    // cuando la BD este lista, el companero crea RepositorioSql : IRepositorio
    // y en Game1 se cambia una sola linea (new RepositorioArchivos() -> new RepositorioSql()).
    public class RepositorioArchivos : IRepositorio
    {
        public TiposAsteroides[] ObtenerTiposAsteroides()
        {
            // Datos de prueba, desordenados a proposito (los ordena OrdenarPorId)
            return new TiposAsteroides[]
            {
                new TiposAsteroides { Id = 3, Tamano = "Grande",  VelocidadBase = 1.3f, PuntosEvasion = 30 },
                new TiposAsteroides { Id = 1, Tamano = "Pequeno", VelocidadBase = 3.0f, PuntosEvasion = 10 },
                new TiposAsteroides { Id = 2, Tamano = "Mediano", VelocidadBase = 2.0f, PuntosEvasion = 20 },
            };
        }

        public TipoPowerUp[] ObtenerTiposPowerUp()
        {
            return new TipoPowerUp[]
            {
                new TipoPowerUp { Id = 1, Nombre = "Lento",     Letra = "L",  Duracion = 5f, Color = new Color(92, 225, 255) },
                new TipoPowerUp { Id = 2, Nombre = "Escudo",    Letra = "E",  Duracion = 4f, Color = new Color(255, 216, 77) },
                new TipoPowerUp { Id = 3, Nombre = "Puntos x2", Letra = "x2", Duracion = 8f, Color = new Color(125, 255, 154) },
            };
        }

        // Formato de cada linea: jugador;tiempo;esquivados;cerca;puntos;fecha
        public Sesion[] ObtenerSesiones()
        {
            List<Sesion> lista = new List<Sesion>();
            try
            {
                string ruta = Archivos.Ruta("partidas.txt");
                if (File.Exists(ruta))
                {
                    foreach (string linea in File.ReadAllLines(ruta))
                    {
                        string[] p = linea.Split(';');
                        if (p.Length < 6) continue;
                        int t, e, c, pt;
                        if (!int.TryParse(p[1], out t) || !int.TryParse(p[2], out e) ||
                            !int.TryParse(p[3], out c) || !int.TryParse(p[4], out pt)) continue;
                        lista.Add(new Sesion
                        {
                            Jugador = p[0],
                            TiempoSupervivencia = t,
                            Esquivados = e,
                            Cerca = c,
                            Puntos = pt,
                            Fecha = p[5]
                        });
                    }
                }
            }
            catch { }
            return lista.ToArray();
        }

        public void GuardarSesion(Sesion s)
        {
            try
            {
                string linea = s.Jugador.Replace(';', ',') + ";" + s.TiempoSupervivencia + ";" +
                               s.Esquivados + ";" + s.Cerca + ";" + s.Puntos + ";" + s.Fecha;
                File.AppendAllText(Archivos.Ruta("partidas.txt"), linea + Environment.NewLine);
            }
            catch { }
        }

        public void RegistrarEvento(EventoJuego e)
        {
            Archivos.Log(e.Tipo + ";t=" + e.Tiempo.ToString("0.0") + ";asteroide=" + e.AsteroideTipo +
                         ";puntos=" + e.Puntos + ";dist=" + e.Distancia.ToString("0.0"));
        }
    }
}
