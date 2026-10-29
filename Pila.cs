using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace juego_def
{
    // PILA (LIFO): el ultimo en entrar es el primero en salir.
    // Uso: historial de eventos; en la pantalla final se muestran los ultimos.
    public class Pila<T>
    {
        Nodo<T> _tope;
        public int Cantidad { get; private set; }
        public bool EstaVacia { get { return _tope == null; } }

        public void Apilar(T valor)
        {
            Nodo<T> n = new Nodo<T>(valor);
            n.Siguiente = _tope;
            _tope = n;
            Cantidad++;
        }

        public T Desapilar()
        {
            if (_tope == null) throw new InvalidOperationException("La pila esta vacia");
            T v = _tope.Valor;
            _tope = _tope.Siguiente;
            Cantidad--;
            return v;
        }

        public void Vaciar() { _tope = null; Cantidad = 0; }
    }
}
