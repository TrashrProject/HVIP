
package io.github.brenoepics.roleplay.utilities.types;

import io.github.brenoepics.roleplay.RolePlay;
import io.github.brenoepics.roleplay.features.job.JobEntity;
import io.github.brenoepics.roleplay.features.job.JobService;
import io.github.brenoepics.roleplay.features.organizations.OrganizationType;
import java.sql.ResultSet;
import java.sql.SQLException;
import java.util.ArrayList;
import java.util.List;
import lombok.Getter;
import org.slf4j.Logger;

@Getter
public class RPItem {

  private final Logger log = org.slf4j.LoggerFactory.getLogger(RPItem.class);

  private final int id;
  private final String displayName;
  private final String interactionType;
  private final String permission;
  private final int enableId;
  private final String extraData;
  private final int max;
  private final int price;
  private final JobEntity requirementJob;
  private List<OrganizationType> crafterOrganizations = new ArrayList<>();
  private final JobEntity offerJob;
  // Hand item that must be held by the offering user to offer this RPItem (0 = no requirement)
  private final int requiredHanditem;

  public RPItem(int id, String displayName, String interactionType, String extraData, int max,
      int price,
      JobEntity requirementJob, String permission, int enableId,
      List<OrganizationType> crafterOrganizations,
      JobEntity offerJob) {
    this.id = id;
    this.displayName = displayName;
    this.interactionType = interactionType;
    this.extraData = extraData;
    this.max = max;
    this.price = price;
    this.requirementJob = requirementJob;
    this.permission = permission;
    this.enableId = enableId;
    this.crafterOrganizations = crafterOrganizations;
    this.offerJob = offerJob;
    this.requiredHanditem = 0; // default when not specified
  }

  public RPItem(ResultSet set) throws SQLException {
    this.id = set.getInt("id");
    this.displayName = set.getString("name");
    this.interactionType = set.getString("interaction_type");
    this.permission = set.getString("permission");
    this.enableId = set.getInt("enable_id");
    this.extraData = set.getString("extra_data");
    this.max = set.getInt("max");
    this.price = set.getInt("price");

    JobService jobService = RolePlay.getJobService();

    Integer requiredJobId = set.getObject("required_job_id", Integer.class);
    if (requiredJobId != null) {
      this.requirementJob = jobService.getJobById(requiredJobId).orElse(null);
    } else {
        this.requirementJob = null;
    }

    Integer offerJobId = set.getObject("offer_job_id", Integer.class);
    if (offerJobId != null) {
      this.offerJob = jobService.getJobById(offerJobId).orElse(null);
    } else {
      this.offerJob = null;
    }

    Integer requiredHanditem = null;
    try {
      requiredHanditem = set.getObject("required_handitem", Integer.class);
    } catch (SQLException e) {
      // Column might not exist in older schemas; default to no requirement
      requiredHanditem = null;
    }
    this.requiredHanditem = requiredHanditem != null ? requiredHanditem : 0;

      if (set.getString("crafter_organizations") == null) {
          return;
      }

    String[] organizations = set.getString("crafter_organizations").split(",");
    for (String organization : organizations) {
        if (organization.isEmpty()) {
            continue;
        }
      try {
        crafterOrganizations.add(OrganizationType.valueOf(organization.toUpperCase()));
      } catch (IllegalArgumentException e) {
        log.error("Organization {} not found for item with id {} check the crafter_organizations",
            organization, this.id);
      }
    }
  }

  public String getDisplayName() {
    return FoodPresentation.localizedName(this.id, this.interactionType, this.displayName);
  }

  public String getRawDisplayName() {
    return this.displayName;
  }

  public boolean requiresJob() {
    return requirementJob != null;
  }

  public boolean canUse(JobEntity userJob) {
      if (!requiresJob()) {
          return true;
      }
    return userJob != null && userJob.getId() == requirementJob.getId();
  }

  public boolean isOfferedByJob() {
    return offerJob != null;
  }

  public boolean isOfferedBy(JobEntity job) {
      if (!isOfferedByJob()) {
          return false;
      }
    return job != null && job.getId() == offerJob.getId();
  }
}