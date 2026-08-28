package io.github.brenoepics.roleplay.features.hospital.ems;

import com.eu.habbo.Emulator;
import java.sql.Connection;
import java.sql.PreparedStatement;
import java.sql.ResultSet;
import java.sql.SQLException;
import java.sql.Statement;
import java.util.ArrayList;
import java.util.List;
import java.util.Optional;
import lombok.extern.slf4j.Slf4j;

@Slf4j
public class EmsRepository {

  public Optional<EmsCall> createCall(int callerUserId, String callerName, int roomId,
      String roomName, String reason) {
    String sql = "INSERT INTO rp_ems_calls "
        + "(caller_user_id, caller_name, room_id, room_name, reason, status) "
        + "VALUES (?, ?, ?, ?, ?, 'OPEN')";
    try (Connection connection = Emulator.getDatabase().getDataSource().getConnection();
        PreparedStatement statement = connection.prepareStatement(sql,
            Statement.RETURN_GENERATED_KEYS)) {
      statement.setInt(1, callerUserId);
      statement.setString(2, callerName);
      statement.setInt(3, roomId);
      statement.setString(4, roomName);
      statement.setString(5, reason);
      statement.executeUpdate();
      try (ResultSet keys = statement.getGeneratedKeys()) {
        if (keys.next()) {
          return findById(connection, keys.getLong(1));
        }
      }
    } catch (SQLException exception) {
      log.error("[EMS] Impossible de creer l'appel de {}", callerUserId, exception);
    }
    return Optional.empty();
  }

  public Optional<EmsCall> findById(long callId) {
    try (Connection connection = Emulator.getDatabase().getDataSource().getConnection()) {
      return findById(connection, callId);
    } catch (SQLException exception) {
      log.error("[EMS] Impossible de charger l'appel {}", callId, exception);
      return Optional.empty();
    }
  }

  private Optional<EmsCall> findById(Connection connection, long callId) throws SQLException {
    try (PreparedStatement statement = connection.prepareStatement(
        "SELECT * FROM rp_ems_calls WHERE id = ? LIMIT 1")) {
      statement.setLong(1, callId);
      try (ResultSet result = statement.executeQuery()) {
        return result.next() ? Optional.of(map(result)) : Optional.empty();
      }
    }
  }

  public List<EmsCall> findActiveCalls(int limit) {
    List<EmsCall> calls = new ArrayList<>();
    String sql = "SELECT * FROM rp_ems_calls WHERE status IN ('OPEN','ASSIGNED') "
        + "ORDER BY created_at ASC LIMIT ?";
    try (Connection connection = Emulator.getDatabase().getDataSource().getConnection();
        PreparedStatement statement = connection.prepareStatement(sql)) {
      statement.setInt(1, Math.max(1, Math.min(limit, 50)));
      try (ResultSet result = statement.executeQuery()) {
        while (result.next()) {
          calls.add(map(result));
        }
      }
    } catch (SQLException exception) {
      log.error("[EMS] Impossible de charger les appels actifs", exception);
    }
    return calls;
  }

  public boolean hasActiveCall(int callerUserId) {
    String sql = "SELECT 1 FROM rp_ems_calls WHERE caller_user_id = ? "
        + "AND status IN ('OPEN','ASSIGNED') LIMIT 1";
    try (Connection connection = Emulator.getDatabase().getDataSource().getConnection();
        PreparedStatement statement = connection.prepareStatement(sql)) {
      statement.setInt(1, callerUserId);
      try (ResultSet result = statement.executeQuery()) {
        return result.next();
      }
    } catch (SQLException exception) {
      log.error("[EMS] Impossible de verifier les appels de {}", callerUserId, exception);
      return true;
    }
  }

  public boolean assign(long callId, int medicUserId, String medicName) {
    String sql = "UPDATE rp_ems_calls SET status = 'ASSIGNED', assigned_medic_user_id = ?, "
        + "assigned_medic_name = ?, assigned_at = CURRENT_TIMESTAMP "
        + "WHERE id = ? AND status = 'OPEN' AND assigned_medic_user_id IS NULL";
    return executeUpdate(sql, statement -> {
      statement.setInt(1, medicUserId);
      statement.setString(2, medicName);
      statement.setLong(3, callId);
    }) == 1;
  }

  public boolean close(long callId) {
    String sql = "UPDATE rp_ems_calls SET status = 'CLOSED', closed_at = CURRENT_TIMESTAMP "
        + "WHERE id = ? AND status IN ('OPEN','ASSIGNED')";
    return executeUpdate(sql, statement -> statement.setLong(1, callId)) == 1;
  }

  public boolean cancel(long callId, int callerUserId) {
    String sql = "UPDATE rp_ems_calls SET status = 'CANCELLED', closed_at = CURRENT_TIMESTAMP "
        + "WHERE id = ? AND caller_user_id = ? AND status IN ('OPEN','ASSIGNED')";
    return executeUpdate(sql, statement -> {
      statement.setLong(1, callId);
      statement.setInt(2, callerUserId);
    }) == 1;
  }

  public void releaseMedicAssignments(int medicUserId) {
    String sql = "UPDATE rp_ems_calls SET status = 'OPEN', assigned_medic_user_id = NULL, "
        + "assigned_medic_name = NULL, assigned_at = NULL "
        + "WHERE assigned_medic_user_id = ? AND status = 'ASSIGNED'";
    executeUpdate(sql, statement -> statement.setInt(1, medicUserId));
  }

  public void recordTreatment(int patientUserId, int medicUserId, int roomId,
      String treatmentType, int healthBefore, int healthAfter, String notes) {
    String sql = "INSERT INTO rp_ems_treatments "
        + "(patient_user_id, medic_user_id, room_id, treatment_type, "
        + "health_before, health_after, notes) VALUES (?, ?, ?, ?, ?, ?, ?)";
    executeUpdate(sql, statement -> {
      statement.setInt(1, patientUserId);
      statement.setInt(2, medicUserId);
      statement.setInt(3, roomId);
      statement.setString(4, treatmentType);
      statement.setInt(5, healthBefore);
      statement.setInt(6, healthAfter);
      statement.setString(7, notes);
    });
  }

  private int executeUpdate(String sql, StatementBinder binder) {
    try (Connection connection = Emulator.getDatabase().getDataSource().getConnection();
        PreparedStatement statement = connection.prepareStatement(sql)) {
      binder.bind(statement);
      return statement.executeUpdate();
    } catch (SQLException exception) {
      log.error("[EMS] Erreur SQL", exception);
      return 0;
    }
  }

  private static EmsCall map(ResultSet result) throws SQLException {
    int assignedId = result.getInt("assigned_medic_user_id");
    Integer assignedMedicUserId = result.wasNull() ? null : assignedId;
    return new EmsCall(
        result.getLong("id"),
        result.getInt("caller_user_id"),
        result.getString("caller_name"),
        result.getInt("room_id"),
        result.getString("room_name"),
        result.getString("reason"),
        EmsCall.Status.fromDatabase(result.getString("status")),
        assignedMedicUserId,
        result.getString("assigned_medic_name"),
        result.getTimestamp("created_at"),
        result.getTimestamp("assigned_at"),
        result.getTimestamp("closed_at"));
  }

  @FunctionalInterface
  private interface StatementBinder {
    void bind(PreparedStatement statement) throws SQLException;
  }
}

