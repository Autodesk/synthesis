using Assets.Scripts.FSM;
using BulletSharp;
using BulletUnity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.FEA
{
    public class ReplayState : SimState
    {
        private const float CircleRenderDistance = 10f;

        private const int SliderLeftMargin = 192;
        private const int SliderRightMargin = 192;
        private const int SliderBottomMargin = 32;
        private const int SliderThickness = 16;

        private const int ThumbWidth = 16;
        private const int ThumbHeight = 16;

        private const int KeyframeWidth = 8;
        private const int KeyframeHeight = 16;

        private const int CircleRadius = 8;

        private const int ControlButtonSize = 32;
        private const int ControlMargin = (SliderLeftMargin - ControlButtonSize * 3) / 4;

        private const int CollisionButtonMargin = 16;

        private const int CollisionSliderMargin = 16;
        private const int CollisionSliderHeight = 128;
        private const float CollisionSliderTopValue = 20f;

        enum PlaybackMode
        {
            Paused,
            Play,
            Rewind
        }

        private PlaybackMode playbackMode;

        private float rewindTime;
        private float playbackSpeed;
        private float sliderPos;
        private float contactThreshold;

        private bool firstFrame;
        private bool active;
        private bool showCollisionSlider;

        private Camera camera;
        private List<Tracker> trackers;
        private List<List<ContactDescriptor>> contactPoints;

        private Texture keyframeTexture;
        private Texture circleTexture;

        private GUIStyle windowStyle;
        private GUIStyle thumbStyle;
        private GUIStyle rewindStyle;
        private GUIStyle stopStyle;
        private GUIStyle playStyle;
        private GUIStyle collisionStyle;

        /// <summary>
        /// The normalized replay time.
        /// </summary>
        private float ReplayTime
        {
            get
            {
                return (rewindTime / Tracker.Lifetime) * (Tracker.Length - 1);
            }
        }

        /// <summary>
        /// An adjusted contact threshold to represent the range of most common values.
        /// </summary>
        private float AdjustedContactThreshold
        {
            get
            {
                return contactThreshold * contactThreshold;
            }
        }

        /// <summary>
        /// Creates a new ReplayState instance.
        /// </summary>
        public ReplayState(FixedQueue<List<ContactDescriptor>> contactPoints, List<Tracker> trackers)
        {
            this.contactPoints = contactPoints.ToList();
            this.trackers = trackers;

            playbackMode = PlaybackMode.Paused;
            firstFrame = true;
            active = false;
            contactThreshold = Mathf.Sqrt(CollisionSliderTopValue / 2);

            camera = UnityEngine.Object.FindObjectOfType<Camera>();

            Texture2D thumbTexture = (Texture2D)Resources.Load("Images/thumb");

            Texture2D rewindTexture = (Texture2D)Resources.Load("Images/rewind");
            Texture2D rewindHoverTexture = (Texture2D)Resources.Load("Images/rewindHover");
            Texture2D rewindPressedTexture = (Texture2D)Resources.Load("Images/rewindPressed");

            Texture2D stopTexture = (Texture2D)Resources.Load("Images/stop");
            Texture2D stopHoverTexture = (Texture2D)Resources.Load("Images/stopHover");
            Texture2D stopPressedTexture = (Texture2D)Resources.Load("Images/stopPressed");

            Texture2D playTexture = (Texture2D)Resources.Load("Images/play");
            Texture2D playHoverTexture = (Texture2D)Resources.Load("Images/playHover");
            Texture2D playPressedTexture = (Texture2D)Resources.Load("Images/playPressed");

            Texture2D collisionTexture = (Texture2D)Resources.Load("Images/collision");
            Texture2D collisionHoverTexture = (Texture2D)Resources.Load("Images/collisionHover");
            Texture2D collisionPressedTexture = (Texture2D)Resources.Load("Images/collisionPressed");

            circleTexture = (Texture)Resources.Load("Images/circle");
            keyframeTexture = (Texture)Resources.Load("Images/keyframe");

            Texture2D sliderBackground = new Texture2D(1, 1);
            sliderBackground.SetPixel(0, 0, new Color(0.1f, 0.15f, 0.15f, 0.75f));
            sliderBackground.Apply();

            windowStyle = new GUIStyle
            {
                alignment = TextAnchor.UpperCenter,
                normal = new GUIStyleState
                {
                    background = sliderBackground,
                    textColor = Color.white
                }
            };

            thumbStyle = new GUIStyle
            {
                fixedWidth = ThumbWidth,
                fixedHeight = ThumbHeight,
                normal = new GUIStyleState
                {
                    background = thumbTexture
                }
            };

            rewindStyle = new GUIStyle
            {
                fixedWidth = ControlButtonSize,
                fixedHeight = ControlButtonSize,
                normal = new GUIStyleState { background = rewindTexture },
                hover = new GUIStyleState { background = rewindHoverTexture },
                active = new GUIStyleState { background = rewindPressedTexture }
            };

            stopStyle = new GUIStyle
            {
                fixedWidth = ControlButtonSize,
                fixedHeight = ControlButtonSize,
                normal = new GUIStyleState { background = stopTexture },
                hover = new GUIStyleState { background = stopHoverTexture },
                active = new GUIStyleState { background = stopPressedTexture }
            };

            playStyle = new GUIStyle
            {
                fixedWidth = ControlButtonSize,
                fixedHeight = ControlButtonSize,
                normal = new GUIStyleState { background = playTexture },
                hover = new GUIStyleState { background = playHoverTexture },
                active = new GUIStyleState { background = playPressedTexture }
            };

            collisionStyle = new GUIStyle
            {
                fixedWidth = ControlButtonSize,
                fixedHeight = ControlButtonSize,
                normal = new GUIStyleState { background = collisionTexture },
                hover = new GUIStyleState { background = collisionHoverTexture },
                active = new GUIStyleState { background = collisionPressedTexture }
            };
        }

        /// <summary>
        /// Initializes the state.
        /// </summary>
        public override void Start()
        {
            foreach (Tracker t in trackers)
            {
                RigidBody r = (RigidBody)t.GetComponent<BRigidBody>().GetCollisionObject();
                r.LinearVelocity = r.AngularVelocity = BulletSharp.Math.Vector3.Zero;
                r.LinearFactor = r.AngularFactor = BulletSharp.Math.Vector3.Zero;

                if (t.Trace)
                {
                    LineRenderer lr = t.gameObject.AddComponent<LineRenderer>();
                    lr.startWidth = 0.01f;
                    lr.endWidth = 0.01f;
                    lr.material = new Material(Shader.Find("Particles/Alpha Blended"));
                    lr.startColor = new Color(0.0f, 0.75f, 0.0f, 1.0f);
                    lr.endColor = new Color(0.0f, 1.0f, 0.0f, 0.25f);
                    lr.positionCount = t.States.Length;
                    lr.SetPositions(t.States.Select((x) => x.Position.ToUnity()).ToArray());
                }
            }

            rewindTime = 0.0f;
            playbackSpeed = 1.0f;
        }

        /// <summary>
        /// Draws the horizontal slider.
        /// </summary>
        public override void OnGUI()
        {
            Rect controlRect = new Rect(ControlMargin, Screen.height - (SliderBottomMargin + SliderThickness + SliderThickness / 2),
                ControlButtonSize, ControlButtonSize);

            if (GUI.Button(controlRect, string.Empty, rewindStyle))
            {
                if (rewindTime == Tracker.Lifetime)
                    rewindTime = 0f;

                playbackMode = PlaybackMode.Rewind;
            }

            controlRect.x += ControlButtonSize + ControlMargin;

            if (GUI.Button(controlRect, string.Empty, stopStyle))
                playbackMode = PlaybackMode.Paused;

            controlRect.x += ControlButtonSize + ControlMargin;

            if (GUI.Button(controlRect, string.Empty, playStyle))
            {
                if (rewindTime == 0f)
                    rewindTime = Tracker.Lifetime;

                playbackMode = PlaybackMode.Play;
            }

            controlRect.x = Screen.width - SliderRightMargin + CollisionButtonMargin;

            if (GUI.Button(controlRect, string.Empty, collisionStyle))
                showCollisionSlider = !showCollisionSlider;

            Rect collisionSliderRect = new Rect(controlRect.x + (ControlButtonSize / 2 - SliderThickness / 2),
                controlRect.y - CollisionSliderMargin - CollisionSliderHeight, SliderThickness, CollisionSliderHeight);

            if (showCollisionSlider)
                contactThreshold = GUI.VerticalSlider(collisionSliderRect, contactThreshold, Mathf.Sqrt(CollisionSliderTopValue), 0f, windowStyle, thumbStyle);

            Rect collisionLabelRect = new Rect(controlRect.x + controlRect.width + CollisionButtonMargin, controlRect.y,
                Screen.width - (controlRect.x + controlRect.width + CollisionButtonMargin * 2), controlRect.height);

            GUI.Label(collisionLabelRect, "Impact Threshold:\n" + AdjustedContactThreshold.ToString("F2") + " Newtons", windowStyle);

            Rect sliderRect = new Rect(SliderLeftMargin, Screen.height - (SliderBottomMargin + SliderThickness),
                Screen.width - (SliderRightMargin + SliderLeftMargin), SliderThickness);

            rewindTime = GUI.HorizontalSlider(sliderRect, rewindTime, Tracker.Lifetime, 0.0f, windowStyle, thumbStyle);

            Rect guiRect = new Rect(0, Screen.height - (SliderBottomMargin + SliderThickness + KeyframeHeight),
                Screen.width, SliderBottomMargin + SliderThickness + KeyframeHeight);

            if (!active && (guiRect.Contains(Event.current.mousePosition) || (showCollisionSlider && collisionSliderRect.Contains(Event.current.mousePosition))) &&
                Input.GetMouseButton(0))
            {
                DynamicCamera.MovingEnabled = false;
                active = true;
                playbackMode = PlaybackMode.Paused;
            }
            else if (active && !Input.GetMouseButton(0))
            {
                DynamicCamera.MovingEnabled = true;
                active = false;
            }

            int i = Tracker.Length - 1;

            foreach (List<ContactDescriptor> l in contactPoints)
            {
                if (l != null)
                {
                    ContactDescriptor lastContact = default(ContactDescriptor);

                    foreach (ContactDescriptor m in l)
                    {
                        if (m.AppliedImpulse >= AdjustedContactThreshold)
                        {
                            float keyframeTime = Tracker.Lifetime - ((float)i / (Tracker.Length - 1)) * Tracker.Lifetime;

                            if (!lastContact.Equals(m))
                            {
                                lastContact = m;

                                float pixelsPerValue = (sliderRect.width - KeyframeWidth - ThumbWidth / 2) / Tracker.Lifetime;

                                Rect keyframeRect = new Rect(
                                    (Tracker.Lifetime - keyframeTime) * pixelsPerValue + sliderRect.x + (ThumbWidth - KeyframeWidth) / 2,
                                    sliderRect.y - KeyframeHeight,
                                    KeyframeWidth,
                                    sliderRect.height);

                                if (GUI.Button(keyframeRect, keyframeTexture, GUIStyle.none))
                                {
                                    rewindTime = keyframeTime;
                                    playbackMode = PlaybackMode.Paused;
                                }
                            }

                            Vector3 collisionPoint = camera.WorldToScreenPoint(m.Position.ToUnity());

                            if (collisionPoint.z > 0.0f)
                            {
                                float distance = Math.Abs((Tracker.Length - 1 - i) - ReplayTime);

                                Rect circleRect = new Rect(collisionPoint.x - CircleRadius, Screen.height - (collisionPoint.y - CircleRadius),
                                    CircleRadius * 2, CircleRadius * 2);

                                if (circleRect.Contains(Event.current.mousePosition))
                                    GUI.color = Color.white;
                                else
                                    GUI.color = new Color(1f, 1f, 1f, Math.Max((CircleRenderDistance - distance) / CircleRenderDistance, 0.1f));

                                if (GUI.Button(circleRect, circleTexture, GUIStyle.none))
                                    rewindTime = keyframeTime;

                                GUI.color = Color.white;
                            }
                        }
                    }
                }

                i--;
            }
        }

        /// <summary>
        /// Updates the positions and rotations of each tracker's parent object according to the replay time.
        /// </summary>
        public override void Update()
        {
            if (firstFrame)
            {
                firstFrame = false;
            }
            else if (Input.GetKeyDown(KeyCode.Space))
            {
                switch (playbackMode)
                {
                    case PlaybackMode.Paused:
                        if (rewindTime == 0.0f)
                            rewindTime = Tracker.Lifetime;
                        playbackMode = PlaybackMode.Play;
                        break;
                    case PlaybackMode.Play:
                    case PlaybackMode.Rewind:
                        playbackMode = PlaybackMode.Paused;
                        break;
                }
            }

            switch (playbackMode)
            {
                case PlaybackMode.Rewind:
                    rewindTime += Time.smoothDeltaTime * playbackSpeed;
                    break;
                case PlaybackMode.Play:
                    rewindTime -= Time.smoothDeltaTime * playbackSpeed;
                    break;
            }

            if (Input.GetKey(KeyCode.LeftArrow))
            {
                rewindTime += Time.smoothDeltaTime * 0.25f;
                playbackMode = PlaybackMode.Paused;
            }

            if (Input.GetKey(KeyCode.RightArrow))
            {
                rewindTime -= Time.smoothDeltaTime * 0.25f;
                playbackMode = PlaybackMode.Paused;
            }

            if (rewindTime < 0.0f)
            {
                rewindTime = 0.0f;
                playbackMode = PlaybackMode.Paused;
            }
            else if (rewindTime > Tracker.Lifetime)
            {
                rewindTime = Tracker.Lifetime;
                playbackMode = PlaybackMode.Paused;
            }

            foreach (Tracker t in trackers)
            {
                float replayTime = ReplayTime;
                int currentIndex = (int)Math.Floor(replayTime);

                StateDescriptor lowerState = t.States[currentIndex];
                StateDescriptor upperState = currentIndex < t.States.Length - 1 ? t.States[currentIndex + 1] : lowerState;

                float percent = replayTime - currentIndex;

                RigidBody r = (RigidBody)t.GetComponent<BRigidBody>().GetCollisionObject();

                if (!r.IsActive)
                    r.Activate();

                BulletSharp.Math.Matrix worldTransform = r.WorldTransform;

                worldTransform.Origin = BulletSharp.Math.Vector3.Lerp(lowerState.Position, upperState.Position, percent);
                worldTransform.Basis = BulletSharp.Math.Matrix.Lerp(lowerState.Rotation, upperState.Rotation, percent);

                r.WorldTransform = worldTransform;
            }

            if (Input.GetKey(KeyCode.Return))
                StateMachine.Instance.PopState();    
        }

        /// <summary>
        /// Resets the trackers and reenables the RigidBodies.
        /// </summary>
        public override void End()
        {
            foreach (Tracker t in trackers)
            {
                if (t.Trace)
                    UnityEngine.Object.Destroy(t.gameObject.GetComponent<LineRenderer>());

                StateDescriptor currentState = t.States[(int)Math.Floor(ReplayTime)];

                RigidBody r = (RigidBody)t.GetComponent<BRigidBody>().GetCollisionObject();
                r.LinearFactor = r.AngularFactor = BulletSharp.Math.Vector3.One;
                r.LinearVelocity = currentState.LinearVelocity;
                r.AngularVelocity = currentState.AngularVelocity;

                t.Clear();
            }
        }
    }
}
