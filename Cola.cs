using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace juego_def
{
    // COLA (FIFO): el primero en entrar es el primero en salir.
    // Uso: asteroides pendientes de aparecer y buffer de eventos.
    public class Cola<T>
    {
        Nodo<T> _frente, _final;
        public int Cantidad { get; private set; }
        public bool EstaVacia { get { return _frente == null; } }

        public void Encolar(T valor)
        {
            Nodo<T> n = new Nodo<T>(valor);
            if (_final == null) { _frente = n; _final = n; }
            else { _final.Siguiente = n; _final = n; }
            Cantidad++;
        }

        public T Desencolar()
        {
            if (_frente == null) throw new InvalidOperationException("La cola esta vacia");
            T v = _frente.Valor;
            _frente = _frente.Siguiente;
            if (_frente == null) _final = null;
            Cantidad--;
            return v;
        }

        public void Vaciar() { _frente = null; _final = null; Cantidad = 0; }
    }

}
