using BulletUnity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Synthesis.Configuration
{
    public class MoveArrows : MonoBehaviour 
    {
        private const float Scale = 0.075f;
        private Vector3 initialScale;
        private ArrowType activeArrow;

        private void Awake()
        {
            activeArrow = ArrowType.None;
        }

        private void Start()
        {
            GameObject arrowsPrefab = Resources.Load<GameObject>("Prefabs\\MoveArrows");
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;

            initialScale = transform.localScale;
        }

        private void LateUpdate()
        {
            Plane plane = new Plane(UnityEngine.Camera.main.transform.forward, UnityEngine.Camera.main.transform.position);
            float dist = plane.GetDistanceToPoint(transform.position);
            transform.localScale = initialScale * Scale * dist;
        }

        private void OnArrowSelected(object arrowType)
        {
            activeArrow = (ArrowType)arrowType;
        }

        private void OnArrowReleased()
        {
            activeArrow = ArrowType.None;
        }
    }
}
