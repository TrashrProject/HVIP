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
    RPItem lobster = item(19, "Homard", "food");
    RPItem weapon = item(6110, "AK47", "weapon");

    assertEquals("https://paradiserp.fr/Dynamics/img/food/pomme.png",
        FoodPresentation.imageUrl(apple));
    assertNull(FoodPresentation.imageUrl(lobster));
    assertNull(FoodPresentation.imageUrl(weapon));
  }

  private static RPItem item(int id, String name, String interactionType) {
    return new RPItem(id, name, interactionType, "10", 10, 0, null, null, -1,
        List.of(), null);
  }
}
