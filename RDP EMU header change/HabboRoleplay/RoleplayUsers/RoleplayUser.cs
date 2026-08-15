using System;
using System.Data;
using System.Linq;
using System.Collections.Generic;
using System.Collections.Concurrent;
using Plus.HabboHotel.Items;
using Plus.HabboHotel.GameClients;
using Plus.Database.Interfaces;
using Plus.HabboHotel.Catalog.Clothing;
using Plus.HabboHotel.Users.Messenger;
using Plus.Communication.Packets.Outgoing.Messenger;
using Plus.HabboHotel.Rooms;
using Fleck;
using Plus.HabboRoleplay.Web.Outgoing.Statistics;
using Plus.HabboRoleplay.Events;
using Newtonsoft.Json;
using Plus.HabboRoleplay.Timers;
using Plus.HabboRoleplay.Cooldowns;
using Plus.HabboRoleplay.Weapons;
using Plus.HabboRoleplay.Vehicles;
using Plus.HabboRoleplay.RoleplayUsers.Offers;
using System.Drawing;
using Plus.HabboRoleplay.Houses;
using Plus.HabboRoleplay.PhoneOwned;
using Plus.HabboRoleplay.ProductOwned;
using Plus.HabboRoleplay.Misc;
using Plus.HabboRoleplay.PhoneAppOwned;
using Plus.HabboRoleplay.PhonesApps;
using Plus.HabboHotel.RolePlay.PlayInternet;

namespace Plus.HabboRoleplay.RoleplayUsers
{
    public class RoleplayUser
    {
        #region Saved Variables
        // Client Info
        GameClient Client;

        // Basic Info
        private uint mId;

        // Level
        private int mLevel;
        private int mCurXP;
        private int mNeedXP;

        // Human Needs
        private int mMaxHealth;
        private int mCurHealth;
        private int mArmor;
        private int mHunger;
        private int mHygiene;

        // Jailed/Dead - Wanted - Sendhome - Cuffed
        private bool mIsDying;
        private int mDyingTimeLeft;
        private bool mIsDead;
        private int mDeadTimeLeft;
        private bool mIsJailed;
        private int mJailedTimeLeft;
        private bool mIsWanted;
        private int mWantedLevel;
        private int mWantedTimeLeft;
        private bool mCuffed;
        private int mCuffedTimeLeft;
        private int mSendHomeTimeLeft;
        private bool mChNDisabled;
        private bool mChNBanned;
        private int mChNBannedTimeLeft;
        private bool mIsSanc;
        private int mSancTimeLeft;
        private int mSancs;

        // Misc
        private string mLastCoordinates;

        // Passive Mode
        private bool mPassiveMode;

        // Reportero
        private bool mIsNewsReporter;

        // Noob
        private bool mIsNoob;
        private int mNoobTimeLeft;

        // Banking
        private int mBank;

        // Outfits
        public string OriginalOutfit = null;
        public ClothingItem Clothing = null;
        public bool PurchasingClothing = false;

        // Levelable Stats
        private int mStrength;
        // Extra Variables for Levelable Stats
        private int mStrengthEXP;

        // Manages the offers for the user
        public OfferManager OfferManager;

        // Manages the timers for the user
        public TimerManager TimerManager;

        // Handles the users data
        public UserDataHandler UserDataHandler;

        // Manages the cooldowns for the user
        public CooldownManager CooldownManager;

        // Saved Cooldowns
        public ConcurrentDictionary<string, int> SpecialCooldowns = new ConcurrentDictionary<string, int>();

        // Weapons Owned
        private ConcurrentDictionary<string, Weapon> mOwnedWeapons;

        // Products Owned
        private ConcurrentDictionary<int, ProductsOwned> mOwnedProducts;

        // Inventory
        private int mWeed;
        private int mCocaine;
        private int mMedicines;
        private int mBidon;
        private int mMecParts;
        private int mArmMat;
        private int mArmPieces;

        // Phone
        public int mPhone = 0;
        public int mPhoneModelId = 0;
        public string mPhoneNumber = "";

        // Phone App Owend
        private ConcurrentDictionary<int, PhonesAppsOwned> mOwnedPhonesApps;

        // Combat
        public bool InCombat = false;
        public bool CombatMode = false;
        public Weapon EquippedWeapon = null;
        public string LastCommand = "";
        public int GunShots = 0;

        // Vehicles
        private int mCarLimit;
        private int mCarType;
        private int mCarFuel;
        private int mCarMaxFuel;
        //private ConcurrentDictionary<string, Vehicle> mOwnedVehicles;

        // Statistics
        private int mTimeWorked;
        private int mPunches;
        private int mKills;
        private int mHitKills;
        private int mGunKills;
        private int mCopKills;
        private int mDeaths;
        private int mCopDeaths;
        private int mArrests;
        private int mArrested;
        private int mEvasions;
        private int mCocaineTaken;
        private int mMedicinesTaken;
        private int mWeedtaken;
        private int mGunsFab;
        private int mTotalShifts;
        private int mMoneyEarned;
        private int mPLEarned;
        private int mDrugsTaken;

        private int mTutorialStep;

        public int mCorp;
        public int mGang;

        #region Jobs Levels

        #region Camionero
        private int mCamLvl;
        private int mCamXP;
        #endregion

        #region Minero
        private int mMinerLvl;
        private int mMinerXP;
        #endregion

        #region Armero
        private int mArmLvl;
        private int mArmXP;
        #endregion

        #region Mecánico
        private int mMecLvl;
        private int mMecXP;
        #endregion

        #region Basurero
        private int mBasuLvl;
        private int mBasuXP;
        #endregion

        #region Ladrón
        private int mLadronLvl;
        private int mLadronXP;
        #endregion

        #endregion

        private int mChangeNameCount;
        #endregion

        #region UnSaved Variables
        // Change Name
        public bool ViewChangeName = false;

        // Staff Logs
        public double StaffLoginTime = 0;
        public double StaffCounterTime = 0;

        // Taxi WS
        public bool LoadRoomNodes = false;
        public int LastRoomNodeLoaded = 0;
        public bool ViewTaxiList = false;
        public bool CallingTaxi = false;
        public int TaxiNodeGo = -1;
        public int TaxiLastIndex = 1;

        // Tutorial
        public bool InTutorial = false;

        // Gangs
        public bool TurfCapturing = false;
        public int TurfFlagId = 0;

        // ARMERO
        public int ArmPiecesTo = 0;
        public int ArmUserTo = 0;
        // END ARMERO

        // GeneralTimer
        public bool BreakGeneralTimer = false;

        // God
        public bool FirstTickBool = false;
        public int GodModeTicks = 0;
        public bool GodMode = false;
        public bool IsGodMode = false;

        // Passive
        //public bool PassiveMode = false;
        public bool TogglingPSV = false;

        // CarCorp Respawn
        public int CarJobId = 0;
        public int CarJobLastItemId = 0;

        // Check My Corp
        public bool ViewMyCorp = false;
        public int ViewCorpId = 0;
        public bool BuyingCorp = false;

        // HOSPITAL
        // Hosp Herido
        public string HeridaName = null;
        public int RevisPaci = 0;
        public bool Revisado = false;
        public int HeridaPaci = 0;
        //:usarbotiquin
        public int BotiquinDoc = 0;
        public string BotiquinName = null;
        // Ambulancia
        public bool PediMedico = false;
        public bool TargetReanim = false;
        public int HospReanim = 0;
        // END HOSPITAL

        // Weapons
        public bool PediArm = false;

        // Toggles
        public bool DisableRadio = false;

        // Target
        public bool TargetLock = false;
        public string Target = "";

        // Weapons
        public int Bullets = 0;
        public int WLife = 0;

        // Phone Chats
        public int LastChatID = 0;
        public string LastChat = null;
        public bool UpdateChats = false;
        public string LastWhatsChat = null;
        public bool UpdateWhatsChats = false;

        // Mecanic
        public int MecPartsTo = 0;
        public int MecPriceTo = 0;
        public bool IsMecLoading = false;
        public int MecUserToRepair = 0;
        public int MecCarToRepair = 0;
        public int MecNewState = 0;
        public int MecRotPosition = 0;
        public Point MecCordinates;
        public bool PediMec = false;

        // Farming
        public bool WateringCan = false;
        public bool FarmSeeds = false;
        public bool IsFarming = false;
        //public FarmingItem FarmingItem = null;

        // Police Related
        public bool PoliceTrial = false;
        public string WantedFor = "";

        // Misc
        public int TextTimer = 0;
        public int RapeTimer = 0;
        public int KissTimer = 0;
        public int HugTimer = 0;
        public int SexTimer = 0;
        public bool InsideTaxi = false;
        public bool AntiArrowCheck = false;
        public bool BeingHealed = false;
        public bool IsWorkingOut = false;
        public bool InShower = false;
        public bool HighOffCocaine = false;
        public bool HighOffWeed = false;
        public bool DeathInOut = false; // Variable para indicar si murió fuera de un interior. (para confiscar armas al aceptar muerte9.
        //public ConcurrentDictionary<int, List<PollQuestion>> AnsweredPollQuestions;

        // CAMIONERO
        public int CamCargId = 0;
        // Timers
        public bool IsCamLoading = false;
        public bool IsCamUnLoading = false;
        // END CAMIONERO

        // Ladron
        public bool IsForcingHouse = false;
        public House HouseToForce = null;
        public string Object = "";
        public int ObjectPrice = 0;

        // Timers
        public int LoadingTimeLeft = 0;
        public int ATMRobTimeLeft = 180;
        public bool IsRobATM = false;
        public Item ATMRobbinItem = null;

        // Fuel
        public bool IsFuelCharging = false;
        public int FuelChargingCant = 0;

        // Minero
        public bool IsMinerLoading = false;
        public bool MinerRock = false;
        public int MinerRockLvl = 0;

        // Basurero
        public bool IsBasuChofer = false;
        public bool IsBasuPasaj = false;
        public int BasuTeamId = 0;
        public string BasuTeamName = "";
        public int BasuTrashCount = 0;

        //Sistema de Pasajeros Final Jeihden
        public bool Pasajero = false;
        public bool Chofer = false;
        public int ChoferID = 0;
        //Para el chofer
        public int PasajerosCount = 0;
        public string Pasajeros = "";
        public string ChoferName = "";
        public GameClient ChoferClient = null;
        public int Ficha = 0;
        public int FichaTimer = 0;

        // Taxi
        public int TaxiTimeLeft = 0;
        public bool TaxiPaying = false;

        // Vars Positions
        public int LastX = 0;
        public int LastY = 0;

        public int ToX = 0;
        public int ToY = 0;

        //Sistema de Escoltar
        public bool IsEscorted = false;
        public bool WalkNoDiagonal = false;
        public int EscortedID = 0;//Para el poli.
        public string EscortedName = "";
        public int PoliceEscortingID = 0;//Para el preso.
        public string EscortPoliceName = "";
        public bool EscortingWalk = false;

        public bool EscortedRepos = false;

        // Cars
        public bool isParking = false;// Para que deje colocar furni (users sin permisos/sin terreno)
        public int DrivingCarId = 0;
        public int DrivingCarItem = 0;
        public bool DrivingCar = false;
        public bool DrivingInCar = false;// ID en diccionario play_vehicles_owned
        public int CarEnableId = 0;        
        public int CarEffectId = 0;
        public int CarTimer = 0;
        public int CarLife = 0;
        public string CarWSBaul = "";
        public int VehicleTimer = 0;
        /* This not use 'cause the dicctionary was a fucking miracle* /
        public int CarState = 0;
        public int CarTraba = 0;
        public bool CarAlarm = false;
        public int CarId = 0;
        public int CarFurniId = 0;
        public int CarLastOWn = 0;
        public int CarMaxDoors = 0;
        public int CarCarCorp = 0;
        public string CarBaul = "";
        public int CarBaulState = 0;
        public string CarWSBaul = "";
        public string CarModel = "";
        /* */
        // Jobs
        public int JobId = 0;
        public int JobRank = 0;

        public bool IsDisconnecting = false;

        // Internet
        public List<string> InternetHisto = null;
        public string InternetCurPage = "";


        // WebSocket
        public int UserViewing = 0;
        public IWebSocketConnection WebSocketConnection
        {
            get
            {
                if (PlusEnvironment.GetGame().GetWebEventManager() != null)
                    return PlusEnvironment.GetGame().GetWebEventManager().GetUsersConnection(Client);
                else
                    return null;
            }
        }
        /*
        public IWebSocketConnection MusWebSocketConnection
        {
            get
            {
                if (PlusEnvironment.GetGame().GetMusWebEventManager() != null)
                    return PlusEnvironment.GetGame().GetMusWebEventManager().GetUsersConnection(Client);
                else
                    return null;
            }
        }
        */
        public bool GroupRoom = false;
        public bool ViewHouse = false;
        public bool UsingAtm = false;
        public bool InState = false;
        public bool JobRequest = false;
        public bool ViewBaul = false;
        public bool ViewCarList = false;
        public bool ViewProducts = false;
        public bool UsingPhone = false;
        public bool SendMsg = false;
        public string InApp = "";
        public bool SendToName = false;
        public bool ViewShopPhones = false;
        public bool ViewApartments = false;
        public bool ViewCamCargas = false;
        public bool ViewArmeroPieces = false;
        public bool ViewArmeroWeapons = false;
        public bool ViewHospBotiq = false;

        // To Houses WS
        /* We don't need this. We can consult this var info with the HouseManager        
        public int HousePrice = 0;
        public int HouseLevel = 1;
        public bool HouseForSale = false;
        */
        public int HouseSignId = 0;
        public string HouseOwner = "";

        // Houses
        public bool ExitingHouse = false;
        public int HouseX = 0;
        public int HouseY = 0;
        public double HouseZ = 0;

        // Work
        public bool IsWorking = false;

        // Noob
        public bool NoobWarned = false;
        public bool NoobWarned2 = false;

        // Captcha
        public bool CaptchaSent = false;
        public int CaptchaTime = 0;

        #endregion

        #region Getters & Setters
        public int MaxHealth
        {
            get { return mMaxHealth; }
            set { mMaxHealth = value; }
        }
        public int CurHealth
        {
            get { return mCurHealth; }
            set { mCurHealth = value; EventManager.TriggerEvent("OnHealthChange", Client); }
        }
        public int Armor
        {
            get { return mArmor; }
            set { mArmor = value; EventManager.TriggerEvent("OnHealthChange", Client); }
        }
        public int Hunger
        {
            get { return mHunger; }
            set { mHunger = value; EventManager.TriggerEvent("OnHealthChange", Client); }
        }
        public int Level
        {
            get { return mLevel; }
            set { mLevel = value; }
        }
        public int CurXP
        {
            get { return mCurXP; }
            set { mCurXP = value; }
        }
        public int NeedXP
        {
            get { return mNeedXP; }
            set { mNeedXP = value; }
        }
        public int Hygiene
        {
            get { return mHygiene; }
            set { mHygiene = value; }
        }
        public int Strength
        {
            get { return mStrength; }
            set { mStrength = value; }
        }
        public int StrengthEXP
        {
            get { return mStrengthEXP; }
            set { mStrengthEXP = value; }
        }
        public int Bank
        {
            get { return mBank; }
            set { mBank = value; }
        }
        public bool IsDying
        {
            get { return mIsDying; }
            set { mIsDying = value; }
        }
        public int DyingTimeLeft
        {
            get { return mDyingTimeLeft; }
            set { mDyingTimeLeft = value; }
        }
        public bool IsDead
        {
            get { return mIsDead; }
            set { mIsDead = value; }
        }
        public int DeadTimeLeft
        {
            get { return mDeadTimeLeft; }
            set { mDeadTimeLeft = value; }
        }
        public bool IsJailed
        {
            get { return mIsJailed; }
            set { mIsJailed = value; }
        }
        public int JailedTimeLeft
        {
            get { return mJailedTimeLeft; }
            set { mJailedTimeLeft = value; }
        }
        public bool IsSanc
        {
            get { return mIsSanc; }
            set { mIsSanc = value; }
        }
        public int SancTimeLeft
        {
            get { return mSancTimeLeft; }
            set { mSancTimeLeft = value; }
        }
        public int Sancs
        {
            get { return mSancs; }
            set { mSancs = value; }
        }
        public bool IsWanted
        {
            get { return mIsWanted; }
            set { mIsWanted = value; }
        }
        public int WantedLevel
        {
            get { return mWantedLevel; }
            set { mWantedLevel = value; }
        }
        public int WantedTimeLeft
        {
            get { return mWantedTimeLeft; }
            set { mWantedTimeLeft = value; }
        }
        public bool Cuffed
        {
            get { return mCuffed; }
            set { mCuffed = value; }
        }
        public int CuffedTimeLeft
        {
            get { return mCuffedTimeLeft; }
            set { mCuffedTimeLeft = value; }
        }
        public bool ChNDisabled
        {
            get { return mChNDisabled; }
            set { mChNDisabled = value; }
        }
        public bool ChNBanned
        {
            get { return mChNBanned; }
            set { mChNBanned = value; }
        }
        public int ChNBannedTimeLeft
        {
            get { return mChNBannedTimeLeft; }
            set { mChNBannedTimeLeft = value; }
        }
        public string LastCoordinates
        {
            get { return mLastCoordinates; }
            set { mLastCoordinates = value; }
        }
        public bool IsNoob
        {
            get { return mIsNoob; }
            set { mIsNoob = value; }
        }
        public bool PassiveMode
        {
            get { return mPassiveMode; }
            set { mPassiveMode = value; }
        }

        public bool IsNewsReporter
        {
            get { return mIsNewsReporter; }
            set { mIsNewsReporter = value; }
        }

        public int NoobTimeLeft
        {
            get { return mNoobTimeLeft; }
            set { mNoobTimeLeft = value; }
        }

        public int Bidon
        {
            get { return mBidon; }
            set { mBidon = value; }
        }
        public int Phone
        {
            get { return mPhone; }
            set { mPhone = value; }
        }
        public int PhoneModelId
        {
            get { return mPhoneModelId; }
            set { mPhoneModelId = value; }
        }
        public string PhoneNumber
        {
            get { return mPhoneNumber; }
            set { mPhoneNumber = value; }
        }
        public ConcurrentDictionary<string, Weapon> OwnedWeapons
        {
            get { return mOwnedWeapons; }
            set { mOwnedWeapons = value; }
        }
        public ConcurrentDictionary<int, ProductsOwned> OwnedProducts
        {
            get { return mOwnedProducts; }
            set { mOwnedProducts = value; }
        }
        public ConcurrentDictionary<int, PhonesAppsOwned> OwnedPhonesApps
        {
            get { return mOwnedPhonesApps; }
            set { mOwnedPhonesApps = value; }
        }
        /*
        public ConcurrentDictionary<string, Vehicle> OwnedVehicles
        {
            get { return mOwnedVehicles; }
            set { mOwnedVehicles = value; }
        }
        */
        public int CarLimit
        {
            get { return mCarLimit; }
            set { mCarLimit = value; }
        }
        public int CarType
        {
            get { return mCarType; }
            set { mCarType = value; }
        }
        public int CarFuel
        {
            get { return mCarFuel; }
            set { mCarFuel = value; }
        }
        public int CarMaxFuel
        {
            get { return mCarMaxFuel; }
            set { mCarMaxFuel = value; }
        }
        public int TimeWorked
        {
            get { return mTimeWorked; }
            set { mTimeWorked = value; }
        }
        public int Weed
        {
            get { return mWeed; }
            set { mWeed = value; }
        }
        public int Cocaine
        {
            get { return mCocaine; }
            set { mCocaine = value; }
        }
        public int Medicines
        {
            get { return mMedicines; }
            set { mMedicines = value; }
        }
        public int MecParts
        {
            get { return mMecParts; }
            set { mMecParts = value; }
        }
        public int Arrests
        {
            get { return mArrests; }
            set { mArrests = value; }
        }
        public int Punches
        {
            get { return mPunches; }
            set { mPunches = value; }
        }
        public int Kills
        {
            get { return mKills; }
            set { mKills = value; }
        }
        public int HitKills
        {
            get { return mHitKills; }
            set { mHitKills = value; }
        }
        public int GunKills
        {
            get { return mGunKills; }
            set { mGunKills = value; }
        }
        public int CopKills
        {
            get { return mCopKills; }
            set { mCopKills = value; }
        }
        public int Deaths
        {
            get { return mDeaths; }
            set { mDeaths = value; }
        }
        public int CopDeaths
        {
            get { return mCopDeaths; }
            set { mCopDeaths = value; }
        }
        public int Evasions
        {
            get { return mEvasions; }
            set { mEvasions = value; }
        }
        public int CocaineTaken
        {
            get { return mCocaineTaken; }
            set { mCocaineTaken = value; }
        }
        public int MedicinesTaken
        {
            get { return mMedicinesTaken; }
            set { mMedicinesTaken = value; }
        }
        public int WeedTaken
        {
            get { return mWeedtaken; }
            set { mWeedtaken = value; }
        }
        public int GunsFab
        {
            get { return mGunsFab; }
            set { mGunsFab = value; }
        }
        public int TotalShifts
        {
            get { return mTotalShifts; }
            set { mTotalShifts = value; }
        }
        public int MoneyEarned
        {
            get { return mMoneyEarned; }
            set { mMoneyEarned = value; }
        }
        public int PLEarned
        {
            get { return mPLEarned; }
            set { mPLEarned = value; }
        }
        public int DrugsTaken
        {
            get { return mDrugsTaken; }
            set { mDrugsTaken = value; }
        }
        public int TutorialStep
        {
            get { return mTutorialStep; }
            set { mTutorialStep = value; }
        }
        public int ChangeNameCount
        {
            get { return mChangeNameCount; }
            set { mChangeNameCount = value; }
        }
        public int Arrested
        {
            get { return mArrested; }
            set { mArrested = value; }
        }
        public int SendHomeTimeLeft
        {
            get { return mSendHomeTimeLeft; }
            set { mSendHomeTimeLeft = value; }
        }
        public int CamLvl
        {
            get { return mCamLvl; }
            set { mCamLvl = value; }
        }
        public int CamXP
        {
            get { return mCamXP; }
            set { mCamXP = value; }
        }
        public int MinerLvl
        {
            get { return mMinerLvl; }
            set { mMinerLvl = value; }
        }
        public int MinerXP
        {
            get { return mMinerXP; }
            set { mMinerXP = value; }
        }
        public int ArmLvl
        {
            get { return mArmLvl; }
            set { mArmLvl = value; }
        }
        public int ArmXP
        {
            get { return mArmXP; }
            set { mArmXP = value; }
        }
        public int ArmMat
        {
            get { return mArmMat; }
            set { mArmMat = value; }
        }
        public int ArmPieces
        {
            get { return mArmPieces; }
            set { mArmPieces = value; }
        }
        public int MecLvl
        {
            get { return mMecLvl; }
            set { mMecLvl = value; }
        }
        public int MecXP
        {
            get { return mMecXP; }
            set { mMecXP = value; }
        }
        public int BasuLvl
        {
            get { return mBasuLvl; }
            set { mBasuLvl = value; }
        }
        public int BasuXP
        {
            get { return mBasuXP; }
            set { mBasuXP = value; }
        }
        public int LadronLvl
        {
            get { return mLadronLvl; }
            set { mLadronLvl = value; }
        }
        public int LadronXP
        {
            get { return mLadronXP; }
            set { mLadronXP = value; }
        }
        public int Corp
        {
            get { return mCorp; }
            set { mCorp = value; }
        }
        public int Gang
        {
            get { return mGang; }
            set { mGang = value; }
        }
        #endregion

        #region Constructor
        /// <summary>
        /// Constructs the class
        /// </summary>
        public RoleplayUser(GameClient Client, DataRow user, DataRow cooldown)
        {
            // public RoleplayUser(GameClient Client, DataRow user, DataRow cooldown, DataRow farming)

            // Client Info
            this.Client = Client;

            // Basic Info
            this.mId = Convert.ToUInt32(user["id"]);
            
            // Level
            this.mLevel = Convert.ToInt32(user["level"]);
            this.mCurXP = Convert.ToInt32(user["curxp"]);
            this.mNeedXP = Convert.ToInt32(user["needxp"]);

            // Human Needs
            this.mMaxHealth = Convert.ToInt32(user["maxhealth"]);
            this.mCurHealth = Convert.ToInt32(user["curhealth"]);
            this.mArmor = Convert.ToInt32(user["armor"]);
            this.mHunger = Convert.ToInt32(user["hunger"]);
            this.mHygiene = Convert.ToInt32(user["hygiene"]);

            // Jailed - Dead - Wanted - Probation
            this.mIsDying = PlusEnvironment.EnumToBool(user["is_dying"].ToString());
            this.mDyingTimeLeft = Convert.ToInt32(user["dying_time_left"]);
            this.mIsDead = PlusEnvironment.EnumToBool(user["is_dead"].ToString());
            this.mDeadTimeLeft = Convert.ToInt32(user["dead_time_left"]);
            this.mIsJailed = PlusEnvironment.EnumToBool(user["is_jailed"].ToString());
            this.mJailedTimeLeft = Convert.ToInt32(user["jailed_time_left"]);
            this.mIsWanted = PlusEnvironment.EnumToBool(user["is_wanted"].ToString());
            this.mWantedLevel = Convert.ToInt32(user["wanted_level"]);
            this.mWantedTimeLeft = Convert.ToInt32(user["wanted_time_left"]);
            this.mCuffed = PlusEnvironment.EnumToBool(user["is_cuffed"].ToString());
            this.mCuffedTimeLeft = Convert.ToInt32(user["cuffed_time_left"]);
            this.mChNDisabled = PlusEnvironment.EnumToBool(user["chn_disabled"].ToString());
            this.mChNBanned = PlusEnvironment.EnumToBool(user["chn_banned"].ToString());
            this.mChNBannedTimeLeft = Convert.ToInt32(user["chn_banned_time_left"]);
            this.mSendHomeTimeLeft = Convert.ToInt32(user["send_home_time"]);
            this.mIsSanc = PlusEnvironment.EnumToBool(user["is_sanc"].ToString());
            this.mSancTimeLeft = Convert.ToInt32(user["sanc_time_left"]);
            this.mSancs = Convert.ToInt32(user["sancs"]);

            // Misc
            this.mLastCoordinates = user["last_coordinates"].ToString();
            this.mIsNoob = PlusEnvironment.EnumToBool(user["is_noob"].ToString());
            this.mNoobTimeLeft = Convert.ToInt32(user["noob_time_left"]);

            // Levelable Statistics
            this.mStrength = Convert.ToInt32(user["strength"]);
            // Extra Variables for Levelable Stats
            this.mStrengthEXP = Convert.ToInt32(user["strength_exp"]);

            // Banking
            this.mBank = Convert.ToInt32(user["bank"]);

            // Passive Mode
            this.mPassiveMode = PlusEnvironment.EnumToBool(user["passive_mode"].ToString());

            // Passive Mode
            this.mIsNewsReporter = PlusEnvironment.EnumToBool(user["is_news_reporter"].ToString());

            // Manages the offers for the user
            this.OfferManager = new OfferManager(Client);

            // Manages the timers for the user
            this.TimerManager = new TimerManager(Client);

            // Handles the users data
            this.UserDataHandler = new UserDataHandler(Client, this);

            // Manages the cooldowns for the user
            this.CooldownManager = new CooldownManager(Client);
            this.SpecialCooldowns = this.LoadAndReturnCooldowns(cooldown);            

            // Inventory
            this.mWeed = 0;
            this.mCocaine = 0;
            this.mMedicines = 0;
            this.mBidon = 0;
            this.mMecParts = 0;
            this.mArmMat = 0;
            this.mArmPieces = 0;

            // Products
            this.mOwnedProducts = LoadAndReturnProducts();

            // Weapons
            this.mOwnedWeapons = LoadAndReturnWeapons();

            // Phone
            this.mPhone = 0;
            this.mPhoneModelId = 0;
            this.mPhoneNumber = "";

            List<PhonesOwned> PO = PlusEnvironment.GetGame().GetPhonesOwnedManager().getMyPhonesOwned(Convert.ToInt32(user["id"]));
            if (PO != null && PO.Count > 0)
            {
                this.mPhone = PO[0].Id;
                this.mPhoneModelId = Convert.ToInt32(PO[0].PhoneId);
                this.mPhoneNumber = PO[0].PhoneNumber;
            }

            // Phones Apps Owned
            this.mOwnedPhonesApps = LoadAndReturnPhonesApps();

            // Vehicles
            this.mCarLimit = Convert.ToInt32(user["car_limit"]);
            //this.mOwnedVehicles = LoadAndReturnVehicles();

            // Statistics
            this.mTimeWorked = Convert.ToInt32(user["time_worked"]);
            this.mPunches = Convert.ToInt32(user["punches"]);
            this.mKills = Convert.ToInt32(user["kills"]);
            this.mHitKills = Convert.ToInt32(user["hitkills"]);
            this.mGunKills = Convert.ToInt32(user["gunkills"]);
            this.mCopKills = Convert.ToInt32(user["copkills"]);
            this.mDeaths = Convert.ToInt32(user["deaths"]);
            this.mCopDeaths = Convert.ToInt32(user["copdeaths"]);
            this.mArrests = Convert.ToInt32(user["arrests"]);
            this.mArrested = Convert.ToInt32(user["arrested"]);
            this.mEvasions = Convert.ToInt32(user["evasions"]);
            this.mCocaineTaken = Convert.ToInt32(user["cocaine_taken"]);
            this.mMedicinesTaken = Convert.ToInt32(user["medicines_taken"]);
            this.mWeedtaken = Convert.ToInt32(user["weed_taken"]);
            this.mGunsFab = Convert.ToInt32(user["guns_fab"]);
            this.mTotalShifts = Convert.ToInt32(user["total_shifts"]);
            this.mMoneyEarned = Convert.ToInt32(user["money_earned"]);
            this.mPLEarned = Convert.ToInt32(user["pl_earned"]);
            this.mDrugsTaken = Convert.ToInt32(user["drugs_taken"]);
            this.mTutorialStep = Convert.ToInt32(user["tutorial_step"]);

            // Jobs Levels
            //Camionero
            this.mCamLvl = Convert.ToInt32(user["CamLvl"]);
            this.mCamXP = Convert.ToInt32(user["CamXP"]);

            // Minero
            this.mMinerLvl = Convert.ToInt32(user["MinerLvl"]);
            this.mMinerXP = Convert.ToInt32(user["MinerXP"]);

            // Armero
            this.mArmLvl = Convert.ToInt32(user["ArmLvl"]);
            this.mArmXP = Convert.ToInt32(user["ArmXP"]);

            // Mecánico
            this.mMecLvl = Convert.ToInt32(user["MecLvl"]);
            this.mMecXP = Convert.ToInt32(user["MecXP"]);

            // Basurero
            this.mBasuLvl = Convert.ToInt32(user["BasuLvl"]);
            this.mBasuXP = Convert.ToInt32(user["BasuXP"]);

            // Ladron
            this.mLadronLvl = Convert.ToInt32(user["LadronLvl"]);
            this.mLadronXP = Convert.ToInt32(user["LadronXP"]);

            // Groups
            this.mCorp = Convert.ToInt32(user["corp"]);
            this.mGang = Convert.ToInt32(user["gang"]);

            // Change Name
            this.mChangeNameCount = Convert.ToInt32(user["changename_count"]);

        }
        #endregion

        #region Methods
        #region LoadAndReturnVehicles OFF
        /*
        /// <summary>
        /// Loads and returns the owned cars
        /// </summary>
        /// <returns></returns>
        internal ConcurrentDictionary<string, Vehicle> LoadAndReturnVehicles()
        {
            DataTable Weps = null;
            ConcurrentDictionary<string, Vehicle> Vehicles = new ConcurrentDictionary<string, Vehicle>();

            Vehicles.Clear();

            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("SELECT * FROM `play_vehicles_owned` WHERE `owner` = '" + this.mId + "'");
                Weps = dbClient.getTable();
                uint id = 0;

                if (Weps != null)
                {
                    foreach (DataRow Row in Weps.Rows)
                    {
                        id++;

                        int furni_id = Convert.ToInt32(Row["furni_id"]);
                        int item_id = Convert.ToInt32(Row["item_id"]);
                        int owner = Convert.ToInt32(Row["owner"]);
                        int lastuser = Convert.ToInt32(Row["last_user"]);
                        string basename = Convert.ToString(Row["model"]);
                        int fuel = Convert.ToInt32(Row["fuel"]);
                        int km = Convert.ToInt32(Row["km"]);
                        int state = Convert.ToInt32(Row["state"]);
                        int traba = Convert.ToInt32(Row["traba"]);
                        bool alarma = PlusEnvironment.EnumToBool(Convert.ToString(Row["alarm"]));
                        int location = Convert.ToInt32(Row["location"]);
                        int x = Convert.ToInt32(Row["x"]);
                        int y = Convert.ToInt32(Row["y"]);
                        double z = Convert.ToDouble(Row["z"]);
                        string baul = Convert.ToString(Row["baul"]);
                        int baul_state = Convert.ToInt32(Row["baul_state"]);
                        int CamCargID = Convert.ToInt32(Row["CamCargID"]);

                        if (!Vehicles.ContainsKey(basename))
                        {
                            Vehicle BaseVehicle = VehicleManager.getVehicle(basename);

                            //Si tiene un auto que ni siquiera existe en la tienda, lo genera.
                            if (BaseVehicle != null)
                            {
                                Vehicle Vehicle = new Vehicle(id, item_id, "none", 0, 0, basename, "none", 90, 4, 3, 1, 1);

                                if (Vehicle != null)
                                    Vehicles.TryAdd(basename, Vehicle);
                            }
                        }
                    }
                }
            }

            return Vehicles;
        }
        */
        #endregion
        /// <summary>
        /// Loads and returns special cooldowns
        /// </summary>
        /// <param name="Row"></param>
        /// <returns></returns>
        internal ConcurrentDictionary<string, int> LoadAndReturnCooldowns(DataRow Row)
        {
            ConcurrentDictionary<string, int> specialCooldowns = new ConcurrentDictionary<string, int>();

            int RobberyCooldown = Convert.ToInt32(Row["robbery"]);
            specialCooldowns.TryAdd("robbery", RobberyCooldown);

            foreach (var cooldown in specialCooldowns)
            {
                if (cooldown.Value > 0)
                {
                    this.CooldownManager.CreateCooldown(cooldown.Key, 1000, cooldown.Value);
                }
            }

            return specialCooldowns;
        }
        /// <summary>
        /// Returns the cooldown time if cooldown exists
        /// </summary>
        public bool TryGetCooldown(string cooldown, bool sendwhisper = true)
        {
            if (this.CooldownManager != null && this.CooldownManager.ActiveCooldowns != null)
            {
                if (this.CooldownManager.ActiveCooldowns.ContainsKey(cooldown.ToLower()))
                {
                    var CoolDown = this.CooldownManager.ActiveCooldowns[cooldown.ToLower()];

                    if (CoolDown == null)
                        return false;

                    if (this.Client.GetHabbo().VIPRank == 2 && CoolDown.Type.ToLower() == "fist" && CoolDown.Type.ToLower() == "reload")
                        return false;

                    if (sendwhisper && !this.Client.GetPlay().IsEscorted)
                    {
                        if (!cooldown.Equals("cansancio"))
                            this.Client.SendWhisper("Debes esperar [" + (CoolDown.TimeLeft / 1000) + " segs. / " + CoolDown.Amount + " segs.]", 1);
						else
                            this.Client.SendWhisper("Estás demasiado cansad@. Por favor espera un poco [" + (CoolDown.TimeLeft / 1000) + " segs. / " + CoolDown.Amount + " segs.]", 1);
                    }
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Ends all timers/cooldowns and removes all offers
        /// </summary>
        public void EndCycle()
        {
            if (TimerManager != null)
            {
                TimerManager.EndAllTimers();
                TimerManager = null;
            }
            
            if (OfferManager != null)
            {
                OfferManager.EndAllOffers();
                OfferManager = null;
            }
            if (CooldownManager != null)
            {
                CooldownManager.EndAllCooldowns();
                CooldownManager = null;
            }
            
        }

        /// <summary>
        /// Sets stats to max
        /// </summary>
        public void ReplenishStats(bool Bool = false)
        {

            if (CurHealth < MaxHealth)
                CurHealth = MaxHealth;

            if (Hunger > 0)
                Hunger = 0;

            if (Hygiene < 100)
                Hygiene = 100;
            /*
            if (!Bool)
            {
                if (CurEnergy < MaxEnergy)
                    CurEnergy = MaxEnergy;
            }
            */
        }

        /// <summary>
        /// Opens statistic dialogue for target user
        /// </summary>
        /// <param name="Target">User targetting</param>
        public void OpenUsersDialogue(GameClient Target)
        {
            if (Target == null)
                return;

            if (Target != Client)
            {
                if (UserViewing != Target.GetHabbo().Id)
                    UserViewing = Target.GetHabbo().Id;
            }

            PlusEnvironment.GetGame().GetWebEventManager().ExecuteWebEvent(Client, "event_characterbar", "" + Target.GetHabbo().Id);
        }

        /// <summary>
        /// Closes the statistic dialogue of anybody viewing the users one
        /// </summary>
        public void CloseInteractingUserDialogues()
        {
            foreach (GameClient iClient in PlusEnvironment.GetGame().GetClientManager().GetClients.ToList())
            {
                if (iClient == null)
                    continue;

                if (iClient.GetPlay() == null)
                    continue;

                if (iClient.GetPlay().UserViewing != Client.GetHabbo().Id)
                    continue;

                if (iClient.LoggingOut)
                    continue;

                if (iClient.GetPlay().WebSocketConnection == null)
                    continue;

                iClient.GetPlay().ClearWebSocketDialogue();
            }
        }

        /// <summary>
        /// Refreshes statistic dialogue of anybody viewing users one
        /// </summary>
        public void UpdateInteractingUserDialogues()
        {
            foreach (GameClient iClient in PlusEnvironment.GetGame().GetClientManager().GetClients.ToList())
            {
                if (iClient == null)
                    continue;

                if (iClient.GetPlay() == null)
                    continue;

                if (iClient.GetPlay().UserViewing != Client.GetHabbo().Id)
                    continue;

                if (iClient.LoggingOut)
                    continue;

                if (iClient.GetPlay().WebSocketConnection == null)
                    continue;

                PlusEnvironment.GetGame().GetWebEventManager().ExecuteWebEvent(iClient, "event_characterbar", "" + Client.GetHabbo().Id);
            }
        }

        /// <summary>
        /// Refreshes users statistic dialogue
        /// </summary>
        public void RefreshStatDialogue()
        {
            if (WebSocketConnection == null)
                return;

            PlusEnvironment.GetGame().GetWebEventManager().ExecuteWebEvent(Client, "event_retrieveconnectingstatistics", "");
        }

        /// <summary>
        /// Refreshes users statistic dialogue
        /// </summary>
        public void InitStatDialogue()
        {
            if (WebSocketConnection == null)
                return;

            PlusEnvironment.GetGame().GetWebEventManager().ExecuteWebEvent(Client, "event_initstatdialogue", "");
        }

        public void InitWSDialogues()
        {
            if (WebSocketConnection == null)
                return;

            PlusEnvironment.GetGame().GetWebEventManager().ExecuteWebEvent(Client, "event_initwsdialogues", "");
        }

        /// <summary>
        /// Clear websocket dialogues
        /// </summary>
        public void ClearWebSocketDialogue(bool Force = false)
        {
            UserViewing = 0;

            // New Target System
            Client.GetPlay().Target = "";
            Client.GetPlay().TargetLock = false;

            GetUserComponent.ClearStatisticsDialogue(Client);
        }

        public void UpdateTimerDialogue(string timer, string action, int val1, int val2)
        {
            if (WebSocketConnection != null)
                PlusEnvironment.GetGame().GetWebEventManager().ExecuteWebEvent(Client, "event_timerdialogue", "action:" + action + ",timer:" + timer + ",value:" + val1 + "/" + val2 + ",bypass:true");
        }

        /// <summary>
        /// Loads and returns the owned weapons
        /// </summary>
        /// <returns></returns>
        internal ConcurrentDictionary<string, Weapon> LoadAndReturnWeapons()
        {
            DataTable Weps = null;
            ConcurrentDictionary<string, Weapon> Weapons = new ConcurrentDictionary<string, Weapon>();

            Weapons.Clear();

            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("SELECT * FROM `play_weapons_owned` WHERE `user_id` = '" + this.mId + "' AND `baul_car_id` = '0'");
                Weps = dbClient.getTable();
                uint id = 0;

                if (Weps != null)
                {
                    foreach (DataRow Row in Weps.Rows)
                    {
                        id++;

                        string basename = Convert.ToString(Row["base_weapon"]);
                        string name = Convert.ToString(Row["name"]);
                        int mindam = Convert.ToInt32(Row["min_damage"]);
                        int maxdam = Convert.ToInt32(Row["max_damage"]);
                        int range = Convert.ToInt32(Row["range"]);
                        int totalbullets = Convert.ToInt32(Row["bullets"]);
                        bool canuse = PlusEnvironment.EnumToBool(Row["can_use"].ToString());
                        int wlife = Convert.ToInt32(Row["life"]);
                        int baulcar = Convert.ToInt32(Row["baul_car_id"]);

                        if (!Weapons.ContainsKey(basename))
                        {
                            Weapon BaseWeapon = WeaponManager.getWeapon(basename);

                            if (BaseWeapon != null)
                            {
                                Weapon Weapon = new Weapon(id, basename, name, BaseWeapon.FiringText, BaseWeapon.EquipText, BaseWeapon.UnEquipText, BaseWeapon.ReloadText, BaseWeapon.Energy, BaseWeapon.EffectID, BaseWeapon.HandItem, range, mindam, maxdam, BaseWeapon.ClipSize, BaseWeapon.ReloadTime, BaseWeapon.Cost, BaseWeapon.CostFine, BaseWeapon.Stock, BaseWeapon.LevelRequirement, canuse, totalbullets, wlife, baulcar);

                                if (Weapon != null)
                                    Weapons.TryAdd(basename, Weapon);
                            }
                        }
                    }
                }
            }

            return Weapons;
        }

        internal ConcurrentDictionary<int, ProductsOwned> LoadAndReturnProducts()
        {
            DataTable Prods = null;
            ConcurrentDictionary<int, ProductsOwned> Products = new ConcurrentDictionary<int, ProductsOwned>();

            Products.Clear();

            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("SELECT * FROM `play_products_owned` WHERE `user_id` = '" + this.mId + "'");
                Prods = dbClient.getTable();
                int id = 0;

                if (Prods != null)
                {
                    foreach (DataRow Row in Prods.Rows)
                    {
                        id++;
                        int ProductId = Convert.ToInt32(Row["product_id"]);
                        int UserId = Convert.ToInt32(Row["user_id"]);
                        string Extradata = Convert.ToString(Row["extradata"]);

                        if (!Products.ContainsKey(ProductId))
                        {
                            ProductsOwned newPhOwn = new ProductsOwned(id, ProductId, UserId, Extradata);
                            Products.TryAdd(ProductId, newPhOwn);
                        }

                        if (ProductId == RoleplayManager.WeedID)
                            this.Weed = Convert.ToInt32(Extradata);
                        if (ProductId == RoleplayManager.CocaineID)
                            this.Cocaine = Convert.ToInt32(Extradata);
                        if (ProductId == RoleplayManager.MedicinesID)
                            this.Medicines = Convert.ToInt32(Extradata);
                        if (ProductId == RoleplayManager.BidonID)
                            this.Bidon = Convert.ToInt32(Extradata);
                        if (ProductId == RoleplayManager.MecPartsID)
                            this.MecParts = Convert.ToInt32(Extradata);
                        if (ProductId == RoleplayManager.ArmMatID)
                            this.ArmMat = Convert.ToInt32(Extradata);
                        if (ProductId == RoleplayManager.ArmPiecesID)
                            this.ArmPieces = Convert.ToInt32(Extradata);
                    }
                }
            }

            return Products;
        }

        internal ConcurrentDictionary<int, PhonesAppsOwned> LoadAndReturnPhonesApps()
        {
            DataTable Apps = null;
            ConcurrentDictionary<int, PhonesAppsOwned> PhonesApps = new ConcurrentDictionary<int, PhonesAppsOwned>();

            PhonesApps.Clear();

            if (this.mPhone > 0)
            {
                using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
                {
                    dbClient.SetQuery("SELECT * FROM `play_phones_apps_owned` WHERE `phone_id` = '" + this.mPhone + "'");
                    Apps = dbClient.getTable();
                    int id = 0;

                    if (Apps != null)
                    {
                        foreach (DataRow Row in Apps.Rows)
                        {
                            id++;
                            int PhoneId = Convert.ToInt32(Row["phone_id"]);
                            int AppId = Convert.ToInt32(Row["app_id"]);
                            int ScreenId = Convert.ToInt32(Row["screen_id"]);
                            int SlotId = Convert.ToInt32(Row["slot_id"]);
                            string Extradata = Convert.ToString(Row["extradata"]);

                            if (!PhonesApps.ContainsKey(AppId))
                            {
                                PhonesAppsOwned newPhOwn = new PhonesAppsOwned(id, PhoneId, AppId, ScreenId, SlotId, Extradata);
                                PhonesApps.TryAdd(AppId, newPhOwn);
                            }
                        }
                    }
                }
            }

            return PhonesApps;
        }
        #endregion
    }
}