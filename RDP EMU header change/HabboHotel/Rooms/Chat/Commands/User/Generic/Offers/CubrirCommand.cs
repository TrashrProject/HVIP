using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

using Plus.HabboHotel.Groups;
using Plus.HabboHotel.GameClients;
using Plus.HabboRoleplay.Misc;
using Plus.HabboRoleplay.Weapons;
using Plus.HabboHotel.Items;
using Plus.HabboRoleplay.Vehicles;
using System.Data;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Users.Generic.Offers
{
    class CubrirCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_cubrir"; }
        }
        public string Parameters
        {
            get { return "%user% %price%"; }
        }

        public string Description
        {
            get { return "Siendo guardaespaldas, vende protección a otra persona."; }
        }

        public void Execute(GameClients.GameClient Session, Rooms.Room Room, string[] Params)
        {
            if (Session.GetPlay().TryGetCooldown("cubrir"))
                return;

            int Price = 0;
            GameClient Target = null;
            RoomUser TargetUser = null;          

            #region Group Conditions
            List<Group> Groups = PlusEnvironment.GetGame().GetGroupManager().GetJobsForUser(Session.GetHabbo().Id);

            if (Groups.Count <= 0)
            {
                Session.SendWhisper("No tienes ningún trabajo para hacer eso.", 1);
                return;
            }

            int GroupNumber = -1;

            if (Groups[0].GType != 2)
            {
                if (Groups.Count > 1)
                {
                    if (Groups[1].GType != 2)
                    {
                        Session.SendWhisper("((No perteneces a ningún trabajo usar ese comando))", 1);
                        return;
                    }
                    GroupNumber = 1; // Segundo indicie de variable
                }
                else
                {
                    Session.SendWhisper("((No perteneces a ningún trabajo para usar ese comando))", 1);
                    return;
                }
            }
            else
            {
                GroupNumber = 0; // Primer indice de Variable Group
            }

            Session.GetPlay().JobId = Groups[GroupNumber].Id;
            Session.GetPlay().JobRank = Groups[GroupNumber].Members[Session.GetHabbo().Id].UserRank;
            #endregion

            #region Extra Conditions            
            // Existe el trabajo?
            if (!PlusEnvironment.GetGame().GetGroupManager().JobExists(Session.GetPlay().JobId, Session.GetPlay().JobRank))
            {
                Session.GetPlay().TimeWorked = 0;
                Session.GetPlay().JobId = 0; // Desempleado
                Session.GetPlay().JobRank = 0;

                //Room.Group.DeleteMember(Session.GetHabbo().Id);// OJO ACÁ

                Session.SendWhisper("Lo sentimos, ese trabajo no existe. Te hemos removido ese trabajo.", 1);
                return;
            }

            if (!PlusEnvironment.GetGame().GetGroupManager().HasJobCommand(Session, "cubrir"))
            {
                Session.SendWhisper("Solo guardaespaldas pueden ofrecer protección.", 1);
                return;
            }
            #endregion

            #region Conditions
            if (Params.Length != 3)
            {
                Session.SendWhisper("Comándo inválido, quizás quisiste decir: ':curbir [usuario] [precio]'", 1);
                return;
            }
            else
            {
                Target = PlusEnvironment.GetGame().GetClientManager().GetClientByUsername(Params[1]);
                if (Target == null)
                {
                    Session.SendWhisper("No se ha podido encontrar al usuario.", 1);
                    return;
                }

                TargetUser = Room.GetRoomUserManager().GetRoomUserByHabbo(Target.GetHabbo().Username);
                if (TargetUser == null)
                {
                    Session.SendWhisper("Ha ocurrido un error en encontrar al usuario, probablemente esté desconectado o no está en la Zona.", 1);
                    return;
                }
                if (Target.GetPlay().PassiveMode)
                {
                    Session.SendWhisper("No puedes proteger a una persona que está en modo pasivo.", 1);
                    return;
                }
            }

            #region Basic Conditions
            if (Session.GetPlay().PassiveMode)
            {
                Session.SendWhisper("No puedes hacer eso mientras estás en modo pasivo.", 1);
                return;
            }
            if (Session.GetPlay().Cuffed)
            {
                Session.SendWhisper("No puedes hacer eso mientras estás esposad@", 1);
                return;
            }
            if (!Session.GetRoomUser().CanWalk)
            {
                Session.SendWhisper("Al parecer no puedes ni moverte para hacer eso.", 1);
                return;
            }
            if (Session.GetPlay().Pasajero)
            {
                Session.SendWhisper("¡No puedes hacer eso mientras vas de Pasajer@!", 1);
                return;
            }
            if (Session.GetPlay().IsDead)
            {
                Session.SendWhisper("¡No puedes hacer esto mientras estás muert@!", 1);
                return;
            }
            if (Session.GetPlay().IsJailed)
            {
                Session.SendWhisper("¡No puedes hacer eso mientras estás muert@!", 1);
                return;
            }
            if (Session.GetPlay().IsDying)
            {
                Session.SendWhisper("¡No puedes hacer eso mientras estás muert@!", 1);
                return;
            }
            if (Session.GetPlay().DrivingCar)
            {
                Session.SendWhisper("¡No puedes hacer eso mientras conduces!", 1);
                return;
            }
            if (Target.GetHabbo().Id == Session.GetHabbo().Id)
            {
                Session.SendWhisper("No puedes hacerte eso a ti mism@.", 1);
                return;
            }
            #endregion

            #endregion

            #region Execute
            if (int.TryParse(Params[2], out Price))
            {
                #region Conditions Price
                if (Price < 400 || Price > 1000)
                {
                    Session.SendWhisper("El precio debe ser entre $400 y $1,000.", 1);
                    return;
                }
                #endregion

                #region Offer
                RoleplayManager.Shout(Session, "*Ofrece una protección a " + Target.GetHabbo().Username + " por $" + String.Format("{0:N0}", Price) + "*", 5);
                Target.GetPlay().OfferManager.CreateOffer("proteccion", Session.GetHabbo().Id, Price);
                Target.SendWhisper("Te han ofrecido una protección por $" + String.Format("{0:N0}", Price) + ". Escribe ':aceptar proteccion' para aceptarla ó en su defecto ':rechazar proteccion'.", 1);
                Session.GetPlay().CooldownManager.CreateCooldown("cubrir", 1000, 15);
                return;
                #endregion
            }
            else
            {
                Session.SendWhisper("El precio es inválido", 1);
                return;
            }
            #endregion
        }
    }
}