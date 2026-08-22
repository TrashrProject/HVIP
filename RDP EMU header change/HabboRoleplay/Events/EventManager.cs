using System;
using System.Collections.Concurrent;
using Plus.HabboRoleplay.Events.Methods;
using log4net;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Rooms;

namespace Plus.HabboRoleplay.Events
{
    public static class EventManager
    {
        /// <summary>
        /// log4net
        /// </summary>
        private static readonly ILog log = LogManager.GetLogger("Plus.HabboRoleplay.Misc.RoleplayData");

        /// <summary>
        /// Dictionary containing the events
        /// </summary>
        public static ConcurrentDictionary<string, IEvent> Events;

        /// <summary>
        /// Registers the events
        /// </summary>
        public static void Initialize()
        {
            Events = new ConcurrentDictionary<string, IEvent>();
            Events.TryAdd("OnAddedToRoom", new OnAddedToRoom());
            Events.TryAdd("OnHealthChange", new OnHealthChange());
            Events.TryAdd("OnLogin", new OnLogin());
            Events.TryAdd("OnDisconnect", new OnDisconnect());

            log.Info("EventManager (" + Events.Count + ") -> LOADED");
        }

        /// <summary>
        /// Triggers an event
        /// </summary>
        public static void TriggerEvent(string EventName, object Source, params object[] Params)
        {
            if (!Events.ContainsKey(EventName))
                return;

            IEvent Event = Events[EventName];
            Event.Execute(Source, Params);
        }
    }
}

namespace Plus.HabboRoleplay.Paradise.Messaging
{
    public enum ParadiseSystemMessageCategory
    {
        System,
        Success,
        Error,
        Combat,
        Money,
        Inventory,
        Document
    }

    public enum ParadiseSystemMessageScope
    {
        Self,
        Target,
        Room
    }

    /// <summary>
    /// Central gameplay feedback service. It deliberately reuses the existing
    /// Habbo room whisper transport instead of introducing a second chat stack.
    /// Messages are plain server text prefixed with a semantic category; the
    /// client remains responsible for any future visual styling.
    /// </summary>
    public static class ParadiseSystemMessageService
    {
        public static void SendSelf(GameClient recipient, ParadiseSystemMessageCategory category, string message)
        {
            SendToClient(recipient, category, message);
        }

        public static void SendTarget(GameClient actor, GameClient target, ParadiseSystemMessageCategory category,
            string actorMessage, string targetMessage)
        {
            SendToClient(actor, category, actorMessage);

            if (target != null && target != actor)
                SendToClient(target, category, targetMessage);
        }

        public static void SendRoom(Room room, ParadiseSystemMessageCategory category, string message)
        {
            if (room == null || room.GetRoomUserManager() == null || String.IsNullOrWhiteSpace(message))
                return;

            foreach (RoomUser roomUser in room.GetRoomUserManager().GetRoomUsers())
            {
                if (roomUser == null || roomUser.IsBot || roomUser.IsPet)
                    continue;

                GameClient client = roomUser.GetClient();
                if (client == null || client.GetHabbo() == null || client.LoggingOut)
                    continue;

                SendToClient(client, category, message);
            }
        }

        public static string Format(ParadiseSystemMessageCategory category, string message)
        {
            if (String.IsNullOrWhiteSpace(message))
                return String.Empty;

            return "[" + GetCategoryLabel(category) + "] " + message.Trim();
        }

        private static void SendToClient(GameClient client, ParadiseSystemMessageCategory category, string message)
        {
            if (client == null || client.GetHabbo() == null || client.LoggingOut || String.IsNullOrWhiteSpace(message))
                return;

            client.SendWhisper(Format(category, message), 1);
        }

        private static string GetCategoryLabel(ParadiseSystemMessageCategory category)
        {
            switch (category)
            {
                case ParadiseSystemMessageCategory.Success:
                    return "SUCCÈS";
                case ParadiseSystemMessageCategory.Error:
                    return "ERREUR";
                case ParadiseSystemMessageCategory.Combat:
                    return "COMBAT";
                case ParadiseSystemMessageCategory.Money:
                    return "ARGENT";
                case ParadiseSystemMessageCategory.Inventory:
                    return "INVENTAIRE";
                case ParadiseSystemMessageCategory.Document:
                    return "DOCUMENT";
                default:
                    return "SYSTÈME";
            }
        }
    }
}
