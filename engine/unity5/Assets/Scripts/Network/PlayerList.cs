//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using UnityEngine;
//using UnityEngine.UI;

//namespace Synthesis.Network
//{
//    public class PlayerList : MonoBehaviour
//    {
//        /// <summary>
//        /// The global <see cref="PlayerList"/> instance.
//        /// </summary>
//        public static PlayerList Instance { get; private set; }

//        private Dictionary<PlayerIdentity, PlayerEntry> entries;

//        /// <summary>
//        /// Initializes this instance.
//        /// </summary>
//        private void Awake()
//        {
//            Instance = this;
//            entries = new Dictionary<PlayerIdentity, PlayerEntry>();
//        }

//        /// <summary>
//        /// Adds a <see cref="PlayerIdentity"/> to the dictionary of tracked <see cref="PlayerIdentity"/> instances.
//        /// </summary>
//        /// <param name="identity"></param>
//        public void AddPlayerEntry(PlayerIdentity identity)
//        {
//            GameObject newEntry = (GameObject)Instantiate(Resources.Load("Prefabs/PlayerEntry"), gameObject.transform);

//            PlayerEntry entry = newEntry.GetComponent<PlayerEntry>();
//            entry.PlayerIdentity = identity;

//            entries.Add(identity, entry);
//        }

//        /// <summary>
//        /// Removes a <see cref="PlayerIdentity"/> from the dictionary of tracked <see cref="PlayerIdentity"/> instances.
//        /// </summary>
//        /// <param name="identity"></param>
//        public void RemovePlayerEntry(PlayerIdentity identity)
//        {
//            if (!entries[identity].Equals(null))
//                Destroy(entries[identity].gameObject);

//            entries.Remove(identity);
//        }
//    }
//}
