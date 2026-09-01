package com.eu.habbo.habbohotel.commands;

import java.util.Arrays;
import java.util.HashMap;
import java.util.HashSet;
import java.util.List;
import java.util.Locale;
import java.util.Map;
import java.util.Set;
import java.util.function.Predicate;

/** Central documentation metadata used by the in-game command centre. */
public final class CommandDocumentation {
    private static final String GENERAL = "Général";
    private static final String ROLEPLAY = "RP";
    private static final String ROOM = "Appartement";
    private static final String CHARACTER = "Personnage";
    private static final String STAFF = "Staff";

    private static final List<Rule> RULES = Arrays.asList(
            prefix("cmd_ems_", ROLEPLAY, "EMS", "EMS en service"),
            permissions(ROLEPLAY, "EMS", "Selon la commande : patient ou EMS en service",
                    "cmd_ems", "cmd_cancel_ems", "cmd_accept_ems", "cmd_close_ems", "cmd_bandage",
                    "cmd_stabilize", "cmd_revive", "cmd_transport_hospital"),
            permissions(ROLEPLAY, "Police", "Police en service",
                    "cmd_tazor", "cmd_detaser", "cmd_handcuff", "cmd_unhandcuff", "cmd_escort",
                    "cmd_stopescort", "cmd_prison", "cmd_release", "cmd_charge", "cmd_pardon",
                    "cmd_wanted_list", "cmd_911", "cmd_arrest"),
            permissions(ROLEPLAY, "Banque", "Compte bancaire RP",
                    "cmd_openaccount", "cmd_balance", "cmd_give", "cmd_transactions", "cmd_deposit",
                    "cmd_withdraw"),
            permissions(ROLEPLAY, "Taxi", "Tous les joueurs", "cmd_taxi"),
            prefix("cmd_org_", ROLEPLAY, "Organisations", "Membre d'une organisation selon l'action"),
            permissions(ROLEPLAY, "Entreprises et métiers", "Selon le métier et le grade",
                    "cmd_job", "cmd_start_work", "cmd_stop_work", "cmd_quit_job", "cmd_hire", "cmd_fire",
                    "cmd_promote", "cmd_demote", "cmd_send_home", "cmd_apply", "cmd_sell_rpitem",
                    "cmd_offer_rpitem", "cmd_accept_offer", "cmd_decline_offer", "cmd_clear_offer"),
            permissions(ROLEPLAY, "Combat", "Joueurs RP",
                    "cmd_rob", "cmd_hit", "cmd_spit", "cmd_shoot", "cmd_equip", "cmd_unequip",
                    "cmd_passive", "cmd_combat_stats", "cmd_target_lock"),
            permissions(ROLEPLAY, "Économie", "Joueurs RP", "cmd_bucks", "cmd_sell_item"),
            permissions(ROLEPLAY, "Outils RP", "Joueurs RP", "cmd_open_macro", "cmd_hotrooms"),

            permissions(ROOM, "Gestion", "Propriétaire ou détenteur des droits selon l'action",
                    "cmd_pickall", "cmd_setmax", "cmd_setspeed", "cmd_setpublic", "cmd_buildheight",
                    "cmd_closedice", "cmd_disable_effects", "cmd_diagonal", "cmd_coords", "cmd_sellroom",
                    "cmd_buyroom", "cmd_bundle", "cmd_set", "cmd_setrotation", "cmd_setstate"),
            permissionContains(ROOM, "Gestion", "Droits d'appartement ou staff selon l'action", "room"),
            permissions(ROOM, "Déplacement", "Selon les droits disponibles", "cmd_teleport"),

            permissions(CHARACTER, "Actions", "Tous les joueurs",
                    "cmd_brb", "cmd_hoverboard", "cmd_hug", "cmd_kiss", "cmd_lay", "cmd_sit",
                    "cmd_sitdown", "cmd_stand", "cmd_slime", "cmd_moonwalk", "cmd_fastwalk"),
            permissions(CHARACTER, "Apparence", "Selon les permissions disponibles",
                    "cmd_changename", "cmd_faceless", "cmd_mimic", "cmd_transform", "cmd_enable",
                    "cmd_hand_item", "cmd_chatcolor"),
            permissions(CHARACTER, "Informations", "Tous les joueurs", "cmd_pet_info"),

            permissions(STAFF, "Sanctions", "Staff autorisé",
                    "cmd_ban", "cmd_super_ban", "cmd_ip_ban", "cmd_machine_ban", "cmd_unban", "cmd_mute",
                    "cmd_unmute", "cmd_kickall", "cmd_disconnect", "cmd_softkick"),
            permissions(STAFF, "Modération", "Staff autorisé",
                    "cmd_alert", "cmd_blockalert", "cmd_filterword", "cmd_freeze", "cmd_freeze_bots",
                    "cmd_mute_bots", "cmd_mute_pets", "cmd_ha", "cmd_hal", "cmd_roomalert",
                    "cmd_staffalert", "cmd_event"),
            permissions(STAFF, "Gestion des joueurs", "Staff autorisé",
                    "cmd_badge", "cmd_take_badge", "cmd_give_rank", "cmd_credits", "cmd_duckets",
                    "cmd_points", "cmd_gift", "cmd_massbadge", "cmd_masscredits", "cmd_massduckets",
                    "cmd_massgift", "cmd_masspoints", "cmd_empty", "cmd_empty_bots", "cmd_empty_pets",
                    "cmd_userinfo", "cmd_allow_trading", "cmd_superhire", "cmd_staff_kill",
                    "cmd_staff_revive", "cmd_set_stats"),
            permissions(STAFF, "Téléportation", "Staff autorisé",
                    "cmd_stalk", "cmd_summon", "cmd_summonrank", "cmd_superpull", "cmd_pull", "cmd_push",
                    "cmd_goto_room", "cmd_send_room"),
            permissions(STAFF, "Actions RP", "Staff autorisé",
                    "cmd_staff_arrest", "cmd_staff_release", "cmd_staff_hit", "cmd_room_heal",
                    "cmd_room_release", "cmd_global_heal", "cmd_super_heal", "cmd_make_territory"),
            permissionPrefix(STAFF, "Technique", "Administration technique", "cmd_update_"),
            permissions(STAFF, "Technique", "Administration technique",
                    "cmd_shutdown", "cmd_plugins", "cmd_reload_room", "cmd_reload_farm", "cmd_unload",
                    "cmd_furnidata", "cmd_connect_camera", "cmd_control", "cmd_invisible", "cmd_multi",
                    "cmd_happyhour", "cmd_set_poll", "cmd_promote_offer", "acc_debug", "cmd_test"),
            permissions(STAFF, "Communication", "Staff autorisé",
                    "cmd_say", "cmd_say_all", "cmd_shout", "cmd_shout_all", "cmd_talk", "cmd_welcome",
                    "cmd_staffonline"),

            permissions(GENERAL, "Aide", "Tous les joueurs", "cmd_commands", "cmd_explain", "cmd_about"),
            permissions(GENERAL, "Informations", "Tous les joueurs",
                    "cmd_calendar", "cmd_ping", "cmd_credits", "cmd_redeem", "cmd_word_quiz", "cmd_bots")
    );

    private static final Map<String, String> DESCRIPTIONS = new HashMap<>();

    static {
        describe("cmd_commands", "Ouvre le Centre de commandes et affiche les commandes auxquelles vous avez accès.");
        describe("cmd_explain", "Affiche l'utilisation, les alias et les informations d'une commande précise.");
        describe("cmd_about", "Affiche les informations techniques et la version de l'émulateur.");
        describe("cmd_911", "Envoie un appel d'urgence aux policiers actuellement en service.");
        describe("cmd_taxi", "Appelle un taxi vers l'appartement indiqué après 10 secondes, pour un coût de 3 crédits.");
        describe("cmd_ems_heal", "Permet à un membre EMS en service de soigner un joueur conscient situé à proximité.");
        describe("cmd_ems_revive", "Permet à un membre EMS en service de réanimer un joueur inconscient situé à proximité.");
        describe("cmd_ems_diagnostic", "Permet à un membre EMS en service de consulter la santé et l'état d'un joueur proche.");
        describe("cmd_ems_carry", "Permet à un membre EMS en service de transporter un patient situé à proximité.");
        describe("cmd_ems_drop", "Arrête le transport du patient actuellement pris en charge.");
        describe("cmd_ems", "Envoie une demande d'intervention aux membres EMS actuellement en service.");
        describe("cmd_cancel_ems", "Annule votre demande d'intervention EMS encore ouverte.");
        describe("cmd_ems_calls", "Affiche aux membres EMS en service la liste des demandes médicales ouvertes.");
        describe("cmd_accept_ems", "Permet à un membre EMS en service de prendre en charge l'appel médical indiqué.");
        describe("cmd_close_ems", "Permet à un membre EMS de clôturer l'intervention médicale en cours.");
        describe("cmd_bandage", "Pose un bandage sur un patient proche afin de traiter ses blessures.");
        describe("cmd_stabilize", "Stabilise un patient inconscient situé à proximité avant son transport.");
        describe("cmd_revive", "Réanime un patient inconscient selon les règles du service médical.");
        describe("cmd_transport_hospital", "Transporte le patient pris en charge vers l'hôpital.");
        describe("cmd_tazor", "Permet à un policier en service d'immobiliser pendant 40 secondes un joueur situé à deux cases maximum.");
        describe("cmd_detaser", "Retire immédiatement l'effet du taser appliqué à un joueur.");
        describe("cmd_handcuff", "Permet à un policier en service de menotter un joueur neutralisé situé à proximité.");
        describe("cmd_unhandcuff", "Permet à un policier en service de retirer les menottes d'un joueur.");
        describe("cmd_escort", "Permet à un policier en service d'escorter un joueur menotté placé sur une case voisine.");
        describe("cmd_stopescort", "Arrête l'escorte policière actuellement en cours.");
        describe("cmd_prison", "Envoie un joueur de la salle en prison pendant 1 à 60 minutes avec une raison.");
        describe("cmd_release", "Libère un joueur actuellement emprisonné.");
        describe("cmd_charge", "Ajoute le délit indiqué au dossier d'un joueur ciblé.");
        describe("cmd_pardon", "Retire les avis de recherche et accusations applicables au joueur indiqué.");
        describe("cmd_wanted_list", "Affiche la liste des joueurs actuellement recherchés par la police.");
        describe("cmd_start_work", "Commence le service dans votre métier actuel et affiche votre fonction RP.");
        describe("cmd_stop_work", "Termine votre service dans votre métier actuel.");
        describe("cmd_job", "Affiche votre métier, votre grade et votre état de service actuels.");
        describe("cmd_quit_job", "Quitte définitivement votre métier actuel et retire votre grade professionnel.");
        describe("cmd_hire", "Permet à un responsable d'entreprise de recruter le joueur indiqué.");
        describe("cmd_fire", "Permet à un responsable d'entreprise de licencier un employé.");
        describe("cmd_promote", "Permet à un responsable d'entreprise de promouvoir un employé au grade suivant.");
        describe("cmd_demote", "Permet à un responsable d'entreprise de rétrograder un employé d'un grade.");
        describe("cmd_send_home", "Renvoie temporairement un employé chez lui pour la durée indiquée.");
        describe("cmd_apply", "Utilise l'objet indiqué depuis votre inventaire RP.");
        describe("cmd_offer_rpitem", "Propose à un joueur l'achat d'un objet RP de votre inventaire.");
        describe("cmd_accept_offer", "Accepte une offre RP en attente à l'aide de son code.");
        describe("cmd_decline_offer", "Refuse une offre RP en attente à l'aide de son code.");
        describe("cmd_clear_offer", "Annule toutes vos offres RP actuellement en attente.");
        describe("cmd_openaccount", "Ouvre votre compte bancaire RP.");
        describe("cmd_balance", "Affiche le solde de votre compte bancaire RP.");
        describe("cmd_give", "Effectue un virement depuis votre compte bancaire vers le joueur indiqué.");
        describe("cmd_transactions", "Affiche l'historique récent des opérations de votre compte bancaire.");
        describe("cmd_deposit", "Dépose le montant indiqué sur votre compte lorsque vous êtes près d'un distributeur.");
        describe("cmd_withdraw", "Retire le montant indiqué de votre compte lorsque vous êtes près d'un distributeur.");
        describe("cmd_rob", "Tente de voler le montant indiqué au joueur ciblé selon les règles de combat RP.");
        describe("cmd_hit", "Frappe le joueur ciblé selon la portée et le délai du système de combat.");
        describe("cmd_spit", "Effectue une action RP permettant de cracher sur le joueur ciblé.");
        describe("cmd_shoot", "Tire sur le joueur ciblé avec l'arme actuellement équipée.");
        describe("cmd_equip", "Équipe une arme ou une protection disponible dans votre inventaire RP.");
        describe("cmd_unequip", "Range l'arme ou la protection actuellement équipée.");
        describe("cmd_passive", "Active ou désactive le mode passif qui bloque les actions de combat RP.");
        describe("cmd_combat_stats", "Affiche vos statistiques de combat ou celles du joueur indiqué.");
        describe("cmd_target_lock", "Verrouille ou libère la cible RP actuellement sélectionnée.");
        describe("cmd_bucks", "Affiche le montant d'argent RP actuellement disponible sur votre personnage.");
        describe("cmd_sell_item", "Vend la quantité indiquée d'un objet éligible de votre inventaire RP.");
        describe("cmd_open_macro", "Ouvre l'interface des raccourcis et actions rapides RP.");
        describe("cmd_org_create", "Crée une organisation RP avec les informations demandées.");
        describe("cmd_org_join", "Rejoint l'organisation RP correspondant à votre invitation en attente.");
        describe("cmd_org_leave", "Quitte votre organisation RP actuelle.");
        describe("cmd_org_invite", "Invite le joueur indiqué à rejoindre votre organisation RP.");
        describe("cmd_org_kick", "Exclut le membre indiqué de votre organisation RP.");
        describe("cmd_org_rename", "Renomme votre organisation RP lorsque votre rôle vous y autorise.");
        describe("cmd_org_delete", "Dissout définitivement l'organisation RP que vous dirigez.");
        describe("cmd_org_rankup", "Promeut le membre indiqué au rôle supérieur de l'organisation.");
        describe("cmd_org_rankdown", "Rétrograde le membre indiqué au rôle inférieur de l'organisation.");
        describe("cmd_superhire", "Permet à un staff de niveau 5 à 9 d'attribuer directement un métier et un grade à un joueur.");
        describe("cmd_staff_kill", "Permet à un staff autorisé de rendre immédiatement inconscient le joueur indiqué.");
        describe("cmd_staff_revive", "Permet à un staff autorisé de réanimer immédiatement le joueur indiqué.");
        describe("cmd_ping", "Vérifie que le serveur répond et affiche le délai de réponse.");
        describe("cmd_hotrooms", "Affiche les appartements qui comptent actuellement le plus de joueurs.");
        describe("cmd_pickall", "Ramasse tous les meubles de l'appartement que vous possédez.");
        describe("cmd_setmax", "Modifie le nombre maximal de joueurs autorisés dans l'appartement.");
        describe("cmd_setspeed", "Modifie la vitesse de déplacement des unités dans l'appartement.");
        describe("cmd_buildheight", "Définit la hauteur de construction utilisée pour placer les meubles.");
        describe("cmd_coords", "Affiche les coordonnées de votre personnage dans l'appartement.");
        describe("cmd_sellroom", "Met en vente l'appartement actuel au prix indiqué.");
        describe("cmd_buyroom", "Achète l'appartement actuel lorsqu'il est proposé à la vente.");
        describe("cmd_teleport", "Active ou désactive la téléportation par clic dans l'appartement.");
        describe("cmd_brb", "Indique votre absence ou votre retour aux joueurs présents.");
        describe("cmd_mimic", "Copie temporairement l'apparence du joueur indiqué.");
        describe("cmd_moonwalk", "Active ou désactive la marche arrière de votre personnage.");
        describe("cmd_fastwalk", "Active ou désactive la marche rapide de votre personnage.");
        describe("cmd_changename", "Ouvre ou autorise la procédure de changement de pseudo.");
        describe("cmd_hand_item", "Place l'objet à porter indiqué dans la main de votre personnage.");
        describe("cmd_hug", "Effectue une interaction permettant de serrer le joueur indiqué dans ses bras.");
        describe("cmd_kiss", "Effectue une interaction permettant d'embrasser le joueur indiqué.");
        describe("cmd_ban", "Bannit le joueur indiqué pour la durée autorisée par votre rang.");
        describe("cmd_unban", "Retire le bannissement associé au joueur indiqué.");
        describe("cmd_mute", "Empêche temporairement le joueur indiqué de parler.");
        describe("cmd_unmute", "Rend la parole au joueur indiqué.");
        describe("cmd_alert", "Envoie un message d'alerte privé au joueur indiqué.");
        describe("cmd_ha", "Envoie une alerte à tous les joueurs connectés.");
        describe("cmd_roomalert", "Envoie une alerte à tous les joueurs de l'appartement actuel.");
        describe("cmd_filterword", "Ajoute un mot et son remplacement éventuel au filtre de discussion.");
        describe("cmd_userinfo", "Affiche les informations de modération du joueur indiqué.");
        describe("cmd_stalk", "Rejoint l'appartement dans lequel se trouve le joueur indiqué.");
        describe("cmd_summon", "Téléporte le joueur indiqué jusqu'à votre appartement actuel.");
        describe("cmd_shutdown", "Arrête proprement l'émulateur après confirmation.");
        describe("cmd_plugins", "Affiche les extensions actuellement chargées par l'émulateur.");
        describe("cmd_make_territory", "Transforme l'appartement actuel en territoire RP pour l'organisation autorisée.");
        describe("cmd_room_heal", "Soigne les joueurs présents dans l'appartement selon les droits staff accordés.");
        describe("cmd_room_release", "Libère les joueurs concernés dans l'appartement actuel.");
        describe("cmd_global_heal", "Soigne tous les joueurs concernés sur l'ensemble de l'hôtel.");
        describe("cmd_super_heal", "Soigne immédiatement le joueur indiqué sans les restrictions médicales habituelles.");
        describe("cmd_staff_arrest", "Place immédiatement le joueur indiqué en état d'arrestation.");
        describe("cmd_staff_release", "Libère immédiatement le joueur indiqué d'une arrestation RP.");
        describe("cmd_staff_hit", "Applique au joueur indiqué une attaque RP avec les privilèges staff.");
        describe("cmd_set_stats", "Modifie la statistique RP indiquée pour le joueur ciblé.");
        describe("cmd_send_room", "Envoie le joueur indiqué dans l'appartement précisé.");
        describe("cmd_goto_room", "Rejoint directement l'appartement indiqué.");
        describe("cmd_reload_farm", "Recharge la configuration et les données du système de ferme RP.");
        describe("cmd_update_item_marketplace", "Recharge la liste des objets pouvant être vendus sur le marché RP.");
    }

    private CommandDocumentation() {
    }

    public static Metadata resolve(String permission, String commandName) {
        String normalized = permission == null ? "" : permission.toLowerCase(Locale.ROOT);
        for (Rule rule : RULES) {
            if (rule.matches.test(normalized)) {
                return new Metadata(rule.category, rule.subcategory, rule.access,
                        description(normalized, commandName, rule.category, rule.subcategory));
            }
        }
        return new Metadata(GENERAL, "Autres", "Selon les permissions de votre rang",
                description(normalized, commandName, GENERAL, "Autres"));
    }

    public static String descriptionOverride(String permission) {
        return DESCRIPTIONS.get(permission == null ? "" : permission.toLowerCase(Locale.ROOT));
    }

    private static String description(String permission, String commandName, String category, String subcategory) {
        String value = DESCRIPTIONS.get(permission);
        if (value != null) {
            return value;
        }
        String label = commandName == null || commandName.isEmpty() ? "Cette commande" : commandName;
        if (STAFF.equals(category)) {
            return label + " exécute une action " + subcategory.toLowerCase(Locale.ROOT)
                    + " réservée aux membres du staff autorisés.";
        }
        if (ROOM.equals(category)) {
            return label + " exécute une action liée à l'appartement selon vos droits actuels.";
        }
        if (CHARACTER.equals(category)) {
            return label + " exécute une action liée à votre personnage.";
        }
        if (ROLEPLAY.equals(category)) {
            return label + " exécute une action du système RP « " + subcategory + " » selon vos permissions.";
        }
        return label + " exécute l'action indiquée selon les permissions de votre rang.";
    }

    private static void describe(String permission, String description) {
        DESCRIPTIONS.put(permission, description);
    }

    private static Rule prefix(String prefix, String category, String subcategory, String access) {
        return permissionPrefix(category, subcategory, access, prefix);
    }

    private static Rule permissionPrefix(String category, String subcategory, String access, String prefix) {
        return new Rule(value -> value.startsWith(prefix), category, subcategory, access);
    }

    private static Rule permissionContains(String category, String subcategory, String access, String fragment) {
        return new Rule(value -> value.contains(fragment), category, subcategory, access);
    }

    private static Rule permissions(String category, String subcategory, String access, String... permissions) {
        Set<String> values = new HashSet<>(Arrays.asList(permissions));
        return new Rule(values::contains, category, subcategory, access);
    }

    private static final class Rule {
        private final Predicate<String> matches;
        private final String category;
        private final String subcategory;
        private final String access;

        private Rule(Predicate<String> matches, String category, String subcategory, String access) {
            this.matches = matches;
            this.category = category;
            this.subcategory = subcategory;
            this.access = access;
        }
    }

    public static final class Metadata {
        public final String category;
        public final String subcategory;
        public final String access;
        public final String defaultDescription;

        private Metadata(String category, String subcategory, String access, String defaultDescription) {
            this.category = category;
            this.subcategory = subcategory;
            this.access = access;
            this.defaultDescription = defaultDescription;
        }
    }
}
