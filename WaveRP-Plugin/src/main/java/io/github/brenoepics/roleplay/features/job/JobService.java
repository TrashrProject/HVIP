package io.github.brenoepics.roleplay.features.job;

import java.util.List;
import java.util.Map;
import java.util.Optional;
import java.util.concurrent.ConcurrentHashMap;
import lombok.extern.slf4j.Slf4j;

@Slf4j
public class JobService {

  private final JobRepository jobRepository;
  private final JobRankRepository jobRankRepository;

  // Cache to improve performance
  private final Map<Integer, JobEntity> jobCacheById = new ConcurrentHashMap<>();
  private final Map<String, JobEntity> jobCacheByName = new ConcurrentHashMap<>();
  private final Map<Integer, JobRankEntity> rankCacheById = new ConcurrentHashMap<>();
  private final Map<String, JobRankEntity> rankCacheByName = new ConcurrentHashMap<>();

  private List<JobEntity> allJobs;

  public JobService() {
    this.jobRepository = new JobRepository();
    this.jobRankRepository = new JobRankRepository();
    loadAllJobs();
  }

  public void loadAllJobs() {
    log.info("Loading all jobs from database...");
    allJobs = jobRepository.findAllJobs();

    // Populate caches
    jobCacheById.clear();
    jobCacheByName.clear();
    rankCacheById.clear();
    rankCacheByName.clear();

    if (allJobs == null) {
      log.error("Error loading all jobs from database");
      return;
    }

    for (JobEntity job : allJobs) {
      jobCacheById.put(job.getId(), job);
      jobCacheByName.put(job.getName().toLowerCase(), job);

      for (JobRankEntity rank : job.getRanks()) {
        rank.setJob(job);
        rankCacheById.put(rank.getId(), rank);
        rankCacheByName.put(rank.getName().toLowerCase(), rank);
      }
    }

    log.info("Loaded {} jobs with a total of {} ranks", allJobs.size(), rankCacheById.size());
  }

  public List<JobEntity> getAllJobs() {
    return allJobs;
  }

  public Optional<JobEntity> getJobById(int id) {
    return Optional.ofNullable(jobCacheById.get(id));
  }

  public Optional<JobEntity> getJobByName(String name) {
    return Optional.ofNullable(jobCacheByName.get(name.toLowerCase()));
  }

  public Optional<JobRankEntity> getRankById(int id) {
    return Optional.ofNullable(rankCacheById.get(id));
  }

  public Optional<JobRankEntity> getRankByName(String name) {
    return Optional.ofNullable(rankCacheByName.get(name.toLowerCase()));
  }

  public Optional<JobRankEntity> getRankByJobAndLevel(JobEntity job, int level) {
    return job.getRanks().stream().filter(rank -> rank.getLevel() == level).findFirst();
  }

  public Optional<JobRankEntity> getRankByJobAndName(JobEntity job, String name) {
    return job.getRanks().stream().filter(rank -> rank.getName().equalsIgnoreCase(name))
        .findFirst();
  }

  public JobEntity getUnemployedJob() {
    return getJobByName("unemployed").orElseThrow(
        () -> new IllegalStateException("Unemployed job not found in database"));
  }

  public JobRankEntity getUnemployedRank() {
    return getRankByName("unemployed").orElseThrow(
        () -> new IllegalStateException("Unemployed rank not found in database"));
  }
}