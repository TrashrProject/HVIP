package io.github.brenoepics.roleplay.utilities.types;

import java.nio.charset.StandardCharsets;
import java.util.Base64;

public final class FoodPresentation {

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
      case 18 -> "Pâtes";
      case 19 -> "Homard";
      case 20 -> "Bœuf";
      case 21 -> "Repas complet";
      default -> fallback;
    };
  }

  public static String imageDataUri(RPItem item) {
    if (item == null || !"food".equalsIgnoreCase(item.getInteractionType())) {
      return null;
    }

    String svg = switch (item.getId()) {
      case 9 -> svg("<rect x='13' y='3' width='5' height='5' fill='#6b3d18'/><rect x='18' y='4' width='6' height='3' fill='#3b8d31'/><rect x='8' y='9' width='17' height='17' fill='#d93636'/><rect x='5' y='13' width='22' height='10' fill='#e84a43'/><rect x='9' y='10' width='5' height='4' fill='#ff7a69'/><rect x='11' y='24' width='12' height='3' fill='#a92727'/>");
      case 10 -> svg("<rect x='6' y='8' width='5' height='4' fill='#8d6b16'/><rect x='9' y='10' width='7' height='8' fill='#f0c52d'/><rect x='14' y='15' width='8' height='8' fill='#ffd84c'/><rect x='20' y='20' width='7' height='5' fill='#edbd24'/><rect x='25' y='22' width='3' height='4' fill='#7b5b12'/>");
      case 11 -> svg("<rect x='6' y='7' width='20' height='6' fill='#d69a55'/><rect x='7' y='13' width='18' height='4' fill='#75a83a'/><rect x='7' y='17' width='18' height='4' fill='#d94a3b'/><rect x='6' y='21' width='20' height='6' fill='#c88343'/><rect x='9' y='9' width='14' height='2' fill='#f3c37d'/>");
      case 12 -> svg("<rect x='7' y='6' width='18' height='6' fill='#d88a3d'/><rect x='5' y='11' width='22' height='3' fill='#efad51'/><rect x='6' y='14' width='20' height='4' fill='#57a33e'/><rect x='6' y='18' width='20' height='4' fill='#7b3f24'/><rect x='7' y='22' width='18' height='5' fill='#d88a3d'/><rect x='10' y='8' width='3' height='2' fill='#f5d77f'/><rect x='17' y='8' width='3' height='2' fill='#f5d77f'/>");
      case 13 -> svg("<polygon points='5,25 26,24 10,6' fill='#f0b84b'/><polygon points='10,6 26,24 23,27 7,26' fill='#c97933'/><circle cx='14' cy='15' r='3' fill='#cc3d35'/><circle cx='20' cy='20' r='3' fill='#cc3d35'/><rect x='11' y='20' width='4' height='3' fill='#5aa54b'/>");
      case 14 -> svg("<path d='M6 10h20v5c0 8-5 12-10 12S6 23 6 15z' fill='#e7b04a'/><rect x='9' y='12' width='14' height='4' fill='#65a440'/><rect x='10' y='16' width='5' height='4' fill='#b94035'/><rect x='17' y='16' width='6' height='4' fill='#7d4a2b'/>");
      case 15 -> svg("<rect x='5' y='8' width='8' height='17' rx='2' fill='#263e35'/><rect x='7' y='10' width='4' height='13' fill='#f7f2df'/><rect x='8' y='13' width='2' height='7' fill='#e36c4b'/><rect x='16' y='8' width='11' height='8' rx='2' fill='#263e35'/><rect x='18' y='10' width='7' height='4' fill='#f7f2df'/><rect x='16' y='18' width='11' height='8' rx='2' fill='#263e35'/><rect x='18' y='20' width='7' height='4' fill='#f7f2df'/>");
      case 16 -> svg("<path d='M7 8c5-4 15-2 19 4 3 5-1 12-7 14-7 2-13-1-14-7-1-4 0-8 2-11z' fill='#8e4336'/><path d='M10 10c4-2 10-1 13 3 2 3 0 7-4 9-5 2-9 0-10-4-1-3 0-6 1-8z' fill='#c65d49'/><rect x='14' y='13' width='6' height='2' fill='#f0a07a'/>");
      case 18 -> svg("<rect x='6' y='21' width='20' height='5' rx='2' fill='#d9d9d9'/><path d='M8 19c3-9 13-10 17-3-4 2-5 5-3 7H9z' fill='#e7b83f'/><path d='M10 18c4-4 8-5 12-2M11 21c4-4 8-4 11-1' stroke='#f8df77' stroke-width='2' fill='none'/><circle cx='18' cy='14' r='2' fill='#c84d3e'/>");
      case 19 -> svg("<rect x='14' y='5' width='4' height='20' fill='#b83c35'/><rect x='9' y='8' width='5' height='5' fill='#d84e43'/><rect x='18' y='8' width='5' height='5' fill='#d84e43'/><rect x='7' y='13' width='7' height='5' fill='#df5d4c'/><rect x='18' y='13' width='7' height='5' fill='#df5d4c'/><rect x='10' y='23' width='5' height='4' fill='#7e2a26'/><rect x='17' y='23' width='5' height='4' fill='#7e2a26'/>");
      case 20 -> svg("<path d='M6 10c6-5 15-4 20 1v12c-5 4-14 5-20 0z' fill='#8a4033'/><path d='M9 12c5-3 11-3 14 0v8c-4 3-10 3-14 0z' fill='#bd5a46'/><rect x='13' y='14' width='6' height='4' fill='#e49a77'/>");
      case 21 -> svg("<ellipse cx='16' cy='23' rx='12' ry='5' fill='#d6d9dd'/><ellipse cx='16' cy='21' rx='10' ry='4' fill='#f1f3f4'/><rect x='7' y='12' width='8' height='6' fill='#8f4b36'/><rect x='17' y='11' width='8' height='7' fill='#e5bd48'/><rect x='13' y='8' width='5' height='10' fill='#5da347'/><circle cx='22' cy='9' r='3' fill='#cf4a3f'/>");
      default -> svg("<rect x='7' y='7' width='18' height='18' rx='3' fill='#8aa5b5'/><text x='16' y='21' text-anchor='middle' font-size='12' fill='white'>?</text>");
    };

    return "data:image/svg+xml;base64," + Base64.getEncoder()
        .encodeToString(svg.getBytes(StandardCharsets.UTF_8));
  }

  private static String svg(String body) {
    return "<svg xmlns='http://www.w3.org/2000/svg' width='32' height='32' viewBox='0 0 32 32' shape-rendering='crispEdges'>"
        + body + "</svg>";
  }
}
