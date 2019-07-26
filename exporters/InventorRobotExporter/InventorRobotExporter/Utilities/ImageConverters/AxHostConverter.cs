using System.Drawing;
using System.Windows.Forms;

namespace InventorRobotExporter.Utilities.ImageFormat
{
    internal class AxHostConverter : AxHost
    {
        private AxHostConverter() : base("")
        {
        }

        public static stdole.IPictureDisp ImageToPictureDisp(Image image)
        {
            return (stdole.IPictureDisp) GetIPictureDispFromPicture(image);
        }

        public static Image PictureDispToImage(stdole.IPictureDisp pictureDisp)
        {
            return GetPictureFromIPicture(pictureDisp);
        }
    }
}