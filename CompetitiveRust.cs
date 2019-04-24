using System;
using System.Collections.Generic;
using System.Linq;

namespace Oxide.Plugins {
    [Info("CompetitiveRust", "br0wnard", "0.1")]
    [Description("Setup configuration and rules for competitive play in Rust.")]
    public class CompetitiveRust : RustPlugin {

        #region Configuration Data

        private bool configChanged;

        // Plugin settings
        private const string DefaultChatPrefix = "[CompetitiveRust]";
        private const string DefaultChatPrefixColor = "#ff8d00ff";

        public string ChatPrefix { get; private set; }
        public string ChatPrefixColor { get; private set; }

        // Plugin options
        private const int DefaultPreparationTime = 70;
        private const int DefaultBlueHoodie = 887162672;
        private const int DefaultRedHoodie = 887173152;

        public int PreparationTime { get; private set; }
        public int BlueHoodie { get; private set; }
        public int RedHoodie { get; private set; }

        // Plugin messages
        private const string DefaultTimeLeft = "{0} seconds left for the preparation phase.";
        private const string DefaultTimeUp = "The preparation is finished.";
        private const string DefaultNoPermission = "You don't have permission to use this command.";
        private const string DefaultEmptyTeam = "Can't start the game, one team is empty.";
        private const string DefaultStarted = "The game is already started.";
        private const string DefaultNoTeam = "Please enter a team name between '{0}' and '{1}'";
        private const string DefaultCantChangeTeam = "You can't change your team when the game is started.";
        private const string DefaultNow = "{0} is now in the {1} team.";
        private const string DefaultCantStop = "You can't stop the game, because it's not started yet.";
        private const string DefaultStop = "The game is now stopped. Teams have been cleared.";
        private const string DefaultStartCup = "You can't place a cupboard until the start of the game.";
        private const string DefaultCupLimit = "Your team already has a cupboard placed.";
        private const string DefaultCupPlaced = "You placed your team cupboard.";
        private const string DefaultGameStart = "Game is started. You have {0} seconds left for the preparation phase, good luck.";
        private const string DefaultWin = "{0} team cupboard is down. {1} team won !";
        private const string DefaultReady = "{0} is now ready.";
        private const string DefaultUnready = "{0} is now unready.";
        private const string DefaultChoose = "Please choose a team before you get ready.";
        private const string DefaultRemaining = "{0} seconds remaining before the end of the preparation phase.";
        private const string DefaultRedLower = "<color=#ed3434ff>red</color>";
        private const string DefaultBlueLower = "<color=#1340d6ff>blue</color>";
        private const string DefaultRedUpper = "<color=#ed3434ff>Red</color>";
        private const string DefaultBlueUpper = "<color=#1340d6ff>Blue</color>";

        public string CurrentTimeLeft { get; private set; }
        public string CurrentTimeUp { get; private set; }
        public string CurrentNoPermission { get; private set; }
        public string CurrentEmptyTeam { get; private set; }
        public string CurrentStarted { get; private set; }
        public string CurrentNoTeam { get; private set; }
        public string CurrentCantChangeTeam { get; private set; }
        public string CurrentNow { get; private set; }
        public string CurrentCantStop { get; private set; }
        public string CurrentStop { get; private set; }
        public string CurrentStartCup { get; private set; }
        public string CurrentCupLimit { get; private set; }
        public string CurrentCupPlaced { get; private set; }
        public string CurrentGameStart { get; private set; }
        public string CurrentWin { get; private set; }
        public string CurrentReady { get; private set; }
        public string CurrentUnready { get; private set; }
        public string CurrentChoose { get; private set; }
        public string CurrentRemaining { get; private set; }
        public string CurrentRedLower { get; private set; }
        public string CurrentBlueLower { get; private set; }
        public string CurrentRedUpper { get; private set; }
        public string CurrentBlueUpper { get; private set; }

        #endregion

        private bool DefaultPreparationUp = false;
        private bool DefaultGameStarted = false;
        private bool DefaultCupBoardRed = false;
        private bool DefaultCupBoardBlue = false;
        private string DefaultCupBoardRedString = "";
        private string DefaultCupBoardBlueString = "";
        
        public bool PreparationUp { get; private set; }
        public bool GameStarted { get; private set; }
        public bool CupBoardRed { get; private set; }
        public bool CupBoardBlue { get; private set; }
        public string CupBoardRedString { get; private set; }
        public string CupBoardBlueString { get; private set; }
        

        public List<string> UnreadyList { get; private set; }
        public List<string> RedTeam { get; private set; }
        public List<string> BlueTeam { get; private set; }
        public List<string> RedReady { get; private set; }
        public List<string> BlueReady { get; private set; }

        public Timer TimeLeft;
        public Timer message;

        int AlternativeMessage;

        public Random rnd;

        private void Loaded() => LoadConfigValues();

        private void OnServerInitialized()
        {
            // List initialization
            UnreadyList = new List<string>();
            RedTeam = new List<string>();
            BlueTeam = new List<string>();
            RedReady = new List<string>();
            BlueReady = new List<string>();

            rnd = new Random();
            AlternativeMessage = 0;

            message = timer.Repeat(15, -1, () =>
            {
                ++AlternativeMessage;
                if (AlternativeMessage % 2 == 0)
                {
                    BasePlayer.activePlayerList.ForEach(x => SendChatMessage(x, "Please pick a team with /join and get ready with /ready."));
                }
                else
                {
                    BasePlayer.activePlayerList.ForEach(x => AddToUnreadyList(x));
                    BasePlayer.activePlayerList.ForEach(x => SendChatMessage(x, "Players unready : {0}", string.Join(",", UnreadyList)));
                    UnreadyList.Clear();
                }

            }
            );
        }

        private void Unload()
        {
        }

        protected override void LoadDefaultConfig() => PrintWarning("Configuration file has been created.");

        private void LoadConfigValues()
        {
            // Plugin settings
            ChatPrefix = GetConfigValue("Settings", "ChatPrefix", DefaultChatPrefix);
            ChatPrefixColor = GetConfigValue("Settings", "ChatPrefixColor", DefaultChatPrefixColor);

            // Plugin options
            PreparationTime = GetConfigValue("Options", "PreparationTime", DefaultPreparationTime);
            BlueHoodie = GetConfigValue("Options", "BlueHoodie", DefaultBlueHoodie);
            RedHoodie = GetConfigValue("Options", "RedHoodie", DefaultRedHoodie);

            // Plugin messages
            CurrentTimeLeft = GetConfigValue("Messages", "CurrentTimeLeft", DefaultTimeLeft);
            CurrentTimeUp = GetConfigValue("Messages", "CurrentTimeUp", DefaultTimeUp);
            CurrentNoPermission = GetConfigValue("Messages", "CurrentNoPermission", DefaultNoPermission);
            CurrentEmptyTeam = GetConfigValue("Messages", "CurrentEmptyTeam", DefaultEmptyTeam);
            CurrentStarted = GetConfigValue("Messages", "CurrentStarted", DefaultStarted);
            CurrentNoTeam = GetConfigValue("Messages", "CurrentNoTeam", DefaultNoTeam);
            CurrentCantChangeTeam = GetConfigValue("Messages", "CurrentCantChangeTeam", DefaultCantChangeTeam);
            CurrentNow = GetConfigValue("Messages", "CurrentNow", DefaultNow);
            CurrentCantStop = GetConfigValue("Messages", "CurrentCantStop", DefaultCantStop);
            CurrentStop = GetConfigValue("Messages", "CurrentStop", DefaultStop);
            CurrentStartCup = GetConfigValue("Messages", "CurrentStartCup", DefaultStartCup);
            CurrentCupLimit = GetConfigValue("Messages", "CurrentCupLimit", DefaultCupLimit);
            CurrentCupPlaced = GetConfigValue("Messages", "CurrentCupPlaced", DefaultCupPlaced);
            CurrentGameStart = GetConfigValue("Messages", "CurrentGameStart", DefaultGameStart);
            CurrentWin = GetConfigValue("Messages", "CurrentWin", DefaultWin);
            CurrentReady = GetConfigValue("Messages", "CurrentReady", DefaultReady);
            CurrentUnready = GetConfigValue("Messages", "CurrentUnready", DefaultUnready);
            CurrentChoose = GetConfigValue("Messages", "CurrentChoose", DefaultChoose);
            CurrentRemaining = GetConfigValue("Messages", "CurrentRemaining", DefaultRemaining);
            CurrentRedLower = GetConfigValue("Messages", "CurrentRedLower", DefaultRedLower);
            CurrentBlueLower = GetConfigValue("Messages", "CurrentBlueLower", DefaultBlueLower);
            CurrentRedUpper = GetConfigValue("Messages", "CurrentRedUpper", DefaultRedUpper);
            CurrentBlueUpper = GetConfigValue("Messages", "CurrentBlueUpper", DefaultBlueUpper);
        
            if (!configChanged){ return;}
            Puts("Configuration file updated.");
            SaveConfig();
        }

        #region Chat/Console command.

        [ChatCommand("timeleft")]
        private void TimeCommandChat(BasePlayer player, string command, string[] args)
        {
            SendChatMessage(player, CurrentTimeLeft, PreparationTime);
        }

        [ChatCommand("start")]
        private void StartCommandChat(BasePlayer player, string command, string[] args)
        {
            if (!player.IsAdmin)
            {
                SendChatMessage(player, CurrentNoPermission);
                return;
            }
            if (GameStarted)
            {
                SendChatMessage(player, CurrentStarted);
                return;
            }
            if (!RedTeam.Any() && !BlueTeam.Any())
            {
                SendChatMessage(player, CurrentEmptyTeam);
                return;
            }
            BeginGame();
        }

        [ChatCommand("join")]
        private void JoinCommandChat(BasePlayer player, string command, string[] args)
        {
            if (GameStarted)
            {
                SendChatMessage(player, CurrentCantChangeTeam);
                return;
            }
            if (args.Length != 1)
            {
                SendChatMessage(player, CurrentNoTeam, CurrentRedLower, CurrentBlueLower);
                return;
            }
            if (args[0] == "red")
            {
                JoinRed(player);
                return;
            }
            if (args[0] == "blue")
            {
                JoinBlue(player);
                return;
            }
            SendChatMessage(player, CurrentNoTeam, CurrentRedLower, CurrentBlueLower);
            return;
        }

        [ChatCommand("stop")]
        private void StopCommandChat(BasePlayer player, string command, string[] args)
        {
            if (!player.IsAdmin)
            {
                SendChatMessage(player, CurrentNoPermission);
                return;
            }
            if (!GameStarted) {
                SendChatMessage(player, CurrentCantStop);
                return;
            }
            GameStarted = false;
            ClearGame();
            BasePlayer.activePlayerList.ForEach(x => SendChatMessage(x, CurrentStop));
            return;
        }

        [ChatCommand("ready")]
        private void ReadyCommandChat(BasePlayer player, string command, string[] args)
        {
            if (GameStarted) { return; }
            if (RedTeam.Contains(player.UserIDString) && !RedReady.Contains(player.UserIDString))
            {
                RedReady.Add(player.UserIDString);
                BasePlayer.activePlayerList.ForEach(x => SendChatMessage(x, CurrentReady, player.displayName));
            }
            else if (BlueTeam.Contains(player.UserIDString) && !BlueReady.Contains(player.UserIDString))
            {
                BlueReady.Add(player.UserIDString);
                BasePlayer.activePlayerList.ForEach(x => SendChatMessage(x, CurrentReady, player.displayName));
            }
            else
            {
                SendChatMessage(player, CurrentChoose);
                return;
            }
            if (RedTeam.Count == RedReady.Count && BlueTeam.Count == BlueReady.Count)
            {
                BeginGame();
            }
        }

        [ChatCommand("unready")]
        private void UnreadyCommandChat(BasePlayer player, string command, string[] args)
        {
            if (GameStarted) { return; }
            if (RedReady.Contains(player.UserIDString))
            {
                RedReady.Remove(player.UserIDString);
                BasePlayer.activePlayerList.ForEach(x => SendChatMessage(x, CurrentUnready, player.displayName));
            }
            else if (BlueReady.Contains(player.UserIDString))
            {
                BlueReady.Remove(player.UserIDString);
                BasePlayer.activePlayerList.ForEach(x => SendChatMessage(x, CurrentUnready, player.displayName));
            }
        }

        [ChatCommand("random")]
        private void RandomCommandChat(BasePlayer player, string command, string[] args)
        {
            if (GameStarted) { return; }
            BasePlayer.activePlayerList.ForEach(x => JoinRand(x));
        }

        #endregion

        void OnEntityTakeDamage(BaseEntity entity, HitInfo info)
        {
            if (!GameStarted) { return; }
            if (PreparationUp && GameStarted) { return; }
            if (entity == null || info == null) { return; }
            BasePlayer target = entity as BasePlayer;
            if (info.Initiator == null) { return; }
            BasePlayer from = info.Initiator as BasePlayer;
            if (target == null || from == null) { return; }
            if (target.UserIDString == from.UserIDString) { return; }
            info.damageTypes.ScaleAll(0);
        }

        void OnEntityKill(BaseNetworkable entity)
        {
            if (entity.ShortPrefabName.Contains("cupboard.tool"))
            {
                if (entity.ToString() == CupBoardRedString)
                {
                    BasePlayer.activePlayerList.ForEach(x => SendChatMessage(x, CurrentWin, CurrentRedUpper, CurrentBlueUpper));
                    ClearGame();
                    return;
                }
                if (entity.ToString() == CupBoardBlueString)
                {
                    BasePlayer.activePlayerList.ForEach(x => SendChatMessage(x, CurrentWin, CurrentBlueUpper, CurrentRedUpper));
                    ClearGame();
                    return;
                }
            }
        }

        void OnEntitySpawned(BaseEntity entity, UnityEngine.GameObject gameObject)
        {
            if (entity.ShortPrefabName.Contains("cupboard.tool"))
            {
                BasePlayer player = BasePlayer.FindByID(entity.OwnerID);
                Console.WriteLine(entity.ToString());
                if (player == null) { return; }
                if (!GameStarted)
                {
                    entity.KillMessage();
                    var itemtogive = ItemManager.CreateByItemID(-97956382, 1);
                    if (itemtogive != null) player.inventory.GiveItem(itemtogive);
                    SendChatMessage(player, CurrentStartCup);
                    return;
                }
                if (RedTeam.Contains(player.UserIDString) && CupBoardRed)
                {
                    entity.KillMessage();
                    var itemtogive = ItemManager.CreateByItemID(-97956382, 1);
                    if (itemtogive != null) player.inventory.GiveItem(itemtogive);
                    SendChatMessage(player, CurrentCupLimit);
                    return;
                }
                if (BlueTeam.Contains(player.UserIDString) && CupBoardBlue)
                {
                    entity.KillMessage();
                    var itemtogive = ItemManager.CreateByItemID(-97956382, 1);
                    if (itemtogive != null) player.inventory.GiveItem(itemtogive);
                    SendChatMessage(player, CurrentCupLimit);
                    return;
                }
                if (RedTeam.Contains(player.UserIDString))
                {
                    CupBoardRed = true;
                    CupBoardRedString = entity.ToString();
                    SendChatMessage(player, CurrentCupPlaced);
                    return;
                }
                if (BlueTeam.Contains(player.UserIDString))
                {
                    CupBoardBlue = true;
                    CupBoardBlueString = entity.ToString();
                    SendChatMessage(player, CurrentCupPlaced);
                    return;
                }
            }
        }

        void OnPlayerRespawned(BasePlayer player)
        {
            if (RedTeam.Contains(player.UserIDString))
            {
                var i = ItemManager.CreateByItemID(1751045826, 1, (ulong)RedHoodie);
                if (i != null) { player.inventory.GiveItem(i, player.inventory.containerWear); }
                return;
            }
            if (BlueTeam.Contains(player.UserIDString))
            {
                var i = ItemManager.CreateByItemID(1751045826, 1, (ulong)BlueHoodie);
                if (i != null) { player.inventory.GiveItem(i, player.inventory.containerWear); }
                return;
            }
            return;
        }

        #region Helper methods

        private void ClearGame()
        {
            PreparationTime = DefaultPreparationTime;
            PreparationUp = DefaultPreparationUp;
            GameStarted = DefaultGameStarted;
            CupBoardRed = DefaultCupBoardRed;
            CupBoardBlue = DefaultCupBoardBlue;
            CupBoardRedString = DefaultCupBoardRedString;
            CupBoardBlueString = DefaultCupBoardBlueString;
            BlueHoodie = DefaultBlueHoodie;
            RedHoodie = DefaultRedHoodie;
            RedTeam.Clear();
            BlueTeam.Clear();
            RedReady.Clear();
            BlueReady.Clear();
            SetConfigValue("Options", "PreparationTime", DefaultPreparationTime);
            SetConfigValue("Options", "BlueHoodie", DefaultBlueHoodie);
            SetConfigValue("Options", "RedHoodie", DefaultRedHoodie);
            OnServerInitialized();
        }

        private void SendChatMessage(BasePlayer player, string message, params object[] args) => player?.SendConsoleCommand("chat.add", -1, string.Format($"<color={ChatPrefixColor}>{ChatPrefix}</color>: {message}", args), 1.0);

        T GetConfigValue<T>(string category, string setting, T defaultValue)
        {
            var data = Config[category] as Dictionary<string, object>;
            if (data == null)
            {
                data = new Dictionary<string, object>();
                Config[category] = data;
                configChanged = true;
            }
            object value;
            if (!data.TryGetValue(setting, out value))
            {
                value = defaultValue;
                data[setting] = value;
                configChanged = true;
            }
            return (T)Convert.ChangeType(value, typeof(T));
        }

        void SetConfigValue<T>(string category, string setting, T newValue)
        {
            var data = Config[category] as Dictionary<string, object>;
            object value;
            if (data != null && data.TryGetValue(setting, out value))
            {
                value = newValue;
                data[setting] = value;
                configChanged = true;
            }
            SaveConfig();
        }

        private void RemoveAll()
        {
            var allDropped = UnityEngine.GameObject.FindObjectsOfType<DroppedItem>();
            for (int i = 0; i < allDropped.Count(); i++)
            {
                var droppedItem = allDropped[i];
                if (droppedItem == null) continue;
                droppedItem.Kill(BaseNetworkable.DestroyMode.None);
            }
            var allCorpse = UnityEngine.GameObject.FindObjectsOfType<LootableCorpse>();
            for (int i = 0; i < allCorpse.Count(); i++)
            {
                var corpseItem = allCorpse[i];
                if (corpseItem == null) continue;
                corpseItem.Kill(BaseNetworkable.DestroyMode.None);
            }
            var allBag = UnityEngine.GameObject.FindObjectsOfType<DroppedItemContainer>();
            for (int i = 0; i < allBag.Count(); i++)
            {
                var bagItem = allBag[i];
                if (bagItem == null) continue;
                bagItem.Kill(BaseNetworkable.DestroyMode.None);
            }
        }

        private void BeginGame()
        {
            GameStarted = true;
            timer.Destroy(ref message);
            // Kill all player 5 seconds after start and TODO : clean entity before match start
            Timer killCountdown = timer.Once(5, () =>
            {
                if (GameStarted) { BasePlayer.activePlayerList.ForEach(x => x.Hurt(1000)); }
                RemoveAll();
            }
            );
            TimeLeft = timer.Once(PreparationTime, () =>
            {
                if (!GameStarted) { return; }
                PreparationUp = true;
                BasePlayer.activePlayerList.ForEach(x => SendChatMessage(x, CurrentTimeUp));
                if (!CupBoardBlue)
                {
                    BasePlayer.activePlayerList.ForEach(x => SendChatMessage(x, CurrentWin, CurrentBlueUpper, CurrentRedUpper));
                    ClearGame();
                }
                else if (!CupBoardRed)
                {
                    BasePlayer.activePlayerList.ForEach(x => SendChatMessage(x, CurrentWin, CurrentRedUpper, CurrentBlueUpper));
                    ClearGame();
                }
            }
            );
            Timer before60 = timer.Once(PreparationTime - 60, () =>
            {
                BasePlayer.activePlayerList.ForEach(x => SendChatMessage(x, CurrentRemaining, "60"));
            }
            );
            Timer before30 = timer.Once(PreparationTime - 30, () =>
            {
                BasePlayer.activePlayerList.ForEach(x => SendChatMessage(x, CurrentRemaining, "30"));
            }
            );
            Timer before15 = timer.Once(PreparationTime - 15, () =>
            {
                BasePlayer.activePlayerList.ForEach(x => SendChatMessage(x, CurrentRemaining, "15"));
            }
            );
            // Send message to all players
            BasePlayer.activePlayerList.ForEach(x => SendChatMessage(x, CurrentGameStart, PreparationTime));
        }

        private void JoinRed(BasePlayer player)
        {
            if (!RedTeam.Contains(player.UserIDString))
            {
                RedTeam.Add(player.UserIDString);
            }
            if (BlueTeam.Contains(player.UserIDString))
            {
                BlueTeam.Remove(player.UserIDString);
            }
            if (BlueReady.Contains(player.UserIDString))
            {
                BlueReady.Remove(player.UserIDString);
            }
            BasePlayer.activePlayerList.ForEach(x => SendChatMessage(x, CurrentNow, player.displayName, CurrentRedLower));
        }

        private void JoinBlue(BasePlayer player)
        {
            if (!BlueTeam.Contains(player.UserIDString))
            {
                BlueTeam.Add(player.UserIDString);
            }
            if (RedTeam.Contains(player.UserIDString))
            {
                RedTeam.Remove(player.UserIDString);
            }
            if (RedReady.Contains(player.UserIDString))
            {
                RedReady.Remove(player.UserIDString);
            }
            BasePlayer.activePlayerList.ForEach(x => SendChatMessage(x, CurrentNow, player.displayName, CurrentBlueLower));
        }

        private void JoinRand(BasePlayer player)
        {
            int teamNumber = rnd.Next() % 2;
            if (teamNumber == 0)
            {
                JoinRed(player);
            }
            else
            {
                JoinBlue(player);
            }
        }

        private void AddToUnreadyList(BasePlayer player)
        {
            if (!RedReady.Contains(player.displayName) && !BlueReady.Contains(player.displayName))
            {
                UnreadyList.Add(player.displayName);
            }
        }

        #endregion
    }
}
