-- ParadiseRP / WaveRP - adaptation of the useful HabboRPbr combat item set.
-- Uses the active rp_items + user_inventory architecture; no legacy inventory tables are imported.
-- Idempotent: safe to run more than once.
--
-- Weapon profile syntax consumed by WeaponProfile.java:
-- mode=ranged|melee;damage=min-max;range=N;durability=N;magazine=N
--
-- The melee bonuses and USP-S profile come directly from the legacy `armas` data.
-- The other firearm values are deliberately rebalanced for ParadiseRP while preserving the
-- legacy hierarchy (SMG/rifles < sniper) because the scanned SQL line truncates part of those
-- Portuguese descriptions.

INSERT INTO `rp_items`
(`id`,`name`,`interaction_type`,`permission`,`enable_id`,`extra_data`,`max`,`price`,`required_job_id`,`offer_job_id`,`required_handitem`,`crafter_organizations`) VALUES
(6101, 'Matraque',          'weapon',  NULL, 510, 'mode=melee;damage=2;range=1;durability=1',                         1,  120, NULL, NULL, 0, ''),
(6102, 'Batte',             'weapon',  NULL, 510, 'mode=melee;damage=3;range=1;durability=1',                         1,  140, NULL, NULL, 0, ''),
(6103, 'Épée',              'weapon',  NULL, 162, 'mode=melee;damage=3;range=1;durability=1',                         1,  150, NULL, NULL, 0, ''),
(6104, 'Katana',            'weapon',  NULL, 162, 'mode=melee;damage=4;range=1;durability=1',                         1,  170, NULL, NULL, 0, ''),
(6105, 'Hache rouge',       'weapon',  NULL,  -1, 'mode=melee;damage=4;range=1;durability=1',                         1,  180, NULL, NULL, 0, ''),
(6106, 'Hache',             'weapon',  NULL,  -1, 'mode=melee;damage=5;range=1;durability=1',                         1,  190, NULL, NULL, 0, ''),
(6107, 'Épée VIP',          'weapon',  NULL, 162, 'mode=melee;damage=6;range=1;durability=1',                         1,  200, NULL, NULL, 0, ''),
(6108, 'Hache VIP',         'weapon',  NULL,  -1, 'mode=melee;damage=7;range=1;durability=1',                         1,  220, NULL, NULL, 0, ''),
(6109, 'USP-S',             'weapon',  NULL, 164, 'mode=ranged;damage=10-15;range=6;durability=1;magazine=5',         1,  550, NULL, NULL, 0, ''),
(6110, 'AK47',              'weapon',  NULL, 583, 'mode=ranged;damage=15-22;range=8;durability=1;magazine=5',         1,  850, NULL, NULL, 0, ''),
(6111, 'Gilet pare-balles', 'shield',  NULL,   0, '100',                                                            1,  100, NULL, NULL, 0, ''),
(6112, 'Sniper',            'weapon',  NULL,  -1, 'mode=ranged;damage=25-40;range=12;durability=2;magazine=1',        1, 4500, NULL, NULL, 0, ''),
(6113, 'MP5',               'weapon',  NULL,  -1, 'mode=ranged;damage=17-23;range=6;durability=1;magazine=5',         1, 1200, NULL, NULL, 0, ''),
(6114, 'Kit de réparation', 'repair',  NULL,   0, 'armor=100',                                                      5,   40, NULL, NULL, 0, ''),
(6115, 'Canne à pêche',     'fishing', NULL,  -1, 'legacy_key=vara',                                                1,  100, NULL, NULL, 0, ''),
(6116, 'G36',               'weapon',  NULL, 584, 'mode=ranged;damage=18-25;range=8;durability=1;magazine=5',         1, 2000, NULL, NULL, 0, ''),
(6117, 'AKM',               'weapon',  NULL, 575, 'mode=ranged;damage=18-27;range=8;durability=1;magazine=5',         1, 2100, NULL, NULL, 0, ''),
(6118, 'Graine',            'seed',    NULL,   0, 'legacy_key=semente',                                             20,   25, NULL, NULL, 0, '')
ON DUPLICATE KEY UPDATE
  `name`=VALUES(`name`),
  `interaction_type`=VALUES(`interaction_type`),
  `permission`=VALUES(`permission`),
  `enable_id`=VALUES(`enable_id`),
  `extra_data`=VALUES(`extra_data`),
  `max`=VALUES(`max`),
  `price`=VALUES(`price`),
  `required_job_id`=VALUES(`required_job_id`),
  `offer_job_id`=VALUES(`offer_job_id`),
  `required_handitem`=VALUES(`required_handitem`),
  `crafter_organizations`=VALUES(`crafter_organizations`);
