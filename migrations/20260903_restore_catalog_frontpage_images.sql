-- Restore the four Nitro front-page promotions with assets that exist in the
-- active ParadiseRP c_images library. Safe to run more than once.
UPDATE catalog_featured_pages
SET image = CASE slot_id
    WHEN 1 THEN 'catalogue/feature_cata_hort_easter18.png'
    WHEN 2 THEN 'catalogue/feature_cata_hort_HC.png'
    WHEN 3 THEN 'catalogue/feature_cata_hort_puraiced16.png'
    WHEN 4 THEN 'catalogue/feature_cata_vert_val18bun1.png'
    ELSE image
END
WHERE slot_id IN (1, 2, 3, 4)
  AND image IN (
      'catalogue/feature_cata/feature_cata_hort_pets.png',
      'catalogue/feature_cata/feature_cata_hort_clothes.png',
      'catalogue/feature_cata/feature_cata_hort_habbo20_bun2.png',
      'catalogue/feature_cata/feature_cata_vert_habbo20_roselinedball.png'
  );
