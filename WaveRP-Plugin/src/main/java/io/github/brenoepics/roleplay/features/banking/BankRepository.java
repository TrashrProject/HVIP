package io.github.brenoepics.roleplay.features.banking;

import com.eu.habbo.Emulator;
import io.github.brenoepics.roleplay.features.banking.entities.ATMRobbery;
import io.github.brenoepics.roleplay.features.banking.entities.BankAccount;
import io.github.brenoepics.roleplay.features.banking.entities.BankTransaction;
import lombok.extern.slf4j.Slf4j;
import java.math.BigDecimal;
import java.security.SecureRandom;
import java.sql.*;
import java.util.*;

/** Source unique: users.credits = espèces, users_currency/type 200 = banque. */
@Slf4j public class BankRepository {
  public record BalanceSnapshot(int bank,int cash){}
  private static final SecureRandom RANDOM=new SecureRandom();
  private Connection db() throws SQLException{return Emulator.getDatabase().getDataSource().getConnection();}

  public Optional<BankAccount> findBankAccountByUserId(int id){
    String q="SELECT ba.user_id,ba.account_number,ba.is_active,ba.created_at,ba.updated_at,COALESCE(uc.amount,0) balance FROM bank_accounts ba LEFT JOIN users_currency uc ON uc.user_id=ba.user_id AND uc.type=200 WHERE ba.user_id=?";
    try(Connection c=db();PreparedStatement s=c.prepareStatement(q)){s.setInt(1,id);try(ResultSet r=s.executeQuery()){if(r.next())return Optional.of(new BankAccount(r.getInt(1),r.getString(2),BigDecimal.valueOf(r.getInt(6)),r.getBoolean(3),r.getTimestamp(4),r.getTimestamp(5)));}}catch(SQLException e){log.error("Lecture compte {}",id,e);}return Optional.empty();
  }
  public BankAccount createOrReactivateAccount(int id)throws SQLException{
    try(Connection c=db()){c.setAutoCommit(false);try{
      String number=null;try(PreparedStatement s=c.prepareStatement("SELECT account_number FROM bank_accounts WHERE user_id=? FOR UPDATE")){s.setInt(1,id);try(ResultSet r=s.executeQuery()){if(r.next())number=r.getString(1);}}
      if(number==null){number=number(c);try(PreparedStatement s=c.prepareStatement("INSERT INTO bank_accounts(user_id,account_number,is_active) VALUES(?,?,1)")){s.setInt(1,id);s.setString(2,number);s.executeUpdate();}}
      else try(PreparedStatement s=c.prepareStatement("UPDATE bank_accounts SET is_active=1,updated_at=CURRENT_TIMESTAMP WHERE user_id=?")){s.setInt(1,id);s.executeUpdate();}
      ensureCurrency(c,id);c.commit();return new BankAccount(id,number);
    }catch(Exception e){rollback(c);throw e;}}
  }
  public boolean setAccountActive(int id,boolean active){try(Connection c=db();PreparedStatement s=c.prepareStatement("UPDATE bank_accounts SET is_active=?,updated_at=CURRENT_TIMESTAMP WHERE user_id=?")){s.setBoolean(1,active);s.setInt(2,id);return s.executeUpdate()==1;}catch(SQLException e){log.error("Fermeture compte {}",id,e);return false;}}

  public Optional<BalanceSnapshot> move(int id,int amount,boolean deposit,int room,BankTransaction.TransactionType type,Integer employee,boolean mobile){
    try(Connection c=db()){c.setAutoCommit(false);try{
      if(!active(c,id)){rollback(c);return Optional.empty();}ensureCurrency(c,id);
      if(mobile){ensureMobileCooldown(c,id);if(mobileCooldownSeconds(c,id)>0){rollback(c);return Optional.empty();}}
      int cash=value(c,"SELECT credits FROM users WHERE id=? FOR UPDATE",id), bank=value(c,"SELECT amount FROM users_currency WHERE user_id=? AND type=200 FOR UPDATE",id);
      if(deposit?cash<amount:bank<amount){rollback(c);return Optional.empty();}
      int nc=deposit?cash-amount:cash+amount, nb=deposit?bank+amount:bank-amount;
      update(c,"UPDATE users SET credits=? WHERE id=?",nc,id);update(c,"UPDATE users_currency SET amount=? WHERE user_id=? AND type=200",nb,id);
      if(mobile)try(PreparedStatement s=c.prepareStatement("UPDATE bank_mobile_deposit_cooldowns SET last_deposit_at=CURRENT_TIMESTAMP WHERE user_id=?")){s.setInt(1,id);s.executeUpdate();}
      transaction(c,id,id,type,amount,room,bank,nb,employee);c.commit();return Optional.of(new BalanceSnapshot(nb,nc));
    }catch(Exception e){rollback(c);log.error("Mouvement bancaire atomique {}",id,e);return Optional.empty();}}catch(SQLException e){log.error("Connexion banque",e);return Optional.empty();}
  }
  public long getMobileCooldownSeconds(int id){try(Connection c=db()){ensureMobileCooldown(c,id);return mobileCooldownSeconds(c,id);}catch(SQLException e){log.error("Cooldown dépôt mobile {}",id,e);return 1800;}}
  public Optional<BalanceSnapshot[]> transfer(int from,int to,int amount,int room){
    try(Connection c=db()){c.setAutoCommit(false);try{
      int a=Math.min(from,to),b=Math.max(from,to);if(!active(c,a)||!active(c,b)){rollback(c);return Optional.empty();}ensureCurrency(c,from);ensureCurrency(c,to);
      int fb=value(c,"SELECT amount FROM users_currency WHERE user_id=? AND type=200 FOR UPDATE",from),tb=value(c,"SELECT amount FROM users_currency WHERE user_id=? AND type=200 FOR UPDATE",to);if(fb<amount){rollback(c);return Optional.empty();}
      update(c,"UPDATE users_currency SET amount=? WHERE user_id=? AND type=200",fb-amount,from);update(c,"UPDATE users_currency SET amount=? WHERE user_id=? AND type=200",tb+amount,to);
      transaction(c,from,to,BankTransaction.TransactionType.TRANSFER,amount,room,fb,fb-amount,null);int fc=value(c,"SELECT credits FROM users WHERE id=?",from),tc=value(c,"SELECT credits FROM users WHERE id=?",to);c.commit();return Optional.of(new BalanceSnapshot[]{new BalanceSnapshot(fb-amount,fc),new BalanceSnapshot(tb+amount,tc)});
    }catch(Exception e){rollback(c);log.error("Virement atomique {} -> {}",from,to,e);return Optional.empty();}}catch(SQLException e){log.error("Connexion banque",e);return Optional.empty();}
  }
  public List<BankTransaction> getTransactionsByUserId(int id,int limit){String q="SELECT id,from_user_id,to_user_id,transaction_type,amount,fee_amount,description,room_id,created_at FROM bank_transactions WHERE from_user_id=? OR to_user_id=? ORDER BY id DESC LIMIT ?";List<BankTransaction> out=new ArrayList<>();try(Connection c=db();PreparedStatement s=c.prepareStatement(q)){s.setInt(1,id);s.setInt(2,id);s.setInt(3,limit);try(ResultSet r=s.executeQuery()){while(r.next()){BankTransaction t=new BankTransaction();t.setId(r.getInt(1));t.setFromUserId((Integer)r.getObject(2));t.setToUserId((Integer)r.getObject(3));t.setTransactionType(BankTransaction.TransactionType.valueOf(r.getString(4).toUpperCase()));t.setAmount(r.getBigDecimal(5));t.setFeeAmount(r.getBigDecimal(6));t.setDescription(r.getString(7));t.setRoomId((Integer)r.getObject(8));t.setCreatedAt(r.getTimestamp(9));out.add(t);}}}catch(SQLException e){log.error("Historique {}",id,e);}return out;}
  public boolean saveTransaction(BankTransaction t){try(Connection c=db()){transaction(c,t.getFromUserId(),t.getToUserId(),t.getTransactionType(),t.getAmount().intValue(),t.getRoomId(),null,null,null);return true;}catch(SQLException e){log.error("Transaction journal",e);return false;}}
  public boolean saveATMRobbery(ATMRobbery r){String q="INSERT INTO atm_robberies(user_id,room_id,furni_id,amount_stolen,success,weapon_used,police_alerted) VALUES(?,?,?,?,?,?,?)";try(Connection c=db();PreparedStatement s=c.prepareStatement(q)){s.setInt(1,r.getUserId());s.setInt(2,r.getRoomId());s.setInt(3,r.getFurniId());s.setBigDecimal(4,r.getAmountStolen());s.setBoolean(5,r.isSuccess());s.setString(6,r.getWeaponUsed());s.setBoolean(7,r.isPoliceAlerted());return s.executeUpdate()==1;}catch(SQLException e){log.error("Braquage journal",e);return false;}}
  public List<ATMRobbery> getATMRobberiesByUserId(int id,int limit){return List.of();}
  private void ensureCurrency(Connection c,int id)throws SQLException{try(PreparedStatement s=c.prepareStatement("INSERT IGNORE INTO users_currency(user_id,type,amount) VALUES(?,200,0)")){s.setInt(1,id);s.executeUpdate();}}
  private void ensureMobileCooldown(Connection c,int id)throws SQLException{try(PreparedStatement s=c.prepareStatement("INSERT IGNORE INTO bank_mobile_deposit_cooldowns(user_id,last_deposit_at) VALUES(?,NULL)")){s.setInt(1,id);s.executeUpdate();}}
  private long mobileCooldownSeconds(Connection c,int id)throws SQLException{try(PreparedStatement s=c.prepareStatement("SELECT GREATEST(0,1800-TIMESTAMPDIFF(SECOND,last_deposit_at,CURRENT_TIMESTAMP)) FROM bank_mobile_deposit_cooldowns WHERE user_id=? FOR UPDATE")){s.setInt(1,id);try(ResultSet r=s.executeQuery()){return r.next()?r.getLong(1):0;}}}
  private boolean active(Connection c,int id)throws SQLException{try(PreparedStatement s=c.prepareStatement("SELECT is_active FROM bank_accounts WHERE user_id=? FOR UPDATE")){s.setInt(1,id);try(ResultSet r=s.executeQuery()){return r.next()&&r.getBoolean(1);}}}
  private int value(Connection c,String q,int id)throws SQLException{try(PreparedStatement s=c.prepareStatement(q)){s.setInt(1,id);try(ResultSet r=s.executeQuery()){if(!r.next())throw new SQLException("Utilisateur absent");return r.getInt(1);}}}
  private void update(Connection c,String q,int v,int id)throws SQLException{try(PreparedStatement s=c.prepareStatement(q)){s.setInt(1,v);s.setInt(2,id);if(s.executeUpdate()!=1)throw new SQLException("Solde non modifié");}}
  private void transaction(Connection c,Integer from,Integer to,BankTransaction.TransactionType type,int amount,Integer room,Integer oldB,Integer newB,Integer employee)throws SQLException{String q="INSERT INTO bank_transactions(from_user_id,to_user_id,transaction_type,amount,fee_amount,description,room_id,old_balance,new_balance,employee_user_id) VALUES(?,?,?,?,0,?,?,?,?,?)";try(PreparedStatement s=c.prepareStatement(q)){s.setObject(1,from);s.setObject(2,to);s.setString(3,type.name().toLowerCase());s.setInt(4,amount);s.setString(5,type.name());s.setObject(6,room);s.setObject(7,oldB);s.setObject(8,newB);s.setObject(9,employee);s.executeUpdate();}}
  private String number(Connection c)throws SQLException{for(int i=0;i<30;i++){String n=String.format("%010d",Math.floorMod(RANDOM.nextLong(),10_000_000_000L));try(PreparedStatement s=c.prepareStatement("SELECT 1 FROM bank_accounts WHERE account_number=?")){s.setString(1,n);try(ResultSet r=s.executeQuery()){if(!r.next())return n;}}}throw new SQLException("Numéro unique indisponible");}
  private void rollback(Connection c){try{c.rollback();}catch(SQLException ignored){}}
}
