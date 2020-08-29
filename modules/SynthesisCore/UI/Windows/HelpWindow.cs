using SynthesisAPI.AssetManager;
using SynthesisAPI.UIManager;
using SynthesisAPI.UIManager.UIComponents;
using SynthesisAPI.UIManager.VisualElements;
using SynthesisAPI.Utilities;

namespace SynthesisCore.UI.Windows
{
    public class HelpWindow
    {
        public Panel Panel { get; }
        private VisualElement Window;
        private ListView HelpList;

        public HelpWindow()
        {
            var helpWindowAsset = AssetManager.GetAsset<VisualElementAsset>("/modules/synthesis_core/UI/uxml/Help.uxml");
            Panel = new Panel("Help", helpWindowAsset, OnWindowOpen);

            Button helpButton = (Button)UIManager.RootElement.Get("help-button");
            helpButton.Subscribe(x => UIManager.TogglePanel("Help"));
        }

        private void OnWindowOpen(VisualElement helpWindow)
        {
            Window = helpWindow;
            HelpList = (ListView)Window.Get("help-list");

            LoadWindowContents();
            RegisterButtons();
        }

        private void LoadWindowContents()
        {
            Button gettingStartedButton = (Button)Window.Get("getting-started-button");
            gettingStartedButton?.Subscribe(_ => System.Diagnostics.Process.Start("https://synthesis.autodesk.com/tutorials.html"));
            
            Button communityForumButton = (Button)Window.Get("community-forum-button");
            communityForumButton?.Subscribe(_ => System.Diagnostics.Process.Start("https://forums.autodesk.com/t5/bxd-synthesis-forum/bd-p/99"));

            Button gitHubButton = (Button)Window.Get("github-button");
            gitHubButton?.Subscribe(_ => System.Diagnostics.Process.Start("https://github.com/Autodesk/synthesis"));
            
            Button reportAnIssueButton = (Button)Window.Get("report-an-issue-button");
            reportAnIssueButton?.Subscribe(_ => System.Diagnostics.Process.Start("https://github.com/Autodesk/synthesis/issues/new/choose"));

            Button submitAnIdeaButton = (Button)Window.Get("submit-an-idea-button");
            submitAnIdeaButton?.Subscribe(_ => System.Diagnostics.Process.Start("https://forums.autodesk.com/t5/bxd-synthesis-ideas/idb-p/104"));

            Button aboutButton = (Button)Window.Get("about-button");
            aboutButton?.Subscribe(_ => System.Diagnostics.Process.Start("https://synthesis.autodesk.com/about.html"));
        }

        private void RegisterButtons()
        {
            Button closeButton = (Button)Window.Get("close-button");
            closeButton?.Subscribe(x => UIManager.ClosePanel(Panel.Name));
        }
    }
}
