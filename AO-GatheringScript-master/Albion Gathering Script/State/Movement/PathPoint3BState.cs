using Ennui.Api;
using Ennui.Api.Method;
using Ennui.Api.Script;

namespace Ennui.Script.Official
{
    public class PathPoint3BState : StateScript
    {
        private Configuration config;
        private Context context;

        public PathPoint3BState(Configuration config, Context context)
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
                if (!config.PP3Area.RealArea(Api).Contains(localPlayer.Location))
                {
                    context.State = "Going to PP3 (returning)...";

                    var config = new PointPathFindConfig();
                    config.ClusterName = this.config.PP3Name;
                    config.Point = this.config.PP3Dest.RealVector3();
                    config.UseWeb = false;
                    config.UseMount = true;
                    Movement.PathFindTo(config);
                    Time.SleepUntil(() =>
                    {
                        return localPlayer.IsMoving;
                    }, 1000);
                    return 0;
                }

                if (config.PP3Area.RealArea(Api).Contains(localPlayer.Location))
                {
                    parent.EnterState("pptwob");
                }

            }

            return 500;
        }
    }
}
