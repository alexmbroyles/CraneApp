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

namespace CraneApp
{

    ////// Nick asked me to load a demo.  Make a load xml or spreadsheet button, and a save/export to spreadsheet.
    //////
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //ObservableCollection<Activity3D> ActivityList3D;
        bool SelectFirst;
        bool IsSelecting;
        bool LastMoveClicked;
        bool editMode;
        double totalDistance;
        double craneHeight;
        double sphereRad;


        Activity3D CurrentActivity;
        Point3D point3d1;
        Point3D point3d2;
        List<Point3D> WayPoints;
        Activity3D ActivityToAdd;
        List<SubActivity3D> SubActivitiesToAdd;


        public MainWindow()
        {
            InitializeComponent();
            editMode = false;
            globals.ActivityList3D = new ObservableCollection<Activity3D>();
            WayPoints = new List<Point3D>();
            var x = new HelixToolkit.Wpf.ModelImporter();
            string modelfile = @"models\Containment-2.obj";
            if (!System.IO.File.Exists(modelfile))
            {
                MessageBox.Show("Cannot find containment file at: " + modelfile + "\nThis program cannot run without this file.  Please locate the 3d file and retry.", "Fatal error", MessageBoxButton.OK, MessageBoxImage.Error);
                Application.Current.Shutdown();
            }
            Model3DGroup group = x.Load(modelfile);
            //Model3DGroup group = x.Load(@"C:\Users\abroyle\Documents\Visual Studio 2015\Projects\CraneOptimization\CraneOptimization\images\Containment-2.obj");
            //Assembly assembly = Assembly.GetCallingAssembly();
            //Model3DGroup group = x.Load(@"pack://application:,,,/" + assembly.GetName().Name + ";component/models/Containment-2.obj");
            var newVisual = new ModelVisual3D { Content = group };
            ThreeDModel.Children.Add(newVisual);
            craneHeight = 100.0;
            sphereRad = 3.0;
            globals.ActivityList3D = new ObservableCollection<Activity3D>();
            LastMove.IsEnabled = false;
            LastMoveClicked = false;
            globals.SaveAsFile = "";
            globals.LoadFromFile = "";
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if(!IsSelecting) { return; }

            if (SelectFirst)
            {
                LastMoveClicked = false;
                LastMove.IsEnabled = true;
                if (editMode)
                {
                    ActivityToAdd = CurrentActivity;
                }
                else
                {
                    ActivityToAdd = new Activity3D();
                }
                if (!editMode)
                {
                    ActivityToAdd.sequence = globals.ActivityList3D.Count + 1;
                    ActivityToAdd.ActivityDescription = "Enter description for activity " + ActivityToAdd.sequence.ToString();
                    ActivityToAdd.Sysem = "Enter system for activity " + ActivityToAdd.sequence.ToString();
                    ActivityToAdd.CanChangeSequence = true;
                    ActivityToAdd.ActivityID = "ActID for activity " + ActivityToAdd.sequence.ToString();
                }
                SubActivitiesToAdd = new List<SubActivity3D>();
                var meshBuilder = new MeshBuilder(false, false);
                var p3d = ThreeDModel.CursorPosition.Value;
                if (p3d.X > 250.0) p3d.X = 250.0;
                if (p3d.X < -250.0) p3d.X = -250.0;
                if (p3d.Y > 250.0) p3d.Y = 250.0;
                if (p3d.Y < -250.0) p3d.Y = -250.0;
                point3d1 = p3d;
                point3d1.Z = 0.0;
                double x = p3d.X;
                double y = p3d.Y;
                double z = 0.0;
                point3d2 = new Point3D(x, y, craneHeight);
                meshBuilder.AddSphere(new Point3D(x, y, z), sphereRad, 10, 10);
                meshBuilder.AddSphere(new Point3D(x, y, craneHeight), sphereRad, 10, 10);

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
                ThreeDModel.Children.Add(myModelVisual3D);
                TubeVisual3D tube1 = new TubeVisual3D();
                tube1.Path = new Point3DCollection();
                tube1.Path.Add(point3d1);
                tube1.Path.Add(point3d2);
                tube1.Diameter = 2.0;
                tube1.Fill = Brushes.DarkBlue;
                tube1.IsPathClosed = false;
                ThreeDModel.Children.Add(tube1);

                cmdTxt.Content = "Select the next waypoint using the left mouse button.  Click last move button before clicking the last point in the move.";
                SelectFirst = false;

                ActivityToAdd.StartX = point3d1.X;
                ActivityToAdd.StartY = point3d1.Y;
                ActivityToAdd.StartZ = point3d1.Z;
                SubActivity3D sub1 = new SubActivity3D();
                sub1.StartPoint = point3d1;
                sub1.EndPoint = point3d2;
                sub1.Distance = Geo.CalculateDistance(point3d1, point3d2);
                SubActivitiesToAdd.Add(sub1);
                ActivityToAdd.SubMoves = SubActivitiesToAdd; 


                return;
            }
            else if(!SelectFirst && !LastMoveClicked)
            {

                ///// We need logic for making the crane move up high if distance is greater than crane rad



                SubActivity3D subActivity3D = new SubActivity3D();
                point3d1 = point3d2;
                var meshBuilder = new MeshBuilder(false, false);
                var p3d = ThreeDModel.CursorPosition.Value;

                // this doesn't do anything important:
                //if (p3d.X > 250.0) p3d.X = 250.0;
                //if (p3d.X < -250.0) p3d.X = -250.0;
                //if (p3d.Y > 250.0) p3d.Y = 250.0;
                //if (p3d.Y < -250.0) p3d.Y = -250.0;


                if(Geo.CalculateDistance(point3d1,globals.Orig) < globals.CraneRad && Geo.CalculateDistance(p3d, globals.Orig) > globals.CraneRad)
                {
                    //MessageBox.Show("The crane must move up through the top");





                }
                if (Geo.CalculateDistance(point3d1, globals.Orig) > globals.CraneRad && Geo.CalculateDistance(p3d, globals.Orig) < globals.CraneRad)
                {
                    //MessageBox.Show("The crane must move up through the top");
                }

                point3d2 = p3d;
                point3d2.Z = craneHeight;
                double x = p3d.X;
                double y = p3d.Y;
                double z = craneHeight;
                point3d2 = new Point3D(x, y, craneHeight);
                meshBuilder.AddSphere(new Point3D(x, y, z), sphereRad, 10, 10);
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
                ThreeDModel.Children.Add(myModelVisual3D);
                TubeVisual3D tube1 = new TubeVisual3D();
                tube1.Path = new Point3DCollection();
                tube1.Path.Add(point3d1);
                tube1.Path.Add(point3d2);
                tube1.Diameter = 2.0;
                tube1.Fill = Brushes.DarkBlue;
                tube1.IsPathClosed = false;
                ThreeDModel.Children.Add(tube1);
                // Add activities to list:
                SubActivity3D sub1 = new SubActivity3D();
                sub1.StartPoint = point3d1;
                sub1.EndPoint = point3d2;
                sub1.Distance = Geo.CalculateDistance(point3d1, point3d2);
                SubActivitiesToAdd.Add(sub1);
                ActivityToAdd.SubMoves = SubActivitiesToAdd;

            }
            else if(IsSelecting && LastMoveClicked && !SelectFirst)
            {

                SubActivity3D subActivity3D = new SubActivity3D();
                point3d1 = point3d2;
                var meshBuilder = new MeshBuilder(false, false);
                var p3d = ThreeDModel.CursorPosition.Value;
                if (p3d.X > 250.0) p3d.X = 250.0;
                if (p3d.X < -250.0) p3d.X = -250.0;
                if (p3d.Y > 250.0) p3d.Y = 250.0;
                if (p3d.Y < -250.0) p3d.Y = -250.0;
                point3d2 = p3d;
                point3d2.Z = 0.0;
                double x = p3d.X;
                double y = p3d.Y;
                double z = 0.0;
                point3d2 = new Point3D(x, y, 0.0);
                meshBuilder.AddSphere(new Point3D(x, y, z), sphereRad, 10, 10);
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
                ThreeDModel.Children.Add(myModelVisual3D);
                TubeVisual3D tube1 = new TubeVisual3D();
                tube1.Path = new Point3DCollection();
                tube1.Path.Add(point3d1);
                tube1.Path.Add(point3d2);
                tube1.Diameter = 2.0;
                tube1.Fill = Brushes.DarkBlue;
                tube1.IsPathClosed = false;
                ThreeDModel.Children.Add(tube1);



                // Add activities to list:

                ActivityToAdd.EndX = point3d2.X;
                ActivityToAdd.EndY = point3d2.Y;
                ActivityToAdd.EndZ = point3d2.Z;

                SubActivity3D sub1 = new SubActivity3D();
                sub1.StartPoint = point3d1;
                sub1.EndPoint = point3d2;
                sub1.Distance = Geo.CalculateDistance(point3d1, point3d2);
                SubActivitiesToAdd.Add(sub1);
                ActivityToAdd.SubMoves = SubActivitiesToAdd;

                double temp = 0.0;
                foreach(var item in SubActivitiesToAdd)
                {
                    temp = temp + item.Distance;
                }
                var msg = MessageBox.Show("Does this accurately show the move?\n(at the moment, we can't do anything about it if you say no. \nThis feature is still in development.)", "Redraw", MessageBoxButton.YesNo, MessageBoxImage.Question);




                ActivityToAdd.TotalDistance = temp;

                if (editMode)
                {
                    editMode = false;
                }
                else
                {
                    globals.ActivityList3D.Add(ActivityToAdd);
                }


                // cleanup:
                ActivityToAdd = new Activity3D();
                SubActivitiesToAdd = new List<SubActivity3D>();
                IsSelecting = false;
                LastMoveClicked = false;
                SelectFirst = false;
                AddMove.IsEnabled = true;
                cmdTxt.Content = "Move added.  Click button to add more";
            }
            
            return;
        }

        private void Window_MouseMove(object sender, MouseEventArgs e)
        {

        }

        private void Window_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            cmdTxt.Content = "";
            IsSelecting = false;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private void MoveDataGrid_Loaded(object sender, RoutedEventArgs e)
        {
            var grid = sender as DataGrid;
            grid.ItemsSource = globals.ActivityList3D;
        }

        private void AddMove_Click(object sender, RoutedEventArgs e)
        {
            TabGraphical.IsSelected = true;
            if(globals.ActivityList3D.Count > 4)
            {
                int num = Factorial(globals.ActivityList3D.Count+2);
                var result = MessageBox.Show("Warning: There are " + num.ToString() + " permutations (excluding fixed points).\nThis calculation could potentailly take a vary long time, or fail with insufficient memory.\nProceed?", "Warning", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if(result == MessageBoxResult.No)
                {
                    return;
                }
            }
            AddMove.IsEnabled = false;
            SelectFirst = true;
            IsSelecting = true;
            cmdTxt.Content = "Select location where item is to be picked up using the left mouse button.";
            CurrentActivity = new Activity3D();
        }

        private void LastMove_Click(object sender, RoutedEventArgs e)
        {
            LastMoveClicked = true;
            LastMove.IsEnabled = false;
            return;
        }

        private void btnCalc_Click(object sender, RoutedEventArgs e)
        {
            tbCalc.Text = "";
            if (globals.ActivityList3D.Count < 2)
            {
                tbCalc.Text = "Not enough moves to optimize.  Please add moves graphically or manually.";
                return;
            }
            List<List<Activity3D>> ListPermutation = new List<List<Activity3D>>();
            int NumFixedMoves = Geo.CalculateFixedPoints(globals.ActivityList3D.ToList());
            int Permutations = Geo.CalculatePermutations(globals.ActivityList3D.ToList(), NumFixedMoves);
            List<List<Activity3D>> tempDoubleList = Geo.GeneratePermutations<Activity3D>(globals.ActivityList3D.ToList());
            int TotalPermutations = tempDoubleList.Count;



            //// Note - the following manupulations, if done in the GeneratePermutations method, will save lots of processor time.
            // This gets rid of permutations that don't have fixed points:
            List<KeyValuePair<int, Activity3D>> IndexesWhichMustBeFixed = new List<KeyValuePair<int, Activity3D>>();
            for (int i = 0; i < globals.ActivityList3D.Count; i++)
            {
                if (!globals.ActivityList3D[i].CanChangeSequence)
                {
                    IndexesWhichMustBeFixed.Add(new KeyValuePair<int, Activity3D>(i, globals.ActivityList3D[i]));
                }
            }
            foreach (var list in tempDoubleList)
            {
                bool OKtoAdd = true;
                for (int i = 0; i < IndexesWhichMustBeFixed.Count; i++)
                {
                    if (list[IndexesWhichMustBeFixed[i].Key] != IndexesWhichMustBeFixed[i].Value)
                    {
                        OKtoAdd = false;
                    }
                }
                if (OKtoAdd)
                {
                    ListPermutation.Add(list);
                }
            }
            // We need to determine if MustBeBefore and MustBeAfter
            tempDoubleList.Clear();
            tempDoubleList = new List<List<Activity3D>>(ListPermutation);
            foreach(var list in tempDoubleList)
            {
                int currentIndex = ListPermutation.IndexOf(list);
                for(int i = 0; i < list.Count; i++)
                {
                    var currentItem = list[i];
                    if(currentItem.MustAppearAfter == currentItem.sequence) { continue; }
                    if (currentItem.MustAppearBefore == currentItem.sequence) { continue; }
                    bool remove = false;
                    if (currentItem.MustAppearAfter != 0)
                    {
                        remove = true;
                        for(int j = 0; j < i; j++)
                        {
                            if(list[j].sequence == currentItem.MustAppearAfter)
                            {
                                remove = false;
                            }
                        }

                    }

                    if (currentItem.MustAppearBefore != 0)
                    {
                        remove = true;
                        for (int j = i; j < list.Count; j++)
                        {
                            if (list[j].sequence == currentItem.MustAppearBefore)
                            {
                                remove = false;
                            }
                        }
                    }
                    if (remove)
                    {
                        if (currentIndex > -1)
                        {
                            ListPermutation.RemoveAt(currentIndex);
                            break;
                        }
                    }
                }
            }
            //ListPermutation.Clear();
            //ListPermutation = new List<List<Activity3D>>(tempDoubleList);



            tbCalc.Text += "Total number of permutations: " + TotalPermutations.ToString() + Environment.NewLine;
            tbCalc.Text += "Total fixed points: " + NumFixedMoves.ToString() + Environment.NewLine;
            tbCalc.Text += "Permutations accounting for fixed points: " + Permutations.ToString() + Environment.NewLine;
            tbCalc.Text += "Permutation list count with must be before/after logic: " + ListPermutation.Count.ToString() + Environment.NewLine;
            tbCalc.Text += "Adding crane return moves... " + Environment.NewLine;
            int count = ListPermutation.Count;
            for (int i = 0; i < count; i++)
            {
                ListPermutation[i] = Geo.AddReturnMoves(ListPermutation[i]);
            }
            tbCalc.Text += "Added crane return moves. Calculating total Distances... " + Environment.NewLine;

            List<KeyValuePair<List<Activity3D>, double>> DistanceList = new List<KeyValuePair<List<Activity3D>, double>>();
            for (int i = 0; i < ListPermutation.Count; i++)
            {
                double dist = 0.0;
                for (int j = 0; j < ListPermutation[i].Count; j++)
                {
                    dist = dist + ListPermutation[i][j].TotalDistance;
                }
                DistanceList.Add(new KeyValuePair<List<Activity3D>, double>(ListPermutation[i], dist));

            }
            for (int i = 0; i < DistanceList.Count; i++)
            {
                tbCalc.Text += "Permutation: " + Convert.ToString(i + 1) + "\tTotal Distance: " + DistanceList[i].Value.ToString() + Environment.NewLine;
            }

            tbCalc.Text += Environment.NewLine + "Minimum Distance(s):" + Environment.NewLine;
            List<KeyValuePair<List<Activity3D>, double>> OptimizedList = new List<KeyValuePair<List<Activity3D>, double>>();
            double temp = 0;
            try
            {
                temp = DistanceList.Min(x => x.Value);
            }
            catch(Exception ex)
            {
                MessageBox.Show("Please check the logic you applied to these moves\nIt seems that there is no possible permutation based on the\nmust appear before/after and fixed moves applied.\n\nMore information:\n" + ex.Message + Environment.NewLine + ex.InnerException, "Please check the logic", MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }
            foreach (var ent in DistanceList)
            {
                if (ent.Value <= temp)
                {
                    OptimizedList.Add(ent);
                }
            }
            tbCalc.Text += "There are " + OptimizedList.Count.ToString() + " combinations with the same distance." + Environment.NewLine;
            globals.OptimizedActivityList = new ObservableCollection<ObservableCollection<Activity3D>>();
            foreach(var x in OptimizedList)
            {
                var o = new ObservableCollection<Activity3D>(x.Key);
                globals.OptimizedActivityList.Add(o);
            }
            foreach (var k in OptimizedList)
            {
                List<Activity3D> lact = k.Key;
                tbCalc.Text += "Distance = " + k.Value.ToString() + Environment.NewLine;
                for (int i = 0; i < lact.Count; i++)
                {
                    var act = lact[i];
                    if (String.IsNullOrEmpty(act.Sysem)) act.Sysem = "";
                    if (String.IsNullOrEmpty(act.ActivityDescription)) act.ActivityDescription = "";
                    if (String.IsNullOrEmpty(act.ActivityID)) act.ActivityID = "";

                    tbCalc.Text += "Seq. " + Convert.ToString(i + 1) + "\t" + act.ActivityID.ToString() + "\t" + act.Sysem.ToString() + "\t" + act.ActivityDescription + "\tdistance: " + act.TotalDistance.ToString() + Environment.NewLine; ;
                }

            }
            CraneApp.windows.OptimizedView ov = new windows.OptimizedView();
            ov.Show();

        }
        public int Factorial(int numberInt)
        {
            int result = 1;
            if (numberInt == 0) return result;
            for (int i = 1; i < numberInt; i++)
            {
                result = result * i;
            }
            return result;

        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            if(globals.ActivityList3D.Count < 1)
            {
                MessageBox.Show("Nothing to save.  Please add moves before using this function.", "Nothing to save", MessageBoxButton.OK, MessageBoxImage.Asterisk);
                return;
            }
            Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
            dlg.FileName = "Moves"; // Default file name
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
                CraneApp.code.XMLHelper.SerializeObject<ObservableCollection<Activity3D>>(globals.ActivityList3D, globals.SaveAsFile);
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            if(System.IO.File.Exists(globals.SaveAsFile))
            {
                System.Diagnostics.Process.Start(globals.SaveAsFile);
            }
            else
            {
                MessageBox.Show("There was an error saving the file.");
            }
            

        }

        private void BtnLoad_Click(object sender, RoutedEventArgs e)
        {

            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.DefaultExt = ".XML";
            dlg.Filter = "XML Files (*.xml)|*.xml";


            // Display OpenFileDialog by calling ShowDialog method 
            Nullable<bool> result = dlg.ShowDialog();


            // Get the selected file name and display in a TextBox 
            if (result == true)
            {
                globals.ActivityList3D.Clear();
                globals.ActivityList3D = new ObservableCollection<Activity3D>();
                try
                {
                    globals.ActivityList3D = CraneApp.code.XMLHelper.DeSerializeObject<ObservableCollection<Activity3D>>(dlg.FileName);
                }
                catch(Exception ex)
                {
                    MessageBox.Show("There was an error loading the xml file:\n" + ex.Message, "Error loading file", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
            }
            else
            {
                return;
            }
            foreach(var A in globals.ActivityList3D)
            {
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
                ThreeDModel.Children.Add(myModelVisual3D);
                if (A.SubMoves != null)
                {
                    if (A.SubMoves.Count > 0)
                    {
                        foreach (var S in A.SubMoves)
                        {
                            var meshBuilder2 = new MeshBuilder(false, false);
                            meshBuilder2.AddSphere(S.StartPoint, sphereRad, 10, 10);
                            meshBuilder2.AddSphere(S.EndPoint, sphereRad, 10, 10);
                            var mesh2 = meshBuilder2.ToMesh(true);
                            var modelGroup2 = new Model3DGroup();
                            var greenMaterial2 = MaterialHelper.CreateMaterial(Colors.Black);
                            var insideMaterial2 = MaterialHelper.CreateMaterial(Colors.Black);
                            var mygeometry2 = new GeometryModel3D();
                            mygeometry2.Material = greenMaterial2;
                            mygeometry2.BackMaterial = insideMaterial2;
                            mygeometry2.Geometry = mesh2;
                            var myModelVisual3D2 = new ModelVisual3D();
                            myModelVisual3D2.Content = mygeometry2;
                            ThreeDModel.Children.Add(myModelVisual3D2);


                            TubeVisual3D tube1 = new TubeVisual3D();
                            tube1.Path = new Point3DCollection();
                            tube1.Path.Add(S.StartPoint);
                            tube1.Path.Add(S.EndPoint);
                            tube1.Diameter = 2.0;
                            tube1.Fill = Brushes.DarkBlue;
                            tube1.IsPathClosed = false;
                            ThreeDModel.Children.Add(tube1);
                        }
                    }
                    else
                    {
                        TubeVisual3D tube1 = new TubeVisual3D();
                        tube1.Path = new Point3DCollection();
                        tube1.Path.Add(new Point3D(A.StartX, A.StartY, A.StartZ ));
                        tube1.Path.Add(new Point3D(A.EndX, A.EndY, A.EndZ));
                        tube1.Diameter = 2.0;
                        tube1.Fill = Brushes.DarkBlue;
                        tube1.IsPathClosed = false;
                        ThreeDModel.Children.Add(tube1);
                    }
                }
                else
                {
                    TubeVisual3D tube1 = new TubeVisual3D();
                    tube1.Path = new Point3DCollection();
                    tube1.Path.Add(new Point3D(A.StartX, A.StartY, A.StartZ));
                    tube1.Path.Add(new Point3D(A.EndX, A.EndY, A.EndZ));
                    tube1.Diameter = 2.0;
                    tube1.Fill = Brushes.DarkBlue;
                    tube1.IsPathClosed = false;
                    ThreeDModel.Children.Add(tube1);
                }
            }
            MoveDataGrid.Items.Refresh();
            MoveDataGrid.Items.Refresh();

        }
        int i = 0;
        private void breakme_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                i = i + 1;
                tbCalc.Text = "Try: " + i.ToString();
                System.IO.File.Open("C:/FileThatDoesNotExist.x", System.IO.FileMode.Open);
            }
            catch(System.IO.IOException ioe)
            {
                tbCalc.Text += "That file doesn't exist: " + ioe.Message + "Retrying...";
                CraneApp.code.Retry.Do(() => breakme_Click(sender, e), TimeSpan.FromMilliseconds(2000), 1);
                return;
            }
            catch(Exception ex) { tbCalc.Text += "This time you get the other exception: " + ex.Message + "Not retrying"; }
        }
        private void BtnLoadXL_Click(object sender, RoutedEventArgs e)
        {
            globals.ActivityList3D.Clear();
            globals.ActivityList3D = new ObservableCollection<Activity3D>();
            Random rnd = new Random();
            string tempDir = "";
            if(!System.IO.Directory.Exists("temp"))
            {
                System.IO.Directory.CreateDirectory("temp");
            }
            tempDir = "temp" + System.IO.Path.DirectorySeparatorChar;
            string fileName = tempDir + rnd.Next(0,99999).ToString() + "_template.xlsx";
            try
            {
                CraneApp.code.GeneratedClass.CreatePackage(fileName);
            }
            catch(Exception ex)
            {
                MessageBox.Show("There was an error loading the template:\n" + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            var dlg = MessageBox.Show("Please copy polar crane activities from P6 spreadsheet including these fields:\n\"Activity ID\"\n\"System\"\n\"Activity Description\"\n\"Start\"\n\"Finish\"", "Copy and paste activities", MessageBoxButton.OKCancel, MessageBoxImage.Information);
            if(dlg == MessageBoxResult.Cancel) { return; }

            var p = System.Diagnostics.Process.Start(fileName);

            try
            {
                p.WaitForExit();

            }
            catch(Exception ex)
            {
                MessageBox.Show("An error occurred:\n" + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try
            {
                var book = new LinqToExcel.ExcelQueryFactory(fileName);
                var query =
                            from row in book.Worksheet("PC Moves")
                            let item = new
                            {
                                ActivityID = row["ActivityID"].Cast<string>(),
                                Sysem = row["Sysem"].Cast<string>(),
                                ActivityDescription = row["ActivityDescription"].Cast<string>(),
                                StartDate = row["StartDate"].Cast<DateTime>(),
                                EndDate = row["EndDate"].Cast<DateTime>(),
                                sequence = row["sequence"].Cast<int>(),
                                MustAppearBefore    = row["MustAppearBefore"].Cast<int>(),
                                MustAppearAfter = row["MustAppearAfter"].Cast<int>(),
                                CanChangeSequence = row["CanChangeSequence"].Cast<bool>(),

                            }
                            select item;
                int i = 1;
                foreach(var item in query)
                {

                    if(String.IsNullOrEmpty(item.ActivityDescription))
                    {
                        continue;
                    }

                    var act = new Activity3D();
                    act.ActivityDescription = item.ActivityDescription;
                    act.ActivityID = item.ActivityID;
                    act.StartDate = item.StartDate;
                    act.EndDate = item.EndDate;
                    act.Sysem = item.Sysem;
                    if (item.sequence != 0)
                    {
                        act.sequence = item.sequence;
                    }
                    else
                    {
                        act.sequence = i;
                        i = i + 1;
                    }
                    act.MustAppearAfter = item.MustAppearAfter;
                    act.MustAppearBefore = item.MustAppearBefore;
                    act.CanChangeSequence = true;
                    globals.ActivityList3D.Add(act);
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show("An error occurred:\n" + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            try
            {
                System.IO.File.Delete(fileName);
            }
            catch { }


            /*
            /// this method not implimented:
            /// 
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.DefaultExt = ".CSV";
            dlg.Filter = "CSV Files (*.csv)|*.csv";


            // Display OpenFileDialog by calling ShowDialog method 
            Nullable<bool> result = dlg.ShowDialog();


            // Get the selected file name and display in a TextBox 
            if (result == true)
            {
                globals.ActivityList3D.Clear();
                globals.ActivityList3D = new ObservableCollection<Activity3D>();
                try
                {
                    using(System.IO.FileStream fs = System.IO.File.Open(dlg.FileName, System.IO.FileMode.Open))
                    globals.ActivityList3D = CsvSerializer.DeserializeFromStream<ObservableCollection<Activity3D>>(fs);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("There was an error loading the xml file:\n" + ex.Message, "Error loading file", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
            }
            else
            {
                return;
            }
            foreach (var A in globals.ActivityList3D)
            {
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
                ThreeDModel.Children.Add(myModelVisual3D);
                if (A.SubMoves != null)
                {
                    if (A.SubMoves.Count > 0)
                    {
                        foreach (var S in A.SubMoves)
                        {
                            var meshBuilder2 = new MeshBuilder(false, false);
                            meshBuilder2.AddSphere(S.StartPoint, sphereRad, 10, 10);
                            meshBuilder2.AddSphere(S.EndPoint, sphereRad, 10, 10);
                            var mesh2 = meshBuilder2.ToMesh(true);
                            var modelGroup2 = new Model3DGroup();
                            var greenMaterial2 = MaterialHelper.CreateMaterial(Colors.Black);
                            var insideMaterial2 = MaterialHelper.CreateMaterial(Colors.Black);
                            var mygeometry2 = new GeometryModel3D();
                            mygeometry2.Material = greenMaterial2;
                            mygeometry2.BackMaterial = insideMaterial2;
                            mygeometry2.Geometry = mesh2;
                            var myModelVisual3D2 = new ModelVisual3D();
                            myModelVisual3D2.Content = mygeometry2;
                            ThreeDModel.Children.Add(myModelVisual3D2);


                            TubeVisual3D tube1 = new TubeVisual3D();
                            tube1.Path = new Point3DCollection();
                            tube1.Path.Add(S.StartPoint);
                            tube1.Path.Add(S.EndPoint);
                            tube1.Diameter = 2.0;
                            tube1.Fill = Brushes.DarkBlue;
                            tube1.IsPathClosed = false;
                            ThreeDModel.Children.Add(tube1);
                        }
                    }
                    else
                    {
                        TubeVisual3D tube1 = new TubeVisual3D();
                        tube1.Path = new Point3DCollection();
                        tube1.Path.Add(new Point3D(A.StartX, A.StartY, A.StartZ));
                        tube1.Path.Add(new Point3D(A.EndX, A.EndY, A.EndZ));
                        tube1.Diameter = 2.0;
                        tube1.Fill = Brushes.DarkBlue;
                        tube1.IsPathClosed = false;
                        ThreeDModel.Children.Add(tube1);
                    }
                }
                else
                {
                    TubeVisual3D tube1 = new TubeVisual3D();
                    tube1.Path = new Point3DCollection();
                    tube1.Path.Add(new Point3D(A.StartX, A.StartY, A.StartZ));
                    tube1.Path.Add(new Point3D(A.EndX, A.EndY, A.EndZ));
                    tube1.Diameter = 2.0;
                    tube1.Fill = Brushes.DarkBlue;
                    tube1.IsPathClosed = false;
                    ThreeDModel.Children.Add(tube1);
                }
            }*/


            MoveDataGrid.Items.Refresh();
            
        }
        private void BtnSaveXL_Click(object sender, RoutedEventArgs e)
        {
            string x = "";
            try
            {
                x = CsvSerializer.SerializeToCsv(globals.ActivityList3D);
            }
            catch(Exception ex)
            {
                MessageBox.Show("There was an error serializing the activity list:\n" + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
            dlg.FileName = "Moves"; // Default file name
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
            catch(Exception ex)
            {
                MessageBox.Show("There was an error saving the activity list:\n" + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
        }
        public void AddMovesToItem_Click(object sender, RoutedEventArgs e)
        {
            Activity3D obj = new Activity3D();
            try
            {
                obj = ((FrameworkElement)sender).DataContext as Activity3D;
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occurred:\n" + ex.Message, "Error getting activity information", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            Activity3D SelectedAct = new Activity3D();
            try
            {
                SelectedAct = globals.ActivityList3D.FirstOrDefault(x => x == obj);
                if (SelectedAct == null)
                {
                    throw new Exception();
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show("An internal error occurred while locating this activity in the list:\n" + ex.Message, "Error getting activity information", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            SelectedAct.SubMoves = new List<SubActivity3D>();
            SelectedAct.StartX = 0.0;
            SelectedAct.StartY = 0.0;
            SelectedAct.StartZ = 0.0;
            SelectedAct.EndX = 0.0;
            SelectedAct.EndY = 0.0;
            SelectedAct.EndZ = 0.0;
            SelectedAct.TotalDistance = 0.0;


            MoveDataGrid.Items.Refresh();
            TabGraphical.IsSelected = true;

            AddMove.IsEnabled = false;
            SelectFirst = true;
            IsSelecting = true;
            cmdTxt.Content = "Select location where item is to be picked up using the left mouse button.";
            CurrentActivity = SelectedAct;
            editMode = true;
        }
    }
}
