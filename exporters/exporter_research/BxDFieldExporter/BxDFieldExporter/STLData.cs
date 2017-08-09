using System;
using System.Collections.Generic;
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
            try
            {
                List<byte> fullSTLData = new List<byte>();
                fullSTLData.AddRange(BitConverter.GetBytes(ID));
                for (int x = 0; x != 4; x++)
                {
                    for(int y = 0; y != 4; y++)
                    {
                        fullSTLData.AddRange(BitConverter.GetBytes(translationData[x, y]));
                    }
                }
                fullSTLData.AddRange(data);
                return fullSTLData.ToArray();
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
