using UnityEngine;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Synthesis.Input.Inputs;
using Synthesis.Input.Enums;
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
        /// Saves all primary and secondary controls.
        /// Source: https://github.com/Gris87/InputControl
        /// </summary>
        public static void Save()
        {
            for (int player_i = 0; player_i < Player.PLAYER_COUNT; player_i++)
            {
                Players[player_i].SaveActiveProfile();
            }
        }

        /// <summary>
        /// Checks if the user has saved their control settings by comparing strings.
        /// </summary>
        /// <returns>True: If user did save their controls
        ///          False: If the user hasn't saved their controls.
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
        /// Loads all primary and secondary controls.
        /// Source: https://github.com/Gris87/InputControl
        /// </summary>
        public static void Load()
        {
            for (int player_i = 0; player_i < Player.PLAYER_COUNT; player_i++)
            {
                Players[player_i].LoadActiveProfile();
            }
        }

        public static Player.Profile CreateDefault(int player_i, Player.ControlProfile controlProfile)
        {
            Player.Profile profile = new Player.Profile();
            var key_i = player_i + 1;

            if (System.Enum.TryParse("Joystick" + key_i, false, out Joystick joy))
            {
                switch (controlProfile)
                {
                    case Player.ControlProfile.ArcadeKeyboard:
                        {
                            #region Arcade Controls

                            profile.buttons.forward = new KeyMapping("Forward", new JoystickInput(JoystickAxis.Axis2Negative, joy));
                            profile.buttons.backward = new KeyMapping("Backward", new JoystickInput(JoystickAxis.Axis2Positive, joy));
                            profile.buttons.left = new KeyMapping("Left", new JoystickInput(JoystickAxis.Axis4Negative, joy));
                            profile.buttons.right = new KeyMapping("Right", new JoystickInput(JoystickAxis.Axis4Positive, joy));

                            profile.axes.horizontal = new Axis("Joystick " + key_i + " Axis 2", profile.buttons.left, profile.buttons.right);
                            profile.axes.vertical = new Axis("Joystick " + key_i + " Axis 4", profile.buttons.backward, profile.buttons.forward);

                            #endregion
                        }
                        break;
                    case Player.ControlProfile.TankJoystick:
                        {
                            #region Tank Controls

                            profile.buttons.tankFrontLeft = new KeyMapping("Tank Front Left", new JoystickInput(JoystickAxis.Axis2Negative, joy));
                            profile.buttons.tankBackLeft = new KeyMapping("Tank Back Left", new JoystickInput(JoystickAxis.Axis2Positive, joy));
                            profile.buttons.tankFrontRight = new KeyMapping("Tank Front Right", new JoystickInput(JoystickAxis.Axis5Negative, joy));
                            profile.buttons.tankBackRight = new KeyMapping("Tank Back Right", new JoystickInput(JoystickAxis.Axis5Positive, joy));

                            profile.axes.tankLeftAxes = new Axis("Joystick " + key_i + " Axis 9", profile.buttons.tankBackLeft, profile.buttons.tankFrontLeft);
                            profile.axes.tankRightAxes = new Axis("Joystick " + key_i + " Axis 10", profile.buttons.tankFrontRight, profile.buttons.tankBackRight);

                            #endregion
                        }
                        break;
                    default:
                        throw new Player.UnhandledControlProfileException();
                }


                #region General Controls

                //PWM controls
                for (int pwm_i = 0; pwm_i < DriveJoints.PWM_COUNT - DriveJoints.PWM_OFFSET; pwm_i++)
                {
                    var pwm_key_i = pwm_i + 1;
                    if (System.Enum.TryParse("Axis" + pwm_key_i + "Positive", false, out JoystickAxis axis_pos) &&
                        System.Enum.TryParse("Axis" + pwm_key_i + "Negative", false, out JoystickAxis axis_neg))
                    {
                        profile.buttons.pwmPos[pwm_i] = new KeyMapping("PWM " + (pwm_i + DriveJoints.PWM_OFFSET) + " Positive", new JoystickInput(axis_pos, joy));
                        profile.buttons.pwmNeg[pwm_i] = new KeyMapping("PWM " + (pwm_i + DriveJoints.PWM_OFFSET) + " Negative", new JoystickInput(axis_neg, joy));

                        profile.axes.pwmAxes[pwm_i] = new Axis("PWM " + (pwm_i + DriveJoints.PWM_OFFSET) + " Axis " + (pwm_key_i + DriveJoints.PWM_OFFSET), profile.buttons.pwmNeg[pwm_i], profile.buttons.pwmPos[pwm_i]);
                    }
                    else
                    {
                        throw new System.Exception("Error configuring PWM buttons");
                    }
                }

                //Other Controls
                profile.buttons.resetRobot = new KeyMapping("Reset Robot", new JoystickInput(JoystickButton.Button8, joy));
                profile.buttons.resetField = new KeyMapping("Reset Field", new JoystickInput(JoystickButton.Button9, joy));
                profile.buttons.replayMode = new KeyMapping("Replay Mode", new JoystickInput(JoystickButton.Button12, joy));
                profile.buttons.cameraToggle = new KeyMapping("Camera Toggle", new JoystickInput(JoystickButton.Button7, joy));
                profile.buttons.scoreboard = new KeyMapping("Scoreboard", new JoystickInput(JoystickButton.Button10, joy));
                profile.buttons.trajectory = new KeyMapping("Toggle Trajectory", new JoystickInput(JoystickButton.Button11, joy));
                profile.buttons.duplicateRobot = new KeyMapping("Duplicate Robot", new JoystickInput(JoystickButton.Button1, joy));
                profile.buttons.switchActiveRobot = new KeyMapping("Switch Active Robot", new JoystickInput(JoystickButton.Button2, joy));

                #endregion
            }
            else
            {
                throw new System.Exception("Failed to establish joystick index");
            }
            return profile;
        }


        public static void UpdateFieldControls(int player_i)
        {
            Player.ControlProfile controlProfile = Players[player_i].GetActiveControlProfile();
            Players[player_i].GetProfile(controlProfile).buttons.pickup.Clear();
            Players[player_i].GetProfile(controlProfile).buttons.release.Clear();
            Players[player_i].GetProfile(controlProfile).buttons.spawnPieces.Clear();

            var key_i = player_i + 1;

            for (int i = 0; i < FieldDataHandler.gamepieces.Count; i++)
            {
                if (System.Enum.TryParse("Joystick" + key_i, false, out Joystick joy))
                {
                    Players[player_i].GetProfile(controlProfile).buttons.pickup.Add(new KeyMapping("Pick Up " + FieldDataHandler.gamepieces[i].name, new JoystickInput(JoystickButton.Button3, joy)));
                    Players[player_i].GetProfile(controlProfile).buttons.release.Add(new KeyMapping("Release " + FieldDataHandler.gamepieces[i].name, new JoystickInput(JoystickButton.Button4, joy)));
                    Players[player_i].GetProfile(controlProfile).buttons.spawnPieces.Add(new KeyMapping("Spawn " + FieldDataHandler.gamepieces[i].name, new JoystickInput(JoystickButton.Button5, joy)));
                }
                else
                {
                    throw new System.Exception("Failed to establish joystick index");
                }
            }
        }
        public static void UpdateFieldControls()
        {
            for (int player_i = 0; player_i < Player.PLAYER_COUNT; player_i++)
            {
                for (int i = 0; i < FieldDataHandler.gamepieces.Count; i++)
                {
                    UpdateFieldControls(player_i);
                }
            }
        }
    }
}