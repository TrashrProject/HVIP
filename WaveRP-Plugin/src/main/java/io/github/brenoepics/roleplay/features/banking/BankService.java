package io.github.brenoepics.roleplay.features.banking;

import com.eu.habbo.Emulator;
import com.eu.habbo.habbohotel.users.Habbo;
import io.github.brenoepics.roleplay.features.banking.entities.*;
import lombok.extern.slf4j.Slf4j;
import java.math.BigDecimal;
import java.util.*;
import java.util.concurrent.ConcurrentHashMap;
import java.util.concurrent.locks.ReentrantLock;

@Slf4j public class BankService {
  private static final BigDecimal ATM_FEE_PERCENTAGE=BigDecimal.ZERO;
  private final BankRepository repository=new BankRepository();
  private final Map<Integer,ReentrantLock> locks=new ConcurrentHashMap<>();
  private final Map<Integer,Long> lastOperation=new ConcurrentHashMap<>();
  public Optional<BankAccount> getBankAccount(int id){return repository.findBankAccountByUserId(id).filter(BankAccount::isActive);}
  public Optional<BankAccount> getAnyBankAccount(int id){return repository.findBankAccountByUserId(id);}
  public boolean hasBankAccount(int id){return getBankAccount(id).isPresent();}
  public BankAccount createBankAccount(int id){try{return repository.createOrReactivateAccount(id);}catch(Exception e){throw new IllegalStateException("Ouverture du compte impossible",e);}}
  public boolean closeBankAccount(int id){return repository.setAccountActive(id,false);}
  public boolean deposit(int id,BigDecimal amount,int room){return move(id,amount,true,room,BankTransaction.TransactionType.DEPOSIT,null);}
  public boolean mobileDeposit(int id,BigDecimal amount,int room){int units=units(amount);if(units<1||!rateAllowed(id))return false;ReentrantLock l=locks.computeIfAbsent(id,k->new ReentrantLock());l.lock();try{Optional<BankRepository.BalanceSnapshot> result=repository.move(id,units,true,room,BankTransaction.TransactionType.DEPOSIT,null,true);result.ifPresent(s->sync(id,s));return result.isPresent();}finally{l.unlock();}}
  public long getMobileDepositCooldownSeconds(int id){return repository.getMobileCooldownSeconds(id);}
  public boolean withdraw(int id,BigDecimal amount,int room){return move(id,amount,false,room,BankTransaction.TransactionType.WITHDRAW,null);}
  public boolean bankerDeposit(int id,BigDecimal amount,int room,int employee){return move(id,amount,true,room,BankTransaction.TransactionType.BANKER_DEPOSIT,employee);}
  public boolean bankerWithdraw(int id,BigDecimal amount,int room,int employee){return move(id,amount,false,room,BankTransaction.TransactionType.BANKER_WITHDRAWAL,employee);}
  private boolean move(int id,BigDecimal amount,boolean deposit,int room,BankTransaction.TransactionType type,Integer employee){int units=units(amount);if(units<1||!rateAllowed(id))return false;ReentrantLock l=locks.computeIfAbsent(id,k->new ReentrantLock());l.lock();try{Optional<BankRepository.BalanceSnapshot> result=repository.move(id,units,deposit,room,type,employee,false);result.ifPresent(s->sync(id,s));return result.isPresent();}finally{l.unlock();}}
  public boolean transfer(int from,int to,BigDecimal amount,int room){int units=units(amount);if(units<1||from==to||!rateAllowed(from))return false;int a=Math.min(from,to),b=Math.max(from,to);ReentrantLock la=locks.computeIfAbsent(a,k->new ReentrantLock()),lb=locks.computeIfAbsent(b,k->new ReentrantLock());la.lock();lb.lock();try{Optional<BankRepository.BalanceSnapshot[]> r=repository.transfer(from,to,units,room);r.ifPresent(x->{sync(from,x[0]);sync(to,x[1]);});return r.isPresent();}finally{lb.unlock();la.unlock();}}
  private boolean rateAllowed(int id){long now=System.currentTimeMillis();Long previous=lastOperation.put(id,now);return previous==null||now-previous>=750L;}
  private int units(BigDecimal amount){try{if(amount==null||amount.signum()<=0||amount.stripTrailingZeros().scale()>0)return -1;return amount.intValueExact();}catch(ArithmeticException e){return -1;}}
  private void sync(int id,BankRepository.BalanceSnapshot s){Habbo h=Emulator.getGameEnvironment().getHabboManager().getHabbo(id);if(h==null)return;h.getHabboInfo().getCurrencies().put(200,s.bank());h.getHabboInfo().setCredits(s.cash());h.getHabboInfo().run();}
  public boolean canDeposit(int id,BigDecimal a){Habbo h=Emulator.getGameEnvironment().getHabboManager().getHabbo(id);int n=units(a);return hasBankAccount(id)&&h!=null&&n>0&&h.getHabboInfo().getCredits()>=n;}
  public boolean canWithdraw(int id,BigDecimal a){int n=units(a);return n>0&&getBankAccount(id).map(x->x.getBankBalance().intValue()>=n).orElse(false);}
  public boolean canTransfer(int from,int to,BigDecimal a){return from!=to&&hasBankAccount(to)&&canWithdraw(from,a);}
  public List<BankTransaction> getTransactionHistory(int id,int limit){return repository.getTransactionsByUserId(id,limit);}
  public List<ATMRobbery> getATMRobberyHistory(int id,int limit){return repository.getATMRobberiesByUserId(id,limit);}
  public BigDecimal getATMFeePercentage(){return ATM_FEE_PERCENTAGE;}
  public String formatCurrency(BigDecimal a){return String.format("%,d crédits",a.intValue());}
  public boolean attemptATMRobbery(int id,int room,int furni,String weapon){return false;}
}
