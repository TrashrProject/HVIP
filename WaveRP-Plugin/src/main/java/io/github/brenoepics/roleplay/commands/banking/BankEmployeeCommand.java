package io.github.brenoepics.roleplay.commands.banking;

import com.eu.habbo.habbohotel.commands.Command;
import com.eu.habbo.habbohotel.gameclients.GameClient;
import com.eu.habbo.habbohotel.rooms.RoomChatMessageBubbles;
import com.eu.habbo.habbohotel.users.Habbo;
import io.github.brenoepics.roleplay.RolePlay;
import io.github.brenoepics.roleplay.features.banking.BankComputerSessionManager;
import io.github.brenoepics.roleplay.features.banking.BankManager;
import io.github.brenoepics.roleplay.features.banking.entities.BankAccount;
import io.github.brenoepics.roleplay.features.user.RpAvatar;
import java.math.BigDecimal;
import java.util.Optional;

public class BankEmployeeCommand extends Command {
  public enum Action { ACCOUNT, OPEN, CLOSE, DEPOSIT, WITHDRAW }
  private final Action action;
  public BankEmployeeCommand(String permission,String[] keys,Action action){super(permission,keys);this.action=action;}

  @Override public boolean handle(GameClient client,String[] params){
    Habbo employee=client.getHabbo(); RpAvatar rp=RolePlay.getAvatarManager().getRpAvatar(employee);
    if(rp==null||!rp.isDuty()||rp.getJobEntity()==null||!"bank".equalsIgnoreCase(rp.getJobEntity().getName())){
      employee.whisper("Vous devez être employé de banque en service pour utiliser cette commande.",RoomChatMessageBubbles.ALERT);return true;
    }
    if(!BankComputerSessionManager.hasActiveSession(employee)){
      employee.whisper("Connectez-vous à un ordinateur de Paradise Bank en double-cliquant dessus et restez à proximité.",RoomChatMessageBubbles.ALERT);return true;
    }
    int expected=(action==Action.DEPOSIT||action==Action.WITHDRAW)?3:2;
    if(params.length!=expected){employee.whisper("Utilisation : "+usage(),RoomChatMessageBubbles.ALERT);return true;}
    Habbo target=employee.getHabboInfo().getCurrentRoom()==null?null:employee.getHabboInfo().getCurrentRoom().getHabbo(params[1]);
    if(target==null){employee.whisper("Le client doit être présent au guichet dans cette salle.",RoomChatMessageBubbles.ALERT);return true;}
    BankManager bank=RolePlay.getBankManager(); int targetId=target.getHabboInfo().getId();

    if(action==Action.ACCOUNT){
      Optional<BankAccount> account=bank.getAnyBankAccount(targetId);
      if(account.isEmpty()){employee.whisper("Ce client ne possède aucun compte Paradise Bank.",RoomChatMessageBubbles.ALERT);return true;}
      BankAccount value=account.get();
      employee.whisper("Compte de "+target.getHabboInfo().getUsername()+" | N° "+value.getAccountNumber()+" | Statut : "+(value.isActive()?"actif":"fermé")+" | Banque : "+bank.formatCurrency(value.getBankBalance())+" | Espèces : "+bank.formatCurrency(BigDecimal.valueOf(target.getHabboInfo().getCredits())),RoomChatMessageBubbles.ALERT);
      employee.shout("* Consulte le compte bancaire de "+target.getHabboInfo().getUsername()+" *",RoomChatMessageBubbles.NORMAL);return true;
    }
    if(action==Action.OPEN){
      if(bank.hasBankAccount(targetId)){employee.whisper("Ce client possède déjà un compte actif.",RoomChatMessageBubbles.ALERT);return true;}
      BankAccount account=bank.createBankAccount(targetId);
      employee.shout("* Ouvre un compte bancaire pour "+target.getHabboInfo().getUsername()+" *",RoomChatMessageBubbles.NORMAL);
      target.whisper("Votre compte Paradise Bank est actif. Numéro : "+account.getAccountNumber()+". Aucun argent n'a été ajouté.",RoomChatMessageBubbles.ALERT);return true;
    }
    if(action==Action.CLOSE){
      if(bank.getAnyBankAccount(targetId).isEmpty()){employee.whisper("Ce client ne possède aucun compte bancaire.",RoomChatMessageBubbles.ALERT);return true;}
      if(!bank.closeBankAccount(targetId)){employee.whisper("Ce compte est déjà fermé ou la fermeture a échoué.",RoomChatMessageBubbles.ALERT);return true;}
      employee.shout("* Ferme le compte bancaire de "+target.getHabboInfo().getUsername()+" *",RoomChatMessageBubbles.NORMAL);
      target.whisper("Votre compte bancaire est fermé. Votre argent est intégralement conservé.",RoomChatMessageBubbles.ALERT);return true;
    }

    BigDecimal amount;
    try{amount=new BigDecimal(params[2]);if(amount.signum()<=0||amount.stripTrailingZeros().scale()>0)throw new NumberFormatException();}
    catch(Exception e){employee.whisper(BankManager.ERROR_INVALID_AMOUNT+" Les centimes ne sont pas acceptés.",RoomChatMessageBubbles.ALERT);return true;}
    int roomId=employee.getHabboInfo().getCurrentRoom().getId(); int employeeId=employee.getHabboInfo().getId();
    boolean success=action==Action.DEPOSIT?bank.bankerDeposit(targetId,amount,roomId,employeeId):bank.bankerWithdraw(targetId,amount,roomId,employeeId);
    if(!success){employee.whisper(action==Action.DEPOSIT?"Versement refusé : espèces insuffisantes ou compte inactif.":"Retrait refusé : solde bancaire insuffisant ou compte inactif.",RoomChatMessageBubbles.ALERT);return true;}
    String operation=action==Action.DEPOSIT?"versement":"retrait client";
    String actionLabel=action==Action.DEPOSIT?"versement bancaire":"retrait bancaire";
    employee.shout("* Effectue un "+actionLabel+" de "+amount.toPlainString()+" crédits pour "+target.getHabboInfo().getUsername()+" *",RoomChatMessageBubbles.NORMAL);
    target.whisper("Paradise Bank : "+operation+" de "+bank.formatCurrency(amount)+" validé par "+employee.getHabboInfo().getUsername()+".",RoomChatMessageBubbles.ALERT);return true;
  }

  private String usage(){return switch(action){case ACCOUNT->":compte [pseudo]";case OPEN->":ouvrircompte [pseudo]";case CLOSE->":fermercompte [pseudo]";case DEPOSIT->":versement [pseudo] [montant]";case WITHDRAW->":retraitclient [pseudo] [montant]";};}
}
