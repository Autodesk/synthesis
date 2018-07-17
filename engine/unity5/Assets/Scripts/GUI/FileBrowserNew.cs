using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using Crosstales.FB;

namespace Crosstales.FB.Demo
{
    public class FileBrowserNew : MonoBehaviour
    {

        GameObject ScrollView;
        GameObject TextPrefab;

        public void OpenSingleFolder()
        {
            //Debug.Log("OpenSingleFolder");

            string path = FB.FileBrowserNew.OpenSingleFolder("Open Folder", Environment.SpecialFolder.ApplicationData + "//synthesis//Fields");

            //Debug.Log("Selected folder: " + path);

            RebuildList(path);
        }

        public void OpenFiles()
        {
            //Debug.Log("OpenFiles");

            /*
            var extensions = new[] {
                new ExtensionFilter("Image Files", "png", "jpg", "jpeg" ),
                new ExtensionFilter("Sound Files", "mp3", "wav" ),
                new ExtensionFilter("All Files", "*" ),
            };
            */

            string extensions = "";

            string[] paths = FileBrowserNew.OpenFiles("Open Files", "", .bxdf, true);

            /*
            foreach (string path in paths)
            {
                Debug.Log("Selected file: " + path);
            }
            */

            RebuildList(paths);
        }


        private void RebuildList(params string[] e)
        {
            for (int ii = ScrollView.transform.childCount - 1; ii >= 0; ii--)
            {
                Transform child = ScrollView.transform.GetChild(ii);
                child.SetParent(null);
                Destroy(child.gameObject);
            }

            ScrollView.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 80 * e.Length);

            for (int ii = 0; ii < e.Length; ii++)
            {
                //if (Config.DEBUG)
                //    Debug.Log(e[ii]);

                GameObject go = Instantiate(TextPrefab);

                go.transform.SetParent(ScrollView.transform);
                go.transform.localScale = Vector3.one;
                go.transform.localPosition = new Vector3(10, -80 * ii, 0);
                go.GetComponent<Text>().text = e[ii].ToString();
            }
        }
    }
}
