using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using log4net;
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

        private string[,] _taxiMatrix;
        private string _taxiCsvPath;
        private bool _usingFallbackMatrix;
        private bool _reportedFallback;

        #region OLD OFF
        /*
        private int rango = 0;
        private int[,] L;
        private int[] C;
        private int[] D;
        private int trango = 0;
        */
        #endregion

        public Dijkstra()
        {
            inicio = 0;
            final = 0;
            distancia = 0;
            n = 0;
            actual = 0;
            columna = 0;

            _taxiMatrix = LoadCsv(RoleplayManager.TaxiBotCSV);
            cantNodos = Math.Max(0, Math.Min(_taxiMatrix.GetLength(0), _taxiMatrix.GetLength(1)) - 1);

            if (cantNodos <= 0)
            {
                log.Error("[TAXIBOT ERROR] taxibot.csv did not contain a valid adjacency matrix. Using the emergency direct-route graph.");
                _taxiMatrix = BuildFallbackMatrix(16);
                cantNodos = Math.Max(0, Math.Min(_taxiMatrix.GetLength(0), _taxiMatrix.GetLength(1)) - 1);
                _usingFallbackMatrix = true;
            }

            miGrafo = new CGrafo(cantNodos);
        }

        private string ResolveTaxiCsvPath(string filename)
        {
            string requested = string.IsNullOrWhiteSpace(filename) ? "extra/taxibot.csv" : filename.Trim();

            if (Path.IsPathRooted(requested) && File.Exists(requested))
                return Path.GetFullPath(requested);

            List<string> candidates = new List<string>();
            string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            candidates.Add(Path.Combine(baseDirectory, requested));
            candidates.Add(Path.Combine(baseDirectory, "extra", "taxibot.csv"));

            DirectoryInfo cursor = new DirectoryInfo(baseDirectory);
            for (int i = 0; i < 6 && cursor != null; i++)
            {
                candidates.Add(Path.Combine(cursor.FullName, requested));
                candidates.Add(Path.Combine(cursor.FullName, "extra", "taxibot.csv"));
                cursor = cursor.Parent;
            }

            foreach (string candidate in candidates.Distinct(StringComparer.OrdinalIgnoreCase))
            {
                if (File.Exists(candidate))
                    return Path.GetFullPath(candidate);
            }

            return Path.GetFullPath(Path.Combine(baseDirectory, requested));
        }

        private string[,] LoadCsv(string filename)
        {
            string resolvedPath = ResolveTaxiCsvPath(filename);
            _taxiCsvPath = resolvedPath;

            if (!File.Exists(resolvedPath))
            {
                _usingFallbackMatrix = true;
                log.Error("[TAXIBOT ERROR] taxibot.csv not found. Expected path: " + resolvedPath + ". Required by: Plus.Utilities.Dijkstra. The emulator will continue with a direct-route fallback graph until the real CSV is restored.");
                return BuildFallbackMatrix(16);
            }

            try
            {
                string wholeFile = File.ReadAllText(resolvedPath, Encoding.UTF8);
                wholeFile = wholeFile.Replace("\r\n", "\n").Replace('\r', '\n');
                string[] lines = wholeFile.Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);

                if (lines.Length < 2)
                    throw new InvalidDataException("The file must contain a header row and at least one node row.");

                int numRows = lines.Length;
                int numCols = lines.Max(line => line.Split(',').Length);

                if (numCols < 2)
                    throw new InvalidDataException("The file must contain at least two columns.");

                string[,] values = new string[numRows, numCols];

                for (int r = 0; r < numRows; r++)
                {
                    string[] lineValues = lines[r].Split(',');
                    for (int c = 0; c < numCols; c++)
                    {
                        values[r, c] = c < lineValues.Length ? lineValues[c].Trim() : "0";
                    }
                }

                _usingFallbackMatrix = false;
                return values;
            }
            catch (Exception ex)
            {
                _usingFallbackMatrix = true;
                log.Error("[TAXIBOT ERROR] taxibot.csv could not be read correctly. File: " + resolvedPath + ". Required by: Plus.Utilities.Dijkstra. Reason: " + ex.Message + ". The emulator will continue with a direct-route fallback graph.");
                return BuildFallbackMatrix(16);
            }
        }

        private string[,] BuildFallbackMatrix(int nodeCount)
        {
            int safeNodes = Math.Max(2, nodeCount);
            string[,] values = new string[safeNodes + 1, safeNodes + 1];
            values[0, 0] = "node";

            for (int i = 1; i <= safeNodes; i++)
            {
                values[0, i] = (i - 1).ToString();
                values[i, 0] = (i - 1).ToString();

                for (int j = 1; j <= safeNodes; j++)
                {
                    values[i, j] = i == j ? "0" : "1";
                }
            }

            return values;
        }

        public void Init()
        {
            if (_taxiMatrix == null)
                _taxiMatrix = LoadCsv(RoleplayManager.TaxiBotCSV);

            int matrixNodes = Math.Max(0, Math.Min(_taxiMatrix.GetLength(0), _taxiMatrix.GetLength(1)) - 1);
            if (matrixNodes <= 0 || miGrafo == null)
            {
                log.Error("[TAXIBOT ERROR] Dijkstra graph could not be initialized because the taxi matrix is empty.");
                return;
            }

            for (int i = 1; i <= matrixNodes; i++)
            {
                for (int j = 1; j <= matrixNodes; j++)
                {
                    int weight;
                    string rawValue = _taxiMatrix[i, j];

                    if (!int.TryParse(rawValue, out weight))
                    {
                        if (!string.IsNullOrWhiteSpace(rawValue))
                            log.Warn("[TAXIBOT WARN] Invalid weight in taxibot.csv at row " + i + ", column " + j + ": '" + rawValue + "'. Using 0.");

                        weight = 0;
                    }

                    miGrafo.AdicionaArista(i - 1, j - 1, weight);
                }
            }

            string source = _usingFallbackMatrix ? "fallback direct-route matrix" : _taxiCsvPath;
            log.Info("Loaded Dijkstra Algorithm (" + matrixNodes + " taxi nodes, source: " + source + ")");
        }

        public List<int> RunDijkstra(int origen, int destino)
        {
            inicio = origen;
            final = destino;

            if (cantNodos <= 0 || miGrafo == null)
            {
                LogFallbackRoute(origen, destino, "graph not initialized");
                return BuildDirectRoute(origen, destino);
            }

            if (origen < 0 || destino < 0 || origen >= cantNodos || destino >= cantNodos)
            {
                LogFallbackRoute(origen, destino, "route outside taxibot.csv bounds. Matrix nodes: " + cantNodos);
                return BuildDirectRoute(origen, destino);
            }

            if (origen == destino)
                return new List<int> { origen };

            int[,] tabla = new int[cantNodos, 3];

            for (n = 0; n < cantNodos; n++)
            {
                tabla[n, 0] = 0;
                tabla[n, 1] = int.MaxValue;
                tabla[n, 2] = -1;
            }

            tabla[inicio, 1] = 0;
            actual = inicio;

            do
            {
                tabla[actual, 0] = 1;

                for (columna = 0; columna < cantNodos; columna++)
                {
                    int edgeWeight = miGrafo.ObtenAdyacencia(actual, columna);
                    if (edgeWeight == 0 || tabla[actual, 1] == int.MaxValue)
                        continue;

                    distancia = edgeWeight + tabla[actual, 1];

                    if (distancia < tabla[columna, 1])
                    {
                        tabla[columna, 1] = distancia;
                        tabla[columna, 2] = actual;
                    }
                }

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

            if (tabla[final, 1] == int.MaxValue)
            {
                LogFallbackRoute(origen, destino, "no route found in taxibot.csv");
                return BuildDirectRoute(origen, destino);
            }

            List<int> ruta = new List<int>();
            int nodo = final;
            int guard = 0;

            while (nodo != inicio)
            {
                if (nodo < 0 || nodo >= cantNodos || guard++ > cantNodos)
                {
                    LogFallbackRoute(origen, destino, "route reconstruction failed");
                    return BuildDirectRoute(origen, destino);
                }

                ruta.Add(nodo);
                nodo = tabla[nodo, 2];
            }

            ruta.Add(inicio);
            ruta.Reverse();
            return ruta;
        }

        private List<int> BuildDirectRoute(int origen, int destino)
        {
            List<int> route = new List<int>();
            if (origen >= 0)
                route.Add(origen);
            if (destino >= 0 && destino != origen)
                route.Add(destino);
            return route;
        }

        private void LogFallbackRoute(int origen, int destino, string reason)
        {
            if (_reportedFallback)
                return;

            _reportedFallback = true;
            log.Warn("[TAXIBOT WARN] Using direct route fallback for taxi path " + origen + " -> " + destino + ". Reason: " + reason + ".");
        }

        public static void MostrarTabla(int[,] pTabla)
        {
            for (int n = 0; n < pTabla.GetLength(0); n++)
            {
                Console.WriteLine("{0}-> {1}\t{2}\t{3}", n, pTabla[n, 0], pTabla[n, 1], pTabla[n, 2]);
            }

            Console.WriteLine("------");
        }
    }
}