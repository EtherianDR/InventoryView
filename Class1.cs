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
        // Genie host.
        public static IHost _host;

        // Plugin Form
        private Form _form;

        // This contains all the of the inventory data.
        public static List<CharacterData> characterData = new List<CharacterData>();

        // Path to Genie config.
        private static string basePath = Application.StartupPath;

        // Whether or not InventoryView is currently scanning data, and what state it is in.
        private string ScanMode = null;

        // Keeps track of how many containers deep you are when scanning inventory in containers.
        private int level = 1;

        // The current character & source being scanned.
        private CharacterData currentData = null;

        // The last item tha was scanned.
        private ItemData lastItem = null;

        private bool Debug = false;
        private string LastText = "";

        public void Initialize(IHost host)
        {
            _host = host;

            basePath = _host.get_Variable("PluginPath");

            // Load inventory from the XML config if available.
            LoadSettings(initial: true);
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
            if (ScanMode != null) // If a scan isn't in progress, do nothing here.
            {
                string trimtext = text.Trim(new char[] { '\n', '\r', ' ' }); // Trims spaces and newlines.
                LastText = trimtext;
                if (trimtext.StartsWith("XML") && trimtext.EndsWith("XML")) { } // Skip XML parser lines
                else if (string.IsNullOrEmpty(trimtext)) { } // Skip blank lines
                else if (ScanMode == "Start") // When a scan is initiated, it starts here.
                {
                    if (trimtext == "You have:") // Text that appears at the beginning of "inventory list"
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
                    if (text.StartsWith("[Use INVENTORY HELP"))
                    {
                        // Skip
                    }
                    else if (text.StartsWith("Roundtime:")) // text that appears at the end of "inventory list"
                    {
                        // Inventory List has a RT based on the number of items, so grab the number and pause the thread for that length.
                        Match match = Regex.Match(trimtext, @"^Roundtime:\s{1,3}(\d{1,3})\s{1,3}secs?\.$");
                        ScanMode = "VaultStart";
                        _host.EchoText(string.Format("Pausing {0} seconds for RT.", int.Parse(match.Groups[1].Value)));
                        System.Threading.Thread.Sleep(int.Parse(match.Groups[1].Value) * 1000);
                        _host.SendText("get my vault book");
                    }
                    else
                    {
                        // The first level of inventory has a padding of 2 spaces to the left, and each level adds an additional 3 spaces.
                        // 2, 5, 8, 11, 14, etc..
                        int spaces = text.Length - text.TrimStart().Length;
                        int newlevel = (spaces + 1) / 3;
                        string tap = trimtext;
                        // remove the - from the beginning if it exists.
                        if (tap.StartsWith("-")) tap = tap.Remove(0, 1);

                        // The logic below builds a tree of inventory items.
                        if (newlevel == 1) // If the item is in the first level, add to the root item list
                        {
                            lastItem = currentData.AddItem(new ItemData() { tap = tap });
                        }
                        else if (newlevel == level) // If this is the same level as the previous item, add to the previous item's parent's item list.
                        {
                            lastItem = lastItem.parent.AddItem(new ItemData() { tap = tap });
                        }
                        else if (newlevel == level + 1) // If this item is down a level from the previous, add it to the previous item's item list.
                        {
                            lastItem = lastItem.AddItem(new ItemData() { tap = tap });
                        }
                        else // Else, if the item is up a level, loop back until you reach the correct level.
                        {
                            for (int i = newlevel; i <= level; i++)
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
                    // Get the vault book & read it.
                    Match match = Regex.Match(trimtext, @"^You get a.*vault book.*from");
                    if (match.Success || trimtext == "You are already holding that.")
                    {
                        _host.EchoText("Scanning Vault.");
                        _host.SendText("read my vault book");
                    }
                    else if (trimtext == "Vault Inventory:") // This text appears at the beginning of the vault list.
                    {
                        ScanMode = "Vault";
                        currentData = new CharacterData() { name = _host.get_Variable("charactername"), source = "Vault" };
                        characterData.Add(currentData);
                        level = 1;
                    }
                    // If you don't have a vault book or you can't read a vault book, it skips to checking your deed register.
                    else if (trimtext == "What were you referring to?" || trimtext == "The script that the vault book is written in is unfamiliar to you.  You are unable to read it." || trimtext == "The vault book is filled with blank pages pre-printed with branch office letterhead.  An advertisement touting the services of Rundmolen Bros. Storage Co. is pasted on the inside cover.")
                    {
                        _host.EchoText("Skipping Vault.");
                        ScanMode = "DeedStart";
                        _host.SendText("get my deed register");
                    }
                } //end if VaultStart
                else if (ScanMode == "Vault")
                {
                    // This text indicates the end of the vault inventory list.
                    if (text.StartsWith("The last note in your book indicates that your vault contains"))
                    {
                        ScanMode = "DeedStart";
                        _host.SendText("stow my vault book");
                        _host.SendText("get my deed register");
                    }
                    else
                    {
                        // Determine how many levels down an item is based on the number of spaces before it.
                        // Anything greater than 4 levels down shows up at the same level as its parent.
                        int spaces = text.Length - text.TrimStart().Length;
                        int newlevel = 1;
                        if (spaces > 4)
                            newlevel += (spaces - 4) / 2;
                        //switch (spaces)
                        //{
                        //    case 4:
                        //        newlevel = 1;
                        //        break;
                        //    case 6:
                        //        newlevel = 2;
                        //        break;
                        //    case 8:
                        //        newlevel = 3;
                        //        break;
                        //    case 10:
                        //        newlevel = 4;
                        //        break;
                        //}
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
                            for (int i = newlevel; i <= level; i++)
                            {
                                lastItem = lastItem.parent;
                            }
                            lastItem = lastItem.AddItem(new ItemData() { tap = tap });
                        }
                        level = newlevel;
                    }
                } //end if Vault
                else if (ScanMode == "DeedStart")
                {
                    // Get the vault book & read it.
                    Match match = Regex.Match(trimtext, @"^You get a.*deed register.*from");
                    if (match.Success || trimtext == "You are already holding that.")
                    {
                        _host.EchoText("Scanning Deed Register.");
                        _host.SendText("turn my deed register to contents");
                        _host.SendText("read my deed register");
                    }
                    else if (trimtext == "Page -- Deed") // This text appears at the beginning of the deed register list.
                    {
                        ScanMode = "Deed";
                        currentData = new CharacterData() { name = _host.get_Variable("charactername"), source = "Deed" };
                        characterData.Add(currentData);
                        level = 1;
                    }
                    // If you don't have a deed register or it is empty, it skips to checking your house.
                    else if (trimtext == "What were you referring to?" || trimtext.StartsWith("You haven't stored any deeds in this register."))
                    {
                        _host.EchoText("Skipping Deed Register.");
                        ScanMode = "HomeStart";
                        _host.SendText("home recall");
                    }
                } //end if DeedStart
                else if (ScanMode == "Deed")
                {
                    if (trimtext.StartsWith("Currently stored"))
                    {
                        _host.SendText("stow my deed register");
                        ScanMode = "HomeStart";
                        _host.SendText("home recall");
                    }
                    else
                    {
                        string tap = trimtext.Substring(trimtext.IndexOf("--") + 3);
                        lastItem = currentData.AddItem(new ItemData() { tap = tap, storage = false });
                    }
                } //end if Deed
                else if (ScanMode == "HomeStart")
                {
                    if (trimtext == "The home contains:") // This text appears at the beginning of the home list.
                    {
                        _host.EchoText("Scanning Home.");
                        ScanMode = "Home";
                        currentData = new CharacterData() { name = _host.get_Variable("charactername"), source = "Home" };
                        characterData.Add(currentData);
                        level = 1;
                    }
                    // This text appears if you don't have a home, skips and saves the results.
                    else if (trimtext.StartsWith("Your documentation filed with the Estate Holders"))
                    {
                        _host.EchoText("Skipping Home.");
                        if (_host.get_Variable("guild") == "Trader")
                        {
                            ScanMode = "TraderStart";
                            _host.SendText("get my storage book");
                        }
                        else
                        {
                            ScanMode = null;
                            _host.EchoText("Scan Complete.");
                            _host.SendText("#parse InventoryView scan complete");
                            SaveSettings();
                        }
                    }
                    else if (trimtext == "You shouldn't do that while inside of a home.  Step outside if you need to check something.")
                    {
                        _host.EchoText("You cannot check the contents of your home while inside of a home. Step outside and try again.");
                        if (_host.get_Variable("guild") == "Trader")
                        {
                            ScanMode = "TraderStart";
                            _host.SendText("get my storage book");
                        }
                        else
                        {
                            ScanMode = null;
                            _host.EchoText("Scan Complete.");
                            _host.SendText("#parse InventoryView scan complete");
                            SaveSettings();
                        }
                    }
                } //end if HomeStart
                else if (ScanMode == "Home")
                {
                    if (trimtext == ">") // There is no text after the home list, so watch for the next >
                    {
                        if (_host.get_Variable("guild") == "Trader")
                        {
                            ScanMode = "TraderStart";
                            _host.SendText("get my storage book");
                        }
                        else
                        {
                            ScanMode = null;
                            _host.EchoText("Scan Complete.");
                            _host.SendText("#parse InventoryView scan complete");
                            SaveSettings();
                        }
                    }
                    else if (trimtext.StartsWith("Attached:")) // If the item is attached, it is in/on/under/behind a piece of furniture.
                    {
                        string tap = trimtext.Replace("Attached: ", "");
                        lastItem = (lastItem.parent != null ? lastItem.parent : lastItem ).AddItem(new ItemData() { tap = tap });
                    }
                    else // Otherwise, it is a piece of furniture.
                    {
                        string tap = trimtext.Substring(trimtext.IndexOf(":")+2);
                        lastItem = currentData.AddItem(new ItemData() { tap = tap, storage = true });
                    }
                } //end if Home
                else if (ScanMode == "TraderStart")
                {
                    // Get the storage book & read it.
                    Match match = Regex.Match(trimtext, @"^You get a.*storage book.*from");
                    if (match.Success || trimtext == "You are already holding that.")
                    {
                        _host.EchoText("Scanning Trader Storage.");
                        _host.SendText("read my storage book");
                    }
                    else if (trimtext == "in the known realms since 402.") // This text appears at the beginning of the storage book list.
                    {
                        ScanMode = "Trader";
                        currentData = new CharacterData() { name = _host.get_Variable("charactername"), source = "TraderStorage" };
                        characterData.Add(currentData);
                        level = 1;
                    }
                    // If you don't have a vault book or you can't read a vault book, it skips to checking your house.
                    else if (trimtext == "What were you referring to?" || trimtext == "The storage book is filled with complex lists of inventory that make little sense to you.")
                    {
                        ScanMode = null;
                        _host.EchoText("Skipping Trader Storage.");
                        _host.EchoText("Scan Complete.");
                        _host.SendText("#parse InventoryView scan complete");
                        SaveSettings();
                    }
                } // end if trader start
                else if (ScanMode == "Trader")
                {
                    // This text indicates the end of the vault inventory list.
                    if (text.StartsWith("A notation at the bottom indicates"))
                    {
                        ScanMode = null;
                        _host.EchoText("Scan Complete.");
                        _host.SendText("#parse InventoryView scan complete");
                        SaveSettings();
                    }
                    else
                    {
                        // Determine how many levels down an item is based on the number of spaces before it.
                        // Anything greater than 4 levels down shows up at the same level as its parent.
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
                           default:
                                newlevel = 3;
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
                            for (int i = newlevel; i <= level; i++)
                            {
                                lastItem = lastItem.parent;
                            }
                            lastItem = lastItem.AddItem(new ItemData() { tap = tap });
                        }
                        level = newlevel;
                    }
                } //end if Trader
            }

            return text;
        }

        public string ParseInput(string text)
        {
            if (text.ToLower().StartsWith("/inventoryview ") || text.ToLower().StartsWith("/iv "))
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
                else if (SplitText[1].ToLower() == "debug")
                {
                    Debug = !Debug;
                    _host.EchoText("InventoryView Debug Mode " + (Debug ? "ON" : "OFF"));
                }
                else if (SplitText[1].ToLower() == "lasttext")
                {
                    Debug = !Debug;
                    _host.EchoText("InventoryView Debug Last Text: " + LastText);
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
            get { return "1.8"; }
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

        public static void LoadSettings(bool initial = false)
        {
            string configFile = Path.Combine(basePath, "InventoryView.xml");
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
                    if (!initial)
                        _host.EchoText("InventoryView data loaded.");
                }
                catch (IOException ex)
                {
                    _host.EchoText("Error reading InventoryView file: " + ex.Message);
                }
            }
        }

        public static void SaveSettings()
        {
            string configFile = Path.Combine(basePath, "InventoryView.xml");
            try
            {
                // Can't serialize a class with circular references, so I have to remove the parent links first.
                foreach (var cData in characterData)
                {
                    RemoveParents(cData.items);
                }
                FileStream writer = new FileStream(configFile, FileMode.Create);
                XmlSerializer serializer = new XmlSerializer(typeof(List<CharacterData>));
                serializer.Serialize(writer, characterData);
                writer.Close();
                
                // ..and add them back again afterwards.
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