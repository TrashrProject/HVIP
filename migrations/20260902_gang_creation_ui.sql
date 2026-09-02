ALTER TABLE `rp_organizations`
  ADD COLUMN IF NOT EXISTS `status` varchar(64) NOT NULL DEFAULT '' AFTER `name`,
  ADD COLUMN IF NOT EXISTS `primary_color` int(11) NOT NULL DEFAULT 0 AFTER `admin_id`,
  ADD COLUMN IF NOT EXISTS `secondary_color` int(11) NOT NULL DEFAULT 0 AFTER `primary_color`,
  ADD COLUMN IF NOT EXISTS `badge` varchar(255) NOT NULL DEFAULT '' AFTER `secondary_color`;

ALTER TABLE `rp_organizations` CONVERT TO CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;
ALTER TABLE `rp_organization_members` CONVERT TO CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;
