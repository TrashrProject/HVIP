package io.github.brenoepics.roleplay.features.items;

import com.eu.habbo.Emulator;
import io.github.brenoepics.roleplay.utilities.types.RPItem;
import java.sql.Connection;
import java.sql.PreparedStatement;
import java.sql.ResultSet;
import java.sql.SQLException;
import java.util.HashMap;
import lombok.Getter;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;

public class ItemManager {
    @Getter
    private HashMap<Integer, RPItem> items;
    private final Logger log = LoggerFactory.getLogger(ItemManager.class);

    public ItemManager() {
        loadItems();
    }

    public void loadItems() {
        items = new HashMap<>();
        try (final Connection connection = Emulator.getDatabase().getDataSource().getConnection(); final PreparedStatement statement = connection.prepareStatement("SELECT * FROM `rp_items`")) {
            try (final ResultSet set = statement.executeQuery()) {
                while (set.next()) {
                    RPItem item = new RPItem(set);
                    items.put(item.getId(), item);
                }
            }
        } catch (SQLException e) {
            log.error("[NaHabbo RolePlay]", e);
        } finally {
            log.info("[NaHabbo RolePlay] Loaded {} items", items.size());
        }
    }

    public RPItem getItemByName(String name) {
        for (RPItem item : items.values()) {
            if (item.getDisplayName().equalsIgnoreCase(name)) {
                return item;
            }
        }
        return null;
    }

    public RPItem getItemById(int id) {
        return items.get(id);
    }
}
