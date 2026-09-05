-- ParadiseRP - niveaux de recherche manuelle pour :rechercher
-- Rejouable : n'insère une ligne que si le niveau correspondant n'existe pas déjà.

INSERT INTO crimes (name, stars, police_alert, instant_alert, is_auto_charge, notes)
SELECT 'Recherche manuelle 1', 1, TRUE, FALSE, FALSE, 'Niveau de recherche manuel attribué par la Police nationale'
WHERE NOT EXISTS (SELECT 1 FROM crimes WHERE name = 'Recherche manuelle 1');

INSERT INTO crimes (name, stars, police_alert, instant_alert, is_auto_charge, notes)
SELECT 'Recherche manuelle 2', 2, TRUE, FALSE, FALSE, 'Niveau de recherche manuel attribué par la Police nationale'
WHERE NOT EXISTS (SELECT 1 FROM crimes WHERE name = 'Recherche manuelle 2');

INSERT INTO crimes (name, stars, police_alert, instant_alert, is_auto_charge, notes)
SELECT 'Recherche manuelle 3', 3, TRUE, FALSE, FALSE, 'Niveau de recherche manuel attribué par la Police nationale'
WHERE NOT EXISTS (SELECT 1 FROM crimes WHERE name = 'Recherche manuelle 3');

INSERT INTO crimes (name, stars, police_alert, instant_alert, is_auto_charge, notes)
SELECT 'Recherche manuelle 4', 4, TRUE, FALSE, FALSE, 'Niveau de recherche manuel attribué par la Police nationale'
WHERE NOT EXISTS (SELECT 1 FROM crimes WHERE name = 'Recherche manuelle 4');

INSERT INTO crimes (name, stars, police_alert, instant_alert, is_auto_charge, notes)
SELECT 'Recherche manuelle 5', 5, TRUE, FALSE, FALSE, 'Niveau de recherche manuel attribué par la Police nationale'
WHERE NOT EXISTS (SELECT 1 FROM crimes WHERE name = 'Recherche manuelle 5');
