-- ParadiseRP legacy catalogue image cleanup.
-- Fixes catalogue page teaser values that point to missing c_images/catalogue assets.

UPDATE catalog_pages
SET page_teaser = 'black_base_teaser'
WHERE page_teaser IN ('image_teaser_atcg', 'image_teaser_atcg.gif');

UPDATE catalog_pages
SET page_headline = 'black_base_teaser'
WHERE page_headline IN ('image_teaser_atcg', 'image_teaser_atcg.gif');
