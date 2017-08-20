using Ennui.Api;
using Ennui.Api.Method;
using Ennui.Api.Script;

namespace Ennui.Script.Official
{
    public class PathPoint3State : StateScript
    {
        private Configuration config;
        private Context context;

        public PathPoint3State(Configuration config, Context context)
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
                    context.State = "Go to PP3";

                    var config = new PointPathFindConfig();
                    config.ClusterName = this.config.PP3Name;
                    config.Point = this.config.PP3Dest.RealVector3();
                    config.UseWeb = false;
                    config.UseMount = true;
                    Movement.PathFindTo(config);
                    Time.SleepUntil(() =>
                    {
                        return localPlayer.IsMoving;
                    }, 3000);
                    return 0;
                }

                if (config.PP3Area.RealArea(Api).Contains(localPlayer.Location))
                {
                    if (config.roamPointFirst)
                    {
                        context.State = "Walking to first roampoint..";

                        var config = new PointPathFindConfig();
                        config.ClusterName = this.config.ResourceClusterName;
                        config.Point = ConfigState.firstRoamPoint;
                        config.UseWeb = false;
                        config.UseMount = true;
                        Movement.PathFindTo(config);
                        if (Movement.PathFindTo(config) != PathFindResult.Success)
                        {
                            Logging.Log("Local player failed to find path to resource area!", LogLevel.Error);
                            return 10_000;
                        }
                    }
                    var amIClose = Players.LocalPlayer.Location.SimpleDistance(ConfigState.firstRoamPoint);
                    if (amIClose <= 10)
                    {
                        parent.EnterState("gather"); 
                    }
                    return 0;
                }
                else
                {
                    parent.EnterState("gather");
                    return 0;
                }

            }

            return 1000;
        }
    }
}
