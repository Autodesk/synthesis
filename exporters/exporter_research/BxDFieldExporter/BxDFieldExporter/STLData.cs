using System;
using System.Windows.Forms;

namespace ExportProcess {
    public class STLData {
        #region Properties
        private int ID;
        private byte[] data;
        private float[,] translationData;
        #endregion
        public STLData(int ID, byte[] data, float[,] translationData) {
            this.ID = ID;
            this.data = data;
            this.translationData = translationData;
        }
        public STLData() {

        }
        public int getID() {
            try {
                return ID;
            }
            catch (Exception e) {
                MessageBox.Show(e.Message);
                return new int();
            }
        }
        public byte[] getData() {
            try {
                return data;
            }
            catch (Exception e) {
                MessageBox.Show(e.Message);
                return null;
            }
        }
        public float[,] getTranslationData() {
            try {
                return translationData;
            }
            catch (Exception e) {
                MessageBox.Show(e.Message);
                return null;
            }
        }
    }
}
