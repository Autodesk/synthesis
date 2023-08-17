using Synthesis.UI.Dynamic;
using UnityEngine;
using UnityEngine.UI;
using SynthesisAPI.Utilities;
using System.IO;
using UI.Dynamic.Modals.Spawning;
using Logger = SynthesisAPI.Utilities.Logger;

public class ToastModal : ModalDynamic {
    public ToastModal(string text, LogLevel level) : base(new Vector2(500, 500)) {
        _toastText  = text;
        _toastLevel = level;
    }

    private readonly string _toastText;
    private readonly LogLevel _toastLevel;

    public override void Create() {
        Title.SetStretch<Content>();

        Title.SetText("    " + _toastLevel + " Message: ").SetFontSize(30);
        AcceptButton
            .AddOnClickedEvent(
                _ => { WriteToFile(); })
            .Label!.SetText("Write File");
        Description.RootGameObject.SetActive(false);
        ModalIcon.RootGameObject.SetActive(false);
        CancelButton.Label!.SetText("Close");

        var sv      = MainContent.CreateScrollView().SetStretch<ScrollView>();
        var content = sv.Content.CreateSubContent(new Vector2(sv.Content.Size.x, 0));
        sv.Content.RootGameObject.AddComponent<VerticalLayoutGroup>().padding = new RectOffset(10, 10, 10, 10);
        sv.Content.RootGameObject.AddComponent<ContentSizeFitter>().verticalFit =
            ContentSizeFitter.FitMode.PreferredSize;

        var label = content.CreateLabel(0);
        label.SetText(_toastText).SetVerticalAlignment(TMPro.VerticalAlignmentOptions.Top);
        label.SetFontStyle(TMPro.FontStyles.Normal);
        label.SetTopStretch<Content>();
        label.SetFont(SynthesisAssetCollection.Instance.Fonts[1]);
        label.RootGameObject.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        label.RootGameObject.transform.SetParent(sv.Content.RootGameObject.transform);

        Object.Destroy(content.RootGameObject);
    }

    public override void Delete() {}

    public override void Update() {}

    private void WriteToFile() {
        string root = AddRobotModal.ParsePath(Path.Combine("$appdata/Autodesk/Synthesis", "Log Messages"), '/');

        try {
            if (!Directory.Exists(root)) {
                Directory.CreateDirectory(root);
            }

        } catch (IOException) {
        }

        // Write some text to the test.txt file
        StreamWriter writer = new StreamWriter(
            root + "/message_" + System.DateTime.Now.ToString().Replace('/', '-').Replace(' ', '_').Replace(':', '-') +
                ".txt",
            true);
        writer.WriteLine(_toastText);
        writer.Close();
        OpenFolder(root);
        Logger.Log("File written at path: " + root);
    }

    private void OpenFolder(string path) {
        path = path.Replace(@"/", @"\");
        System.Diagnostics.Process.Start("explorer.exe", "/select," + path);
    }
}
