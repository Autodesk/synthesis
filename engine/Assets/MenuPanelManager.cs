using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Synthesis.UI
{
    //made for alternative UI design
    public class MenuPanelManager : MonoBehaviour
    {

        //this script is attached to the mask object
        public void OpenPanel(Animator animator)
        {
            bool isOpen = animator.GetBool("open");
            animator.SetBool("open", !isOpen);
        }
    }
}