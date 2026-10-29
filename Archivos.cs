using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace juego_def
{
    public class Archivos
    {
        public static readonly string Carpeta = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "AsteroidDodge");

        public static string Ruta(string nombre)
        {
            Directory.CreateDirectory(Carpeta);
            return Path.Combine(Carpeta, nombre);
        }

        public static string CargarJugador()
        {
            try
            {
                string ruta = Ruta("config.txt");
                if (File.Exists(ruta))
                {
                    foreach (string linea in File.ReadAllLines(ruta))
                        if (linea.StartsWith("jugador="))
                        {
                            string n = linea.Substring(8).Trim();
                            if (n.Length > 0) return n;
                        }
                }
                string porDefecto = Environment.UserName;
                File.WriteAllText(ruta, "jugador=" + porDefecto);
                return porDefecto;
            }
            catch { return "Jugador"; }
        }

        public static void Log(string linea)
        {
            try
            {
                File.AppendAllText(Ruta("log.txt"),
                    DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + linea + Environment.NewLine);
            }
            catch { }
        }

        public static void GuardarRespaldo(Respaldo r)
        {
            try
            {
                using (BinaryWriter bw = new BinaryWriter(File.Open(Ruta("respaldo.bin"), FileMode.Create)))
                {
                    bw.Write(r.T); bw.Write(r.Puntos);
                    bw.Write(r.NaveX); bw.Write(r.NaveY);
                    bw.Write(r.Esquivados); bw.Write(r.Cerca);
                    for (int i = 0; i < 4; i++) bw.Write(r.Fx[i]);
                }
            }
            catch { }
        }

        public static Respaldo CargarRespaldo()
        {
            string ruta = Ruta("respaldo.bin");
            if (!File.Exists(ruta)) return null;
            try
            {
                using (BinaryReader br = new BinaryReader(File.OpenRead(ruta)))
                {
                    Respaldo r = new Respaldo();
                    r.T = br.ReadSingle(); r.Puntos = br.ReadSingle();
                    r.NaveX = br.ReadSingle(); r.NaveY = br.ReadSingle();
                    r.Esquivados = br.ReadInt32(); r.Cerca = br.ReadInt32();
                    for (int i = 0; i < 4; i++) r.Fx[i] = br.ReadSingle();
                    return r;
                }
            }
            catch { return null; }
        }

        public static void BorrarRespaldo()
        {
            try { File.Delete(Ruta("respaldo.bin")); } catch { }
        }
    }
}
