using System.Collections;
using System.Collections.Generic;
using Synthesis.UI.Dynamic;
using UnityEngine;
using UnityEngine.UI;
using SynthesisAPI.Utilities;
using TMPro;

public class ToastModal : ModalDynamic
{
    public ToastModal() : base(new Vector2(500, 500)) { }
    public static string toastText = "";
    public static LogLevel toastLevel = LogLevel.Info;
    public override void Create()
    {
        Title.SetStretch<Content>();
        Title.SetText("    " + toastLevel.ToString() + " Message: ").SetFontSize(30);
        AcceptButton.RootGameObject.SetActive(false);
        Description.RootGameObject.SetActive(false);
        ModalImage.RootGameObject.SetActive(false);
        CancelButton.Label.SetText("Close");

        var sv = MainContent.CreateScrollView().SetStretch<ScrollView>();
        var content = sv.Content.CreateSubContent(new Vector2(sv.Content.Size.x, 0));
        sv.Content.RootGameObject.AddComponent<VerticalLayoutGroup>().padding = new RectOffset(10, 10, 10, 10);
        sv.Content.RootGameObject.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        var label = content.CreateLabel(0);
        label.SetText(toastText).SetVerticalAlignment(TMPro.VerticalAlignmentOptions.Top);
        label.SetFontStyle(TMPro.FontStyles.Normal);
        label.SetTopStretch<Content>();
        label.SetFont(SynthesisAssetCollection.Instance.Fonts[1]);
        label.RootGameObject.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        label.RootGameObject.transform.SetParent(sv.Content.RootGameObject.transform);

        GameObject.Destroy(content.RootGameObject);
    }

    public override void Delete() { }

    public override void Update() { }
}
