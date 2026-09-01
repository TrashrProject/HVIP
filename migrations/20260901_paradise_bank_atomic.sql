-- Paradise Bank : migration rejouable, soldes atomiques et ATM explicitement configurés.
SET NAMES utf8mb4;
ALTER TABLE users_currency ROW_FORMAT=DYNAMIC, ENGINE=InnoDB;

DROP PROCEDURE IF EXISTS paradise_bank_migrate;
DELIMITER $$
CREATE PROCEDURE paradise_bank_migrate()
BEGIN
  IF NOT EXISTS(SELECT 1 FROM information_schema.columns WHERE table_schema=DATABASE() AND table_name='bank_accounts' AND column_name='is_active') THEN
    ALTER TABLE bank_accounts ADD COLUMN is_active TINYINT(1) NOT NULL DEFAULT 1 AFTER account_number;
  END IF;
  IF EXISTS(SELECT 1 FROM information_schema.columns WHERE table_schema=DATABASE() AND table_name='bank_accounts' AND column_name='bank_balance') THEN
    INSERT INTO users_currency(user_id,type,amount)
      SELECT user_id,200,GREATEST(0,ROUND(bank_balance)) FROM bank_accounts
      ON DUPLICATE KEY UPDATE amount=GREATEST(users_currency.amount,VALUES(amount));
    ALTER TABLE bank_accounts DROP COLUMN bank_balance;
  END IF;
  IF NOT EXISTS(SELECT 1 FROM information_schema.columns WHERE table_schema=DATABASE() AND table_name='bank_transactions' AND column_name='old_balance') THEN
    ALTER TABLE bank_transactions ADD COLUMN old_balance INT NULL, ADD COLUMN new_balance INT NULL, ADD COLUMN employee_user_id INT NULL;
  END IF;
END$$
DELIMITER ;
CALL paradise_bank_migrate();
DROP PROCEDURE paradise_bank_migrate;

ALTER TABLE bank_transactions MODIFY transaction_type ENUM('deposit','withdraw','transfer','banker_deposit','banker_withdrawal','atm_fee','robbery') NOT NULL;

CREATE TABLE IF NOT EXISTS rp_bank_atm_items (
  base_item_id INT NOT NULL PRIMARY KEY,
  active TINYINT(1) NOT NULL DEFAULT 1,
  created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;
CREATE TABLE IF NOT EXISTS bank_mobile_deposit_cooldowns (
  user_id INT NOT NULL PRIMARY KEY,
  last_deposit_at TIMESTAMP NULL DEFAULT NULL,
  CONSTRAINT bank_mobile_cooldown_user_fk FOREIGN KEY(user_id) REFERENCES users(id) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;
INSERT INTO rp_bank_atm_items(base_item_id,active) VALUES(9732160,1) ON DUPLICATE KEY UPDATE active=VALUES(active);
UPDATE items_base SET interaction_type='rp_atm' WHERE id IN (SELECT base_item_id FROM rp_bank_atm_items WHERE active=1);

CREATE TABLE IF NOT EXISTS rp_bank_computer_items (
  item_id INT NOT NULL PRIMARY KEY,
  active TINYINT(1) NOT NULL DEFAULT 1,
  created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;
INSERT INTO rp_bank_computer_items(item_id,active) VALUES(32010,1),(32012,1) ON DUPLICATE KEY UPDATE active=VALUES(active);
UPDATE items_base b JOIN items i ON i.item_id=b.id JOIN rp_bank_computer_items c ON c.item_id=i.id AND c.active=1 SET b.interaction_type='rp_bank_computer';

UPDATE job_ranks SET name='bank_intern',display_name='Stagiaire',level=0,is_manager=0,salary=45,permissions='["bank.account.view"]' WHERE id=24 AND job_id=7;
UPDATE job_ranks SET name='bank_advisor',display_name='Conseiller bancaire',level=1,is_manager=0,salary=55,permissions='["bank.account.view"]' WHERE id=25 AND job_id=7;
INSERT INTO job_ranks(job_id,name,display_name,level,is_manager,salary,permissions,active) VALUES
(7,'banker','Banquier',2,0,65,'["bank.account.view","bank.counter.deposit","bank.counter.withdraw"]',1),
(7,'bank_senior_advisor','Conseiller senior',3,0,75,'["bank.account.view","bank.counter.deposit","bank.counter.withdraw","bank.transfer"]',1),
(7,'bank_supervisor','Responsable bancaire',4,0,85,'["bank.account.view","bank.counter.deposit","bank.counter.withdraw","bank.transfer"]',1)
ON DUPLICATE KEY UPDATE display_name=VALUES(display_name),level=VALUES(level),salary=VALUES(salary),permissions=VALUES(permissions),active=1;
UPDATE job_ranks SET name='bank_deputy_manager',display_name='Directeur adjoint',level=5,is_manager=1,salary=105,permissions='["bank.account.view","bank.counter.deposit","bank.counter.withdraw","bank.account.manage","bank.transfer","job.hire","job.promote","job.demote"]' WHERE id=26 AND job_id=7;
UPDATE job_ranks SET display_name='Directeur de banque',level=6,is_manager=1,salary=130,permissions='["bank.account.view","bank.counter.deposit","bank.counter.withdraw","bank.account.manage","bank.transfer","job.hire","job.fire","job.promote","job.demote","job.manage_schedule"]' WHERE id=27 AND job_id=7;
INSERT INTO jobs_rooms(job_id,rooms) SELECT 7,'-1' WHERE NOT EXISTS(SELECT 1 FROM jobs_rooms WHERE job_id=7);
