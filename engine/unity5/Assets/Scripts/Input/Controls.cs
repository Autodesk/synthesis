using Synthesis.Field;

namespace Synthesis.Input
{
    public class Controls
    {
        public static Player[] Players;

        /// <summary>
        /// Initializes the <see cref="Controls"/> class.
        /// </summary>
        static Controls()
        {
            Players = new Player[Player.PLAYER_COUNT];
            for (int i = 0; i < Player.PLAYER_COUNT; i++)
            {
                Players[i] = new Player(i);
            }

            Load();
        }

        /// <summary>
        /// Saves all player controls.
        /// Source: https://github.com/Gris87/InputControl
        /// </summary>
        public static void Save()
        {
            for (int player_i = 0; player_i < Player.PLAYER_COUNT; player_i++)
            {
                Players[player_i].SaveActiveProfile();
            }
            GUI.UserMessageManager.Dispatch("Player preferences saved.", 5);
        }

        /// <summary>
        /// Checks if the user has saved their control settings by comparing strings.
        /// </summary>
        /// <returns>
        /// True if user has saved their controls
        /// </returns>
        public static bool CheckIfSaved()
        {
            for (int player_i = 0; player_i < Player.PLAYER_COUNT; player_i++)
            {
                if (!Players[player_i].CheckIfSaved())
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Loads all active player profile controls.
        /// </summary>
        public static void Load()
        {
            for (int player_i = 0; player_i < Player.PLAYER_COUNT; player_i++)
            {
                Players[player_i].LoadActiveProfile();
            }
        }

        public static void UpdateFieldControls()
        {
            for (int player_i = 0; player_i < Player.PLAYER_COUNT; player_i++)
            {
                Players[player_i].GetProfile(Players[player_i].GetActiveProfileMode()).UpdateFieldControls(player_i);
            }
        }
    }
}