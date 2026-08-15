using System;
using Plus.HabboHotel.GameClients;

namespace Plus.HabboRoleplay.Combat
{
    public interface ICombat
    {
        /// <summary>
        /// Checks to see if the client can complete the action
        /// </summary>
        bool CanCombat(GameClient Client, GameClient TargetClient);

        /// <summary>
        /// Performs the action
        /// </summary>
        void Execute(GameClient Client, GameClient TargetClient, bool HitClosest = true);


        /// <summary>
        /// Gets the XP from the combat type
        /// </summary>
        /// <returns>EXP retrieved</returns>
        int GetEXP(GameClient Client, GameClient TargetClient);

        /// <summary>
        /// Gets the coins from the combat type
        /// </summary>
        /// <returns>Coins retrieved</returns>
        int GetCoins(GameClient TargetClient);

        /// <summary>
        /// Get Rewards from combat type
        /// </summary>
        void GetRewards(GameClient Client, GameClient TargetClient);
    }
}
