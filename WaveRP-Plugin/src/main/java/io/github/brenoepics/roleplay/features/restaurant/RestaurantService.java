package io.github.brenoepics.roleplay.features.restaurant;

import com.eu.habbo.Emulator;
import com.eu.habbo.habbohotel.rooms.RoomChatMessageBubbles;
import com.eu.habbo.habbohotel.users.Habbo;
import io.github.brenoepics.roleplay.RolePlay;
import io.github.brenoepics.roleplay.features.job.JobEntity;
import io.github.brenoepics.roleplay.features.user.RpAvatar;
import io.github.brenoepics.roleplay.utilities.types.RPItem;
import java.sql.Connection;
import java.sql.PreparedStatement;
import java.sql.ResultSet;
import java.sql.SQLException;
import java.util.ArrayList;
import java.util.List;
import java.util.Map;
import java.util.concurrent.ConcurrentHashMap;

/** Moteur Restaurant commun à tous les métiers restaurant ParadiseRP. */
public final class RestaurantService {

  private static final int TARGET_RANGE = 3;
  private static final Map<Integer, WorkingOrder> WORKING_ORDERS = new ConcurrentHashMap<>();
  private static final Map<String, Invoice> INVOICES = new ConcurrentHashMap<>();

  private RestaurantService() {
  }

  public static boolean isRestaurant(JobEntity job) {
    if (job == null) return false;
    return "zycroque".equalsIgnoreCase(job.getName())
        || "tastycrousty".equalsIgnoreCase(job.getName());
  }

  public static boolean authorize(Habbo employee, String permission) {
    RpAvatar data = RolePlay.getAvatarManager().getRpAvatar(employee);
    if (data == null || !isRestaurant(data.getJobEntity())) {
      employee.whisper("Vous ne travaillez pas dans un restaurant.", RoomChatMessageBubbles.ALERT);
      return false;
    }
    if (!data.isDuty()) {
      employee.whisper("Vous n'êtes pas en service.", RoomChatMessageBubbles.ALERT);
      return false;
    }
    if (data.getJobRankEntity() == null || !data.getJobRankEntity().hasPermission(permission)) {
      employee.whisper("Votre grade ne vous permet pas de faire cela.", RoomChatMessageBubbles.ALERT);
      return false;
    }
    return true;
  }

  public static void showMenu(Habbo employee) {
    RpAvatar data = RolePlay.getAvatarManager().getRpAvatar(employee);
    List<MenuItem> items = loadMenu(data.getJobEntity().getId());
    if (items.isEmpty()) {
      employee.whisper("La carte de ce restaurant est vide.", RoomChatMessageBubbles.ALERT);
      return;
    }
    StringBuilder message = new StringBuilder(data.getJobEntity().getDisplayName()).append(" — Carte : ");
    for (int i = 0; i < items.size(); i++) {
      MenuItem item = items.get(i);
      if (i > 0) message.append(" | ");
      message.append(item.displayName()).append(" (").append(item.code()).append(") : ")
          .append(item.price()).append(" crédits");
    }
    employee.whisper(message.toString(), RoomChatMessageBubbles.ALERT);
  }

  public static void takeOrder(Habbo employee, String username) {
    Habbo customer = nearby(employee, username);
    if (customer == null) return;

    RpAvatar data = RolePlay.getAvatarManager().getRpAvatar(employee);
    WORKING_ORDERS.put(employee.getHabboInfo().getId(), new WorkingOrder(
        data.getJobEntity().getId(), data.getJobEntity().getDisplayName(),
        customer.getHabboInfo().getId(), customer.getHabboInfo().getUsername(), null));

    employee.shout("* Prend la commande de " + customer.getHabboInfo().getUsername() + " *");
    customer.whisper(employee.getHabboInfo().getUsername() + " prend votre commande chez "
        + data.getJobEntity().getDisplayName() + ".", RoomChatMessageBubbles.ALERT);
  }

  public static void prepare(Habbo employee, String product) {
    WorkingOrder order = WORKING_ORDERS.get(employee.getHabboInfo().getId());
    if (order == null) {
      employee.whisper("Prenez d'abord la commande d'un client avec :prendrecommande <pseudo>.",
          RoomChatMessageBubbles.ALERT);
      return;
    }

    MenuItem item = findMenuItem(order.jobId(), product);
    if (item == null) {
      employee.whisper("Ce produit n'existe pas sur la carte. Utilisez :menu.",
          RoomChatMessageBubbles.ALERT);
      return;
    }

    RPItem rpItem = RolePlay.getItemManager().getItems().get(item.itemId());
    if (rpItem == null || !"food".equalsIgnoreCase(rpItem.getInteractionType())) {
      employee.whisper("Cet aliment n'est pas disponible dans le système nourriture.",
          RoomChatMessageBubbles.ALERT);
      return;
    }

    WORKING_ORDERS.put(employee.getHabboInfo().getId(), order.withPrepared(item));
    employee.shout("* Prépare " + item.displayName() + " pour " + order.customerName() + " *");
    employee.whisper("Le plat a été préparé.", RoomChatMessageBubbles.ALERT);
  }

  public static void serve(Habbo employee, String username) {
    WorkingOrder order = WORKING_ORDERS.get(employee.getHabboInfo().getId());
    if (order == null || order.prepared() == null) {
      employee.whisper("Aucun plat n'est prêt à être servi.", RoomChatMessageBubbles.ALERT);
      return;
    }

    Habbo customer = nearby(employee, username);
    if (customer == null) return;
    if (customer.getHabboInfo().getId() != order.customerId()) {
      employee.whisper("Ce plat a été préparé pour " + order.customerName() + ".",
          RoomChatMessageBubbles.ALERT);
      return;
    }

    RPItem rpItem = RolePlay.getItemManager().getItems().get(order.prepared().itemId());
    RpAvatar customerData = RolePlay.getAvatarManager().getRpAvatar(customer);
    if (rpItem == null || customerData == null) {
      employee.whisper("Impossible de servir ce produit.", RoomChatMessageBubbles.ALERT);
      return;
    }

    customerData.getInventory().addItem(customer, rpItem, 1);
    String key = invoiceKey(order.jobId(), customer.getHabboInfo().getId());
    INVOICES.compute(key, (ignored, existing) -> {
      Invoice invoice = existing == null
          ? new Invoice(order.jobId(), order.restaurantName(), customer.getHabboInfo().getId(),
              customer.getHabboInfo().getUsername())
          : existing;
      invoice.add(order.prepared().displayName(), order.prepared().price());
      return invoice;
    });

    WORKING_ORDERS.remove(employee.getHabboInfo().getId());
    employee.shout("* Sert " + order.prepared().displayName() + " à " + order.customerName() + " *");
    customer.whisper("Commande servie : " + order.prepared().displayName() + ".",
        RoomChatMessageBubbles.ALERT);
  }

  public static void sendBill(Habbo employee, String username) {
    Habbo customer = nearby(employee, username);
    if (customer == null) return;

    RpAvatar data = RolePlay.getAvatarManager().getRpAvatar(employee);
    Invoice invoice = INVOICES.get(invoiceKey(data.getJobEntity().getId(), customer.getHabboInfo().getId()));
    if (invoice == null || invoice.total() <= 0) {
      employee.whisper("Aucune addition en attente pour ce client.", RoomChatMessageBubbles.ALERT);
      return;
    }

    customer.whisper(invoice.restaurantName() + " vous demande " + invoice.total()
        + " crédits pour : " + String.join(", ", invoice.items()) + ".",
        RoomChatMessageBubbles.ALERT);
    employee.whisper("Addition envoyée à " + customer.getHabboInfo().getUsername() + " : "
        + invoice.total() + " crédits.", RoomChatMessageBubbles.ALERT);
  }

  public static void cash(Habbo employee, String username) {
    Habbo customer = nearby(employee, username);
    if (customer == null) return;

    RpAvatar data = RolePlay.getAvatarManager().getRpAvatar(employee);
    String key = invoiceKey(data.getJobEntity().getId(), customer.getHabboInfo().getId());
    Invoice invoice = INVOICES.get(key);
    if (invoice == null || invoice.total() <= 0) {
      employee.whisper("Aucune addition en attente pour ce client.", RoomChatMessageBubbles.ALERT);
      return;
    }

    synchronized (invoice) {
      if (invoice.paid) {
        employee.whisper("Cette addition a déjà été réglée.", RoomChatMessageBubbles.ALERT);
        return;
      }
      PaymentResult result = debitAndCreditCompany(invoice.jobId(), customer.getHabboInfo().getId(), invoice.total());
      if (result == PaymentResult.NOT_ENOUGH) {
        employee.whisper("Le client n'a pas assez d'argent.", RoomChatMessageBubbles.ALERT);
        customer.whisper("Vous n'avez pas assez de crédits pour payer cette addition.",
            RoomChatMessageBubbles.ALERT);
        return;
      }
      if (result == PaymentResult.ERROR) {
        employee.whisper("Le paiement a échoué. Aucun crédit n'a été débité.",
            RoomChatMessageBubbles.ALERT);
        return;
      }

      invoice.paid = true;
      INVOICES.remove(key, invoice);
      customer.giveCredits(-invoice.total());
      employee.shout("* Encaisse " + customer.getHabboInfo().getUsername() + " pour "
          + invoice.total() + " crédits *");
      employee.whisper("Paiement effectué avec succès.", RoomChatMessageBubbles.ALERT);
      customer.whisper("Paiement de " + invoice.total() + " crédits effectué chez "
          + invoice.restaurantName() + ".", RoomChatMessageBubbles.ALERT);
    }
  }

  private static Habbo nearby(Habbo employee, String username) {
    if (employee.getHabboInfo().getCurrentRoom() == null) {
      employee.whisper("Vous n'êtes pas dans un appartement.", RoomChatMessageBubbles.ALERT);
      return null;
    }
    Habbo customer = employee.getHabboInfo().getCurrentRoom().getHabbo(username);
    if (customer == null) {
      employee.whisper("Le joueur " + username + " est introuvable dans l'appartement.",
          RoomChatMessageBubbles.ALERT);
      return null;
    }
    if (customer == employee) {
      employee.whisper("Vous ne pouvez pas cibler votre propre personnage.",
          RoomChatMessageBubbles.ALERT);
      return null;
    }
    int x = Math.abs(employee.getRoomUnit().getX() - customer.getRoomUnit().getX());
    int y = Math.abs(employee.getRoomUnit().getY() - customer.getRoomUnit().getY());
    if (x > TARGET_RANGE || y > TARGET_RANGE) {
      employee.whisper("Le client doit être à " + TARGET_RANGE + " cases maximum.",
          RoomChatMessageBubbles.ALERT);
      return null;
    }
    return customer;
  }

  private static List<MenuItem> loadMenu(int jobId) {
    List<MenuItem> result = new ArrayList<>();
    String sql = "SELECT code, display_name, item_id, price FROM restaurant_menu "
        + "WHERE job_id = ? AND active = 1 ORDER BY id";
    try (Connection connection = Emulator.getDatabase().getDataSource().getConnection();
         PreparedStatement statement = connection.prepareStatement(sql)) {
      statement.setInt(1, jobId);
      try (ResultSet set = statement.executeQuery()) {
        while (set.next()) {
          result.add(new MenuItem(set.getString("code"), set.getString("display_name"),
              set.getInt("item_id"), set.getInt("price")));
        }
      }
    } catch (SQLException exception) {
      Emulator.getLogging().logSQLException(exception);
    }
    return result;
  }

  private static MenuItem findMenuItem(int jobId, String query) {
    String sql = "SELECT code, display_name, item_id, price FROM restaurant_menu "
        + "WHERE job_id = ? AND active = 1 AND (LOWER(code)=LOWER(?) OR LOWER(display_name)=LOWER(?)) LIMIT 1";
    try (Connection connection = Emulator.getDatabase().getDataSource().getConnection();
         PreparedStatement statement = connection.prepareStatement(sql)) {
      statement.setInt(1, jobId);
      statement.setString(2, query.trim());
      statement.setString(3, query.trim());
      try (ResultSet set = statement.executeQuery()) {
        if (set.next()) {
          return new MenuItem(set.getString("code"), set.getString("display_name"),
              set.getInt("item_id"), set.getInt("price"));
        }
      }
    } catch (SQLException exception) {
      Emulator.getLogging().logSQLException(exception);
    }
    return null;
  }

  private static PaymentResult debitAndCreditCompany(int jobId, int customerId, int amount) {
    String read = "SELECT credits FROM users WHERE id = ? FOR UPDATE";
    String debit = "UPDATE users SET credits = credits - ? WHERE id = ? AND credits >= ?";
    String credit = "INSERT INTO restaurant_accounts(job_id,balance) VALUES(?,?) "
        + "ON DUPLICATE KEY UPDATE balance = balance + VALUES(balance)";

    try (Connection connection = Emulator.getDatabase().getDataSource().getConnection()) {
      connection.setAutoCommit(false);
      try {
        int currentCredits;
        try (PreparedStatement statement = connection.prepareStatement(read)) {
          statement.setInt(1, customerId);
          try (ResultSet set = statement.executeQuery()) {
            if (!set.next()) {
              connection.rollback();
              return PaymentResult.ERROR;
            }
            currentCredits = set.getInt("credits");
          }
        }
        if (currentCredits < amount) {
          connection.rollback();
          return PaymentResult.NOT_ENOUGH;
        }
        try (PreparedStatement statement = connection.prepareStatement(debit)) {
          statement.setInt(1, amount);
          statement.setInt(2, customerId);
          statement.setInt(3, amount);
          if (statement.executeUpdate() != 1) {
            connection.rollback();
            return PaymentResult.NOT_ENOUGH;
          }
        }
        try (PreparedStatement statement = connection.prepareStatement(credit)) {
          statement.setInt(1, jobId);
          statement.setInt(2, amount);
          statement.executeUpdate();
        }
        connection.commit();
        return PaymentResult.OK;
      } catch (SQLException exception) {
        connection.rollback();
        throw exception;
      } finally {
        connection.setAutoCommit(true);
      }
    } catch (SQLException exception) {
      Emulator.getLogging().logSQLException(exception);
      return PaymentResult.ERROR;
    }
  }

  private static String invoiceKey(int jobId, int customerId) {
    return jobId + ":" + customerId;
  }

  public record MenuItem(String code, String displayName, int itemId, int price) {
  }

  private record WorkingOrder(int jobId, String restaurantName, int customerId,
                              String customerName, MenuItem prepared) {
    WorkingOrder withPrepared(MenuItem item) {
      return new WorkingOrder(jobId, restaurantName, customerId, customerName, item);
    }
  }

  private static final class Invoice {
    private final int jobId;
    private final String restaurantName;
    private final int customerId;
    private final String customerName;
    private final List<String> items = new ArrayList<>();
    private int total;
    private boolean paid;

    private Invoice(int jobId, String restaurantName, int customerId, String customerName) {
      this.jobId = jobId;
      this.restaurantName = restaurantName;
      this.customerId = customerId;
      this.customerName = customerName;
    }

    void add(String item, int price) {
      items.add(item);
      total += price;
    }

    int jobId() { return jobId; }
    String restaurantName() { return restaurantName; }
    List<String> items() { return List.copyOf(items); }
    int total() { return total; }
  }

  private enum PaymentResult {
    OK,
    NOT_ENOUGH,
    ERROR
  }
}
