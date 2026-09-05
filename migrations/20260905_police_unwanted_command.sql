-- ParadiseRP: expose :wanted et :unwanted sur le même handler Wanted.
-- :wanted affiche la liste des personnes recherchées.
-- :unwanted <pseudo> retire une personne de la liste (police en service uniquement).

INSERT INTO `emulator_texts` (`key`, `value`)
VALUES
('commands.cmd_wanted_list.keys', 'wanted;unwanted'),
('commands.description.cmd_wanted_list', ':wanted - Voir les personnes recherchées. | :unwanted <pseudo> - Retirer une personne de la liste des recherchés.')
ON DUPLICATE KEY UPDATE `value` = VALUES(`value`);
