using Synthesis.Field;

namespace Synthesis.Input
{
    public class Controls
    {
        public static Player[] Players;
        public static GlobalPlayer Global;

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

            Global = new GlobalPlayer();

            Load();
        }

        /// <summary>
        /// Saves all player controls.
        /// Source: https://github.com/Gris87/InputControl
        /// </summary>
        public static void Save(bool quiet = false)
        {
            for (int player_i = 0; player_i < Player.PLAYER_COUNT; player_i++)
            {
                Players[player_i].SaveActiveProfile();
            }
            Global.Save();
            if (!quiet)
                GUI.UserMessageManager.Dispatch("Player controls saved.", 5);
        }

        /// <summary>
        /// Checks if the user has saved their control settings by comparing strings.
        /// </summary>
        /// <returns>
        /// True if user has saved their controls
        /// </returns>
        public static bool HasBeenSaved()
        {
            for (int player_i = 0; player_i < Player.PLAYER_COUNT; player_i++)
            {
                if (!Players[player_i].HasBeenSaved())
                {
                    return false;
                }
            }
            return Global.HasBeenSaved();
        }

        /// <summary>
        /// Loads all active player profile controls.
        /// </summary>
        public static void Load()
        {
            bool failure = false;
            for (int player_i = 0; player_i < Player.PLAYER_COUNT; player_i++)
            {
                try
                {
                    Players[player_i].LoadActiveProfile();
                }
                catch (System.Exception)
                {
                    failure = true;
                }
            }
            try
            {
                Global.Load();
            }
            catch (System.Exception)
            {
                failure = true;
            }
            if (failure)
            {
                GUI.UserMessageManager.Dispatch("Error loading controls. Resetting to defaults.", 5);
            }
        }

        public static void UpdateFieldControls()
        {
            for (int player_i = 0; player_i < Player.PLAYER_COUNT; player_i++)
            {
                Players[player_i].GetActiveProfile().UpdateFieldControls(player_i);
            }
        }
    }
}