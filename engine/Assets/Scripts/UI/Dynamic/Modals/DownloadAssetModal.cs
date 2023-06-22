using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

namespace Synthesis.UI.Dynamic {
    public class DownloadAssetModal : ModalDynamic {
        private const string MANIFEST_URL   = "https://synthesis.autodesk.com/Downloadables/Mira/manifest.json";
        private const string BASE_ASSET_URL = "https://synthesis.autodesk.com/Downloadables/Mira/";
        private JObject manifest;
        private bool manifestWasNull = true;

        private string _robotDir;
        private string _fieldDir;

        private List<string> _downloadedRobots;
        private List<string> _downloadedFields;

        private const float VERTICAL_PADDING = 16f;
        private const int CONTENT_HEIGHT     = 400;

        private ScrollView robotScrollView;
        private ScrollView fieldScrollView;

        private WebClient client;

        private readonly Func<UIComponent, UIComponent> VerticalLayout = (u) => {
            var offset = (-u.Parent!.RectOfChildren(u).yMin) + VERTICAL_PADDING;
            u.SetTopStretch<UIComponent>(anchoredY: offset, leftPadding: 0, rightPadding: 0);
            return u;
        };

        public DownloadAssetModal() : base(new Vector2(1200, CONTENT_HEIGHT + 30)) {
        }

        private async void GetAvailableAssets() {
            HttpClient httpClient        = new HttpClient();
            HttpResponseMessage response = await httpClient.GetAsync(MANIFEST_URL);
            string content               = await response.Content.ReadAsStringAsync();
            manifest                     = JObject.Parse(content);
        }

        public override void Create() {
            Task.Run(GetAvailableAssets);

            Title.SetText("Download Assets");
            Description.SetText("Download robots and fields from the Synthesis Asset Library.");

            AcceptButton.StepIntoLabel(l => l.SetText("Close"))
                .AddOnClickedEvent(b => DynamicUIManager.CloseActiveModal());
            CancelButton.RootGameObject.SetActive(false);

            _robotDir = AddRobotModal.ParsePath(Path.Combine("$appdata/Autodesk/Synthesis", "Mira"), '/');
            if (!Directory.Exists(_robotDir)) {
                Directory.CreateDirectory(_robotDir);
            }

            _downloadedRobots = Directory.GetFiles(_robotDir)
                                    .Where(x => Path.GetExtension(x).Equals(".mira"))
                                    .Map<string>(x => x.Split('/').Last());

            _fieldDir = AddRobotModal.ParsePath(Path.Combine("$appdata/Autodesk/Synthesis", "Mira", "Fields"), '/');
            if (!Directory.Exists(_fieldDir)) {
                Directory.CreateDirectory(_fieldDir);
            }

            _downloadedFields = Directory.GetFiles(_fieldDir)
                                    .Where(x => Path.GetExtension(x).Equals(".mira"))
                                    .Map<string>(x => x.Split('/').Last());

            (Content leftContent, Content rightContent) = MainContent.SplitLeftRight(580, 20);
            leftContent.CreateLabel()
                .SetText("Robots")
                .ApplyTemplate(VerticalLayout)
                .SetHorizontalAlignment(HorizontalAlignmentOptions.Center);

            robotScrollView =
                leftContent.CreateScrollView().SetHeight<ScrollView>(CONTENT_HEIGHT).ApplyTemplate(VerticalLayout);

            robotScrollView.RootGameObject.GetComponent<UnityEngine.UI.Image>().color = Color.clear;

            rightContent.CreateLabel()
                .SetText("Fields")
                .ApplyTemplate(VerticalLayout)
                .SetHorizontalAlignment(HorizontalAlignmentOptions.Center);

            fieldScrollView =
                rightContent.CreateScrollView().SetHeight<ScrollView>(CONTENT_HEIGHT).ApplyTemplate(VerticalLayout);

            fieldScrollView.RootGameObject.GetComponent<UnityEngine.UI.Image>().color = Color.clear;
        }

        private void DownloadAsset(string url, string path) {
            if (client == null) {
                client = new WebClient();
            }

            client.DownloadFile(url, path);
        }

        private void DownloadRobot(string robotName) {
            DownloadAsset(BASE_ASSET_URL + "Robots/" + robotName,
                AddRobotModal.ParsePath(Path.Combine("$appdata/Autodesk/Synthesis", "Mira", robotName), '/'));
            _downloadedRobots.Add(robotName);
        }

        private void DownloadField(string fieldName) {
            DownloadAsset(BASE_ASSET_URL + "Fields/" + fieldName,
                AddRobotModal.ParsePath(Path.Combine("$appdata/Autodesk/Synthesis", "Mira", "Fields", fieldName), '/'));
            _downloadedFields.Add(fieldName);
        }

        private void CreateAssetEntries() {
            string[] robots = manifest["robots"].Map(token => token.ToString()).ToArray();
            string[] fields = manifest["fields"].Map(token => token.ToString()).ToArray();

            foreach (string robotName in robots) {
                bool alreadyDownloaded = _downloadedRobots.Exists(x => Path.GetFileName(x).Equals(robotName));
                var downloadButton =
                    robotScrollView.Content.CreateLabeledButton()
                        .StepIntoLabel(l => l.SetText(robotName.Split('.')[0]))
                        .StepIntoButton(
                            b => b.StepIntoLabel(l => l.SetText("Download" + (alreadyDownloaded ? "ed" : ""))))
                        .ApplyTemplate(VerticalLayout);

                if (!alreadyDownloaded) {
                    downloadButton.Button.AddOnClickedEvent(b => {
                        DownloadRobot(robotName);
                        b.Label.SetText("Downloaded");
                    });
                }
            }

            foreach (string fieldName in fields) {
                bool alreadyDownloaded = _downloadedFields.Exists(x => Path.GetFileName(x).Equals(fieldName));
                var downloadButton =
                    fieldScrollView.Content.CreateLabeledButton()
                        .StepIntoLabel(l => l.SetText(fieldName.Split('.')[0]))
                        .StepIntoButton(
                            b => b.StepIntoLabel(l => l.SetText("Download" + (alreadyDownloaded ? "ed" : ""))))
                        .ApplyTemplate(VerticalLayout);

                if (!alreadyDownloaded) {
                    downloadButton.Button.AddOnClickedEvent(b => {
                        DownloadField(fieldName);
                        b.Label.SetText("Downloaded");
                    });
                }
            }
        }

        public override void Update() {
            if (manifest != null && manifestWasNull) {
                CreateAssetEntries();
                manifestWasNull = false;
            }
        }

        public override void Delete() {
        }
    }
}
