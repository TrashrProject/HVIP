package io.github.brenoepics.roleplay.utilities;

import com.eu.habbo.habbohotel.commands.Command;
import com.eu.habbo.habbohotel.commands.CommandViewProvider;
import com.eu.habbo.habbohotel.gameclients.GameClient;
import io.github.brenoepics.roleplay.RolePlay;
import io.github.brenoepics.roleplay.features.job.JobEntity;
import io.github.brenoepics.roleplay.features.job.JobPermissions;
import io.github.brenoepics.roleplay.features.job.JobRankEntity;
import io.github.brenoepics.roleplay.features.user.RpAvatar;
import java.util.Map;

/** Adapte l'affichage des commandes aux métiers et permissions RP existants. */
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
        && !requiredJob.equalsIgnoreCase(avatar.getJobEntity().getName())) return false;
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
    if (associatedJob(command) == null) return generalCategory(command);
    RpAvatar avatar = avatar(client);
    if (avatar == null || avatar.getJobEntity() == null) return null;
    return displayJob(avatar.getJobEntity());
  }

  @Override
  public String subcategory(GameClient client, Command command) {
    return category(client, command);
  }

  @Override
  public String access(GameClient client, Command command) {
    String permission = REQUIRED_PERMISSIONS.get(command.permission);
    if (permission != null) return "Métier et grade autorisés";
    return null;
  }

  private static RpAvatar avatar(GameClient client) {
    return client == null || client.getHabbo() == null ? null
        : RolePlay.getAvatarManager().getRpAvatar(client.getHabbo());
  }

  private static String associatedJob(Command command) {
    if (command == null) return null;
    String className = command.getClass().getName();
    String permission = command.permission == null ? "" : command.permission;
    if (className.contains(".commands.banking.BankEmployeeCommand")) return "bank";
    if (className.contains(".commands.jobs.police.") || className.contains(".commands.escort.")) return "police";
    if (className.contains(".commands.jobs.hospital.")) return "hospital";
    if (REQUIRED_PERMISSIONS.containsKey(permission) && permission.matches("cmd_(hire|fire|promote|demote)")) {
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
    StringBuilder valueBuilder = new StringBuilder(command.permission == null ? "" : command.permission);
    if (command.keys != null) {
      for (String key : command.keys) if (key != null) valueBuilder.append(' ').append(key);
    }
    String value = valueBuilder.toString().toLowerCase();
    if (value.matches(".*(balance|solde|deposit|depot|withdraw|retirer|transaction|virement|bucks).*")) return "Économie";
    if (value.matches(".*(taxi|goto|stalk|teleport|room|sendhome).*")) return "Déplacements";
    if (value.matches(".*(friend|kiss|hug|spit|whisper|follow).*")) return "Social";
    if (value.matches(".*(inventory|equip|unequip|macro|commands|help|ping).*")) return "Utilitaires";
    if (value.matches(".*(staff|ban|mute|alert|super|shutdown|mass|give_rank|update_).*")) return "Staff";
    if (value.matches(".*(rob|shoot|hit|passive|combat|org_|rpitem).*")) return "RP";
    return null;
  }
}
