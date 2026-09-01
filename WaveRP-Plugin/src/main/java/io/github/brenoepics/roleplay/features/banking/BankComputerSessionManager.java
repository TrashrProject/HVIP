package io.github.brenoepics.roleplay.features.banking;

import com.eu.habbo.Emulator;
import com.eu.habbo.habbohotel.rooms.Room;
import com.eu.habbo.habbohotel.rooms.RoomTile;
import com.eu.habbo.habbohotel.users.Habbo;
import com.eu.habbo.habbohotel.users.HabboItem;
import java.sql.Connection;
import java.sql.PreparedStatement;
import java.sql.ResultSet;
import java.util.Map;
import java.util.concurrent.ConcurrentHashMap;

/** Session courte et liée au poste physique : quitter la salle ou s'éloigner coupe l'accès. */
public final class BankComputerSessionManager {
  private static final long SESSION_MS = 10 * 60 * 1000L;
  private static final int MAX_DISTANCE = 2;
  private static final Map<Integer, Session> SESSIONS = new ConcurrentHashMap<>();
  private record Session(int roomId, int itemId, long expiresAt) {}
  private BankComputerSessionManager() {}

  public static boolean isConfigured(int itemId) {
    String sql = "SELECT 1 FROM rp_bank_computer_items WHERE item_id=? AND active=1";
    try (Connection c=Emulator.getDatabase().getDataSource().getConnection(); PreparedStatement s=c.prepareStatement(sql)) {
      s.setInt(1,itemId); try(ResultSet r=s.executeQuery()){return r.next();}
    } catch(Exception e){return false;}
  }
  public static void connect(Habbo habbo, Room room, HabboItem computer) {
    SESSIONS.put(habbo.getHabboInfo().getId(),new Session(room.getId(),computer.getId(),System.currentTimeMillis()+SESSION_MS));
  }
  public static void disconnect(int userId){SESSIONS.remove(userId);}
  public static boolean hasActiveSession(Habbo habbo) {
    if(habbo==null||habbo.getRoomUnit()==null)return false;
    int userId=habbo.getHabboInfo().getId(); Session session=SESSIONS.get(userId);
    Room room=habbo.getHabboInfo().getCurrentRoom();
    if(session==null||room==null||session.expiresAt<System.currentTimeMillis()||session.roomId!=room.getId()){SESSIONS.remove(userId);return false;}
    for(HabboItem item:room.getFloorItems())if(item.getId()==session.itemId){
      RoomTile user=habbo.getRoomUnit().getCurrentLocation(); RoomTile device=room.getLayout().getTile((short)item.getX(),(short)item.getY());
      boolean near=user!=null&&device!=null&&Math.max(Math.abs(user.x-device.x),Math.abs(user.y-device.y))<=MAX_DISTANCE;
      if(!near)SESSIONS.remove(userId); return near;
    }
    SESSIONS.remove(userId);return false;
  }
}
