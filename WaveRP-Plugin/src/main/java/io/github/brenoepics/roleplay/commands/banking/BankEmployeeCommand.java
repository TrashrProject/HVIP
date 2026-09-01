package io.github.brenoepics.roleplay.commands.banking;
import com.eu.habbo.habbohotel.commands.Command;
import com.eu.habbo.habbohotel.gameclients.GameClient;
import com.eu.habbo.habbohotel.rooms.RoomChatMessageBubbles;
import com.eu.habbo.habbohotel.users.Habbo;
import io.github.brenoepics.roleplay.RolePlay;
import io.github.brenoepics.roleplay.features.banking.BankManager;
import io.github.brenoepics.roleplay.features.banking.BankComputerSessionManager;
import io.github.brenoepics.roleplay.features.job.JobPermissions;
import io.github.brenoepics.roleplay.features.user.RpAvatar;
import java.math.BigDecimal;

public class BankEmployeeCommand extends Command {
  public enum Action{ACCOUNT,OPEN,CLOSE,DEPOSIT,WITHDRAW}
  private final Action action;
  public BankEmployeeCommand(String permission,String[] keys,Action action){super(permission,keys);this.action=action;}
  public boolean handle(GameClient client,String[] p){Habbo employee=client.getHabbo();RpAvatar rp=RolePlay.getAvatarManager().getRpAvatar(employee);
    String required=(action==Action.ACCOUNT)?JobPermissions.BANK_ACCOUNT_VIEW:(action==Action.OPEN||action==Action.CLOSE)?JobPermissions.BANK_ACCOUNT_MANAGE:(action==Action.DEPOSIT)?JobPermissions.BANK_COUNTER_DEPOSIT:JobPermissions.BANK_COUNTER_WITHDRAW;
    if(rp==null||!rp.isDuty()||rp.getJobEntity()==null||!"bank".equalsIgnoreCase(rp.getJobEntity().getName())||rp.getJobRankEntity()==null||!rp.getJobRankEntity().hasPermission(required)){employee.whisper("Vous devez être employé de banque en service avec l'autorisation requise.",RoomChatMessageBubbles.ALERT);return true;}
    if(!BankComputerSessionManager.hasActiveSession(employee)){employee.whisper("Connectez-vous d'abord à un ordinateur de Paradise Bank en double-cliquant dessus, puis restez à proximité.",RoomChatMessageBubbles.ALERT);return true;}
    int expected=(action==Action.DEPOSIT||action==Action.WITHDRAW)?3:2;if(p.length!=expected){employee.whisper("Syntaxe bancaire incorrecte.",RoomChatMessageBubbles.ALERT);return true;}
    Habbo target=employee.getHabboInfo().getCurrentRoom()==null?null:employee.getHabboInfo().getCurrentRoom().getHabbo(p[1]);if(target==null){employee.whisper("Le client doit être présent au guichet.",RoomChatMessageBubbles.ALERT);return true;}
    BankManager bank=RolePlay.getBankManager();int id=target.getHabboInfo().getId();
    if(action==Action.ACCOUNT){employee.whisper(bank.getFormattedBalance(id),RoomChatMessageBubbles.ALERT);return true;}
    if(action==Action.OPEN){if(bank.hasBankAccount(id)){employee.whisper("Ce client possède déjà un compte actif.",RoomChatMessageBubbles.ALERT);return true;}bank.createBankAccount(id);target.whisper("Votre compte Paradise Bank est maintenant ouvert.",RoomChatMessageBubbles.ALERT);employee.shout("* Ouvre le compte bancaire de "+p[1]+" *",RoomChatMessageBubbles.NORMAL);return true;}
    if(action==Action.CLOSE){if(!bank.closeBankAccount(id)){employee.whisper("Fermeture impossible.",RoomChatMessageBubbles.ALERT);return true;}target.whisper("Votre compte bancaire a été fermé. Votre solde reste conservé.",RoomChatMessageBubbles.ALERT);return true;}
    BigDecimal amount;try{amount=new BigDecimal(p[2]);}catch(Exception e){employee.whisper(BankManager.ERROR_INVALID_AMOUNT,RoomChatMessageBubbles.ALERT);return true;}int room=employee.getHabboInfo().getCurrentRoom().getId();boolean ok=action==Action.DEPOSIT?bank.bankerDeposit(id,amount,room,employee.getHabboInfo().getId()):bank.bankerWithdraw(id,amount,room,employee.getHabboInfo().getId());
    if(ok){employee.shout("* Effectue une opération bancaire de "+bank.formatCurrency(amount)+" pour "+p[1]+" *",RoomChatMessageBubbles.NORMAL);target.whisper("Opération validée : "+bank.formatCurrency(amount)+".",RoomChatMessageBubbles.ALERT);}else employee.whisper("Opération refusée : compte inactif ou solde insuffisant.",RoomChatMessageBubbles.ALERT);return true;
  }
}
