INSERT INTO emulator_settings (`key`, `value`) VALUES
  ('commands.cmd_tazor.keys', 'taser;tazor;taze;tase'),
  ('commands.cmd_release.keys', 'liberer;release'),
  ('commands.cmd_detaser.keys', 'detaser'),
  ('commands.cmd_prison.keys', 'prison'),
  ('commands.cmd_stopescort.keys', 'stopescort'),
  ('features.taxi.seconds_delay', '10'),
  ('features.taxi.effectid', '21'),
  ('features.police.tazor.duration_seconds', '40'),
  ('nahabbo.features.jail.roomid', '132'),
  ('features.prison.release.roomid', '133')
ON DUPLICATE KEY UPDATE `value` = VALUES(`value`);
