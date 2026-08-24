package io.github.brenoepics.roleplay.features.farm.data;

import com.eu.habbo.Emulator;
import gnu.trove.map.hash.THashMap;
import java.sql.ResultSet;
import java.sql.SQLException;
import java.util.ArrayList;
import java.util.List;
import java.util.Map;
import lombok.extern.slf4j.Slf4j;

@Slf4j
public class FarmItem {

  private final int id;
  private final int furniture;
  private final THashMap<Integer, ItemState> states;
  private final Map<Integer, Integer> rewards;
  private final boolean ownerOnly;
  private final boolean delete;

  public FarmItem(final ResultSet result) throws SQLException {
    this.id = result.getInt("id");
    this.furniture = result.getInt("item_id");
    this.states = new THashMap<>();
    this.rewards = new THashMap<>();
    this.delete = result.getInt("delete") == 1;
    this.ownerOnly = result.getInt("owner_only") == 1;

    String[] stateList = result.getString("states").split("\\|");
    String[] rewardList = result.getString("rewards").split(";");

    for (String state : stateList) {
      this.states.put(Integer.parseInt(state.split(",")[0]), new ItemState(state));
    }

    for (String prize : rewardList) {
      try {
        int rewardId = 0;
        int chance = 100;

        if (prize.contains(":") && prize.split(":").length == 2) {
          rewardId = Integer.parseInt(prize.split(":")[0]);
          chance = Integer.parseInt(prize.split(":")[1]);
        } else if (prize.contains(":")) {
          log.error(
              "Invalid configuration of farmable prizes (id: {}}). '{}' format should be itemId:chance.",
              this.id, prize);
        } else {
          rewardId = Integer.parseInt(prize.replace(":", ""));
        }

        if (chance > 100) {
          chance = 100;
        }

        this.rewards.put(rewardId, chance);
      } catch (Exception e) {
        log.error("Caught exception", e);
      }
    }
  }

  public int getId() {
    return this.id;
  }

  public ItemState getState(int stateId) {
    return this.states.get(stateId);
  }

  public THashMap<Integer, ItemState> getStates() {
    return states;
  }

  public int getFurniId() {
    return furniture;
  }

  public List<Integer> getRandomRewards() {
      if (this.rewards.isEmpty()) {
          return new ArrayList<>();
      }

    List<Integer> rewardList = new ArrayList<>();

    for (Map.Entry<Integer, Integer> set : this.rewards.entrySet()) {
      if (Emulator.getRandom().nextInt(100) <= set.getValue()) {
        rewardList.add(set.getKey());
      }
    }

    return rewardList;
  }


  public boolean delete() {
    return delete;
  }

  public boolean isOwnerOnly() {
    return ownerOnly;
  }
}
