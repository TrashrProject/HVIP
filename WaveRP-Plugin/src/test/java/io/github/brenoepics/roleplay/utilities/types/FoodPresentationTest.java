package io.github.brenoepics.roleplay.utilities.types;

import static org.junit.jupiter.api.Assertions.assertEquals;
import static org.junit.jupiter.api.Assertions.assertNull;

import java.util.List;
import org.junit.jupiter.api.Test;

class FoodPresentationTest {

  @Test
  void exposesCorrectUtf8FoodNames() {
    assertEquals("P\u00e2tes", FoodPresentation.localizedName(18, "food", "P?tes"));
    assertEquals("B\u0153uf", FoodPresentation.localizedName(20, "food", "B?uf"));
  }

  @Test
  void mapsCustomCmsPngAndKeepsUnknownFoodsOnFallback() {
    RPItem apple = item(9, "Pomme", "food");
    RPItem beef = item(20, "B?uf", "food");
    RPItem tastyCrousty = item(6123, "Tasty Crousty", "food");
    RPItem lobster = item(19, "Homard", "food");
    RPItem weapon = item(6110, "AK47", "weapon");

    assertEquals("https://paradiserp.fr/Dynamics/img/food/pomme.png",
        FoodPresentation.imageUrl(apple));
    assertEquals("https://paradiserp.fr/Dynamics/img/food/boeuf.png",
        FoodPresentation.imageUrl(beef));
    assertEquals("https://paradiserp.fr/Dynamics/img/food/tastycrousty.png",
        FoodPresentation.imageUrl(tastyCrousty));
    assertNull(FoodPresentation.imageUrl(lobster));
    assertNull(FoodPresentation.imageUrl(weapon));
  }

  @Test
  void formatsFoodActionWithCorrectArticleAndUppercaseText() {
    assertEquals("* MANGE UNE POMME ET R\u00c9CUP\u00c8RE 2 POINTS DE FAIM *",
        FoodPresentation.consumptionMessage(item(9, "Pomme", "food"), 2));
    assertEquals("* MANGE UN BURGER ET R\u00c9CUP\u00c8RE 10 POINTS DE FAIM *",
        FoodPresentation.consumptionMessage(item(12, "Burger", "food"), 10));
    assertEquals("* MANGE DES P\u00c2TES ET R\u00c9CUP\u00c8RE 5 POINTS DE FAIM *",
        FoodPresentation.consumptionMessage(item(18, "P\u00e2tes", "food"), 5));
  }

  private static RPItem item(int id, String name, String interactionType) {
    return new RPItem(id, name, interactionType, "10", 10, 0, null, null, -1,
        List.of(), null);
  }
}
