using System;
using Synthesis.UI.Dynamic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadingScreenModal : ModalDynamic
{
    
    public LoadingScreenModal() : base(new Vector2(300, -80)) {}

    public override void Create()
    {
        Title.SetText("Loading...").SetFontSize(40);
        Description.SetText("");
        
        AcceptButton.RootGameObject.SetActive(false);
        CancelButton.RootGameObject.SetActive(false);
    }
    
    public override void Update() {}

    public override void Delete()
    {
    }
}