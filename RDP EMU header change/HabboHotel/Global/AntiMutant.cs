using System;
using System.Linq;
using System.Xml.Linq;
using System.Collections.Generic;
using System.IO;

namespace Plus.HabboHotel.Global
{
    public class AntiMutant
    {
        private Dictionary<string, Dictionary<string, Figure>> _parts;
        private bool _loadedCompatibilitySeed;

        public AntiMutant()
        {
            _parts = new Dictionary<string, Dictionary<string, Figure>>();
            _loadedCompatibilitySeed = false;
            Init();
        }

        public void Init()
        {
            if (this._parts.Count > 0)
                this._parts.Clear();

            string figureDataPath = ResolveFigureDataPath();

            try
            {
                Console.WriteLine("[BOOT] Loading AntiMutant figuredata: " + figureDataPath);

                XDocument Doc = XDocument.Load(figureDataPath);
                _loadedCompatibilitySeed = IsCompatibilitySeed(Doc);

                var data = (from tItem in Doc.Descendants("settype") select new { Part = tItem.Elements("set"), Type = tItem.Attribute("type") });
                foreach (var item in data.ToList())
                {
                    if (item.Type == null || String.IsNullOrWhiteSpace(item.Type.Value))
                        continue;

                    foreach (var part in item.Part.ToList())
                    {
                        XAttribute id = part.Attribute("id");
                        XAttribute gender = part.Attribute("gender");
                        XAttribute colorable = part.Attribute("colorable");

                        if (id == null || String.IsNullOrWhiteSpace(id.Value))
                            continue;

                        string PartName = item.Type.Value;
                        if (!_parts.ContainsKey(PartName))
                            _parts.Add(PartName, new Dictionary<string, Figure>());

                        string partGender = gender != null && !String.IsNullOrWhiteSpace(gender.Value) ? gender.Value : "U";
                        string partColorable = colorable != null && !String.IsNullOrWhiteSpace(colorable.Value) ? colorable.Value : "0";

                        Figure toAddFigure = new Figure(PartName, id.Value, partGender, partColorable);

                        if (!_parts[PartName].ContainsKey(id.Value))
                            _parts[PartName].Add(id.Value, toAddFigure);
                    }
                }

                if (_parts.Count <= 0)
                {
                    throw new InvalidDataException("[ANTIMUTANT ERROR] figuredata.xml was loaded but no <settype> / <set> entries were found.\nFile: " + figureDataPath);
                }

                Console.WriteLine("[BOOT] figuredata.xml -> LOADED (" + _parts.Count + " set types" + (_loadedCompatibilitySeed ? ", compatibility seed" : "") + ")");
                Console.WriteLine("[BOOT] AntiMutant -> LOADED");
            }
            catch (Exception e)
            {
                throw new InvalidOperationException("[ANTIMUTANT ERROR] Unable to load figuredata.xml.\nExpected path: " + figureDataPath + "\nReason: " + e.Message, e);
            }
        }

        private string ResolveFigureDataPath()
        {
            List<string> candidates = new List<string>();

            string configuredPath;
            if (PlusEnvironment.GetConfig() != null && PlusEnvironment.GetConfig().TryGetValue("antimutant.figuredata.path", out configuredPath) && !String.IsNullOrWhiteSpace(configuredPath))
            {
                candidates.Add(Path.IsPathRooted(configuredPath) ? configuredPath : Path.Combine(AppDomain.CurrentDomain.BaseDirectory, configuredPath));
                candidates.Add(Path.IsPathRooted(configuredPath) ? configuredPath : Path.Combine(Directory.GetCurrentDirectory(), configuredPath));
            }

            candidates.Add(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "extra", "figuredata.xml"));
            candidates.Add(Path.Combine(Directory.GetCurrentDirectory(), "extra", "figuredata.xml"));
            candidates.Add(Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "extra", "figuredata.xml")));
            candidates.Add(Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "extra", "figuredata.xml")));

            foreach (string candidate in candidates.Distinct().ToList())
            {
                if (!String.IsNullOrWhiteSpace(candidate) && File.Exists(candidate))
                    return candidate;
            }

            throw new FileNotFoundException("[ANTIMUTANT ERROR] figuredata.xml not found.\nExpected locations:\n - " + String.Join("\n - ", candidates.Distinct().ToArray()));
        }

        private bool IsCompatibilitySeed(XDocument doc)
        {
            if (doc == null || doc.Root == null)
                return false;

            XAttribute seed = doc.Root.Attribute("compatibility_seed");
            return seed != null && seed.Value == "1";
        }

        public string RunLook(string Look)
        {
            if (String.IsNullOrWhiteSpace(Look))
                return "sh-3338-93.ea-1406-62.hr-831-49.ha-3331-92.hd-180-7.ch-3334-93-1408.lg-3337-92.ca-1813-62";

            List<string> toReturnFigureParts = new List<string>();
            List<string> fParts = new List<string>();
            string[] requiredParts = { "hd", "ch" };
            bool flagForDefault = false;

            string[] FigureParts = Look.Split('.');
            string genderLook = GetLookGender(Look);

            foreach (string Part in FigureParts.ToList())
            {
                string newPart = Part;
                string[] tPart = Part.Split('-');
                if (tPart.Count() < 2)
                {
                    flagForDefault = true;
                    continue;
                }

                string partName = tPart[0];
                string partId = tPart[1];
                bool missingPart = !_parts.ContainsKey(partName) || !_parts[partName].ContainsKey(partId);
                bool wrongGender = false;

                if (!missingPart)
                    wrongGender = genderLook != "U" && _parts[partName][partId].Gender != "U" && _parts[partName][partId].Gender != genderLook;

                if (missingPart || wrongGender)
                {
                    if (_loadedCompatibilitySeed && missingPart)
                    {
                        newPart = Part;
                    }
                    else if (partName == "hd" && partId == "99999") 
                    {
                        if (tPart.Count() == 2)
                        {
                            newPart = SetDefault(partName, genderLook);
                        }
                    }
                    else
                    {
                        newPart = SetDefault(partName, genderLook);
                    }
                }

                if (!fParts.Contains(partName)) fParts.Add(partName);
                if (!toReturnFigureParts.Contains(newPart)) toReturnFigureParts.Add(newPart);
            }

            if (flagForDefault)
            {
                toReturnFigureParts.Clear();
                toReturnFigureParts.AddRange("sh-3338-93.ea-1406-62.hr-831-49.ha-3331-92.hd-180-7.ch-3334-93-1408.lg-3337-92.ca-1813-62".Split('.'));
            }

            foreach (string requiredPart in requiredParts.Where(requiredPart => !fParts.Contains(requiredPart) && !toReturnFigureParts.Contains(SetDefault(requiredPart, genderLook))))
            {
                toReturnFigureParts.Add(SetDefault(requiredPart, genderLook));
            }

            return string.Join(".", toReturnFigureParts);
        }

        private string GetLookGender(string Look)
        {
            string[] FigureParts = Look.Split('.');

            foreach (string Part in FigureParts.ToList())
            {
                string[] tPart = Part.Split('-');
                if (tPart.Count() < 2)
                    continue;

                string partName = tPart[0];
                string partId = tPart[1];

                return this._parts.ContainsKey(partName) && this._parts[partName].ContainsKey(partId) ? this._parts[partName][partId].Gender : "U";
            }
            return "U";
        }

        private string SetDefault(string partName, string Gender)
        {
            string partId = "0";
            if (this._parts.ContainsKey(partName))
            {
                KeyValuePair<string, Figure> part = _parts[partName].FirstOrDefault(x => x.Value.Gender == Gender || x.Value.Gender == "U" || Gender == "U");
                partId = part.Equals(default(KeyValuePair<string, Figure>)) ? "0" : part.Key;
            }
            return partName + "-" + partId + "-1";
        }
    }

    class Figure
    {
        private string Part;
        private string PartId;
        public string Gender;
        private string Colorable;

        public Figure(string Part, string PartId, string Gender, string Colorable)
        {
            this.Part = Part;
            this.PartId = PartId;
            this.Gender = Gender;
            this.Colorable = Colorable;
        }
    }
}