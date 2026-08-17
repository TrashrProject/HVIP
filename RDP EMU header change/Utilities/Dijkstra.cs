using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using log4net;
using System.Runtime.CompilerServices;
using Plus.Communication.Packets.Outgoing.Alfas;
using System.Net.NetworkInformation;
using Plus.HabboRoleplay.Misc;
using System.IO;

namespace Plus.Utilities
{
    public class Dijkstra
    {
        private static ILog log = LogManager.GetLogger("Plus.Utilities");

        int inicio;
        int final;
        int distancia;
        int n;
        int cantNodos;
        int actual;
        int columna;
        CGrafo miGrafo;

        #region OLD OFF 
        /*
        private int rango = 0;
        private int[,] L;   // Matriz de adyacencia
        private int[] C;    // Arreglo de nodos
        private int[] D;    // Arreglo de distancias
        private int trango = 0;

        public Dijkstra()
        {
        }

        public Dijkstra(int paramRango, int[,] paramArreglo)
        {
            L = new int[paramRango, paramRango];
            C = new int[paramRango];
            D = new int[paramRango];
            rango = paramRango;

            // Llenado de matriz
            for(int i = 0; i<rango; i++)
            {
                for(int j = 0; j<rango; j++)
                {
                    L[i, j] = paramArreglo[i, j];
                }
            }

            // Llenado de nodos
            for(int i = 0; i <rango; i++)
            {
                C[i] = i;
            }

            C[0] = -1;

            for (int i = 1; i < rango; i++)
                D[i] = L[0, i];
        }

        // Rutina de solución
        public void SolDijkstra()
        {
            int minValor = Int32.MaxValue;
            int minNodo = 0;

            for(int i = 0; i<rango; i++)
            {
                if (C[i] == -1)
                    continue;

                if(D[i] > 0 && D[i] < minValor)
                {
                    minValor = D[i];
                    minNodo = i;
                }
            }

            C[minNodo] = -1;

            for(int i = 0; i<rango; i++)
            {
                if (L[minNodo, i] < 0) // Si no existe arco
                    continue;

                if(D[i] < 0) // Si no hay un peso asignado
                {
                    D[i] = minValor + L[minNodo, i];
                    continue;
                }

                if ((D[minNodo] + L[minNodo, i]) < D[i])
                    D[i] = minValor + L[minNodo, i];
            }
        }

        // Función de implementación del algoritmo
        public void CorrerDijkstra()
        {
            for(trango = 1; trango < rango; trango++)
            {
                SolDijkstra();
                Console.WriteLine("Iteración No. " + trango);
                Console.WriteLine("Matriz de distancias: ");

                for (int i = 0; i < rango; i++)
                    Console.WriteLine(i + " ");

                Console.WriteLine("");

                for (int i = 0; i<rango; i++)
                    Console.Write(D[i] + "");

                Console.WriteLine("");
                Console.WriteLine("");
            }
        }

        public void Init()
        {
            // Definición de la matriz de adyacencia
            int[,] L =
            {
                {-1, 10, 18, -1, -1, -1, -1},
                {-1, -1, 6, -1, 3, -1, -1},
                {-1, -1, -1, 3, -1, 20, -1},
                {-1, -1, 2, -1, -1, -1, 2},
                {-1, -1, -1, 6, -1, -1, 10},
                {-1, -1, -1, -1, -1, -1, -1},
                {-1, -1, 10, -1, -1, 5, -1}
            };

            Dijkstra prueba = new Dijkstra((int)Math.Sqrt(L.Length), L);
            prueba.CorrerDijkstra();

            Console.WriteLine("La solución de la ruta más corta tomando como nodo inicial el NODO 1 es: ");

            int nodo = 1;
            foreach(int i in prueba.D)
            {
                Console.Write("Distancia mínima a nodo "+nodo+" es ");
                Console.WriteLine(i);
                nodo++;
            }

            Console.WriteLine();

            log.Info("Loaded Dijkstra Algorithm");
        }
        */
        #endregion

        public Dijkstra()
        {
            inicio = 0;
            final = 0;
            distancia = 0;
            n = 0;
            cantNodos = (LoadCsv(RoleplayManager.TaxiBotCSV).GetLength(0) - 1);
            actual = 0;
            columna = 0;
            miGrafo = new CGrafo(cantNodos);
        }

        private string[,] LoadCsv(string filename)
        {
            // Get the file's text.
            string whole_file = System.IO.File.ReadAllText(filename);

            // Split into lines.
            whole_file = whole_file.Replace('\n', '\r');
            string[] lines = whole_file.Split(new char[] { '\r' },
                StringSplitOptions.RemoveEmptyEntries);

            // See how many rows and columns there are.
            int num_rows = lines.Length;
            int num_cols = lines[0].Split(',').Length;

            // Allocate the data array.
            string[,] values = new string[num_rows, num_cols];

            // Load the array.
            for (int r = 0; r < num_rows; r++)
            {
                string[] line_r = lines[r].Split(',');
                for (int c = 0; c < num_cols; c++)
                {
                    values[r, c] = line_r[c];
                }
            }

            // Return the values.
            return values;
        }

        public void Init()
        {
            // Cargamos la matriz de adyacencia desde el CSV
            for (int i = 1; i < LoadCsv(RoleplayManager.TaxiBotCSV).GetLength(0); i++)
            {
                for(int j = 1; j < LoadCsv(RoleplayManager.TaxiBotCSV).GetLength(0); j++)
                {
                    miGrafo.AdicionaArista((i - 1), (j - 1), Convert.ToInt32(LoadCsv(RoleplayManager.TaxiBotCSV)[i, j]));
                }
            }

            //miGrafo.MuestraAdyacencia();
            log.Info("Loaded Dijkstra Algorithm");
        }

        // Obtenemos list con los nodos de la ruta solución
        public List<int> RunDijkstra(int origen, int destino)
        {
            inicio = origen;
            final = destino;

            // Creamos la tabla
            // 0 - Visitado
            // 1 - Distancia
            // 2 - Previo
            int[,] tabla = new int[cantNodos, 3];

            // Inicializamos la tabla
            for (n = 0; n < cantNodos; n++)
            {
                tabla[n, 0] = 0;
                tabla[n, 1] = int.MaxValue;
                tabla[n, 2] = 0;
            }

            tabla[inicio, 1] = 0;

            //MostrarTabla(tabla);

            // Inicio Dijkstra
            actual = inicio;
            do
            {
                // Marcar nodo como visitado
                tabla[actual, 0] = 1;

                for (columna = 0; columna < cantNodos; columna++)
                {
                    // Buscamos a quién se dirige
                    if (miGrafo.ObtenAdyacencia(actual, columna) != 0)
                    {
                        // Calculamos la distancia
                        distancia = miGrafo.ObtenAdyacencia(actual, columna) + tabla[actual, 1];

                        // Colocamos las distancias
                        if (distancia < tabla[columna, 1])
                        {
                            tabla[columna, 1] = distancia;

                            // Colocamos la información de padre
                            tabla[columna, 2] = actual;
                        }
                    }
                }

                // El nuevo actual es el nodo con la menor distancia que no ha sido visitado
                int indiceMenor = -1;
                int distanciaMenor = int.MaxValue;

                for (int x = 0; x < cantNodos; x++)
                {
                    if (tabla[x, 1] < distanciaMenor && tabla[x, 0] == 0)
                    {
                        indiceMenor = x;
                        distanciaMenor = tabla[x, 1];
                    }
                }

                actual = indiceMenor;

            } while (actual != -1);

            //MostrarTabla(tabla);

            // Obtenemos la ruta            
            List<int> ruta = new List<int>();
            int nodo = final;

            while (nodo != inicio)
            {
                ruta.Add(nodo);
                nodo = tabla[nodo, 2];
            }

            ruta.Add(inicio);
            ruta.Reverse();

            //foreach (int posicion in ruta)
            //  Console.Write("{0}->", posicion);

            return ruta;
        }

        public static void MostrarTabla(int[,] pTabla)
        {
            for(int n = 0; n < pTabla.GetLength(0); n++)
            {
                Console.WriteLine("{0}-> {1}\t{2}\t{3}", n, pTabla[n, 0], pTabla[n, 1], pTabla[n, 2]);
            }

            Console.WriteLine("------");
        }
    }
}
