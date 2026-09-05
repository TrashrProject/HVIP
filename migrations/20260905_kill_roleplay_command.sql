-- Reserve :kill for the ParadiseRP staff command that kills an RP avatar.
-- The emulator's native disconnection command remains available as :disconnect and :dc.
UPDATE emulator_texts
SET `value` = 'dc;disconnect'
WHERE `key` = 'commands.keys.cmd_disconnect';
