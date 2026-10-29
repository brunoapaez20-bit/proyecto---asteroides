using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace juego_def
{
    // Contrato de acceso a datos: hoy lo implementa RepositorioArchivos, despues puede ser RepositorioSql
    public interface IRepositorio
    {
        TiposAsteroides[] ObtenerTiposAsteroides();
        TipoPowerUp[] ObtenerTiposPowerUp();
        Sesion[] ObtenerSesiones();
        void GuardarSesion(Sesion s);
        void RegistrarEvento(EventoJuego e);
    }
}
