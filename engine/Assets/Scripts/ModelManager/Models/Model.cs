using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Synthesis.ModelManager.Models
{
    public class Model : MonoBehaviour
    {
        public string ObjectName { get; set; }
        public string InstanceName { get => gameObject.name; set => gameObject.name = value; }

        public float Mass { get; private set; }

        protected HashSet<Motor> motors = new HashSet<Motor>();
        public IEnumerable<Motor> Motors => gameObject.GetComponents<Motor>();

        void Awake()
        {
            if (gameObject.GetComponent<Motor>() != null)
                return;
            foreach (Transform t in gameObject.transform)
            {
                if (t.GetComponent<Rigidbody>() != null)
                    Mass += t.GetComponent<Rigidbody>().mass;
                if (t.GetComponent<HingeJoint>() != null)
                    gameObject.AddComponent<Motor>().Joint = t.GetComponent<HingeJoint>();
            }
        }

        public void Show() => gameObject.SetActive(true);

        private static int counter = 0;
        public bool AddMotor(HingeJoint joint)
        {
            Motor m = GameObject.AddComponent<Motor>();
            m.Joint = joint;
            m.Meta = ($"Motor {counter}", counter.ToString());
            counter++;
            joint.name = m.Meta.Name;
            return motors.Add(m);
        }
        public void Hide() => gameObject.SetActive(false);
    }
}