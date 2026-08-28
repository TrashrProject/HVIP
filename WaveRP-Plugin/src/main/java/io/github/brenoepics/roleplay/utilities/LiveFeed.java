package io.github.brenoepics.roleplay.utilities;

import club.minnced.discord.webhook.WebhookClient;
import club.minnced.discord.webhook.send.AllowedMentions;
import club.minnced.discord.webhook.send.WebhookEmbed;
import club.minnced.discord.webhook.send.WebhookEmbed.EmbedTitle;
import club.minnced.discord.webhook.send.WebhookEmbedBuilder;
import club.minnced.discord.webhook.send.WebhookMessageBuilder;
import com.eu.habbo.Emulator;
import com.eu.habbo.habbohotel.users.Habbo;
import com.eu.habbo.messages.outgoing.generic.alerts.BubbleAlertComposer;
import gnu.trove.map.hash.THashMap;
import io.github.brenoepics.roleplay.RolePlay;
import io.github.brenoepics.roleplay.features.job.JobEntity;
import java.util.Set;
import java.util.regex.Matcher;
import java.util.regex.Pattern;
import lombok.Getter;
import lombok.extern.slf4j.Slf4j;
import org.owasp.html.HtmlPolicyBuilder;
import org.owasp.html.PolicyFactory;

@Slf4j
public class LiveFeed {

  public static final PolicyFactory policy = new HtmlPolicyBuilder().toFactory();

  private static String format(String text, String color) {
    return "<span style=\"color: %color%;\">%text%</span>".replace("%text%", policy.sanitize(text))
        .replace("%color%", color);
  }

  public static String formatUsername(String username, PlayerState state) {
    return format(username, state.getColor());
  }


  public static BubbleAlertComposer alert(String message) {
    THashMap<String, String> bubbleContent = new THashMap<>();
    bubbleContent.put("display", "BUBBLE");
    bubbleContent.put("message", message);
    bubbleContent.put("image", "livefeed");
    return new BubbleAlertComposer("livefeed", bubbleContent);
  }

  public static void sendGlobalAlert(BubbleAlertComposer message) {
    Emulator.getGameServer().getGameClientManager().sendBroadcastResponse(message);
    discordEmbed(message);
  }

  public static String formatCombat(String template, LivePlayer attacker, LivePlayer victim) {
    return template.replace("%attacker%", attacker.getText()).replace("%victim%", victim.getText());
  }

  public static String formatAction(String template, LivePlayer player, String action) {
    return template.replace("%player%", player.getText()).replace("%action%", action);
  }

  public static String formatPassive(String template, LivePlayer actor, LivePlayer receiver,
      String action) {
    return template.replace("%actor%", actor.getText()).replace("%receiver%", receiver.getText())
        .replace("%action%", action);
  }

  public static void sendJobAlert(String message, JobEntity job) {
    sendJobAlert(message, job, "livefeed");
  }

  public static void sendJobAlert(String message, JobEntity job, String key) {
    THashMap<String, String> alertData = new THashMap<>();
    alertData.put("display", "BUBBLE");
    alertData.put("image", "");
    alertData.put("message", message);

    Set<Habbo> onDutyEmployees = RolePlay.getJobsManager().getOnDutyEmployees().get(job);
    for (Habbo officer : onDutyEmployees) {
      officer.getClient().sendResponse(new BubbleAlertComposer(key, alertData));
    }
  }

  public static void discordEmbed(BubbleAlertComposer bubble) {
    if (!Emulator.getConfig().getBoolean("roleplay.livefeed.enabled", true) || Emulator.getConfig().getValue("roleplay.livefeed.webhook.url").length() <= 5) {
      return;
    }

    try (WebhookClient client = WebhookClient.withUrl(
        Emulator.getConfig().getValue("roleplay.livefeed.webhook.url"))) {
      WebhookMessageBuilder message = new WebhookMessageBuilder().setAllowedMentions(
          AllowedMentions.none());
      EmbedTitle title = new EmbedTitle(
          Emulator.getTexts().getValue("roleplay.livefeed.webhook.title"), null);
      String discordMarkdown = convertHtmlToDiscordMarkdown(bubble.getKeys().get("message"));
      WebhookEmbed embed = new WebhookEmbedBuilder().setColor(0x03A9F4).setTitle(title)
          .setDescription(discordMarkdown).build();
      message.addEmbeds(embed);
      client.send(message.build());
    } catch (Exception e) {
      log.error("Error while sending webhook message", e);
    }
  }

  /**
   * Converts an input HTML string to Discord-compatible Markdown based on specific styling rules.
   * This method processes HTML spans with inline color styles and maps them to equivalent Discord
   * Markdown syntax (e.g., bold, italic, strikethrough).
   *
   * @param html the HTML string to be converted; may contain span tags with color styles. If the
   *             input is null or empty, it will be returned unchanged.
   * @return the converted string in Discord Markdown format. If no applicable styling is found, the
   * original text will be returned without modification.
   */
  public static String convertHtmlToDiscordMarkdown(String html) {
    if (html == null || html.isEmpty()) {
      return html;
    }

    Pattern spanPattern = Pattern.compile("<span style=\"color: (#[0-9a-fA-F]{6});\">(.*?)</span>");
    Matcher matcher = spanPattern.matcher(html);

    StringBuilder result = new StringBuilder();
    while (matcher.find()) {
      String colorHex = matcher.group(1);
      String text = matcher.group(2);

      String replacement = switch (colorHex.toLowerCase()) {
        case "#00ff00" -> // green - attacker
            "**" + text + "**"; // Bold for attacker
        case "#0099ff" -> // blue - passive
            "_" + text + "_"; // Italic for passive
        case "#ff0000" -> // red - victim
            "~~" + text
                + "~~"; // Strikethrough for a victim (or could use "__" + text + "__" for underline)
        default -> text; // Fallback to plain text
      };

      matcher.appendReplacement(result, Matcher.quoteReplacement(replacement));
    }
    matcher.appendTail(result);

    return result.toString();
  }

  public record LivePlayer(String username, PlayerState state) {

    public String getText() {
      return formatUsername(username, state);
    }
  }

  @Getter
  public enum PlayerState {
    ATTACKER("#00ff00"), PASSIVE("#0099ff"), VICTIM("#ff0000");

    private final String color;

    PlayerState(String s) {
      this.color = s;
    }
  }
}
