package io.github.brenoepics.roleplay.features.hospital.ems;

import java.sql.Timestamp;

public record EmsCall(
    long id,
    int callerUserId,
    String callerName,
    int roomId,
    String roomName,
    String reason,
    Status status,
    Integer assignedMedicUserId,
    String assignedMedicName,
    Timestamp createdAt,
    Timestamp assignedAt,
    Timestamp closedAt) {

  public enum Status {
    OPEN,
    ASSIGNED,
    CLOSED,
    CANCELLED;

    public static Status fromDatabase(String value) {
      if (value == null || value.isBlank()) {
        return OPEN;
      }
      try {
        return Status.valueOf(value.toUpperCase());
      } catch (IllegalArgumentException ignored) {
        return OPEN;
      }
    }
  }

  public boolean isActive() {
    return status == Status.OPEN || status == Status.ASSIGNED;
  }

  public boolean canBeAccepted() {
    return status == Status.OPEN && assignedMedicUserId == null;
  }

  public boolean isAssignedTo(int medicUserId) {
    return status == Status.ASSIGNED
        && assignedMedicUserId != null
        && assignedMedicUserId == medicUserId;
  }
}

