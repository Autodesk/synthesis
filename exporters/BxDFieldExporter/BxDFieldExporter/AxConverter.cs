using System.Windows.Forms;
using System.Drawing;

namespace ExportProcess
{
    public class AxConverter : AxHost {
        private AxConverter() : base("") { }

        static public stdole.IPictureDisp ImageToPictureDisp(Image image) {
            return (stdole.IPictureDisp)GetIPictureDispFromPicture(image);
        }

        static public Image PictureDispToImage(stdole.IPictureDisp pictureDisp) {
            return GetPictureFromIPicture(pictureDisp);
        }
    }
}