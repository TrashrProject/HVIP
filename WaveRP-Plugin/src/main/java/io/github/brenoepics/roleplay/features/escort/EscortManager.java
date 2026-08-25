package io.github.brenoepics.roleplay.features.escort;

import com.eu.habbo.Emulator;
import com.eu.habbo.habbohotel.users.Habbo;
import io.github.brenoepics.roleplay.features.crime.PoliceHandcuffService;
import io.github.brenoepics.roleplay.features.crime.PoliceTaserService;
import io.github.brenoepics.roleplay.runnables.HabboFHabbo;
import java.util.ArrayList;
import java.util.Collections;
import java.util.List;
import java.util.Set;
import java.util.concurrent.ConcurrentHashMap;

public class EscortManager {

  public static final String ESCORT_VARIABLE = "ESCORTING_USER";
  private final ConcurrentHashMap<Integer, Set<Integer>> escorting = new ConcurrentHashMap<>();

  public EscortManager() {
    /* TODO document why this constructor is empty */
  }

  public void startEscorting(int police, int prisoner) {
    escorting.computeIfAbsent(police, ignored -> ConcurrentHashMap.newKeySet()).add(prisoner);
  }

  public boolean startEscorting(Habbo police, Habbo prisoner) {
    if (police == null || prisoner == null || police.getRoomUnit() == null
        || prisoner.getRoomUnit() == null || police.getHabboInfo().getCurrentRoom() == null
        || police.getHabboInfo().getCurrentRoom() != prisoner.getHabboInfo().getCurrentRoom()) {
      return false;
    }

    int policeId = police.getHabboInfo().getId();
    int prisonerId = prisoner.getHabboInfo().getId();
    startEscorting(policeId, prisonerId);
    prisoner.getHabboStats().cache.put(ESCORT_VARIABLE, policeId);

    // Manual walk packets remain blocked, but pathfinding may move the escorted player.
    prisoner.getRoomUnit().setCanWalk(true);
    Emulator.getThreading().run(
        new HabboFHabbo(prisoner, police, police.getHabboInfo().getCurrentRoom(), 0, 0, false));
    return true;
  }

  public void stopEscorting(int prisoner) {
    escorting.forEach((police, prisoners) -> {
      prisoners.remove(prisoner);
      if (prisoners.isEmpty()) {
        escorting.remove(police, prisoners);
      }
    });

    Habbo target = Emulator.getGameEnvironment().getHabboManager().getHabbo(prisoner);
    if (target == null) {
      return;
    }

    target.getHabboStats().cache.remove(ESCORT_VARIABLE);
    if (target.getRoomUnit() != null) {
      target.getRoomUnit().setCanWalk(!PoliceTaserService.isTased(prisoner)
          && !PoliceHandcuffService.isHandcuffed(prisoner));
    }
  }

  public void stopEscortingByOfficer(int police) {
    for (int prisoner : new ArrayList<>(getEscorted(police))) {
      stopEscorting(prisoner);
    }
    escorting.remove(police);
  }

  public boolean isEscorted(int police) {
    return escorting.containsKey(police) && !escorting.get(police).isEmpty();
  }

  public boolean stillEscorting(int escortingPolice, int prisoner) {
    return isEscorted(escortingPolice) && escorting.get(escortingPolice).contains(prisoner);
  }

  public boolean isPrisonerEscorted(int prisoner) {
    return escorting.values().stream().anyMatch(users -> users.contains(prisoner));
  }

  public int getEscortingOfficer(int prisoner) {
    return escorting.entrySet().stream().filter(entry -> entry.getValue().contains(prisoner))
        .map(java.util.Map.Entry::getKey).findFirst().orElse(0);
  }

  public List<Integer> getEscorted(int police) {
    Set<Integer> prisoners = escorting.get(police);
    return prisoners == null ? Collections.emptyList() : new ArrayList<>(prisoners);
  }

  public static boolean onEscortList(Habbo prisoner, List<Integer> users) {
    if (prisoner == null || prisoner.getHabboInfo() == null) {
      return false;
    }

    return users.contains(prisoner.getHabboInfo().getId());
  }
}
