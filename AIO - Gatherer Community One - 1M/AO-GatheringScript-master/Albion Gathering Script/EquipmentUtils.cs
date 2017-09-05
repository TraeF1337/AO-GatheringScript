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
        private static int getToolTier(IItemStack toolStack)
        {
            foreach(KeyValuePair<int, string> entry in TOOL_TIER_IDENTIFIERS)
                if(toolStack.UniqueName.Contains(entry.Value))
                    return entry.Key;
            return -1;
        }
    }
}
