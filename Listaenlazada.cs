using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace juego_def
{
    // LISTA ENLAZADA SIMPLE
    // Uso: asteroides y power-ups activos en pantalla (insercion/eliminacion
    // eficiente sin redimensionar arreglos).
    public class ListaEnlazada<T> : IEnumerable<T>
    {
        Nodo<T> _primero, _ultimo;
        public int Cantidad { get; private set; }

        public void Agregar(T valor)
        {
            Nodo<T> n = new Nodo<T>(valor);
            if (_primero == null) { _primero = n; _ultimo = n; }
            else { _ultimo.Siguiente = n; _ultimo = n; }
            Cantidad++;
        }

        // Elimina todos los elementos que cumplan la condicion. Devuelve cuantos quito.
        public int RemoverDonde(Predicate<T> condicion)
        {
            int quitados = 0;
            Nodo<T> anterior = null, actual = _primero;
            while (actual != null)
            {
                if (condicion(actual.Valor))
                {
                    if (anterior == null) _primero = actual.Siguiente;
                    else anterior.Siguiente = actual.Siguiente;
                    if (actual == _ultimo) _ultimo = anterior;
                    Cantidad--;
                    quitados++;
                }
                else
                {
                    anterior = actual;
                }
                actual = actual.Siguiente;
            }
            return quitados;
        }

        public void Vaciar() { _primero = null; _ultimo = null; Cantidad = 0; }

        public IEnumerator<T> GetEnumerator()
        {
            for (Nodo<T> n = _primero; n != null; n = n.Siguiente)
                yield return n.Valor;
        }

        IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }
    }
}
