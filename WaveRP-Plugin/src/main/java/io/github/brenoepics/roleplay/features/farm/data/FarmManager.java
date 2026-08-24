package io.github.brenoepics.roleplay.features.farm.data;

import com.eu.habbo.Emulator;
import gnu.trove.map.hash.THashMap;

import java.sql.Connection;
import java.sql.PreparedStatement;
import java.sql.ResultSet;
import java.sql.SQLException;
import java.util.ArrayList;
import java.util.List;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;

public class FarmManager {

    private static final Logger log = LoggerFactory.getLogger(FarmManager.class);
    private final List<FarmItem> fables = new ArrayList<>();
    private final List<RewardItem> rewards = new ArrayList<>();
    private final THashMap<Integer, Integer> cooldowns = new THashMap<>();

    public FarmManager() {
        this.load();
    }

    public boolean load() {
        this.dispose();
        try (final Connection connection = Emulator.getDatabase().getDataSource().getConnection();

             final PreparedStatement statement = connection.prepareStatement("SELECT * FROM `items_farmable`");
             final ResultSet set = statement.executeQuery()) {
            while (set.next()) {
                final FarmItem item = new FarmItem(set);
                this.fables.add(item);
            }
        } catch (SQLException e) {
            log.error("[RP-FARM]", e);
            return false;
        }

        try (final Connection connection = Emulator.getDatabase().getDataSource().getConnection();
             final PreparedStatement statement = connection.prepareStatement("SELECT * FROM `items_farmable_rewards`");
             final ResultSet set = statement.executeQuery()) {
            while (set.next()) {
                final RewardItem item = new RewardItem(set);
                this.rewards.add(item);
            }
        } catch (SQLException e) {
            log.error("[RP-FARM]", e);
            return false;
        }

        
        log.info("[RP-FARM] Loaded {} items successfully!", this.fables.size());
        return true;
    }

    public void dispose() {
        this.fables.clear();
        this.rewards.clear();
    }

    public boolean log(final int itemId, final int userId, final int roomId, final int rewardId) {
        try (final Connection connection = Emulator.getDatabase().getDataSource().getConnection();
             final PreparedStatement statement = connection.prepareStatement("INSERT INTO `items_farmable_log` (`item_id`, `timestamp`, `user_id`, `room_id`, `reward_id`) VALUES (?, ?, ?, ?, ?)")) {
            statement.setInt(1, itemId);
            statement.setInt(2, Emulator.getIntUnixTimestamp());
            statement.setInt(3, userId);
            statement.setInt(4, roomId);
            statement.setInt(5, rewardId);
            statement.execute();
        } catch (SQLException e) {
            log.error("[RP-FARM]", e);
            return false;
        }
        return true;
    }

    public List<FarmItem> getItems() {
        return fables;
    }

    public FarmItem getItemByFurni(int id) {
        return fables.stream().filter(fb -> fb.getFurniId() == id).findAny().orElse(null);
    }

    public List<RewardItem> getRewards() {
        return rewards;
    }

    public RewardItem getReward(int id) {
        return rewards.stream().filter(fb -> fb.getId() == id).findAny().orElse(null);
    }

    public Integer getCooldown(Integer id) {
        return cooldowns.get(id) - Emulator.getIntUnixTimestamp();
    }

    public boolean isOnCooldown(int id) {
        if (cooldowns.containsKey(id)) {
            if (cooldowns.get(id) > Emulator.getIntUnixTimestamp()) {
                return true;
            } else {
                cooldowns.remove(id);
                return false;
            }
        } else {
            return false;
        }
    }

    public void setCooldown(int id, int time) {
        cooldowns.put(id, Emulator.getIntUnixTimestamp() + time);
    }
}
