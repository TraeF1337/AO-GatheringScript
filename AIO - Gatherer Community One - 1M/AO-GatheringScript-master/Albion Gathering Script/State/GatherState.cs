﻿using Ennui.Api.Meta;
using Ennui.Api.Method;
using Ennui.Api.Object;
using Ennui.Api.Script;
using Ennui.Api.Util;
using System;
using System.Collections.Generic;

namespace Ennui.Script.Official
{
    public class GatherState : StateScript
    {
        private Configuration config;
        private Context context;

        private IHarvestableObject harvestableTarget;
        private IMobObject mobTarget;
        private int gatherAttempts = 0;
        private List<long> blacklist = new List<long>();
        private Random rand = new Random();
        private Vector3<float> mountLoc;
        private int counter = 0; 

        public GatherState(Configuration config, Context context)
        {
            this.config = config;
            this.context = context;
        }

        private void Reset()
        {
            harvestableTarget = null;
            mobTarget = null;
            gatherAttempts = 0;
        }

        private bool NeedsNew()
        {
            if (harvestableTarget == null && mobTarget == null)
            {
                return true;
            }

            if (mobTarget != null && (!mobTarget.IsValid || mobTarget.CurrentHealth <= 0))
            {
                return true;
            }

            if (harvestableTarget != null && (!harvestableTarget.IsValid || harvestableTarget.Depleted))
            {
                return true;
            }

            if (gatherAttempts >= config.GatherAttemptsTimeout)
            {
                if (harvestableTarget != null)
                {
                    blacklist.Add(harvestableTarget.Id);
                }
                if (mobTarget != null)
                {
                    blacklist.Add(mobTarget.Id);
                }
                return true;
            }

            return false;
        }

        private IMobObject Closest(Vector3<float> center, List<IMobObject> mobs)
        {
            var dist = 0.0f;
            IMobObject closest = null;
            foreach (var m in mobs)
            {
                var cdist = m.Location.SimpleDistance(center);
                if (closest == null || cdist < dist)
                {
                    dist = cdist;
                    closest = m;
                }
            }

            return closest;
        }

        private bool FindResource(Vector3<float> center)
        {
            context.State = "Finding resource...";

            Reset();
            var territoryAreas = new List<Area>();
            var graph = Graphs.LookupByDisplayName(Game.ClusterName);
            foreach (var t in graph.Territories)
            {
                var tCenter = t.Center;
                var tSize = t.Size;
                var tCenter3d = new Vector3f(tCenter.X, 0, tCenter.Y);
                var tBegin = tCenter3d.Translate(0 - (tSize.X / 2), -100, 0 - (tSize.Y / 2));
                var tEnd = tCenter3d.Translate(tSize.X / 2, 100, tSize.Y / 2);
                var tArea = new Area(tBegin, tEnd);
                territoryAreas.Add(tArea);
            }

            if (config.AttackMobs)
            {
                var lpo = Players.LocalPlayer;
                if (lpo != null && config.IgnoreMobsOnLowHealth && lpo.HealthPercentage > config.IgnoreMobHealthPercent)
                {
                    var resourceMobs = new List<IMobObject>();
                    foreach (var ent in Entities.MobChain.ExcludeByArea(territoryAreas.ToArray()).ExcludeWithIds(blacklist.ToArray()).AsList)
                    {
                        var drops = ent.HarvestableDropChain.FilterByTypeSet(SafeTypeSet.BatchConvert(config.TypeSetsToUse)).AsList;
                        Logging.Log(drops.ToString());
                        if (drops.Count > 0)
                        {
                            if (ent.CurrentActionState != ActionState.Attacking)
                            {
                                resourceMobs.Add(ent);
                            }
                        }
                    }

                    if (resourceMobs.Count > 0)
                    {
                        mobTarget = Closest(center, resourceMobs);
                    }
                }
            }

            harvestableTarget = Objects
                .HarvestableChain
                .FilterDepleted()
                .ExcludeWithIds(blacklist.ToArray())
                .ExcludeByArea(territoryAreas.ToArray())
                .FilterByTypeSet(SafeTypeSet.BatchConvert(config.TypeSetsToUse))
                .FilterWithSetupState(HarvestableSetupState.Invalid)
                .FilterWithSetupState(HarvestableSetupState.Owned)
                .Closest(center);

            if (mobTarget != null && harvestableTarget != null)
            {
                var mobDist = mobTarget.ThreadSafeLocation.SimpleDistance(center);
                var resDist = harvestableTarget.ThreadSafeLocation.SimpleDistance(center);
                if (mobDist < resDist)
                {
                    harvestableTarget = null;
                }
                else
                {
                    mobTarget = null;
                }
            }


            if (config.ingnoreMobCampNodes)
            {
                foreach (var ent in Entities.MobChain.ExcludeByArea(territoryAreas.ToArray()).ExcludeWithIds(blacklist.ToArray()).AsList)
                {
                    if (harvestableTarget != null)
                    {
                        var mobDistanceFromNode = harvestableTarget.Location.SimpleDistance(ent.Location);
                        var drops = ent.HarvestableDropChain.FilterByTypeSet(SafeTypeSet.BatchConvert(config.TypeSetsToUse)).AsList;
                        if (mobDistanceFromNode < 35 && drops.Count <= 0)
                        {
                            if (ent.MaxHealth > 500)
                            {
                                blacklist.Add(harvestableTarget.Id);
                                Reset();
                            }
                        }
                    }

                    if (mobTarget != null)
                    {
                        var mobDistanceFromNode = mobTarget.Location.SimpleDistance(ent.Location);
                        var drops = ent.HarvestableDropChain.FilterByTypeSet(SafeTypeSet.BatchConvert(config.TypeSetsToUse)).AsList;
                        if (mobDistanceFromNode < 35 && drops.Count <= 0)
                        {
                            if (ent.MaxHealth > 500)
                            {
                                blacklist.Add(mobTarget.Id);
                                Reset();
                            }
                        }
                    }
                }
            }

            return harvestableTarget != null || mobTarget != null;
        }       

        private Vector3<float> RandomRoamPoint()
        {
            if (config.RoamPoints.Count == 0)
            {
                return null;
            }
            if (config.RoamPoints.Count == 1)
            {
                return config.RoamPoints[0].RealVector3();
            }
            return config.RoamPoints[rand.Next(config.RoamPoints.Count)].RealVector3();
        }


        private Boolean ShouldUseMount(float heldWeight, float dist)
        {
            var useMount = true;
            
            if (dist >= 15)
            {
                useMount = true;
            }
            else if (heldWeight >= 100 && dist >= 6)
            {
                useMount = true;
            }
            else if (heldWeight >= 120 && dist >= 3)
            {
                useMount = true;
            }
            else if (heldWeight >= 135)
            {
                useMount = true;
            }

            return useMount;
        }

        public override int OnLoop(IScriptEngine se)
        {
            var localPlayer = Players.LocalPlayer;
            var localLocation = localPlayer.Location;

            // ediablo do nothing if dead
            if (localPlayer.CurrentActionState == (ActionState)17 || localPlayer.CurrentActionState == ActionState.KnockedDown)
            {
                return 250;
            }

            // <------------ wtf123 start
            if (blacklist.Count > 350)
            {
                blacklist.Clear();
            }
                if (localPlayer.HealthPercentage < 30 || blacklist.Count>350)
            {
                blacklist.Clear();
                var loc = localLocation;
                Logging.Log("Add mobcamp point " + loc.X + " " + loc.Y + " " + loc.Z);
                config.ResourceClusterName = Game.ClusterName;
                config.mobCamps.Add(new SafeVector3(new Vector3f(loc.X, loc.Y, loc.Z)));
            }

            if (localPlayer.IsMounted || mountLoc == null)
            {
                mountLoc = localLocation;
                var loc = mountLoc;
                this.config.mountLoc = new SafeVector3(new Vector3f(loc.X, loc.Y, loc.Z));
            }
            this.config.blackList = blacklist.Count;

            // <------------ wtf123 End
            if (config.RepairDest != null && Api.HasBrokenItems() && (config.skipRepairing == false))
            {
                blacklist.Clear();
                if (config.usePathPoints)
                {
                    parent.EnterState("ppthreeb");
                    return 0;
                }
                else
                {
                    parent.EnterState("repair");
                    return 0;
                }
            }

            if (localPlayer == null)
            {
                context.State = "Failed to find local player!";
                Logging.Log("Failed to find local player!", LogLevel.Error);
                return 10_000;
            }

            if (localPlayer.IsUnderAttack)
            {
                Logging.Log("Local player under attack, fight back!", LogLevel.Atom);
                parent.EnterState("combat");
                return 0;
            }

            //if (!config.GatherArea.RealArea(Api).Contains(localLocation))
            if (!config.ResourceArea.RealArea(Api).Contains(localLocation))
            {
                if (config.roamPointFirst)
                {
                    context.State = "Walking to 1st Roam Point..";
                    Logging.Log("Local player not in gather area, walk there!", LogLevel.Atom);
                    var moveConfig = new PointPathFindConfig();
                    moveConfig.UseWeb = false;
                    moveConfig.ClusterName = config.ResourceClusterName;
                    moveConfig.Point = ConfigState.firstRoamPoint;
                    if (Movement.PathFindTo(moveConfig) != PathFindResult.Success)
                    {
                        Logging.Log("Local player failed to find path to resource area!", LogLevel.Error);
                        return 10_000;
                    }
                }
                else
                {
                    context.State = "Walking to gather area...";
                    Logging.Log("Local player not in gather area, walk there!", LogLevel.Atom);
                    var moveConfig = new PointPathFindConfig();
                    moveConfig.UseWeb = false;
                    moveConfig.ClusterName = config.ResourceClusterName;
                    if (Movement.PathFindTo(moveConfig) != PathFindResult.Success)
                    {
                        Logging.Log("Local player failed to find path to resource area!", LogLevel.Error);
                        return 10_000;
                    }
                }
            }
            
			config.currentWeight = localPlayer.TotalHoldWeight;
            var currentWeight = config.currentWeight;
            var maxHoldWeight = config.MaxHoldWeight;
            if (localPlayer.WeighedDownPercent > 98 && !localPlayer.IsMounted)
            {
                if (harvestableTarget != null && Players.LocalPlayer.Location.SimpleDistance(harvestableTarget.Location) > 3)
                {
                    localPlayer.ToggleMount(true);
                }
            }

            Time.SleepUntil(() =>
                {
                    return localPlayer.IsMounted;
                }, 1000);


            if (localPlayer.WeighedDownPercent > 98 && localPlayer.IsMounted)
            {
                Logging.Log("Local player has too much weight, banking!", LogLevel.Atom);
                blacklist.Clear();
                if (config.usePathPoints)
                {
                    parent.EnterState("ppthreeb");
                    return 0;
                }
                else
                {
                    parent.EnterState("bank");
                    return 0;
                }
            }             

            if (NeedsNew())
            {
                if (!FindResource(localLocation))
                {
                    var point = RandomRoamPoint();
                    if (point == null)
                    {
                        context.State = "Failed to find roam point!";
                        Logging.Log("Cannot roam as roam points were not added!");
                        return 15000;
                    }

                    context.State = "Roaming";
                    var moveConfig = new PointPathFindConfig();
                    moveConfig.UseWeb = false;
                    moveConfig.ClusterName = config.ResourceClusterName;
                    moveConfig.Point = point;
                    moveConfig.UseMount = true;
                    moveConfig.ExitHook = (() =>
                    {
                        var local = Players.LocalPlayer;
                        if (local != null)
                        {
                            return FindResource(local.Location);
                        }
                        return false;
                    });

                    if (Movement.PathFindTo(moveConfig) != PathFindResult.Success)
                    {
                        context.State = "Failed to find path to roaming point...";
                        return 10_000;
                    }
                    return 5000;
                }
            }

            if (harvestableTarget != null)
            {
                 if (config.mobCamps.Count > 0)
                {
                    foreach (var m in config.mobCamps)
                    {
                        var mobCampArea = m.RealVector3().Expand(25, 3, 25);
                        if (mobCampArea.Contains(harvestableTarget.Location))
                        {
                            if (!blacklist.Contains(harvestableTarget.Id))
                            {
                                blacklist.Add(harvestableTarget.Id);
                                Reset();
                                return 0;
                            }
                        }
                    }
                }
                if (harvestableTarget.Type == ResourceType.Fiber)
                {
                    var area = harvestableTarget.Location.Expand(8, 8, 8);
                    if (area.Contains(mountLoc))
                    {
                        if (localPlayer.IsMounted)
                        {
                            localPlayer.ToggleMount(false);
                        }

                        context.State = "Attempting to gather " + harvestableTarget.Type + " (" + gatherAttempts + ")";
                        harvestableTarget.Click();

                        //if (harvestableTarget.RareState == 0 && harvestableTarget.Tier != 3)
   //                     if (harvestableTarget.RareState == 0 && config.blacklistEnabled)
    //                    {
     //                       if (!blacklist.Contains(harvestableTarget.Id))
      //                          blacklist.Add(harvestableTarget.Id);
       //                 }

                        Time.SleepUntil(() =>
                        {
                            return localPlayer.IsHarvesting;
                        }, 1000);

                        if (!localPlayer.IsHarvesting)
                        {
                            gatherAttempts = gatherAttempts + 1;
                        }

                        return 100;
                    }
                    else
                    {
                        context.State = "Walking to resource...";

                        var dist = mountLoc.SimpleDistance(harvestableTarget.Location);
                        var config2 = new PointPathFindConfig();
                        this.config.dist = dist;
                        config2.ClusterName = this.config.ResourceClusterName;
                        config2.UseWeb = false;
                        config2.Point = harvestableTarget.Location;
                        config2.UseMount = ShouldUseMount(currentWeight, dist);
                        config2.ExitHook = (() =>
                        {
                            var lpo = Players.LocalPlayer;
                            if (lpo == null) return false;

                            if (!lpo.IsMounted && lpo.IsUnderAttack)
                            {
                                parent.EnterState("combat");
                                return true;
                            }

                            if (config.forceMount)
                            {
                                if (!lpo.IsMounted && dist > 15)
                                {
                                    localPlayer.ToggleMount(true);
                                    Time.SleepUntil(() =>
                                    {
                                        return localPlayer.IsMounted;
                                    }, 3000);
                                    return true;
                                }
                            }


                            if (!lpo.IsMounted && dist > 15)
                            {
                                localPlayer.ToggleMount(true);
                                Time.SleepUntil(() =>
                                {
                                    return localPlayer.IsMounted;
                                }, 3000);
                                return true;
                            }

                            if (!harvestableTarget.IsValid || harvestableTarget.Depleted)
                            {
                                return true;
                            }

                            return false;
                        });

                        var result = Movement.PathFindTo(config2);
                        if (result == PathFindResult.Failed)
                        {
                            context.State = "Failed to path find to resource!";
                            blacklist.Add(harvestableTarget.Id);
                            Reset();
                        }
                        return 0;
                    }
                }
                else
                {
                    var area = harvestableTarget.InteractBounds;
                    if (area.Contains(localLocation))
                    {
                        if (localPlayer.IsMounted)
                        {
                            localPlayer.ToggleMount(false);
                        }

                        context.State = "Attempting to gather " + harvestableTarget.Type + " (" + gatherAttempts + ")";
                        harvestableTarget.Click();

                        //if (harvestableTarget.RareState == 0 && harvestableTarget.Tier != 3)
        //                if (harvestableTarget.RareState == 0 && config.blacklistEnabled)
         //               {
          //                  if (!blacklist.Contains(harvestableTarget.Id))
           //                     blacklist.Add(harvestableTarget.Id);
            //            }


                        Time.SleepUntil(() =>
                        {
                            return localPlayer.IsHarvesting;
                        }, 3000);

                        if (!localPlayer.IsHarvesting)
                        {
                            gatherAttempts = gatherAttempts + 1;
                        }

                        return 100;
                    }
                    else
                    {
                        context.State = "Walking to resource...";

                        var dist = localLocation.SimpleDistance(harvestableTarget.Location);
                        var config = new ResourcePathFindConfig();
                        this.config.dist = dist;
                        config.ClusterName = this.config.ResourceClusterName;
                        config.UseWeb = false;
                        config.Target = harvestableTarget;
                        config.UseMount = ShouldUseMount(currentWeight, dist);
                        config.ExitHook = (() =>
                        {
                            var lpo = Players.LocalPlayer;
                            if (lpo == null) return false;

                            if (!lpo.IsMounted && lpo.IsUnderAttack)
                            {
                                parent.EnterState("combat");
                                return true;

                            }

                            if (!lpo.IsMounted && dist > 15)
                            {
                                localPlayer.ToggleMount(true);
                                Time.SleepUntil(() =>
                                {
                                    return localPlayer.IsMounted;
                                }, 3000);
                                return true;
                            }

                            if (!harvestableTarget.IsValid || harvestableTarget.Depleted)
                            {
                                return true;
                            }

                            return false;
                        });

                        var result = Movement.PathFindTo(config);
                        if (result == PathFindResult.Failed)
                        {
                            context.State = "Failed to path find to resource!";
                            blacklist.Add(harvestableTarget.Id);
                            Reset();
                        }
                        return 0;
                    }
                }

            }
            else if (mobTarget != null && !mobTarget.IsUnderAttack)
            {
                if (config.mobCamps.Count > 0)
                {
                    foreach (var m in config.mobCamps)
                    {
                        var mobCampArea = m.RealVector3().Expand(25, 3, 25);
                        if (mobCampArea.Contains(mobTarget.Location))
                        {
                            if (!blacklist.Contains(mobTarget.Id))
                            {
                                blacklist.Add(mobTarget.Id);
                                Reset();
                                return 0;
                            }
                        }
                    }
                }
                if (config.mobCamps.Count > 0)
                {
                    foreach (var m in config.mobCamps)
                    {
                        var mobCampArea = m.RealVector3().Expand(20, 8, 20);
                        if (mobCampArea.Contains(mobTarget.Location))
                        {
                            if (!blacklist.Contains(mobTarget.Id))
                                blacklist.Add(mobTarget.Id);
                        }
                    }
                }
                var mobGatherArea = mobTarget.Location.Expand(8, 8, 8);
                if (mobGatherArea.Contains(localLocation))
                {
                    context.State = "Attempting to kill mob";

                    if (localPlayer.IsMounted)
                    {
                        localPlayer.ToggleMount(false);
                    }

                    localPlayer.SetSelectedObject(mobTarget);
                    localPlayer.AttackSelectedObject();

                    if (localPlayer.HealthPercentage < 1)
                    {
                        var loc = localLocation;
                        Logging.Log("Add mobcamp point " + loc.X + " " + loc.Y + " " + loc.Z);
                        config.ResourceClusterName = Game.ClusterName;
                        config.mobCamps.Add(new SafeVector3(new Vector3f(loc.X, loc.Y, loc.Z)));
                    }

                    if (!mobTarget.IsValid || mobTarget.CurrentHealth <= 0 && config.blacklistEnabled)
                    {
                        blacklist.Add(mobTarget.Id);
                    }
                    var mobDrop = mobTarget.HarvestableDropChain
                           .FilterByTypeSet(SafeTypeSet.BatchConvert(config.TypeSetsToUse))
                           .AsList;

                    //if (mobTarget.RareState == 0 || mobDrop[0].Tier == 2) mobTarget
         //           if (mobTarget.RareState == 0 && this.config.blacklistEnabled)
          //          {
           //             if (!blacklist.Contains(mobTarget.Id))
            //                blacklist.Add(mobTarget.Id);
             //       }


                    Time.SleepUntil(() =>
                    {
                        return localPlayer.IsUnderAttack;
                    }, 1000);

                    if (localPlayer.IsUnderAttack)
                    {
                        parent.EnterState("combat");
                    }
                    else
                    {
                        gatherAttempts = gatherAttempts + 1;
                    }

                    return 100;
                }
                else
                {
                    context.State = "Walking to mob...";

                    var dist = mountLoc.SimpleDistance(mobTarget.Location);
                    var config = new PointPathFindConfig();
                    //this.config.dist = dist;
                    config.ClusterName = this.config.ResourceClusterName;
                    config.UseWeb = false;
                    config.Point = mobTarget.Location;
                    config.UseMount = ShouldUseMount(currentWeight, dist);
                    config.ExitHook = (() =>
                    {
                        var lpo = Players.LocalPlayer;
                        if (lpo == null) return false;

                        if (!lpo.IsMounted && lpo.IsUnderAttack)
                        {
                            parent.EnterState("combat");
                            return true;
                        }
                        

                        //if (mobTarget.RareState == 0)
      //                  if (mobTarget.RareState == 0 && this.config.blacklistEnabled)
       //                 {
        //                    if (!blacklist.Contains(mobTarget.Id))
         //                       blacklist.Add(mobTarget.Id);
          //              }

                        if (!mobTarget.IsValid || mobTarget.CurrentHealth <= 0)
                        {
                            blacklist.Add(mobTarget.Id);
                            return true;
                        }


                        return false;
                    });

                    if (Movement.PathFindTo(config) == PathFindResult.Failed)
                    {
                        context.State = "Failed to path find to mob!";
                        counter = counter + 1;
                        if (counter > 2)
                        {
                            counter = 0; 
                            blacklist.Add(mobTarget.Id);
                            Reset();
                        }
                    }
                    return 0;
                }
            }
            return 100;
        }
    }
}
