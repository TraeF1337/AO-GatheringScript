using Ennui.Api;
using Ennui.Api.Builder;
using System.Collections.Generic;

namespace Ennui.Script.Official
{
    public class Configuration
    {
        public SafeVector3 mountLoc = null;

        public List<SafeVector3> mobCamps = new List<SafeVector3>();
        public float dist = 0f;
        public float blackList = 0f;
        public bool debugInfo = true;
        public bool blacklistEnabled = true;
        public bool ingnoreMobCampNodes = true;

        //public string CityClusterName = "";
        public SafeMapArea ResourceArea;
        public string ResourceClusterName = "";


        //************* ediablo
        public string PP1Name = "";
        public SafeMapArea PP1Area;
        public SafeVector3 PP1Dest;
        public string PP2Name = "";
        public SafeMapArea PP2Area;
        public SafeVector3 PP2Dest;
        public string PP3Name = "";
        public SafeMapArea PP3Area;
        public SafeVector3 PP3Dest;
        public bool usePathPoints = true;
        public float myX;
        public float myY;
        public float myZ;
        public string WOName = "";
        public bool roamPointFirst = true;
        //

        //public SafeMapArea GatherArea;
        public SafeMapArea VaultArea;
        //public SafeMapArea RepairArea;
        public SafeVector3 VaultDest;
        public string VaultClusterName = "";      
        public SafeMapArea RepairArea;
        public SafeVector3 RepairDest;
        public string RepairClusterName = "";

        public bool AutoRelogin = false;
        public string LoginEmail = "";
        public string LoginPassword = "";
        public string LoginCharacterName = "";
        public int GatherAttemptsTimeout = 2;

        public bool AttackMobs = false;
        public bool IgnoreMobsOnLowHealth = true;
        public int IgnoreMobHealthPercent = 60;

        public bool FleeOnLowHealth = true;
        public int FleeHealthPercent = 30;

        public float MaxHoldWeight = 100.0f;

        public bool GatherWood = false;
        public bool GatherOre = false;
        public bool GatherFiber = false;
        public bool GatherHide = false;
        public bool GatherStone = false;
       
        public List<SafeTypeSet> TypeSetsToUse = new List<SafeTypeSet>();
        public List<SafeVector3> RoamPoints = new List<SafeVector3>();

		// MadMonk Extras
		public bool skipRepairing = false;
		public float currentWeight = 0.0f;
        public bool enableRepairWayPoints = false;

        //public bool enableTwoZoneCrossing = false;

        public SafeMapArea ExitArea;
		public SafeVector3 ExitDest;

		public SafeMapArea RepairWayPointOneArea;
		public SafeMapArea RepairWayPointTwoArea;
		public SafeMapArea RepairWayPointThreeArea;

		public SafeVector3 RepairWayPointOneDest;
		public SafeVector3 RepairWayPointTwoDest;
		public SafeVector3 RepairWayPointThreeDest;

		//public SafeMapArea interConnectOneArea;
		//public SafeMapArea interConnectTwoArea;

		//public SafeVector3 interConnectOneDest;
		//public SafeVector3 interConnectTwoDest;
	}
}
