using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using R2API.Utils;
using RoR2;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

namespace DropInMultiplayer
{
    [BepInPlugin(guid, modName, version)]
    [NetworkCompatibility(CompatibilityLevel.NoNeedForSync, VersionStrictness.DifferentModVersionsAreOk)]
    [BepInDependency("com.bepis.r2api", BepInDependency.DependencyFlags.HardDependency)]
    public class DropInMultiplayer : BaseUnityPlugin
    {
        #region BepinPlugin
        const string guid = "com.AwokeinanEngima.DropInMultiplayer";
        const string modName = "Drop In Multiplayer";
        const string version = "1.0.0";
        #endregion
        #region Events
        public static event Action awake;

        //Usually you won't need this, but it's good to have it anyways.
        public static event Action start;
        #endregion
        #region Config
        private static ConfigEntry<bool> ImmediateSpawn { get; set; }
        private static ConfigEntry<bool> NormalSurvivorsOnly { get; set; }
        private static ConfigEntry<bool> AllowSpawnAsWhileAlive { get; set; }
        private static ConfigEntry<bool> StartWithItems { get; set; }
        public static ConfigEntry<bool> SpawnAsEnabled { get; set; }
        public static ConfigEntry<bool> HostOnlySpawnAs { get; set; }
        public static ConfigEntry<bool> GiveLunarItems { get; set; }
        public static ConfigEntry<bool> GiveRedItems { get; set; }
        public static ConfigEntry<bool> WelcomeMessage { get; set; }
        public static ConfigEntry<bool> GiveExactItems { get; set; }
        #endregion
        #region Instance and logger
        public static DropInMultiplayer instance;
        public static ManualLogSource logger;
        #endregion
        #region Lists
        private List<string> survivorList;
        private List<List<string>> bodies = new List<List<string>> {
            new List<string> { "AssassinBody", "Assassin"},
            //Survivors
            new List<string> { "CommandoBody", "Commando"},
            new List<string> { "HuntressBody", "Huntress"},
            new List<string> { "EngiBody", "Engi", "Engineer"},
            new List<string> { "ToolbotBody", "Toolbot", "MULT", "MUL-T"},
            new List<string> { "MercBody", "Merc", "Mercenary"},
            new List<string> { "MageBody", "Mage", "Artificer", "Arti"},
            new List<string> { "TreebotBody", "Support", "Rex"},
            new List<string> { "LoaderBody", "Loader", "Loadie"},
            new List<string> { "CrocoBody", "Croco", "Acrid"},
            new List<string> { "EnforcerBody", "Enforcer"},
            new List<string> { "CaptainBody", "Captain", "Cap"},
            new List<string> { "PaladinBody", "Paladin"},
            //drones
            new List<string> { "BackupDroneBody", "BackupDrone"},
            new List<string> { "BackupDroneOldBody", "BackupDroneOld"},
            new List<string> { "Drone1Body", "GunnerDrone", "Gunner"},
            new List<string> { "Drone2Body", "HealingDrone", "Healer"},
            new List<string> { "FlameDroneBody", "FlameDrone"},
            new List<string> { "MegaDroneBody", "TC280Prototype"},
            new List<string> { "MissileDroneBody", "MissileDrone"},
            new List<string> { "EquipmentDroneBody", "EquipmentDrone", "EquipDrone"},
            new List<string> { "EmergencyDroneBody", "EmergencyDrone", "EmDrone"},
            new List<string> { "Turret1Body", "GunnerTurret"},
            //everything else
            new List<string> { "AltarSkeletonBody", "AltarSkeleton"},
            new List<string> { "AncientWispBody", "AncientWisp"},
            new List<string> { "ArchWispBody", "ArchWisp"},
            new List<string> { "GravekeeperBody", "Gravekeeper"},
            new List<string> { "GreaterWispBody", "GreaterWisp"},
            //beetles
            new List<string> { "BeetleBody", "Beetle"},
            new List<string> { "BeetleGuardAllyBody", "BeetleGuardAlly"},
            new List<string> { "BeetleGuardBody", "BeetleGuard"},
            new List<string> { "BeetleQueen2Body", "BeetleQueen"},
            //Clay
            new List<string> { "ClayBody", "Clay", "ClayMan"},
            new List<string> { "ClayBossBody", "ClayBoss", "Dunestrider", "ClayDunestrider"},
            new List<string> { "ClayBruiserBody", "ClayBruiser", "ClayTemplar"},
            new List<string> { "Pot2Body", "Pot2"},
            //Imps
            new List<string> { "BellBody", "Bell"},
            new List<string> { "BirdsharkBody", "Birdshark"},
            new List<string> { "BisonBody", "Bison"},
            new List<string> { "BomberBody", "Bomber"},
            //Worms
            new List<string> { "ElectricWormBody", "ElectricWorm", "OverloadingWorm", "TheReminder"},
            new List<string> { "MagmaWormBody", "MagmaWorm", "MagmaWorm"},
            //Engineer Turrets
            new List<string> { "EngiBeamTurretBody", "EngiBeamTurret"},
            new List<string> { "EngiTurretBody", "EngiTurret"},
            //Golems
            new List<string> { "GolemBody", "Golem"},
            new List<string> { "GolemBodyInvincible", "GolemInvincible"},
            //weird shit
            new List<string> { "CommandoPerformanceTestBody", "CommandoPerformanceTest"},
            //Destructibles
            new List<string> { "ExplosivePotDestructibleBody", "ExplosivePotDestructible"},
            new List<string> { "FusionCellDestructibleBody", "FusionCellDestructible"},
            new List<string> { "TimeCrystalBody", "TimeCrystal"},
            //Cars??
            new List<string> { "PotMobile2Body", "PotMobile2"},
            new List<string> { "PotMobileBody", "PotMobile"},
            new List<string> { "HaulerBody", "Hauler"},
            //Crabs
            new List<string> { "HermitCrabBody", "HermitCrab"},
            //Imps
            new List<string> { "ImpBody", "Imp"},
            new List<string> { "ImpBossBody", "ImpBoss"},
            //Jellyfish
            new List<string> { "JellyfishBody", "Jellyfish"},
            new List<string> { "VagrantBody", "Vagrant"},
            //Lemurians
            new List<string> { "LemurianBody", "Lemurian"},
            new List<string> { "LemurianBruiserBody", "LemurianBruiser"},
            //Siren Call Entities
            new List<string> { "VultureBody", "Vulture" },
            new List<string> { "SuperRoboBallBossBody", "AWU" },
            new List<string> { "RoboBallMiniBody", "Solus", "Probe"},
            //npcs
            new List<string> { "SquidTurretBody", "SquidTurret"},
            new List<string> { "ShopkeeperBody", "Shopkeeper"},
            //Spectators
            new List<string> { "SpectatorBody", "Spectator"},
            new List<string> { "SpectatorSlowBody", "SpectatorSlow"},
            //Titans
            new List<string> { "TitanBody", "Titan"},
            new List<string> { "TitanGoldBody", "TitanGold"},
            //Thing??
            new List<string> { "UrchinTurretBody", "UrchinTurret"},
            //Scav
            new List<string> { "ScavBody", "Scav", "Scavenger"},
            new List<string> { "ScavLunar1Body", "Kipkip", "Gentle"},
            new List<string> { "ScavLunar2Body", "Wipwip", "Wild"},
            new List<string> { "ScavLunar3Body", "Twiptwip", "Devotee"},
            new List<string> { "ScavLunar4Body", "Guragura", "Lucky"},
            //thing 2
            new List<string> { "NullifierBody", "VoidReaver", "Reaver", "Void"},
            //Lunar LunarGolemBody 
            new List<string> { "BrotherBody", "Mithrix", "BigBro", "Bro"},
            new List<string> { "LunarGolemBody", "MinigunnerLunarChimera", "LunarGolem"},
            new List<string> { "LunarWispBody", "WispLunarChimera", "LunarWisp"},

        };
        #endregion
        #region Methods
        private ItemIndex GetRandomItem(List<ItemIndex> items)
        {
            int itemID = UnityEngine.Random.Range(0, items.Count);

            return items[itemID];
        }
        public string GetValue(List<string> args, int index)
        {
            if (index < args.Count && index >= 0)
            {
                return args[index];
            }
            return "";
        }
        #endregion
        #region Mod
        //This is your constructor, use it wisely.
        DropInMultiplayer()
        {
            //Subscribe to our awake event.
            awake += Load;
            start += FirstFrame;
        }
        private void Load() {
            if (instance == null || logger == null)
            {
                instance = this;
                logger = base.Logger;
            }
            DropIn();
            LogM("Drop-In Multiplayer Loaded!");
#if DEBUG
            LogW("You're on a debug build. If you see this after downloading from the thunderstore, panic!");
            On.RoR2.Networking.GameNetworkManager.OnClientConnect += (self, user, t) => { };
#endif
        }
        private void DropIn() {
            DefineConfig();
            Hook();
        }
        private void DefineConfig() {
            ImmediateSpawn = Config.Bind("Enable/Disable", "ImmediateSpawn", false, "Enables or disables immediate spawning as you join");
            NormalSurvivorsOnly = Config.Bind("Enable/Disable", "NormalSurvivorsOnly", true, "Changes whether or not join_as can only be used to turn into survivors");
            StartWithItems = Config.Bind("Enable/Disable", "StartWithItems", true, "Enables or disables giving players items if they join mid-game");
            AllowSpawnAsWhileAlive = Config.Bind("Enable/Disable", "AllowJoinAsWhileAlive", false, "Enables or disables players using join_as while alive");
            SpawnAsEnabled = Config.Bind("Enable/Disable", "Join_As", true, "Enables or disables the join_as command");
            HostOnlySpawnAs = Config.Bind("Enable/Disable", "HostOnlyJoin_As", false, "Changes the join_as command to be host only");
            GiveLunarItems = Config.Bind("Enable/Disable", "GiveLunarItems", false, "Allows lunar items to be given to players, needs StartWithItems to be enabled!");
            GiveRedItems = Config.Bind("Enable/Disable", "GiveRedItems", true, "Allows red items to be given to players, needs StartWithItems to be enabled!");
            //AllowUnusedSurvivors = Config.Bind("Enable/Disable", "AllowUnusedSurvivors", true, "Allows people to spawn as unused characters, such as bandit and HAN-D.");
            WelcomeMessage = Config.Bind("Enable/Disable", "WelcomeMessage", true, "Sends the welcome message when a new player joins.");
            GiveExactItems = Config.Bind("Enable/Disable", "GiveExactItems", false, "Chooses a random member in the game and gives the new player their items, should be used with ShareSuite, needs StartWithItems to be enabled, and also disables GiveRedItems, GiveLunarItems!");
        }

        private void Hook() {
            On.RoR2.Run.SetupUserCharacterMaster += GiveItems;
            On.RoR2.Console.RunCmd += Console_RunCmd; 
            On.RoR2.NetworkUser.Start += GreetNewPlayer;
        }

        private void Console_RunCmd(On.RoR2.Console.orig_RunCmd orig, RoR2.Console self, RoR2.Console.CmdSender sender, string concommandName, List<string> userArgs)
        {
            orig(self, sender, concommandName, userArgs);

            if (concommandName.Equals("say", StringComparison.OrdinalIgnoreCase))
            {
                var userMsg = ArgsHelper.GetValue(userArgs, 0).ToLower();
                var isRequest = userMsg.StartsWith("join_as");
                if (isRequest)
                {
                    var argsRequest = userMsg.Split(' ').ToList();
                    string bodyString = ArgsHelper.GetValue(argsRequest, 1);
                    string userString = ArgsHelper.GetValue(argsRequest, 2);

                    JoinAs(sender.networkUser, bodyString, userString);
                }
            }
        }

        private void GreetNewPlayer(On.RoR2.NetworkUser.orig_Start orig, NetworkUser self)
        {
            orig(self);
            if (NetworkServer.active && Stage.instance != null) {
                if (WelcomeMessage.Value) {
                    AddChatMessage("Hello " + self.userName + "! Join the game by typing 'join_as [name]' (without the apostrophes of course) into the chat. Available survivors are Acrid, Artificer, Commando, Captain, Engineer, Huntress, Loader, Mercancy, MULT, and Rex!", 1f);
                }
            }
        }

        private void GiveItems(On.RoR2.Run.orig_SetupUserCharacterMaster orig, Run self, NetworkUser user)
        {
            orig(self, user);
        }

        private void FirstFrame() {
            PopulateSurvivorList();
        }

        private void PopulateSurvivorList() {
            survivorList = new List<string>();
            foreach (SurvivorDef def in SurvivorCatalog.allSurvivorDefs) {
                if (def.bodyPrefab) { 
                survivorList.Add(def.bodyPrefab.name);
                }
            }
        }
        #endregion
        #region Methods
        private void JoinAs(NetworkUser user, string bodyString, string userString) {
            if (!SpawnAsEnabled.Value) {
                LogW("JoinAs :: SpawnAsEnabled.Value disabled. Returning...");
                return;
            }
            if (HostOnlySpawnAs.Value) {
                if (NetworkUser.readOnlyInstancesList[0].netId != user.netId) {
                    LogW("JoinAs :: HostOnlySpawnAs.Value enabled and the person using join_as isn't host. Returning!");
                    return;
                }
            }

            //Finding the NetworkUser from the person who is using the command.
            NetworkUser player = GetNetUserFromString(userString);

            //https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/operators/null-coalescing-operator
            player = player ?? user;
            CharacterMaster characterMaster = player.master;

            //Finding the body the player wants to spawn as.
            bodyString = GetBodyNameFromString(bodyString);
            GameObject bodyPrefab = BodyCatalog.FindBodyPrefab(bodyString);

            #region Body Checks
            //These are just to ensure that a null reference isn't thrown when we try to spawn our new player.
            if (!bodyPrefab) {
                AddChatMessage("Couldn't find " + bodyString + ", " + player.userName + ". Options for join_as are Acrid, Artificer, Commando, Captain, Engineer, Huntress, Loader, Mercancy, MULT, and Rex!");
                LogW("JoinAs :: Sent message to player informing them that what they requested to join as does not exist. Also bodyPrefab does not exist, returning!");
                return;
            }

            if (NormalSurvivorsOnly.Value && !survivorList.Contains(bodyString)) {

                AddChatMessage("You can only spawn as normal survivors");
                LogW("JoinAs :: NormalSurvivorsOnly.Value is enabled and the object the player attempting to spawn is not a normal survivor. Sent a chat message informing them of this. Returning...");
                return;
            }
            #endregion


            //If the characterMaster exists.
            if (characterMaster)
            {
                //If the characterMaster is alive.
                bool hasBody = characterMaster.hasBody;

                if (hasBody)
                {
                    //If the characterMaster is alive, do stuff!
                    #region hasBody logic
                    if (AllowSpawnAsWhileAlive.Value)
                    {
                        characterMaster.bodyPrefab = bodyPrefab;
                        characterMaster.Respawn(characterMaster.GetBody().transform.position, characterMaster.GetBody().transform.rotation);
                        AddChatMessage(player.userName + " is respawning as " + Language.GetString(bodyPrefab.GetComponent<CharacterBody>().baseNameToken) + "!");
                    }
                    else if (!hasBody)
                    {
                        AddChatMessage("Sorry " + player.userName + "! You can't use join_as while dead.");
                    }
                    else if (!AllowSpawnAsWhileAlive.Value && hasBody)
                    {
                        AddChatMessage("Sorry " + player.userName + "! The host has made it so you can't use join_as while alive.");
                    }
                    #endregion
                }
            }
            else
            {   //Else, it doesn't have a characterMaster
                Run.instance.SetFieldValue("allowNewParticipants", true);
                Run.instance.OnUserAdded(user);

                var backup = user.master;

                backup.bodyPrefab = bodyPrefab;
                
                //Offset for players.
                Transform spawnTransform = Stage.instance.GetPlayerSpawnTransform();
                Vector3 posOffset = new Vector3(0, 3, 0);

                CharacterBody body = backup.SpawnBody(bodyPrefab, spawnTransform.position + posOffset, spawnTransform.rotation);
                Run.instance.HandlePlayerFirstEntryAnimation(body, spawnTransform.position + posOffset, spawnTransform.rotation);

                //Inform the chat that [Player] is spawning 
                AddChatMessage(player.userName + " is spawning as " + bodyString + "!");

                if (!ImmediateSpawn.Value)
                {
                    Run.instance.SetFieldValue("allowNewParticipants", false);
                }
            }

        }
        #region Helpers
        public string GetBodyNameFromString(string name) {
            foreach (var bodyLists in bodies) {
                foreach (var tempName in bodyLists) {
                    if (tempName.Equals(name, StringComparison.OrdinalIgnoreCase)) {
                        return bodyLists[0];
                    }
                }
            }
            return name;
        }
        private NetworkUser GetNetUserFromString(string playerString)
        {
            int result = 0;
            if (playerString != "")
            {
                if (int.TryParse(playerString, out result))
                {
                    if (result < NetworkUser.readOnlyInstancesList.Count && result >= 0)
                    {

                        return NetworkUser.readOnlyInstancesList[result];
                    }
                    LogE("Specified player index does not exist");
                    return null;
                }
                else
                {
                    foreach (NetworkUser n in NetworkUser.readOnlyInstancesList)
                    {
                        if (n.userName.Equals(playerString, StringComparison.CurrentCultureIgnoreCase))
                        {
                            return n;
                        }
                    }
                    return null;
                }
            }
            return null;
        }
        #endregion
        #region Chat Messages
        private void AddChatMessage(string message, float time = 0.1f) {
            instance.StartCoroutine(AddHelperMessage(message, time));
        }
        private IEnumerator AddHelperMessage(string message, float time) {
            yield return new WaitForSeconds(time);
            var chatMessage = new Chat.SimpleChatMessage { baseToken = message };
            Chat.SendBroadcastChat(chatMessage);
        }
        #endregion
        #endregion
        #region Basic
        private void Awake()
        {
            Action awake = DropInMultiplayer.awake;
            if (awake == null)
            {
                return;
            }
            awake();
        }
        private void Start()
        {
            //Only use this if you need something that's created after RoR2's catalogs are loaded.
            Action start = DropInMultiplayer.start;
            if (start == null)
            {
                return;
            }
            start();
        }
        #endregion
        #region Logging
        public static void LogF(object data) => logger.LogFatal(data);
        public static void LogE(object data) => logger.LogError(data);
        public static void LogW(object data) => logger.LogWarning(data);
        public static void LogM(object data) => logger.LogMessage(data);
        public static void LogI(object data) => logger.LogInfo(data);
        public static void LogD(object data) => logger.LogDebug(data);
        #endregion
    }
    public class ArgsHelper
    {
        public static string GetValue(List<string> args, int index)
        {
            if (index < args.Count && index >= 0)
            {
                return args[index];
            }

            return "";
        }
    }
}