using Ennui.Api;
using Ennui.Api.Direct.Object;
using Ennui.Api.Meta;
using Ennui.Api.Method;
using Ennui.Api.Script;
using System;
using System.Collections.Generic;

namespace Ennui.Script.Official
{
    [LocalScript]
    public class GatheringState : StateScript
    {
        private Configuration config;
        private Context context;
        private Timer timer;
        private Api.Direct.Object.ILocalPlayerObject localPlayer;

        private void LoadConfig()
        {
            localPlayer = Players.LocalPlayer;
            var extention = ".json";
            var title = "Albion_Ennui_gatherer_for_";
            var file = title + localPlayer.Name + extention;
            try
            {
                if (Files.Exists(file))
                {
                    config = Codecs.FromJson<Configuration>(Files.ReadAllText(file));
                    if (config.TypeSetsToUse == null)
                    {
                        config.TypeSetsToUse = new List<SafeTypeSet>();
                    }
                }
            }
            catch (Exception e)
            {
                Logging.Log("Failed to log config " + e, LogLevel.Error);
            }

            if (config == null)
            {
                config = new Configuration();
            }
        }
        public override bool OnStart(IScriptEngine se)
        {
            LoadConfig();

            context = new Context();
            timer = new Timer();

            AddHook(() =>
            {
				if (RunningKey != "config"
                    && RunningKey != "login"
                    && config.AutoRelogin
                    && (LoginWindow.IsOpen || CharacterSelectWindow.IsOpen || LoginErrorWindow.IsOpen))
				{
					Logging.Log("Hooking login");
					EnterState("login");
				}
				return false;
			});

            AddState("config", new ConfigState(config, context));
            AddState("resolve", new ResolveState(config, context));
            AddState("gather", new GatherState(config, context));
            AddState("combat", new CombatState(config, context));
            AddState("repair", new RepairState(config, context));
            AddState("bank", new BankState(config, context));
			AddState("login", new LoginState(config, context));
			EnterState("config");
            return base.OnStart(se);
        }

        public override void OnPaint(IScriptEngine se, GraphicContext g)
        {
            if (config.debugInfo)
            {
                var theEqualizer = Game.ScreenSize.X - 265;
                g.SetColor(new Color(0.3f, 0.3f, 0.3f, 1.0f));
                //g.FillRect(theEqualizer - 5, 100, 265, 325);
                g.SetColor(new Color(1.0f, 1.0f, 1.0f, 1.0f));
                g.DrawString("Albion Ennui Gatherer", theEqualizer, 100);
                g.DrawString(string.Format("Runtime: {0}", Time.FormatElapsed(timer.ElapsedMs)), theEqualizer, 115);
                g.DrawString(string.Format("State: {0}", context.State), theEqualizer, 130);
                g.DrawString(string.Format("Max hold weight: {0}", config.MaxHoldWeight), theEqualizer, 145);
                //g.DrawString(string.Format("City cluster: {0}", config.CityClusterName), 20, 160);
                //g.DrawString(string.Format("Resource cluster: {0}", config.ResourceClusterName), 20, 175);
                g.DrawString(string.Format("Bank cluster: {0}", config.VaultClusterName), theEqualizer, 160);
                g.DrawString(string.Format("Repair cluster: {0}", config.RepairClusterName), theEqualizer, 175);
                g.DrawString(string.Format("Resource cluster: {0}", config.ResourceClusterName), theEqualizer, 190);
                g.DrawString(string.Format("Skip Repairing: {0}", config.skipRepairing), theEqualizer, 205);
                g.DrawString(string.Format("Current Weight: {0}", config.currentWeight), theEqualizer, 220);
                g.DrawString(string.Format("Prioritize Roam Point: {0}", config.roamPointFirst), theEqualizer, 235);
                g.DrawString(string.Format("Repair Waypoints Enabled: {0}", config.enableRepairWayPoints), theEqualizer, 250);
                g.DrawString(string.Format("Blacklisted Nodes: {0}", config.blackList), theEqualizer, 265);
                g.DrawString(string.Format("Distance to next node: {0}", config.dist), theEqualizer, 280);
                g.DrawString(string.Format("MobCamps: {0}", config.mobCamps.Count), theEqualizer, 295);
                if (config.ResourceClusterName == Game.ClusterName)
                {
                    foreach (var p in config.RoamPoints)
                    {
                        p.RealVector3().Expand(3, 3, 3).Render(Api, Color.Red, Color.Red.MoreTransparent());
                    }
                }

                //if (config.VaultArea != null && config.CityClusterName == Game.ClusterName)
                if (config.VaultArea != null && config.VaultClusterName == Game.ClusterName)
                {
                    config.VaultArea.RealArea(Api).Render(Api, Color.Cyan, Color.Cyan.MoreTransparent());
                }

                //if (config.RepairArea != null && config.CityClusterName == Game.ClusterName)
                if (config.RepairArea != null && config.RepairClusterName == Game.ClusterName)
                {
                    config.RepairArea.RealArea(Api).Render(Api, Color.Purple, Color.Purple.MoreTransparent());
                }

                // MadMonk Extra

                if (config.ExitArea != null && config.RepairClusterName == Game.ClusterName)
                {
                    config.ExitArea.RealArea(Api).Render(Api, Color.Orange, Color.Orange.MoreTransparent());
                }

                if (config.RepairWayPointOneArea != null && config.RepairClusterName == Game.ClusterName)
                {
                    config.RepairWayPointOneArea.RealArea(Api).Render(Api, Color.Black, Color.Black.MoreTransparent());
                }

                if (config.RepairWayPointTwoArea != null && config.RepairClusterName == Game.ClusterName)
                {
                    config.RepairWayPointTwoArea.RealArea(Api).Render(Api, Color.Green, Color.Green.MoreTransparent());
                }

                if (config.RepairWayPointThreeArea != null && config.RepairClusterName == Game.ClusterName)
                {
                    config.RepairWayPointThreeArea.RealArea(Api).Render(Api, Color.Yellow, Color.Yellow.MoreTransparent());
                }

                // wtf123 Extra

                if (config.mountLoc == null)
                {
                    config.mountLoc = new SafeVector3(new Vector3f(localPlayer.Location.X, localPlayer.Location.Y, localPlayer.Location.Z));
                }
                else
                {
                    // config.mountLoc.RealVector3().Expand(1, 1, 1).Render(Api, Color.Blue, Color.Blue.MoreTransparent());
                }

                if (config.ResourceClusterName == Game.ClusterName)
                {
                    foreach (var p in config.mobCamps)
                    {
                        p.RealVector3().Expand(25, 1, 25).Render(Api, Color.Red, Color.Red.MoreTransparent());
                    }
                }
            }
        }
    }
}
