using Ennui.Api;
using Ennui.Api.Meta;
using System.Collections.Generic;

namespace Ennui.Script.Official
{
    public static class EquipmentUtils
    {
        public static bool HasBrokenItems(this IApi api)
        {
            var equipment = api.Equipment.AllItems;
            if (equipment != null)
            {
                foreach (var item in equipment)
                {
                    if (item.Integrity <= 10)
                    {
                        return true;
                    }
                }
            }

            var inventory = api.Inventory.AllItems;
            if (inventory != null)
            {
                foreach (var item in inventory)
                {
                    if (item.Integrity <= 10)
                    {
                        return true;
                    }
                }
            }

            return false;
        }
        public static bool WeaponOrArmourBroken(this IApi api)
        {
            var equipment = api.Equipment.AllItems;
            if (equipment != null)
                foreach (var item in equipment)
                    if (item.Integrity <= 10)
                        return true;
            return false;
        }

        /**
         * Gets all tools that aren't broken and returns their collection type mapped into ResourceType
         * List of ResourceType with a List of teir collection types available
         */
        public static List<KeyValuePair<ResourceType, List<int>>> GetCollectionTypesRemaining(this IApi api)
        {
            //Find all tools
            List<Api.Direct.IItemStack> axes = api.Inventory.GetItemsBySubstring(" axe");
            List<Api.Direct.IItemStack> picks = api.Inventory.GetItemsBySubstring("pickaxe");
            List<Api.Direct.IItemStack> knives = api.Inventory.GetItemsBySubstring("knife");
            List<Api.Direct.IItemStack> hammers = api.Inventory.GetItemsBySubstring("hammer");
            List<Api.Direct.IItemStack> sickles = api.Inventory.GetItemsBySubstring("sickle");
            List<List<Api.Direct.IItemStack>> allTools = new List<List<Api.Direct.IItemStack>> { axes, picks, knives, hammers, sickles };

            List<KeyValuePair<ResourceType, List<int>>> returnList = new List<KeyValuePair<ResourceType, List<int>>>();

            int i = 0;
            int j = 0;
            do
            {
                List<int> listToAdd = new List<int>();
                do
                {
                    if (allTools[i][j].Integrity > 10)
                    {
                        int toolTier = getToolTier(allTools[i][j]);
                        if(!listToAdd.Contains(toolTier))
                            listToAdd.Add(toolTier);
                    }
                } while (j++ < allTools[i].Count);
                if (allTools[i].Count == 0)
                    allTools.RemoveAt(i);
                if(listToAdd.Count > 0)
                    returnList.Add(new KeyValuePair<ResourceType, List<int>>(getResourceType(i), listToAdd) );
            } while (i++ < allTools.Count);
            return returnList;
        }

        private static ResourceType getResourceType(int type)
        {
            if (type == 0)
                return ResourceType.Wood;
            if(type == 1)
                return ResourceType.Ore;
            if (type == 2)
                return ResourceType.Hide;
            if (type == 3)
                return ResourceType.Rock;
            if (type == 4)
                return ResourceType.Fiber;
            return ResourceType.Unknown;
        }

        private static readonly Dictionary<int, string> TOOL_TIER_IDENTIFIERS = new Dictionary<int, string> {
            { 1,"beginner" },
            { 2,"novice" },
            { 3,"journeyman" },
            { 4,"adept" },
            { 5,"expert" },
            { 6,"master" },
            { 7,"grandmaster" },
            { 8,"elder" },
        };
        private static int getToolTier(Api.Direct.IItemStack toolStack)
        {
            foreach(KeyValuePair<int, string> entry in TOOL_TIER_IDENTIFIERS)
                if(toolStack.UniqueName.Contains(entry.Value))
                    return entry.Key;
            return -1;
        }
    }
}
