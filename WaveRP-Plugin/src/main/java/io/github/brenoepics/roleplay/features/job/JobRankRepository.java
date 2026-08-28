package io.github.brenoepics.roleplay.features.job;


import com.eu.habbo.Emulator;
import java.sql.Connection;
import java.sql.PreparedStatement;
import java.sql.ResultSet;
import java.sql.SQLException;
import java.time.LocalDateTime;
import java.util.Optional;
import lombok.extern.slf4j.Slf4j;

@Slf4j
public class JobRankRepository {

  private static final String SELECT_RANK_BY_ID = """
      SELECT jr.id, jr.job_id, jr.name, jr.display_name, jr.level, jr.is_manager, 
             jr.salary, jr.permissions, jr.active, jr.created_at, jr.updated_at
      FROM job_ranks jr 
      WHERE jr.id = ? AND jr.active = TRUE
      """;

  private static final String SELECT_RANK_BY_NAME = """
      SELECT jr.id, jr.job_id, jr.name, jr.display_name, jr.level, jr.is_manager, 
             jr.salary, jr.permissions, jr.active, jr.created_at, jr.updated_at
      FROM job_ranks jr 
      WHERE jr.name = ? AND jr.active = TRUE
      """;

  private static final String SELECT_RANK_BY_JOB_AND_LEVEL = """
      SELECT jr.id, jr.job_id, jr.name, jr.display_name, jr.level, jr.is_manager, 
             jr.salary, jr.permissions, jr.active, jr.created_at, jr.updated_at
      FROM job_ranks jr 
      WHERE jr.job_id = ? AND jr.level = ? AND jr.active = TRUE
      ORDER BY jr.name
      LIMIT 1
      """;

  public Optional<JobRankEntity> findRankById(int id) {
    try (Connection connection = Emulator.getDatabase().getDataSource().getConnection();
        PreparedStatement statement = connection.prepareStatement(SELECT_RANK_BY_ID)) {

      statement.setInt(1, id);
      try (ResultSet resultSet = statement.executeQuery()) {
        if (resultSet.next()) {
          return Optional.of(mapJobRankFromResultSet(resultSet));
        }
      }
    } catch (SQLException e) {
      log.error("Error loading job rank by ID: {}", id, e);
    }
    return Optional.empty();
  }

  public Optional<JobRankEntity> findRankByName(String name) {
    try (Connection connection = Emulator.getDatabase().getDataSource().getConnection();
        PreparedStatement statement = connection.prepareStatement(SELECT_RANK_BY_NAME)) {

      statement.setString(1, name);
      try (ResultSet resultSet = statement.executeQuery()) {
        if (resultSet.next()) {
          return Optional.of(mapJobRankFromResultSet(resultSet));
        }
      }
    } catch (SQLException e) {
      log.error("Error loading job rank by name: {}", name, e);
    }
    return Optional.empty();
  }

  public Optional<JobRankEntity> findRankByJobAndLevel(int jobId, int level) {
    try (Connection connection = Emulator.getDatabase().getDataSource().getConnection();
        PreparedStatement statement = connection.prepareStatement(SELECT_RANK_BY_JOB_AND_LEVEL)) {

      statement.setInt(1, jobId);
      statement.setInt(2, level);
      try (ResultSet resultSet = statement.executeQuery()) {
        if (resultSet.next()) {
          return Optional.of(mapJobRankFromResultSet(resultSet));
        }
      }
    } catch (SQLException e) {
      log.error("Error loading job rank by job ID {} and level {}", jobId, level, e);
    }
    return Optional.empty();
  }

  private JobRankEntity mapJobRankFromResultSet(ResultSet resultSet) throws SQLException {
    return new JobRankEntity(
        resultSet.getInt("id"),
        resultSet.getInt("job_id"),
        resultSet.getString("name"),
        resultSet.getString("display_name"),
        resultSet.getInt("level"),
        resultSet.getBoolean("is_manager"),
        resultSet.getBigDecimal("salary"),
        resultSet.getString("permissions"),
        resultSet.getBoolean("active"),
        resultSet.getObject("created_at", LocalDateTime.class),
        resultSet.getObject("updated_at", LocalDateTime.class),
        null, // Job entity not loaded here to avoid circular reference
        null //parsedPermissions will be lazily loaded when getPermissions() is called
    );
  }
}