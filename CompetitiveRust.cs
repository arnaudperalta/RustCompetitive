using System;
using System.Collections.Generic;
using System.Linq;
using Oxide.Core.Libraries.Covalence;

// TODO server name loading

// TODEBUG
// votekick, decay
namespace Oxide.Plugins {
    [Info("CompetitiveRust", "br0wnard", "0.4")]
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
        private const int DefaultTeamSize = 5;
        private const int DefaultPreparationTime = 600;
        private const int DefaultDecayTickRate = 1;
        private const int DefaultConsumeRate = 20;
        private const int DefaultGatherRate = 6;
        private const int DefaultPickupRate = 3;
        private const int DefaultCraftRate = 6;
        private const int DefaultScrapRate = 3;
        private const int DefaultBlueHoodie = 887162672;
        private const int DefaultRedHoodie = 887173152;
        private const bool DefaultOnlyDay = true;
        private const bool DefaultUnlockedBP = true;
        private const bool DefaultNoItemWear = true;

        public int TeamSize { get; private set; }
        public int PreparationTime { get; private set; }
        public int DecayTickRate { get; private set; }
        public int ConsumeRate { get; private set; }
        public int GatherRate { get; private set; }
        public int PickupRate { get; private set; }
        public int CraftRate { get; private set; }
        public int ScrapRate { get; private set; }
        public int BlueHoodie { get; private set; }
        public int RedHoodie { get; private set; }
        public bool OnlyDay { get; private set; }
        public bool UnlockedBP { get; private set; }
        public bool NoItemWear { get; private set; }

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
        private const string DefaultWin = "{0} team cupboard has been destroyed. {1} team won the game !";
        private const string DefaultReady = "{0} is now ready.";
        private const string DefaultUnready = "{0} is now unready.";
        private const string DefaultChoose = "Please choose a team before you get ready.";
        private const string DefaultRemaining = "{0} seconds remaining before the end of the preparation phase.";
        private const string DefaultRedLower = "<color=#ed3434ff>red</color>";
        private const string DefaultBlueLower = "<color=#1340d6ff>blue</color>";
        private const string DefaultRedUpper = "<color=#ed3434ff>Red</color>";
        private const string DefaultBlueUpper = "<color=#1340d6ff>Blue</color>";
        private const string DefaultBlue = "<color=#1340d6ff>{0}</color>";
        private const string DefaultRed = "<color=#ed3434ff>{0}</color>";
        private const string DefaultGrey = "<color=#9a9ca0ff>{0}</color>";
        private const string DefaultKill = "{0} killed {1}";
        private const string DefaultNoTC = "{0} team didn't place a cupboard. {1} team win the game.";
        private const string DefaultDraw = "No cupboard has been placed, draw.";
        private const string DefaultSurrend = "{0} team gave up. {1} team win.";
        private const string DefaultTeamFull = "This team is full.";
        private const string DefaultNotEnough = "Not enough player to do this.";
        private const string DefaultVoteKick = "{0} voted to kick {1} ({2}/{3}).";
        private const string DefaultVoteDone = "Vote successeful, player kicked.";
        private const string DefaultPlayerLeft = "{0} players remaining.";
        private const string DefaultGameProgress = "Game in progress.";
        private const string DefaultConnected = "{0} joined the game.";
        private const string DefaultDisconnected = "{0} left the game.";

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
        public string CurrentBlue { get; private set; }
        public string CurrentRed { get; private set; }
        public string CurrentGrey { get; private set; }
        public string CurrentKill { get; private set; }
        public string CurrentNoTC { get; private set; }
        public string CurrentDraw { get; private set; }
        public string CurrentSurrend { get; private set; }
        public string CurrentTeamFull { get; private set; }
        public string CurrentNotEnough { get; private set; }
        public string CurrentVoteKick { get; private set; }
        public string CurrentVoteDone { get; private set; }
        public string CurrentPlayerLeft { get; private set; }
        public string CurrentGameProgress { get; private set; }
        public string CurrentConnected { get; private set; }
        public string CurrentDisconnected { get; private set; }

        #endregion

        #region Variables

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
        public List<ulong> RedTeam { get; private set; }
        public List<ulong> BlueTeam { get; private set; }
        public List<ulong> RedReady { get; private set; }
        public List<ulong> BlueReady { get; private set; }

        public Dictionary<string, List<string>> VoteKickDcty { get; private set; }

        private Timer TimeLeft;
        private Timer message;
        private Timer TimeCheck;

        private int AlternativeMessage;
        private bool INIT = false;
        private Random rnd;
        private Covalence coval = new Covalence();
        private string DefaultHostName;

        #endregion

        #region Server Loading

        private void Loaded() => LoadConfigValues();

        private void Unload() => UnloadCraftTime();

        protected override void LoadDefaultConfig() => PrintWarning("Configuration file has been created.");

        private void LoadConfigValues()
        {
            // Plugin settings
            ChatPrefix = GetConfigValue("Settings", "ChatPrefix", DefaultChatPrefix);
            ChatPrefixColor = GetConfigValue("Settings", "ChatPrefixColor", DefaultChatPrefixColor);

            // Plugin options
            TeamSize = GetConfigValue("Options", "TeamSize", DefaultTeamSize);
            PreparationTime = GetConfigValue("Options", "PreparationTime", DefaultPreparationTime);
            DecayTickRate = GetConfigValue("Options", "DecayTickRate", DefaultDecayTickRate);
            ConsumeRate = GetConfigValue("Options", "ConsumeRate", DefaultConsumeRate);
            GatherRate = GetConfigValue("Options", "GatherRate", DefaultGatherRate);
            PickupRate = GetConfigValue("Options", "PickupRate", DefaultPickupRate);
            CraftRate = GetConfigValue("Options", "CraftRate", DefaultCraftRate);
            ScrapRate = GetConfigValue("Options", "ScrapRate", DefaultScrapRate);
            BlueHoodie = GetConfigValue("Options", "BlueHoodie", DefaultBlueHoodie);
            RedHoodie = GetConfigValue("Options", "RedHoodie", DefaultRedHoodie);
            OnlyDay = GetConfigValue("Options", "OnlyDay", DefaultOnlyDay);
            UnlockedBP = GetConfigValue("Options", "CurrentUnlockedBP", DefaultUnlockedBP);
            NoItemWear = GetConfigValue("Options", "CurrentNoItemWear", DefaultNoItemWear);

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
            CurrentBlue = GetConfigValue("Messages", "CurrentBlue", DefaultBlue);
            CurrentRed = GetConfigValue("Messages", "CurrentRed", DefaultRed);
            CurrentGrey = GetConfigValue("Messages", "CurrentGrey", DefaultGrey);
            CurrentKill = GetConfigValue("Messages", "CurrentKill", DefaultKill);
            CurrentNoTC = GetConfigValue("Messages", "CurrentNoTC", DefaultNoTC);
            CurrentDraw = GetConfigValue("Messages", "CurrentDraw", DefaultDraw);
            CurrentSurrend = GetConfigValue("Messages", "CurrentSurrend", DefaultSurrend);
            CurrentTeamFull = GetConfigValue("Messages", "CurrentTeamFull", DefaultTeamFull);
            CurrentNotEnough = GetConfigValue("Messages", "CurrentNotEnough", DefaultNotEnough);
            CurrentVoteKick = GetConfigValue("Messages", "CurrentVoteKick", DefaultVoteKick);
            CurrentVoteDone = GetConfigValue("Messages", "CurrentVoteDone", DefaultVoteDone);
            CurrentPlayerLeft = GetConfigValue("Messages", "CurrentPlayerLeft", DefaultPlayerLeft);
            CurrentGameProgress = GetConfigValue("Messages", "CurrentGameProgress", DefaultGameProgress);
            CurrentConnected = GetConfigValue("Messages", "CurrentConnected", DefaultConnected);
            CurrentDisconnected = GetConfigValue("Messages", "CurrentDisconnected", DefaultDisconnected);
        
            if (!configChanged){ return;}
            Puts("Configuration file updated.");
            SaveConfig();
        }

        #endregion

        #region Chat/Console command

        [ChatCommand("votekick")]
        private void VoteKickCommandChat(BasePlayer player, string command, string[] args)
        {
            int playerCount = BasePlayer.activePlayerList.Count;
            if (playerCount < 3)
            {
                SendChatMessage(player, CurrentNotEnough);
                return;
            }
            if (args.Length == 0)
            {
                SendChatMessage(player, "No target.");
                return;
            }
            IPlayer target = coval.Players.FindPlayer(args[0]);
            if (target == null || !target.IsConnected)
            {
                SendChatMessage(player, "Target not found.");
                return;
            }
            if (player.Equals(target))
            {
                SendChatMessage(player, "Can't vote for yourself.");
                return;
            }
            if (target.IsAdmin)
            {
                SendChatMessage(player, "You can't votekick an admin.");
                return;
            }
            // si deja votekick
            List<string> voteList;
            if (VoteKickDcty.ContainsKey(target.Id))
            {
                if (VoteKickDcty.TryGetValue(target.Id, out voteList))
                {
                    // si on a pas déja voter pour lui
                    if (!voteList.Contains(player.userID.ToString()))
                    {
                        voteList.Add(player.userID.ToString());
                    }
                }
            }
            else
            {
                voteList = new List<string>();
                voteList.Add(player.userID.ToString());
                VoteKickDcty.Add(target.Id, voteList);
            }
            if (voteList.Count >= playerCount * 0.6f)
            {
                target.Kick("Vote kicked.");
                BasePlayer.activePlayerList.ForEach(x => SendChatMessage(x, CurrentVoteDone));
            }
            else
            {
                BasePlayer.activePlayerList.ForEach(x => SendChatMessage(x, CurrentVoteKick, player.displayName, target.Id, voteList.Count, playerCount * 0.6f));
            }
        }

        [ChatCommand("gg")]
        private void SurrendCommandChat(BasePlayer player, string command, string[] args)
        {
            if (!GameStarted)
            {
                if (RedTeam.Contains(player.userID))
                {
                    BasePlayer.activePlayerList.ForEach(x => SendChatMessage(x, CurrentSurrend, CurrentRedUpper, CurrentBlueUpper));
                    ClearGame();
                }
                else if (BlueTeam.Contains(player.userID))
                {
                    BasePlayer.activePlayerList.ForEach(x => SendChatMessage(x, CurrentSurrend, CurrentBlueUpper, CurrentRedUpper));
                    ClearGame();
                }
            }
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
            if (RedTeam.Contains(player.userID) && !RedReady.Contains(player.userID))
            {
                RedReady.Add(player.userID);
                BasePlayer.activePlayerList.ForEach(x => SendChatMessage(x, CurrentReady, player.displayName));
            }
            else if (BlueTeam.Contains(player.userID) && !BlueReady.Contains(player.userID))
            {
                BlueReady.Add(player.userID);
                BasePlayer.activePlayerList.ForEach(x => SendChatMessage(x, CurrentReady, player.displayName));
            }
            else if (!RedTeam.Contains(player.userID) && !BlueReady.Contains(player.userID))
            {
                SendChatMessage(player, CurrentChoose);
            }
            if (RedReady.Count == TeamSize && BlueReady.Count == TeamSize)
            {
                BeginGame();
            }
        }

        [ChatCommand("unready")]
        private void UnreadyCommandChat(BasePlayer player, string command, string[] args)
        {
            if (GameStarted) { return; }
            if (RedReady.Contains(player.userID))
            {
                RedReady.Remove(player.userID);
                BasePlayer.activePlayerList.ForEach(x => SendChatMessage(x, CurrentUnready, player.displayName));
            }
            else if (BlueReady.Contains(player.userID))
            {
                BlueReady.Remove(player.userID);
                BasePlayer.activePlayerList.ForEach(x => SendChatMessage(x, CurrentUnready, player.displayName));
            }
        }

        [ChatCommand("random")]
        private void RandomCommandChat(BasePlayer player, string command, string[] args)
        {
            if (GameStarted) { return; }
            if (!player.IsAdmin)
            {
                SendChatMessage(player, CurrentNoPermission);
                return;
            }
            BasePlayer.activePlayerList.ForEach(x => JoinRand(x));
        }

        [ChatCommand("help")]
        private void HelpCommandChat(BasePlayer player, string command, string[] args)
        {
            SendChatMessage(player, "Rules:");
            SendChatMessage(player, "Two teams, red or blue, you need to destroy the ennemy TC.");
            SendChatMessage(player, "A preparation phase of " + PreparationTime + "seconds without PvP.");
            SendChatMessage(player, "During this phase you need to place your team cupboard.");
            SendChatMessage(player, "Commands: \n");
            SendChatMessage(player, "/votekick <player name> for votekick a player.");
            SendChatMessage(player, "/gg to surrend.");
            SendChatMessage(player, "/join to join a team before the game start.");
            SendChatMessage(player, "/ready to get ready before the game start.");
            SendChatMessage(player, "/unready to get unready before the game start.");
        }

        #endregion

        #region ServerHook

        private void OnServerInitialized()
        {
            // List initialization
            UnreadyList = new List<string>();
            RedTeam = new List<ulong>();
            BlueTeam = new List<ulong>();
            RedReady = new List<ulong>();
            BlueReady = new List<ulong>();

            VoteKickDcty = new Dictionary<string, List<string>>();

            rnd = new Random();
            AlternativeMessage = 0;

            message = timer.Repeat(15, -1, () =>
            {
                if (!GameStarted)
                {
                    ++AlternativeMessage;
                    if (AlternativeMessage % 2 == 0)
                    {
                        BasePlayer.activePlayerList.ForEach(x => SendChatMessage(x, "Please pick a team with /join and get ready with /ready."));
                    }
                    else
                    {
                        BasePlayer.activePlayerList.ForEach(x =>
                        {
                            if (!RedReady.Contains(x.userID) && !BlueReady.Contains(x.userID))
                            {
                                UnreadyList.Add(x.displayName);
                            }
                        });
                        BasePlayer.activePlayerList.ForEach(x => SendChatMessage(x, "Players unready : {0}", string.Join(",", UnreadyList)));
                        UnreadyList.Clear();
                    }
                }
            });
            if (OnlyDay)
            {
                TimeCheck = timer.Repeat(60, -1, () =>
                {
                    TOD_Sky.Instance.Cycle.Hour = 12;
                });
            }
            LoadCraftTime();
            DefaultHostName = ConVar.Server.hostname;
            int Consume = 1440 / ConsumeRate;
            covalence.Server.Command("decay.upkeep_period_minutes " + Consume);
            covalence.Server.Command("decay.scale " + ConsumeRate);
            covalence.Server.Command("decay.tick " + DecayTickRate);
            RefreshServerName();
            INIT = true;
        }

        private void OnPlayerDisconnected(BasePlayer player, string reason)
        {
            timer.Once(5, () => {
                RefreshServerName();
            });
            NotifyPlayerConnection(false, player.displayName);
        }

        private object OnLootSpawn(LootContainer container)
        {
            if (INIT)
            {
                if (container?.inventory?.itemList == null) return null;
                while (container.inventory.itemList.Count > 0)
                {
                    var item = container.inventory.itemList[0];
                    item.RemoveFromContainer();
                    item.Remove(0f);
                }
                container.PopulateLoot();
                foreach (Item i in container.inventory.itemList)
                {
                    if (i.amount > 2)
                    {
                        i.amount *= ScrapRate;
                    }
                }
                return container;
            }
            return null;
        }

        private void OnDispenserGather(ResourceDispenser dispenser, BaseEntity entity, Item item)
        {
            if (!entity.ToPlayer()) { return; }
            item.amount = item.amount * GatherRate;
            try
            {
                dispenser.containedItems.Single(x => x.itemid == item.info.itemid).amount += item.amount - item.amount / GatherRate;

                if (dispenser.containedItems.Single(x => x.itemid == item.info.itemid).amount < 0)
                {
                    item.amount += (int)dispenser.containedItems.Single(x => x.itemid == item.info.itemid).amount;
                }
            }
            catch { }
        }

        private void OnDispenserBonus(ResourceDispenser dispenser, BaseEntity entity, Item item)
        {
            OnDispenserGather(dispenser, entity, item);
        }

        private void OnCropGather(PlantEntity plant, Item item)
        {
            item.amount = (int)(item.amount * GatherRate);
        }

        private void OnQuarryGather(MiningQuarry quarry, Item item)
        {
            item.amount = (int)(item.amount * GatherRate);
        }

        private void OnCollectiblePickup(Item item, BasePlayer player)
        {
            item.amount = (int)(item.amount * GatherRate);
        }

        private void OnSurveyGather(SurveyCharge surveyCharge, Item item)
        {
            item.amount = (int)(item.amount * GatherRate);
        }

        private void OnEntityDeath(BaseCombatEntity entity, HitInfo info)
        {
            if (GameStarted)
            {
                if (info == null || entity == null) { return; }
                if (entity is BaseNpc) { return; }
                BasePlayer victim = entity.ToPlayer();
                if (info?.Initiator?.ToPlayer() == null) { return; }
                BasePlayer killer = info.Initiator.ToPlayer();
                if (victim == null || killer == null) { return; }
                if (BlueTeam.Contains(killer.userID))
                {
                    BasePlayer.activePlayerList.ForEach(x => SendChatMessage(x, CurrentKill, String.Format(CurrentBlue, killer.displayName), String.Format(CurrentRed, victim.displayName)));
                }
                else if (RedTeam.Contains(killer.userID))
                {
                    BasePlayer.activePlayerList.ForEach(x => SendChatMessage(x, CurrentKill, String.Format(CurrentRed, killer.displayName), String.Format(CurrentBlue, victim.displayName)));
                }
            }
        }

        private void OnLoseCondition(Item item, ref float amount)
        {
            if (item != null && NoItemWear)
            {
                BasePlayer player;
                if (item.GetOwnerPlayer() == null)
                {
                    if (item?.info == null) return;
                    if (!item.info.shortname.Contains("mod")) return;
                    player = item?.GetRootContainer()?.GetOwnerPlayer();
                    if (player == null)
                        return;
                }
                else player = item.GetOwnerPlayer();
                if (player != null)
                {
                    var def = ItemManager.FindItemDefinition(item.info.itemid);
                    if (item.hasCondition) { item.RepairCondition(amount); }
                }
            }
        }

        private void OnEntityTakeDamage(BaseEntity entity, HitInfo info)
        {
            if (!GameStarted) { return; }
            // rate decay
            if (PreparationUp && GameStarted) { return; }
            if (entity == null || info == null) { return; }
            BasePlayer target = entity as BasePlayer;
            if (info.Initiator == null) { return; }
            BasePlayer from = info.Initiator as BasePlayer;
            if (target == null || from == null) { return; }
            if (target.UserIDString == from.UserIDString) { return; }
            info.damageTypes.ScaleAll(0);
        }

        private void OnEntityKill(BaseNetworkable entity)
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

        private void OnEntitySpawned(BaseEntity entity, UnityEngine.GameObject gameObject)
        {
            if (entity.ShortPrefabName.Contains("cupboard.tool"))
            {
                BasePlayer player = BasePlayer.FindByID(entity.OwnerID);
                if (player == null) { return; }
                if (!GameStarted)
                {
                    entity.KillMessage();
                    var itemtogive = ItemManager.CreateByItemID(-97956382, 1);
                    if (itemtogive != null) player.inventory.GiveItem(itemtogive);
                    SendChatMessage(player, CurrentStartCup);
                    return;
                }
                if (RedTeam.Contains(player.userID) && CupBoardRed)
                {
                    entity.KillMessage();
                    var itemtogive = ItemManager.CreateByItemID(-97956382, 1);
                    if (itemtogive != null) player.inventory.GiveItem(itemtogive);
                    SendChatMessage(player, CurrentCupLimit);
                    return;
                }
                if (BlueTeam.Contains(player.userID) && CupBoardBlue)
                {
                    entity.KillMessage();
                    var itemtogive = ItemManager.CreateByItemID(-97956382, 1);
                    if (itemtogive != null) player.inventory.GiveItem(itemtogive);
                    SendChatMessage(player, CurrentCupLimit);
                    return;
                }
                if (RedTeam.Contains(player.userID))
                {
                    CupBoardRed = true;
                    CupBoardRedString = entity.ToString();
                    SendChatMessage(player, CurrentCupPlaced);
                    return;
                }
                if (BlueTeam.Contains(player.userID))
                {
                    CupBoardBlue = true;
                    CupBoardBlueString = entity.ToString();
                    SendChatMessage(player, CurrentCupPlaced);
                    return;
                }
            }
        }

        private void OnPlayerRespawned(BasePlayer player)
        {
            if (RedTeam.Contains(player.userID))
            {
                var i = ItemManager.CreateByItemID(1751045826, 1, (ulong)RedHoodie);
                if (i != null) { player.inventory.GiveItem(i, player.inventory.containerWear); }
                return;
            }
            if (BlueTeam.Contains(player.userID))
            {
                var i = ItemManager.CreateByItemID(1751045826, 1, (ulong)BlueHoodie);
                if (i != null) { player.inventory.GiveItem(i, player.inventory.containerWear); }
                return;
            }
            return;
        }

        private void OnPlayerInit(BasePlayer player)
        {
            if (GameStarted)
            {
                if (!BlueTeam.Contains(player.userID) || !RedTeam.Contains(player.userID))
                {
                    player.Kick("Game started. Please connect when the servername is updated.");
                }
            }
            NotifyPlayerConnection(true, player.displayName);
            RefreshServerName();
            if (UnlockedBP) {
                UnlockBP(player);
            }
        }

        #endregion

        #region Helper methods

        private void NotifyPlayerConnection(bool type, string playerName)
        {
            if (type)
            {
                BasePlayer.activePlayerList.ForEach(x => SendChatMessage(x, CurrentConnected, playerName));
            } else
            {
                BasePlayer.activePlayerList.ForEach(x => SendChatMessage(x, CurrentDisconnected, playerName));
            }
        }

        private void RefreshServerName()
        {
            if (GameStarted)
            {
                covalence.Server.Command("server.hostname "
                + '"' + DefaultHostName
                + ' '
                + CurrentGameProgress + '"');
                return;
            }
            int slotRemaining = ConVar.Server.maxplayers - BasePlayer.activePlayerList.Count;
            covalence.Server.Command("server.hostname " 
                + '"' + DefaultHostName 
                + ' ' 
                + String.Format(CurrentPlayerLeft, slotRemaining) + '"');
        }

        private void LoadCraftTime()
        {
            foreach (var bp in ItemManager.bpList)
            {
                if (CraftRate != 0f)
                {
                    bp.time = bp.time / CraftRate;
                }
                else
                {
                    bp.time = 0f;
                }
            }
        }

        private void UnloadCraftTime()
        {
            foreach (var bp in ItemManager.bpList)
            {
                if (CraftRate != 0f)
                {
                    bp.time = bp.time * CraftRate;
                }
                else
                {
                    bp.time = 0f;
                }
            }
        }

        private void ClearGame()
        {
            PreparationUp = DefaultPreparationUp;
            GameStarted = DefaultGameStarted;
            CupBoardRed = DefaultCupBoardRed;
            CupBoardBlue = DefaultCupBoardBlue;
            CupBoardRedString = DefaultCupBoardRedString;
            CupBoardBlueString = DefaultCupBoardBlueString;
            RedTeam.Clear();
            BlueTeam.Clear();
            RedReady.Clear();
            BlueReady.Clear();
            RefreshServerName();
        }

        private void SendChatMessage(BasePlayer player, string message, params object[] args) 
            => player?.SendConsoleCommand("chat.add", -1, string.Format($"<color={ChatPrefixColor}>{ChatPrefix}</color>: {message}", args), 1.0);

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
            var allVehicules = UnityEngine.GameObject.FindObjectsOfType<BaseVehicle>();
            for (int i = 0; i < allVehicules.Count(); i++)
            {
                var vehicules = allVehicules[i];
                if (vehicules == null) continue;
                vehicules.Kill(BaseNetworkable.DestroyMode.None);
            }
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
            var allEntity = UnityEngine.GameObject.FindObjectsOfType<BaseEntity>();
            for (int i = 0; i < allEntity.Count(); i++)
            {
                var entity = allEntity[i];
                if (entity == null || entity.OwnerID == 0) continue;
                entity.Kill(BaseNetworkable.DestroyMode.None);
            }
        }

        private void BeginGame()
        {
            GameStarted = true;
            timer.Destroy(ref message);
            Timer killCountdown = timer.Once(2, () =>
            {
                if (GameStarted) {
                    BasePlayer.activePlayerList.ForEach(x => x.Hurt(1000));
                    BasePlayer.sleepingPlayerList.ForEach(x => x.Kill(BaseNetworkable.DestroyMode.None));
                }
                RemoveAll();
            }
            );
            TimeLeft = timer.Once(PreparationTime, () =>
            {
                if (!GameStarted) { return; }
                PreparationUp = true;
                BasePlayer.activePlayerList.ForEach(x => SendChatMessage(x, CurrentTimeUp));
                if (!CupBoardBlue && !CupBoardRed)
                {
                    BasePlayer.activePlayerList.ForEach(x => SendChatMessage(x, CurrentDraw));
                    ClearGame();
                }
                else if (!CupBoardBlue)
                {
                    BasePlayer.activePlayerList.ForEach(x => SendChatMessage(x, CurrentNoTC, CurrentBlueUpper, CurrentRedLower));
                    ClearGame();
                }
                else if (!CupBoardRed)
                {
                    BasePlayer.activePlayerList.ForEach(x => SendChatMessage(x, CurrentNoTC, CurrentRedUpper, CurrentBlueLower));
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
            BasePlayer.activePlayerList.ForEach(x => SendChatMessage(x, CurrentGameStart, PreparationTime));
            RefreshServerName();
        }

        private void JoinRed(BasePlayer player)
        {
            if (RedTeam.Count == TeamSize)
            {
                SendChatMessage(player, CurrentTeamFull);
                return;
            }
            if (!RedTeam.Contains(player.userID))
            {
                RedTeam.Add(player.userID);
            }
            if (BlueTeam.Contains(player.userID))
            {
                BlueTeam.Remove(player.userID);
            }
            if (BlueReady.Contains(player.userID))
            {
                BlueReady.Remove(player.userID);
            }
            BasePlayer.activePlayerList.ForEach(x => SendChatMessage(x, CurrentNow, player.displayName, CurrentRedLower));
        }

        private void JoinBlue(BasePlayer player)
        {
            if (BlueTeam.Count == TeamSize)
            {
                SendChatMessage(player, CurrentTeamFull);
                return;
            }
            if (!BlueTeam.Contains(player.userID))
            {
                BlueTeam.Add(player.userID);
            }
            if (RedTeam.Contains(player.userID))
            {
                RedTeam.Remove(player.userID);
            }
            if (RedReady.Contains(player.userID))
            {
                RedReady.Remove(player.userID);
            }
            BasePlayer.activePlayerList.ForEach(x => SendChatMessage(x, CurrentNow, player.displayName, CurrentBlueLower));
        }

        private void JoinRand(BasePlayer player)
        {
            int teamNumber = rnd.Next() % 2;
            if (teamNumber == 0 && RedTeam.Count < TeamSize)
            {
                JoinRed(player);
            }
            else if (BlueTeam.Count < TeamSize)
            {
                JoinBlue(player);
            }
        }

        private void UnlockBP(BasePlayer player)
        {
            var info = SingletonComponent<ServerMgr>.Instance.persistance.GetPlayerInfo(player.userID);
            info.unlockedItems = ItemManager.bpList
                .Select(x => x.targetItem.itemid)
                .ToList();
            SingletonComponent<ServerMgr>.Instance.persistance.SetPlayerInfo(player.userID, info);
            player.SendNetworkUpdate();
        }

        #endregion
    }
}
