-- ParadiseRP Phase 2 rollback
-- WARNING: this removes ONLY Phase 2 additive tables and therefore deletes
-- Character/Document data created after the migration. Legacy users/play_stats
-- are never modified by this rollback.

DROP TABLE IF EXISTS `rp_ui_events`;
DROP TABLE IF EXISTS `rp_document_shares`;
DROP TABLE IF EXISTS `rp_player_documents`;
DROP TABLE IF EXISTS `rp_document_types`;
DROP TABLE IF EXISTS `rp_characters`;
