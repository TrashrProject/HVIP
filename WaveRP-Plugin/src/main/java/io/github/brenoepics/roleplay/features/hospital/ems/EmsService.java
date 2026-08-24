package io.github.brenoepics.roleplay.features.hospital.ems;

import com.eu.habbo.Emulator;
import com.eu.habbo.habbohotel.rooms.Room;
import com.eu.habbo.habbohotel.rooms.RoomChatMessageBubbles;
import com.eu.habbo.habbohotel.rooms.RoomUnit;
import com.eu.habbo.habbohotel.users.Habbo;
import com.eu.habbo.messages.outgoing.generic.alerts.BubbleAlertComposer;
import gnu.trove.map.hash.THashMap;
import io.github.brenoepics.roleplay.RolePlay;
import io.github.brenoepics.roleplay.features.job.JobPermissions;
import io.github.brenoepics.roleplay.features.user.RpAvatar;
import java.util.List;
import java.util.Optional;
import java.util.concurrent.ConcurrentHashMap;
import lombok.extern.slf4j.Slf4j;

@Slf4j
public class EmsService {

  private final EmsRepository repository;
  private final ConcurrentHashMap<String, Long> treatmentCooldowns = new ConcurrentHashMap<>();
  private final ConcurrentHashMap<Integer, Long> callCooldowns = new ConcurrentHashMap<>();

  public EmsService() {
    this(new EmsRepository());
  }

  EmsService(EmsRepository repository) {
    this.repository = repository;
  }

  public CallResult createCall(Habbo caller, String reason) {
    if (caller == null || caller.getRoomUnit() == null
        || caller.getHabboInfo().getCurrentRoom() == null) {
      return CallResult.failure("Vous devez etre dans un appartement pour appeler les EMS.");
    }

    int userId = caller.getHabboInfo().getId();
    long now = System.currentTimeMillis();
    long cooldownMs = Emulator.getConfig().getInt("features.ems.call.cooldown.seconds", 30)
        * 1000L;
    Long lastCall = callCooldowns.get(userId);
    if (lastCall != null && now - lastCall < cooldownMs) {
      long remaining = Math.max(1, (cooldownMs - (now - lastCall) + 999) / 1000);
      return CallResult.failure("Veuillez attendre " + remaining + " seconde(s) avant un nouvel appel.");
    }
    if (repository.hasActiveCall(userId)) {
      return CallResult.failure("Vous avez deja un appel EMS en cours.");
    }

    Room room = caller.getHabboInfo().getCurrentRoom();
    String safeReason = sanitizeReason(reason);
    Optional<EmsCall> created = repository.createCall(userId,
        caller.getHabboInfo().getUsername(), room.getId(), room.getName(), safeReason);
    if (created.isEmpty()) {
      return CallResult.failure("Le central EMS est indisponible. Reessayez dans un instant.");
    }

    callCooldowns.put(userId, now);
    notifyMedicalStaff(created.get());
    return CallResult.success("Votre appel EMS #" + created.get().id() + " a ete transmis.",
        created.get());
  }

  public List<EmsCall> getActiveCalls() {
    return repository.findActiveCalls(
        Emulator.getConfig().getInt("features.ems.calls.list.limit", 15));
  }

  public CallResult acceptCall(Habbo medic, long callId) {
    if (!isMedicOnDuty(medic, JobPermissions.MEDICAL_AMBULANCE)) {
      return CallResult.failure("Vous devez etre ambulancier en service.");
    }
    Optional<EmsCall> existing = repository.findById(callId);
    if (existing.isEmpty() || !existing.get().canBeAccepted()) {
      return CallResult.failure("Cet appel n'est plus disponible.");
    }
    if (!repository.assign(callId, medic.getHabboInfo().getId(),
        medic.getHabboInfo().getUsername())) {
      return CallResult.failure("Un autre medecin a deja accepte cet appel.");
    }
    EmsCall assigned = repository.findById(callId).orElse(existing.get());
    Habbo caller = Emulator.getGameEnvironment().getHabboManager()
        .getHabbo(assigned.callerUserId());
    if (caller != null) {
      caller.whisper("L'EMS " + medic.getHabboInfo().getUsername()
          + " a pris votre appel en charge.", RoomChatMessageBubbles.RADIO);
    }
    return CallResult.success("Appel #" + callId + " accepte : " + assigned.roomName()
        + " (appart " + assigned.roomId() + ").", assigned);
  }

  public CallResult closeCall(Habbo medic, long callId) {
    if (!isMedicOnDuty(medic, JobPermissions.MEDICAL_AMBULANCE)) {
      return CallResult.failure("Vous devez etre ambulancier en service.");
    }
    Optional<EmsCall> call = repository.findById(callId);
    if (call.isEmpty() || !call.get().isActive()) {
      return CallResult.failure("Cet appel est deja termine ou introuvable.");
    }
    RpAvatar medicData = RolePlay.getAvatarManager().getRpAvatar(medic);
    boolean dispatcher = medicData.getJobRankEntity()
        .hasPermission(JobPermissions.MEDICAL_DISPATCH);
    if (!dispatcher && !call.get().isAssignedTo(medic.getHabboInfo().getId())) {
      return CallResult.failure("Cet appel n'est pas assigne a votre equipe.");
    }
    if (!repository.close(callId)) {
      return CallResult.failure("Impossible de cloturer cet appel.");
    }
    Habbo caller = Emulator.getGameEnvironment().getHabboManager()
        .getHabbo(call.get().callerUserId());
    if (caller != null) {
      caller.whisper("Votre appel EMS #" + callId + " est termine.",
          RoomChatMessageBubbles.RADIO);
    }
    return CallResult.success("Appel EMS #" + callId + " cloture.", call.get());
  }

  public CallResult cancelCall(Habbo caller, long callId) {
    if (caller == null || !repository.cancel(callId, caller.getHabboInfo().getId())) {
      return CallResult.failure("Cet appel ne vous appartient pas ou est deja termine.");
    }
    return CallResult.success("Votre appel EMS #" + callId + " a ete annule.", null);
  }

  public TreatmentResult bandage(Habbo medic, Habbo patient) {
    TreatmentResult validation = validateTreatment(medic, patient,
        JobPermissions.MEDICAL_BANDAGE, "bandage");
    if (!validation.success()) {
      return validation;
    }
    RpAvatar patientData = RolePlay.getAvatarManager().getRpAvatar(patient);
    if (patientData.isDead()) {
      return TreatmentResult.failure("Le patient est inconscient : stabilisez-le puis reanimez-le.");
    }
    if (patientData.getHealth() >= patientData.getMaxHealth()) {
      return TreatmentResult.failure("Le patient n'a pas besoin de bandage.");
    }
    int before = patientData.getHealth();
    int amount = Math.max(1, Emulator.getConfig().getInt("features.ems.bandage.health", 20));
    patientData.setHealth(Math.min(patientData.getMaxHealth(), before + amount));
    persistTreatment(medic, patient, "BANDAGE", before, patientData.getHealth(), "Bandage EMS");
    markTreatmentAction(medic, "bandage");
    patientData.updateLife();
    patientData.updateDatabase();
    consumeMedicAction(medic);
    patient.whisper("Un EMS vous a pose un bandage. Sante : " + patientData.getHealth()
        + "/" + patientData.getMaxHealth() + ".", RoomChatMessageBubbles.RADIO);
    return TreatmentResult.success("Bandage pose sur " + patient.getHabboInfo().getUsername()
        + " (" + before + " -> " + patientData.getHealth() + ").");
  }

  public TreatmentResult stabilize(Habbo medic, Habbo patient) {
    TreatmentResult validation = validateTreatment(medic, patient,
        JobPermissions.MEDICAL_STABILIZE, "stabilize");
    if (!validation.success()) {
      return validation;
    }
    RpAvatar patientData = RolePlay.getAvatarManager().getRpAvatar(patient);
    if (!patientData.isDead()) {
      return TreatmentResult.failure("Le patient est conscient et n'a pas besoin d'etre stabilise.");
    }
    int seconds = Math.max(10,
        Emulator.getConfig().getInt("features.ems.stabilize.seconds", 90));
    RolePlay.getDeathHandler().stabilize(patient, seconds);
    persistTreatment(medic, patient, "STABILIZE", patientData.getHealth(),
        patientData.getHealth(), "Stabilisation " + seconds + " secondes");
    markTreatmentAction(medic, "stabilize");
    consumeMedicAction(medic);
    patient.whisper("Vous avez ete stabilise pendant " + seconds + " seconde(s).",
        RoomChatMessageBubbles.RADIO);
    return TreatmentResult.success(patient.getHabboInfo().getUsername()
        + " est stabilise pendant " + seconds + " seconde(s).");
  }

  public TreatmentResult revive(Habbo medic, Habbo patient) {
    TreatmentResult validation = validateTreatment(medic, patient,
        JobPermissions.MEDICAL_REVIVE, "revive");
    if (!validation.success()) {
      return validation;
    }
    RpAvatar patientData = RolePlay.getAvatarManager().getRpAvatar(patient);
    if (!patientData.isDead()) {
      return TreatmentResult.failure("Le patient est deja conscient.");
    }
    int before = patientData.getHealth();
    int reviveHealth = Math.max(1, Math.min(patientData.getMaxHealth(),
        Emulator.getConfig().getInt("features.ems.revive.health", 35)));
    RolePlay.getDeathHandler().cancelHospitalTransfer(patient);
    RolePlay.getHospitalService().finishHealing(patient);
    patientData.setDead(false);
    patientData.setHealth(reviveHealth);
    if (patient.getRoomUnit() != null) {
      RoomUnit unit = patient.getRoomUnit();
      unit.cmdLay = false;
      unit.cmdSit = false;
      unit.clearStatus();
      unit.setCanWalk(true);
      unit.statusUpdate(true);
      Room room = patient.getHabboInfo().getCurrentRoom();
      if (room != null) {
        room.updateHabbo(patient);
      }
    }
    patientData.updateLife();
    patientData.updateDatabase();
    persistTreatment(medic, patient, "REVIVE", before, reviveHealth, "Reanimation EMS");
    markTreatmentAction(medic, "revive");
    consumeMedicAction(medic);
    patient.whisper("Vous avez ete reanime avec " + reviveHealth + " points de sante.",
        RoomChatMessageBubbles.RADIO);
    return TreatmentResult.success(patient.getHabboInfo().getUsername()
        + " a ete reanime avec succes.");
  }

  public TreatmentResult transportToHospital(Habbo medic, Habbo patient) {
    TreatmentResult validation = validateTreatment(medic, patient,
        JobPermissions.MEDICAL_AMBULANCE, "transport");
    if (!validation.success()) {
      return validation;
    }
    Optional<Room> hospital = RolePlay.getHospitalService().getHospital();
    if (hospital.isEmpty()) {
      return TreatmentResult.failure("L'appartement de l'hopital n'est pas configure.");
    }
    RpAvatar patientData = RolePlay.getAvatarManager().getRpAvatar(patient);
    if (!patientData.isDead() && patientData.getHealth() >= patientData.getMaxHealth()) {
      return TreatmentResult.failure("Le patient n'a pas besoin d'un transport medical.");
    }
    int roomId = currentRoomId(medic);
    RolePlay.getDeathHandler().cancelHospitalTransfer(patient);
    persistTreatment(medic, patient, "TRANSPORT", patientData.getHealth(),
        patientData.getHealth(), "Transport vers l'hopital " + hospital.get().getId(), roomId);
    markTreatmentAction(medic, "transport");
    consumeMedicAction(medic);
    patient.goToRoom(hospital.get().getId());
    return TreatmentResult.success(patient.getHabboInfo().getUsername()
        + " a ete transporte vers l'hopital.");
  }

  public boolean isMedicOnDuty(Habbo medic, String permission) {
    if (medic == null) {
      return false;
    }
    RpAvatar data = RolePlay.getAvatarManager().getRpAvatar(medic);
    return data != null && data.isDuty() && data.getJobRankEntity() != null
        && data.getJobRankEntity().hasPermission(permission);
  }

  public void onDisconnect(Habbo habbo) {
    if (habbo == null) {
      return;
    }
    int userId = habbo.getHabboInfo().getId();
    repository.releaseMedicAssignments(userId);
    callCooldowns.remove(userId);
    treatmentCooldowns.keySet().removeIf(key -> key.startsWith(userId + ":"));
  }

  private TreatmentResult validateTreatment(Habbo medic, Habbo patient, String permission,
      String action) {
    if (!isMedicOnDuty(medic, permission)) {
      return TreatmentResult.failure("Vous n'avez pas l'autorisation EMS requise ou n'etes pas en service.");
    }
    if (patient == null || medic == patient) {
      return TreatmentResult.failure("Patient invalide.");
    }
    if (medic.getRoomUnit() == null || patient.getRoomUnit() == null
        || medic.getHabboInfo().getCurrentRoom() == null
        || medic.getHabboInfo().getCurrentRoom().getId()
        != patient.getHabboInfo().getCurrentRoom().getId()) {
      return TreatmentResult.failure("Le patient doit etre dans le meme appartement.");
    }
    int range = Math.max(1, Emulator.getConfig().getInt("features.ems.treatment.range", 1));
    int distanceX = Math.abs(patient.getRoomUnit().getX() - medic.getRoomUnit().getX());
    int distanceY = Math.abs(patient.getRoomUnit().getY() - medic.getRoomUnit().getY());
    if (distanceX > range || distanceY > range) {
      return TreatmentResult.failure("Vous devez vous rapprocher du patient.");
    }
    long now = System.currentTimeMillis();
    long cooldown = Math.max(1,
        Emulator.getConfig().getInt("features.ems.treatment.cooldown.seconds", 3)) * 1000L;
    String key = medic.getHabboInfo().getId() + ":" + action;
    Long previous = treatmentCooldowns.get(key);
    if (previous != null && now - previous < cooldown) {
      return TreatmentResult.failure("Ce traitement est encore en recharge.");
    }
    return TreatmentResult.success("");
  }

  private void markTreatmentAction(Habbo medic, String action) {
    treatmentCooldowns.put(medic.getHabboInfo().getId() + ":" + action,
        System.currentTimeMillis());
  }

  private void consumeMedicAction(Habbo medic) {
    RpAvatar medicData = RolePlay.getAvatarManager().getRpAvatar(medic);
    if (medicData != null) {
      medicData.executeAction();
    }
  }

  private void persistTreatment(Habbo medic, Habbo patient, String type, int healthBefore,
      int healthAfter, String notes) {
    persistTreatment(medic, patient, type, healthBefore, healthAfter, notes,
        currentRoomId(medic));
  }

  private void persistTreatment(Habbo medic, Habbo patient, String type, int healthBefore,
      int healthAfter, String notes, int roomId) {
    repository.recordTreatment(patient.getHabboInfo().getId(), medic.getHabboInfo().getId(),
        roomId, type, healthBefore, healthAfter, notes);
  }

  private static int currentRoomId(Habbo habbo) {
    Room room = habbo == null ? null : habbo.getHabboInfo().getCurrentRoom();
    return room == null ? 0 : room.getId();
  }

  private void notifyMedicalStaff(EmsCall call) {
    for (Habbo medic : Emulator.getGameEnvironment().getHabboManager().getOnlineHabbos().values()) {
      if (!isMedicOnDuty(medic, JobPermissions.MEDICAL_AMBULANCE)
          && !isMedicOnDuty(medic, JobPermissions.MEDICAL_DISPATCH)) {
        continue;
      }
      String message = "Appel EMS #" + call.id() + " - " + call.callerName() + " : "
          + call.reason() + " dans " + call.roomName() + " (" + call.roomId() + ")";
      medic.whisper(message, RoomChatMessageBubbles.RADIO);
      THashMap<String, String> keys = new THashMap<>();
      keys.put("display", "BUBBLE");
      keys.put("image", "${image.library.url}notifications/mention.png");
      keys.put("linkUrl", "event:navigator/goto/" + call.roomId());
      keys.put("message", message);
      medic.getClient().sendResponse(new BubbleAlertComposer("emscall", keys));
    }
  }

  static String sanitizeReason(String reason) {
    String value = reason == null ? "" : reason.replaceAll("[\\p{Cntrl}&&[^\\r\\n\\t]]", "")
        .replaceAll("\\s+", " ").trim();
    if (value.isEmpty()) {
      return "Urgence medicale";
    }
    return value.length() > 160 ? value.substring(0, 160) : value;
  }

  public record CallResult(boolean success, String message, EmsCall call) {
    static CallResult success(String message, EmsCall call) {
      return new CallResult(true, message, call);
    }

    static CallResult failure(String message) {
      return new CallResult(false, message, null);
    }
  }

  public record TreatmentResult(boolean success, String message) {
    static TreatmentResult success(String message) {
      return new TreatmentResult(true, message);
    }

    static TreatmentResult failure(String message) {
      return new TreatmentResult(false, message);
    }
  }
}

