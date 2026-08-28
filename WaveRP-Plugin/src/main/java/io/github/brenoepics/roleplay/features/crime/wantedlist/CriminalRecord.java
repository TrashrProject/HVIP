package io.github.brenoepics.roleplay.features.crime.wantedlist;

import java.sql.Timestamp;
import lombok.AllArgsConstructor;
import lombok.Data;
import lombok.NoArgsConstructor;

@Data
@NoArgsConstructor
@AllArgsConstructor
public class CriminalRecord {

  private int id;
  private int userId;
  private int crimeId;
  private Integer chargedBy;
  private Timestamp chargedAt;
  private boolean servedTime;
  private boolean paidFine;
  private Timestamp endTime;

  // References to related objects
  private Crime crime;
  private String criminalUsername;
  private String officerUsername;
}