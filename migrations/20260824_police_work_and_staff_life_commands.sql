UPDATE job_ranks AS rank_entry
INNER JOIN jobs AS job_entry ON job_entry.id = rank_entry.job_id
SET rank_entry.permissions = JSON_ARRAY_APPEND(rank_entry.permissions, '$', 'police.taze')
WHERE job_entry.name = 'police'
  AND rank_entry.level >= 1
  AND JSON_CONTAINS(rank_entry.permissions, JSON_QUOTE('police.taze')) = 0;

INSERT INTO emulator_texts (`key`, `value`) VALUES
('commands.description.cmd_tazor', ':taser <pseudo> - Immobilise temporairement un joueur.'),
('commands.description.cmd_detaser', ':detaser <pseudo> - Retire l''effet du taser.'),
('commands.description.cmd_handcuff', ':menotter <pseudo> - Menotte un joueur tase.'),
('commands.description.cmd_unhandcuff', ':demenotter <pseudo> - Retire les menottes.'),
('commands.description.cmd_start_work', ':travailler - Commence votre service.'),
('commands.description.cmd_stop_work', ':arreter - Termine votre service.'),
('commands.description.cmd_release', ':liberer <pseudo> - Libere un joueur emprisonne.'),
('commands.description.cmd_prison', ':prison <pseudo> <minutes> <raison> - Emprisonne un joueur.'),
('commands.description.cmd_escort', ':escort <pseudo> - Escorte un joueur menotte.'),
('commands.description.cmd_stopescort', ':stopescort <pseudo> - Arrete une escorte.')
ON DUPLICATE KEY UPDATE `value` = VALUES(`value`);
