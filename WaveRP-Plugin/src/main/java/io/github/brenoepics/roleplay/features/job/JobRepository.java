package io.github.brenoepics.roleplay.features.job;


import com.eu.habbo.Emulator;
import java.sql.Connection;
import java.sql.PreparedStatement;
import java.sql.ResultSet;
import java.sql.SQLException;
import java.time.LocalDateTime;
import java.util.ArrayList;
import java.util.List;
import java.util.Optional;
import lombok.extern.slf4j.Slf4j;

@Slf4j
public class JobRepository {

  private static final String SELECT_ALL_JOBS = """
      SELECT j.id, j.name, j.display_name, j.description, j.active, j.created_at, j.updated_at
      FROM jobs j 
      WHERE j.active = TRUE
      ORDER BY j.name
      """;

  private static final String SELECT_JOB_BY_ID = """
      SELECT j.id, j.name, j.display_name, j.description, j.active, j.created_at, j.updated_at
      FROM jobs j 
      WHERE j.id = ? AND j.active = TRUE
      """;

  private static final String SELECT_JOB_BY_NAME = """
      SELECT j.id, j.name, j.display_name, j.description, j.active, j.created_at, j.updated_at
      FROM jobs j 
      WHERE j.name = ? AND j.active = TRUE
      """;

  private static final String SELECT_RANKS_BY_JOB_ID = """
      SELECT jr.id, jr.job_id, jr.name, jr.display_name, jr.level, jr.is_manager, 
             jr.salary, jr.permissions, jr.active, jr.created_at, jr.updated_at
      FROM job_ranks jr 
      WHERE jr.job_id = ? AND jr.active = TRUE
      ORDER BY jr.level, jr.name
      """;

  public List<JobEntity> findAllJobs() {
    List<JobEntity> jobs = new ArrayList<>();

    try (Connection connection = Emulator.getDatabase().getDataSource().getConnection();
        PreparedStatement statement = connection.prepareStatement(SELECT_ALL_JOBS)) {

      try (ResultSet resultSet = statement.executeQuery()) {
        while (resultSet.next()) {
          JobEntity job = mapJobFromResultSet(resultSet);
          jobs.add(job);
        }
      }

      for (JobEntity job : jobs) {
        job.setRanks(findRanksByJobId(job.getId()));
      }

    } catch (SQLException e) {
      log.error("Error loading all jobs", e);
    }

    return jobs;
  }

  public Optional<JobEntity> findJobById(int id) {
    try (Connection connection = Emulator.getDatabase().getDataSource().getConnection();
        PreparedStatement statement = connection.prepareStatement(SELECT_JOB_BY_ID)) {

      statement.setInt(1, id);
      try (ResultSet resultSet = statement.executeQuery()) {
        if (resultSet.next()) {
          JobEntity job = mapJobFromResultSet(resultSet);
          job.setRanks(findRanksByJobId(job.getId()));
          return Optional.of(job);
        }
      }
    } catch (SQLException e) {
      log.error("Error loading job by ID: {}", id, e);
    }
    return Optional.empty();
  }

  public Optional<JobEntity> findJobByName(String name) {
    try (Connection connection = Emulator.getDatabase().getDataSource().getConnection();
        PreparedStatement statement = connection.prepareStatement(SELECT_JOB_BY_NAME)) {

      statement.setString(1, name);
      try (ResultSet resultSet = statement.executeQuery()) {
        if (resultSet.next()) {
          JobEntity job = mapJobFromResultSet(resultSet);
          job.setRanks(findRanksByJobId(job.getId()));
          return Optional.of(job);
        }
      }
    } catch (SQLException e) {
      log.error("Error loading job by name: {}", name, e);
    }
    return Optional.empty();
  }

  public List<JobRankEntity> findRanksByJobId(int jobId) {
    List<JobRankEntity> ranks = new ArrayList<>();

    try (Connection connection = Emulator.getDatabase().getDataSource().getConnection();
        PreparedStatement statement = connection.prepareStatement(SELECT_RANKS_BY_JOB_ID)) {

      statement.setInt(1, jobId);
      try (ResultSet resultSet = statement.executeQuery()) {
        while (resultSet.next()) {
          ranks.add(mapJobRankFromResultSet(resultSet));
        }
      }
    } catch (SQLException e) {
      log.error("Error loading ranks for job ID: {}", jobId, e);
    }

    return ranks;
  }

  private JobEntity mapJobFromResultSet(ResultSet resultSet) throws SQLException {
    return new JobEntity(
        resultSet.getInt("id"),
        resultSet.getString("name"),
        resultSet.getString("display_name"),
        resultSet.getString("description"),
        resultSet.getBoolean("active"),
        resultSet.getObject("created_at", LocalDateTime.class),
        resultSet.getObject("updated_at", LocalDateTime.class),
        new ArrayList<>() // Will be populated separately
    );
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
        null  // parsedPermissions will be lazily loaded when getPermissions() is called
    );
  }
}