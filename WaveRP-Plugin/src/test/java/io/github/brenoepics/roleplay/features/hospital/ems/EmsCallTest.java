package io.github.brenoepics.roleplay.features.hospital.ems;

import static org.junit.jupiter.api.Assertions.assertFalse;
import static org.junit.jupiter.api.Assertions.assertTrue;

import java.sql.Timestamp;
import java.time.Instant;
import org.junit.jupiter.api.Test;

class EmsCallTest {

  @Test
  void openCallCanBeAccepted() {
    EmsCall call = call(EmsCall.Status.OPEN, null);
    assertTrue(call.isActive());
    assertTrue(call.canBeAccepted());
  }

  @Test
  void assignedCallOnlyBelongsToItsMedic() {
    EmsCall call = call(EmsCall.Status.ASSIGNED, 42);
    assertTrue(call.isActive());
    assertTrue(call.isAssignedTo(42));
    assertFalse(call.isAssignedTo(7));
    assertFalse(call.canBeAccepted());
  }

  @Test
  void closedCallIsInactive() {
    assertFalse(call(EmsCall.Status.CLOSED, 42).isActive());
  }

  private EmsCall call(EmsCall.Status status, Integer medicId) {
    return new EmsCall(1L, 10, "Patient", 20, "Hopital", "Malaise", status,
        medicId, medicId == null ? null : "Medic", Timestamp.from(Instant.now()), null, null);
  }
}

