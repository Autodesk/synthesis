using Assets.Scripts.FSM;
using BulletSharp;
using BulletUnity;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Analytics;

namespace Assets.Scripts.FEA
{
    public class ReplayState : SimState
    {
        private const float CircleRenderDistance = 10f;
        private const float ConsolidationEpsilon = 0.25f;

        private const int SliderLeftMargin = 192;
        private const int SliderRightMargin = 192;
        private const int SliderBottomMargin = 32;
        private const int SliderThickness = 16;

        private const int ThumbWidth = 16;
        private const int ThumbHeight = 16;

        private const int KeyframeWidth = 8;
        private const int KeyframeHeight = 16;

        private const int CircleRadius = 8;

        private const int ButtonSize = 32;
        private const int ControlButtonMargin = (SliderLeftMargin - ButtonSize * 3) / 4;
        private const int EditButtonMargin = (SliderRightMargin - ButtonSize * 2) / 3;

        private const int CollisionSliderMargin = 16;
        private const int CollisionSliderHeight = 256;
        private const float CollisionSliderTopValue = 500f;

        private const int InfoBoxWidth = 112;
        private const int InfoBoxHeight = 16;

        private const int ReturnWidth = 128;
        private const int ReturnHeight = 64;
        private const int ReturnMargin = 16;

        private const int SaveWidth = 64;
        private const int SaveHeight = 64;
        private const int SaveMargin = 16;

        private readonly Color HighlightColor = new Color(1.0f, 0.0f, 0.0f);

        private enum PlaybackMode
        {
            Paused,
            Play,
            Rewind
        }

        private PlaybackMode playbackMode;

        private enum EditMode
        {
            None,
            Threshold,
            Consolidate
        }

        private EditMode editMode;

        private string fieldPath;

        private float rewindTime;
        private float playbackSpeed;
        private float sliderPos;
        private float contactThreshold;

        private bool firstFrame;
        private bool active;

        private Camera camera;
        private DynamicCamera dynamicCamera;
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
        private GUIStyle consolidateStyle;

        private BRigidBody _selectedBody;

        private float tStart; //for analytics--tracking when user starts replay mode

        /// <summary>
        /// The body being currently highlighted.
        /// </summary>
        private BRigidBody SelectedBody
        {
            get
            {
                return _selectedBody;
            }
            set
            {
                if (value == _selectedBody)
                    return;

                if (_selectedBody != null)
                    CurrentColor = Color.black;

                if ((_selectedBody = value) != null)
                    CurrentColor = HighlightColor;
            }
        }

        /// <summary>
        /// Used for setting the color of the selected body.
        /// </summary>
        private Color CurrentColor
        {
            set
            {
                foreach (Renderer r in SelectedBody.GetComponentsInChildren<Renderer>())
                    foreach (Material m in r.materials)
                        m.SetColor("_EmissionColor", value);
            }
        }

        /// <summary>
        /// The replay time in frames.
        /// </summary>
        private float RewindFrame
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
        public ReplayState(string fieldPath, FixedQueue<List<ContactDescriptor>> contactPoints)
        {
            tStart = Time.time;

            this.fieldPath = fieldPath;
            this.contactPoints = contactPoints.ToList();
            trackers = UnityEngine.Object.FindObjectsOfType<Tracker>().ToList();

            playbackMode = PlaybackMode.Paused;
            firstFrame = true;
            active = false;
            contactThreshold = Mathf.Sqrt(30f);

            DynamicCamera.MovingEnabled = true;

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

            Texture2D consolidateTexture = (Texture2D)Resources.Load("Images/consolidate");
            Texture2D consolidateHoverTexture = (Texture2D)Resources.Load("Images/consolidateHover");
            Texture2D consolidatePressedTexture = (Texture2D)Resources.Load("Images/consolidatePressed");

            circleTexture = (Texture)Resources.Load("Images/circle");
            keyframeTexture = (Texture)Resources.Load("Images/keyframe");

            Texture2D sliderBackground = new Texture2D(1, 1);
            sliderBackground.SetPixel(0, 0, new Color(0.1f, 0.15f, 0.15f, 0.75f));
            sliderBackground.Apply();

            windowStyle = new GUIStyle
            {
                alignment = TextAnchor.UpperLeft,
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

            rewindStyle = CreateButtonStyle("rewind");
            stopStyle = CreateButtonStyle("stop");
            playStyle = CreateButtonStyle("play");
            collisionStyle = CreateButtonStyle("collision");
            consolidateStyle = CreateButtonStyle("consolidate");
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

            Button returnButton = GameObject.Find("ReturnButton").GetComponent<Button>();
            returnButton.onClick.RemoveAllListeners();
            returnButton.onClick.AddListener(ReturnToMainState);

            Button saveButton = GameObject.Find("SaveButton").GetComponent<Button>();
            saveButton.onClick.RemoveAllListeners();
            saveButton.onClick.AddListener(PushSaveReplayState);        
        }

        /// <summary>
        /// Renders the GUI.
        /// </summary>
        public override void OnGUI()
        {
            Rect controlRect = new Rect(ControlButtonMargin, Screen.height - (SliderBottomMargin + SliderThickness + SliderThickness / 2),
                ButtonSize, ButtonSize);

            if (GUI.Button(controlRect, string.Empty, rewindStyle))
            {
                if (rewindTime == Tracker.Lifetime)
                    rewindTime = 0f;

                playbackMode = PlaybackMode.Rewind;
            }

            controlRect.x += ButtonSize + ControlButtonMargin;

            if (GUI.Button(controlRect, string.Empty, stopStyle))
                playbackMode = PlaybackMode.Paused;

            controlRect.x += ButtonSize + ControlButtonMargin;

            if (GUI.Button(controlRect, string.Empty, playStyle))
            {
                if (rewindTime == 0f)
                    rewindTime = Tracker.Lifetime;

                playbackMode = PlaybackMode.Play;
            }

            controlRect.x = Screen.width - SliderRightMargin + EditButtonMargin;

            if (GUI.Button(controlRect, string.Empty, collisionStyle))
                editMode = editMode == EditMode.Threshold ? EditMode.None : EditMode.Threshold;

            Rect collisionSliderRect = new Rect(controlRect.x + (ButtonSize - SliderThickness) / 2,
                controlRect.y - CollisionSliderMargin - CollisionSliderHeight, SliderThickness, CollisionSliderHeight);

            if (editMode == EditMode.Threshold)
            {
                contactThreshold = GUI.VerticalSlider(collisionSliderRect, contactThreshold, Mathf.Sqrt(CollisionSliderTopValue), 0f, windowStyle, thumbStyle);

                Rect collisionLabelRect = new Rect(controlRect.x + controlRect.width, controlRect.y - controlRect.height - CollisionSliderMargin,
                    Screen.width - (controlRect.x + controlRect.width), controlRect.height);

                GUI.Label(collisionLabelRect, "Impulse Threshold:\n" + AdjustedContactThreshold.ToString("F2") + " Newtons", windowStyle);
            }

            controlRect.x += ButtonSize + EditButtonMargin;

            bool consolidatePressed = editMode == EditMode.Consolidate ? GUI.Button(controlRect, consolidateStyle.hover.background, GUIStyle.none) :
                GUI.Button(controlRect, string.Empty, consolidateStyle);

            if (consolidatePressed)
                editMode = editMode == EditMode.Consolidate ? EditMode.None : EditMode.Consolidate;

            if (editMode == EditMode.Consolidate)
                GUI.Label(new Rect(Screen.width - SliderLeftMargin, controlRect.y - InfoBoxHeight - CollisionSliderMargin,
                    Screen.width - SliderLeftMargin, InfoBoxHeight), "Select a collision to consolidate.", windowStyle);

            Rect sliderRect = new Rect(SliderLeftMargin, Screen.height - (SliderBottomMargin + SliderThickness),
                Screen.width - (SliderRightMargin + SliderLeftMargin), SliderThickness);

            rewindTime = GUI.HorizontalSlider(sliderRect, rewindTime, Tracker.Lifetime, 0.0f, windowStyle, thumbStyle);

            Rect guiRect = new Rect(0, Screen.height - (SliderBottomMargin + SliderThickness + KeyframeHeight),
                Screen.width, SliderBottomMargin + SliderThickness + KeyframeHeight);

            if (!active && (guiRect.Contains(Event.current.mousePosition) ||
                (editMode == EditMode.Threshold && collisionSliderRect.Contains(Event.current.mousePosition))) &&
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

            bool circleHovered = false;

            int i = Tracker.Length - 1;

            foreach (List<ContactDescriptor> l in contactPoints)
            {
                if (l != null)
                {
                    ContactDescriptor lastContact = default(ContactDescriptor);

                    for (int j = 0; j < l.Count; j++)
                    {
                        ContactDescriptor currentContact = l[j];

                        if (currentContact.AppliedImpulse >= AdjustedContactThreshold)
                        {
                            float keyframeTime = Tracker.Lifetime - ((float)i / (Tracker.Length - 1)) * Tracker.Lifetime;

                            if (!lastContact.Equals(currentContact))
                            {
                                lastContact = currentContact;

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

                            Vector3 collisionPoint = camera != null ? camera.WorldToScreenPoint(currentContact.Position.ToUnity()) : Vector3.zero;

                            if (collisionPoint.z > 0.0f)
                            {
                                Rect circleRect = new Rect(collisionPoint.x - CircleRadius, Screen.height - (collisionPoint.y + CircleRadius),
                                    CircleRadius * 2, CircleRadius * 2);

                                bool shouldActivate = false;

                                if (circleRect.Contains(Event.current.mousePosition) && !circleHovered)
                                {
                                    GUI.color = Color.white;
                                    SelectedBody = currentContact.RobotBody;
                                    shouldActivate = true;
                                }
                                else
                                {
                                    GUI.color = new Color(1f, 1f, 1f, Math.Max((CircleRenderDistance -
                                        Math.Abs((Tracker.Length - 1 - i) - RewindFrame)) / CircleRenderDistance, 0.1f));
                                }

                                if (GUI.Button(circleRect, circleTexture, GUIStyle.none) && Event.current.button == 0 && shouldActivate)
                                {
                                    if (editMode == EditMode.Consolidate)
                                    {
                                        ConsolidateContacts(l, currentContact);
                                        editMode = EditMode.None;
                                        break;
                                    }
                                    else
                                    {
                                        rewindTime = keyframeTime;
                                        playbackMode = PlaybackMode.Paused;
                                    }
                                }

                                if (circleRect.Contains(Event.current.mousePosition) && shouldActivate)
                                    GUI.Label(new Rect(Event.current.mousePosition.x, Event.current.mousePosition.y - InfoBoxHeight,
                                        InfoBoxWidth, InfoBoxHeight), "Impulse: " + currentContact.AppliedImpulse.ToString("F2") + " N", windowStyle);

                                GUI.color = Color.white;

                                circleHovered = circleHovered || shouldActivate;
                            }
                        }
                    }
                }

                i--;
            }

            if (!circleHovered)
                SelectedBody = null;
        }

        /// <summary>
        /// Returns to the main state.
        /// </summary>
        private void ReturnToMainState()
        {
            StateMachine.Instance.PopState();
        }

        /// <summary>
        /// Pushes the save replay state.
        /// </summary>
        private void PushSaveReplayState()
        {
            MainState mainState = StateMachine.Instance.FindState<MainState>();
            foreach (Robot robot in mainState.SpawnedRobots)
            {
                if (robot.RobotIsMixAndMatch)
                {
                    UserMessageManager.Dispatch("Cannot save replays with Mix and Match robots", 5);
                    return;
                }
            }
            StateMachine.Instance.PushState(new SaveReplayState(fieldPath, trackers, contactPoints));
        }

        /// <summary>
        /// Pops the replay state if the tab key is pressed.
        /// </summary>
        public override void Update()
        {
            if (Input.GetKeyDown(KeyCode.Tab))
            //if (InputControl.GetButtonDown(Controls.buttons[controlIndex].replayMode))
                StateMachine.Instance.PopState();
        }

        /// <summary>
        /// Updates the positions and rotations of each tracker's parent object according to the replay time.
        /// </summary>
        public override void LateUpdate()
        {
            if (dynamicCamera == null)
            {
                dynamicCamera = UnityEngine.Object.FindObjectOfType<DynamicCamera>();
                camera = dynamicCamera.GetComponent<Camera>();
            }

            if (InputControl.GetButtonDown(Controls.buttons[0].cameraToggle))
                dynamicCamera.ToggleCameraState(dynamicCamera.cameraState);

            if (firstFrame)
            {
                firstFrame = false;
            }
            else if (Input.GetKeyDown(KeyCode.Space))
            {
                if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
                {
                    rewindTime = Tracker.Lifetime;
                    playbackMode = PlaybackMode.Play;
                }
                else
                {
                    switch (playbackMode)
                    {
                        case PlaybackMode.Paused:
                            if (rewindTime == 0f)
                                rewindTime = Tracker.Lifetime;
                            playbackMode = PlaybackMode.Play;
                            break;
                        case PlaybackMode.Play:
                        case PlaybackMode.Rewind:
                            playbackMode = PlaybackMode.Paused;
                            break;
                    }
                }
            }

            switch (playbackMode)
            {
                case PlaybackMode.Rewind:
                    rewindTime += Time.deltaTime;
                    break;
                case PlaybackMode.Play:
                    rewindTime -= Time.deltaTime;
                    break;
            }

            if (Input.GetKey(KeyCode.LeftArrow))
            {
                rewindTime += Time.deltaTime * (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl) ? 0.05f : 0.25f);
                playbackMode = PlaybackMode.Paused;
            }

            if (Input.GetKey(KeyCode.RightArrow))
            {
                rewindTime -= Time.deltaTime * (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl) ? 0.05f : 0.25f);
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
                float replayTime = RewindFrame;
                int currentIndex = (int)Math.Floor(replayTime);

                StateDescriptor lowerState = t.States[currentIndex];
                StateDescriptor upperState = currentIndex < t.States.Length - 1 ? t.States[currentIndex + 1] : lowerState;

                float percent = replayTime - currentIndex;

                BRigidBody rb = t.GetComponent<BRigidBody>();

                if (!rb.GetCollisionObject().IsActive)
                    rb.GetCollisionObject().Activate();

                rb.SetPosition(BulletSharp.Math.Vector3.Lerp(lowerState.Position, upperState.Position, percent).ToUnity());
                rb.SetRotation(BulletSharp.Math.Matrix.Lerp(lowerState.Rotation, upperState.Rotation, percent).GetOrientation().ToUnity());
            }
        }

        /// <summary>
        /// Resets the trackers and reenables the RigidBodies.
        /// </summary>
        public override void End()
        {
            SelectedBody = null;
            //(SimUI.changeAnalytics.ToString());
            if (SimUI.changeAnalytics)
            {
                Analytics.CustomEvent("Replay Mode", new Dictionary<string, object>
                {
                    { "time", Time.time - tStart},
                });
            }

            foreach (Tracker t in trackers)
            {
                if (t.Trace)
                    UnityEngine.Object.Destroy(t.gameObject.GetComponent<LineRenderer>());

                StateDescriptor currentState = t.States[(int)Math.Floor(RewindFrame)];

                RigidBody r = (RigidBody)t.GetComponent<BRigidBody>().GetCollisionObject();
                r.LinearFactor = r.AngularFactor = BulletSharp.Math.Vector3.One;
                r.LinearVelocity = currentState.LinearVelocity;
                r.AngularVelocity = currentState.AngularVelocity;

                t.Clear();
            }
        }

        /// <summary>
        /// Consolidates the given contact with its surrounding contacts.
        /// </summary>
        /// <param name="contacts"></param>
        /// <param name="start"></param>
        /// <returns></returns>
        private void ConsolidateContacts(List<ContactDescriptor> contacts, ContactDescriptor start)
        {
            contacts.Remove(start);

            List<ContactDescriptor> removedContacts = new List<ContactDescriptor>();

            BulletSharp.Math.Vector3 lastPoint = start.Position;

            int startIndex = contactPoints.IndexOf(contacts);
            int lowerBound = startIndex;

            for (; lowerBound >= 0; lowerBound--)
            {
                List<ContactDescriptor> lc = GetValidContacts(lowerBound, lastPoint, ref start);

                int numContacts = lc.Count;

                if (numContacts == 0)
                {
                    if (lowerBound == startIndex)
                        continue;

                    break;
                }

                lastPoint = BulletSharp.Math.Vector3.Zero;

                foreach (ContactDescriptor c in lc)
                {
                    lastPoint += c.Position;
                    removedContacts.Add(c);
                }

                lastPoint /= numContacts;
            }

            for (int i = startIndex; i >= lowerBound; i--)
            {
                List<ContactDescriptor> currentContacts = contactPoints[i];

                if (currentContacts == null)
                    continue;

                currentContacts.RemoveAll((x) => removedContacts.Contains(x));
            }

            contacts.Add(start);
        }

        /// <summary>
        /// Finds all nearby contacts within the consolidation epsilon.
        /// </summary>
        /// <param name="index"></param>
        /// <param name="point"></param>
        /// <param name="contact"></param>
        /// <returns></returns>
        private List<ContactDescriptor> GetValidContacts(int index, BulletSharp.Math.Vector3 point, ref ContactDescriptor contact)
        {
            List<ContactDescriptor> currentContacts = contactPoints[index];
            List<ContactDescriptor> validContacts = new List<ContactDescriptor>();

            if (currentContacts == null)
                return validContacts;

            foreach (ContactDescriptor c in currentContacts)
            {
                if (c.RobotBody != contact.RobotBody || c.Equals(contact) || (c.Position - point).Length > ConsolidationEpsilon)
                    continue;

                contact.AppliedImpulse += c.AppliedImpulse;
                validContacts.Add(c);
            }

            return validContacts;
        }

        /// <summary>
        /// Creates a button from the given texture name.
        /// </summary>
        /// <param name="textureName"></param>
        /// <returns></returns>
        private GUIStyle CreateButtonStyle(string textureName)
        {
            Texture2D normalTexture = (Texture2D)Resources.Load("Images/" + textureName);
            Texture2D hoverTexture = (Texture2D)Resources.Load("Images/" + textureName + "Hover");
            Texture2D pressedTexture = (Texture2D)Resources.Load("Images/" + textureName + "Pressed");

            return new GUIStyle
            {
                normal = new GUIStyleState { background = normalTexture },
                hover = new GUIStyleState { background = hoverTexture },
                active = new GUIStyleState { background = pressedTexture }
            };
        }
    }
}