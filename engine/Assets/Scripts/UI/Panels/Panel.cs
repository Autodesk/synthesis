using UnityEngine;
using System.Collections;

namespace Synthesis.UI.Panels {
    public class Panel : MonoBehaviour {
        public virtual void Close() {
            Destroy(gameObject);
        }
    }
}
