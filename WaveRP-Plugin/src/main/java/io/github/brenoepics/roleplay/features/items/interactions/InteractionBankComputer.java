package io.github.brenoepics.roleplay.features.items.interactions;

import com.eu.habbo.habbohotel.gameclients.GameClient;
import com.eu.habbo.habbohotel.items.Item;
import com.eu.habbo.habbohotel.items.interactions.InteractionDefault;
import com.eu.habbo.habbohotel.rooms.Room;
import com.eu.habbo.habbohotel.rooms.RoomChatMessageBubbles;
import io.github.brenoepics.roleplay.RolePlay;
import io.github.brenoepics.roleplay.features.banking.BankComputerSessionManager;
import io.github.brenoepics.roleplay.features.user.RpAvatar;
import java.sql.ResultSet;
import java.sql.SQLException;

public class InteractionBankComputer extends InteractionDefault {
  public InteractionBankComputer(ResultSet set,Item baseItem)throws SQLException{super(set,baseItem);}
  public InteractionBankComputer(int id,int userId,Item item,String extra,int limitedStack,int limitedSells){super(id,userId,item,extra,limitedStack,limitedSells);}
  public boolean isUsable(){return true;}
  public void onClick(GameClient client,Room room,Object[] objects)throws Exception{
    if(client.getHabbo()==null||room==null)return;
    if(!BankComputerSessionManager.isConfigured(getId(),room.getId())){super.onClick(client,room,objects);return;}
    RpAvatar rp=RolePlay.getAvatarManager().getRpAvatar(client.getHabbo());
    if(rp==null||!rp.isDuty()||rp.getJobEntity()==null||!"bank".equalsIgnoreCase(rp.getJobEntity().getName())){client.getHabbo().whisper("Vous devez être employé de banque en service pour utiliser ce poste.",RoomChatMessageBubbles.ALERT);return;}
    if(!BankComputerSessionManager.hasActiveSession(client.getHabbo())
        &&!BankComputerSessionManager.isAtAssignedChair(client.getHabbo(),this)){
      client.getHabbo().whisper("Vous devez être assis devant l'ordinateur pour vous connecter.",RoomChatMessageBubbles.ALERT);return;
    }
    boolean connected=BankComputerSessionManager.toggle(client.getHabbo(),room,this);
    if(connected){
      client.getHabbo().shout("* Se connecte au poste informatique de Paradise Bank *",RoomChatMessageBubbles.NORMAL);
      client.getHabbo().whisper("Session bancaire ouverte pendant 10 minutes. Vous restez assis jusqu'à la fermeture du poste.",RoomChatMessageBubbles.ALERT);
    }else{
      client.getHabbo().shout("* Se déconnecte du poste informatique de Paradise Bank *",RoomChatMessageBubbles.NORMAL);
      client.getHabbo().whisper("Session bancaire fermée.",RoomChatMessageBubbles.ALERT);
    }
  }
}
