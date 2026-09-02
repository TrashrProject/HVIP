package io.github.brenoepics.roleplay.communication.outgoing.roleplay;

import com.eu.habbo.habbohotel.users.Habbo;
import io.github.brenoepics.roleplay.communication.outgoing.OutgoingWebMessage;
import io.github.brenoepics.roleplay.features.job.JobEntity;
import io.github.brenoepics.roleplay.features.job.JobRankEntity;
import io.github.brenoepics.roleplay.features.user.RpAvatar;

/**
 * Web payload for the ParadiseRP player HUD.
 *
 * <p>The values come directly from the active RpAvatar instance so the browser never has to infer
 * combat data from DOM elements or query the database.</p>
 */
public final class PlayerHudComposer extends OutgoingWebMessage {

  public static final String HEADER = "paradise_player_hud";

  public PlayerHudComposer(Habbo habbo, RpAvatar avatar) {
    super(HEADER);

    this.data.addProperty("id", habbo.getHabboInfo().getId());
    this.data.addProperty("username", habbo.getHabboInfo().getUsername());
    this.data.addProperty("look", habbo.getHabboInfo().getLook());
    this.data.addProperty("health", avatar.getHealth());
    this.data.addProperty("maxHealth", avatar.getMaxHealth());
    this.data.addProperty("shield", avatar.getShield());
    this.data.addProperty("maxShield", avatar.getMaxShield());

    JobEntity job = avatar.getJobEntity();
    JobRankEntity rank = avatar.getJobRankEntity();
    boolean employed = job != null && !job.isUnemployed();

    this.data.addProperty("role", employed && job.getDisplayName() != null
        ? job.getDisplayName() : "");
    this.data.addProperty("roleRank", employed && rank != null && rank.getDisplayName() != null
        ? rank.getDisplayName() : "");
  }
}
