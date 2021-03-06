//
// This file is part of the game Voxalia, created by Frenetic LLC.
// This code is Copyright (C) 2016-2017 Frenetic LLC under the terms of a strict license.
// See README.md or LICENSE.txt in the source root for the contents of the license.
// If neither of these are available, assume that neither you nor anyone other than the copyright holder
// hold any right or permission to use this software until such time as the official license is identified.
//

using System.Collections.Generic;
using Voxalia.ServerGame.WorldSystem;
using System.Drawing;
using FreneticScript.TagHandlers;
using Voxalia.Shared;

namespace Voxalia.ServerGame.ItemSystem
{
    public class Inventory
    {
        public WorldSystem.Region TheWorld;

        public Inventory(WorldSystem.Region tregion)
        {
            TheWorld = tregion;
        }

        public List<ItemStack> Items = new List<ItemStack>();

        /// <summary>
        /// Returns an item in the quick bar.
        /// Can return air.
        /// </summary>
        /// <param name="slot">The slot, any number is permitted.</param>
        /// <returns>A valid item.</returns>
        public ItemStack GetItemForSlot(int slot)
        {
            slot = slot % (Items.Count + 1);
            while (slot < 0)
            {
                slot += Items.Count + 1;
            }
            if (slot == 0)
            {
                return new ItemStack("Air", TheWorld.TheServer, 1, "clear", "Air", "An empty slot.", Color.White, "blank.dae", true, 0);
            }
            else
            {
                return Items[slot - 1];
            }
        }

        public int GetSlotForItem(ItemStack item)
        {
            for (int i = 0; i < Items.Count; i++)
            {
                if (Items[i] == item)
                {
                    return i + 1;
                }
            }
            return -1;
        }

        public static bool ItemsMatch(ItemStack item, ItemStack item2)
        {
            return item2.Datum == item.Datum &&
                    item2.Name == item.Name &&
                    item2.DisplayName == item.DisplayName &&
                    item2.Description == item.Description &&
                    item2.DrawColor == item.DrawColor &&
                    item2.Image == item.Image &&
                    item2.Volume == item.Volume &&
                    item2.Weight == item.Weight &&
                    item2.Model == item.Model &&
                    ItemAttrsMatch(item2, item) &&
                    ItemSharedAttrsMatch(item2, item) &&
                    item2.IsBound == item.IsBound;
            // NOTE: Intentionally don't check the count or temperature here.
        }

        public static bool ItemAttrsMatch(ItemStack i1, ItemStack i2)
        {
            if (i1.Attributes.Count != i2.Attributes.Count)
            {
                return false;
            }
            foreach (string str in i1.Attributes.Keys)
            {
                if (!i2.Attributes.ContainsKey(str))
                {
                    return false;
                }
                if (i1.Attributes[str].ToString() != i2.Attributes[str].ToString()) // TODO: Proper tag equality checks?
                {
                    return false;
                }
            }
            return true;
        }


        public static bool ItemSharedAttrsMatch(ItemStack i1, ItemStack i2)
        {
            if (i1.SharedAttributes.Count != i2.SharedAttributes.Count)
            {
                return false;
            }
            foreach (string str in i1.SharedAttributes.Keys)
            {
                if (!i2.SharedAttributes.ContainsKey(str))
                {
                    return false;
                }
                if (i1.SharedAttributes[str].ToString() != i2.SharedAttributes[str].ToString())
                {
                    return false;
                }
            }
            return true;
        }

        public void GiveItem(ItemStack item)
        {
            if (item.Name == "air")
            {
                return;
            }
            GiveItemNoDup(item.Duplicate());
        }

        protected virtual ItemStack GiveItemNoDup(ItemStack item)
        {
            for (int i = 0; i < Items.Count; i++)
            {
                if (ItemsMatch(item, Items[i]))
                {
                    Items[i].Temperature = (item.Temperature * item.Count) + (Items[i].Temperature * item.Count) / (item.Count + Items[i].Count);
                    Items[i].Count += item.Count;
                    return Items[i];
                }
            }
            Items.Add(item);
            return item;
        }

        public virtual void SetSlot(int slot, ItemStack item)
        {
            Items[slot] = item;
        }

        public bool RemoveItem(ItemStack item, int count)
        {
            for (int i = 0; i < Items.Count; i++)
            {
                if (ItemsMatch(item, Items[i]))
                {
                    if (item.Count > count)
                    {
                        item.Count -= count;
                        SetSlot(i, item);
                        return true;
                    }
                    else
                    {
                        count -= item.Count;
                        RemoveItem(i + 1);
                        if (count == 0)
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        public virtual void RemoveItem(int item)
        {
            item = item % (Items.Count + 1);
            if (item < 0)
            {
                item += Items.Count + 1;
            }
            Items.RemoveAt(item - 1);
        }

    }
}
