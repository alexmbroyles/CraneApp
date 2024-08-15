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
using ServiceStack.Text;
using LinqToExcel;

namespace CraneApp.windows
{
    /// <summary>
    /// Interaction logic for OptimizedView.xaml
    /// </summary>
    public partial class OptimizedView : Window
    {
        bool ready;
        int currentIndex;
        bool drawMe;
        double sphereRad = 3.0;
        double craneheight = 100.0;

        RotateTransform3D rotX;
        RotateTransform3D rotY;
        RotateTransform3D rotZ;

        Visual3D childCrane;
        Visual3D childContainment;
        Visual3D childHook;


        System.Windows.Threading.DispatcherTimer UITimer;
        ObservableCollection<Activity3D> ThisCollection3D;
        Activity3D currentDrawing;
        Activity3D previousDrawing;
        public OptimizedView()
        {
            ready = false;
            InitializeComponent();
            drawMe = false;
            currentIndex = 0;
            var x = new HelixToolkit.Wpf.ModelImporter();
            string modelfile1 = @"models\Containment-2.obj";
            string modelfile2 = @"models\Crane Hook.obj";
            string modelfile3 = @"models\Crane.obj";
            if (!System.IO.File.Exists(modelfile1) || !System.IO.File.Exists(modelfile2) || !System.IO.File.Exists(modelfile3))
            {
                MessageBox.Show("Cannot find containment file at: \nThis program cannot run without this file.  Please locate the 3d file and retry.", "Fatal error", MessageBoxButton.OK, MessageBoxImage.Error);
                Application.Current.Shutdown();
            }
            Model3DGroup group = x.Load(modelfile1);
            var newVisual = new ModelVisual3D { Content = group };
            newVisual.SetName("Containment");
            GraphicViewOpt.Children.Add(newVisual);
            childContainment = GraphicViewOpt.Children.FirstOrDefault(c => c.GetName() == "Containment");

            Model3DGroup group3 = x.Load(modelfile3);
            var newVisual3 = new ModelVisual3D { Content = group3 };
            newVisual3.SetName("Crane");
            GraphicViewOpt.Children.Add(newVisual3);
            childCrane = GraphicViewOpt.Children.FirstOrDefault(c => c.GetName() == "Crane");
            GraphicViewOpt.Children.FirstOrDefault(c => c.GetName() == "Crane").Transform = new TranslateTransform3D(new Vector3D(0, 0, 0));



            Model3DGroup group2 = x.Load(modelfile2);
            var newVisual2 = new ModelVisual3D { Content = group2 };
            newVisual2.SetName("Hook");
            GraphicViewOpt.Children.Add(newVisual2);
            GraphicViewOpt.Children.FirstOrDefault(c => c.GetName() == "Hook").Transform = new TranslateTransform3D(new Vector3D(0, 0, 0));
            childHook = GraphicViewOpt.Children.FirstOrDefault(c => c.GetName() == "Hook");

            rotX = new RotateTransform3D(new AxisAngleRotation3D(new Vector3D(1, 0, 0), 5));
            rotY = new RotateTransform3D(new AxisAngleRotation3D(new Vector3D(0, 1, 0), 5));
            rotZ = new RotateTransform3D(new AxisAngleRotation3D(new Vector3D(0, 0, 1), 5));

            var moveCrane = new TranslateTransform3D(new Vector3D(0, 0, 0));
            childCrane.Transform = moveCrane;

        }
        bool moveCrane;
        private void UITimer_Tick(object sender, EventArgs e)
        {
            if(!drawMe) { return; }
            Activity3D A = ThisCollection3D[currentIndex];
            currentDrawing = A;
            //foreach(var sub in A.SubMoves)
            //{

            //}

            if (moveCrane)
            {
                ///Temp - remove this when done playing:
                Random rnd = new Random();
                int j = rnd.Next(0, 360);
                for (int i = 0; i < j; i++)
                {
                    var rot = new RotateTransform3D(new AxisAngleRotation3D(new Vector3D(0, 0, 1), i));
                    rot.CenterX = -0;
                    rot.CenterY = -0;
                    rot.CenterZ = 0;
                    
                    childCrane.Transform = rot;
                    
                   // var CraneMove = new TranslateTransform3D(new Vector3D(0, 0, 0));
                   // // childCrane.Traverse(new Point3D(0, 0, 0));
                   // CraneMove.OffsetX = -150;
                   // CraneMove.OffsetY = -90;
                   // CraneMove.OffsetZ = 0;
                   // childCrane.Transform = CraneMove;


                    moveCrane = false;
                }
                return;
            }

            /// end temp


            // Add sphere for current point
            var meshBuilder = new MeshBuilder(false, false);
            meshBuilder.AddSphere(new Point3D(A.StartX, A.StartY, A.StartZ), sphereRad, 10, 10);
            meshBuilder.AddSphere(new Point3D(A.EndX, A.EndY, A.EndZ), sphereRad, 10, 10);
            var mesh = meshBuilder.ToMesh(true);
            var modelGroup = new Model3DGroup();
            var greenMaterial = MaterialHelper.CreateMaterial(Colors.Black);
            var insideMaterial = MaterialHelper.CreateMaterial(Colors.Black);
            var mygeometry = new GeometryModel3D();
            mygeometry.Material = greenMaterial;
            mygeometry.BackMaterial = insideMaterial;
            mygeometry.Geometry = mesh;
            var myModelVisual3D = new ModelVisual3D();
            myModelVisual3D.Content = mygeometry;
            GraphicViewOpt.Children.Add(myModelVisual3D);
            GraphicViewOpt.Title = A.ActivityDescription;
            TubeVisual3D tube1 = new TubeVisual3D();
            tube1.Path = new Point3DCollection();
            tube1.Path.Add(new Point3D(A.EndX, A.EndY, A.EndZ));
            tube1.Path.Add(new Point3D(A.StartX, A.StartY, A.StartZ));
            tube1.Diameter = 2.0;
            moveCrane = true; ///

            if (A.ActivityDescription.Contains("Moving crane from "))
            {
                tube1.Fill = Brushes.DarkGray;
            }
            else
            {
                tube1.Fill = Brushes.DarkOrange;
            }
            tube1.IsPathClosed = false;
            GraphicViewOpt.Children.Add(tube1);



            //cleanup
            previousDrawing = currentDrawing;
            currentIndex = currentIndex + 1;
            if(currentIndex == ThisCollection3D.Count)
            {
                drawMe = false;
                UITimer.IsEnabled = false;
                GraphicViewOpt.Title = "Containment";
                currentDrawing = new Activity3D();
                previousDrawing = new Activity3D();
            }


        }
        private void ComboBoxSelect_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            lblDistance.Content = "Total Distance: ";
            if (!ready) return;
            if (ComboBoxSelect.SelectedIndex < 0) { return; }
            if (ComboBoxSelect.SelectedItem == null) { return; }
            optimizeddataGrid.ItemsSource = globals.OptimizedActivityList[ComboBoxSelect.SelectedIndex];
            double dist = 0.0;
            foreach (var item in globals.OptimizedActivityList[ComboBoxSelect.SelectedIndex])
            {
                dist = dist + item.TotalDistance;
            }
            lblDistance.Content = lblDistance.Content + dist.ToString();
            ThisCollection3D = globals.OptimizedActivityList[ComboBoxSelect.SelectedIndex];
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < globals.OptimizedActivityList.Count; i++)
            {
                ComboBoxSelect.Items.Add("Optimized choice " + Convert.ToString(i + 1));
            }
            ready = true;
            ThisCollection3D = new ObservableCollection<Activity3D>();
            UITimer = new System.Windows.Threading.DispatcherTimer();
            UITimer.Interval = TimeSpan.FromSeconds(2);
            UITimer.IsEnabled = false;
            UITimer.Tick += new EventHandler(UITimer_Tick);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            moveCrane = true;
            bool proceed = true;
            if (ComboBoxSelect.SelectedIndex < 0) { proceed = false; }
            if (ComboBoxSelect.SelectedItem == null) { proceed = false; }
            if(!proceed)
            {
                MessageBox.Show("Please select a combination from the data view tab before clicking this button.", "Select an item", MessageBoxButton.OK, MessageBoxImage.Stop);
                return;
            }
            UITimer.IsEnabled = true;
            drawMe = true;
            currentIndex = 0;



        }

        private void BtnBreak_Click(object sender, RoutedEventArgs e)
        {
            int i = 0;

            
            i = 1;
            
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (ThisCollection3D.Count < 1)
            {
                MessageBox.Show("Please select a combination from the data view tab before clicking this button.", "Select an item", MessageBoxButton.OK, MessageBoxImage.Stop);
                return;
            }
            Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
            dlg.FileName = "Optimized Moves"; // Default file name
            dlg.DefaultExt = ".xml"; // Default file extension
            dlg.Filter = "XML Document (.xml)|*.xml"; // Filter files by extension

            // Show save file dialog box
            Nullable<bool> result = dlg.ShowDialog();

            // Process save file dialog box results
            if (result == true)
            {
                // Save document
                globals.SaveAsFile = dlg.FileName;
            }
            /* 
            Project p = new Project();
            string result = p.XmlSerialize();
            Project p2 = result.XmlDeserialize<Project>();
            */
            try
            {
                CraneApp.code.XMLHelper.SerializeObject<ObservableCollection<Activity3D>>(ThisCollection3D, globals.SaveAsFile);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            if (System.IO.File.Exists(globals.SaveAsFile))
            {
                System.Diagnostics.Process.Start(globals.SaveAsFile);
            }
            else
            {
                MessageBox.Show("There was an error saving the file.");
            }
        }

        private void MenuItem_Click_1(object sender, RoutedEventArgs e)
        {
            string x = "";
            if(ThisCollection3D.Count == 0)
            {
                MessageBox.Show("Please select a combination from the data view tab before clicking this button.", "Select an item", MessageBoxButton.OK, MessageBoxImage.Stop);
                return;
            }
            try
            {
                x = CsvSerializer.SerializeToCsv(ThisCollection3D);
            }
            catch (Exception ex)
            {
                MessageBox.Show("There was an error serializing the activity list:\n" + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
            dlg.FileName = "Optimized Moves"; // Default file name
            dlg.DefaultExt = ".csv"; // Default file extension
            dlg.Filter = "CSV Documents (.csv)|*.csv"; // Filter files by extension

            // Show save file dialog box
            Nullable<bool> result = dlg.ShowDialog();

            // Process save file dialog box 
            string filename = "";
            if (result == true)
            {
                // Save document
                filename = dlg.FileName;
            }
            try
            {
                System.IO.File.WriteAllText(filename, x);
            }
            catch (Exception ex)
            {
                MessageBox.Show("There was an error saving the activity list:\n" + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
        }

        private void MenuItem_Click_2(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("This feature has not yet been implimented.");
        }
    }
}
