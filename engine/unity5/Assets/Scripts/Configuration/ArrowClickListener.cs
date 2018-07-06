using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Synthesis.Configuration
{
    public class ArrowClickListener : MonoBehaviour
    {
        private ArrowType arrowType;

        private void Start()
        {
            if (!Enum.TryParse(name, out arrowType))
                arrowType = ArrowType.None;
        }

        private void OnMouseDown()
        {
            SendMessageUpwards("OnArrowSelected", arrowType);
        }

        private void OnMouseUp()
        {
            SendMessageUpwards("OnArrowReleased");
        }
    }
}
