using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Permissions;
using System.Xml.Serialization;
using LeagueSharp;

namespace AutoLeveler
{
    [PermissionSet(SecurityAction.Assert, Unrestricted = true)]
    internal class Save
    {
        public Champion Champion;
        public string Data;
        public string FilePath;
        public Items List;
        public int[] Sequence;

        public Save(string path, string data, bool updateSave = false)
        {
            FilePath = Path.Combine(CurrentDirectory, path);
            Data = data;

            if (!File.Exists(FilePath) || updateSave) // || IsNewerVersion())
            {
                Directory.CreateDirectory(CurrentDirectory + "AutoLeveler");
                File.WriteAllText(FilePath, Data);
            }
            try
            {
                var serializer = new XmlSerializer(typeof(Items));
                List = (Items) serializer.Deserialize(new FileStream(FilePath, FileMode.Open));

                var champs = List.Champion;
                Champion = champs.FirstOrDefault(o => o.Name.Equals(ObjectManager.Player.ChampionName));

                //new champ
                if (Champion == null)
                {
                    Array.Clear(Sequence, 0, 18);
                    List.Champion[List.Champion.Length + 1] = new Champion
                    {
                        Name = ObjectManager.Player.ChampionName,
                        Sequence =
                            String.Join("", new List<int>(Sequence).ConvertAll(i => i.ToString() + ", ").ToArray())
                                .TrimEnd(',', ' ')
                    };
                    Champion = List.Champion[List.Champion.Length];
                }
                else
                {
                    Sequence = Champion.Sequence.Trim().Split(',').Select(n => Convert.ToInt32(n)).ToArray();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            AppDomain.CurrentDomain.DomainUnload += CurrentDomain_DomainUnload;
            AppDomain.CurrentDomain.ProcessExit += CurrentDomain_ProcessExit;
        }

        public string CurrentDirectory
        {
            get { return AppDomain.CurrentDomain.BaseDirectory; }
        }

        private void CurrentDomain_ProcessExit(object sender, EventArgs e)
        {
            SaveData();
        }

        private void CurrentDomain_DomainUnload(object sender, EventArgs e)
        {
            SaveData();
        }

        public void SaveData()
        {
            try
            {
                var serializer = new XmlSerializer(typeof(Items));
                serializer.Serialize(new FileStream(FilePath, FileMode.Create), List);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        public void UpdateSequence(int[] sequence)
        {
            Sequence = sequence;
            Champion.Sequence =
                string.Join("", new List<int>(Sequence).ConvertAll(i => i.ToString() + ", ").ToArray())
                    .TrimEnd(',', ' ');
        }
    }

    [XmlType(AnonymousType = true)]
    [XmlRoot(Namespace = "", IsNullable = false, ElementName = "levels")]
    public class Items
    {
        [XmlElement("Champion")]
        public Champion[] Champion { get; set; }
    }


    [XmlType(AnonymousType = true, TypeName = "levelsChampion")]
    public class Champion
    {
        public string Name { get; set; }
        public string Sequence { get; set; }
    }
}