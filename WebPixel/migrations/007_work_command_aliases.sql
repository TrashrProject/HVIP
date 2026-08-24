UPDATE `permissions_commands`
SET `keys` = 'stopwork;arreter'
WHERE `command` = 'command_rp_stop_work';

UPDATE `permissions_commands`
SET `keys` = ''
WHERE `command` = 'command_rp_police_service';
