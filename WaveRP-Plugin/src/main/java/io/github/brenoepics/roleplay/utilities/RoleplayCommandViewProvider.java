package io.github.brenoepics.roleplay.utilities;

import com.eu.habbo.habbohotel.commands.Command;
import com.eu.habbo.habbohotel.commands.CommandViewProvider;
import com.eu.habbo.habbohotel.gameclients.GameClient;
import io.github.brenoepics.roleplay.RolePlay;
import io.github.brenoepics.roleplay.features.job.JobEntity;
import io.github.brenoepics.roleplay.features.job.JobPermissions;
import io.github.brenoepics.roleplay.features.job.JobRankEntity;
import io.github.brenoepics.roleplay.features.user.RpAvatar;
import java.util.Locale;
import java.util.Map;

/**
 * Adapte uniquement la présentation de :commands aux métiers et permissions RP existants.
 * Ce provider ne donne jamais de permission et ne change jamais le comportement d'une commande.
 */
public final class RoleplayCommandViewProvider implements CommandViewProvider {
  private static final Map<String, String> REQUIRED_PERMISSIONS = Map.ofEntries(
      Map.entry("cmd_ems_heal", JobPermissions.MEDICAL_HEAL),
      Map.entry("cmd_ems_revive", JobPermissions.MEDICAL_REVIVE),
      Map.entry("cmd_bandage", JobPermissions.MEDICAL_BANDAGE),
      Map.entry("cmd_stabilize", JobPermissions.MEDICAL_STABILIZE),
      Map.entry("cmd_ems_calls", JobPermissions.MEDICAL_DISPATCH),
      Map.entry("cmd_accept_ems", JobPermissions.MEDICAL_DISPATCH),
      Map.entry("cmd_close_ems", JobPermissions.MEDICAL_DISPATCH),
      Map.entry("cmd_tazor", JobPermissions.POLICE_TAZE),
      Map.entry("cmd_detaser", JobPermissions.POLICE_TAZE),
      Map.entry("cmd_handcuff", JobPermissions.POLICE_CUFF),
      Map.entry("cmd_unhandcuff", JobPermissions.POLICE_CUFF),
      Map.entry("cmd_escort", JobPermissions.POLICE_CUFF),
      Map.entry("cmd_stopescort", JobPermissions.POLICE_CUFF),
      Map.entry("cmd_prison", JobPermissions.POLICE_ARREST),
      Map.entry("cmd_release", JobPermissions.POLICE_ARREST),
      Map.entry("cmd_wanted_list", JobPermissions.POLICE_WANTED_ACCESS),
      Map.entry("cmd_restaurant_menu", JobPermissions.RESTAURANT_MENU),
      Map.entry("cmd_restaurant_take_order", JobPermissions.RESTAURANT_ORDER),
      Map.entry("cmd_restaurant_prepare", JobPermissions.RESTAURANT_PREPARE),
      Map.entry("cmd_restaurant_serve", JobPermissions.RESTAURANT_SERVE),
      Map.entry("cmd_restaurant_bill", JobPermissions.RESTAURANT_BILL),
      Map.entry("cmd_restaurant_cash", JobPermissions.RESTAURANT_CASH),
      Map.entry("cmd_hire", JobPermissions.JOB_HIRE),
      Map.entry("cmd_fire", JobPermissions.JOB_FIRE),
      Map.entry("cmd_promote", JobPermissions.JOB_PROMOTE),
      Map.entry("cmd_demote", JobPermissions.JOB_DEMOTE)
  );

  @Override
  public boolean isVisible(GameClient client, Command command) {
    String requiredJob = associatedJob(command);
    if (requiredJob == null) return true;

    RpAvatar avatar = avatar(client);
    if (avatar == null || avatar.getJobEntity() == null || avatar.getJobEntity().isUnemployed()) {
      return false;
    }

    if (!"current".equals(requiredJob)
        && !requiredJob.equalsIgnoreCase(avatar.getJobEntity().getName())) {
      return false;
    }

    JobRankEntity rank = avatar.getJobRankEntity();
    if ("cmd_ems_calls".equals(command.permission)) {
      return rank != null && (rank.hasPermission(JobPermissions.MEDICAL_AMBULANCE)
          || rank.hasPermission(JobPermissions.MEDICAL_DISPATCH));
    }

    String requiredPermission = REQUIRED_PERMISSIONS.get(command.permission);
    return requiredPermission == null || (rank != null && rank.hasPermission(requiredPermission));
  }

  @Override
  public String category(GameClient client, Command command) {
    String requiredJob = associatedJob(command);
    if (requiredJob != null) {
      RpAvatar avatar = avatar(client);
      if (avatar == null || avatar.getJobEntity() == null) return null;
      return displayJob(avatar.getJobEntity());
    }
    return generalCategory(command);
  }

  @Override
  public String subcategory(GameClient client, Command command) {
    if (associatedJob(command) != null) return category(client, command);

    String permission = permission(command);
    if (isOneOf(permission, "cmd_start_work", "cmd_stop_work", "cmd_quit_job", "cmd_apply")) {
      return "Gestion du travail";
    }
    if (isOneOf(permission, "cmd_911", "cmd_ems", "cmd_cancel_ems")) {
      return "Services d'urgence";
    }
    if (isOneOf(permission, "cmd_balance", "cmd_give", "cmd_transactions", "cmd_deposit", "cmd_withdraw")) {
      return "Banque personnelle";
    }
    if (permission.startsWith("cmd_org_")) return "Organisations";
    if (permission.contains("offer") || "cmd_sell_rpitem".equals(permission)) return "Échanges RP";

    return generalCategory(command);
  }

  @Override
  public String access(GameClient client, Command command) {
    String requiredJob = associatedJob(command);
    if (requiredJob != null) {
      String permission = REQUIRED_PERMISSIONS.get(command.permission);
      return permission == null ? "Métier requis" : "Métier et grade autorisés";
    }
    return null;
  }

  private static RpAvatar avatar(GameClient client) {
    return client == null || client.getHabbo() == null ? null
        : RolePlay.getAvatarManager().getRpAvatar(client.getHabbo());
  }

  private static String associatedJob(Command command) {
    if (command == null) return null;

    String className = command.getClass().getName();
    String permission = permission(command);

    if (className.contains(".commands.banking.BankEmployeeCommand")) return "bank";
    if (className.contains(".commands.jobs.police.") || className.contains(".commands.escort.")) {
      return "police";
    }
    if (className.contains(".commands.jobs.hospital.")) return "hospital";
    if (className.contains(".commands.jobs.restaurant.")) return "current";

    if (isOneOf(permission, "cmd_hire", "cmd_fire", "cmd_promote", "cmd_demote", "cmd_send_home")) {
      return "current";
    }

    return null;
  }

  private static String displayJob(JobEntity job) {
    if ("hospital".equalsIgnoreCase(job.getName())) return "EMS";
    if (job.getDisplayName() != null && !job.getDisplayName().isBlank()) return job.getDisplayName();
    return job.getName();
  }

  private static String generalCategory(Command command) {
    String permission = permission(command);

    if (isOneOf(permission, "cmd_start_work", "cmd_stop_work", "cmd_quit_job", "cmd_apply")) {
      return "Métiers";
    }

    if (isOneOf(permission, "cmd_911", "cmd_ems", "cmd_cancel_ems")) {
      return "RP";
    }

    if (isOneOf(permission, "cmd_balance", "cmd_give", "cmd_transactions", "cmd_deposit", "cmd_withdraw", "cmd_bucks")) {
      return "Économie";
    }

    if (permission.startsWith("cmd_org_") || permission.contains("offer")
        || "cmd_sell_rpitem".equals(permission)) {
      return "RP";
    }

    StringBuilder valueBuilder = new StringBuilder(permission);
    if (command != null && command.keys != null) {
      for (String key : command.keys) {
        if (key != null) valueBuilder.append(' ').append(key);
      }
    }
    String value = valueBuilder.toString().toLowerCase(Locale.ROOT);

    if (value.matches(".*(balance|solde|deposit|depot|withdraw|retirer|transaction|virement|bucks).*")) {
      return "Économie";
    }
    if (value.matches(".*(taxi|goto|stalk|teleport|hotrooms).*")) return "Déplacements";
    if (value.matches(".*(friend|kiss|hug|whisper|follow).*")) return "Social";
    if (value.matches(".*(inventory|equip|unequip|macro|commands|ping|deleteitem|supprimerobjet).*")) {
      return "Utilitaires";
    }
    if (value.matches(".*(staff|ban|mute|alert|super|shutdown|mass|give_rank|update_|global_heal|room_heal|set_stats).*")) {
      return "Staff";
    }
    if (value.matches(".*(rob|shoot|hit|spit|passive|combat|target_lock|rpitem).*")) return "RP";

    return null;
  }

  private static String permission(Command command) {
    return command == null || command.permission == null
        ? ""
        : command.permission.toLowerCase(Locale.ROOT);
  }

  private static boolean isOneOf(String value, String... expected) {
    for (String item : expected) if (item.equals(value)) return true;
    return false;
  }
}
