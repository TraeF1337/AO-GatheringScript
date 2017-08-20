using Ennui.Api;
using Ennui.Api.Method;
using Ennui.Api.Script;

namespace Ennui.Script.Official
{
    public class PathPoint1BState : StateScript
    {
        private Configuration config;
        private Context context;

        public PathPoint1BState(Configuration config, Context context)
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
                if (!config.PP1Area.RealArea(Api).Contains(localPlayer.Location))
                {
                    context.State = "Going to PP1 (returning)...";

                    var config = new PointPathFindConfig();
                    config.ClusterName = this.config.PP1Name;
                    config.Point = this.config.PP1Dest.RealVector3();
                    config.UseWeb = false;
                    config.UseMount = true;
                    Movement.PathFindTo(config);
                    Time.SleepUntil(() =>
                    {
                        return localPlayer.IsMoving;
                    }, 1000);
                    return 0;
                }

                if (config.PP1Area.RealArea(Api).Contains(localPlayer.Location))
                {
                    parent.EnterState("walkin"); 
                }

            }

            return 500;
        }
    }
}
