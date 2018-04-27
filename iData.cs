using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InventoryView
{
    [Serializable]
    public class CharacterData
    {
        public string name;
        public string source;
        public List<ItemData> items = new List<ItemData>();

        public ItemData AddItem(ItemData newItem)
        {
            items.Add(newItem);
            return newItem;
        }

        public ItemData AddItem(string tap, bool storage = false)
        {
            ItemData newItem = new ItemData() { tap = tap, storage = storage };
            items.Add(newItem);
            return newItem;
        }
    }

    [Serializable]
    public class ItemData
    {
        public bool storage;
        public string tap;
        public ItemData parent;
        public List<ItemData> items = new List<ItemData>();

        public ItemData AddItem(ItemData newItem)
        {
            newItem.parent = this;
            items.Add(newItem);
            return newItem;
        }

        public ItemData AddItem(string tap, bool storage = false)
        {
            ItemData newItem = new ItemData() { tap = tap, storage = storage, parent = this };
            items.Add(newItem);
            return newItem;
        }
    }
}
