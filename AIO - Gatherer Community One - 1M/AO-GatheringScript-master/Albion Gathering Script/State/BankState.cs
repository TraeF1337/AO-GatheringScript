﻿using Ennui.Api;
using Ennui.Api.Method;
using Ennui.Api.Script;
using System.Collections.Generic;
using Ennui.Api.Util;

namespace Ennui.Script.Official
{
    public class BankState : StateScript
    {
        private Configuration config;
        private Context context;

        public BankState(Configuration config, Context context)
        {
            this.config = config;
            this.context = context;
        }

        public override int OnLoop(IScriptEngine se)
        {
            var localPlayer = Players.LocalPlayer;
            if (localPlayer == null)
            {
                context.State = "Failed to find local player!";
                return 10_000;
            }

            if (!config.VaultArea.RealArea(Api).Contains(localPlayer.Location))
            {
                context.State = "Walking to vault...";

                var config = new PointPathFindConfig();
                //config.ClusterName = this.config.CityClusterName;
                config.ClusterName = this.config.VaultClusterName;
                config.UseWeb = false;
                config.Point = this.config.VaultDest.RealVector3();
                config.UseMount = true;

                Movement.PathFindTo(config);
                return 0;
            }

            if (config.FleeOnLowHealth && localPlayer.HealthPercentage <= config.FleeHealthPercent)
            {
                return 1000;
            }

            if (!Banking.IsOpen)
            {
                context.State = "Opening vault...";

                var bank = Objects.BankChain.Closest(localPlayer.Location);
                if (bank == null)
                {
                    context.State = "Failed to find vault!";
                    parent.EnterState("gather");
                }

                bank.Click();
                Time.SleepUntil(() =>
                {
                    return Banking.IsOpen;
                }, 4000);
            }

            if (Banking.IsOpen)
            {
                context.State = "Depositing items...";

                var beginWeight = localPlayer.WeighedDownPercent;
                var allItems = Inventory.GetItemsBySubstring("_ROCK", "_ORE", "_HIDE", "_WOOD", "_FIBER");
                var toDeposit = new List<IItemStack>();
                foreach (var stack in allItems)
                {
                    if (!stack.UniqueName.Contains("JOURNAL"))
                    {
                        toDeposit.Add(stack);
                    }
                }

                if (toDeposit.Count == 0 || Banking.Deposit(toDeposit.ToArray()))
                {
                    if (localPlayer.TotalHoldWeight < 99)
                    {
                        if (config.RepairDest != null && Api.HasBrokenItems() && (config.skipRepairing == false))
                        {
                            parent.EnterState("repair");
                        }
                        else
                        {
                            parent.EnterState("walkout");
                        }
                        return 0;
                    }
                }
            }

            return 100;
        }
    }
}
