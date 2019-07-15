using System.Drawing;
using System.Windows.Forms;

namespace BxDRobotExporter.JointEditor
{
    internal class AxHostConverter : AxHost

    {
        private AxHostConverter() : base("")
        {
        }

        static public stdole.IPictureDisp ImageToPictureDisp(Image image)
        {
            return (stdole.IPictureDisp) GetIPictureDispFromPicture(image);
        }

        static public Image PictureDispToImage(stdole.IPictureDisp pictureDisp)
        {
            return GetPictureFromIPicture(pictureDisp);
        }
    }
}