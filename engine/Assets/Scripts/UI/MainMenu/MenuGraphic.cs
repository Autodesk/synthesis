using UnityEngine;

[ExecuteAlways]
public class MenuGraphic : MonoBehaviour {
    [SerializeField]
    private float _maxRatio;

    public void OnRectTransformDimensionsChange() {
        float rat = (float) Screen.width / (float) Screen.height;
        this.gameObject.SetActive(rat > _maxRatio);
    }
}
