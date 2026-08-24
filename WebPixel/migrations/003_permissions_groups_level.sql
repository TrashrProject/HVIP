-- Compatibilite entre les rangs historiques WavePlus et le CMS ParadiseWave.
-- Le CMS trie et filtre les rangs avec permissions_groups.level.

ALTER TABLE `permissions_groups`
    ADD COLUMN IF NOT EXISTS `level` INT UNSIGNED NOT NULL DEFAULT 1 AFTER `id`;

UPDATE `permissions_groups`
SET `level` = `id`
WHERE `level` = 1 AND `id` <> 1;

