using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Plus.Utilities
{
    public class CGrafo
    {
        private int[,] mAdyacencia;
        private int[] indegree;
        private int nodos;

        public CGrafo(int pNodos)
        {
            nodos = pNodos;

            // Instanciamos matriz de adyacencia
            mAdyacencia = new int[nodos, nodos];

            // Instanciamos arreglo de indegree
            indegree = new int[nodos];
        }

        public void AdicionaArista(int pNodoInicio, int pNodoFinal)
        {
            mAdyacencia[pNodoInicio, pNodoFinal] = 1;
        } 

        public void AdicionaArista(int pNodoInicio, int pNodoFinal, int pPeso)
        {
            mAdyacencia[pNodoInicio, pNodoFinal] = pPeso;
        }

        public void MuestraAdyacencia()
        {
            Console.ForegroundColor = ConsoleColor.Yellow;

            for (int n = 0; n < nodos; n++)
                Console.Write("\t{0}", n);

            Console.WriteLine();

            for(int n = 0; n<nodos; n++)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.Write(n);
                for(int m = 0; m<nodos; m++)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.Write("\t{0}", mAdyacencia[n, m]);
                }
                Console.WriteLine();
            }
        }

        public int ObtenAdyacencia(int pFila, int pColumna)
        {
            return mAdyacencia[pFila, pColumna];
        }

        public void CalcularIndegree()
        {
            for(int n = 0; n < nodos; n++)
            {
                for(int m = 0; m < nodos; m++)
                {
                    if (mAdyacencia[m, n] == 1)
                        indegree[n]++;
                }
            }
        }

        public void MostrarIndegree()
        {
            Console.ForegroundColor = ConsoleColor.White;
            for (int n = 0; n < nodos; n++)
                Console.WriteLine("Nodo: {0}, {1}", n, indegree[n]);
        }

        public int EncuentraI0()
        {            
            for(int n = 0; n < nodos; n++)
            {
                if (indegree[n] == 0)
                    return n;
            }
            
            return -1; // Código que indica que no se ha encontrado
        }

        public void DecrementaIndigree(int pNodo)
        {
            indegree[pNodo] = -1;

            for(int n = 0; n < nodos; n++)
            {
                if (mAdyacencia[pNodo, n] == 1)
                    indegree[n]--;
            }
        }
    }
}
