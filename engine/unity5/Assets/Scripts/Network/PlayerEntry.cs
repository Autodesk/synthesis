using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace Synthesis.Network
{
    public class PlayerEntry : MonoBehaviour
    {
        public PlayerIdentity PlayerIdentity { get; set; }

        private Text playerTagText;

        private void Awake()
        {
            playerTagText = transform.Find("PlayerTagText").GetComponent<Text>();
        }

        private void OnGUI()
        {
            playerTagText.text = PlayerIdentity.PlayerTag;
        }
    }
}
