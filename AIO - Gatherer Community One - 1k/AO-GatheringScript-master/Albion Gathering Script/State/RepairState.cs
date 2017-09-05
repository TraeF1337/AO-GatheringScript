using Ennui.Api;
using Ennui.Api.Method;
using Ennui.Api.Script;

namespace Ennui.Script.Official
{
    public class RepairState : StateScript
    {
        private Configuration config;
        private Context context;

        public RepairState(Configuration config, Context context)
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

			if (config.enableRepairWayPoints)
			{
				if (!config.RepairArea.RealArea(Api).Contains(localPlayer.Location))
				{
					context.State = "Walking to repair area...";
					bool reachedWP1 = false;
					bool reachedWP2 = false;
					bool reachedWP3 = false;

					do
					{
						if (!reachedWP1 && !reachedWP2 && !reachedWP3)
						{
							context.State = "Walking to repair WP-1...";
							var config1 = new PointPathFindConfig();
							config1.ClusterName = this.config.RepairClusterName;
							config1.Point = this.config.RepairWayPointOneDest.RealVector3();
							config1.UseWeb = false;
							config1.UseMount = true;
							Movement.PathFindTo(config1);
							reachedWP1 = true;
						}

						if (reachedWP1 && !reachedWP2 && !reachedWP3)
						{
							context.State = "Walking to repair WP-2...";
							var config2 = new PointPathFindConfig();
							config2.ClusterName = this.config.RepairClusterName;
							config2.Point = this.config.RepairWayPointTwoDest.RealVector3();
							config2.UseWeb = false;
							config2.UseMount = true;
							Movement.PathFindTo(config2);
							reachedWP2 = true;
						}

						if (reachedWP1 && reachedWP2 && !reachedWP3)
						{
							context.State = "Walking to repair WP-3...";
							var config3 = new PointPathFindConfig();
							config3.ClusterName = this.config.RepairClusterName;
							config3.Point = this.config.RepairWayPointThreeDest.RealVector3();
							config3.UseWeb = false;
							config3.UseMount = true;
							Movement.PathFindTo(config3);
							reachedWP3 = true;
						}

					} while (!reachedWP1 && !reachedWP2 && !reachedWP3);

					var config = new PointPathFindConfig();
                    //config.ClusterName = this.config.CityClusterName;
                    config.ClusterName = this.config.RepairClusterName;
                    config.Point = this.config.RepairDest.RealVector3();
					config.UseWeb = false;
					config.UseMount = true;
					Movement.PathFindTo(config);
					return 0;
				}
			} 
			else
			{
				if (!config.RepairArea.RealArea(Api).Contains(localPlayer.Location))
				{
					context.State = "Walking to repair area...";

					var config = new PointPathFindConfig();
					config.ClusterName = this.config.RepairClusterName;
					config.Point = this.config.RepairDest.RealVector3();
					config.UseWeb = false;
					config.UseMount = true;
					Movement.PathFindTo(config);
					return 0;
				}
				
			}


            if (localPlayer.IsMounted)
            {
                localPlayer.ToggleMount(false);
            }

            if (!Api.HasBrokenItems())
            {
				if (config.enableRepairWayPoints)
				{
					bool reachedWP1 = false;
					bool reachedWP2 = false;
					bool reachedWP3 = false;

					do
					{
						if (!reachedWP1 && !reachedWP2 && !reachedWP3)
						{
							context.State = "Walking to repair WP-3...";
							var config1 = new PointPathFindConfig();
							config1.ClusterName = this.config.RepairClusterName;
							config1.Point = this.config.RepairWayPointThreeDest.RealVector3();
							config1.UseWeb = false;
							config1.UseMount = true;
							Movement.PathFindTo(config1);
							reachedWP3 = true;
						}

						if (!reachedWP1 && !reachedWP2 && reachedWP3)
						{
							context.State = "Walking to repair WP-2...";
							var config2 = new PointPathFindConfig();
							config2.ClusterName = this.config.RepairClusterName;
							config2.Point = this.config.RepairWayPointTwoDest.RealVector3();
							config2.UseWeb = false;
							config2.UseMount = true;
							Movement.PathFindTo(config2);
							reachedWP2 = true;
						}

						if (!reachedWP1 && reachedWP2 && reachedWP3)
						{
							context.State = "Walking to repair WP-1...";
							var config3 = new PointPathFindConfig();
							config3.ClusterName = this.config.RepairClusterName;
							config3.Point = this.config.RepairWayPointOneDest.RealVector3();
							config3.UseWeb = false;
							config3.UseMount = true;
							Movement.PathFindTo(config3);
							reachedWP1 = true;
						}

					} while (!reachedWP1 && !reachedWP2 && !reachedWP3);
				}

				if (localPlayer.WeighedDownPercent >= 5)
                {
					parent.EnterState("bank");
				}
                else
                {
					parent.EnterState("walkout");	
				}
            }

            if (!RepairWindow.IsOpen)
            {
                context.State = "Opening repair building...";

                var building = Objects.RepairChain.Closest(localPlayer.Location);
                if (building == null)
                {
                    context.State = "Failed to find repair building!";
                    return 5000;
                }

                building.Click();
                Time.SleepUntil(() =>
                  {
                      return RepairWindow.IsOpen;
                  }, 100);
            }

            if (RepairWindow.IsOpen)
            {
                context.State = "Repairing...";

                if (RepairWindow.RepairAll())
                {
                    Time.SleepUntil(() =>
                    {
                        return !Api.HasBrokenItems();
                    }, 60000);
                }
            }

            return 100;
        }
    }
}
