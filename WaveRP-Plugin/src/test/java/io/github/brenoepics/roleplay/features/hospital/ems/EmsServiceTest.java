package io.github.brenoepics.roleplay.features.hospital.ems;

import static org.junit.jupiter.api.Assertions.assertEquals;

import org.junit.jupiter.api.Test;

class EmsServiceTest {

  @Test
  void sanitizesEmptyAndLongReasons() {
    assertEquals("Urgence medicale", EmsService.sanitizeReason("   "));
    assertEquals(160, EmsService.sanitizeReason("x".repeat(200)).length());
  }

  @Test
  void normalizesWhitespace() {
    assertEquals("douleur thoracique severe",
        EmsService.sanitizeReason(" douleur\n\tthoracique    severe "));
  }
}

