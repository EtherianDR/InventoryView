using System;
using System.Collections.Generic;
using GeniePlugin.Interfaces;
using System.Windows.Forms;
using System.Xml.Serialization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Text.RegularExpressions;

namespace InventoryView
{
    public class Class1 : GeniePlugin.Interfaces.IPlugin
    {

        public static IHost _host;

        private Form _form;

        public static List<CharacterData> characterData = new List<CharacterData>();

        private static string basePath = Application.StartupPath;

        private string ScanMode = null;
        private int level = 1;
        private CharacterData currentData = null;
        private ItemData lastItem = null;

        public void Initialize(IHost host)
        {
            _host = host;

            if (!Directory.Exists(Path.Combine(basePath, "Config")))
                basePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), Application.ProductName);

            LoadSettings();
        }

        public void Show()
        {
            if (_form == null || _form.IsDisposed)
                _form = new InventoryViewForm();

            _form.Show();
        }

        public void VariableChanged(string variable)
        {

        }

        public string ParseText(string text, string window)
        {
            if (ScanMode != null)
            {
                string trimtext = text.Trim(new char[] { '\n', '\r', ' ' });
                if (string.IsNullOrEmpty(trimtext)) { }
                else if (ScanMode == "Start")
                {
                    if (trimtext == "You have:")
                    {
                        _host.EchoText("Scanning Inventory.");
                        ScanMode = "Inventory";
                        currentData = new CharacterData() { name = _host.get_Variable("charactername"), source = "Inventory" };
                        characterData.Add(currentData);
                        level = 1;
                    }
                } //end if Start
                else if (ScanMode == "Inventory")
                {
                    if (text.StartsWith("Roundtime:"))
                    {
                        Match match = Regex.Match(trimtext, @"^Roundtime:\s{1,3}(\d{1,3})\s{1,3}secs?$");
                        ScanMode = "VaultStart";
                        _host.EchoText(string.Format("Pausing {0} seconds for RT.", int.Parse(match.Groups[1].Value)));
                        System.Threading.Thread.Sleep(int.Parse(match.Groups[1].Value) * 1000);
                        _host.SendText("get my vault book");
                    }
                    else
                    {
                        int spaces = text.Length - text.TrimStart().Length;
                        int newlevel = (spaces + 1) / 3;
                        string tap = trimtext;
                        if (tap.StartsWith("-")) tap = tap.Remove(0, 1);
                        if (newlevel == 1)
                        {
                            lastItem = currentData.AddItem(new ItemData() { tap = tap });
                        }
                        else if (newlevel == level)
                        {
                            lastItem = lastItem.parent.AddItem(new ItemData() { tap = tap });
                        }
                        else if (newlevel == level + 1)
                        {
                            lastItem = lastItem.AddItem(new ItemData() { tap = tap });
                        }
                        else
                        {
                            for (int i = newlevel; i < level; i++)
                            {
                                lastItem = lastItem.parent;
                            }
                            lastItem = lastItem.AddItem(new ItemData() { tap = tap });
                        }
                        level = newlevel;
                    }
                } //end if Inventory
                else if (ScanMode == "VaultStart")
                {
                    Match match = Regex.Match(trimtext, @"^You get a.*vault book.*from");
                    if (match.Success || trimtext == "You are already holding that.")
                    {
                        _host.EchoText("Scanning Vault.");
                        _host.SendText("read my vault book");
                    }
                    else if (trimtext == "Vault Inventory:")
                    {
                        ScanMode = "Vault";
                        currentData = new CharacterData() { name = _host.get_Variable("charactername"), source = "Vault" };
                        characterData.Add(currentData);
                        level = 1;
                    }
                    else if (trimtext == "What were you referring to?" || trimtext == "The script that the vault book is written in is unfamiliar to you.  You are unable to read it.")
                    {
                        _host.EchoText("Skipping Vault.");
                        ScanMode = "HomeStart";
                        _host.SendText("home recall");
                    }
                } //end if VaultStart
                else if (ScanMode == "Vault")
                {
                    if (text.StartsWith("The last note in your book indicates that your vault contains"))
                    {
                        ScanMode = "HomeStart";
                        _host.SendText("stow my vault book");
                        _host.SendText("home recall");
                    }
                    else
                    {
                        int spaces = text.Length - text.TrimStart().Length;
                        int newlevel = 1;
                        switch (spaces)
                        {
                            case 4:
                                newlevel = 1;
                                break;
                            case 8:
                                newlevel = 2;
                                break;
                            case 12:
                                newlevel = 3;
                                break;
                            case 15:
                                newlevel = 4;
                                break;
                        }
                        string tap = trimtext;
                        if (tap.StartsWith("-")) tap = tap.Remove(0, 1);
                        if (newlevel == 1)
                        {
                            lastItem = currentData.AddItem(new ItemData() { tap = tap, storage = true });
                        }
                        else if (newlevel == level)
                        {
                            lastItem = lastItem.parent.AddItem(new ItemData() { tap = tap });
                        }
                        else if (newlevel == level + 1)
                        {
                            lastItem = lastItem.AddItem(new ItemData() { tap = tap });
                        }
                        else
                        {
                            for (int i = newlevel; i < level; i++)
                            {
                                lastItem = lastItem.parent;
                            }
                            lastItem = lastItem.AddItem(new ItemData() { tap = tap });
                        }
                        level = newlevel;
                    }
                } //end if Vault
                else if (ScanMode == "HomeStart")
                {
                    if (trimtext == "The home contains:")
                    {
                        _host.EchoText("Scanning Home.");
                        ScanMode = "Home";
                        currentData = new CharacterData() { name = _host.get_Variable("charactername"), source = "Home" };
                        characterData.Add(currentData);
                        level = 1;
                    }
                    else if (trimtext.StartsWith("Your documentation filed with the Estate Holders"))
                    {
                        _host.EchoText("Skipping Home.");
                        _host.EchoText("Scan Complete.");
                        ScanMode = null;
                        SaveSettings();
                    }
                } //end if HomeStart
                else if (ScanMode == "Home")
                {
                    if (trimtext == ">")
                    {
                        _host.EchoText("Scan Complete.");
                        ScanMode = null;
                        SaveSettings();
                    }
                    else if (trimtext.StartsWith("Attached:"))
                    {
                        string tap = trimtext.Replace("Attached: ", "");
                        lastItem = (lastItem.parent != null ? lastItem.parent : lastItem ).AddItem(new ItemData() { tap = tap });
                    }
                    else
                    {
                        string tap = trimtext.Substring(trimtext.IndexOf(":")+2);
                        lastItem = currentData.AddItem(new ItemData() { tap = tap, storage = true });
                    }
                } //end if Home
            }

            return text;
        }

        public string ParseInput(string text)
        {
            if (text.ToLower().StartsWith("/inventoryview"))
            {
                var SplitText = text.Split(' ');
                if (SplitText.Length == 1 || SplitText[1].ToLower() == "help")
                {
                    Help();
                }
                else if (SplitText[1].ToLower() == "scan")
                {
                    if (_host.get_Variable("connected") == "0")
                    {
                        _host.EchoText("You must be connected to the server to do a scan.");
                    }
                    else
                    {
                        LoadSettings();
                        ScanMode = "Start";
                        while (characterData.Where(tbl => tbl.name == _host.get_Variable("charactername")).Count() > 0)
                        {
                            characterData.Remove(characterData.Where(tbl => tbl.name == _host.get_Variable("charactername")).First());
                        }
                        _host.SendText("inventory list");
                    }
                }
                else if (SplitText[1].ToLower() == "open")
                {
                    Show();
                }
                else
                    Help();

                return string.Empty;
            }
            return text;
        }

        public void Help()
        {
            _host.EchoText("Inventory View plugin options:");
            _host.EchoText("/InventoryView scan  -- scan the items on the current character.");
            _host.EchoText("/InventoryView open  -- open the InventoryView Window to see items.");
        }

        public static void RemoveParents(List<ItemData> iList)
        {
            foreach (var iData in iList)
            {
                iData.parent = null;
                RemoveParents(iData.items);
            }
        }

        public static void AddParents(List<ItemData> iList, ItemData parent)
        {
            foreach (var iData in iList)
            {
                iData.parent = parent;
                AddParents(iData.items, iData);
            }
        }

        public void ParseXML(string xml)
        {

        }

        public void ParentClosing()
        {

        }

        public string Name
        {
            get { return "Inventory View"; }
        }

        public string Version
        {
            get { return "1.3"; }
        }

        public string Description
        {
            get { return "Stores your character inventory and allows you to search items across characters."; }
        }

        public string Author
        {
            get { return "Etherian <EtherianDR@gmail.com>"; }
        }

        private bool _enabled = true;
        public bool Enabled
        {
            get { return _enabled; }
            set { _enabled = value; }
        }

        public static void LoadSettings()
        {
            string configFile = Path.Combine(basePath, "Plugins", "InventoryView.xml");
            if (File.Exists(configFile))
            {
                try
                {
                    using (Stream stream = File.Open(configFile, FileMode.Open))
                    {
                        XmlSerializer serializer = new XmlSerializer(typeof(List<CharacterData>));
                        characterData = (List<CharacterData>)serializer.Deserialize(stream);
                    }

                    foreach (var cData in characterData)
                    {
                        AddParents(cData.items, null);
                    }
                }
                catch (IOException ex)
                {
                    _host.EchoText("Error reading InventoryView file: " + ex.Message);
                }
            }
        }

        public static void SaveSettings()
        {
            string configFile = Path.Combine(basePath, "Plugins", "InventoryView.xml");
            try
            {
                
                foreach (var cData in characterData)
                {
                    RemoveParents(cData.items);
                }
                FileStream writer = new FileStream(configFile, FileMode.Create);
                XmlSerializer serializer = new XmlSerializer(typeof(List<CharacterData>));
                serializer.Serialize(writer, characterData);
                writer.Close();

                foreach (var cData in characterData)
                {
                    AddParents(cData.items, null);
                }
            }
            catch (IOException ex)
            {
                _host.EchoText("Error writing to InventoryView file: " + ex.Message);
            }
        }
    }
}