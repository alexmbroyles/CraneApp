using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using HelixToolkit.Wpf;
using HelixToolkit;
using System.Collections.ObjectModel;
using System.Windows.Media.Media3D;
using System.Reflection;


namespace CraneApp
{
    /// <summary>
    /// The data classes that I will be using for this app.
    /// </summary>
    [Serializable]
    public class Activity3D
    {
        public int sequence { get; set; }
        public string ActivityID { get; set; }
        public string Sysem { get; set; }
        public string ActivityDescription { get; set; }
        public int MustAppearBefore { get; set; }
        public int MustAppearAfter { get; set; }
        public bool CanChangeSequence { get; set; }
        public int Priority { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public double StartX { get; set; }
        public double StartY { get; set; }
        public double StartZ { get; set; }
        public double EndX { get; set; }
        public double EndY { get; set; }
        public double EndZ { get; set; }
        public double TotalDistance { get; set; }
        public List<SubActivity3D> SubMoves { get; set; }


    }
    [Serializable]
    public class SubActivity3D
    {
        public Point3D StartPoint { get; set; }
        public Point3D EndPoint { get; set; }
        public double Distance { get; set; }
    }
}
