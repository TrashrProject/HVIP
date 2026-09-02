package io.github.brenoepics.roleplay.utilities;

import com.eu.habbo.Emulator;
import com.eu.habbo.habbohotel.commands.Command;
import com.eu.habbo.habbohotel.commands.CommandHandler;
import io.github.brenoepics.roleplay.commands.combat.CombatStatsCommand;
import io.github.brenoepics.roleplay.commands.combat.EquipCommand;
import io.github.brenoepics.roleplay.commands.combat.HitCommand;
import io.github.brenoepics.roleplay.commands.combat.RobCommand;
import io.github.brenoepics.roleplay.commands.combat.ShootCommand;
import io.github.brenoepics.roleplay.commands.combat.UnequipCommand;
import io.github.brenoepics.roleplay.commands.combat.SpitCommand;
import io.github.brenoepics.roleplay.commands.generic.HotRoomsCommand;
import io.github.brenoepics.roleplay.commands.generic.PingCommand;
import io.github.brenoepics.roleplay.commands.escort.EscortCommand;
import io.github.brenoepics.roleplay.commands.escort.StopEscortCommand;
import io.github.brenoepics.roleplay.commands.generic.ApplyCommand;
import io.github.brenoepics.roleplay.commands.generic.BucksCommand;
import io.github.brenoepics.roleplay.commands.generic.HelpCommand;
import io.github.brenoepics.roleplay.commands.generic.EmsCallCommand;
import io.github.brenoepics.roleplay.commands.generic.EmsCancelCommand;
import io.github.brenoepics.roleplay.commands.generic.JobCommand;
import io.github.brenoepics.roleplay.commands.generic.PassiveCommand;
import io.github.brenoepics.roleplay.commands.generic.SellCommand;
import io.github.brenoepics.roleplay.commands.generic.TargetLockCommand;
import io.github.brenoepics.roleplay.commands.generic.TaxiCommand;
import io.github.brenoepics.roleplay.commands.jobs.DemoteCommand;
import io.github.brenoepics.roleplay.commands.jobs.FireCommand;
import io.github.brenoepics.roleplay.commands.jobs.HireCommand;
import io.github.brenoepics.roleplay.commands.jobs.PromoteCommand;
import io.github.brenoepics.roleplay.commands.jobs.QuitJobCommand;
import io.github.brenoepics.roleplay.commands.jobs.SendHomeCommand;
import io.github.brenoepics.roleplay.commands.jobs.StartWorkCommand;
import io.github.brenoepics.roleplay.commands.jobs.StopWorkCommand;
import io.github.brenoepics.roleplay.commands.jobs.hospital.BandageCommand;
import io.github.brenoepics.roleplay.commands.jobs.hospital.CarryPatientCommand;
import io.github.brenoepics.roleplay.commands.jobs.hospital.DiagnosticCommand;
import io.github.brenoepics.roleplay.commands.jobs.hospital.DropPatientCommand;
import io.github.brenoepics.roleplay.commands.jobs.hospital.EmsAcceptCommand;
import io.github.brenoepics.roleplay.commands.jobs.hospital.EmsCallsCommand;
import io.github.brenoepics.roleplay.commands.jobs.hospital.EmsCloseCommand;
import io.github.brenoepics.roleplay.commands.jobs.hospital.HealCommand;
import io.github.brenoepics.roleplay.commands.jobs.hospital.MedicalReviveCommand;
import io.github.brenoepics.roleplay.commands.jobs.hospital.StabilizeCommand;
import io.github.brenoepics.roleplay.commands.jobs.hospital.TransportHospitalCommand;
import io.github.brenoepics.roleplay.commands.jobs.offer.AcceptOfferCommand;
import io.github.brenoepics.roleplay.commands.jobs.offer.ClearOffersCommand;
import io.github.brenoepics.roleplay.commands.jobs.offer.DeclineOfferCommand;
import io.github.brenoepics.roleplay.commands.jobs.offer.OfferCommand;
import io.github.brenoepics.roleplay.commands.jobs.police.ChargeCommand;
import io.github.brenoepics.roleplay.commands.jobs.police.DetaserCommand;
import io.github.brenoepics.roleplay.commands.jobs.police.HandcuffCommand;
import io.github.brenoepics.roleplay.commands.jobs.police.PrisonCommand;
import io.github.brenoepics.roleplay.commands.jobs.police.ReleaseCommand;
import io.github.brenoepics.roleplay.commands.jobs.police.TazorCommand;
import io.github.brenoepics.roleplay.commands.jobs.police.UnhandcuffCommand;
import io.github.brenoepics.roleplay.commands.macro.OpenMacroCommand;
import io.github.brenoepics.roleplay.commands.organizations.CreateOrganizationCommand;
import io.github.brenoepics.roleplay.commands.organizations.DisbandOrganizationCommand;
import io.github.brenoepics.roleplay.commands.organizations.InviteOrganizationCommand;
import io.github.brenoepics.roleplay.commands.organizations.JoinOrganizationCommand;
import io.github.brenoepics.roleplay.commands.organizations.KickOrganizationCommand;
import io.github.brenoepics.roleplay.commands.organizations.LeaveOrganizationCommand;
import io.github.brenoepics.roleplay.commands.organizations.RankDownOrganizationCommand;
import io.github.brenoepics.roleplay.commands.organizations.RankUpOrganizationCommand;
import io.github.brenoepics.roleplay.commands.organizations.RenameOrganizationCommand;
import io.github.brenoepics.roleplay.commands.staff.GlobalHealCommand;
import io.github.brenoepics.roleplay.commands.staff.GotoRoomCommand;
import io.github.brenoepics.roleplay.commands.staff.KillCommand;
import io.github.brenoepics.roleplay.commands.staff.MakeTerritoryCommand;
import io.github.brenoepics.roleplay.commands.staff.ReviveCommand;
import io.github.brenoepics.roleplay.commands.staff.RoomHealCommand;
import io.github.brenoepics.roleplay.commands.staff.RoomReleaseCommand;
import io.github.brenoepics.roleplay.commands.staff.SendRoomCommand;
import io.github.brenoepics.roleplay.commands.staff.SetStatsCommand;
import io.github.brenoepics.roleplay.commands.staff.StaffArrestCommand;
import io.github.brenoepics.roleplay.commands.staff.StaffHitCommand;
import io.github.brenoepics.roleplay.commands.staff.StaffReleaseCommand;
import io.github.brenoepics.roleplay.commands.staff.SuperHealCommand;
import io.github.brenoepics.roleplay.commands.staff.SuperHireCommand;
import io.github.brenoepics.roleplay.commands.wanted.WantedListCommand;
import io.github.brenoepics.roleplay.commands.banking.BalanceCommand;
import io.github.brenoepics.roleplay.commands.banking.BankEmployeeCommand;
import io.github.brenoepics.roleplay.commands.banking.DepositCommand;
import io.github.brenoepics.roleplay.commands.banking.GiveCommand;
import io.github.brenoepics.roleplay.commands.banking.OpenAccountCommand;
import io.github.brenoepics.roleplay.commands.banking.TransactionHistoryCommand;
import io.github.brenoepics.roleplay.commands.banking.WithdrawCommand;
import io.github.brenoepics.roleplay.features.farm.commands.ReloadFarmCommand;
import io.github.brenoepics.roleplay.features.farm.commands.ReloadMarketplaceCommand;
import io.github.brenoepics.roleplay.features.farm.commands.SellItemCommand;
import io.github.brenoepics.roleplay.features.user.CheckDatabase;
import io.github.brenoepics.roleplay.features.user.CheckDatabase.PermissionState;
import org.jetbrains.annotations.NotNull;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;

public class LoadPlayerCommands {

  private static final Logger LOGGER = LoggerFactory.getLogger(LoadPlayerCommands.class);

  public static void loadCommands() {
    try {

      CheckDatabase.registerPermission("acc_change_anywhere", CheckDatabase.PermissionState.DENIED);
      CheckDatabase.registerPermission("acc_infinite_hunger", CheckDatabase.PermissionState.DENIED);

      LoadPlayerCommands.addCommand(
          new BucksCommand("cmd_bucks", getSplit("commands.cmd_bucks.keys")), new String[]{"bucks"},
          CheckDatabase.PermissionState.ALLOWED);
      LoadPlayerCommands.addCommand(new JobCommand("cmd_job", getSplit("commands.cmd_job.keys")),
          new String[]{"job"}, CheckDatabase.PermissionState.DENIED);
      LoadPlayerCommands.addCommand(
          new SendHomeCommand("cmd_send_home", getSplit("commands.cmd_send_home.keys")),
          new String[]{"sendhome"}, CheckDatabase.PermissionState.ALLOWED);
      LoadPlayerCommands.addCommand(new TaxiCommand("cmd_taxi", getSplit("commands.cmd_taxi.keys")),
          new String[]{"taxi", "uber"}, CheckDatabase.PermissionState.ALLOWED);
      LoadPlayerCommands.addCommand(new RobCommand("cmd_rob", getSplit("commands.cmd_rob.keys")),
          new String[]{"rob"}, CheckDatabase.PermissionState.DENIED);
      LoadPlayerCommands.addCommand(new HitCommand("cmd_hit", getSplit("commands.cmd_hit.keys")),
          new String[]{"hit"}, CheckDatabase.PermissionState.ALLOWED);
      LoadPlayerCommands.addCommand(new SpitCommand("cmd_spit", getSplit("commands.cmd_spit.keys")),
          new String[]{"spit"}, CheckDatabase.PermissionState.ALLOWED);
      LoadPlayerCommands.addCommand(
          new ShootCommand("cmd_shoot", getSplit("commands.cmd_shoot.keys")), new String[]{"shoot"},
          CheckDatabase.PermissionState.ALLOWED);
      LoadPlayerCommands.addCommand(
          new EquipCommand("cmd_equip", getSplit("commands.cmd_equip.keys")), new String[]{"equip"},
          CheckDatabase.PermissionState.ALLOWED);
      LoadPlayerCommands.addCommand(
          new UnequipCommand("cmd_unequip", getSplit("commands.cmd_unequip.keys")),
          new String[]{"unequip"}, CheckDatabase.PermissionState.ALLOWED);
      LoadPlayerCommands.addCommand(
          new PassiveCommand("cmd_passive", getSplit("commands.cmd_passive.keys")),
          new String[]{"passive"}, CheckDatabase.PermissionState.ALLOWED);
      setDescription("cmd_ems_heal", ":soigner <pseudo> - Soigner un joueur bless\u00e9.");
      LoadPlayerCommands.addCommand(new HealCommand("cmd_ems_heal", new String[]{"soigner", "heal"}),
          new String[]{"soigner", "heal"}, CheckDatabase.PermissionState.ALLOWED);
      setDescription("cmd_ems_revive",
          ":reanimer <pseudo> - R\u00e9animer un joueur inconscient.");
      LoadPlayerCommands.addCommand(
          new MedicalReviveCommand("cmd_ems_revive", new String[]{"reanimer", "medicalrevive"}),
          new String[]{"reanimer", "medicalrevive"}, CheckDatabase.PermissionState.ALLOWED);
      setDescription("cmd_ems_diagnostic",
          ":diagnostic <pseudo> - Consulter l'\u00e9tat de sant\u00e9 d'un joueur.");
      LoadPlayerCommands.addCommand(
          new DiagnosticCommand("cmd_ems_diagnostic", new String[]{"diagnostic", "diagnose"}),
          new String[]{"diagnostic", "diagnose"}, CheckDatabase.PermissionState.ALLOWED);
      setDescription("cmd_ems_carry", ":porter <pseudo> - Transporter un joueur bless\u00e9.");
      LoadPlayerCommands.addCommand(
          new CarryPatientCommand("cmd_ems_carry", new String[]{"porter", "carry"}),
          new String[]{"porter", "carry"}, CheckDatabase.PermissionState.ALLOWED);
      setDescription("cmd_ems_drop", ":deposerpatient [pseudo] - Arr\u00eater le transport d'un joueur.");
      LoadPlayerCommands.addCommand(
          new DropPatientCommand("cmd_ems_drop", new String[]{"deposerpatient", "poserpatient", "drop"}),
          new String[]{"deposerpatient", "poserpatient", "drop"}, CheckDatabase.PermissionState.ALLOWED);
      LoadPlayerCommands.addCommand(
          new EmsCallCommand("cmd_ems", new String[]{"ems", "medecin", "ambulance"}),
          new String[]{"ems", "medecin", "ambulance"}, CheckDatabase.PermissionState.ALLOWED);
      LoadPlayerCommands.addCommand(
          new EmsCancelCommand("cmd_cancel_ems", new String[]{"annulerems"}),
          new String[]{"annulerems"}, CheckDatabase.PermissionState.ALLOWED);
      LoadPlayerCommands.addCommand(
          new EmsCallsCommand("cmd_ems_calls", new String[]{"appelsems", "emscalls"}),
          new String[]{"appelsems", "emscalls"}, CheckDatabase.PermissionState.ALLOWED);
      LoadPlayerCommands.addCommand(
          new EmsAcceptCommand("cmd_accept_ems", new String[]{"accepterems"}),
          new String[]{"accepterems"}, CheckDatabase.PermissionState.ALLOWED);
      LoadPlayerCommands.addCommand(
          new EmsCloseCommand("cmd_close_ems", new String[]{"fermerems"}),
          new String[]{"fermerems"}, CheckDatabase.PermissionState.ALLOWED);
      LoadPlayerCommands.addCommand(
          new BandageCommand("cmd_bandage", new String[]{"bandage", "panser"}),
          new String[]{"bandage", "panser"}, CheckDatabase.PermissionState.ALLOWED);
      LoadPlayerCommands.addCommand(
          new StabilizeCommand("cmd_stabilize", new String[]{"stabiliser"}),
          new String[]{"stabiliser"}, CheckDatabase.PermissionState.ALLOWED);
      // :reanimer is the EMS action registered above. Keep :revive exclusively for staff;
      // registering the same aliases twice makes the command selected depend on load order.
      LoadPlayerCommands.addCommand(
          new TransportHospitalCommand("cmd_transport_hospital",
              new String[]{"transporthopital", "evacuer"}),
          new String[]{"transporthopital", "evacuer"}, CheckDatabase.PermissionState.ALLOWED);
      LoadPlayerCommands.addCommand(
          new QuitJobCommand("cmd_quit_job", getSplit("commands.cmd_quit_job.keys")),
          new String[]{"quitjob"}, CheckDatabase.PermissionState.ALLOWED);
      setDescription("cmd_tazor", ":taser <pseudo> - Immobilise temporairement un joueur.");
      LoadPlayerCommands.addCommand(
          new TazorCommand("cmd_tazor", new String[]{"taser", "tazor", "taze", "tase"}),
          new String[]{"taser", "tazor", "taze", "tase"}, CheckDatabase.PermissionState.ALLOWED);
      setDescription("cmd_detaser", ":detaser <pseudo> - Retire l'effet du taser.");
      LoadPlayerCommands.addCommand(
          new DetaserCommand("cmd_detaser", new String[]{"detaser"}),
          new String[]{"detaser"}, CheckDatabase.PermissionState.ALLOWED);
      setDescription("cmd_handcuff", ":menotter <pseudo> - Menotte un joueur tase.");
      LoadPlayerCommands.addCommand(
          new HandcuffCommand("cmd_handcuff", new String[]{"menotter", "handcuff"}),
          new String[]{"menotter", "handcuff"}, CheckDatabase.PermissionState.ALLOWED);
      setDescription("cmd_unhandcuff", ":demenotter <pseudo> - Retire les menottes.");
      LoadPlayerCommands.addCommand(
          new UnhandcuffCommand("cmd_unhandcuff", new String[]{"demenotter", "unhandcuff"}),
          new String[]{"demenotter", "unhandcuff"}, CheckDatabase.PermissionState.ALLOWED);
      setDescription("cmd_start_work", ":travailler - Commence votre service.");
      LoadPlayerCommands.addCommand(
          new StartWorkCommand("cmd_start_work", new String[]{"travailler", "startwork"}),
          new String[]{"travailler", "startwork"}, CheckDatabase.PermissionState.ALLOWED);
      setDescription("cmd_stop_work", ":arreter - Termine votre service.");
      LoadPlayerCommands.addCommand(
          new StopWorkCommand("cmd_stop_work", new String[]{"arreter", "stopwork"}),
          new String[]{"arreter", "stopwork"}, CheckDatabase.PermissionState.ALLOWED);
      LoadPlayerCommands.addCommand(
          new OpenMacroCommand("cmd_open_macro", getSplit("commands.cmd_open_macro.keys")),
          new String[]{"macro", "open_macro"}, CheckDatabase.PermissionState.ALLOWED);

      // Generic utility commands
      LoadPlayerCommands.addCommand(
          new HotRoomsCommand("cmd_hotrooms", getSplit("commands.cmd_hotrooms.keys")),
          new String[]{"hotrooms", "hot"}, CheckDatabase.PermissionState.ALLOWED);
      LoadPlayerCommands.addCommand(
          new PingCommand("cmd_ping", getSplit("commands.cmd_ping.keys")),
          new String[]{"ping"}, CheckDatabase.PermissionState.ALLOWED);

      // Banking commands
      setDescription("cmd_openaccount", ":ouvrircompte [pseudo] - Ouvre le compte d'un client au guichet.");
      setDescription("cmd_balance", ":solde - Consulte votre solde bancaire et vos espèces.");
      setDescription("cmd_give", ":virement [pseudo] [montant] - Effectue un virement bancaire.");
      setDescription("cmd_transactions", ":historique [limite] - Affiche vos dernières opérations.");
      setDescription("cmd_deposit", ":deposer [montant] - Dépose des espèces depuis un ATM.");
      setDescription("cmd_withdraw", ":retirer [montant] - Retire des espèces depuis un ATM.");
      setDescription("cmd_bank_account", ":compte [pseudo] - Consulte le compte d'un client.");
      setDescription("cmd_bank_close", ":fermercompte [pseudo] - Ferme le compte d'un client.");
      setDescription("cmd_bank_counter_deposit", ":versement [pseudo] [montant] - Dépôt au guichet.");
      setDescription("cmd_bank_counter_withdraw", ":retraitclient [pseudo] [montant] - Retrait au guichet.");
      LoadPlayerCommands.addCommand(
          new BankEmployeeCommand("cmd_openaccount", new String[]{"ouvrircompte", "openaccount"}, BankEmployeeCommand.Action.OPEN),
          new String[]{"ouvrircompte", "openaccount"}, CheckDatabase.PermissionState.ALLOWED);
      LoadPlayerCommands.addCommand(
          new BalanceCommand("cmd_balance", new String[]{"solde", "balance"}),
          new String[]{"solde", "balance"}, CheckDatabase.PermissionState.ALLOWED);
      LoadPlayerCommands.addCommand(
          new GiveCommand("cmd_give", new String[]{"virement", "give"}),
          new String[]{"virement", "give"}, CheckDatabase.PermissionState.ALLOWED);
      LoadPlayerCommands.addCommand(
          new TransactionHistoryCommand("cmd_transactions", new String[]{"historique", "transactions", "history"}),
          new String[]{"historique", "transactions", "history"}, CheckDatabase.PermissionState.ALLOWED);
      LoadPlayerCommands.addCommand(
          new DepositCommand("cmd_deposit", new String[]{"deposer", "depot", "deposit"}),
          new String[]{"deposer", "depot", "deposit"}, CheckDatabase.PermissionState.ALLOWED);
      LoadPlayerCommands.addCommand(
          new WithdrawCommand("cmd_withdraw", new String[]{"retirer", "withdraw"}),
          new String[]{"retirer", "withdraw"}, CheckDatabase.PermissionState.ALLOWED);
      LoadPlayerCommands.addCommand(new BankEmployeeCommand("cmd_bank_account",new String[]{"compte"},BankEmployeeCommand.Action.ACCOUNT),new String[]{"compte"},CheckDatabase.PermissionState.ALLOWED);
      LoadPlayerCommands.addCommand(new BankEmployeeCommand("cmd_bank_close",new String[]{"fermercompte"},BankEmployeeCommand.Action.CLOSE),new String[]{"fermercompte"},CheckDatabase.PermissionState.ALLOWED);
      LoadPlayerCommands.addCommand(new BankEmployeeCommand("cmd_bank_counter_deposit",new String[]{"versement"},BankEmployeeCommand.Action.DEPOSIT),new String[]{"versement"},CheckDatabase.PermissionState.ALLOWED);
      LoadPlayerCommands.addCommand(new BankEmployeeCommand("cmd_bank_counter_withdraw",new String[]{"retraitclient"},BankEmployeeCommand.Action.WITHDRAW),new String[]{"retraitclient"},CheckDatabase.PermissionState.ALLOWED);

      LoadPlayerCommands.addCommand(
          new SellCommand("cmd_sell_rpitem", getSplit("commands.cmd_sell_rpitem.keys")),
          new String[]{"sell"}, CheckDatabase.PermissionState.ALLOWED);
      LoadPlayerCommands.addCommand(
          new OfferCommand("cmd_offer_rpitem", getSplit("commands.cmd_offer_rpitem.keys")),
          new String[]{"offer"}, CheckDatabase.PermissionState.ALLOWED);
      LoadPlayerCommands.addCommand(
          new AcceptOfferCommand("cmd_accept_offer", getSplit("commands.cmd_accept_offer.keys")),
          new String[]{"acceptoffer"}, CheckDatabase.PermissionState.ALLOWED);
      LoadPlayerCommands.addCommand(
          new ClearOffersCommand("cmd_clear_offer", getSplit("commands.cmd_clear_offer.keys")),
          new String[]{"clearoffers"}, CheckDatabase.PermissionState.ALLOWED);
      LoadPlayerCommands.addCommand(
          new DeclineOfferCommand("cmd_decline_offer", getSplit("commands.cmd_decline_offer.keys")),
          new String[]{"declineoffer"}, CheckDatabase.PermissionState.ALLOWED);
      //LoadPlayerCommands.addCommand(
      //    new StarCommand("cmd_give_stars", getSplit("commands.cmd_give_stars.keys")),
      //    new String[]{"stars"}, CheckDatabase.PermissionState.ALLOWED);
      setDescription("cmd_release", ":liberer <pseudo> - Libère un joueur emprisonné.");
      LoadPlayerCommands.addCommand(
          new ReleaseCommand("cmd_release", new String[]{"liberer", "release"}),
          new String[]{"liberer", "release"}, CheckDatabase.PermissionState.ALLOWED);
      setDescription("cmd_prison", ":prison <pseudo> <minutes> <raison> - Emprisonne un joueur.");
      LoadPlayerCommands.addCommand(
          new PrisonCommand("cmd_prison", new String[]{"prison"}),
          new String[]{"prison"}, CheckDatabase.PermissionState.ALLOWED);
      LoadPlayerCommands.addCommand(
          new ChargeCommand("cmd_charge", getSplit("commands.cmd_charge.keys")),
          new String[]{"charge"}, CheckDatabase.PermissionState.ALLOWED);
      LoadPlayerCommands.addCommand(new HireCommand("cmd_hire", getSplit("commands.cmd_hire.keys")),
          new String[]{"hire"}, CheckDatabase.PermissionState.ALLOWED);
      LoadPlayerCommands.addCommand(
          new ApplyCommand("cmd_apply", getSplit("commands.cmd_apply.keys")), new String[]{"apply"},
          CheckDatabase.PermissionState.ALLOWED);
      LoadPlayerCommands.addCommand(new HelpCommand("cmd_911", getSplit("commands.cmd_911.keys")),
          new String[]{"help", "911", "callpolice"}, CheckDatabase.PermissionState.ALLOWED);
      LoadPlayerCommands.addCommand(new FireCommand("cmd_fire", getSplit("commands.cmd_fire.keys")),
          new String[]{"fire"}, CheckDatabase.PermissionState.ALLOWED);
      LoadPlayerCommands.addCommand(
          new PromoteCommand("cmd_promote", getSplit("commands.cmd_promote.keys")),
          new String[]{"promote"}, CheckDatabase.PermissionState.ALLOWED);
      LoadPlayerCommands.addCommand(
          new DemoteCommand("cmd_demote", getSplit("commands.cmd_demote.keys")),
          new String[]{"demote"}, CheckDatabase.PermissionState.ALLOWED);

      LoadPlayerCommands.addCommand(
          new CreateOrganizationCommand("cmd_org_create", getSplit("commands.cmd_org_create.keys")),
          new String[]{"create", "create_organization"}, CheckDatabase.PermissionState.ALLOWED);
      LoadPlayerCommands.addCommand(
          new JoinOrganizationCommand("cmd_org_join", getSplit("commands.cmd_org_join.keys")),
          new String[]{"join_organization", "join"}, CheckDatabase.PermissionState.ALLOWED);
      LoadPlayerCommands.addCommand(
          new LeaveOrganizationCommand("cmd_org_leave", getSplit("commands.cmd_org_leave.keys")),
          new String[]{"leave", "leave_organization"}, CheckDatabase.PermissionState.ALLOWED);
      LoadPlayerCommands.addCommand(
          new InviteOrganizationCommand("cmd_org_invite", getSplit("commands.cmd_org_invite.keys")),
          new String[]{"recruit", "invite_organization"}, CheckDatabase.PermissionState.ALLOWED);
      LoadPlayerCommands.addCommand(
          new KickOrganizationCommand("cmd_org_kick", getSplit("commands.cmd_org_kick.keys")),
          new String[]{"remove", "kick_organization"}, CheckDatabase.PermissionState.ALLOWED);
      LoadPlayerCommands.addCommand(
          new RenameOrganizationCommand("cmd_org_rename", getSplit("commands.cmd_org_rename.keys")),
          new String[]{"rename", "rename_organization"}, CheckDatabase.PermissionState.ALLOWED);
      LoadPlayerCommands.addCommand(new DisbandOrganizationCommand("cmd_org_delete",
              getSplit("commands.cmd_org_delete.keys")),
          new String[]{"delete", "disband", "disband_organization"},
          CheckDatabase.PermissionState.ALLOWED);
      LoadPlayerCommands.addCommand(
          new RankUpOrganizationCommand("cmd_org_rankup", getSplit("commands.cmd_org_rankup.keys")),
          new String[]{"rankup", "rankup_organization"}, CheckDatabase.PermissionState.ALLOWED);
      LoadPlayerCommands.addCommand(new RankDownOrganizationCommand("cmd_org_rankdown",
              getSplit("commands.cmd_org_rankdown.keys")),
          new String[]{"rankdown", "rankdown_organization"}, CheckDatabase.PermissionState.ALLOWED);
      LoadPlayerCommands.addCommand(new MakeTerritoryCommand("cmd_make_territory",
              getSplit("commands.cmd_make_territory.keys")),
          new String[]{"territory", "create_territory"}, CheckDatabase.PermissionState.DENIED);

      setDescription("cmd_escort", ":escorter <pseudo> - Escorte un joueur menotté.");
      LoadPlayerCommands.addCommand(
          new EscortCommand("cmd_escort", new String[]{"escorter", "escort"}),
          new String[]{"escorter", "escort"}, CheckDatabase.PermissionState.ALLOWED);
      setDescription("cmd_stopescort", ":arreterescorte [pseudo] - Arrête une escorte.");
      LoadPlayerCommands.addCommand(
          new StopEscortCommand("cmd_stopescort", new String[]{"arreterescorte", "stopescort"}),
          new String[]{"arreterescorte", "stopescort"}, CheckDatabase.PermissionState.ALLOWED);

      LoadPlayerCommands.addCommand(
          new WantedListCommand("cmd_wanted_list", getSplit("commands.cmd_wanted_list.keys")),
          new String[]{"wanted_list"}, CheckDatabase.PermissionState.ALLOWED);
      LoadPlayerCommands.addCommand(
          new CombatStatsCommand("cmd_combat_stats", getSplit("commands.cmd_combat_stats.keys")),
          new String[]{"combat_stats"}, CheckDatabase.PermissionState.ALLOWED);
      LoadPlayerCommands.addCommand(
          new TargetLockCommand("cmd_target_lock", getSplit("commands.cmd_target_lock.keys")),
          new String[]{"locktarget", "lt", "lock"}, CheckDatabase.PermissionState.ALLOWED);

      LoadPlayerCommands.addCommand(
          new RoomHealCommand("cmd_room_heal", getSplit("commands.cmd_room_heal.keys")),
          new String[]{"room_heal"}, PermissionState.DENIED);

      LoadPlayerCommands.addCommand(
          new StaffHitCommand("cmd_staff_hit", getSplit("commands.cmd_staff_hit.keys")),
          new String[]{"staff_hit"}, PermissionState.DENIED);

      LoadPlayerCommands.addCommand(
          new SuperHealCommand("cmd_super_heal", getSplit("commands.cmd_super_heal.keys")),
          new String[]{"super_heal"}, PermissionState.DENIED);

      LoadPlayerCommands.addCommand(
          new SendRoomCommand("cmd_send_room", getSplit("commands.cmd_send_room.keys")),
          new String[]{"send_room"}, PermissionState.DENIED);

      LoadPlayerCommands.addCommand(
          new GotoRoomCommand("cmd_goto_room", getSplit("commands.cmd_goto_room.keys")),
          new String[]{"goto_room"}, PermissionState.DENIED);

      LoadPlayerCommands.addCommand(
          new StaffArrestCommand("cmd_staff_arrest", getSplit("commands.cmd_staff_arrest.keys")),
          new String[]{"staff_arrest"}, PermissionState.DENIED);

      LoadPlayerCommands.addCommand(
          new StaffReleaseCommand("cmd_staff_release", getSplit("commands.cmd_release.keys")),
          new String[]{"staff_release"}, PermissionState.DENIED);

      LoadPlayerCommands.addCommand(
          new RoomReleaseCommand("cmd_room_release", getSplit("commands.cmd_room_release.keys")),
          new String[]{"room_release"}, PermissionState.DENIED);

      LoadPlayerCommands.addCommand(
          new SetStatsCommand("cmd_set_stats", getSplit("commands.cmd_set_stats.keys")),
          new String[]{"set_stats"}, PermissionState.DENIED);

      Emulator.getTexts().register("commands.description.cmd_global_heal",
          ":revivertous - Remet la vie de tous les joueurs connectés au maximum.");
      LoadPlayerCommands.addCommand(
          new GlobalHealCommand("cmd_global_heal",
              new String[]{"revivertous", "reviveall", "soignertous", "global_heal"}),
          new String[]{"revivertous", "reviveall", "soignertous", "global_heal"},
          PermissionState.DENIED);
      CheckDatabase.allowPermissionForRankRange("cmd_global_heal", 5, 9);

      Emulator.getTexts().register("commands.description.cmd_superhire",
          ":superhire <pseudo> <metier|id> <rank>");
      LoadPlayerCommands.addCommand(
          new SuperHireCommand("cmd_superhire", new String[]{"superhire", "superrecruter"}),
          new String[]{"superhire", "superrecruter"}, PermissionState.DENIED);
      CheckDatabase.allowPermissionForRankRange("cmd_superhire", 5, 9);

      Emulator.getTexts().register("commands.description.cmd_staff_kill", ":kill <pseudo>");
      LoadPlayerCommands.addCommand(
          new KillCommand("cmd_staff_kill", new String[]{"kill"}),
          new String[]{"kill"}, PermissionState.DENIED);
      CheckDatabase.allowPermissionForRankRange("cmd_staff_kill", 5, 9);

      Emulator.getTexts().register("commands.description.cmd_staff_revive",
          ":revive <pseudo> - Réanime entièrement n'importe quel joueur connecté (staff uniquement).");
      LoadPlayerCommands.addCommand(
          new ReviveCommand("cmd_staff_revive", new String[]{"revive"}),
          new String[]{"revive"}, PermissionState.DENIED);
      CheckDatabase.allowPermissionForRankRange("cmd_staff_revive", 5, 9);

      LoadPlayerCommands.addCommand(
          new SellItemCommand("cmd_sell_item", getSplit("commands.keys.cmd_sell_item")),
          new String[]{"sell_item"}, PermissionState.DENIED);
      LoadPlayerCommands.addCommand(new ReloadMarketplaceCommand("cmd_update_item_marketplace",
              getSplit("commands.keys.cmd_update_item_marketplace")),
          new String[]{"update_item_marketplace"}, PermissionState.DENIED);
      LoadPlayerCommands.addCommand(
          new ReloadFarmCommand("cmd_reload_farm", getSplit("commands.keys.cmd_reload_farm")),
          new String[]{"reload_farm"}, PermissionState.DENIED);

      Emulator.getGameEnvironment().getPermissionsManager().reload();
    } catch (Exception ex) {
      LOGGER.error("[NaHabbo RolePlay] Error while loading player commands: ", ex);
    }
  }

  private static String @NotNull [] getSplit(String key) {
    return Emulator.getConfig().getValue(key).split(";");
  }

  private static void addCommand(Command command, String[] keys,
      CheckDatabase.PermissionState permissionState) {
    Emulator.getConfig()
        .register("commands." + command.permission + ".keys", String.join(";", keys));
    Emulator.getTexts().register("commands.description." + command.permission, ":" + keys[0]);
    CommandHandler.addCommand(command);
    CheckDatabase.registerPermission(command.permission, permissionState);
  }

  private static void setDescription(String permission, String description) {
    String key = "commands.description." + permission;
    Emulator.getTexts().register(key, description);
    Emulator.getTexts().update(key, description);
  }
}
