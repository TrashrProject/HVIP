using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Plus.HabboHotel.Users;
using Fleck;
using Plus.HabboHotel.Items;
using Plus.HabboHotel.GameClients;
using System.IO;
using Plus.HabboRoleplay.Misc;
using Plus.HabboRoleplay.Houses;
using System.Collections.Generic;
using Plus.HabboRoleplay.Apartments;
using Plus.HabboHotel.Rooms;
using Plus.HabboRoleplay.ApartmentsOwned;
using Plus.HabboHotel.Groups;
using Plus.Communication.Packets.Outgoing.Groups;
using System.Resources;
using System.Data;
using Plus.Database.Interfaces;
using Plus.HabboRoleplay.GangTurfs;

namespace Plus.HabboHotel.Roleplay.Web.Outgoing.Misc
{
    /// <summary>|
    /// GangsWebEvent class.
    /// </summary>
    class GangsWebEvent : IWebEvent
    {
        /// <summary>
        /// Executes socket data.
        /// </summary>
        /// <param name="Client"></param>
        /// <param name="Data"></param>
        /// <param name="Socket"></param>
        public void Execute(GameClient Client, string Data, IWebSocketConnection Socket)
        {

            if (!PlusEnvironment.GetGame().GetWebEventManager().SocketReady(Client, true) || !PlusEnvironment.GetGame().GetWebEventManager().SocketReady(Socket))
                return;

            string Action = (Data.Contains(',') ? Data.Split(',')[0] : Data);

            switch (Action)
            {
                #region Open
                case "open":
                    {
                        // Gang List & Stats
                        string html = "", tabs = "";
                        string HasGang = "False";

                        List<Group> Groups = PlusEnvironment.GetGame().GetGroupManager().GetGangsForUser(Client.GetHabbo().Id);
                        if (Groups != null && Groups.Count > 0)
                            HasGang = "True";

                        string ButtonText = "Crear banda ($ "+ String.Format("{0:N0}", RoleplayManager.GangsPrice) +")";

                        #region HTML
                        html += "<div class=\"heading\">Lista de Bandas</div>";

                        html += "<input id=\"GA_Search\" type=\"text\" data-lpignore=\"true\" class=\"dark-input-text flex-1 mr-1\" value=\"\" maxlength=\"50\" autocomplete=\"off\" placeholder=\"Buscar bandas por nombre\" style=\"width: 628px\">";
                        html += "<br><br>";

                        html += "<div id=\"GA_List\" class=\"-m-1 flex flex-wrap\">";
                        foreach (Group group in PlusEnvironment.GetGame().GetGroupManager().Groups.ToList())
                        {
                            if (group.GType != 3)
                                continue;

                            html += "<div class=\"flex bg-dark-1 m-1 rounded-lg p-1 pl-2 items-center\" style=\"width: 45.5%;\">";
                            html += "<div class=\"mr-2\">";
                            html += "<p>"+group.Name+"</p>";
                            html += "<p>"+group.GetAllMembers.Count()+" miembro(s)</p>";
                            html += "</div>";
                            html += "<div class=\"bg-dark-2 px-2 py-1 rounded-lg ml-auto data-gang\" data-balloon=\"Ver info\" data-balloon-pos=\"left\" data-gang=\"" + group.Id + "\"><img src=\"https://swf.habbovip.us/group-badge/badge/"+ group.GetBadge() + "\" draggable=\"false\" ondragstart=\"return false;\" style=\"cursor: pointer;\"></div>";
                            html += "</div>";
                        }
                        html += "</div>";
                        #endregion

                        #region TABS
                        if (HasGang == "True")
                        {
                            tabs += "<div id=\"GA_My_Members\" class=\"Tabbed_tab_1apzZ GA_My_Members Tabbed_selected_3aJyT\" style=\"min-width: 75px;\">";
                            tabs += "Miembros";
                            tabs += "</div>";

                            if (Groups[0].IsAdmin(Client.GetHabbo().Id) || Client.GetHabbo().GetPermissions().HasRight("group_management_override"))
                            {
                                tabs += "<div id=\"GA_My_Ranks\" class=\"Tabbed_tab_1apzZ GA_My_Ranks\" style=\"min-width: 75px;\">";
                                tabs += "Rangos";
                                tabs += "</div>";
                            }

                            if (((!Groups[0].IsAdmin(Client.GetHabbo().Id) || !Groups[0].IsMember(Client.GetHabbo().Id)) && !PlusEnvironment.GetGame().GetGroupManager().HasJobCommand(Client, "hire", true)) && !Client.GetHabbo().GetPermissions().HasRight("corporation_rights")) { }
                            else
                            {
                                tabs += "<div id=\"GA_My_Requests\" class=\"Tabbed_tab_1apzZ GA_My_Requests\" style=\"min-width: 75px;\">";
                                tabs += "Solicitudes";
                                tabs += "</div>";
                            }

                            tabs += "<div id=\"GA_My_Stats\" class=\"Tabbed_tab_1apzZ GA_My_Stats\" style=\"min-width: 75px;\">";
                            tabs += "Estad&iacute;sticas";
                            tabs += "</div>";

                            if (Groups[0].IsAdmin(Client.GetHabbo().Id) || Client.GetHabbo().GetPermissions().HasRight("group_management_override"))
                            {
                                tabs += "<div id=\"GA_My_Edit\" class=\"Tabbed_tab_1apzZ GA_My_Edit\" style=\"min-width: 75px;\">";
                                tabs += "Editar";
                                tabs += "</div>";
                            }
                        }
                        #endregion

                        Socket.Send("compose_gang|open|" + html + "|" + HasGang + "|" + ButtonText + "|" + tabs);
                    }
                    break;
                #endregion

                #region Close
                case "close":
                    {
                        Socket.Send("compose_gang|close");
                    }
                    break;
                #endregion

                #region My / New
                case "mynew":
                    {
                        bool HasGang = false;

                        List<Group> Groups = PlusEnvironment.GetGame().GetGroupManager().GetGangsForUser(Client.GetHabbo().Id);
                        if (Groups != null && Groups.Count > 0)
                            HasGang = true;

                        if(!HasGang)
                            Socket.Send("compose_gang|new_gang|");
                        else
                        {
                            bool isAdmin = false;
                            string html = "", tabs = "";

                            #region Check if is Admin
                            if (Groups[0].IsAdmin(Client.GetHabbo().Id) || Client.GetHabbo().GetPermissions().HasRight("group_management_override"))
                                isAdmin = true;
                            #endregion

                            #region HTML
                            html += "<div>";
                            html += "<div class=\"-m-2\">";

                            var AllRanks = Groups[0].Ranks.OrderBy(o => o.Value.RankId).ToList();
                            AllRanks.Reverse();
                            foreach (var Ranks in AllRanks)
                            {
                                //<!-- Rank box -->
                                html += "<div class=\"m-2\">";
                                //<!-- Rank Header -->
                                html += "<div class=\"heading relative group\">";
                                html += "<div>" + Ranks.Value.Name + "</div>";
                                if (isAdmin)
                                {
                                    html += "<div class=\"absolute pin-t pin-r mr-1 h-full hidden group-hover:block\">";
                                    html += "<div class=\"flex items-center h-full\"> ";

                                    html += "<div data-rank=\"" + Ranks.Value.RankId + "\" data-action=\"settings\" class=\"cursor-pointer-r px-1\">";
                                    html += "<img src=\"" + RoleplayManager.CdnURL + "/ws_resources/images/settings.png\">";
                                    html += "</div>";

                                    // Empresas no removibles no pueden alterar rangos
                                    if (Groups[0].Removable)
                                    {
                                        html += "<div data-rank=\"" + Ranks.Value.RankId + "\" data-action=\"up\" class=\"cursor-pointer-r px-1\">";
                                        html += "<img src=\"" + RoleplayManager.CdnURL + "/ws_resources/images/up-arrow.png\">";
                                        html += "</div>";

                                        html += "<div data-rank=\"" + Ranks.Value.RankId + "\" data-action=\"down\" class=\"cursor-pointer-r px-1\">";
                                        html += "<img src=\"" + RoleplayManager.CdnURL + "/ws_resources/images/down-arrow.png\">";
                                        html += "</div>";

                                        html += "<div data-rank=\"" + Ranks.Value.RankId + "\" data-action=\"cross\" class=\"cursor-pointer-r px-1\">";
                                        html += "<img src=\"" + RoleplayManager.CdnURL + "/ws_resources/images/cross.png\">";
                                        html += "</div>";
                                    }

                                    html += "</div>";
                                    html += "</div>";
                                }

                                html += "</div>";

                                // < !-- User box -->
                                html += "<div class=\"flex flex-wrap -m-1 justify-center\">";

                                foreach (var Members in Groups[0].GetAllMembersDict)
                                {
                                    if (Members.Value.UserRank == Ranks.Value.RankId)
                                    {
                                        string Name = PlusEnvironment.GetGame().GetClientManager().GetNameById(Convert.ToInt32(Members.Value.UserId));// <= Busca en diccionario, Si es Off, hace SELECT directo.
                                        string Look = PlusEnvironment.GetGame().GetClientManager().GetLookById(Convert.ToInt32(Members.Value.UserId));// <= Busca en diccionario, Si es Off, hace SELECT directo.

                                        //<!-- User info -->
                                        html += "<div class=\"bg-dark-4 rounded m-1 group cursor-pointer-r\">";
                                        html += "<div class=\"m-px relative\">";
                                        html += "<div class=\"overflow-hidden bg-light-05 rounded-t\" style=\"height: 55px;\">";
                                        html += "<center><div class=\"figure-H_RWF_0\" style=\"background-image: url(&quot;" + RoleplayManager.AVATARIMG + "" + Look + "&quot;); width: 64px; height: 110px; margin-top: -20px;\"></div></center>";
                                        html += "</div>";

                                        bool IsMember = Groups[0].IsMember(Client.GetHabbo().Id);
                                        bool CanAscDesc = PlusEnvironment.GetGame().GetGroupManager().HasJobCommand(Client, "ascdesc", true);
                                        bool CanFire = PlusEnvironment.GetGame().GetGroupManager().HasJobCommand(Client, "fire", true);
                                        bool ItsMe = (Members.Value.UserId == Client.GetHabbo().Id);
                                        bool ItsSup = (Members.Value.UserRank >= Groups[0].Members[Client.GetHabbo().Id].UserRank);
                                        if (((isAdmin || (IsMember && (CanAscDesc || CanFire) && !ItsSup))) || Client.GetHabbo().GetPermissions().HasRight("corporation_rights"))
                                        {
                                            html += "<div class=\"absolute pin-b bg-dark-5 w-full hidden group-hover:block\" style=\"padding-top: 4px;padding-bottom: 4px;\">";
                                            html += "<div class=\"flex justify-around\">";
                                            html += "<div data-user=\"" + Members.Value.UserId + "\" data-action=\"up\" class=\"cursor-pointer-r px-1\">";
                                            html += "<img src=\"" + RoleplayManager.CdnURL + "/ws_resources/images/up-arrow.png\">";
                                            html += "</div>";
                                            html += "<div data-user=\"" + Members.Value.UserId + "\" data-action=\"down\" class=\"cursor-pointer-r px-1\">";
                                            html += "<img src=\"" + RoleplayManager.CdnURL + "/ws_resources/images/down-arrow.png\">";
                                            html += "</div>";
                                            if (isAdmin || CanFire || Client.GetHabbo().GetPermissions().HasRight("corporation_rights"))
                                            {
                                                html += "<div data-user=\"" + Members.Value.UserId + "\" data-action=\"cross\" class=\"cursor-pointer-r px-1\">";
                                                html += "<img src=\"" + RoleplayManager.CdnURL + "/ws_resources/images/cross.png\">";
                                                html += "</div>";
                                            }
                                            html += "</div>";
                                            html += "</div>";
                                        }

                                        html += "</div>";
                                        html += "<div class=\"text-center py-1\">" + Name + "</div>";
                                        html += "</div>";
                                    }
                                }

                                html += "</div>";
                                // < !-- End User box -->

                                html += "</div>";
                            }
                            html += "</div>";
                            html += "</div>";
                            #endregion

                            #region TABS
                            if (HasGang)
                            {
                                tabs += "<div id=\"GA_My_Members\" class=\"Tabbed_tab_1apzZ GA_My_Members Tabbed_selected_3aJyT\" style=\"min-width: 75px;\">";
                                tabs += "Miembros";
                                tabs += "</div>";

                                if (Groups[0].IsAdmin(Client.GetHabbo().Id) || Client.GetHabbo().GetPermissions().HasRight("group_management_override"))
                                {
                                    tabs += "<div id=\"GA_My_Ranks\" class=\"Tabbed_tab_1apzZ GA_My_Ranks\" style=\"min-width: 75px;\">";
                                    tabs += "Rangos";
                                    tabs += "</div>";
                                }

                                if (((!Groups[0].IsAdmin(Client.GetHabbo().Id) || !Groups[0].IsMember(Client.GetHabbo().Id)) && !PlusEnvironment.GetGame().GetGroupManager().HasJobCommand(Client, "hire", true)) && !Client.GetHabbo().GetPermissions().HasRight("corporation_rights")) { }
                                else
                                {
                                    tabs += "<div id=\"GA_My_Requests\" class=\"Tabbed_tab_1apzZ GA_My_Requests\" style=\"min-width: 75px;\">";
                                    tabs += "Solicitudes";
                                    tabs += "</div>";
                                }

                                tabs += "<div id=\"GA_My_Stats\" class=\"Tabbed_tab_1apzZ GA_My_Stats\" style=\"min-width: 75px;\">";
                                tabs += "Estad&iacute;sticas";
                                tabs += "</div>";

                                if (Groups[0].IsAdmin(Client.GetHabbo().Id) || Client.GetHabbo().GetPermissions().HasRight("group_management_override"))
                                {
                                    tabs += "<div id=\"GA_My_Edit\" class=\"Tabbed_tab_1apzZ GA_My_Edit\" style=\"min-width: 75px;\">";
                                    tabs += "Editar";
                                    tabs += "</div>";
                                }
                            }
                            #endregion

                            string SendData = "";
                            SendData += html;
                            Socket.Send("compose_gang|my_gang|" + SendData + "|" + tabs);
                        }
                    }
                    break;
                #endregion

                #region Create
                case "create":
                    {
                        #region Conditions
                        if (Client.GetPlay().TryGetCooldown("ga_create", true))
                            return;

                        #region Principal Conditions
                        if (Client.GetPlay().IsWorking)
                        {
                            Client.SendWhisper("¡No puedes hacer eso mientras estás trabajando!", 1);
                            return;
                        }
                        if (Client.GetPlay().IsDead)
                        {
                            Client.SendWhisper("¡No puedes hacer eso mientras estás muert@!", 1);
                            return;
                        }
                        if (Client.GetPlay().IsDying)
                        {
                            Client.SendWhisper("¡No puedes hacer eso mientras estás inconciente!", 1);
                            return;
                        }
                        if (Client.GetPlay().IsJailed)
                        {
                            Client.SendWhisper("¡No puedes hacer eso mientras estás encarcelad@!", 1);
                            return;
                        }
                        if (Client.GetPlay().Level < 2)
                        {
                            Socket.Send("compose_gang|msg_error|¡Necesitas al menos Nivel 2 para pertenecer a una banda!");
                            return;
                        }
                        if (PlusEnvironment.GetGame().GetGroupManager().HasJobCommand(Client, "law"))
                        {
                            Socket.Send("compose_gang|msg_error|¡No puedes pertenecer a una banda y ser policía a la vez!");
                            return;
                        }
                        #endregion

                        bool HasGang = false;

                        List<Group> Groups = PlusEnvironment.GetGame().GetGroupManager().GetGangsForUser(Client.GetHabbo().Id);
                        if (Groups != null && Groups.Count > 0)
                            HasGang = true;

                        if (HasGang)
                            return;

                        string[] ReceivedData = Data.Split(',');
                        string GetGangName = ReceivedData[1];
                        string GetColor1 = ReceivedData[3];
                        string GetColor2 = ReceivedData[4];
                        string GetBadge = ReceivedData[5];

                        // Filtramos por seguridad
                        GetGangName = System.Text.RegularExpressions.Regex.Replace(GetGangName, "<(.|\\n)*?>", string.Empty);
                        GetColor1 = System.Text.RegularExpressions.Regex.Replace(GetColor1, "<(.|\\n)*?>", string.Empty);
                        GetColor2 = System.Text.RegularExpressions.Regex.Replace(GetColor2, "<(.|\\n)*?>", string.Empty);
                        GetBadge = System.Text.RegularExpressions.Regex.Replace(GetBadge, "<(.|\\n)*?>", string.Empty);

                        if (String.IsNullOrEmpty(GetGangName) || GetGangName.Length < 3)
                        {
                            Socket.Send("compose_gang|msg_error|El nombre de tu banda debe tener al menos 3 caracteres.");
                            return;
                        }
                        if (GetGangName.Length > 11)
                        {
                            Socket.Send("compose_gang|msg_error|El nombre no debe ser mayor a 11 caracteres.");
                            return;
                        }
                        if(!System.Text.RegularExpressions.Regex.IsMatch(GetGangName, @"^[a-zA-Z0-9]+$"))
                        {
                            Socket.Send("compose_gang|msg_error|¡No se aceptan caracteres especiales! Solo números y letras.");
                            return;
                        }
                        int GetAccessType = 0;
                        if (!int.TryParse(ReceivedData[2], out GetAccessType))
                        {
                            Socket.Send("compose_gang|msg_error|Ha ocurrido un problema al obtener la Información del tipo de acceso de la banda.");
                            return;
                        }

                        if (Client.GetHabbo().Credits < RoleplayManager.GangsPrice)
                        {
                            Socket.Send("compose_gang|msg_error|No tienes $ "+ String.Format("{0:N0}", RoleplayManager.GangsPrice) +" para crear una banda.");
                            return;
                        }
                        #endregion

                        #region Execute
                        Group Group = null;                        
                        if (!PlusEnvironment.GetGame().GetGroupManager().TryCreateGroup(Client.GetHabbo(), 3, GetGangName, "", 0, GetBadge, GetColor1, GetColor2, "Indefinido", true, GetAccessType, out Group))
                        {
                            Socket.Send("compose_gang|msg_error|Ocurrió un problema al intentar crear la banda. Contacta con un Administrador.");
                            return;
                        }

                        Client.GetHabbo().Credits -= RoleplayManager.GangsPrice;
                        Client.GetHabbo().UpdateCreditsBalance();

                        RoleplayManager.Shout(Client, "*Ha declarado la creación de una nueva banda llamada "+Group.Name+"*", 5);
                        Socket.Send("compose_gang|msg_success|Banda creada exitosamente. Ahora podrás gestionarla desde aquí.");
                        PlusEnvironment.GetGame().GetWebEventManager().ExecuteWebEvent(Client, "event_gang", "mynew");
                        Client.GetPlay().CooldownManager.CreateCooldown("ga_create", 1000, 10);
                        #endregion
                    }
                    break;
                #endregion

                #region Rank Tools
                case "rank_tools":
                    {
                        List<Group> Groups = PlusEnvironment.GetGame().GetGroupManager().GetGangsForUser(Client.GetHabbo().Id);

                        if (Groups == null || Groups.Count <= 0)
                            return;

                        #region Check if is not Admin
                        if (!Groups[0].IsAdmin(Client.GetHabbo().Id) && !Client.GetHabbo().GetPermissions().HasRight("group_management_override"))
                            return;
                        #endregion

                        string[] ReceivedData = Data.Split(',');
                        int GetRank;
                        string WSAction = ReceivedData[2];

                        if (!int.TryParse(ReceivedData[1], out GetRank))
                            return;

                        var AllRanks = Groups[0].Ranks.ToList();
                        var AllMembers = Groups[0].GetAllMembersDict;

                        // Validamos si existe el rango
                        var check = AllRanks.Where(x => x.Value.RankId == GetRank);
                        if (check.Count() <= 0)
                            return;

                        switch (WSAction)
                        {
                            case "up":
                                {
                                    // Empresas oficiales del RP no son gestionables desde client. Solo DB.
                                    if (!Groups[0].Removable)
                                        return;

                                    int newrank = (GetRank + 1);

                                    // Validamos si existe un rango superior. 
                                    var TopRank = AllRanks.Where(x => x.Value.RankId == newrank);
                                    if (TopRank.Count() <= 0)
                                        return;

                                    using (var DB = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
                                    {
                                        // MYSQL PROCEDIMIENTO
                                        DB.RunQuery("CALL `ModifRank`(" + GetRank + ", " + newrank + ", " + Groups[0].Id + ");");

                                        AllRanks.Where(x => x.Value.RankId == newrank).ToList().ForEach(x => x.Value.RankId = 0);
                                        AllRanks.Where(x => x.Value.RankId == GetRank).ToList().ForEach(x => x.Value.RankId = newrank);
                                        AllRanks.Where(x => x.Value.RankId == 0).ToList().ForEach(x => x.Value.RankId = GetRank);

                                        foreach (var Members in AllMembers.Where(x => x.Value.UserRank == newrank))
                                        {
                                            Members.Value.UserRank = 0;
                                        }
                                        foreach (var Members in AllMembers.Where(x => x.Value.UserRank == GetRank))
                                        {
                                            Members.Value.UserRank = newrank;
                                        }
                                        foreach (var Members in AllMembers.Where(x => x.Value.UserRank == 0))
                                        {
                                            Members.Value.UserRank = GetRank;
                                        }
                                    }

                                    PlusEnvironment.GetGame().GetWebEventManager().ExecuteWebEvent(Client, "event_gang", "mynew");

                                }
                                break;

                            case "down":
                                {
                                    // Empresas oficiales del RP no son gestionables desde client. Solo DB.
                                    if (!Groups[0].Removable)
                                        return;

                                    int newrank = (GetRank - 1);

                                    // Validamos si existe un rango inferior. 
                                    var BottomRank = AllRanks.Where(x => x.Value.RankId == newrank);
                                    if (BottomRank == null)
                                        return;

                                    using (var DB = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
                                    {
                                        // MYSQL PROCEDIMIENTO
                                        DB.RunQuery("CALL `ModifRank`(" + GetRank + ", " + newrank + ", " + Groups[0].Id + ");");

                                        AllRanks.Where(x => x.Value.RankId == newrank).ToList().ForEach(x => x.Value.RankId = 0);
                                        AllRanks.Where(x => x.Value.RankId == GetRank).ToList().ForEach(x => x.Value.RankId = newrank);
                                        AllRanks.Where(x => x.Value.RankId == 0).ToList().ForEach(x => x.Value.RankId = GetRank);

                                        foreach (var Members in AllMembers.Where(x => x.Value.UserRank == newrank))
                                        {
                                            Members.Value.UserRank = 0;
                                        }
                                        foreach (var Members in AllMembers.Where(x => x.Value.UserRank == GetRank))
                                        {
                                            Members.Value.UserRank = newrank;
                                        }
                                        foreach (var Members in AllMembers.Where(x => x.Value.UserRank == 0))
                                        {
                                            Members.Value.UserRank = GetRank;
                                        }
                                    }

                                    PlusEnvironment.GetGame().GetWebEventManager().ExecuteWebEvent(Client, "event_gang", "mynew");
                                }
                                break;

                            case "cross":
                                {
                                    // Empresas oficiales del RP no son gestionables desde client. Solo DB.
                                    if (!Groups[0].Removable)
                                        return;

                                    // Si tiene un solo rango
                                    if (AllRanks.Count() <= 1)
                                        return;

                                    // Rango clicado + 1 es el rango superior.
                                    int uprank = (GetRank + 1);

                                    // Si hay miembros en ese rango, los cambiamos al rankid = 1

                                    foreach (var Members in AllMembers.Where(x => x.Value.UserRank == GetRank))
                                    {
                                        Members.Value.UserRank = 1;
                                    }

                                    // Borramos de DB
                                    using (var DB = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
                                    {
                                        // MYSQL PROCEDIMIENTO
                                        DB.RunQuery("CALL `DropRank`(" + GetRank + ", " + Groups[0].Id + ");");
                                    }

                                    // Borramos del Diccionario
                                    PlusEnvironment.GetGame().GetGroupManager().DeleteGroupRank(Groups[0].Id, GetRank);

                                    // Fix, read again de new list.
                                    AllRanks = Groups[0].Ranks.ToList();

                                    // Si hay rango superior, decrementamos su RankId para cada uno
                                    AllRanks.Where(x => x.Value.RankId >= uprank).ToList().ForEach(x => x.Value.RankId = (x.Value.RankId - 1));

                                    // Si hay usuarios en rangos superiores, decrementamos su RankId para cada uno
                                    AllMembers.Where(x => x.Value.UserRank >= uprank).ToList().ForEach(x => x.Value.UserRank = (x.Value.UserRank - 1));

                                    PlusEnvironment.GetGame().GetWebEventManager().ExecuteWebEvent(Client, "event_gang", "mynew");
                                }
                                break;

                            case "settings":
                                {
                                    GroupRank ThisRank = PlusEnvironment.GetGame().GetGroupManager().GetJobRank(Groups[0].Id, GetRank);

                                    if (ThisRank == null)
                                        return;

                                    string html = "";

                                    #region HTML
                                    html += "<div class=\"heading\">Nombre del Rango</div>";
                                    html += "<div class=\"flex\">";
                                    html += "<input id=\"GangRankNewName\" type=\"text\" data-lpignore=\"true\" class=\"dark-input-text flex-1 mr-1\" value=\"" + ThisRank.Name + "\">";
                                    html += "</div>";
                                    html += "<br>";
                                    
                                    html += "<div class=\"flex\">";
                                    html += "<button data-rank=\"" + GetRank + "\" data-action=\"SaveRank\" class=\"dark-button\" style=\"width: 100%;\">Cambiar nombre</button>";
                                    html += "</div>";

                                    html += "<br>";
                                    html += "<div class=\"heading\">Permisos</div>";
                                    html += "<div class=\"flex\">";
                                    html += "<table class=\"dark\">";
                                    html += "<tr class=\"dark2\">";
                                    html += "<th class=\"dark2\">Ascender/Descender</th>";
                                    html += "<th class=\"dark2\">Reclutar</th>";
                                    html += "<th class=\"dark2\">Expulsar</th>";
                                    html += "<th class=\"dark2\">Invitar</th>";
                                    html += "</tr>";
                                    html += "<tr class=\"dark2\">";
                                    html += "<td class=\"dark2\">";
                                    html += "<div data-rank=\"" + GetRank + "\" data-action=\"ascdesc\" class=\"cursor-pointer-r px-1\">";
                                    if (ThisRank.HasCommand("ascdesc"))
                                        html += "<img src=\"" + RoleplayManager.CdnURL + "/ws_resources/images/up-arrow.png\">";
                                    else
                                        html += "<img src=\"" + RoleplayManager.CdnURL + "/ws_resources/images/cross.png\">";
                                    html += "</div>";
                                    html += "</td>";
                                    html += "<td class=\"dark2\">";
                                    html += "<div data-rank=\"" + GetRank + "\" data-action=\"hire\" class=\"cursor-pointer-r px-1\">";
                                    if (ThisRank.HasCommand("hire"))
                                        html += "<img src=\"" + RoleplayManager.CdnURL + "/ws_resources/images/up-arrow.png\">";
                                    else
                                        html += "<img src=\"" + RoleplayManager.CdnURL + "/ws_resources/images/cross.png\">";
                                    html += "</div>";
                                    html += "</td>";
                                    html += "<td class=\"dark2\">";
                                    html += "<div data-rank=\"" + GetRank + "\" data-action=\"fire\" class=\"cursor-pointer-r px-1\">";
                                    if (ThisRank.HasCommand("fire"))
                                        html += "<img src=\"" + RoleplayManager.CdnURL + "/ws_resources/images/up-arrow.png\">";
                                    else
                                        html += "<img src=\"" + RoleplayManager.CdnURL + "/ws_resources/images/cross.png\">";
                                    html += "</div>";
                                    html += "</td>";
                                    html += "<td class=\"dark2\">";
                                    html += "<div data-rank=\"" + GetRank + "\" data-action=\"invite\" class=\"cursor-pointer-r px-1\">";
                                    if (ThisRank.HasCommand("invite"))
                                        html += "<img src=\"" + RoleplayManager.CdnURL + "/ws_resources/images/up-arrow.png\">";
                                    else
                                        html += "<img src=\"" + RoleplayManager.CdnURL + "/ws_resources/images/cross.png\">";
                                    html += "</div>";
                                    html += "</td>";
                                    html += "</tr>";
                                    html += "</table>";
                                    html += "</div>";
                                    html += "<br>";
                                    #endregion

                                    string SendData = "";
                                    SendData += html + "|";// EventData[2];
                                    Socket.Send("compose_gang|ranks|" + SendData);
                                }
                                break;

                            default:
                                break;
                        }
                    }
                    break;
                #endregion

                #region Member Tools
                case "member_tools":
                    {
                        List<Group> Groups = PlusEnvironment.GetGame().GetGroupManager().GetGangsForUser(Client.GetHabbo().Id);

                        if (Groups == null || Groups.Count <= 0)
                            return;

                        bool isAdmin = false;

                        #region Check if is not Admin
                        if (!Groups[0].IsAdmin(Client.GetHabbo().Id) && !Groups[0].IsMember(Client.GetHabbo().Id) && !Client.GetHabbo().GetPermissions().HasRight("group_management_override") && !PlusEnvironment.GetGame().GetGroupManager().HasJobCommand(Client, "ascdesc", true) && !PlusEnvironment.GetGame().GetGroupManager().HasJobCommand(Client, "fire", true))
                            return;
                        else
                            isAdmin = true;
                        #endregion

                        string[] ReceivedData = Data.Split(',');
                        int GetUserId;
                        string WSAction = ReceivedData[2];
                        if (!int.TryParse(ReceivedData[1], out GetUserId))
                            return;

                        var AllRanks = Groups[0].Ranks.ToList();
                        var AllMembers = Groups[0].GetAllMembersDict;

                        // Validamos si existe el usuario
                        if (WSAction != "accept" && WSAction != "decline" && !Groups[0].IsMember(GetUserId) && !Groups[0].IsAdmin(GetUserId))
                            return;

                        switch (WSAction)
                        {
                            case "up":
                                {

                                    if (!Groups[0].Removable && !Client.GetHabbo().GetPermissions().HasRight("corporation_rights"))
                                    {
                                        if (Groups[0].Members[GetUserId].UserId == Client.GetHabbo().Id)
                                            return;
                                    }

                                    //if (((!Group.IsAdmin(GetUserId) || !Group.IsMember(GetUserId)) && !PlusEnvironment.GetGame().GetGroupManager().HasJobCommand(Client, "ascdesc")) && !Client.GetHabbo().GetPermissions().HasRight("corporation_rights"))
                                    bool IsMember = Groups[0].IsMember(Client.GetHabbo().Id); 
                                    bool CanAscDesc = PlusEnvironment.GetGame().GetGroupManager().HasJobCommand(Client, "ascdesc", true);
                                    bool ItsMe = (Groups[0].Members[GetUserId].UserId == Client.GetHabbo().Id);
                                    bool ItsSup = (Groups[0].Members[GetUserId].UserRank >= Groups[0].Members[Client.GetHabbo().Id].UserRank);
                                    if (((!isAdmin && !(IsMember && CanAscDesc && !ItsSup))) && !Client.GetHabbo().GetPermissions().HasRight("corporation_rights"))
                                        return;

                                    int GetRank = Groups[0].Members[GetUserId].UserRank;
                                    int newrank = (GetRank + 1);

                                    if (!Groups[0].Removable)
                                    {
                                        if (newrank >= 6 && Groups[0].GetAdministrators.Count > 0)
                                        {
                                            Client.SendWhisper("¡No pueden haber dos líderes en una misma banda!", 1);
                                            return;
                                        }
                                    }

                                    // Validamos si existe un rango superior. 
                                    var TopRank = AllRanks.Where(x => x.Value.RankId == newrank);
                                    if (TopRank.Count() <= 0)
                                        return;

                                    using (var DB = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
                                    {
                                        // MYSQL PROCEDIMIENTO
                                        DB.RunQuery("CALL `ModifMember`(" + GetRank + ", " + newrank + ", " + Groups[0].Id + ", " + GetUserId + ");");

                                        AllMembers.Where(x => x.Value.UserId == GetUserId).ToList().ForEach(x => x.Value.UserRank = newrank);

                                    }
                                    #region Trabajos NO Removibles Max Rank es 6. Hacer/Quitar Admin en  Rank 6
                                    if (!Groups[0].Removable)
                                    {
                                        if (newrank == RoleplayManager.AdminRankGroupsNoRemov)
                                        {
                                            Groups[0].MakeAdmin(GetUserId);
                                        }
                                    }
                                    #endregion

                                    #region Mensaje de aviso al Target
                                    GameClient TargetSession = PlusEnvironment.GetGame().GetClientManager().GetClientByUserID(GetUserId);
                                    if (TargetSession != null)
                                    {
                                        RoleplayManager.Shout(TargetSession, "*Ha sido ascendid@ en su banda " + Groups[0].Name + "*", 5);
                                        TargetSession.SendWhisper("¡Buenas noticias! Has sido ascendid@ de rango en tu banda.", 1);

                                        if (Groups[0].IsAdmin(GetUserId))
                                        {
                                            TargetSession.GetPlay().Gang = Groups[0].Id;
                                        }
                                    }
                                    #endregion

                                    PlusEnvironment.GetGame().GetWebEventManager().ExecuteWebEvent(Client, "event_gang", "mynew");

                                }
                                break;

                            case "down":
                                {

                                    if (!Groups[0].Removable && !Client.GetHabbo().GetPermissions().HasRight("corporation_rights"))
                                    {
                                        if (Groups[0].Members[GetUserId].UserId == Client.GetHabbo().Id)
                                            return;
                                    }

                                    //if (((!Group.IsAdmin(GetUserId) || !Group.IsMember(GetUserId)) && !PlusEnvironment.GetGame().GetGroupManager().HasJobCommand(Client, "ascdesc")) && !Client.GetHabbo().GetPermissions().HasRight("corporation_rights"))
                                    bool IsMember = Groups[0].IsMember(Client.GetHabbo().Id);
                                    bool CanAscDesc = PlusEnvironment.GetGame().GetGroupManager().HasJobCommand(Client, "ascdesc", true);
                                    bool ItsMe = (Groups[0].Members[GetUserId].UserId == Client.GetHabbo().Id);
                                    bool ItsSup = (Groups[0].Members[GetUserId].UserRank >= Groups[0].Members[Client.GetHabbo().Id].UserRank);
                                    if (((!isAdmin && !(IsMember && CanAscDesc && !ItsSup))) && !Client.GetHabbo().GetPermissions().HasRight("corporation_rights"))
                                        return;

                                    int GetRank = Groups[0].Members[GetUserId].UserRank;
                                    int newrank = (GetRank - 1);

                                    if (Groups[0].IsAdmin(GetUserId) && !isAdmin && !Client.GetHabbo().GetPermissions().HasRight("corporation_rights"))
                                    {
                                        Client.SendWhisper("No puedes bajar de rango al líder de la Banda");
                                        return;
                                    }

                                    // Validamos si existe un rango superior. 
                                    var Bottom = AllRanks.Where(x => x.Value.RankId == newrank);
                                    if (Bottom.Count() <= 0)
                                        return;

                                    using (var DB = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
                                    {
                                        // MYSQL PROCEDIMIENTO
                                        DB.RunQuery("CALL `ModifMember`(" + GetRank + ", " + newrank + ", " + Groups[0].Id + ", " + GetUserId + ");");

                                        AllMembers.Where(x => x.Value.UserId == GetUserId).ToList().ForEach(x => x.Value.UserRank = newrank);
                                    }
                                    #region Trabajos NO Removibles Max Rank es 6. Hacer/Quitar Admin en  Rank 6
                                    if (!Groups[0].Removable)
                                    {
                                        if (newrank != RoleplayManager.AdminRankGroupsNoRemov)
                                            Groups[0].TakeAdmin(GetUserId);
                                    }
                                    #endregion

                                    #region Mensaje de aviso al Target
                                    GameClient TargetSession = PlusEnvironment.GetGame().GetClientManager().GetClientByUserID(GetUserId);
                                    if (TargetSession != null)
                                    {
                                        RoleplayManager.Shout(TargetSession, "*Ha sido degradad@ de rango en su banda " + Groups[0].Name + "*", 5);
                                        TargetSession.SendWhisper("Has sido degradad@ de rango en tu banda.", 1);

                                        #region IsWorking
                                        if (TargetSession != null)
                                        {
                                            if (TargetSession.GetPlay().IsWorking)
                                            {
                                                WorkManager.RemoveWorkerFromList(TargetSession);
                                                TargetSession.GetPlay().IsWorking = false;
                                                TargetSession.GetHabbo().Poof();

                                            }
                                            if (TargetSession.GetPlay().Ficha > 0)
                                            {
                                                TargetSession.GetPlay().Ficha = 0;
                                            }
                                        }
                                        #endregion

                                        if (!Groups[0].IsAdmin(GetUserId))
                                        {
                                            TargetSession.GetPlay().Gang = 0;
                                        }
                                    }
                                    #endregion

                                    PlusEnvironment.GetGame().GetWebEventManager().ExecuteWebEvent(Client, "event_gang", "mynew");
                                }
                                break;

                            case "cross":
                                {
                                    // if (((!Group.IsAdmin(GetUserId) || !Group.IsMember(GetUserId)) && !PlusEnvironment.GetGame().GetGroupManager().HasJobCommand(Client, "fire")) && !Client.GetHabbo().GetPermissions().HasRight("corporation_rights"))
                                    bool IsMember = Groups[0].IsMember(Client.GetHabbo().Id);
                                    bool CanFire = PlusEnvironment.GetGame().GetGroupManager().HasJobCommand(Client, "fire", true);

                                    if (((!isAdmin && !(IsMember && CanFire)) || Groups[0].Members[GetUserId].UserId == Client.GetHabbo().Id) && !Client.GetHabbo().GetPermissions().HasRight("corporation_rights"))
                                        return;

                                    // Si se intenta borrar a sí mismo (Que use :renunciar [id])
                                    // Si es Admin (Fundador) no puede eliminarse así.
                                    if (Groups[0].Members[GetUserId].UserId == Client.GetHabbo().Id)
                                        return;

                                    if (Groups[0].IsAdmin(GetUserId) && !Client.GetHabbo().GetPermissions().HasRight("corporation_rights"))
                                    {
                                        Client.SendWhisper("No puedes despedir al líner de la Banda");
                                        return;
                                    }

                                    GameClient TargetSession = PlusEnvironment.GetGame().GetClientManager().GetClientByUserID(GetUserId);

                                    string ExtraInf = "";

                                    #region Sacar
                                    if (Groups[0].IsAdmin(GetUserId))
                                    {
                                        ExtraInf = "Se te revocado el liderázgo de " + Groups[0].Name;
                                    }
                                    {
                                        ExtraInf = "Se te ha expulsado de la banda " + Groups[0].Name;
                                    }
                                    if (Groups[0].IsAdmin(GetUserId))
                                        Groups[0].TakeAdmin(GetUserId);

                                    if (Groups[0].IsMember(GetUserId))
                                        Groups[0].DeleteMember(GetUserId);
                                    #endregion

                                    // Si está ON, recibe alerta.
                                    if (TargetSession != null)
                                    {
                                        if (ExtraInf != "")
                                            TargetSession.SendNotification(ExtraInf);

                                        // Refrescar información
                                        PlusEnvironment.GetGame().GetWebEventManager().ExecuteWebEvent(TargetSession, "event_group", "open");
                                        RoleplayManager.Shout(TargetSession, "*Ha sido expulsado de la banda " + Groups[0].Name + "*", 5);
                                        TargetSession.GetPlay().Gang = 0;
                                    }

                                    PlusEnvironment.GetGame().GetWebEventManager().ExecuteWebEvent(Client, "event_gang", "mynew");
                                }
                                break;

                            case "accept":
                                {
                                    #region Check if is not Admin
                                    if (!Groups[0].IsAdmin(Client.GetHabbo().Id) && !Groups[0].IsMember(Client.GetHabbo().Id) && !Client.GetHabbo().GetPermissions().HasRight("group_management_override") && !PlusEnvironment.GetGame().GetGroupManager().HasJobCommand(Client, "hire", true))
                                        return;
                                    else
                                        isAdmin = true;
                                    #endregion

                                    string VIPLider = PlusEnvironment.GetUserInfoBy("rank_vip", "id", Convert.ToString(Groups[0].GetAdministrators[0]));
                                    int LimitMembers = RoleplayManager.GangsMaxMembers;
                                    if (VIPLider == "1")
                                        LimitMembers += 5;
                                    else if (VIPLider == "2")
                                        LimitMembers += 10;

                                    if (Groups[0].GetAllMembers.Count() >= LimitMembers)
                                    {
                                        Client.SendWhisper("¡Has alcanzado el máximo de miembros admitidos en una banda!", 1);
                                        return;
                                    }

                                    GameClient TargetClient = PlusEnvironment.GetGame().GetClientManager().GetClientByUserID(GetUserId);

                                    // Obtenemos los Trabajos del Target
                                    List<Group> Gangs = PlusEnvironment.GetGame().GetGroupManager().GetGangsForUser(GetUserId);
                                    if (Gangs == null)
                                    {
                                        Client.SendWhisper("Ocurrió un problema al buscar las bandas de esa persona. Intentalo más tarde.", 1);
                                        return;
                                    }
                                    int TotalGangs = Gangs.Count;

                                    if (PlusEnvironment.GetGame().GetClientManager().GetLevelById(Convert.ToInt32(GetUserId)) < 2)
                                    {
                                        Client.SendWhisper("¡Esa persona es Nivel 1! Necesita al menos Nivel 2 para pertenecer a una banda.", 1);
                                        return;
                                    }

                                    if (PlusEnvironment.GetGame().GetGroupManager().HasJobCommand(TargetClient, "law"))
                                    {
                                        Client.SendWhisper("¡Esa persona es policía! No puedes aceptarla en tu banda.", 1);
                                        return;
                                    }
                                    else
                                    {
                                        // Si está On
                                        if (TargetClient != null)
                                        {
                                            if (TotalGangs == 0)
                                            {
                                                #region DirectJoin

                                                //Si es Empresa (PRIVATE)
                                                // Se metió a la lista de Requisitos, Aquí forzamos el Ingreso.
                                                #region SendPackets AcceptGroupMembershipEvent
                                                if (Groups[0].GroupType == GroupType.LOCKED)
                                                {
                                                    int UserId = TargetClient.GetHabbo().Id;
                                                    if (!Groups[0].HasRequest(UserId))
                                                        return;

                                                    Habbo Habbo = PlusEnvironment.GetHabboById(UserId);
                                                    if (Habbo == null)
                                                    {
                                                        Client.SendNotification("Oops, ha ocurrido un problema al buscar al usuario, es probable que se haya desconectado. ¡El proceso lo dejó en la Lista de Solicitudes de Banda!");
                                                        return;
                                                    }

                                                    Groups[0].HandleRequest(UserId, true);

                                                    Client.SendMessage(new GroupMemberUpdatedComposer(Groups[0].Id, Habbo, 4));
                                                }
                                                #endregion


                                                // Actualizamos Información del Rank del User
                                                TargetClient.GetPlay().JobId = Groups[0].Id;
                                                TargetClient.GetPlay().JobRank = 1;
                                                Groups[0].UpdateInfoJobMember(TargetClient.GetHabbo().Id);

                                                // Retornamos Vars
                                                TargetClient.GetPlay().JobId = 0;
                                                TargetClient.GetPlay().JobRank = 0;
                                                #endregion
                                                PlusEnvironment.GetGame().GetWebEventManager().ExecuteWebEvent(TargetClient, "event_group", "open");
                                                RoleplayManager.Shout(Client, "*Admite la entrada a " + PlusEnvironment.GetUsernameById(GetUserId) + " en su banda " + Groups[0].Name + "*", 5);
                                                RoleplayManager.Shout(TargetClient, "*Ha sido admitido en la banda " + Groups[0].Name + "*", 5);
                                                TargetClient.SendNotification("¡Felicitaciones! Han aceptado tu solicitud en la banda " + Groups[0].Name);
                                            }
                                            else
                                            {
                                                Client.SendWhisper("Esta persona ya pertenece a otra banda. Pídele que la abandone o rechaza su solicitud.", 1);
                                            }
                                        }
                                        // Si está Off
                                        else
                                        {
                                            if (TotalGangs == 0)
                                            {
                                                Groups[0].HandleRequest(GetUserId, true);
                                                RoleplayManager.Shout(Client, "*Admite la entrada a " + PlusEnvironment.GetUsernameById(GetUserId) + " en su banda " + Groups[0].Name + "*", 5);
                                            }
                                            else
                                            {
                                                Client.SendWhisper("Esta persona ya pertenece a otra banda. Pídele que la abandone o rechaza su solicitud.", 1);
                                            }
                                        }
                                    }
                                    PlusEnvironment.GetGame().GetWebEventManager().ExecuteWebEvent(Client, "event_gang", "requests");

                                }
                                break;

                            case "decline":
                                {
                                    #region Check if is not Admin
                                    if (!Groups[0].IsAdmin(Client.GetHabbo().Id) && !Groups[0].IsMember(Client.GetHabbo().Id) && !Client.GetHabbo().GetPermissions().HasRight("group_management_override") && !PlusEnvironment.GetGame().GetGroupManager().HasJobCommand(Client, "hire", true))
                                        return;
                                    else
                                        isAdmin = true;
                                    #endregion

                                    Groups[0].HandleRequest(GetUserId, false);
                                    PlusEnvironment.GetGame().GetWebEventManager().ExecuteWebEvent(Client, "event_gang", "requests");

                                    GameClient TargetClient = PlusEnvironment.GetGame().GetClientManager().GetClientByUserID(GetUserId);
                                    if(TargetClient != null)
                                        PlusEnvironment.GetGame().GetWebEventManager().ExecuteWebEvent(TargetClient, "event_group", "open");
                                }
                                break;

                            default:
                                break;
                        }

                    }
                    break;
                #endregion

                #region Ranks Tab
                case "ranks":
                    {
                        List<Group> Groups = PlusEnvironment.GetGame().GetGroupManager().GetGangsForUser(Client.GetHabbo().Id);

                        if (Groups == null || Groups.Count <= 0)
                            return;

                        #region Check if is not Admin
                        if (!Groups[0].IsAdmin(Client.GetHabbo().Id) && !Client.GetHabbo().GetPermissions().HasRight("group_management_override"))
                            return;
                        #endregion

                        string html = "";

                        #region HTML
                        html += "<div class=\"heading\">Agregar Rango</div>";
                        html += "<div class=\"flex\">";
                        html += "<input id=\"GA_InputRank\" type=\"text\" data-lpignore=\"true\" class=\"dark-input-text flex-1 mr-1\" value=\"\" maxlength=\"50\" autocomplete=\"off\">";
                        html += "<button id=\"GA_My_AddRank\" class=\"dark-button\">Agregar</button>";
                        html += "</div>";
                        #endregion

                        string SendData = html;
                        Socket.Send("compose_gang|ranks|" + SendData);
                    }
                    break;
                #endregion

                #region Manage
                case "manage":
                    {
                        List<Group> Groups = PlusEnvironment.GetGame().GetGroupManager().GetGangsForUser(Client.GetHabbo().Id);

                        if (Groups == null || Groups.Count <= 0)
                            return;

                        // Empresas oficiales del RP no son gestionables desde client. Solo DB.
                        if (!Groups[0].Removable)
                            return;

                        #region Check if is not Admin
                        if (!Groups[0].IsAdmin(Client.GetHabbo().Id) && !Client.GetHabbo().GetPermissions().HasRight("group_management_override"))
                            return;
                        #endregion

                        string[] ReceivedData = Data.Split(',');

                        string WSAction = ReceivedData[1];
                        string DataString = "";

                        if (WSAction != "open")
                        {
                            DataString = ReceivedData[2];

                            if (String.IsNullOrWhiteSpace(DataString))
                                return;
                        }

                        switch (WSAction)
                        {
                            case "open":
                                {
                                    Socket.Send("compose_gang|ranks|");
                                }
                                break;

                            case "addrank":
                                {
                                    if (DataString.Length > 50)
                                    {
                                        Socket.Send("compose_gang|msg_error|El nombre del rango es demasiado largo.");
                                        return;
                                    }
                                    if (!System.Text.RegularExpressions.Regex.IsMatch(DataString, @"^[a-zA-Z0-9]+$"))
                                    {
                                        Socket.Send("compose_gang|msg_error|¡No se aceptan caracteres especiales! Solo números y letras.");
                                        return;
                                    }

                                    if (Groups[0].Ranks.Count() >= 8)
                                    {
                                        Socket.Send("compose_gang|msg_error|Límite de 8 rangos alcanzados.");
                                        return;
                                    }

                                    int NewRank = Groups[0].Ranks.Count() + 1;

                                    string[] commands = Groups[0].GetCommandsbyActivity(Groups[0].GActivity).ToString().Split(',');
                                    string[] workrooms = Groups[0].RoomId.ToString().Split(',');

                                    /*
                                    string[] commands = new string[1];
                                    string[] workrooms = new string[1];
                                    commands[0] = "";
                                    workrooms[0] = "*";
                                    */

                                    Groups[0].AddRank(Groups[0].Id, NewRank, DataString, "", "", 0, commands, workrooms, 0, 0);
                                    Socket.Send("compose_gang|msg_success|Rango agregado exitosamente.");
                                    PlusEnvironment.GetGame().GetWebEventManager().ExecuteWebEvent(Client, "event_gang", "mynew");
                                }
                                break;

                            default:
                                break;
                        }

                    }
                    break;
                #endregion

                #region Edit Rank
                case "editrank":
                    {
                        List<Group> Groups = PlusEnvironment.GetGame().GetGroupManager().GetGangsForUser(Client.GetHabbo().Id);

                        if (Groups == null || Groups.Count <= 0)
                            return;

                        #region Check if is not Admin
                        if (!Groups[0].IsAdmin(Client.GetHabbo().Id) && !Client.GetHabbo().GetPermissions().HasRight("group_management_override"))
                            return;
                        #endregion

                        string[] ReceivedData = Data.Split(',');

                        int GetRank = 0;
                        var AllRanks = Groups[0].Ranks.ToList();

                        string WSAction = ReceivedData[1];

                        if (!int.TryParse(ReceivedData[2], out GetRank))
                            return;

                        // Validamos si existe el rango
                        var check = AllRanks.Where(x => x.Value.RankId == GetRank);
                        if (check.Count() <= 0)
                            return;

                        switch (WSAction)
                        {
                            #region Save Rank
                            case "saverank":
                                {
                                    string RankName = ReceivedData[3];

                                    #region Conditions
                                    if (String.IsNullOrWhiteSpace(RankName))
                                    {
                                        Socket.Send("compose_gang|msg_error|Ese nombre de rango no es válido.");
                                        return;
                                    }
                                    if (RankName.Length > 50)
                                    {
                                        Socket.Send("compose_gang|msg_error|Ese nombre de rango es demasiado largo.");
                                        return;
                                    }
                                    #endregion

                                    // Actualizamos DB y Diccionario en este método.
                                    Groups[0].UpdateJobSettings(GetRank, RankName, 0, 0);
                                    Socket.Send("compose_gang|msg_success|Cambios guardados satisfactoriamente.");
                                }
                                break;
                            #endregion

                            #region Permissions
                            case "permissions":
                                {
                                    string TypeCMD = ReceivedData[3];

                                    if (TypeCMD != "ascdesc" && TypeCMD != "hire" && TypeCMD != "fire" && TypeCMD != "invite")
                                        return;

                                    // Actualizamos DB y Diccionario en este método.
                                    Groups[0].UpdateJobCommads(GetRank, TypeCMD);
                                    PlusEnvironment.GetGame().GetWebEventManager().ExecuteWebEvent(Client, "event_gang", "rank_tools," + GetRank + ",settings");
                                }
                                break;
                            #endregion

                            #region Default
                            default:
                                break;
                                #endregion
                        }
                    }
                    break;
                #endregion

                #region Invitations Receiveds
                case "invitations_re":
                    {
                        string html = "";

                        #region HTML
                        html += "<div>";
                        //<!-- General box -->
                        html += "<div class=\"m-2\">";

                        //<!-- Header -->
                        html += "<div class=\"heading relative group\">";
                        html += "<div>Estas bandas te han invitado a un&iacute;rteles</div>";
                        html += "</div>";

                        int Counter = 0;

                        DataTable PhOwn = null;
                        using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
                        {
                            dbClient.SetQuery("SELECT * FROM `group_requests` WHERE `user_id` = "+Client.GetHabbo().Id+" AND `invited` = '1'");
                            PhOwn = dbClient.getTable();

                            if (PhOwn != null)
                            {
                                foreach (DataRow Row in PhOwn.Rows)
                                {
                                    Group Gang = null;
                                    if (!PlusEnvironment.GetGame().GetGroupManager().TryGetGroup(Convert.ToInt32(Row["group_id"]), out Gang))
                                        continue;

                                    Counter++;
                                    //<!-- Gang box -->
                                    html += "<div class=\"flex flex-wrap -m-1 justify-center\">";
                                    //<!-- Gang info -->
                                    html += "<div class=\"bg-dark-4 rounded m-1 group cursor-pointer-r\" style=\"width: 23%;\">";
                                    html += "<div class=\"m-px relative\">";
                                    html += "<div class=\"overflow-hidden bg-light-05 rounded-t\">";
                                    html += "<center><div class=\"figure-H_RWF_0\" style=\"background-image: url(https://swf.habbovip.us/group-badge/badge/" + Gang.GetBadge() + "); width: 64px; height: 110px; margin-top: -20px;\"></div></center>";
                                    html += "</div>";
                                    html += "<div class=\"absolute pin-b bg-dark-5 w-full group-hover:block\" style=\"padding-top: 4px;padding-bottom: 4px;\">";
                                    html += "<div class=\"flex justify-around\">";
                                    html += "<div data-gang=\"" + Gang.Id + "\" data-action=\"accept\" class=\"cursor-pointer-r px-1\">";
                                    html += "<img src=\"" + RoleplayManager.CdnURL + "/ws_resources/images/up-arrow.png\">";
                                    html += "</div>";
                                    html += "<div data-gang=\"" + Gang.Id + "\" data-action=\"decline\" class=\"cursor-pointer-r px-1\">";
                                    html += "<img src=\"" + RoleplayManager.CdnURL + "/ws_resources/images/cross.png\">";
                                    html += "</div>";
                                    html += "</div>";
                                    html += "</div>";
                                    html += "</div>";
                                    html += "<div class=\"text-center py-1\">"+Gang.Name+"</div>";
                                    html += "</div>";

                                    html += "<div class=\"bg-dark-4 rounded m-1 group\" style=\"width: 69%;padding: 5px;\">";
                                    html += "<p>Estad&iacute;sticas principales de la banda:</p>";
                                    html += "<br>";
                                    html += "<div style=\"max-height: 108px;overflow: auto;display: inline-flex;\">";
                                    html += "<div class=\"gang_inf_box\">";
                                    html += "<div><b>Asesinatos</b></div>";
                                    html += "<div>"+String.Format("{0:N0}", Gang.GangKills)+"</div>";
                                    html += "</div>";
                                    html += "<div class=\"gang_inf_box\">";
                                    html += "<div><b>Barrios capturados</b></div>";
                                    html += "<div>" + String.Format("{0:N0}", Gang.GangTurfsTaken) + "</div>";
                                    html += "</div>";
                                    html += "<div class=\"gang_inf_box\">";
                                    html += "<div><b>Barrios defendidos</b></div>";
                                    html += "<div>" + String.Format("{0:N0}", Gang.GangTurfsDefended) + "</div>";
                                    html += "</div>";
                                    html += "<div class=\"gang_inf_box\">";
                                    html += "<div><b>Riqueza</b></div>";
                                    html += "<div>$ " + String.Format("{0:N0}", Gang.Bank) + "</div>";
                                    html += "</div>";
                                    html += "</div>";
                                    html += "<br><br>";
                                    html += "<i>M&aacute;s info. en el Perfil de la banda.</i>";
                                    html += "</div>";
                                    html += "</div>";
                                    //<!-- End Gang box -->
                                }
                            }
                        }

                        if (Counter <= 0)
                            html += "<center><b style='color:red'>No tienes ninguna invitación de banda pendiente.</b></center>";

                        html += "</div>";
                        html += "</div>";
                        #endregion

                        Socket.Send("compose_gang|invitations_re|" + html);
                    }
                    break;
                #endregion

                #region Invitations Sendeds
                case "invitations_se":
                    {
                        List<Group> Groups = PlusEnvironment.GetGame().GetGroupManager().GetGangsForUser(Client.GetHabbo().Id);
                        if (Groups == null || Groups.Count <= 0)
                        {
                            Socket.Send("compose_gang|invitations_se|<center><b style='color:red'>No perteneces a ninguna banda para poder enviar invitaciones.</b></center>");
                            return;
                        }

                        bool CanInvite;
                        #region Check if is not Admin
                        if (!Groups[0].IsAdmin(Client.GetHabbo().Id) && !Groups[0].IsMember(Client.GetHabbo().Id) && !Client.GetHabbo().GetPermissions().HasRight("group_management_override") && !PlusEnvironment.GetGame().GetGroupManager().HasJobCommand(Client, "invite", true))
                            CanInvite = false;
                        else
                            CanInvite = true;
                        #endregion

                        string html = "";
                        var AllRequest = Groups[0].GetRequests;

                        #region HTML
                        html += "<div>";
                        //<!-- General box -->
                        html += "<div class=\"m-2\">";

                        //<!-- Header -->
                        if (CanInvite)
                        {
                            html += "<div class=\"heading relative group\">";
                            html += "<div>Invitar miembros a tu banda</div>";
                            html += "</div>";
                            html += "<div class=\"flex\">";
                            html += "<input id=\"Input_G_I_S_User\" type=\"text\" data-lpignore=\"true\" class=\"dark-input-text flex-1 mr-1\" value=\"\" maxlength=\"50\" autocomplete=\"off\" placeholder=\"Escribe aquí el nombre de la persona que deseas invitar\">";
                            html += "<button id=\"GA_Send_Inv\" class=\"dark-button\">Invitar</button>";
                            html += "</div>";
                            html += "<br>";
                        }
                        

                        html += "<div class=\"heading relative group\">";
                        html += "<div>Invitaciones pendientes</div>";
                        html += "</div>";
                        html += "<table id=\"financelist\">";

                        foreach (var Request in AllRequest)
                        {
                            string Name = PlusEnvironment.GetGame().GetClientManager().GetNameById(Convert.ToInt32(Request));// <= Busca en diccionario, Si es Off, hace SELECT directo.
                            string Look = PlusEnvironment.GetGame().GetClientManager().GetLookById(Convert.ToInt32(Request));// <= Busca en diccionario, Si es Off, hace SELECT directo.

                            DataRow Row = null;
                            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
                            {
                                dbClient.SetQuery("SELECT * FROM `group_requests` WHERE `group_id` = @id AND `user_id` = @userid AND `invited` = '1' LIMIT 1");
                                dbClient.AddParameter("id", Groups[0].Id);
                                dbClient.AddParameter("userid", Request);
                                Row = dbClient.getRow();

                                if (Row != null)
                                {
                                    html += "<tr>";
                                    html += "<td>";
                                    html += "<img src=\"" + RoleplayManager.AVATARIMG + "" + Look + "&headonly=1\">";
                                    html += Name + " fue invitad@ a la banda por " + Row["invited_by"] + " el " + PlusEnvironment.UnixTimeStampToDateTime(Convert.ToDouble(Row["timestamp"])).ToString("dd \\d\\e MMMM \\d\\e yyyy");
                                    html += "</td>";
                                    html += "</tr>";
                                }
                            }
                        }

                        html += "</table>";
                        html += "</div>";
                        html += "</div>";
                        #endregion

                        Socket.Send("compose_gang|invitations_se|" + html);
                    }
                    break;
                #endregion

                #region Invitations Sending
                case "send_invitation":
                    {
                        #region Conditions
                        List<Group> Groups = PlusEnvironment.GetGame().GetGroupManager().GetGangsForUser(Client.GetHabbo().Id);

                        if (Groups == null || Groups.Count <= 0)
                        {
                            Socket.Send("compose_gang|invitations_se|<center><b style='color:red'>No perteneces a ninguna banda para poder enviar invitaciones.</b></center>");
                            return;
                        }


                        #region Check if is not Admin
                        if (!Groups[0].IsAdmin(Client.GetHabbo().Id) && !Groups[0].IsMember(Client.GetHabbo().Id) && !Client.GetHabbo().GetPermissions().HasRight("group_management_override") && !PlusEnvironment.GetGame().GetGroupManager().HasJobCommand(Client, "invite", true))
                            return;
                        #endregion

                        string[] ReceivedData = Data.Split(',');
                        string Username = ReceivedData[1];

                        // Filtramos por seguridad
                        Username = System.Text.RegularExpressions.Regex.Replace(Username, "<(.|\\n)*?>", string.Empty);

                        if(Groups[0].GroupType == GroupType.OPEN)
                        {
                            Socket.Send("compose_gang|msg_error|¡El acceso a tu banda es abierto! No puedes enviar invitaciones así. Cambia el modo de acceso.");
                            return;
                        }

                        if (String.IsNullOrEmpty(Username))
                        {
                            Socket.Send("compose_gang|msg_error|Debes ingresar un nombe de usuario.");
                            return;
                        }

                        if(Username.ToLower() == Client.GetHabbo().Username.ToLower())
                        {
                            Socket.Send("compose_gang|msg_error|¡No puedes enviarte invitaciones a ti mism@!");
                            return;
                        }

                        Habbo Habbo = PlusEnvironment.GetHabboByUsername(Username);

                        if(Habbo == null)
                        {
                            Socket.Send("compose_gang|msg_error|No se encontró ningún usuario con ese nombre.");
                            return;
                        }

                        if (Groups[0].HasRequest(Habbo.Id))
                        {
                            Socket.Send("compose_gang|msg_error|Ya se ha enviado una invitación a " + Habbo.Username + ".");
                            return;
                        }
                        if (Habbo.GetClient() != null && Habbo.GetClient().GetPlay().Level < 2)
                        {
                            Socket.Send("compose_gang|msg_error|¡Esa persona es Nivel 1! Necesita al menos Nivel 2 para pertenecer a una banda.");
                            return;
                        }
                        if (Habbo.GetClient() != null && PlusEnvironment.GetGame().GetGroupManager().HasJobCommand(Habbo.GetClient(), "law"))
                        {
                            Socket.Send("compose_gang|msg_error|¡Esa persona es policía! No puedes invitarla a tu banda.");
                            return;
                        }
                        #endregion

                        #region Execute
                        Client.GetPlay().BuyingCorp = false;
                        Groups[0].AddMember(Habbo.Id, 1, true, 0, "", "", null, true, Client.GetHabbo().Username);// Metemos directo a db por seguridad y evitar bugs

                        List<GameClient> GroupAdmins = (from Clients in PlusEnvironment.GetGame().GetClientManager().GetClients.ToList() where Clients != null && Clients.GetHabbo() != null && Groups[0].IsAdmin(Clients.GetHabbo().Id) select Clients).ToList();
                        foreach (GameClient Clients in GroupAdmins)
                        {
                            Client.SendMessage(new GroupMembershipRequestedComposer(Groups[0].Id, Habbo, 3));
                        }
                        Client.SendMessage(new GroupInfoComposer(Groups[0], Client));
                        if (Habbo.GetClient() != null)
                        {
                            Habbo.GetClient().SendMessage(new GroupInfoComposer(Groups[0], Habbo.GetClient()));
                            PlusEnvironment.GetGame().GetWebEventManager().ExecuteWebEvent(Habbo.GetClient(), "event_group", "open");
                        }
                        Client.GetPlay().JobRequest = false;

                        Socket.Send("compose_gang|msg_success|Inivitación enviada a " + Habbo.Username + " correctamente.");
                        PlusEnvironment.GetGame().GetWebEventManager().ExecuteWebEvent(Client, "event_gang", "invitations_se");
                        #endregion
                    }
                    break;
                #endregion

                #region Invitations Receiveds Tools
                case "inv_re_tools":
                    {
                        if (Client.GetPlay().TryGetCooldown("inv_re_tools", true))
                            return;

                        string[] ReceivedData = Data.Split(',');
                        int GetGang;
                        string WSAction = ReceivedData[2];

                        if (!int.TryParse(ReceivedData[1], out GetGang))
                            return;

                        Group Gang = null;
                        if (!PlusEnvironment.GetGame().GetGroupManager().TryGetGroup(GetGang, out Gang))
                            return;

                        switch (WSAction)
                        {
                            case "accept":
                                {
                                    if (Client.GetPlay().Level < 2)
                                    {
                                        Socket.Send("compose_gang|msg_error|¡Necesitas al menos Nivel 2 para pertenecer a una banda.");
                                        return;
                                    }

                                    if (PlusEnvironment.GetGame().GetGroupManager().HasJobCommand(Client, "law"))
                                    {
                                        Socket.Send("compose_gang|msg_error|¡No puedes pertenecer a una banda y ser policía a la vez!");
                                        return;
                                    }

                                    List<Group> Groups = PlusEnvironment.GetGame().GetGroupManager().GetGangsForUser(Client.GetHabbo().Id);
                                    if (Groups != null && Groups.Count > 0)
                                    {
                                        Socket.Send("compose_gang|msg_error|¡Ya perteneces a una banda! No puedes ser miembro de más de una a la vez.");
                                        return;
                                    }

                                    string VIPLider = PlusEnvironment.GetUserInfoBy("rank_vip", "id", Convert.ToString(Gang.GetAdministrators[0]));
                                    int LimitMembers = RoleplayManager.GangsMaxMembers;
                                    if (VIPLider == "1")
                                        LimitMembers += 5;
                                    else if (VIPLider == "2")
                                        LimitMembers += 10;

                                    if (Gang.GetAllMembers.Count() >= LimitMembers)
                                    {
                                        Client.SendWhisper("¡La banda está llena! Ya no hay espacio para un/a nuev@ integrante", 1);
                                        return;
                                    }

                                    #region DirectJoin

                                    //Si es Empresa (PRIVATE)
                                    // Se metió a la lista de Requisitos, Aquí forzamos el Ingreso.
                                    #region SendPackets AcceptGroupMembershipEvent
                                    if (Gang.GroupType == GroupType.LOCKED)
                                    {
                                        int UserId = Client.GetHabbo().Id;
                                        if (!Gang.HasRequest(UserId))
                                            return;

                                        Habbo Habbo = PlusEnvironment.GetHabboById(UserId);
                                        if (Habbo == null)
                                        {
                                            Client.SendNotification("Oops, ha ocurrido un problema al buscar tu información.");
                                            return;
                                        }

                                        Gang.HandleRequest(UserId, true);

                                        Client.SendMessage(new GroupMemberUpdatedComposer(Gang.Id, Habbo, 4));
                                    }
                                    #endregion


                                    // Actualizamos Información del Rank del User
                                    Client.GetPlay().JobId = Gang.Id;
                                    Client.GetPlay().JobRank = 1;
                                    Gang.UpdateInfoJobMember(Client.GetHabbo().Id);

                                    // Retornamos Vars
                                    Client.GetPlay().JobId = 0;
                                    Client.GetPlay().JobRank = 0;
                                    #endregion

                                    RoleplayManager.Shout(Client, "*Ha aceptado la invitación de ingreso a la banda "+Gang.Name+"*", 5);
                                    Socket.Send("compose_gang|msg_success|Has aceptado ingresar a la banda " + Gang.Name);

                                    PlusEnvironment.GetGame().GetWebEventManager().ExecuteWebEvent(Client, "event_group", "open");
                                    PlusEnvironment.GetGame().GetWebEventManager().ExecuteWebEvent(Client, "event_gang", "mynew");
                                    PlusEnvironment.GetGame().GetWebEventManager().ExecuteWebEvent(Client, "event_gang", "invitations_re");
                                }
                                break;

                            case "decline":
                                {
                                    Gang.HandleRequest(Client.GetHabbo().Id, false);
                                    RoleplayManager.Shout(Client, "*Ha rechazado la invitación de ingreso a la banda " + Gang.Name + "*", 5);
                                    Socket.Send("compose_gang|msg_success|Has rechazado ingresar a la banda " + Gang.Name);

                                    PlusEnvironment.GetGame().GetWebEventManager().ExecuteWebEvent(Client, "event_group", "open");
                                    PlusEnvironment.GetGame().GetWebEventManager().ExecuteWebEvent(Client, "event_gang", "invitations_re");
                                }
                                break;

                            default:
                                break;
                        }

                        Client.GetPlay().CooldownManager.CreateCooldown("inv_re_tools", 1000, 5);
                    }
                    break;
                #endregion

                #region Requests
                case "requests":
                    {
                        List<Group> Groups = PlusEnvironment.GetGame().GetGroupManager().GetGangsForUser(Client.GetHabbo().Id);

                        if (Groups == null || Groups.Count <= 0)
                            return;

                        #region Check if is not Admin
                        if (!Groups[0].IsAdmin(Client.GetHabbo().Id) && !Groups[0].IsMember(Client.GetHabbo().Id) && !Client.GetHabbo().GetPermissions().HasRight("group_management_override") && !PlusEnvironment.GetGame().GetGroupManager().HasJobCommand(Client, "hire", true))
                            return;
                        #endregion

                        string html = "";
                        var AllRequest = Groups[0].GetRequests;

                        #region HMTL
                        html += "<div>";
                        html += "<div class=\"-m-2\">";
                        //<!-- General box -->
                        html += "<div class=\"m-2\">";

                        //<!-- Header -->
                        html += "<div class=\"heading relative group\">";
                        html += "<div>Solicitudes</div>";
                        html += "</div>";

                        int Counter = 0;
                        foreach (var Request in AllRequest)
                        {
                            string Name = PlusEnvironment.GetGame().GetClientManager().GetNameById(Convert.ToInt32(Request));// <= Busca en diccionario, Si es Off, hace SELECT directo.
                            string Look = PlusEnvironment.GetGame().GetClientManager().GetLookById(Convert.ToInt32(Request));// <= Busca en diccionario, Si es Off, hace SELECT directo.

                            DataRow Row = null;
                            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
                            {
                                dbClient.SetQuery("SELECT * FROM group_requests GR, play_stats PS WHERE GR.group_id = @id AND GR.user_id = @userid AND GR.invited = '0' AND PS.id = GR.user_id LIMIT 1");
                                dbClient.AddParameter("id", Groups[0].Id);
                                dbClient.AddParameter("userid", Request);
                                Row = dbClient.getRow();

                                if (Row != null)
                                {
                                    Counter++;

                                    //< !-- User box -->
                                    html += "<div class=\"flex flex-wrap -m-1 justify-center\">";
                                    //<!-- User info -->
                                    html += "<div class=\"bg-dark-4 rounded m-1 group cursor-pointer-r\" style=\"width: 23%;\">";
                                    html += "<div class=\"m-px relative\">";
                                    html += "<div class=\"overflow-hidden bg-light-05 rounded-t\">";
                                    html += "<center><div class=\"figure-H_RWF_0\" style=\"background-image: url(&quot;" + RoleplayManager.AVATARIMG + "" + Look + "&quot;); width: 64px; height: 110px; margin-top: -20px;\"></div></center>";
                                    html += "</div>";
                                    html += "<div class=\"absolute pin-b bg-dark-5 w-full group-hover:block\" style=\"padding-top: 4px;padding-bottom: 4px;\">";
                                    html += "<div class=\"flex justify-around\">";
                                    html += "<div data-user=\"" + Request + "\" data-action=\"accept\" class=\"cursor-pointer-r px-1\">";
                                    html += "<img src=\"" + RoleplayManager.CdnURL + "/ws_resources/images/up-arrow.png\">";
                                    html += "</div>";
                                    html += "<div data-user=\"" + Request + "\" data-action=\"decline\" class=\"cursor-pointer-r px-1\">";
                                    html += "<img src=\"" + RoleplayManager.CdnURL + "/ws_resources/images/cross.png\">";
                                    html += "</div>";
                                    html += "</div>";
                                    html += "</div>";
                                    html += "</div>";
                                    html += "<div class=\"text-center py-1\">" + Name + "</div>";
                                    html += "</div>";

                                    html += "<div class=\"bg-dark-4 rounded m-1 group\" style=\"width: 69%;padding: 5px;\">";
                                    html += "<p>Estad&iacute;sticas principales de "+Name+":</p>";
                                    html += "<br>";
                                    html += "<div style=\"max-height: 108px;overflow: auto;display: inline-flex;\">";
                                    html += "<div class=\"gang_inf_box\">";
                                    html += "<div><b>Nivel</b></div>";
                                    html += "<div>" + String.Format("{0:N0}", Row["level"]) + "</div>";
                                    html += "</div>";
                                    html += "<div class=\"gang_inf_box\">";
                                    html += "<div><b>Reputación</b></div>";
                                    html += "<div>" + String.Format("{0:N0}", Row["curxp"]) + " / " + String.Format("{0:N0}", Row["needxp"]) + "</div>";
                                    html += "</div>";
                                    html += "<div class=\"gang_inf_box\">";
                                    html += "<div><b>Fuerza</b></div>";
                                    html += "<div>" + String.Format("{0:N0}", Row["strength"]) + "</div>";
                                    html += "</div>";
                                    html += "<div class=\"gang_inf_box\">";
                                    html += "<div><b>Arrestos realizados</b></div>";
                                    html += "<div>" + String.Format("{0:N0}", Row["arrests"]) + "</div>";
                                    html += "</div>";
                                    html += "<div class=\"gang_inf_box\">";
                                    html += "<div><b>Veces arrestado</b></div>";
                                    html += "<div>" + String.Format("{0:N0}", Row["arrested"]) + "</div>";
                                    html += "</div>";
                                    html += "</div>";
                                    html += "<br><br>";
                                    html += "<i>M&aacute;s info. en el Perfil del usuario.</i>";
                                    html += "</div>";
                                    html += "</div>";
                                    //<!-- End User box -->
                                }
                            }
                        }

                        if (Counter <= 0)
                            html += "<center><b style='color:red'>No hay solicitudes nuevas.</b></center>";

                        html += "</div>";

                        html += "</div>";
                        html += "</div>";
                        #endregion

                        string SendData = "";
                        SendData += html;
                        Socket.Send("compose_gang|requests|" + SendData);
                    }
                    break;
                #endregion

                #region Edit Tab
                case "edit":
                    {
                        List<Group> Groups = PlusEnvironment.GetGame().GetGroupManager().GetGangsForUser(Client.GetHabbo().Id);

                        if (Groups == null || Groups.Count <= 0)
                            return;

                        #region Check if is not Admin
                        if (!Groups[0].IsAdmin(Client.GetHabbo().Id) && !Client.GetHabbo().GetPermissions().HasRight("corporation_rights"))
                            return;
                        #endregion

                        string html = "";

                        #region HTML
                        html += "<div class=\"mb-2\">";
                        html += "<div class=\"heading\">Retirar dinero de la Banda</div>";
                        html += "<div class=\"flex\">";
                        html += "<input id=\"GA_Withdraw\" type=\"number\" data-lpignore=\"true\" class=\"dark-input-text flex-1 mr-1\" value=\"\" autocomplete=\"off\" placeholder=\"Cantidad de dinero a retirar\">";
                        html += "<button id =\"GA_Withdraw_Btn\" data-action=\"Withdraw\" class=\"dark-button\">Retirar</button>";
                        html += "</div>";

                        html += "<br>";
                        html += "<div class=\"heading\">Transferir banda</div>";
                        html += "<i>Al transferir el mando a otro miembro de la banda, perderás todas las herramientas de administrador en ella.</i><br>";
                        html += "<div class=\"flex\">";
                        html += "<select id=\"GA_Edit_Trans_U\" class=\"dark-button\" style=\"width: 100%;margin-right:5px\">";
                        html += "<option value=\"0\" style=\"color:black\">Seleccionar miembro:</option>";
                        
                        foreach (var Members in Groups[0].GetAllMembersDict)
                        {
                            if (Members.Value.UserId == Client.GetHabbo().Id)
                                continue;

                            string Name = PlusEnvironment.GetGame().GetClientManager().GetNameById(Convert.ToInt32(Members.Value.UserId));// <= Busca en diccionario, Si es Off, hace SELECT directo.

                            html += "<option value=\""+ Name + "\" style=\"color:black\">"+Name+"</option>";
                        }

                        html += "</select>";
                        html += "<button id =\"GA_Edit_Trans\" class=\"dark-button\" data-action=\"Transfer\">Transferir</button>";
                        html += "</div>";

                        html += "<br>";
                        html += "<div class=\"heading\">Nombre de la Banda</div>";
                        html += "<div class=\"flex\">";
                        html += "<input id=\"GA_Edit_Name\" type=\"text\" data-lpignore=\"true\" class=\"dark-input-text flex-1 mr-1\" value=\""+Groups[0].Name+"\" maxlength=\"50\" autocomplete=\"off\" placeholder=\"Escribe aqu&iacute; un nombre para tu banda\">";
                        html += "<button id =\"GA_Edit_Name_Btn\" data-action=\"EditName\" class=\"dark-button\">Cambiar nombre</button>";
                        html += "</div>";

                        string selected1 = (Groups[0].GroupType == GroupType.LOCKED) ? "selected" : "";
                        string selected2 = (Groups[0].GroupType == GroupType.OPEN) ? "selected" : "";
                        html += "<br>";
                        html += "<div class=\"heading\">Tipo de acceso</div>";
                        html += "<div class=\"flex\">";
                        html += "<select id=\"GA_Edit_Type\" class=\"dark-button\" style=\"width: 100%\">";
                        html += "<option value=\"1\" style=\"color:black\" " + selected1 + ">Por invitaci&oacute;n</option>";
                        html += "<option value=\"0\" style=\"color:black\" " + selected2 + ">Abierto (Cualquiera puede unirse)</option>";
                        html += "</select>";
                        html += "</div>";

                        html += "<br>";
                        html += "<div class=\"heading\">Eliminar banda</div>";
                        html += "<i>Al eliminar tu banda todos los datos serán borrados y los miembros expulsados. (No es reversible.)</i><br>";
                        html += "<div class=\"flex\">";
                        html += "<button id =\"GA_Delete_Btn\" class=\"dark-button\" data-action=\"Delete\" style=\"width: 100%\">Eliminar</button>";
                        html += "</div>";
                        html += "</div>";
                        #endregion

                        string SendData = "";
                        SendData += html;
                        Socket.Send("compose_gang|edit|" + SendData);
                    }
                    break;
                #endregion

                #region Edition
                case "edition":
                    {
                        if (Client.GetPlay().TryGetCooldown("gang_edit", true))
                            return;

                        List<Group> Groups = PlusEnvironment.GetGame().GetGroupManager().GetGangsForUser(Client.GetHabbo().Id);

                        if (Groups == null || Groups.Count <= 0)
                            return;

                        #region Check if is not Admin
                        if (!Groups[0].IsAdmin(Client.GetHabbo().Id) && !Client.GetHabbo().GetPermissions().HasRight("corporation_rights"))
                            return;
                        #endregion

                        string[] ReceivedData = Data.Split(',');
                        string WSAction = ReceivedData[1];

                        switch(WSAction)
                        {
                            case "withdraw":
                                {
                                    int cant = 0;
                                    if (!int.TryParse(ReceivedData[2], out cant))
                                    {
                                        Socket.Send("compose_gang|msg_error|Cantidad de dinero inválida.");
                                        return;
                                    }

                                    if (Groups[0].GetAdministrator.Count <= 0)
                                    {
                                        Socket.Send("compose_gang|msg_error|Esta empresa pertenece al Gobierno y no es posible retirar dinero.");
                                        return;
                                    }

                                    if (Groups[0].Bank < cant)
                                    {
                                        Socket.Send("compose_gang|msg_error|La banda no cuenta con esa cantidad en su Riqueza para retirar.");
                                        return;
                                    }

                                    if (cant <= 0)
                                    {
                                        Socket.Send("compose_gang|msg_error|Debes retirar una cantidad mayor a $ 0.");
                                        return;
                                    }

                                    Client.GetHabbo().Credits += cant;
                                    Client.GetPlay().MoneyEarned += cant;
                                    Client.GetHabbo().UpdateCreditsBalance();

                                    Groups[0].Bank -= cant;
                                    Groups[0].SetBussines(Groups[0].Bank, Groups[0].Stock);

                                    PlusEnvironment.GetGame().GetBusinessBalanceManager().AddBusinessBalance(Client.GetHabbo().Id, Groups[0].Id, "withdraw", cant);

                                    RoleplayManager.Shout(Client, "*Ha retirado $ " + String.Format("{0:N0}", cant) + " de la riqueza de la banda " + Groups[0].Name + "*", 5);

                                    Socket.Send("compose_gang|msg_success|Retiro realizado exitosamente.");
                                    PlusEnvironment.GetGame().GetWebEventManager().ExecuteWebEvent(Client, "event_gang", "edit");
                                }
                                break;
                            case "newname":
                                {
                                    string NewName = ReceivedData[2];
                                    NewName = System.Text.RegularExpressions.Regex.Replace(NewName, "<(.|\\n)*?>", string.Empty);

                                    if (NewName.Length < 3 || String.IsNullOrEmpty(NewName))
                                    {
                                        Socket.Send("compose_gang|msg_error|El nombre de tu banda debe tener al menos 3 caracteres.");
                                        return;
                                    }

                                    Groups[0].UpdateGroupName(NewName);
                                    Socket.Send("compose_gang|msg_success|Nombre de la banda actualizado correctamente.");

                                    foreach (Room Room in PlusEnvironment.GetGame().GetRoomManager().GetRooms())
                                    {
                                        if (Room == null || Room.Group == null || Room.Group.Id != Groups[0].Id)
                                            continue;

                                        foreach (RoomUser RoomUser in Room.GetRoomUserManager().GetRoomUsers())
                                        {
                                            if (RoomUser == null || RoomUser.GetClient() == null)
                                                continue;

                                            PlusEnvironment.GetGame().GetWebEventManager().ExecuteWebEvent(RoomUser.GetClient(), "event_group", "open");
                                        }
                                    }
                                }
                                break;
                            case "newtype":
                                {
                                    int NewType = 0;

                                    if (!int.TryParse(ReceivedData[2], out NewType))
                                    {
                                        Socket.Send("compose_gang|msg_error|Tipo de acceso inválido.");
                                        return;
                                    }

                                    Groups[0].UpdateGroupAccessType(NewType);
                                    Socket.Send("compose_gang|msg_success|Tipo de acceso a la banda actualizado correctamente.");

                                    foreach (Room Room in PlusEnvironment.GetGame().GetRoomManager().GetRooms())
                                    {
                                        if (Room == null || Room.Group == null || Room.Group.Id != Groups[0].Id)
                                            continue;

                                        foreach (RoomUser RoomUser in Room.GetRoomUserManager().GetRoomUsers())
                                        {
                                            if (RoomUser == null || RoomUser.GetClient() == null)
                                                continue;

                                            PlusEnvironment.GetGame().GetWebEventManager().ExecuteWebEvent(RoomUser.GetClient(), "event_group", "open");
                                        }
                                    }
                                }
                                break;
                            case "transfer":
                                {
                                    string NewAdmin = ReceivedData[2];
                                    NewAdmin = System.Text.RegularExpressions.Regex.Replace(NewAdmin, "<(.|\\n)*?>", string.Empty);

                                    Habbo Target = PlusEnvironment.GetHabboByUsername(NewAdmin);

                                    if(Target == null)
                                    {
                                        Socket.Send("compose_gang|msg_error|No se encontró a ningún usuario con ese nombre.");
                                        return;
                                    }

                                    if (Target == Client.GetHabbo())
                                    {
                                        Socket.Send("compose_gang|msg_error|¡Tú ya eres el líder!");
                                        return;
                                    }

                                    if(!Groups[0].IsMember(Target.Id))
                                    {
                                        Socket.Send("compose_gang|msg_error|¡Esa persona no es miembro de tu banda!");
                                        return;
                                    }

                                    var AllMembers = Groups[0].GetAllMembersDict;
                                    int GetRank = Groups[0].Members[Target.Id].UserRank;
                                    int newrank = Groups[0].Members[Client.GetHabbo().Id].UserRank;

                                    using (var DB = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
                                    {
                                        // Quitamos rank a admin actual
                                        // MYSQL PROCEDIMIENTO
                                        DB.RunQuery("CALL `ModifMember`(" + newrank + ", " + GetRank + ", " + Groups[0].Id + ", " + Client.GetHabbo().Id + ");");

                                        AllMembers.Where(x => x.Value.UserId == Client.GetHabbo().Id).ToList().ForEach(x => x.Value.UserRank = GetRank);

                                        // Asignamos el nuevo admin
                                        // MYSQL PROCEDIMIENTO
                                        DB.RunQuery("CALL `ModifMember`(" + GetRank + ", " + newrank + ", " + Groups[0].Id + ", " + Target.Id + ");");

                                        AllMembers.Where(x => x.Value.UserId == Target.Id).ToList().ForEach(x => x.Value.UserRank = newrank);

                                    }

                                    // Quita al old admin y coloca al nuevo admin
                                    Groups[0].MakeAdmin(Target.Id);
                                    // Colocamos el nuevo owner en la tabla groups (DB)
                                    Groups[0].MakeOwner(Target.Id);

                                    Socket.Send("compose_gang|msg_success|Banda transeferida con éxito. Ahora pertenece a " + Target.Username);
                                    RoleplayManager.Shout(Client, "*Le ha transferido el mandato a "+Target.Username+" de la banda "+Groups[0].Name+"*", 5);
                                    PlusEnvironment.GetGame().GetWebEventManager().ExecuteWebEvent(Client, "event_gang", "mynew");

                                    foreach (Room Room in PlusEnvironment.GetGame().GetRoomManager().GetRooms())
                                    {
                                        if (Room == null || Room.Group == null || Room.Group.Id != Groups[0].Id)
                                            continue;

                                        foreach (RoomUser RoomUser in Room.GetRoomUserManager().GetRoomUsers())
                                        {
                                            if (RoomUser == null || RoomUser.GetClient() == null)
                                                continue;

                                            PlusEnvironment.GetGame().GetWebEventManager().ExecuteWebEvent(RoomUser.GetClient(), "event_group", "open");
                                        }
                                    }

                                    if (Target.GetClient() != null)
                                    {
                                        RoleplayManager.Shout(Client, "*Obtiene el mandato de la banda " + Groups[0].Name + "*", 5);
                                        Target.GetClient().SendNotification("¡Ahora eres el líder de la banda "+Groups[0].Name+"! Vuelve a dar clic a la pestaña \"Mi banda\" del panel para ver las nuevas herramientas administrativas.");
                                    }
                                }
                                break;
                            case "delete":
                                {
                                    foreach (Room Room in PlusEnvironment.GetGame().GetRoomManager().GetRooms())
                                    {
                                        if (Room == null || Room.Group == null || Room.Group.Id != Groups[0].Id)
                                            continue;

                                        Room.Group = null;
                                        Room.RoomData.Group = null;//I'm not sure if this is needed or not, becauseof inheritance, but oh well.
                                    }

                                    //Remove it from the cache.
                                    PlusEnvironment.GetGame().GetGroupManager().DeleteGroup(Groups[0].Id);

                                    //Now the :S stuff.
                                    using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
                                    {
                                        dbClient.RunQuery("DELETE FROM `groups` WHERE `id` = '" + Groups[0].Id + "'");
                                        dbClient.RunQuery("DELETE FROM `group_memberships` WHERE `group_id` = '" + Groups[0].Id + "'");
                                        dbClient.RunQuery("DELETE FROM `group_requests` WHERE `group_id` = '" + Groups[0].Id + "'");
                                        dbClient.RunQuery("UPDATE `rooms` SET `group_id` = '0' WHERE `group_id` = '" + Groups[0].Id + "' LIMIT 1");
                                        dbClient.RunQuery("UPDATE `user_stats` SET `groupid` = '0' WHERE `groupid` = '" + Groups[0].Id + "' LIMIT 1");

                                        //dbClient.RunQuery("DELETE FROM `items_groups` WHERE `group_id` = '" + Groups[0].Id + "'");
                                        dbClient.SetQuery("SELECT items_groups.id FROM items_groups, items, rooms WHERE items_groups.group_id = '" + Groups[0].Id + "' AND items_groups.id = items.id AND items.room_id = rooms.id AND rooms.roomtype = 'private' LIMIT 1;");
                                        DataRow Row = dbClient.getRow();
                                        if (Row != null)
                                        {
                                            dbClient.SetQuery("DELETE FROM `items_groups` WHERE `id` = @GFLAG;");
                                            dbClient.AddParameter("GFLAG", Convert.ToInt32(Row["id"]));
                                            dbClient.RunQuery();
                                        }

                                        dbClient.RunQuery("DELETE FROM `groups_logs` WHERE `group_id` = '" + Groups[0].Id + "'");
                                        dbClient.RunQuery("DELETE FROM `play_jobs_ranks` WHERE `job` = '" + Groups[0].Id + "'");
                                        dbClient.RunQuery("UPDATE `play_stats` SET `gang` = '0' WHERE `gang` = '" + Groups[0].Id + "' LIMIT 1");
                                        dbClient.RunQuery("UPDATE `items_groups` SET `group_id` = '0' WHERE `group_id` = '" + Groups[0].Id + "' LIMIT 1");
                                    }

                                    PlusEnvironment.GetGame().GetWebEventManager().ExecuteWebEvent(Client, "event_gang", "mynew");
                                    PlusEnvironment.GetGame().GetWebEventManager().ExecuteWebEvent(Client, "event_gang", "close");

                                    foreach (Room Room in PlusEnvironment.GetGame().GetRoomManager().GetRooms())
                                    {
                                        if (Room == null || Room.Group == null || Room.Group.Id != Groups[0].Id)
                                            continue;

                                        //Unload it last.
                                        PlusEnvironment.GetGame().GetRoomManager().UnloadRoom(Room, true);
                                    }

                                    //Say hey!
                                    Client.SendNotification("Banda eliminada satisfactoriamente.");
                                    Client.GetPlay().Gang = 0;
                                }
                                break;
                            case "savegang":
                                {
                                    if (Client.GetHabbo().Credits < (RoleplayManager.GangsPrice / 4))
                                    {
                                        Socket.Send("compose_gang|msg_error|No tienes el dinero suficiente para pagar tu deuda.");
                                        return;
                                    }

                                    Client.GetHabbo().Credits -= (RoleplayManager.GangsPrice / 4);
                                    Client.GetHabbo().UpdateCreditsBalance();

                                    Groups[0].Bank = 0;
                                    Groups[0].SetBussines(Groups[0].Bank, Groups[0].Stock);
                                    Socket.Send("compose_gang|msg_success|Deuda pagada exitosamente. Tu banda ya puede seguir operando.");
                                    PlusEnvironment.GetGame().GetWebEventManager().ExecuteWebEvent(Client, "event_gang", "edit");
                                    RoleplayManager.Shout(Client, "*Ha pagado la deuda de su banda "+Groups[0].Name+" salvándola de la bancarota*", 5);

                                }
                                break;
                            default:
                                break;
                        }

                        Client.GetPlay().CooldownManager.CreateCooldown("gang_edit", 1000, 10);
                    }
                    break;
                #endregion

                #region Stats
                case "stats":
                    {
                        List<Group> Groups = PlusEnvironment.GetGame().GetGroupManager().GetGangsForUser(Client.GetHabbo().Id);

                        if (Groups == null || Groups.Count <= 0)
                            return;

                        string html = "";

                        #region HTML
                        html += "<div class=\"flex mb-2 items-center justify-center\">";
                        html += "<div id=\"Tool_Colours\" class=\"mr-3 colours-3DCMW_0\">";
                        html += "<img src=\"https://swf.habbovip.us/group-badge/badge/" + Groups[0].GetBadge() + "\" draggable=\"false\" ondragstart=\"return false;\">";
                        html += "</div>";
                        html += "<div id=\"Tool_Text\" class=\"text-3xl font-bold uppercase text-white border-b-4 border-dark-3\">";
                        html += Groups[0].Name;
                        html += "</div>";
                        html += "</div>";

                        if (Groups[0].BankRuptcy)
                        {
                            html += "<div class=\"heading\" style=\"background-color:red\">¡Tu banda est&aacute; en banca rota!</div>";
                            html += "<div class=\"-m-1 flex flex-wrap justify-around\">";
                            html += "Tu banda no puede seguir gozando de beneficios económicos estando en banca rota. Puedes pagar la deuda para salvarla o bien, eliminarla.";
                            html += "<button id =\"GA_Save\" data-action=\"SaveGang\" class=\"dark-button\" style=\"width: 99%;\">Salvar banda ($ " + String.Format("{0:N0}", (RoleplayManager.GangsPrice / 4))+")</button>";
                            html += "</div><br>";
                        }

                        string Founder = "Desconocido";
                        if (Groups[0].GetAdministrator.Count > 0)
                        {
                            Founder = PlusEnvironment.GetGame().GetClientManager().GetNameById(Convert.ToInt32(Groups[0].GetAdministrators[0]));// <= Busca en diccionario, Si es Off, hace SELECT directo.
                        }
                        html += "<div class=\"heading\">Estad&iacute;sticas</div>";
                        html += "<div class=\"-m-1 flex flex-wrap justify-around\">";
                        html += "<div class=\"flex bg-dark-1 m-1 rounded-lg p-1 pl-2 items-center\" style=\"width: 45%;\">";
                        html += "<div class=\"mr-2\">L&iacute;der</div>";
                        html += "<div class=\"bg-dark-2 px-2 py-1 rounded-lg ml-auto\">"+ Founder + "</div>";
                        html += "</div>";
                        html += "<div class=\"flex bg-dark-1 m-1 rounded-lg p-1 pl-2 items-center\" style=\"width: 45%;\">";
                        html += "<div class=\"mr-2\">Fundado el</div>";
                        html += "<div class=\"Shifts bg-dark-2 px-2 py-1 rounded-lg ml-auto\">"+ PlusEnvironment.UnixTimeStampToDateTime(Groups[0].CreateTime).ToString("dd MMMM\\, yyyy")+"</div>";
                        html += "</div>";
                        html += "<div class=\"flex bg-dark-1 m-1 rounded-lg p-1 pl-2 items-center\" style=\"width: 45%;\">";
                        html += "<div class=\"mr-2\">Riqueza</div>";
                        html += "<div class=\"bg-dark-2 px-2 py-1 rounded-lg ml-auto\">$ "+String.Format("{0:N0}", Groups[0].Bank)+"</div>";
                        html += "</div>";

                        int NewTurfsCount = 0;
                        List<GangTurfs> TF = PlusEnvironment.GetGame().GetGangTurfsManager().getTurfsbyGang(Groups[0].Id);
                        if (TF != null && TF.Count > 0)
                            NewTurfsCount = TF.Count;

                        html += "<div class=\"flex bg-dark-1 m-1 rounded-lg p-1 pl-2 items-center\" style=\"width: 45%;\">";
                        html += "<div class=\"mr-2\">Barrios en posesi&oacute;n</div>";
                        html += "<div class=\"Shifts bg-dark-2 px-2 py-1 rounded-lg ml-auto\">"+ String.Format("{0:N0}", NewTurfsCount) + "</div>";
                        html += "</div>";
                        html += "<div class=\"flex bg-dark-1 m-1 rounded-lg p-1 pl-2 items-center\" style=\"width: 45%;\">";
                        html += "<div class=\"mr-2\">Asesinatos</div>";
                        html += "<div class=\"bg-dark-2 px-2 py-1 rounded-lg ml-auto\">" + String.Format("{0:N0}", Groups[0].GangKills) + "</div>";
                        html += "</div>";
                        html += "<div class=\"flex bg-dark-1 m-1 rounded-lg p-1 pl-2 items-center\" style=\"width: 45%;\">";
                        html += "<div class=\"mr-2\">Asesinatos a polic&iacute;as</div>";
                        html += "<div class=\"Shifts bg-dark-2 px-2 py-1 rounded-lg ml-auto\">" + String.Format("{0:N0}", Groups[0].GangCopKills) + "</div>";
                        html += "</div>";
                        html += "<div class=\"flex bg-dark-1 m-1 rounded-lg p-1 pl-2 items-center\" style=\"width: 45%;\">";
                        html += "<div class=\"mr-2\">Muertes</div>";
                        html += "<div class=\"bg-dark-2 px-2 py-1 rounded-lg ml-auto\">" + String.Format("{0:N0}", Groups[0].GangDeaths) + "</div>";
                        html += "</div>";
                        html += "<div class=\"flex bg-dark-1 m-1 rounded-lg p-1 pl-2 items-center\" style=\"width: 45%;\">";
                        html += "<div class=\"mr-2\">Atracos</div>";
                        html += "<div class=\"Shifts bg-dark-2 px-2 py-1 rounded-lg ml-auto\">" + String.Format("{0:N0}", Groups[0].GangHeists) + "</div>";
                        html += "</div>";
                        html += "<div class=\"flex bg-dark-1 m-1 rounded-lg p-1 pl-2 items-center\" style=\"width: 45%;\">";
                        html += "<div class=\"mr-2\">Barrios capturados</div>";
                        html += "<div class=\"bg-dark-2 px-2 py-1 rounded-lg ml-auto\">" + String.Format("{0:N0}", Groups[0].GangTurfsTaken) + "</div>";
                        html += "</div>";
                        html += "<div class=\"flex bg-dark-1 m-1 rounded-lg p-1 pl-2 items-center\" style=\"width: 45%;\">";
                        html += "<div class=\"mr-2\">Barrios defendidos</div>";
                        html += "<div class=\"Shifts bg-dark-2 px-2 py-1 rounded-lg ml-auto\">" + String.Format("{0:N0}", Groups[0].GangTurfsDefended) + "</div>";
                        html += "</div>";
                        html += "<div class=\"flex bg-dark-1 m-1 rounded-lg p-1 pl-2 items-center\" style=\"width: 45%;\">";
                        html += "<div class=\"mr-2\">Cocaína producida</div>";
                        html += "<div class=\"bg-dark-2 px-2 py-1 rounded-lg ml-auto\">" + String.Format("{0:N0}", Groups[0].GangFarmCocaine) + "</div>";
                        html += "</div>";
                        html += "<div class=\"flex bg-dark-1 m-1 rounded-lg p-1 pl-2 items-center\" style=\"width: 45%;\">";
                        html += "<div class=\"mr-2\">Medicamentos producidos</div>";
                        html += "<div class=\"Shifts bg-dark-2 px-2 py-1 rounded-lg ml-auto\">" + String.Format("{0:N0}", Groups[0].GangFarmMedicines) + "</div>";
                        html += "</div>";
                        html += "<div class=\"flex bg-dark-1 m-1 rounded-lg p-1 pl-2 items-center\" style=\"width: 45%;\">";
                        html += "<div class=\"mr-2\">Marihuana producida</div>";
                        html += "<div class=\"bg-dark-2 px-2 py-1 rounded-lg ml-auto\">" + String.Format("{0:N0}", Groups[0].GangFarmWeed) + "</div>";
                        html += "</div>";
                        html += "<div class=\"flex bg-dark-1 m-1 rounded-lg p-1 pl-2 items-center\" style=\"width: 45%;\">";
                        html += "<div class=\"mr-2\">Armas fabricadas</div>";
                        html += "<div class=\"Shifts bg-dark-2 px-2 py-1 rounded-lg ml-auto\">" + String.Format("{0:N0}", Groups[0].GangFabGuns) + "</div>";
                        html += "</div>";
                        html += "</div>";

                        html += "<br><div class=\"heading\">Historial de Actividades</div>";
                        html += "<div class=\"-m-1 flex flex-wrap justify-around\" style=\"margin-bottom: 5px;height: 252px;max-height: 252px;overflow: auto;\">";

                        List<GroupLogs> Logs = Groups[0].getAllLogs();
                        //Logs.Reverse();
                        if (Logs != null && Logs.Count > 0)
                        {
                            html += "<table id=\"financelist\">";

                            foreach (var B in Logs)
                            {
                                Habbo hbo = PlusEnvironment.GetHabboById(B.UserId);
                                if (hbo == null)
                                    continue;

                                html += "<tr>";
                                html += "<td>";
                                html += "<img src=\"" + RoleplayManager.AVATARIMG + "" + hbo.Look + "&headonly=1\">";
                                html += B.Action + " (" + B.TimeStamp.ToString("dd\\/MM\\/yyyy") + ")";
                                html += "</td>";
                                html += "</tr>";
                                html += "<tr>";
                                html += "</tr>";
                            }

                            html += "</table>";
                        }

                        html += "</div>";
                        html += "</div>";

                        html += "</div>";
                        #endregion

                        string SendData = "";
                        SendData += html;
                        Socket.Send("compose_gang|stats|" + SendData);
                    }
                    break;
                #endregion

                #region Search Gang
                case "search":
                    {
                        string[] ReceivedData = Data.Split(',');
                        string Search = ReceivedData[1];

                        if (Search.Length <= 0 || String.IsNullOrEmpty(Search))
                        {
                            PlusEnvironment.GetGame().GetWebEventManager().ExecuteWebEvent(Client, "event_gang", "open");
                            return;
                        }

                        string html = "";
                        int Counter = 0;

                        foreach (Group group in PlusEnvironment.GetGame().GetGroupManager().Groups.Where(x => x.Name.ToLower() == Search.ToLower()).ToList())
                        {
                            if (group.GType != 3)
                                continue;

                            Counter++;
                            html += "<div class=\"flex bg-dark-1 m-1 rounded-lg p-1 pl-2 items-center\" style=\"width: 45.5%;\">";
                            html += "<div class=\"mr-2\">";
                            html += "<p>" + group.Name + "</p>";
                            html += "<p>" + group.GetAllMembers.Count() + " miembro(s)</p>";
                            html += "</div>";
                            html += "<div class=\"bg-dark-2 px-2 py-1 rounded-lg ml-auto data-gang\" data-balloon=\"Ver info\" data-balloon-pos=\"left\" data-gang=\"" + group.Id + "\"><img src=\"https://swf.habbovip.us/group-badge/badge/" + group.GetBadge() + "\" draggable=\"false\" ondragstart=\"return false;\" style=\"cursor: pointer;\"></div>";
                            html += "</div>";
                        }

                        if (Counter <= 0)
                            html = "<center><b style='color:red'>No se encontraron resultados para \""+Search+"\"</b></center>";

                        Socket.Send("compose_gang|gang_list|" + html + "|");
                    }
                    break;
                #endregion

                #region View
                case "view":
                    {
                        string[] ReceivedData = Data.Split(',');
                        int GetGangId = 0;

                        if (!int.TryParse(ReceivedData[1], out GetGangId))
                            return;

                        Group thegroup;
                        if (!PlusEnvironment.GetGame().GetGroupManager().TryGetGroup(GetGangId, out thegroup))
                            return;

                        string html = "";

                        #region HTML
                        html += "<div>";
                        html += "<div class=\"-m-2\">";

                        var AllRanks = thegroup.Ranks.OrderBy(o => o.Value.RankId).ToList();
                        AllRanks.Reverse();
                        foreach (var Ranks in AllRanks)
                        {
                            //<!-- Rank box -->
                            html += "<div class=\"m-2\">";
                            //<!-- Rank Header -->
                            html += "<div class=\"heading relative group\">";
                            html += "<div>" + Ranks.Value.Name + "</div>";
                            html += "</div>";

                            // < !-- User box -->
                            html += "<div class=\"flex flex-wrap -m-1 justify-center\">";

                            foreach (var Members in thegroup.GetAllMembersDict)
                            {
                                if (Members.Value.UserRank == Ranks.Value.RankId)
                                {
                                    string Name = PlusEnvironment.GetGame().GetClientManager().GetNameById(Convert.ToInt32(Members.Value.UserId));// <= Busca en diccionario, Si es Off, hace SELECT directo.
                                    string Look = PlusEnvironment.GetGame().GetClientManager().GetLookById(Convert.ToInt32(Members.Value.UserId));// <= Busca en diccionario, Si es Off, hace SELECT directo.

                                    //<!-- User info -->
                                    html += "<div class=\"bg-dark-4 rounded m-1 group cursor-pointer-r\">";
                                    html += "<div class=\"m-px relative\">";
                                    html += "<div class=\"overflow-hidden bg-light-05 rounded-t\" style=\"height: 55px;\">";
                                    html += "<center><div class=\"figure-H_RWF_0\" style=\"background-image: url(&quot;" + RoleplayManager.AVATARIMG + "" + Look + "&quot;); width: 64px; height: 110px; margin-top: -20px;\"></div></center>";
                                    html += "</div>";
                                    html += "</div>";
                                    html += "<div class=\"text-center py-1\">" + Name + "</div>";
                                    html += "</div>";
                                }
                            }

                            html += "</div>";
                            // < !-- End User box -->

                            html += "</div>";
                        }
                        html += "</div>";
                        html += "</div>";
                        #endregion

                        string SendData = "";
                        SendData += html;
                        Socket.Send("compose_gang|view|" + SendData + "|" + GetGangId);
                    }
                    break;
                #endregion

                #region View Stats
                case "view_stats":
                    {
                        string[] ReceivedData = Data.Split(',');
                        int GetGangId = 0;

                        if (!int.TryParse(ReceivedData[1], out GetGangId))
                            return;

                        Group thegroup;
                        if (!PlusEnvironment.GetGame().GetGroupManager().TryGetGroup(GetGangId, out thegroup))
                            return;

                        string html = "";

                        #region HTML
                        html += "<div class=\"flex mb-2 items-center justify-center\">";
                        html += "<div id=\"Tool_Colours\" class=\"mr-3 colours-3DCMW_0\">";
                        html += "<img src=\"https://swf.habbovip.us/group-badge/badge/" + thegroup.GetBadge() + "\" draggable=\"false\" ondragstart=\"return false;\">";
                        html += "</div>";
                        html += "<div id=\"Tool_Text\" class=\"text-3xl font-bold uppercase text-white border-b-4 border-dark-3\">";
                        html += thegroup.Name;
                        html += "</div>";
                        html += "</div>";
                        string Founder = "Desconocido";
                        if (thegroup.GetAdministrator.Count > 0)
                        {
                            Founder = PlusEnvironment.GetGame().GetClientManager().GetNameById(Convert.ToInt32(thegroup.GetAdministrators[0]));// <= Busca en diccionario, Si es Off, hace SELECT directo.
                        }
                        html += "<div class=\"heading\">Estad&iacute;sticas</div>";
                        html += "<div class=\"-m-1 flex flex-wrap justify-around\">";
                        html += "<div class=\"flex bg-dark-1 m-1 rounded-lg p-1 pl-2 items-center\" style=\"width: 45%;\">";
                        html += "<div class=\"mr-2\">L&iacute;der</div>";
                        html += "<div class=\"bg-dark-2 px-2 py-1 rounded-lg ml-auto\">" + Founder + "</div>";
                        html += "</div>";
                        html += "<div class=\"flex bg-dark-1 m-1 rounded-lg p-1 pl-2 items-center\" style=\"width: 45%;\">";
                        html += "<div class=\"mr-2\">Fundado el</div>";
                        html += "<div class=\"Shifts bg-dark-2 px-2 py-1 rounded-lg ml-auto\">" + PlusEnvironment.UnixTimeStampToDateTime(thegroup.CreateTime).ToString("dd MMMM\\, yyyy") + "</div>";
                        html += "</div>";
                        html += "<div class=\"flex bg-dark-1 m-1 rounded-lg p-1 pl-2 items-center\" style=\"width: 45%;\">";
                        html += "<div class=\"mr-2\">Riqueza</div>";
                        html += "<div class=\"bg-dark-2 px-2 py-1 rounded-lg ml-auto\">$ " + String.Format("{0:N0}", thegroup.Bank) + "</div>";
                        html += "</div>";

                        int NewTurfsCount = 0;
                        List<GangTurfs> TF = PlusEnvironment.GetGame().GetGangTurfsManager().getTurfsbyGang(thegroup.Id);
                        if (TF != null && TF.Count > 0)
                            NewTurfsCount = TF.Count;

                        html += "<div class=\"flex bg-dark-1 m-1 rounded-lg p-1 pl-2 items-center\" style=\"width: 45%;\">";
                        html += "<div class=\"mr-2\">Barrios en posesi&oacute;n</div>";
                        html += "<div class=\"Shifts bg-dark-2 px-2 py-1 rounded-lg ml-auto\">" + String.Format("{0:N0}", NewTurfsCount) + "</div>";
                        html += "</div>";
                        html += "<div class=\"flex bg-dark-1 m-1 rounded-lg p-1 pl-2 items-center\" style=\"width: 45%;\">";
                        html += "<div class=\"mr-2\">Asesinatos</div>";
                        html += "<div class=\"bg-dark-2 px-2 py-1 rounded-lg ml-auto\">" + String.Format("{0:N0}", thegroup.GangKills) + "</div>";
                        html += "</div>";
                        html += "<div class=\"flex bg-dark-1 m-1 rounded-lg p-1 pl-2 items-center\" style=\"width: 45%;\">";
                        html += "<div class=\"mr-2\">Asesinatos a polic&iacute;as</div>";
                        html += "<div class=\"Shifts bg-dark-2 px-2 py-1 rounded-lg ml-auto\">" + String.Format("{0:N0}", thegroup.GangCopKills) + "</div>";
                        html += "</div>";
                        html += "<div class=\"flex bg-dark-1 m-1 rounded-lg p-1 pl-2 items-center\" style=\"width: 45%;\">";
                        html += "<div class=\"mr-2\">Muertes</div>";
                        html += "<div class=\"bg-dark-2 px-2 py-1 rounded-lg ml-auto\">" + String.Format("{0:N0}", thegroup.GangDeaths) + "</div>";
                        html += "</div>";
                        html += "<div class=\"flex bg-dark-1 m-1 rounded-lg p-1 pl-2 items-center\" style=\"width: 45%;\">";
                        html += "<div class=\"mr-2\">Atracos</div>";
                        html += "<div class=\"Shifts bg-dark-2 px-2 py-1 rounded-lg ml-auto\">" + String.Format("{0:N0}", thegroup.GangHeists) + "</div>";
                        html += "</div>";
                        html += "<div class=\"flex bg-dark-1 m-1 rounded-lg p-1 pl-2 items-center\" style=\"width: 45%;\">";
                        html += "<div class=\"mr-2\">Barrios capturados</div>";
                        html += "<div class=\"bg-dark-2 px-2 py-1 rounded-lg ml-auto\">" + String.Format("{0:N0}", thegroup.GangTurfsTaken) + "</div>";
                        html += "</div>";
                        html += "<div class=\"flex bg-dark-1 m-1 rounded-lg p-1 pl-2 items-center\" style=\"width: 45%;\">";
                        html += "<div class=\"mr-2\">Barrios defendidos</div>";
                        html += "<div class=\"Shifts bg-dark-2 px-2 py-1 rounded-lg ml-auto\">" + String.Format("{0:N0}", thegroup.GangTurfsDefended) + "</div>";
                        html += "</div>";
                        html += "<div class=\"flex bg-dark-1 m-1 rounded-lg p-1 pl-2 items-center\" style=\"width: 45%;\">";
                        html += "<div class=\"mr-2\">Cocaína producida</div>";
                        html += "<div class=\"bg-dark-2 px-2 py-1 rounded-lg ml-auto\">" + String.Format("{0:N0}", thegroup.GangFarmCocaine) + "</div>";
                        html += "</div>";
                        html += "<div class=\"flex bg-dark-1 m-1 rounded-lg p-1 pl-2 items-center\" style=\"width: 45%;\">";
                        html += "<div class=\"mr-2\">Medicamentos producidos</div>";
                        html += "<div class=\"Shifts bg-dark-2 px-2 py-1 rounded-lg ml-auto\">" + String.Format("{0:N0}", thegroup.GangFarmMedicines) + "</div>";
                        html += "</div>";
                        html += "<div class=\"flex bg-dark-1 m-1 rounded-lg p-1 pl-2 items-center\" style=\"width: 45%;\">";
                        html += "<div class=\"mr-2\">Marihuana producida</div>";
                        html += "<div class=\"bg-dark-2 px-2 py-1 rounded-lg ml-auto\">" + String.Format("{0:N0}", thegroup.GangFarmWeed) + "</div>";
                        html += "</div>";
                        html += "<div class=\"flex bg-dark-1 m-1 rounded-lg p-1 pl-2 items-center\" style=\"width: 45%;\">";
                        html += "<div class=\"mr-2\">Armas fabricadas</div>";
                        html += "<div class=\"Shifts bg-dark-2 px-2 py-1 rounded-lg ml-auto\">" + String.Format("{0:N0}", thegroup.GangFabGuns) + "</div>";
                        html += "</div>";
                        html += "</div>";

                        html += "<br><div class=\"heading\">Historial de Actividades</div>";
                        html += "<div class=\"-m-1 flex flex-wrap justify-around\" style=\"margin-bottom: 5px;height: 252px;max-height: 252px;overflow: auto;\">";

                        List<GroupLogs> Logs = thegroup.getAllLogs();
                        //Logs.Reverse();
                        if (Logs != null && Logs.Count > 0)
                        {
                            html += "<table id=\"financelist\">";

                            foreach (var B in Logs)
                            {
                                Habbo hbo = PlusEnvironment.GetHabboById(B.UserId);
                                if (hbo == null)
                                    continue;

                                html += "<tr>";
                                html += "<td>";
                                html += "<img src=\"" + RoleplayManager.AVATARIMG + "" + hbo.Look + "&headonly=1\">";
                                html += B.Action + " (" + B.TimeStamp.ToString("dd\\/MM\\/yyyy") + ")";
                                html += "</td>";
                                html += "</tr>";
                                html += "<tr>";
                                html += "</tr>";
                            }

                            html += "</table>";
                        }

                        html += "</div>";
                        html += "</div>";
                        
                        html += "</div>";
                        #endregion

                        string SendData = "";
                        SendData += html;
                        Socket.Send("compose_gang|view_stats|" + SendData + "|" + GetGangId);
                    }
                    break;
                #endregion

                #region Turf Capturing Window
                case "turf_cap_w":
                    {
                        string[] ReceivedData = Data.Split(',');

                        int RoomId = 0;
                        if (!int.TryParse(ReceivedData[1], out RoomId))
                            return;

                        Room Room = RoleplayManager.GenerateRoom(RoomId);
                        if (Room == null)
                            return;

                        int UserAtackId = 0;
                        if (!int.TryParse(ReceivedData[2], out UserAtackId))
                            return;

                        string TurfName = ReceivedData[3];

                        List<Group> GangAtack = PlusEnvironment.GetGame().GetGroupManager().GetGangsForUser(UserAtackId);
                        if (GangAtack == null || GangAtack.Count <= 0)
                            return;

                        string GangAtackedName = "nadie";

                        if (Room.Group != null)
                            GangAtackedName = Room.Group.Name;

                        #region Info
                        string Info = "";
                        Info += "<div> Controlado por <span class=\"font-bold pointer-events-auto cursor-pointer hover:underline\">"+ GangAtackedName + "</span><br>";
                        Info += "</div>";
                        Info += "<div class=\"mt-2\"> Atacado por <span class=\"font-bold pointer-events-auto cursor-pointer hover:underline\">"+GangAtack[0].Name+"</span>";
                        Info += "</div>";
                        #endregion

                        int Per = 0;
                        GameClient TargetSession = PlusEnvironment.GetGame().GetClientManager().GetClientByUserID(UserAtackId);
                        if(TargetSession == null)
                        {                            
                            PlusEnvironment.GetGame().GetWebEventManager().ExecuteWebEvent(Client, "event_gang", "turf_cap_off");
                            Room.TurfCapturing = false;
                            return;
                        }

                        Per = ((RoleplayManager.TurfCapTime - TargetSession.GetPlay().LoadingTimeLeft) * 100) / RoleplayManager.TurfCapTime;

                        if (Per >= 100)
                        {
                            Per = 100;
                            Room.TurfCapturing = false;
                        }
                        else if (!TargetSession.GetPlay().TurfCapturing)
                        {                            
                            PlusEnvironment.GetGame().GetWebEventManager().ExecuteWebEvent(Client, "event_gang", "turf_cap_off");
                            Room.TurfCapturing = false;
                            return;
                        }

                        string SendData = "";
                        SendData += TurfName + "|";
                        SendData += Info + "|";
                        SendData += "Capturando "+Per+"%|";
                        SendData += Per + "|";
                        Socket.Send("compose_gang|capturing|" + SendData);

                        if(Per >= 100)
                        {                            
                            PlusEnvironment.GetGame().GetWebEventManager().ExecuteWebEvent(Client, "event_gang", "turf_cap_off");
                            Room.TurfCapturing = false;
                        }
                    }
                    break;
                #endregion

                #region Turf Capturing Off
                case "turf_cap_off":
                    {
                        Socket.Send("compose_gang|capturing_off");
                    }
                    break;
                    #endregion
            }
        }
    }
}
