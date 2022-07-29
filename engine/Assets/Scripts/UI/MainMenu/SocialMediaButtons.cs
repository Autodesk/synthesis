using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public class SocialMediaButtons : MonoBehaviour {
    public void Twitter() {
        Process.Start(new ProcessStartInfo() {
            FileName = "https://twitter.com/synthesis_adsk",
            UseShellExecute = true
        });
    }

    public void Youtube() {
        Process.Start(new ProcessStartInfo() {
            FileName = "https://www.youtube.com/channel/UCqblCh9SOPyLcdMwMiHWR7g",
            UseShellExecute = true
        });
    }

    public void Discord() {
        Process.Start(new ProcessStartInfo() {
            FileName = "https://www.discord.gg/hHcF9AVgZA",
            UseShellExecute = true
        });
    }

    public void Instagram() {
        Process.Start(new ProcessStartInfo() {
            FileName = "https://www.instagram.com/synthesis.adsk/",
            UseShellExecute = true
        });
    }

    public void Facebook() {
        Process.Start(new ProcessStartInfo() {
            FileName = "https://www.facebook.com/synthesis.adsk/",
            UseShellExecute = true
        });
    }
}
