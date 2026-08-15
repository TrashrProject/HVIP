using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

using Plus.HabboHotel.Groups;
using Plus.HabboHotel.GameClients;
using Plus.HabboRoleplay.Misc;
using Plus.HabboRoleplay.Weapons;
using Plus.HabboRoleplay.Vehicles;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Users.Jobs.Types.Bank
{
    class FichaCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_jobs_taxi_ficha"; }
        }

        public string Parameters
        {
            get { return "%price%"; }
        }

        public string Description
        {
            get { return "Siendo taxista, enciende el Taxímetro bajo el costo que establezcas. [min. $2 - max. $50 / 15 segs.]"; }
        }

        public void Execute(GameClients.GameClient Session, Rooms.Room Room, string[] Params)
        {
            bool endficha = false;
            #region Conditions
            if (RoleplayManager.PurgeEvent)
            {
                Session.SendWhisper("¡No puedes trabajar durante la purga!", 1);
                return;
            }

            if (Params.Length == 1)
                endficha = true;

            else if (Params.Length > 2)
            {
                Session.SendWhisper("Comando Inválido. Usa ':ficha [cantidad]' ó unicamente :ficha para apagar el Taxímetro.", 1);
                return;
            }
            if(!PlusEnvironment.GetGame().GetGroupManager().HasJobCommand(Session, "taxi"))
            {
                Session.SendWhisper("Debes tener el trabajo de Taxista para usar ese comando.", 1);
                return;
            }
            if (!Session.GetPlay().DrivingCar)
            {
                Session.SendWhisper("Debes conducir un Taxi para encender tu Taxímetro.", 1);
                return;
            }
            #region Get Information form VehiclesManager
            Vehicle vehicle = null;
            int corp = 0;
            foreach (Vehicle Vehicle in VehicleManager.Vehicles.Values)
            {
                if (Session.GetPlay().CarEffectId == Vehicle.EffectID)
                {
                    vehicle = Vehicle;
                    corp = Convert.ToInt32(Vehicle.CarCorp);
                }
            }
            if (vehicle == null)
            {
                Session.SendWhisper("¡Ha ocurrido un error al buscar los datos del vehículo que conduces!", 1);
                return;
            }
            #endregion

            if (!PlusEnvironment.GetGame().GetGroupManager().GetJob(corp).Name.Contains("Taxistas"))
            {
                Session.SendWhisper("Debes conducir un Taxi para encender tu Taxímetro.", 1);
                return;
            }
            if (Session.GetPlay().TryGetCooldown("ficha"))
                return;
            #endregion

            #region Execute
            if (!endficha)
            {
                int Amount;
                if (int.TryParse(Params[1], out Amount))
                {
                    if(Amount < 2 || Amount > 50)
                    {
                        Session.SendWhisper("Solo tienes permitido establecer una Tarifa de Min. $2 y Máx. $50", 1);
                        return;
                    }
                    Session.GetPlay().Ficha = Amount;
                    Session.GetPlay().FichaTimer = 0;
                    RoleplayManager.Shout(Session, "Establece su Taxímetro en $" + Amount + " y comienza a trabajar*", 5);
                    Session.GetPlay().CooldownManager.CreateCooldown("ficha", 1000, 5);
                }
                else
                {
                    Session.SendWhisper("Ingresa una cantidad válida.", 1);
                    return;
                }
            }
            else
            {
                if (Session.GetPlay().Ficha != 0)
                {
                    Session.GetPlay().Ficha = 0;
                    Session.GetPlay().FichaTimer = 0;
                    RoleplayManager.Shout(Session, "*Apaga el Taxímetro y deja de trabajar*", 5);
                }
                else
                {
                    Session.SendWhisper("¡Tu Taxímetro ya está apagado!", 1);
                }
                Session.GetPlay().CooldownManager.CreateCooldown("ficha", 1000, 5);
                return;
            }
            #endregion
        }
    }
}
