from ..general_imports import gm, A_EP
from ..configure import setAnalytics


def showAnalyticsAlert():
    ret = "<html><h1> Unity File Exporter Addin </h1>\n<hr>"
    ret += f"<font size=5>The Unity File Exporter would like to collect anonymous analytics information from you. You can find the privacy policy at the following address to review the data that is collected for the individual user session and how that data is used.</font> \n<ul>"
    ret += f"<li><a href=https://myshare.autodesk.com/:b:/g/personal/shawn_hice_autodesk_com/EZ9qyyVyIKpOn8jHqJQrK8MBPvHB_n2K0c0PnTc_lJpzXA?e=YLTBbS><font size=5>Privacy Policy Document</font></a></li>"
    ret += "</ul>\n"
    ret += "<font size=5>May we collect basic usage information to help improve the user workflow and catch problems?</font></html>"

    res = gm.ui.messageBox(
        ret,
        "Unity File Exporter Analytics",
        3,  # This is yes/no
        1,  # This is question icon
    )

    setAnalytics(True) if res == 2 else setAnalytics(False)
