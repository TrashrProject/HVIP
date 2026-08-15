
using log4net;

using Plus.Communication.Packets;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Moderation;
using Plus.HabboHotel.Support;
using Plus.HabboHotel.Catalog;
using Plus.HabboHotel.Items;
using Plus.HabboHotel.Items.Televisions;
using Plus.HabboHotel.Navigator;
using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.Groups;
using Plus.HabboHotel.Quests;
using Plus.HabboHotel.Achievements;
using Plus.HabboHotel.LandingView;
using Plus.HabboHotel.Global;
using Plus.HabboHotel.Groups.Forums;
using Plus.HabboHotel.Games;
using Plus.HabboRoleplay.VehicleOwned;
using Plus.HabboHotel.Rooms.Chat;
using Plus.HabboHotel.Talents;
using Plus.HabboHotel.Bots;
using Plus.HabboHotel.Cache;
using Plus.HabboHotel.Rewards;
using Plus.HabboHotel.Badges;
using Plus.HabboHotel.Permissions;
using Plus.HabboHotel.Subscriptions;
using System.Threading;
using System.Threading.Tasks;
using Plus.HabboHotel.Users.Helpers;
using Plus.HabboHotel.Roleplay.Web;
using Plus.HabboRoleplay.Events;
using Plus.HabboHotel.Rooms.TraxMachine;
using Plus.HabboRoleplay.Misc;
using Plus.HabboHotel.RolePlay.PlayRoom;
using Plus.HabboRoleplay.Food;
using Plus.HabboRoleplay.Combat;
using Plus.HabboRoleplay.Weapons;
using Plus.HabboRoleplay.Vehicles;
using Plus.HabboRoleplay.Houses;
using Plus.HabboRoleplay.PhoneChat;
using Plus.HabboRoleplay.VehiclesJobs;
using Plus.HabboRoleplay.Phones;
using Plus.HabboRoleplay.PhoneOwned;
using Plus.HabboRoleplay.Products;
using Plus.HabboRoleplay.PhonesApps;
using Plus.HabboHotel.RolePlay.PlayInternet;
using Plus.Messages.Net.MusCommunication;
using Plus.HabboRoleplay.Comodin;
using Plus.HabboRoleplay.Apartments;
using Plus.HabboRoleplay.ApartmentsOwned;
using Plus.HabboRoleplay.Business;
using Plus.HabboRoleplay.GangTurfs;
using Plus.Utilities;
using Plus.HabboRoleplay.TaxiNodes;
using Plus.HabboRoleplay.TaxiRoomNodes;
//using Plus.Mus;

namespace Plus.HabboHotel
{
    public class Game
    {
        private static readonly ILog log = LogManager.GetLogger("Plus.HabboHotel.Game");

        private readonly PacketManager _packetManager;
        private readonly MusPacketManager _muspacketManager;
        private readonly GameClientManager _clientManager;
        private readonly ModerationManager _modManager;
        private readonly ModerationTool _moderationTool;//TODO: Initialize from the moderation manager.
        private readonly ItemDataManager _itemDataManager;
        private readonly CatalogManager _catalogManager;
        private readonly TelevisionManager _televisionManager;//TODO: Initialize from the item manager.
        private readonly NavigatorManager _navigatorManager;
        private readonly RoomManager _roomManager;
        private readonly ChatManager _chatManager;
        public readonly GroupManager _groupManager;
        private readonly QuestManager _questManager;
        private readonly AchievementManager _achievementManager;
        private readonly TalentTrackManager _talentTrackManager;
        private readonly LandingViewManager _landingViewManager;//TODO: Rename class
        private readonly GameDataManager _gameDataManager;
        private readonly ServerStatusUpdater _globalUpdater;
        private readonly LanguageLocale _languageLocale;
        private readonly AntiMutant _antiMutant;
        private readonly BotManager _botManager;
        private readonly CacheManager _cacheManager;
        private readonly RewardManager _rewardManager;
        private readonly BadgeManager _badgeManager;
        private readonly PermissionManager _permissionManager;
        public readonly PlayRoomManager _playroomManager;
        private readonly SubscriptionManager _subscriptionManager;
        public readonly GroupForumManager _groupForumManager;
        private readonly GuideManager _guideManager;
        private readonly  HallOfFame _HallOfFame;
        // RP
        private readonly HouseManager _houseManager;
        private readonly ApartmentOwnedManager _apartmentownedManager;
        private readonly PhoneChatManager _phonechatManager;
        private readonly VehiclesOwnedManager _vehiclesownedManager;
        private readonly PhonesOwnedManager _phonesownedManager;
        private readonly WebEventManager _webEventManager;
        //private readonly MusWebEventManager _muswebEventManager;
        private readonly BusinessBalanceManager _businessbalanceManager;
        private readonly GangTurfsManager _gangturfsManager;
        private readonly Dijkstra _dijkstra;

        private bool _cycleEnded;
        private bool _cycleActive;
        private Task _gameCycle;
        private int _cycleSleepTime = 25;

        public Game()
        {
            this._packetManager = new PacketManager();
            this._muspacketManager = new MusPacketManager();
            this._clientManager = new GameClientManager();
            this._modManager = new ModerationManager();
            this._moderationTool = new ModerationTool();

            this._itemDataManager = new ItemDataManager();
            this._itemDataManager.Init();

            this._catalogManager = new CatalogManager();
            this._catalogManager.Init(this._itemDataManager);

            this._televisionManager = new TelevisionManager();

            this._navigatorManager = new NavigatorManager();
            this._roomManager = new RoomManager();
            this._chatManager = new ChatManager();
            this._groupManager = new GroupManager();
            this._groupForumManager = new GroupForumManager();
            this._questManager = new QuestManager();
            this._achievementManager = new AchievementManager();
            this._talentTrackManager = new TalentTrackManager();

            this._landingViewManager = new LandingViewManager();
            this._gameDataManager = new GameDataManager();

            this._globalUpdater = new ServerStatusUpdater();
            this._globalUpdater.Init();

            this._languageLocale = new LanguageLocale();
            this._antiMutant = new AntiMutant();
            this._botManager = new BotManager();
            this._HallOfFame = new HallOfFame();


            this._cacheManager = new CacheManager();

            this._rewardManager = new RewardManager();

            this._badgeManager = new BadgeManager();
            this._badgeManager.Init();

            //Jukebox
            TraxSoundManager.Init();

            FoodManager.Initialize();

            this._permissionManager = new PermissionManager();
            this._permissionManager.Init();

            this._playroomManager = new PlayRoomManager();
            this._playroomManager.Init();

            this._subscriptionManager = new SubscriptionManager();
            this._subscriptionManager.Init();
            this._guideManager = new GuideManager();

            // RP
            RoleplayData.Initialize();
            CombatManager.Initialize();
            WeaponManager.Initialize();
            VehicleManager.Initialize();
            VehicleJobsManager.Initialize();
            PhoneManager.Initialize();
            PhoneAppManager.Initialize();
            PlayInternetManager.Init();
            ProductsManager.Initialize();
            RoleplayManager.AssingInventoryProducts();
            EventManager.Initialize();
            this._webEventManager = new WebEventManager();
            this._webEventManager.Init();
            ComodinManager.Initialize();
            ApartmentManager.Init();

            this._dijkstra = new Dijkstra();
            this._dijkstra.Init();
            TaxiNodeManager.Initialize();
            TaxiRoomNodeManager.Initialize();
            TaxiRoomNodeTemplateManager.Initialize(this._dijkstra);
            /*
            this._muswebEventManager = new MusWebEventManager();
            this._muswebEventManager.Init();*/

            this._houseManager = new HouseManager();
            this._houseManager.Init();

            this._apartmentownedManager = new ApartmentOwnedManager();
            this._apartmentownedManager.Init();

            this._phonechatManager = new PhoneChatManager();
            this._phonechatManager.Init();

            this._vehiclesownedManager = new VehiclesOwnedManager();
            this._vehiclesownedManager.Init();

            this._phonesownedManager = new PhonesOwnedManager();
            this._phonesownedManager.Init();

            this._businessbalanceManager = new BusinessBalanceManager();
            this._businessbalanceManager.Init();

            this._gangturfsManager = new GangTurfsManager();
            this._gangturfsManager.Init();
        }

        public void StartGameLoop()
        {
            this._gameCycle = new Task(GameCycle);
            this._gameCycle.Start();

            this._cycleActive = true;
        }

        private void GameCycle()
        {
            while (this._cycleActive)
            {
                this._cycleEnded = false;

                PlusEnvironment.GetGame().GetRoomManager().OnCycle();
                PlusEnvironment.GetGame().GetClientManager().OnCycle();

                this._cycleEnded = true;
                Thread.Sleep(this._cycleSleepTime);
            }
        }

        public void StopGameLoop()
        {
            this._cycleActive = false;

            while (!this._cycleEnded)
            {
                Thread.Sleep(this._cycleSleepTime);
            }
        }

        public PacketManager GetPacketManager()
        {
            return _packetManager;
        }

        public MusPacketManager GetMusPacketManager()
        {
            return _muspacketManager;
        }

        public GameClientManager GetClientManager()
        {
            return _clientManager;
        }

        public CatalogManager GetCatalog()
        {
            return _catalogManager;
        }

        public NavigatorManager GetNavigator()
        {
            return _navigatorManager;
        }

        public ItemDataManager GetItemManager()
        {
            return _itemDataManager;
        }

        public RoomManager GetRoomManager()
        {
            return _roomManager;
        }

        public AchievementManager GetAchievementManager()
        {
            return _achievementManager;
        }

        public TalentTrackManager GetTalentTrackManager()
        {
            return _talentTrackManager;
        }


        public ServerStatusUpdater GetServerStatusUpdater()
        {
            return this._globalUpdater;
        }

        public ModerationTool GetModerationTool()
        {
            return _moderationTool;
        }

        public ModerationManager GetModerationManager()
        {
            return this._modManager;
        }

        public PermissionManager GetPermissionManager()
        {
            return this._permissionManager;
        }

        public PlayRoomManager GetPlayRoomManager()
        {
            return this._playroomManager;
        }

        public SubscriptionManager GetSubscriptionManager()
        {
            return this._subscriptionManager;
        }

        public QuestManager GetQuestManager()
        {
            return this._questManager;
        }

        public GroupManager GetGroupManager()
        {
            return _groupManager;
        }
        public GroupForumManager GetGroupForumManager()
        {
            return _groupForumManager;
        }

        public LandingViewManager GetLandingManager()
        {
            return _landingViewManager;
        }
        public TelevisionManager GetTelevisionManager()
        {
            return _televisionManager;
        }

        public ChatManager GetChatManager()
        {
            return this._chatManager;
        }

        public GameDataManager GetGameDataManager()
        {
            return this._gameDataManager;
        }

        public HouseManager GetHouseManager()
        {
            return this._houseManager;
        }
        public ApartmentOwnedManager GetApartmentOwnedManager()
        {
            return this._apartmentownedManager;
        }

        public PhoneChatManager GetPhoneChatManager()
        {
            return this._phonechatManager;
        }

        public BusinessBalanceManager GetBusinessBalanceManager()
        {
            return this._businessbalanceManager;
        }

        public VehiclesOwnedManager GetVehiclesOwnedManager()
        {
            return this._vehiclesownedManager;
        }

        public GangTurfsManager GetGangTurfsManager()
        {
            return this._gangturfsManager;
        }

        public PhonesOwnedManager GetPhonesOwnedManager()
        {
            return this._phonesownedManager;
        }

        public Dijkstra GetDijkstra()
        {
            return this._dijkstra;
        }

        public LanguageLocale GetLanguageLocale()
        {
            return this._languageLocale;
        }

        public AntiMutant GetAntiMutant()
        {
            return this._antiMutant;
        }

        public BotManager GetBotManager()
        {
            return this._botManager;
        }

        public CacheManager GetCacheManager()
        {
            return this._cacheManager;
        }

        public RewardManager GetRewardManager()
        {
            return this._rewardManager;
        }

        public BadgeManager GetBadgeManager()
        {
            return this._badgeManager;
        }

        public GuideManager GetGuideManager()
        {
            return _guideManager;
        }

        public HallOfFame GetHallOfFame()
        {
            return _HallOfFame;
        }

        // RP
        public WebEventManager GetWebEventManager()
        {
            return _webEventManager;
        }
        /*
        public MusWebEventManager GetMusWebEventManager()
        {
            return _muswebEventManager;
        }
        */
    }
}