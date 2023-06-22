using System.Collections;
using UnityEngine;

namespace Synthesis.UI.Panels {
    public class Panel : MonoBehaviour {
        public virtual void Close() {
            Destroy(gameObject);
        }
    }
}
