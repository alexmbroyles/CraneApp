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
    /// Global variables for this application
    /// </summary>
    class globals
    {
        public static ObservableCollection<Activity3D> ActivityList3D;
        public static ObservableCollection<ObservableCollection<Activity3D>> OptimizedActivityList;

        //Going to try to find a way to remove geometry when a move is edited or if a move is not drawn correctly.
        //Child items of this 3d viewport lose their identity; these two lists (for tubes and spheres respectively)
        //Will attempt to provide accounting for what is added.
        public static ObservableCollection<TubeVisual3D> TubeChildren; 
        public static ObservableCollection<ModelVisual3D> SphereChildren;
        public static string SaveAsFile;
        public static string LoadFromFile;
        public static Point3D Orig = new Point3D(0, 0, 0);
        public static double CraneRad = 215.0;
    }
}
