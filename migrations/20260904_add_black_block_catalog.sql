SET NAMES utf8mb4;

-- Add the real black Builders Club cube to the dedicated island construction kit.
-- Idempotent: does nothing if this furniture id is already offered somewhere in the catalog.
INSERT INTO catalog_items (
    item_ids,
    page_id,
    catalog_name,
    cost_credits,
    cost_points,
    points_type,
    amount,
    limited_stack,
    limited_sells,
    order_number,
    offer_id,
    song_id,
    extradata,
    have_offer,
    club_only
)
SELECT
    '5480',
    9967301,
    'Grand Cube noir',
    3,
    0,
    0,
    1,
    0,
    0,
    COALESCE((SELECT MAX(ci2.order_number) + 1 FROM catalog_items ci2 WHERE ci2.page_id = 9967301), 1),
    -1,
    0,
    '',
    '1',
    '0'
WHERE NOT EXISTS (
    SELECT 1
    FROM catalog_items ci
    WHERE CAST(SUBSTRING_INDEX(SUBSTRING_INDEX(ci.item_ids, ',', 1), ':', 1) AS UNSIGNED) = 5480
);

-- Clean up the mojibake labels of the existing coloured-orb offers without moving them.
UPDATE catalog_items SET catalog_name='Lumiere Orbe blanche'     WHERE id=36830 AND item_ids='996661605';
UPDATE catalog_items SET catalog_name='Lumiere Orbe bleue'       WHERE id=36831 AND item_ids='996661606';
UPDATE catalog_items SET catalog_name='Lumiere Orbe marron fonce' WHERE id=36832 AND item_ids='996661607';
UPDATE catalog_items SET catalog_name='Lumiere Orbe marron pale'  WHERE id=36833 AND item_ids='996661608';
UPDATE catalog_items SET catalog_name='Lumiere Orbe orange'       WHERE id=36834 AND item_ids='996661609';
UPDATE catalog_items SET catalog_name='Lumiere Orbe rose'         WHERE id=36835 AND item_ids='996661610';
UPDATE catalog_items SET catalog_name='Lumiere Orbe rouge'        WHERE id=36836 AND item_ids='996661611';
UPDATE catalog_items SET catalog_name='Lumiere Orbe turquoise'    WHERE id=36837 AND item_ids='996661612';
UPDATE catalog_items SET catalog_name='Lumiere Orbe verte'        WHERE id=36838 AND item_ids='996661613';
UPDATE catalog_items SET catalog_name='Lumiere Orbe violette'     WHERE id=36839 AND item_ids='996661614';
