/* French presentation layer for legacy CMS templates.
   It deliberately changes only visible text and placeholders, never routes,
   form names, database values or Ajax requests. */
(function () {
    'use strict';

    var textMap = {
        'Inicio': 'Accueil',
        'Mi perfil': 'Mon profil',
        'Ajustes de cuenta': 'Parametres du compte',
        'Cerrar sesion': 'Deconnexion',
        'Equipo Staff': 'Equipe staff',
        'Clasificacion de usuarios': 'Classement des citoyens',
        'Mapa de ciudad': 'Carte de la ville',
        'Usuarios online': 'Citoyens en ligne',
        'Buscar usuarios': 'Rechercher un citoyen',
        'Ver Corporaciones': 'Voir les entreprises',
        'Ver Bandas': 'Voir les gangs',
        'Clasificaciones': 'Classements',
        'Estadisticas de Juego': 'Statistiques de jeu',
        'Cartera': 'Portefeuille',
        'Banco': 'Banque',
        'Platinos': 'Platines',
        'Nivel': 'Niveau',
        'Ciudadanos mas ricos': 'Citoyens les plus riches',
        'Top 3 mas Ricos': 'Top 3 des plus riches',
        'Usuarios Conectados': 'Citoyens connectes',
        'Buscar Usuarios': 'Rechercher des citoyens',
        'Usuarios al azar': 'Citoyens au hasard',
        'Usuarios encontrados': 'Citoyens trouves',
        'Cambiar Contrasena': 'Modifier le mot de passe',
        'Contrasena actual': 'Mot de passe actuel',
        'Nueva Contrasena': 'Nouveau mot de passe',
        'Confirmar contrasena': 'Confirmer le mot de passe',
        'Repite la Contrasena': 'Confirmer le mot de passe',
        'Vincular Cuenta a Facebook': 'Lier un compte Facebook',
        'Vincular': 'Lier',
        'Empleados': 'Employes',
        'Ver Negocio': "Voir l'entreprise",
        'Buscar Banda': 'Rechercher un gang',
        'Mas dinero': 'Plus riches',
        'Riqueza': 'Richesse',
        'Miembros': 'Membres',
        'Ver': 'Voir',
        'Desarrollador / Dueño': 'Developpeur / Proprietaire',
        'Desarrollador': 'Developpeur',
        'Dueño': 'Fondateur',
        'Administrador': 'Administrateur',
        'Moderador': 'Moderateur',
        'Ayudante': 'Assistant',
        '¿Necesitas ayuda?': "Besoin d'aide ?",
        'Reglas': 'Regles',
        'Discord Oficial': 'Discord officiel',
        'Facebook Oficial': 'Facebook officiel',
        'ENTRAR A ROLEAR!': 'ENTRER DANS LE JEU',
        '¡ENTRAR A ROLEAR!': 'ENTRER DANS LE JEU'
    };

    var placeholderMap = {
        'Buscar': 'Rechercher',
        'Escribir nombre a buscar': 'Saisir un pseudo',
        'Escribe el nombre del usuario para buscar...': 'Saisis un pseudo pour lancer la recherche...',
        'Nueva contraseña': 'Nouveau mot de passe',
        'Contraseña actual': 'Mot de passe actuel',
        'Repite la contraseña': 'Confirmer le mot de passe'
    };

    function key(value) {
        return value.replace(/\s+/g, ' ').trim()
            .normalize('NFD').replace(/[\u0300-\u036f]/g, '');
    }

    function translateTextNode(node) {
        if (!node.nodeValue || !node.parentElement) return;
        var parent = node.parentElement;
        if (/^(SCRIPT|STYLE|TEXTAREA|CODE|PRE)$/i.test(parent.tagName) || parent.closest('[data-no-translate]')) return;
        var original = node.nodeValue;
        var replacement = textMap[key(original)];
        if (replacement) node.nodeValue = original.replace(original.trim(), replacement);
    }

    function translate(root) {
        if (!root) return;
        var walker = document.createTreeWalker(root, NodeFilter.SHOW_TEXT);
        var node;
        while ((node = walker.nextNode())) translateTextNode(node);

        var fields = (root.matches && root.matches('[placeholder]') ? [root] : []);
        Array.prototype.push.apply(fields, root.querySelectorAll ? root.querySelectorAll('[placeholder]') : []);
        fields.forEach(function (field) {
            var replacement = placeholderMap[key(field.getAttribute('placeholder') || '')];
            if (replacement) field.setAttribute('placeholder', replacement);
        });
    }

    function start() {
        translate(document.body);
        var observer = new MutationObserver(function (changes) {
            changes.forEach(function (change) {
                change.addedNodes.forEach(function (node) {
                    if (node.nodeType === Node.TEXT_NODE) translateTextNode(node);
                    else if (node.nodeType === Node.ELEMENT_NODE) translate(node);
                });
            });
        });
        observer.observe(document.body, { childList: true, subtree: true });
    }

    if (document.readyState === 'loading') document.addEventListener('DOMContentLoaded', start);
    else start();
}());
