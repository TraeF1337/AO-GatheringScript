using Ennui.Api;
using Ennui.Api.Builder;
using Ennui.Api.Gui;
using Ennui.Api.Meta;
using Ennui.Api.Script;
using System;
using System.Collections.Generic;

namespace Ennui.Script.Official
{
    public class ConfigState : StateScript
    {
        private IPanel primaryPanel;
        private ILabel tierLabel;
        private IInputField harvestWoodInput;
        private ICheckBox harvestWoodCheckBox;
        private IInputField harvestOreInput;
        private ICheckBox harvestOreCheckBox;
        private IInputField harvestFiberInput;
        private ICheckBox harvestFiberCheckBox;
        private IInputField harvestHideInput;
        private ICheckBox harvestHideCheckBox;
        private IInputField harvestStoneInput;
        private ICheckBox harvestStoneCheckBox;

        private ICheckBox killMobsCheckBox;

        private ICheckBox autoLoginCheckbox;
        private ILabel characterNameLabel;
        private IInputField characterNameInput;

        private IButton setVaultAreaButton;
        private IButton setRepairAreaButton;
        private IButton addRoamPointButton;
		private IButton removeRoamPointButton;

        // ediablo
        private IButton setPP1Button;
        private IButton setPP2Button;
        private IButton setPP3Button;
        private ICheckBox usePathPoints;
        public static Vector3<float> firstRoamPoint;
        private IButton removeAllRoamPointButton;
        private ICheckBox forceMount;
        private ICheckBox debugBg; 
        //


        private IButton runButton;

		// MadMonk Extras
		private ICheckBox skipRepairingCheckBox;
		private ICheckBox roamPointFirstCheckBox;
		private ICheckBox enableRepairWayPointsCheckBox;
		//private ICheckBox twoZoneCrossingCheckBox;

		private IButton setExitAreaButton;

		private IButton setRepairWayPointOneButton;
		private IButton setRepairWayPointTwoButton;
		private IButton setRepairWayPointThreeButton;

        //private IButton addFirstInterConnectButton;
        //private IButton addFinalInterConnectButton;

        // wtf123 Extras
        private IButton addMobCampButton;
        private IButton remMobCampButton;
        private IButton remAllMobCampButton;
        private ICheckBox debugInfoCheckBox;
        private ICheckBox blacklistCheckBox;
        private ICheckBox ignorMobCampNodes; //MobCampNodes

        private Configuration config;
        private Context context;

        private Api.Direct.Object.ILocalPlayerObject localPlayer;

        public ConfigState(Configuration config, Context context)
        {
            this.config = config;
            this.context = context;
        }

        public void UpdateForConfig()
        {
            var sets = new Dictionary<ResourceType, List<string>>();
            sets.Add(ResourceType.Fiber, new List<string>());
            sets.Add(ResourceType.Hide, new List<string>());
            sets.Add(ResourceType.Ore, new List<string>());
            sets.Add(ResourceType.Rock, new List<string>());
            sets.Add(ResourceType.Wood, new List<string>());
            
            foreach (var ts in config.TypeSetsToUse)
            {
                if (sets.ContainsKey(ts.Type))
                {
                    Logging.Log("Adding typeset " + (ts.MaxTier + ts.MaxRarity > 0 ? ("." + ts.MaxRarity) : ""));
                    var input = ts.MaxTier + "." + ts.MaxRarity;
                    
                    sets[ts.Type].Add(ts.MaxTier + ((ts.MinRarity > 0) ? ("." + ts.MaxRarity) : ""));
                }
            }

            if (sets[ResourceType.Fiber].Count > 0)
            harvestFiberInput.SetText(string.Join(",", sets[ResourceType.Fiber].ToArray()));

            if (sets[ResourceType.Hide].Count > 0)
                harvestHideInput.SetText(string.Join(",", sets[ResourceType.Hide].ToArray()));

            if (sets[ResourceType.Ore].Count > 0)
                harvestOreInput.SetText(string.Join(",", sets[ResourceType.Ore].ToArray()));

            if (sets[ResourceType.Rock].Count > 0)
                harvestStoneInput.SetText(string.Join(",", sets[ResourceType.Rock].ToArray()));

            if (sets[ResourceType.Wood].Count > 0)
                harvestWoodInput.SetText(string.Join(",", sets[ResourceType.Wood].ToArray()));

            harvestWoodCheckBox.SetSelected(config.GatherWood);
            harvestOreCheckBox.SetSelected(config.GatherOre);
            harvestFiberCheckBox.SetSelected(config.GatherFiber);
            harvestHideCheckBox.SetSelected(config.GatherHide);
            harvestStoneCheckBox.SetSelected(config.GatherStone);

            killMobsCheckBox.SetSelected(config.AttackMobs);
            autoLoginCheckbox.SetSelected(config.AutoRelogin);

            characterNameInput.SetText(config.LoginCharacterName);

			//MadMonk Extras
			skipRepairingCheckBox.SetSelected(config.skipRepairing);
			roamPointFirstCheckBox.SetSelected(config.roamPointFirst);
			enableRepairWayPointsCheckBox.SetSelected(config.enableRepairWayPoints);
            //twoZoneCrossingCheckBox.SetSelected(config.enableTwoZoneCrossing);

            debugInfoCheckBox.SetSelected(true);
            debugBg.SetSelected(false); 
            forceMount.SetSelected(false); 
            blacklistCheckBox.SetSelected(config.blacklistEnabled);
            ignorMobCampNodes.SetSelected(config.ingnoreMobCampNodes);


            config.TypeSetsToUse.Clear();
        }

        private void AddTiers(ResourceType type, string input)
        {
            if (input.Length == 0)
            {
                return;
            }

            try
            {
                var tierGroups = input.Replace(" ", "").Split(',');
                foreach (var tierGroup in tierGroups)
                {
                    var filtered = tierGroup.Trim(' ', ',');
                    if (filtered.Length == 0)
                    {
                        continue;
                    }

                    var targetInfo = filtered.Split('.');

                    var tier = 0;
                    if (targetInfo.Length >= 1)
                    {
                        if (!int.TryParse(targetInfo[0], out tier))
                        {
                            Logging.Log("Failed to parse tier " + input);
                        }
                    }

                    var rarity = 0;
                    if (targetInfo.Length >= 2)
                    {
                        if (!int.TryParse(targetInfo[1], out rarity))
                        {
                            Logging.Log("Failed to parse rarity " + input);
                        }
                    }

                    config.TypeSetsToUse.Add(new SafeTypeSet(tier, tier, type, rarity, rarity));
                }
            }
            catch (Exception e)
            {
                context.State = "Failed to parise tiers " + input;
                context.State = "Failed to parse tiers " + input;
            }
        }

        public void SaveConfig()
        {
            var extention = ".json";
            var title = "Albion_Ennui_gatherer_for_";
            try
            {
                if (Files.Exists(title + localPlayer.Name + extention))
                {
                    Files.Delete(title + localPlayer.Name + extention);
                }
                Files.WriteText(title + localPlayer.Name + extention, Codecs.ToJson(config));
            }
            catch (Exception e)
            {
                Logging.Log("Failed to save config " + e, LogLevel.Error);
            }
        }

        private void SelectedStart()
        {
            if (harvestWoodCheckBox.IsSelected())
            {
                AddTiers(ResourceType.Wood, harvestWoodInput.GetText());
            }

            if (harvestOreCheckBox.IsSelected())
            {
                AddTiers(ResourceType.Ore, harvestOreInput.GetText());
            }

            if (harvestFiberCheckBox.IsSelected())
            {
                AddTiers(ResourceType.Fiber, harvestFiberInput.GetText());
            }

            if (harvestHideCheckBox.IsSelected())
            {
                AddTiers(ResourceType.Hide, harvestHideInput.GetText());
            }

            if (harvestStoneCheckBox.IsSelected())
            {
                AddTiers(ResourceType.Rock, harvestStoneInput.GetText());
            }

            if (config.TypeSetsToUse.Count == 0)
            {
                context.State = "No type sets to gather!";
                return;
            }

            config.GatherWood = harvestWoodCheckBox.IsSelected();
            config.GatherOre = harvestOreCheckBox.IsSelected();
            config.GatherFiber = harvestFiberCheckBox.IsSelected();
            config.GatherHide = harvestHideCheckBox.IsSelected();
            config.GatherStone = harvestStoneCheckBox.IsSelected();
			//config.enableTwoZoneCrossing = twoZoneCrossingCheckBox.IsSelected();

            config.AttackMobs = killMobsCheckBox.IsSelected();
            config.AutoRelogin = autoLoginCheckbox.IsSelected();
            config.LoginCharacterName = localPlayer.Name;
            //config.GatherArea = new SafeMapArea(config.ResourceClusterName, new Vector3f(-10000, -10000, -10000), new Vector3f(10000, 10000, 10000));
            config.ResourceArea = new SafeMapArea(config.ResourceClusterName, new Vector3f(-10000, -10000, -10000), new Vector3f(10000, 10000, 10000));

			// MadMonk Extras
			config.skipRepairing = skipRepairingCheckBox.IsSelected();
			config.roamPointFirst = roamPointFirstCheckBox.IsSelected();
			config.enableRepairWayPoints = enableRepairWayPointsCheckBox.IsSelected();
            config.blacklistEnabled = blacklistCheckBox.IsSelected();
            config.ingnoreMobCampNodes = ignorMobCampNodes.IsSelected();

            // ediablo Extras
            config.usePathPoints = usePathPoints.IsSelected();
            firstRoamPoint = new Vector3f(config.myX, config.myY, config.myZ);
            // end 

            SaveConfig();
            config.debugInfo = debugInfoCheckBox.IsSelected();
            config.debugBg = debugBg.IsSelected(); 
            config.forceMount = forceMount.IsSelected(); 

            primaryPanel.Destroy();
            parent.EnterState("resolve");
        }

        public override bool OnStart(IScriptEngine se)
        {
            localPlayer = Players.LocalPlayer;
            context.State = "Configuring...";

            Game.Sync(() =>
            {
                var screenSize = Game.ScreenSize;
				int panelSize = 510;
                primaryPanel = Factories.CreateGuiPanel();
                GuiScene.Add(primaryPanel);
                primaryPanel.SetSize(320, panelSize);
                primaryPanel.SetPosition(155, ((screenSize.Y / 2) - 50), 0);
                //primaryPanel.SetPosition((screenSize.X/2) - 50, ((screenSize.Y / 2) - 50), 0);
                primaryPanel.SetAnchor(new Vector2f(0.0f, 0.0f), new Vector2f(0.0f, 0.0f));
                primaryPanel.SetPivot(new Vector2f(0.5f, 0.5f));
				

				int theEqualizer = panelSize / 2;

                tierLabel = Factories.CreateGuiLabel();
                primaryPanel.Add(tierLabel);
                tierLabel.SetPosition(-70, (theEqualizer - 25), 0);
                tierLabel.SetSize(100, 25);
                tierLabel.SetText("Input Tiers");

                harvestWoodInput = Factories.CreateGuiInputField();
                primaryPanel.Add(harvestWoodInput);
                harvestWoodInput.SetPosition(-70, (theEqualizer - 50), 0);
                harvestWoodInput.SetSize(120, 25);

                harvestWoodCheckBox = Factories.CreateGuiCheckBox();
                primaryPanel.Add(harvestWoodCheckBox);
                harvestWoodCheckBox.SetPosition(60, (theEqualizer - 50), 0);
                harvestWoodCheckBox.SetSize(100, 25);
                harvestWoodCheckBox.SetText("Harvest Wood");
                harvestWoodCheckBox.SetSelected(true);

                harvestOreInput = Factories.CreateGuiInputField();
                primaryPanel.Add(harvestOreInput);
                harvestOreInput.SetPosition(-70, (theEqualizer - 75), 0);
                harvestOreInput.SetSize(120, 25);

                harvestOreCheckBox = Factories.CreateGuiCheckBox();
                primaryPanel.Add(harvestOreCheckBox);
                harvestOreCheckBox.SetPosition(60, (theEqualizer - 75), 0);
                harvestOreCheckBox.SetSize(100, 25);
                harvestOreCheckBox.SetText("Harvest Ore");
                harvestOreCheckBox.SetSelected(true);

                harvestFiberInput = Factories.CreateGuiInputField();
                primaryPanel.Add(harvestFiberInput);
                harvestFiberInput.SetPosition(-70, (theEqualizer - 100), 0);
                harvestFiberInput.SetSize(120, 25);

                harvestFiberCheckBox = Factories.CreateGuiCheckBox();
                primaryPanel.Add(harvestFiberCheckBox);
                harvestFiberCheckBox.SetPosition(60, (theEqualizer - 100), 0);
                harvestFiberCheckBox.SetSize(100, 25);
                harvestFiberCheckBox.SetText("Harvest Fiber");
                harvestFiberCheckBox.SetSelected(true);

                harvestHideInput = Factories.CreateGuiInputField();
                primaryPanel.Add(harvestHideInput);
                harvestHideInput.SetPosition(-70, (theEqualizer - 125), 0);
                harvestHideInput.SetSize(120, 25);

                harvestHideCheckBox = Factories.CreateGuiCheckBox();
                primaryPanel.Add(harvestHideCheckBox);
                harvestHideCheckBox.SetPosition(60, (theEqualizer - 125), 0);
                harvestHideCheckBox.SetSize(100, 25);
                harvestHideCheckBox.SetText("Harvest Hide");
                harvestHideCheckBox.SetSelected(true);

                harvestStoneInput = Factories.CreateGuiInputField();
                primaryPanel.Add(harvestStoneInput);
                harvestStoneInput.SetPosition(-70, (theEqualizer - 150), 0);
                harvestStoneInput.SetSize(120, 25);

                harvestStoneCheckBox = Factories.CreateGuiCheckBox();
                primaryPanel.Add(harvestStoneCheckBox);
                harvestStoneCheckBox.SetPosition(60, (theEqualizer - 150), 0);
                harvestStoneCheckBox.SetSize(100, 25);
                harvestStoneCheckBox.SetText("Harvest Stone");
                harvestStoneCheckBox.SetSelected(true);

                killMobsCheckBox = Factories.CreateGuiCheckBox();
                primaryPanel.Add(killMobsCheckBox);
                killMobsCheckBox.SetPosition(-70, (theEqualizer - 175), 0);
                killMobsCheckBox.SetSize(125, 25);
                killMobsCheckBox.SetText("Kill Mobs");
                killMobsCheckBox.SetSelected(true);

                autoLoginCheckbox = Factories.CreateGuiCheckBox();
                primaryPanel.Add(autoLoginCheckbox);
                autoLoginCheckbox.SetPosition(60, (theEqualizer - 175), 0);
                autoLoginCheckbox.SetSize(125, 25);
                autoLoginCheckbox.SetText("Auto Relogin");
                autoLoginCheckbox.SetSelected(true);

                characterNameLabel = Factories.CreateGuiLabel();
                primaryPanel.Add(characterNameLabel);
                characterNameLabel.SetPosition(70, (theEqualizer - 200), 0);
                characterNameLabel.SetSize(125, 25);
                characterNameLabel.SetText("Character Name");

                characterNameInput = Factories.CreateGuiInputField();
                primaryPanel.Add(characterNameInput);
                characterNameInput.SetPosition(70, (theEqualizer - 225), 0);
                characterNameInput.SetSize(125, 25);

                setVaultAreaButton = Factories.CreateGuiButton();
                primaryPanel.Add(setVaultAreaButton);
                setVaultAreaButton.SetPosition(-70, (theEqualizer - 200), 0);
                setVaultAreaButton.SetSize(125, 25);
                setVaultAreaButton.SetText("Set Vault Loc.");
                setVaultAreaButton.AddActionListener((e) =>
                {
                    var local = Players.LocalPlayer;
                    if (local != null)
                    {
                        var loc = local.Location;
                        var area = loc.Expand(4, 2, 4);
                        Logging.Log("Set vault loc to " + loc.X + " " + loc.Y + " " + loc.Z);
                        //config.CityClusterName = Game.ClusterName;
                        config.VaultClusterName = Game.ClusterName;
                        config.VaultDest = new SafeVector3(new Vector3f(loc.X, loc.Y, loc.Z));
                        config.VaultArea = new SafeMapArea(Game.Cluster.Name, new Area(area.Start, area.End));
                    }
                });

                setRepairAreaButton = Factories.CreateGuiButton();
                primaryPanel.Add(setRepairAreaButton);
                setRepairAreaButton.SetPosition(-70, (theEqualizer - 225), 0);
                setRepairAreaButton.SetSize(125, 25);
                setRepairAreaButton.SetText("Set Repair Loc.");
                setRepairAreaButton.AddActionListener((e) =>
                {
                    var local = Players.LocalPlayer;
                    if (local != null)
                    {
                        var loc = local.Location;
                        var area = loc.Expand(4, 2, 4);
                        Logging.Log("Set repair loc to " + loc.X + " " + loc.Y + " " + loc.Z);
                        //config.CityClusterName = Game.ClusterName;
                        config.RepairClusterName = Game.ClusterName;
                        config.RepairDest = new SafeVector3(new Vector3f(loc.X, loc.Y, loc.Z));
                        config.RepairArea = new SafeMapArea(Game.ClusterName, new Area(area.Start, area.End));
                    }
                });

                addRoamPointButton = Factories.CreateGuiButton();
                primaryPanel.Add(addRoamPointButton);
                addRoamPointButton.SetPosition(-70, (theEqualizer - 250), 0);
                addRoamPointButton.SetSize(125, 25);
                addRoamPointButton.SetText("Add Roam Point");
                addRoamPointButton.AddActionListener((e) =>
                {
                    var local = Players.LocalPlayer;
                    if (local != null)
                    {
                        var loc = local.Location;
                        Logging.Log("Add roam point " + loc.X + " " + loc.Y + " " + loc.Z);
                        config.ResourceClusterName = Game.ClusterName;
                        config.RoamPoints.Add(new SafeVector3(new Vector3f(loc.X, loc.Y, loc.Z)));
                        config.myX = config.RoamPoints[0].X;
                        config.myY = config.RoamPoints[0].Y;
                        config.myZ = config.RoamPoints[0].Z;
                    }
                });

				removeRoamPointButton = Factories.CreateGuiButton();
				primaryPanel.Add(removeRoamPointButton);
				removeRoamPointButton.SetPosition(70, (theEqualizer - 300), 0);
				removeRoamPointButton.SetSize(125, 25);
				removeRoamPointButton.SetText("Del Roam Point");
				removeRoamPointButton.AddActionListener((e) =>
				{
					var local = Players.LocalPlayer;
					if (local != null)
					{
						var loc = local.Location;
						Logging.Log("Delete roam point " + loc.X + " " + loc.Y + " " + loc.Z);
						for (var i = 0; i < config.RoamPoints.Count; i++)
						{
							if (config.RoamPoints[i].RealVector3().Expand(3, 3, 3).Contains(loc))
							{
								config.RoamPoints.RemoveAt(i);
								i -= 1;
							}
						}
					}
				});

                // ************* ediablo

                debugBg = Factories.CreateGuiCheckBox();
                primaryPanel.Add(debugBg);
                debugBg.SetPosition(70, (theEqualizer - 25), 0);
                debugBg.SetSize(80, 25);
                debugBg.SetText("Fill Debug");
                debugBg.SetSelected(false);

                forceMount = Factories.CreateGuiCheckBox();
                primaryPanel.Add(forceMount);
                forceMount.SetPosition(110, (theEqualizer - 475), 0);
                forceMount.SetSize(125, 25);
                forceMount.SetText("Force Mount");
                forceMount.SetSelected(false);

                setPP1Button = Factories.CreateGuiButton();
                primaryPanel.Add(setPP1Button);
                setPP1Button.SetPosition(-70, (theEqualizer - 450), 0);
                setPP1Button.SetSize(50, 25);
                setPP1Button.SetText("PP1");
                setPP1Button.AddActionListener((e) =>
                {
                    var local = Players.LocalPlayer;
                    if (local != null)
                    {
                        var loc = local.Location;
                        var area = loc.Expand(4, 2, 4);
                        config.PP1Name = Game.ClusterName;
                        config.PP1Dest = new SafeVector3(new Vector3f(loc.X, loc.Y, loc.Z));
                        config.PP1Area = new SafeMapArea(Game.ClusterName, new Area(area.Start, area.End));
                    }
                });

                setPP2Button = Factories.CreateGuiButton();
                primaryPanel.Add(setPP2Button);
                setPP2Button.SetPosition(0, (theEqualizer - 450), 0);
                setPP2Button.SetSize(50, 25);
                setPP2Button.SetText("PP2");
                setPP2Button.AddActionListener((e) =>
                {
                    var local = Players.LocalPlayer;
                    if (local != null)
                    {
                        var loc = local.Location;
                        var area = loc.Expand(4, 2, 4);
                        config.PP2Name = Game.ClusterName;
                        config.PP2Dest = new SafeVector3(new Vector3f(loc.X, loc.Y, loc.Z));
                        config.PP2Area = new SafeMapArea(Game.ClusterName, new Area(area.Start, area.End));
                    }
                });

                setPP3Button = Factories.CreateGuiButton();
                primaryPanel.Add(setPP3Button);
                setPP3Button.SetPosition(70, (theEqualizer - 450), 0);
                setPP3Button.SetSize(50, 25);
                setPP3Button.SetText("PP3");
                setPP3Button.AddActionListener((e) =>
                {
                    var local = Players.LocalPlayer;
                    if (local != null)
                    {
                        var loc = local.Location;
                        var area = loc.Expand(4, 2, 4);
                        config.PP3Name = Game.ClusterName;
                        config.PP3Dest = new SafeVector3(new Vector3f(loc.X, loc.Y, loc.Z));
                        config.PP3Area = new SafeMapArea(Game.ClusterName, new Area(area.Start, area.End));
                    }
                });

                usePathPoints = Factories.CreateGuiCheckBox();
                primaryPanel.Add(usePathPoints);
                usePathPoints.SetPosition(70, (theEqualizer - 375), 0);
                usePathPoints.SetSize(100, 25);
                usePathPoints.SetText("Use PathPoints?");
                usePathPoints.SetSelected(true);

                removeAllRoamPointButton = Factories.CreateGuiButton();
                primaryPanel.Add(removeAllRoamPointButton);
                removeAllRoamPointButton.SetPosition(70, (theEqualizer - 500), 0);
                removeAllRoamPointButton.SetSize(125, 25);
                removeAllRoamPointButton.SetText("Wipe Roam Points");
                removeAllRoamPointButton.AddActionListener((e) =>
                {
                    config.RoamPoints.Clear(); 
                });


                //



                /*
				addFirstInterConnectButton = Factories.CreateGuiButton();
				primaryPanel.Add(addFirstInterConnectButton);
				addFirstInterConnectButton.SetPosition(-70, (theEqualizer - 400), 0);
				addFirstInterConnectButton.SetSize(125, 25);
				addFirstInterConnectButton.SetText("Add First LWP");
				addFirstInterConnectButton.AddActionListener((e) =>
				{
					var local = Players.LocalPlayer;
					if (local != null)
					{
						var loc = local.Location;
						var area = loc.Expand(4, 2, 4);
						Logging.Log("Set First LWP to " + loc.X + " " + loc.Y + " " + loc.Z);
						config.CityClusterName = Game.ClusterName;
						config.interConnectOneDest = new SafeVector3(new Vector3f(loc.X, loc.Y, loc.Z));
						config.interConnectOneArea = new SafeMapArea(Game.Cluster.Name, new Area(area.Start, area.End));
					}
				});

				addFinalInterConnectButton = Factories.CreateGuiButton();
				primaryPanel.Add(addFinalInterConnectButton);
				addFinalInterConnectButton.SetPosition(70, (theEqualizer - 400), 0);
				addFinalInterConnectButton.SetSize(125, 25);
				addFinalInterConnectButton.SetText("Add Final LWP");
				addFinalInterConnectButton.AddActionListener((e) =>
				{
					var local = Players.LocalPlayer;
					if (local != null)
					{
						var loc = local.Location;
						var area = loc.Expand(4, 2, 4);
						Logging.Log("Set First LWP to " + loc.X + " " + loc.Y + " " + loc.Z);
						config.CityClusterName = Game.ClusterName;
						config.interConnectTwoDest = new SafeVector3(new Vector3f(loc.X, loc.Y, loc.Z));
						config.interConnectTwoArea = new SafeMapArea(Game.Cluster.Name, new Area(area.Start, area.End));
					}
				});
				*/

                runButton = Factories.CreateGuiButton();
                primaryPanel.Add(runButton);
                runButton.SetPosition(-30, (theEqualizer - 475), 0);
                runButton.SetSize(125, 25);
                runButton.SetText("Run");
                runButton.AddActionListener((e) =>
                {
                    if (config.VaultDest == null)
                    {
                        context.State = "No vault area set!";
                        return;
                    }

                    if (config.RoamPoints.Count == 0)
                    {
                        context.State = "No roam points added!";
                        return;
                    }

					// Checks if custom waypoints exist or not
					if (config.ExitDest == null)
					{
						context.State = "No exit waypoint set!";
						return;
					}
                    // End of MadMonk Extras				
                     
					SelectedStart();
                });

				// MadMonk Extras
				skipRepairingCheckBox = Factories.CreateGuiCheckBox();
				primaryPanel.Add(skipRepairingCheckBox);
				skipRepairingCheckBox.SetPosition(-80, (theEqualizer - 325), 0);
				skipRepairingCheckBox.SetSize(80, 25);
				skipRepairingCheckBox.SetText("Skip Repairing");
				skipRepairingCheckBox.SetSelected(false);

				roamPointFirstCheckBox = Factories.CreateGuiCheckBox();
				primaryPanel.Add(roamPointFirstCheckBox);
				roamPointFirstCheckBox.SetPosition(-80, (theEqualizer - 350), 0);
				roamPointFirstCheckBox.SetSize(80, 25);
				roamPointFirstCheckBox.SetText("Roam Point 1st");
				roamPointFirstCheckBox.SetSelected(true);

				setExitAreaButton = Factories.CreateGuiButton(); 
				primaryPanel.Add(setExitAreaButton);
				setExitAreaButton.SetPosition(70, (theEqualizer - 250), 0);
				setExitAreaButton.SetSize(125, 25);
				setExitAreaButton.SetText("Set Exit Loc.");
				setExitAreaButton.AddActionListener((e) =>
				{
					var local = Players.LocalPlayer;
					if (local != null)
					{
						var loc = local.Location;
						var area = loc.Expand(4, 2, 4);
						Logging.Log("Set exit loc to " + loc.X + " " + loc.Y + " " + loc.Z);
						config.WOName = Game.ClusterName;
						config.ExitDest = new SafeVector3(new Vector3f(loc.X, loc.Y, loc.Z));
						config.ExitArea = new SafeMapArea(Game.Cluster.Name, new Area(area.Start, area.End));
					}
				});

				setRepairWayPointOneButton = Factories.CreateGuiButton();
				primaryPanel.Add(setRepairWayPointOneButton);
				setRepairWayPointOneButton.SetPosition(-70, (theEqualizer - 275), 0);
				setRepairWayPointOneButton.SetSize(125, 25);
				setRepairWayPointOneButton.SetText("Set RWP-1 Loc.");
				setRepairWayPointOneButton.AddActionListener((e) =>
				{
					var local = Players.LocalPlayer;
					if (local != null)
					{
						var loc = local.Location;
						var area = loc.Expand(4, 2, 4);
						Logging.Log("Set exit loc to " + loc.X + " " + loc.Y + " " + loc.Z);
						config.RepairClusterName = Game.ClusterName;
						config.RepairWayPointOneDest = new SafeVector3(new Vector3f(loc.X, loc.Y, loc.Z));
						config.RepairWayPointOneArea = new SafeMapArea(Game.Cluster.Name, new Area(area.Start, area.End));
					}
				});

				setRepairWayPointTwoButton = Factories.CreateGuiButton();
				primaryPanel.Add(setRepairWayPointTwoButton);
				setRepairWayPointTwoButton.SetPosition(70, (theEqualizer - 275), 0);
				setRepairWayPointTwoButton.SetSize(125, 25);
				setRepairWayPointTwoButton.SetText("Set RWP-2 Loc.");
				setRepairWayPointTwoButton.AddActionListener((e) =>
				{
					var local = Players.LocalPlayer;
					if (local != null)
					{
						var loc = local.Location;
						var area = loc.Expand(4, 2, 4);
						Logging.Log("Set exit loc to " + loc.X + " " + loc.Y + " " + loc.Z);
						config.RepairClusterName = Game.ClusterName;
						config.RepairWayPointTwoDest = new SafeVector3(new Vector3f(loc.X, loc.Y, loc.Z));
						config.RepairWayPointTwoArea = new SafeMapArea(Game.Cluster.Name, new Area(area.Start, area.End));
					}
				});

				setRepairWayPointThreeButton = Factories.CreateGuiButton();
				primaryPanel.Add(setRepairWayPointThreeButton);
				setRepairWayPointThreeButton.SetPosition(-70, (theEqualizer - 300), 0);
				setRepairWayPointThreeButton.SetSize(125, 25);
				setRepairWayPointThreeButton.SetText("Set RWP-3 Loc.");
				setRepairWayPointThreeButton.AddActionListener((e) =>
				{
					var local = Players.LocalPlayer;
					if (local != null)
					{
						var loc = local.Location;
						var area = loc.Expand(4, 2, 4);
						Logging.Log("Set exit loc to " + loc.X + " " + loc.Y + " " + loc.Z);
						config.RepairClusterName = Game.ClusterName;
						config.RepairWayPointThreeDest = new SafeVector3(new Vector3f(loc.X, loc.Y, loc.Z));
						config.RepairWayPointThreeArea = new SafeMapArea(Game.Cluster.Name, new Area(area.Start, area.End));
					}
				});

				enableRepairWayPointsCheckBox = Factories.CreateGuiCheckBox();
				primaryPanel.Add(enableRepairWayPointsCheckBox);
				enableRepairWayPointsCheckBox.SetPosition(-80, (theEqualizer - 375), 0);
				enableRepairWayPointsCheckBox.SetSize(80, 25);
				enableRepairWayPointsCheckBox.SetText("3-Step Repair");
				enableRepairWayPointsCheckBox.SetSelected(false);

                /*
				twoZoneCrossingCheckBox = Factories.CreateGuiCheckBox();
				primaryPanel.Add(twoZoneCrossingCheckBox);
				twoZoneCrossingCheckBox.SetPosition(-70, (theEqualizer - 425), 0);
				twoZoneCrossingCheckBox.SetSize(100, 25);
				twoZoneCrossingCheckBox.SetText("Enable 2-Zone Crossing");
				twoZoneCrossingCheckBox.SetSelected(false);
				*/
                
                // wtf123 Extras
                
                addMobCampButton = Factories.CreateGuiButton();
                primaryPanel.Add(addMobCampButton);
                addMobCampButton.SetPosition(-70, (theEqualizer - 400), 0);
                addMobCampButton.SetSize(125, 25);
                addMobCampButton.SetText("Add Mobcamp");
                addMobCampButton.AddActionListener((e) =>
                {
                    var local = Players.LocalPlayer;
                    if (local != null)
                    {
                        var loc = local.Location;
                        Logging.Log("Add mobcamp point " + loc.X + " " + loc.Y + " " + loc.Z);
                        config.ResourceClusterName = Game.ClusterName;
                        config.mobCamps.Add(new SafeVector3(new Vector3f(loc.X, loc.Y, loc.Z)));
                    }
                });


                remMobCampButton = Factories.CreateGuiButton();
                primaryPanel.Add(remMobCampButton);
                remMobCampButton.SetPosition(70, (theEqualizer - 400), 0);
                remMobCampButton.SetSize(125, 25);
                remMobCampButton.SetText("Del Mobcamp");
                remMobCampButton.AddActionListener((e) =>
                {
                    var local = Players.LocalPlayer;
                    if (local != null)
                    {
                        var loc = local.Location;
                        Logging.Log("Delete mobcamp " + loc.X + " " + loc.Y + " " + loc.Z);
                        for (var i = 0; i < config.mobCamps.Count; i++)
                        {
                            if (config.mobCamps[i].RealVector3().Expand(25, 3, 25).Contains(loc))
                            {
                                config.mobCamps.RemoveAt(i);
                                i -= 1;
                            }
                        }
                    }
                });

                remAllMobCampButton = Factories.CreateGuiButton();
                primaryPanel.Add(remAllMobCampButton);
                remAllMobCampButton.SetPosition(0, (theEqualizer - 425), 0);
                remAllMobCampButton.SetSize(200, 25);
                remAllMobCampButton.SetText("Del All Mobcamps");
                remAllMobCampButton.AddActionListener((e) =>
                {
                    Logging.Log("Delete all mobcamps ");
                    config.mobCamps.Clear();
                });

                debugInfoCheckBox = Factories.CreateGuiCheckBox();
                primaryPanel.Add(debugInfoCheckBox);
                debugInfoCheckBox.SetPosition(-70, (theEqualizer - 500), 0);
                debugInfoCheckBox.SetSize(80, 25);
                debugInfoCheckBox.SetText("Debug Info");
                debugInfoCheckBox.SetSelected(false);

                blacklistCheckBox = Factories.CreateGuiCheckBox();
                primaryPanel.Add(blacklistCheckBox);
                blacklistCheckBox.SetPosition(70, (theEqualizer - 325), 0);
                blacklistCheckBox.SetSize(100, 25);
                blacklistCheckBox.SetText("Blacklist Nodes?");
                blacklistCheckBox.SetSelected(false);

                ignorMobCampNodes = Factories.CreateGuiCheckBox();
                primaryPanel.Add(ignorMobCampNodes);
                ignorMobCampNodes.SetPosition(70, (theEqualizer - 350), 0);
                ignorMobCampNodes.SetSize(100, 25);
                ignorMobCampNodes.SetText("ignore MCN?");
                ignorMobCampNodes.SetSelected(false);

                UpdateForConfig();
            });

            return true;
        }

        public override int OnLoop(IScriptEngine se)
        {
            var lpo = Players.LocalPlayer;
            if (lpo != null && config.MaxHoldWeight < lpo.MaxCarryWeight)
                config.MaxHoldWeight = lpo.MaxCarryWeight;
            return 100;
        }
    }
}
