using System.Collections.Generic;
using Fleck;
using Newtonsoft.Json;
using Plus.HabboHotel.GameClients;
using Plus.HabboRoleplay.Paradise.Character;

namespace Plus.HabboRoleplay.Paradise.Bridge
{
    /// <summary>
    /// First ParadiseBridge message. Uses the existing roleplay WebEvent socket and
    /// never touches Nitro packet framing or the Nitro WebSocket.
    /// </summary>
    public static class ParadiseSnapshotBridge
    {
        private static readonly CharacterService CharacterService = new CharacterService();

        public static bool Send(GameClient client, IWebSocketConnection socket)
        {
            if (client == null || socket == null || !socket.IsAvailable)
                return false;

            CharacterSnapshot snapshot = CharacterService.GetSnapshot(client);
            if (snapshot == null)
                return false;

            var envelope = new Dictionary<string, object>
            {
                { "v", 1 },
                { "event", "player:snapshot" },
                { "data", snapshot }
            };

            socket.Send("compose_paradise|" + JsonConvert.SerializeObject(envelope));
            return true;
        }
    }
}
