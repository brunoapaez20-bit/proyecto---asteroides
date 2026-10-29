public class ColaEnlazada<T>
{
    private class Nodo
    {
        public T Dato;
        public Nodo? Siguiente;
        public Nodo(T dato) { Dato = dato; }
    }

    private Nodo? _frente;
    private Nodo? _final;

    public int Cantidad { get; private set; }
    public bool EstaVacia => _frente == null;

    // O(1)
    public void Encolar(T dato)
    {
        var nuevo = new Nodo(dato);
        if (_final == null)
            _frente = _final = nuevo;
        else
        {
            _final.Siguiente = nuevo;
            _final = nuevo;
        }
        Cantidad++;
    }

    // O(1)
    public T Desencolar()
    {
        if (_frente == null)
            throw new InvalidOperationException("La cola está vacía.");

        T dato = _frente.Dato;
        _frente = _frente.Siguiente;
        if (_frente == null) _final = null;
        Cantidad--;
        return dato;
    }

    // O(1)
    public T Frente()
    {
        if (_frente == null)
            throw new InvalidOperationException("La cola está vacía.");
        return _frente.Dato;
    }
}