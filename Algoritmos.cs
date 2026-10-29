using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace juego_def
{
    public class Algoritmos
    {
        // ================= ORDENAMIENTO =================

        // INSERCION - O(n^2) tiempo, O(1) espacio.
        // Ordena los tipos de asteroide por Id (menor a mayor).
        // Se usa una vez al iniciar, antes de la busqueda binaria.
        public static void OrdenarPorId(TiposAsteroides[] arr)
        {
            for (int i = 1; i < arr.Length; i++)
            {
                TiposAsteroides clave = arr[i];
                int j = i - 1;
                while (j >= 0 && arr[j].Id > clave.Id)
                {
                    arr[j + 1] = arr[j];
                    j--;
                }
                arr[j + 1] = clave;
            }
        }

        // SELECCION - O(n^2) tiempo, O(1) espacio.
        // Ordena la tabla de posiciones: mayor tiempo de supervivencia primero.
        public static void OrdenarRanking(Sesion[] arr)
        {
            for (int i = 0; i < arr.Length - 1; i++)
            {
                int mayor = i;
                for (int j = i + 1; j < arr.Length; j++)
                    if (arr[j].TiempoSupervivencia > arr[mayor].TiempoSupervivencia)
                        mayor = j;

                Sesion temp = arr[i];
                arr[i] = arr[mayor];
                arr[mayor] = temp;
            }
        }

        // ================= BUSQUEDA =================

        // BINARIA - O(log n). El arreglo DEBE estar ordenado por Id.
        // Devuelve la posicion, o -1 si no existe.
        public static int BuscarPorId(TiposAsteroides[] arr, int id)
        {
            int izq = 0, der = arr.Length - 1;
            while (izq <= der)
            {
                int medio = izq + (der - izq) / 2;
                if (arr[medio].Id == id) return medio;
                if (arr[medio].Id < id) izq = medio + 1;
                else der = medio - 1;
            }
            return -1;
        }

        // SECUENCIAL - O(n). No necesita orden. Devuelve el power-up o null.
        public static TipoPowerUp BuscarPowerUp(TipoPowerUp[] arr, int id)
        {
            for (int i = 0; i < arr.Length; i++)
                if (arr[i].Id == id) return arr[i];
            return null;
        }

        // SECUENCIAL - O(n). Primera posicion del jugador (-1 si no esta).
        // Con el ranking ordenado, la primera aparicion es su mejor marca.
        public static int BuscarJugador(Sesion[] arr, string jugador)
        {
            for (int i = 0; i < arr.Length; i++)
                if (string.Equals(arr[i].Jugador, jugador, StringComparison.OrdinalIgnoreCase))
                    return i;
            return -1;
        }

        // ================= RECURSIVIDAD =================

        // Curva de dificultad: cada nivel suma 0.25 al factor anterior, con tope 3.5.
        // Caso base: nivel <= 0. Complejidad O(n) tiempo y O(n) pila de llamadas.
        public static float FactorDificultad(int nivel)
        {
            if (nivel <= 0) return 1f;
            if(nivel >= 10) return 3.5f;   // ya llego al tope
            return Math.Min(3.5f, FactorDificultad(nivel - 1) + 0.25f);
        }

        // Validacion de ruta segura sobre la grilla (array 2D).
        // Verifica que desde la celda de la nave (fila, col) exista un camino
        // libre hacia la fila 0 (por donde entran los asteroides),
        // moviendose una fila hacia arriba y -1/0/+1 columnas por paso.
        public static bool RutaLibre(bool[,] ocupado, int fila, int col)
        {
            if (fila == 0) return true;
            bool[,] visitado = new bool[ocupado.GetLength(0), ocupado.GetLength(1)];
            return HayRutaSegura(ocupado, visitado, fila - 1, col)
                || HayRutaSegura(ocupado, visitado, fila - 1, col - 1)
                || HayRutaSegura(ocupado, visitado, fila - 1, col + 1);
        }

        // Recursiva. Caso base: celda invalida/ocupada/visitada (false) o fila 0 (true).
        // Con "visitado" cada celda se evalua una sola vez: O(filas * columnas).
        static bool HayRutaSegura(bool[,] ocupado, bool[,] visitado, int fila, int col)
        {
            if (fila < 0 || col < 0 || col >= ocupado.GetLength(1)) return false;
            if (ocupado[fila, col] || visitado[fila, col]) return false;
            visitado[fila, col] = true;
            if (fila == 0) return true;
            return HayRutaSegura(ocupado, visitado, fila - 1, col)
                || HayRutaSegura(ocupado, visitado, fila - 1, col - 1)
                || HayRutaSegura(ocupado, visitado, fila - 1, col + 1);
        }
    }
}
