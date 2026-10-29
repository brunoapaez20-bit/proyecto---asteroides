using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace juego_def
{
    public class Nodo<T>
    {
        public T Valor;
        public Nodo<T> Siguiente;
        public Nodo(T valor) { Valor = valor; }
    }
}
