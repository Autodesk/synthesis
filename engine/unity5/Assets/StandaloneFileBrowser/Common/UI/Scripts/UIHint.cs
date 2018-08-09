using System.Collections;
using UnityEngine;

namespace Crosstales.UI
{
    /// <summary>Controls a UI group (hint).</summary>
    public class UIHint : MonoBehaviour
    {

        #region Variables

        /// <summary>Group to fade.</summary>
        [Tooltip("Group to fade.")]
        public CanvasGroup Group;

        /// <summary>Delay in seconds before fading (default: 2).</summary>
        [Tooltip("Delay in seconds before fading (default: 2).")]
        public float Delay = 2f;

        /// <summary>Fade time in seconds (default: 2).</summary>
        [Tooltip("Fade time in seconds (default: 2).")]
        public float FadeTime = 2f;

        /// <summary>Destroy UI element after the fade (default: true).</summary>
        [Tooltip("Destroy UI element after the fade (default: true).")]
        public bool DestroyWhenFinished = true;

        #endregion


        #region MonoBehaviour methods

        public void Start()
        {
            StartCoroutine(fadeTo(0f, Delay, FadeTime, DestroyWhenFinished));
        }

        #endregion


        #region Private methods

        private IEnumerator fadeTo(float aValue, float delay, float aTime, bool destroy)
        {
            yield return new WaitForSeconds(delay);

            float alpha = Group.alpha;

            for (float t = 0f; t < 1f; t += Time.deltaTime / aTime)
            {
                //Debug.Log(Group.alpha + " - " + t);

                Group.alpha = Mathf.Lerp(alpha, aValue, t);
                yield return null;
            }

            if (destroy)
                Destroy(gameObject, .5f);
        }

        #endregion
    }
}
// © 2018 crosstales LLC (https://www.crosstales.com)