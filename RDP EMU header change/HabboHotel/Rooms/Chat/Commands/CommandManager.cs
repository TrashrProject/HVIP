using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

using Plus.Utilities;
using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.GameClients;
using Plus.HabboRoleplay.Paradise.Commands;

using Plus.HabboHotel.Rooms.Chat.Commands.User;
using Plus.HabboHotel.Rooms.Chat.Commands.User.Fun;
using Plus.HabboHotel.Rooms.Chat.Commands.Moderator;
using Plus.HabboHotel.Rooms.Chat.Commands.Moderator.Fun;
using Plus.HabboHotel.Rooms.Chat.Commands.Administrator;
using Plus.HabboHotel.Rooms.Chat.Commands.Users.Generic.Combat;
using Plus.HabboHotel.Rooms.Chat.Commands.Administrators;
using Plus.Communication.Packets.Outgoing.Rooms.Chat;
using Plus.Communication.Packets.Outgoing.Notifications;
using Plus.Database.Interfaces;
using Plus.HabboHotel.Rooms.Chat.Commands.Events;
using Plus.HabboHotel.Items.Wired;
using Plus.HabboHotel.Rewards.Rooms.Chat.Commands.Administrator;
using Plus.HabboHotel.Rewards.Rooms.Chat.Commands.Moderator.Fun;
using Plus.HabboHotel.Rooms.Chat.Commands.Users.Generic.Interactions.Self;
using Plus.HabboHotel.Rooms.Chat.Commands.Users.Generic.Interactions.Items;
using Plus.HabboHotel.Rooms.Chat.Commands.Developers;
using Plus.HabboHotel.Rooms.Chat.Commands.Users.Jobs.General;
using Plus.HabboHotel.Rooms.Chat.Commands.Users.Jobs.Types.Police;
using Plus.HabboHotel.Rooms.Chat.Commands.Users.Generic.Police;
using Plus.HabboHotel.Rooms.Chat.Commands.Users.Jobs.Types.Restaurant;
using Plus.HabboHotel.Rooms.Chat.Commands.Users.Jobs.Types.Bank;
using Plus.HabboHotel.Rooms.Chat.Commands.Users.Generic.Offers;
using Plus.HabboHotel.Rooms.Chat.Commands.Moderators.Seniors;
using Plus.HabboHotel.Rooms.Chat.Commands.Users.Gangs;

namespace Plus.HabboHotel.Rooms.Chat.Commands
{
    public class CommandManager
    {
        /// <summary>
        /// Command Prefix only applies to custom commands.
        /// </summary>
        private string _prefix = ":";

        /// <summary>
        /// Commands registered for use.
        /// </summary>
        private readonly Dictionary<string, IChatCommand> _commands;
        private readonly Dictionary<string, IChatCommand> _staffcommands;
        private readonly Dictionary<string, IChatCommand> _policecommands;
        private readonly Dictionary<string, IChatCommand> _loggedcommands;

        /// <summary>
        /// The default initializer for the CommandManager
        /// </summary>
        public CommandManager(string Prefix)
        {
            this._prefix = Prefix;
            this._commands = new Dictionary<string, IChatCommand>();
            this._staffcommands = new Dictionary<string, IChatCommand>();
            this._loggedcommands = new Dictionary<string, IChatCommand>();
            this._policecommands = new Dictionary<string, IChatCommand>();

            this.RegisterUser();
            this.RegisterVIP();
            this.RegisterEvents();
            this.RegisterModerator();
            this.RegisterAdministrator();
            this.RegisterDeveloper();

            // Keep the legacy registry as the single command source-of-truth;
            // ParadiseRP commands are appended to that existing registry.
            ParadiseCommandBootstrap.Register(this);
        }

        /// <summary>
        /// Request the text to parse and check for commands that need to be executed.
        /// </summary>
        /// <param name="Session">Session calling this method.</param>
        /// <param name="Message">The message to parse.</param>
        /// <returns>True if parsed or false if not.</returns>
        public bool Parse(GameClient Session, string Message)
        {
            if (Session == null || Session.GetHabbo() == null || Session.GetHabbo().CurrentRoom == null)
                return false;

            if (!Message.StartsWith(_prefix))
                return false;

            if (Message == _prefix + "commands" || Message == _prefix + "comandos")
            {
                // Enviamos WS de ventana de comandos.
                PlusEnvironment.GetGame().GetWebEventManager().ExecuteWebEvent(Session, "event_commands", "open");
                return true;
            }
            /* OLD
            if(Message == _prefix + "comandos")
            {
                if (Session.GetHabbo().Rank > 2)
                {
                   Session.SendMessage(new NuxAlertMessageComposer("habbopages/chat/commandsStaff.txt"));
                }
                else
                {
                    Session.SendMessage(new NuxAlertMessageComposer("habbopages/chat/test.txt"));
                }
                return true;
            }
            */


            Message = Message.Substring(1);
            string[] Split = Message.Split(' ');

            if (Split.Length == 0)
                return false;

            IChatCommand Cmd = null;
            IChatCommand LogCmd = null;
            if (_commands.TryGetValue(Split[0].ToLower(), out Cmd))
            {
                if (Session.GetHabbo().GetPermissions().HasRight("mod_tool") || (Session.GetHabbo().Rank > 2))
                    this.LogCommand(Session.GetHabbo().Id, Message, Session.GetHabbo().MachineId);

                _commands.TryGetValue(Split[0].ToLower(), out LogCmd);

                if (Cmd == LogCmd)
                {
                    if (_staffcommands.ContainsKey(Split[0].ToLower()))
                        this.LogCommand(Session.GetHabbo().Id, Message, Session.GetHabbo().MachineId, "staff");
                    else if (_policecommands.ContainsKey(Split[0].ToLower()))
                        this.LogCommand(Session.GetHabbo().Id, Message, Session.GetHabbo().MachineId, "police");
                    else if (_loggedcommands.ContainsKey(Split[0].ToLower()))
                        this.LogCommand(Session.GetHabbo().Id, Message, Session.GetHabbo().MachineId, "logged");
                }

                if (!string.IsNullOrEmpty(Cmd.PermissionRequired))
                {
                    if (!Session.GetHabbo().GetPermissions().HasCommand(Cmd.PermissionRequired))
                    {
                        // Special exceptions
                        if (!((Cmd.PermissionRequired.Equals("command_give") || Cmd.PermissionRequired.Equals("command_give_room")) && Session.GetHabbo().Id == 9447)) // MegaDude
                            return false;
                    }
                }


                Session.GetHabbo().IChatCommand = Cmd;
                Session.GetHabbo().CurrentRoom.GetWired().TriggerEvent(WiredBoxType.TriggerUserSaysCommand, Session.GetHabbo(), this);

                Cmd.Execute(Session, Session.GetHabbo().CurrentRoom, Split);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Registers the default set of commands.
        /// </summary>
        private void RegisterUser()
        {
            #region GENERIC COMMANDS
            this.Register("ayuda", new HelpCommand());
            //this.Register("about", new InfoCommand());
            this.Register("convertcredits", new ConvertCreditsCommand());
            this.Register("ejectall", new EjectAllCommand());
            this.Register("emoji", new EmojiCommand());
            this.Register("empty", new EmptyItems());
            this.Register("ons", new OnlinesCommand());
            this.Register("onlines", new OnlinesCommand());
            this.Register("conectados", new OnlinesCommand());
            this.Register("togglefriends", new EnableFriendsCommand());
            //this.Register("disablefriends", new DisableFriendsCommand()); Comando repetido con togglefriends
            this.Register("togglewhispers", new DisableWhispersCommand());// (Toggle)
            this.Register("mutepets", new MutePetsCommand());
            this.Register("mutebots", new MuteBotsCommand());
            this.Register("pickall", new PickAllCommand());
            this.Register("stats", new StatsCommand());
            this.Register("est", new StatsCommand());
            this.Register("salas", new ActiveRoomsCommand());
            this.Register("zonas", new ActiveRoomsCommand());
            this.Register("lugares", new ActiveRoomsCommand());
            this.Register("versalas", new ActiveRoomsCommand());
            this.Register("verzonas", new ActiveRoomsCommand());
            this.Register("togglegifts", new DisableGiftsCommand());
            this.Register("closedice", new CloseDiceCommand());
        #endregion

            #region ROLEPLAY COMMANDS

            #region Acciones
            this.Register("abrazar", new HugCommand());
            this.Register("abrazo", new HugCommand());
            this.Register("hug", new HugCommand());
            this.Register("besar", new KissCommand());
            this.Register("beso", new KissCommand());
            this.Register("bofetada", new SlapCommand());
            this.Register("violar", new RapeCommand());
            this.Register("escupir", new SpitCommand());
            this.Register("oral", new OralCommand());
            this.Register("dance", new DanceCommand());
            this.Register("lay", new LayCommand());
            this.Register("sit", new SitCommand());
            this.Register("stand", new StandCommand());
            #endregion

            #region Canales
            this.Register("y", new YCommand());
            this.Register("p", new PCommand());
            this.Register("b", new BCommand());
            this.Register("n", new NCommand());
            #endregion

            #region All Generic
            this.Register("inventario", new InventoryCommand());
            this.Register("inv", new InventoryCommand());
            this.Register("aceptarmuerte", new AcceptDeathCommand());
            this.Register("buscados", new WantedListCommand());
            this.Register("fianza", new BailCommand());
            this.Register("911", new Call911Command());
            this.Register("mapa", new MapCommand());
            this.Register("map", new MapCommand());
            this.Register("servicio", new ServiceCommand());
            this.Register("chn", new ToggleChNCommand());            
            this.Register("tirar", new DropCommand());
            this.Register("consumir", new ConsumirCommand());
            #endregion

            #region Item Iteractions
            this.Register("comer", new EatCommand());
            this.Register("beber", new DrinkCommand());
            this.Register("tomar", new DrinkCommand());
            this.Register("apostar", new GambleCommand());
            this.Register("ejercitarse", new WorkoutCommand());
            #endregion

            #region Combat
            this.Register("golpear", new HitCommand());
            this.Register("hit", new HitCommand());
            this.Register("equipar", new EquipCommand());
            this.Register("desequipar", new UnEquipCommand());
            this.Register("ocupar", new EquipCommand());
            this.Register("desocupar", new UnEquipCommand());
            this.Register("disparar", new ShootCommand());
            this.Register("shoot", new ShootCommand());
            this.Register("recargar", new ReloadGunCommand());
            #endregion

            #region Offers/Sells/Shops Revisar funcionamientos PENDIENTE
            this.Register("dar", new GiveCommand(), "logged");
            this.Register("pagar", new GiveCommand(), "logged");
            this.Register("ofertas", new OffersCommand());
            this.Register("ofrecer", new SellCommand(), "logged");// Old OfferCommand            
            this.Register("aceptar", new AcceptCommand(), "logged");
            this.Register("rechazar", new DeclineCommand());
            this.Register("vender", new SellCommand(), "logged");
            this.Register("comprar", new BuyCommand());
            #endregion

            #region Phones
            this.Register("minumero", new MyNumberCommand());
            this.Register("sms", new SmsCommand());
            this.Register("whats", new WhatsCommand());
            #endregion            

            #region Vehicles
            this.Register("arrancar", new DrivingCommand());
            this.Register("encender", new DrivingCommand());
            this.Register("conducir", new DrivingCommand());
            this.Register("manejar", new DrivingCommand());

            this.Register("detener", new ParkingCommand());
            this.Register("estacionar", new ParkingCommand());
            this.Register("parquear", new ParkingCommand());
            this.Register("apagar", new ParkingCommand());

            this.Register("subir", new UpCommand());
            this.Register("bajar", new DownCommand());
            this.Register("abrir", new OpenCommand());
            this.Register("cerrar", new CloseCommand());
            this.Register("localizar", new LocalizarCommand());
            this.Register("misautos", new MyCarsCommand());
            this.Register("autos", new MyCarsCommand());

            // En revisión (BETA PENDIENTE)
            this.Register("baul", new BaulCommand(), "logged");
            this.Register("maletero", new BaulCommand(), "logged");
            // END En revisión (BETA PENDIENTE)

            this.Register("usarbidon", new UseBidonCommand());
            this.Register("combustible", new BuyFuelCommand());
            this.Register("llenartanque", new BuyFuelFillCommand());

            // :luces
            // :alarma
            #endregion

            #region Houses
            //:abrir casa => [Vehicles]
            //:cerrar casa => [Vehicles]
            this.Register("entrar", new EnterCommand());
            this.Register("salir", new ExitCommand());

            // Básicos
            this.Register("unload", new UnloadCommand());
            this.Register("fixroom", new RegenMaps());
            this.Register("setmax", new SetMaxCommand());
            this.Register("setspeed", new SetSpeedCommand());
            this.Register("kickpets", new KickPetsCommand());
            this.Register("kickbots", new KickBotsCommand());
            this.Register("roomext", new RoomExtCommand());
            this.Register("hidewired", new HideWiredCommand());
            #endregion

            #region Apartments

            #endregion

            #region Gangs
            this.Register("abandonar", new LeaveGangCommand());
            #endregion

            #region Jobs
            #region General
            this.Register("trabajar", new StartWorkCommand());
            this.Register("notrabajar", new StopWorkCommand());
            this.Register("renunciar", new LeaveWorkCommand());
            // Secondary Jobs
            this.Register("uniforme", new UniformCommand());
            this.Register("skills", new SkillsCommand());
            this.Register("habilidad", new SkillsCommand());
            this.Register("habilidades", new SkillsCommand());
            #endregion

            #region Basurero
            this.Register("descargarcamion", new ReturnBasuCommand());
            #endregion

            #region Cosechador
            this.Register("sembrar", new SembrarCommand());
            this.Register("regar", new RegarCommand());
            this.Register("cosechar", new CosecharCommand());
            #endregion

            #region Mecanico
            this.Register("reparar", new SellCommand());
            this.Register("revisar", new ReviewMecCommand());
            #endregion

            #region Armero
            // :comprar materiales [v/]
            // :crear piezas [v/]
            // :crear [arma] [v/]
            // :vender [x] [arma] [precio] [v/]
            // :revisar [x] [v/]
            // :reparar [x] [v/]
            this.Register("crear", new CreateCommand());
            #endregion

            #region Hospital
            //In Hospital
            this.Register("revisarpaciente", new ReviewPatientCommand());
            this.Register("verbotiquin", new ViewBotiquCommand());
            this.Register("usarbotiquin", new UseBotiquCommand());
            this.Register("atenderpaciente", new AttendPatientCommand());
            //Out Hospital
            this.Register("reanimar", new ReanimCommand());
            this.Register("subirpaciente", new UpPatientCommand());
            this.Register("salvar", new SavePatientCommand());
            #endregion

            #region Taxista (Death, thanks Taxi Bot)
            //this.Register("ficha", new FichaCommand());
            //this.Register("tarifa", new FichaCommand());
            #endregion

            #region Camionero
            this.Register("cargas", new LoadsCamCommand());
            this.Register("cargarcamion", new CargarCamCommand());
            this.Register("depositarcarga", new DepositCamCommand());
            this.Register("entregarcamion", new ReturnCamCommand());
            this.Register("abandonarcarga", new LeaveCamCommand());
            #endregion

            #region Ladron
            this.Register("forzarcerradura", new ForceCommand());
            this.Register("robar", new RobCommand());
            // :vender objeto => [Offers/Sells/Shops]
            #endregion

            #region Guardaespaldas
            this.Register("cubrir", new CubrirCommand());
            #endregion

            #region Minero
            this.Register("picar", new PickCommand());
            this.Register("dejarroca", new LeaveRockCommand());
            #endregion

            #region Police
            this.Register("cargos", new LawCommand(), "police");
            this.Register("nocargos", new UnLawCommand(), "police");
            this.Register("paralizar", new StunCommand(), "police");
            this.Register("desparalizar", new UnStunCommand(), "police");
            this.Register("cateo", new SearchCommand(), "police");
            this.Register("catear", new SearchCommand(), "police");
            this.Register("esposar", new CuffCommand(), "police");
            this.Register("desesposar", new UnCuffCommand(), "police");
            this.Register("escoltar", new EscortedCommand(), "police");
            this.Register("noescoltar", new UnEscortedCommand(), "police");
            this.Register("soltar", new UnEscortedCommand(), "police");
            this.Register("desescoltar", new UnEscortedCommand(), "police");
            this.Register("arrestar", new ArrestCommand(), "police");
            this.Register("encarcelar", new ArrestCommand(), "police");
            this.Register("liberar", new ReleaseCommand(), "police");
            //this.Register("juicio", new PoliceTrialCommand(), "police");// OJO
            this.Register("limpiarlista", new ClearWantedCommand(), "police");
            this.Register("refuerzos", new BackupCommand(), "police");
            this.Register("ref", new BackupCommand(), "police");
            this.Register("carinfo", new CheckCarInfoCommand(), "police");
            this.Register("infocar", new CheckCarInfoCommand(), "police");
            this.Register("radio", new RadioAlertCommand());
            this.Register("r", new RadioAlertCommand());
            this.Register("tradio", new ToggleRadioAlertCommand());
            this.Register("toggleradio", new ToggleRadioAlertCommand());
            #endregion            

            #region Restaurant & Cafe
            this.Register("servir", new ServeCommand());
            this.Register("ordenar", new OrderCommand());
            #endregion 
            #endregion
            #endregion
        }

        /// <summary>
        /// Registers the VIP set of commands.
        /// </summary>
        private void RegisterVIP()
        {
            this.Register("faceless", new FacelessCommand());
            this.Register("mimic", new MimicCommand());
            this.Register("togglemimic", new DisableMimicCommand());
            this.Register("moonwalk", new MoonwalkCommand());
            this.Register("push", new PushCommand());
            this.Register("pull", new PullCommand());           
        }

        /// <summary>
        /// Registers the Events set of commands.
        /// </summary>
        private void RegisterEvents()
        {
            this.Register("eha", new EventAlertCommand());
            this.Register("eventha", new EventAlertCommand());
            this.Register("event", new EventAlertCommand());
            this.Register("publicidad", new PublicityAlertCommand());
            //this.Register("da2", new DiceAlertCommand()); - Alerta estilo :event para dados (No necesario RP).
            //this.Register("addevent", new Addevent()); - OFF
            this.Register("spush", new SuperPushCommand());
            this.Register("spull", new SuperPullCommand());
            this.Register("banchn", new ChNBanCommand());
            this.Register("unbanchn", new ChNUnBanCommand());
        }

        /// <summary>
        /// Registers the moderator set of commands.
        /// </summary>
        private void RegisterModerator()
        {           
            this.Register("ban", new BanCommand(), "staff");
            this.Register("ipban", new IPBanCommand(), "staff");
            this.Register("mip", new MIPCommand(), "staff");
            this.Register("ui", new UserInfoCommand());
            this.Register("userinfo", new UserInfoCommand());
            this.Register("mute", new MuteCommand(), "staff");
            this.Register("unmute", new UnmuteCommand(), "staff");
            this.Register("roommute", new RoomMuteCommand(), "staff");
            this.Register("roomunmute", new RoomUnmuteCommand(), "staff");            
            this.Register("roomalert", new RoomAlertCommand(), "staff");
            this.Register("alert", new AlertCommand(), "staff");
            this.Register("sa", new StaffAlertCommand(), "staff");
            this.Register("ha", new HotelAlertCommand(), "staff");
            this.Register("hal", new HALCommand(), "staff");
            this.Register("notification", new NotificationCommand(), "staff");
            this.Register("dc", new DisconnectCommand(), "staff");
            this.Register("disconnect", new DisconnectCommand(), "staff");
            this.Register("tradeban", new TradeBanCommand(), "staff");// (Toggle) Ban/Unban
            this.Register("teleport", new TeleportCommand());// (Toggle)
            this.Register("tele", new TeleportCommand());// (Toggle)
            this.Register("summon", new SummonCommand(), "staff");
            this.Register("override", new OverrideCommand());// (Toggle)
            this.Register("freeze", new FreezeCommand(), "staff");
            this.Register("unfreeze", new UnFreezeCommand(), "staff");
            this.Register("fastwalk", new FastwalkCommand());// (Toggle)
            this.Register("run", new SuperFastwalkCommand());
            this.Register("coords", new CoordsCommand());
            this.Register("alleyesonme", new AllEyesOnMeCommand(), "staff");
            this.Register("allaroundme", new AllAroundMeCommand(), "staff");
            this.Register("forcesit", new ForceSitCommand(), "staff");
            this.Register("ignorewhispers", new IgnoreWhispersCommand());// (Toggle)
            this.Register("forcedfx", new DisableForcedFXCommand());// (Toggle)
            this.Register("makesay", new MakeSayCommand(), "staff");
            this.Register("follow", new FollowCommand(), "staff");
            this.Register("flagme", new FlagMeCommand(), "staff");
            this.Register("flaguser", new FlagUserCommand(), "staff");
            this.Register("createpoll", new CreatePollCommand(), "staff");
            this.Register("endpoll", new EndPollCommand(), "staff");
            this.Register("god", new GodCommand());
            this.Register("dios", new GodCommand());
        }

        /// <summary>
        /// Registers the administrator set of commands.
        /// </summary>
        private void RegisterAdministrator()
        {
            this.Register("emptyuser", new EmptyUser(), "staff");
            this.Register("goto", new GOTOCommand());
            this.Register("ir", new GOTOCommand());
            this.Register("roomids", new RoomIdsCommand());
                        
            this.Register("update", new UpdateCommand(), "staff");
            this.Register("superhire", new SuperHireCommand(), "staff");
            this.Register("givestats", new GiveStatsCommand(), "staff");
            this.Register("givestat", new GiveStatsCommand(), "staff");

            this.Register("kill", new KillCommand(), "staff");

            this.Register("hhh", new HHHCommand(), "staff");
            this.Register("heal", new HHHCommand(), "staff");
            this.Register("roomheal", new RoomHealCommand(), "staff");
           
            this.Register("restore", new RestoreCommand(), "staff");
            this.Register("roomrestore", new RoomRestoreCommand(), "staff");

            this.Register("adminrelease", new AdminReleaseCommand(), "staff");
            this.Register("release", new AdminReleaseCommand(), "staff");
            this.Register("roomrelease", new RoomReleaseCommand(), "staff");

            this.Register("adminjail", new AdminJailCommand(), "staff");
            this.Register("purge", new PurgeCommand(), "staff");
            this.Register("purga", new PurgeCommand(), "staff");
            this.Register("sanc", new SancCommand(), "staff");
            this.Register("unsanc", new UnSancCommand(), "staff");
            this.Register("reporter", new NewsReporterToggle(), "staff");
            this.Register("reporters", new NewsReporterList(), "staff");
        }

        /// <summary>
        /// Registers the developer set of commands.
        /// </summary>
        private void RegisterDeveloper()
        {
            this.Register("massgive", new MassGiveCommand(), "staff");
            this.Register("globalgive", new GlobalGiveCommand(), "staff");
            this.Register("give", new GiveAdmCommand(), "staff");
            this.Register("giverank", new GiveRankCommand(), "staff");
            this.Register("roomcredits", new GiveRoom(), "staff");
            this.Register("reload", new ReloadServerCommand(), "staff");

            this.Register("deletegroup", new DeleteGroupCommand());
            this.Register("test", new TestEventCommand());
            this.Register("color", new ColourCommand());
            this.Register("colornombre", new ChatHTMLColorCommand());
            this.Register("sizenombre", new ChatHTMLSizeCommand());
            this.Register("bubble", new BubbleCommand());
            this.Register("enable", new EnableCommand());
            this.Register("handitem", new CarryCommand());
            this.Register("togglediagonal", new DisableDiagonalCommand());// (Toggle)
            this.Register("massenable", new MassEnableCommand());
            this.Register("massdance", new MassDanceCommand());
            this.Register("disco", new DiscoCommand());
            this.Register("pet", new PetCommand());
            this.Register("casino", new CasinoCommand());          
            this.Register("dnd", new DNDCommand());// (Toggle) Desactivar consola  
            this.Register("room", new RoomCommand());
            this.Register("roombadge", new RoomBadgeCommand());//Solo para Logros
            this.Register("massbadge", new MassBadgeCommand());//Solo para Logros
            this.Register("givebadge", new GiveBadgeCommand());//Solo para Logros
            this.Register("takebadge", new TakeBadgeCommand());//Solo para Logros
            this.Register("roomkick", new RoomKickCommand()); // Evitar sacar users de salas.
            this.Register("kick", new KickCommand());// Evitar sacar users de salas.  

            // RP Listos para asignar a rango
            //this.Register("superhire", new SuperHireCommand(), "staff");

            // Adaptar a RP (No listos)
            //this.Register("kill", new KillCommand());
            this.Register("smokeweed", new SmokeWeedCommand());

            // No usamos este sistema de compra/venta de salas
            //this.Register("vendersala", new SellRoomCommand());            
            //this.Register("comprarsala", new BuyRoomCommand());  
        }

        /// <summary>
        /// Registers a Chat Command.
        /// </summary>
        /// <param name="CommandText">Text to type for this command.</param>
        /// <param name="Command">The command to execute.</param>
        public void Register(string CommandText, IChatCommand Command)
        {
            this._commands.Add(CommandText, Command);
        }

        public void Register(string CommandText, IChatCommand Command, string Type = "")
        {
            switch (Type.ToLower())
            {
                case "staff":
                    {
                        this._commands.Add(CommandText, Command);
                        this._staffcommands.Add(CommandText, Command);
                        break;
                    }
                case "police":
                    {
                        this._commands.Add(CommandText, Command);
                        this._policecommands.Add(CommandText, Command);
                        break;
                    }
                case "logged":
                    {
                        this._commands.Add(CommandText, Command);
                        this._loggedcommands.Add(CommandText, Command);
                        break;
                    }
                default:
                    {
                        this._commands.Add(CommandText, Command);
                        break;
                    }
            }
        }

        public static string MergeParams(string[] Params, int Start)
        {
            var Merged = new StringBuilder();
            for (int i = Start; i < Params.Length; i++)
            {
                if (i > Start)
                    Merged.Append(" ");
                Merged.Append(Params[i]);
            }

            return Merged.ToString();
        }

        public void LogCommand(int UserId, string Data, string MachineId)
        {
            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("INSERT INTO `logs_client_staff` (`user_id`,`data_string`,`machine_id`, `timestamp`) VALUES (@UserId,@Data,@MachineId,@Timestamp)");
                dbClient.AddParameter("UserId", UserId);
                dbClient.AddParameter("Data", Data);
                dbClient.AddParameter("MachineId", MachineId);
                dbClient.AddParameter("Timestamp", PlusEnvironment.GetUnixTimestamp());
                dbClient.RunQuery();
            }
        }

        public void LogCommand(int UserId, string Data, string MachineId, string Type)
        {
            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                if (Type.ToLower() == "staff")
                    dbClient.SetQuery("INSERT INTO `command_logs_staff` (`user_id`,`data_string`,`machine_id`, `timestamp`) VALUES (@UserId,@Data,@MachineId,@Timestamp)");
                else if (Type.ToLower() == "police")
                    dbClient.SetQuery("INSERT INTO `command_logs_police` (`user_id`,`data_string`,`machine_id`, `timestamp`) VALUES (@UserId,@Data,@MachineId,@Timestamp)");
                else if (Type.ToLower() == "logged")
                    dbClient.SetQuery("INSERT INTO `command_logs` (`user_id`,`data_string`,`machine_id`,`timestamp`) VALUES (@UserId,@Data,@MachineId,@Timestamp)");
                dbClient.AddParameter("UserId", UserId);
                dbClient.AddParameter("Data", Data);
                dbClient.AddParameter("MachineId", MachineId);
                dbClient.AddParameter("Timestamp", PlusEnvironment.GetUnixTimestamp());
                dbClient.RunQuery();
            }
        }

        public bool TryGetCommand(string Command, out IChatCommand IChatCommand)
        {
            return this._commands.TryGetValue(Command, out IChatCommand);
        }

        // NEW
        public static string GenerateRainbowText(string Name)
        {
            StringBuilder NewName = new StringBuilder();

            string[] Colours = { "FF0000", "FFA500", "FFFF00", "008000", "0000FF", "800080" };

            int Count = 0;
            int Count2 = 0;
            while (Count < Name.Length)
            {
                NewName.Append("<font color='#" + Colours[Count2] + "'>" + Name[Count] + "</font>");

                Count++;
                Count2++;

                if (Count2 >= 6)
                    Count2 = 0;
            }

            return NewName.ToString();
        }
    }
}