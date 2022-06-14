using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SynthesisAPI.Simulation;

namespace Synthesis.UI.Panels {
    public class MultiplayerPanel : Panel {
        [SerializeField]
        public GameObject RobotButtonsContainer;
        [SerializeField]
        public GameObject RobotSelectionPrefab;
        private Color disabledColor = new Color(0.5f, 0.5f, 0.5f, 1f);
        private Color enabledColor = new Color(0.1294118f, 0.5882353f, 0.9529412f, 1f);

        private List<MultiplayerRobotItem> _items = new List<MultiplayerRobotItem>();

        private void Start() {
            PopulateOptions();
        }

        private void UpdateOptions() {
            _items.Clear();
            foreach (var t in RobotButtonsContainer.GetComponentsInChildren<Transform>()) {
                if (t != RobotButtonsContainer.transform)
                    Destroy(t.gameObject);
            }
            PopulateOptions();
        }
        private void PopulateOptions() {
            GameObject selected = null;
            SimulationManager._simulationObject.ForEach(x => {
                if (x.Value is RobotSimObject) {
                    var itemObj = Instantiate(RobotSelectionPrefab, RobotButtonsContainer.transform);
                    var item = itemObj.GetComponent<MultiplayerRobotItem>();
                    item.SetColor(disabledColor);
                    item.Init(x.Value.Name, () => Select(itemObj));
                    if (x.Value.Name == RobotSimObject.CurrentlyPossessedRobot)
                        selected = itemObj;
                    _items.Add(item);
                }
            });
            if (selected != null)
                Select(selected, true);
        }

        public void Select(GameObject robotItem) {
            Select(robotItem, false);
        }

        public void Select(GameObject robotItem, bool visualOnly = false) {

            if (!visualOnly && robotItem.GetComponent<MultiplayerRobotItem>().Name == RobotSimObject.CurrentlyPossessedRobot)
                return;

            MultiplayerRobotItem item = null;
            _items.ForEach(x => {
                if (x.gameObject == robotItem) {
                    item = x;
                    x.SetColor(enabledColor);
                } else {
                    x.SetColor(disabledColor);
                }
            });
            if (item == null)
                throw new System.Exception();

            if (!visualOnly)
                (SimulationManager._simulationObject[item.Name] as RobotSimObject).Possess();
        }

        public void Delete() {
            SimulationManager.RemoveSimObject(RobotSimObject.CurrentlyPossessedRobot);
            UpdateOptions();
        }
    }
}
