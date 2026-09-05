package io.github.brenoepics.roleplay.utilities.types;

import java.util.Locale;

public final class FoodPresentation {

  private static final String FOOD_IMAGE_BASE = "https://paradiserp.fr/Dynamics/img/food/";

  private FoodPresentation() {
  }

  public static String localizedName(int id, String interactionType, String fallback) {
    if (!"food".equalsIgnoreCase(interactionType)) {
      return fallback;
    }

    return switch (id) {
      case 9 -> "Pomme";
      case 10 -> "Banane";
      case 11 -> "Sandwich";
      case 12 -> "Burger";
      case 13 -> "Part de pizza";
      case 14 -> "Taco";
      case 15 -> "Sushi";
      case 16 -> "Steak";
      case 18 -> "P\u00e2tes";
      case 19 -> "Homard";
      case 20 -> "B\u0153uf";
      case 21 -> "Repas complet";
      case 6123 -> "Tasty Crousty";
      default -> fallback;
    };
  }

  /** Returns the CMS image for foods that have a custom PNG, or {@code null}. */
  public static String imageUrl(RPItem item) {
    if (item == null || !"food".equalsIgnoreCase(item.getInteractionType())) {
      return null;
    }

    String filename = switch (item.getId()) {
      case 9 -> "pomme.png";
      case 10 -> "banane.png";
      case 11 -> "sandwich.png";
      case 12 -> "burger.png";
      case 13 -> "pizza.png";
      case 14 -> "tacos.png";
      case 15 -> "sushi.png";
      case 16 -> "steak.png";
      case 20 -> "boeuf.png";
      case 6123 -> "tastycrousty.png";
      default -> null;
    };
    return filename == null ? null : FOOD_IMAGE_BASE + filename;
  }

  public static String indefiniteArticle(RPItem item) {
    if (item == null) {
      return "un";
    }

    return switch (item.getId()) {
      case 9, 10, 13 -> "une";
      case 18 -> "des";
      default -> "un";
    };
  }

  public static String consumptionMessage(RPItem item, int restoredHunger) {
    String name = item == null ? "nourriture" : item.getDisplayName();
    String message = "* Mange " + indefiniteArticle(item) + " " + name + " et r\u00e9cup\u00e8re "
        + restoredHunger + " points de faim *";
    return message.toUpperCase(Locale.FRENCH);
  }
}
