using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using Autodesk.DesignScript.Runtime;
using Point = Autodesk.DesignScript.Geometry.Point;

namespace InventorLibrary.ModulePlacement
{
    [IsVisibleInDynamoLibrary(false)]
    public class ModulePoints : IPointsList//, INotifyPropertyChanged
    {
        List<List<Point>> pointsList;
        List<List<Point>> oldPointsList;
        public event PropertyChangedEventHandler PropertyChanged;


        public ModulePoints(List<List<Point>> pointsList)
        {
            this.pointsList = pointsList;
        }

        public List<List<Point>> PointsList 
        {
            get { return pointsList; }
            set
            {
                pointsList = value;
                //OnPropertyChanged("PointsList");
            }
        }

        public List<List<Point>> OldPointsList { get; set; }

        public bool IsDirty 
        {
            get
            {
                if (PointsList == OldPointsList)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
        }
   
        private void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}

//namespace InventorLibrary.ModulePlacement
//{
//    [IsVisibleInDynamoLibrary(false)]
//    public class ModulePoints : IPointsList, INotifyPropertyChanged
//    {
//        List<List<Point>> pointsList;
//        List<List<Point>> oldPointsList;
//        public event PropertyChangedEventHandler PropertyChanged;

//        public List<List<Point>> PointsList
//        {
//            get { return pointsList; }
//            set
//            {
//                pointsList = value;
//                OnPropertyChanged("PointsList");
//            }
//        }

//        public List<List<Point>> OldPointsList { get; set; }

//        public bool IsDirty
//        {
//            get
//            {
//                if (PointsList == OldPointsList)
//                {
//                    return false;
//                }
//                else
//                {
//                    return true;
//                }
//            }
//        }

//        private void OnPropertyChanged(string propertyName)
//        {
//            PropertyChangedEventHandler handler = PropertyChanged;
//            if (handler != null)
//            {
//                handler(this, new PropertyChangedEventArgs(propertyName));
//            }
//        }
//    }
//}

