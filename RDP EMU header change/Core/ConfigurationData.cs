using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using Plus.Database.Interfaces;


namespace Plus.Core
{
    public class ConfigurationData
    {
        public Dictionary<string, string> data;
        public string FilePath { get; private set; }

        public ConfigurationData(string filePath, bool maynotexist = false)
        {
            data = new Dictionary<string, string>();
            FilePath = filePath;

            if (!File.Exists(filePath))
            {
                if (!maynotexist)
                    throw new ArgumentException("[CONFIG ERROR] Unable to locate configuration file.\nFile: " + filePath);
                else
                    return;
            }

            try
            {
                using (var stream = new StreamReader(filePath))
                {
                    string line = "";
                    int lineNumber = 0;

                    while ((line = stream.ReadLine()) != null)
                    {
                        lineNumber++;
                        string cleanLine = line.Trim();

                        if (cleanLine.Length < 1 || cleanLine.StartsWith("#"))
                        {
                            continue;
                        }

                        int delimiterIndex = cleanLine.IndexOf('=');

                        if (delimiterIndex != -1)
                        {
                            string key = cleanLine.Substring(0, delimiterIndex).Trim();
                            string val = cleanLine.Substring(delimiterIndex + 1).Trim();

                            if (String.IsNullOrWhiteSpace(key))
                                continue;

                            if (data.ContainsKey(key))
                            {
                                Console.WriteLine("[CONFIG WARNING] Duplicate key '" + key + "' in " + filePath + " at line " + lineNumber + ". Last value wins.");
                                data[key] = val;
                            }
                            else
                            {
                                data.Add(key, val);
                            }
                        }
                    }
                }

                ValidateCoreBootKeys();
            }

            catch (Exception e)
            {
                throw new ArgumentException("[CONFIG ERROR] Could not process configuration file.\nFile: " + filePath + "\nReason: " + e.Message);
            }
        }

        private void ValidateCoreBootKeys()
        {
            if (!Path.GetFileName(FilePath).Equals("config.ini", StringComparison.OrdinalIgnoreCase))
                return;

            string[] requiredKeys = new string[]
            {
                "db.hostname",
                "db.port",
                "db.username",
                "db.password",
                "db.name",
                "db.pool.minsize",
                "db.pool.maxsize",
                "mus.tcp.bindip",
                "mus.tcp.port",
                "mus.tcp.allowedaddr",
                "game.tcp.port",
                "game.tcp.conlimit",
                "game.tcp.conperip",
                "game.tcp.enablenagles"
            };

            List<string> missing = new List<string>();
            foreach (string key in requiredKeys)
            {
                if (!data.ContainsKey(key))
                    missing.Add(key);
            }

            if (missing.Count > 0)
            {
                throw new InvalidOperationException("[CONFIG ERROR] Missing required key(s): " + String.Join(", ", missing.ToArray()) + "\nFile: " + FilePath + "\nRequired by: Emulator boot");
            }

            Console.WriteLine("[CONFIG] Checking configuration...");
            Console.WriteLine("[CONFIG] Database configuration OK");
            Console.WriteLine("[CONFIG] MUS configuration OK");
            Console.WriteLine("[CONFIG] Game socket configuration OK");
        }

        public bool TryGetValue(string key, out string value)
        {
            value = null;

            if (String.IsNullOrWhiteSpace(key))
                return false;

            return data.TryGetValue(key, out value);
        }

        public string GetOptionalValue(string key, string defaultValue)
        {
            string value;

            if (!TryGetValue(key, out value))
                return defaultValue;

            return value;
        }

        public string GetRequiredValue(string key, string requiredBy)
        {
            string value;

            if (!TryGetValue(key, out value))
            {
                throw new InvalidOperationException("[CONFIG ERROR] Missing required key: " + key + "\nFile: " + FilePath + "\nRequired by: " + requiredBy);
            }

            return value;
        }
    }

    public class TargetedOffers
    {
        public Dictionary<string, string> DBOffer;

        public TargetedOffers()
        {
            DBOffer = new Dictionary<string, string>();
            DBOffer.Clear();

            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("SELECT * FROM `targeted_offers`");
                DataTable ConfigData = dbClient.getTable();

                if (ConfigData != null)
                {
                    foreach (DataRow Data in ConfigData.Rows)
                    {
                        DBOffer.Add(Data[0].ToString(), Data[1].ToString());
                        
                    }
                }
            }
        }
    }

    public class ConfigData
    {
        public Dictionary<string, string> DBData;

        public ConfigData()
        {
            DBData = new Dictionary<string, string>();
            DBData.Clear();

            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("SELECT * FROM `server_settings`");
                DataTable ConfigData = dbClient.getTable();

                if (ConfigData != null)
                {
                    foreach (DataRow Data in ConfigData.Rows)
                    {
                        DBData.Add(Data[0].ToString(), Data[1].ToString());
                    }
                }
            }
            return;
        }
    }

    
}