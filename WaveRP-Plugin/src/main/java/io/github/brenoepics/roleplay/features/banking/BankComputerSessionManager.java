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
  private static final int MAX_DISTANCE = 1;
  private static final Map<Integer, Session> SESSIONS = new ConcurrentHashMap<>();
  private record Session(int roomId, int itemId, long expiresAt) {}
  private BankComputerSessionManager() {}

  public static boolean isConfigured(int itemId) {
    String sql = "SELECT 1 FROM rp_bank_computer_items WHERE item_id=? AND active=1";
    try (Connection c=Emulator.getDatabase().getDataSource().getConnection(); PreparedStatement s=c.prepareStatement(sql)) {
      s.setInt(1,itemId); try(ResultSet r=s.executeQuery()){return r.next();}
    } catch(Exception e){return false;}
  }
  /** @return true when connected, false when the existing session was closed. */
  public static boolean toggle(Habbo habbo, Room room, HabboItem computer) {
    int userId=habbo.getHabboInfo().getId(); Session current=SESSIONS.get(userId);
    if(current!=null&&current.roomId==room.getId()&&current.itemId==computer.getId()&&current.expiresAt>=System.currentTimeMillis()){
      SESSIONS.remove(userId);return false;
    }
    SESSIONS.put(userId,new Session(room.getId(),computer.getId(),System.currentTimeMillis()+SESSION_MS));return true;
  }
  public static void disconnect(int userId){SESSIONS.remove(userId);}
  public static boolean isBankEmployeeOnDuty(Habbo habbo){
    if(habbo==null)return false;
    io.github.brenoepics.roleplay.features.user.RpAvatar rp=io.github.brenoepics.roleplay.RolePlay.getAvatarManager().getRpAvatar(habbo);
    return rp!=null&&rp.isDuty()&&rp.getJobEntity()!=null&&"bank".equalsIgnoreCase(rp.getJobEntity().getName());
  }
  public static boolean mayUsePersonalBankCommand(Habbo habbo){return !isBankEmployeeOnDuty(habbo)||hasActiveSession(habbo);}
  public static void disconnectIfGoalIsFar(Habbo habbo, RoomTile goal){
    if(habbo==null||goal==null)return; Session session=SESSIONS.get(habbo.getHabboInfo().getId()); Room room=habbo.getHabboInfo().getCurrentRoom();
    if(session==null||room==null||session.roomId!=room.getId())return;
    for(HabboItem item:room.getFloorItems())if(item.getId()==session.itemId){
      if(Math.max(Math.abs(goal.x-item.getX()),Math.abs(goal.y-item.getY()))>=2){SESSIONS.remove(habbo.getHabboInfo().getId());habbo.whisper("Vous vous êtes éloigné du poste : session Paradise Bank déconnectée.",com.eu.habbo.habbohotel.rooms.RoomChatMessageBubbles.ALERT);}return;
    }
    SESSIONS.remove(habbo.getHabboInfo().getId());
  }
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
