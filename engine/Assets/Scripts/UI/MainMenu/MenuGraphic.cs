using UnityEngine;


public class MenuGraphic : MonoBehaviour {

    public void Start() {
        float rat = (float) Screen.width / (float) Screen.height;
        this.gameObject.SetActive(rat > 1.61);
    }

}
