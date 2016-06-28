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
    public interface IPointsList : INotifyPropertyChanged
    {
        List<List<Point>> PointsList { get; set; }
        List<List<Point>> OldPointsList { get; set; }
        bool IsDirty { get; }
        event PropertyChangedEventHandler PropertyChanged;
    }
}
