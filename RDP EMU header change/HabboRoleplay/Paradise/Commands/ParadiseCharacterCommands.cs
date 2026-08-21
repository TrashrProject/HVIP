using System;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.Rooms.Chat.Commands;
using Plus.HabboRoleplay.Paradise.Character;
using Plus.HabboRoleplay.Paradise.Documents;
using Plus.HabboRoleplay.Paradise.UI;

namespace Plus.HabboRoleplay.Paradise.Commands
{
    public static class ParadiseCommandBootstrap
    {
        public static void Register(CommandManager commands)
        {
            if (commands == null) return;

            // English + French aliases. ParadiseRP is francophone first, while
            // keeping the original command names for compatibility.
            commands.Register("profile", new ParadiseProfileCommand());
            commands.Register("profil", new ParadiseProfileCommand());
            commands.Register("id", new ParadiseIdCommand());
            commands.Register("identite", new ParadiseIdCommand());
            commands.Register("identité", new ParadiseIdCommand());
            commands.Register("documents", new ParadiseDocumentsCommand());
            commands.Register("showid", new ParadiseShowIdCommand());
            commands.Register("montrerid", new ParadiseShowIdCommand());
            commands.Register("license", new ParadiseLicenseCommand());
            commands.Register("licence", new ParadiseLicenseCommand());
            commands.Register("permis", new ParadiseLicenseCommand());
            commands.Register("showlicense", new ParadiseShowLicenseCommand());
            commands.Register("showlicence", new ParadiseShowLicenseCommand());
            commands.Register("montrerpermis", new ParadiseShowLicenseCommand());
        }
    }

    internal abstract class ParadiseCharacterCommandBase : IChatCommand
    {
        public string PermissionRequired { get { return String.Empty; } }
        public abstract string Parameters { get; }
        public abstract string Description { get; }
        public abstract void Execute(GameClient session, Room room, string[] parameters);

        protected static bool HasCharacter(GameClient session)
        {
            return session != null && session.GetHabbo() != null &&
                   CharacterService.GetOrLoad(session.GetHabbo().Id) != null;
        }
    }

    internal sealed class ParadiseProfileCommand : ParadiseCharacterCommandBase
    {
        public override string Parameters { get { return ""; } }
        public override string Description { get { return "Ouvre votre profil ParadiseRP."; } }

        public override void Execute(GameClient session, Room room, string[] parameters)
        {
            if (session == null || session.GetHabbo() == null) return;
            if (parameters.Length > 1 && !String.Equals(parameters[1], session.GetHabbo().Username, StringComparison.OrdinalIgnoreCase))
            {
                session.SendWhisper("Le profil public d'un autre joueur sera activé dans une phase dédiée.", 1);
                return;
            }
            ParadiseUiEventService.OpenProfile(session.GetHabbo().Id, HasCharacter(session) ? "overview" : "identity");
        }
    }

    internal sealed class ParadiseIdCommand : ParadiseCharacterCommandBase
    {
        public override string Parameters { get { return ""; } }
        public override string Description { get { return "Ouvre votre carte d'identité de Placid Island."; } }

        public override void Execute(GameClient session, Room room, string[] parameters)
        {
            if (session == null || session.GetHabbo() == null) return;
            ParadiseCharacter character = CharacterService.GetOrLoad(session.GetHabbo().Id);
            if (character == null)
            {
                ParadiseUiEventService.OpenProfile(session.GetHabbo().Id, "identity");
                session.SendWhisper("Créez d'abord votre identité citoyenne.", 1);
                return;
            }
            DocumentService.EnsureIdentityCard(session.GetHabbo().Id, character);
            ParadiseUiEventService.OpenProfile(session.GetHabbo().Id, "documents", DocumentService.IdentityCode);
        }
    }

    internal sealed class ParadiseDocumentsCommand : ParadiseCharacterCommandBase
    {
        public override string Parameters { get { return ""; } }
        public override string Description { get { return "Ouvre vos documents officiels ParadiseRP."; } }

        public override void Execute(GameClient session, Room room, string[] parameters)
        {
            if (session == null || session.GetHabbo() == null) return;
            ParadiseUiEventService.OpenProfile(session.GetHabbo().Id, HasCharacter(session) ? "documents" : "identity");
        }
    }

    internal sealed class ParadiseLicenseCommand : ParadiseCharacterCommandBase
    {
        public override string Parameters { get { return ""; } }
        public override string Description { get { return "Ouvre votre permis de conduire si vous en possédez un."; } }

        public override void Execute(GameClient session, Room room, string[] parameters)
        {
            if (session == null || session.GetHabbo() == null) return;
            ParadiseDocument license = DocumentService.GetDocument(session.GetHabbo().Id, DocumentService.DriverLicenseCode, true);
            if (license == null || !license.IsValid)
            {
                session.SendWhisper("Vous ne possédez pas de permis de conduire.", 1);
                return;
            }
            ParadiseUiEventService.OpenProfile(session.GetHabbo().Id, "documents", DocumentService.DriverLicenseCode);
        }
    }

    internal sealed class ParadiseShowIdCommand : ParadiseCharacterCommandBase
    {
        public override string Parameters { get { return "<joueur>"; } }
        public override string Description { get { return "Présente votre carte d'identité à un joueur dans la même room."; } }

        public override void Execute(GameClient session, Room room, string[] parameters)
        {
            if (parameters.Length < 2)
            {
                session.SendWhisper("Syntaxe : :showid <joueur>", 1);
                return;
            }
            string message;
            if (!DocumentService.Present(session, parameters[1], DocumentService.IdentityCode, out message) && !String.IsNullOrEmpty(message))
                session.SendWhisper(message, 1);
        }
    }

    internal sealed class ParadiseShowLicenseCommand : ParadiseCharacterCommandBase
    {
        public override string Parameters { get { return "<joueur>"; } }
        public override string Description { get { return "Présente votre permis de conduire à un joueur dans la même room."; } }

        public override void Execute(GameClient session, Room room, string[] parameters)
        {
            if (parameters.Length < 2)
            {
                session.SendWhisper("Syntaxe : :showlicense <joueur>", 1);
                return;
            }
            string message;
            if (!DocumentService.Present(session, parameters[1], DocumentService.DriverLicenseCode, out message) && !String.IsNullOrEmpty(message))
                session.SendWhisper(message, 1);
        }
    }
}
