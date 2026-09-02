package io.github.brenoepics.roleplay.features.job;

public final class JobPermissions {

  // Police permissions
  public static final String POLICE_ARREST = "police.arrest";
  public static final String POLICE_TAZE = "police.taze";
  public static final String POLICE_ALERT = "police.alert";
  public static final String POLICE_SEARCH = "police.search";
  public static final String POLICE_CUFF = "police.cuff";
  public static final String POLICE_WANTED_ACCESS = "police.wanted.access";
  public static final String POLICE_INVESTIGATE = "police.investigate";

  // Medical permissions
  public static final String MEDICAL_HEAL = "medical.heal";
  public static final String MEDICAL_BANDAGE = "medical.bandage";
  public static final String MEDICAL_STABILIZE = "medical.stabilize";
  public static final String MEDICAL_REVIVE = "medical.revive";
  public static final String MEDICAL_AMBULANCE = "medical.ambulance";
  public static final String MEDICAL_DISPATCH = "medical.dispatch";
  public static final String MEDICAL_SURGERY = "medical.surgery";

  // Job management permissions
  public static final String JOB_HIRE = "job.hire";
  public static final String JOB_FIRE = "job.fire";
  public static final String JOB_PROMOTE = "job.promote";
  public static final String JOB_DEMOTE = "job.demote";
  public static final String JOB_MANAGE_SCHEDULE = "job.schedule";

  // Business permissions
  public static final String BUSINESS_SELL = "business.sell";
  public static final String BUSINESS_MANAGE_INVENTORY = "business.inventory";
  public static final String BUSINESS_CASH_REGISTER = "business.cashregister";
  public static final String BUSINESS_OPEN_CLOSE = "business.openclose";

  // Security permissions
  public static final String SECURITY_ESCORT = "security.escort";
  public static final String SECURITY_REMOVE = "security.remove";
  public static final String SECURITY_BAN = "security.ban";

  // Banking permissions
  public static final String BANK_TRANSFER = "bank.transfer";
  public static final String BANK_LOAN = "bank.loan";
  public static final String BANK_ACCOUNT_ACCESS = "bank.account.access";
  public static final String BANK_ACCOUNT_VIEW = "bank.account.view";
  public static final String BANK_ACCOUNT_MANAGE = "bank.account.manage";
  public static final String BANK_COUNTER_DEPOSIT = "bank.counter.deposit";
  public static final String BANK_COUNTER_WITHDRAW = "bank.counter.withdraw";

  private JobPermissions() {
    // Utility class
  }
}
