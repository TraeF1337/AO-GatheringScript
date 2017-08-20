using Ennui.Api;
using Ennui.Api.Method;
using Ennui.Api.Script;

namespace Ennui.Script.Official
{
    public class WalkInState : StateScript
    {
        private Configuration config;
        private Context context;

        public WalkInState(Configuration config, Context context)
        {
            this.config = config;
            this.context = context;
        }

        public override int OnLoop(IScriptEngine se)
        {
            Time.SleepUntil(() => !Game.InLoadingScreen, 30000);
            if (Game.InLoadingScreen)
            {
                Logging.Log("In loading screen too long, exiting script...", LogLevel.Error);
                se.StopScript();
                return 0;
            }

            var localPlayer = Players.LocalPlayer;
            if (localPlayer != null)
            {
                if (!config.ExitArea.RealArea(Api).Contains(localPlayer.Location))
                {
                    context.State = "Going to Exit Waypoint (returning)..";

                    var config = new PointPathFindConfig();
                    config.ClusterName = this.config.WOName; 
                    config.Point = this.config.ExitDest.RealVector3();
                    config.UseWeb = false;
                    config.UseMount = true;
                    Movement.PathFindTo(config);
                    return 0;
                }

                if (config.ExitArea.RealArea(Api).Contains(localPlayer.Location))
                {
                    if (config.RepairDest != null && Api.HasBrokenItems() && (config.skipRepairing == false))
                    {
                        parent.EnterState("repair");
                    }
                    else
                    {
                        parent.EnterState("bank"); 
                    }
                }

            }

            return 1000;
        }
    }
}
