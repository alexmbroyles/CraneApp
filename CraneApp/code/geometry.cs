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
    /// This is the bult of the math for this application.
    /// Calculate distance (2 overflows) allows you to calculate distance between either a set of coordinates or 2 point3ds
    /// Addreturnmoves adds the crane returm moves for each permutation.
    /// The others are used to generate permutations for a list<T>
    /// </summary>
    public class Geo
    {
        public static double CalculateDistance(double x1, double y1, double x2, double y2, double z1 = 0, double z2 = 0)
        {
            return Math.Sqrt(Math.Pow((x2 - x1), 2.0) + Math.Pow(y2 - y1, 2.0) + Math.Pow(z2 - z1, 2.0));
        }
        public static double CalculateDistance(Point3D Point1, Point3D Point2)
        {
            double x1, x2, y1, y2, z1, z2 = 0.0;
            x1 = Point1.X; x2 = Point2.X;
            y1 = Point1.Y; y2 = Point2.Y;
            z1 = Point1.Z; z2 = Point2.Z;
            return Math.Sqrt(Math.Pow((x2 - x1), 2.0) + Math.Pow(y2 - y1, 2.0) + Math.Pow(z2 - z1, 2.0));
        }

        // It's ugly, but it somehow works every single time:
        public static List<Activity3D> AddReturnMoves(List<Activity3D> ListIn)
        {

            var newList = new List<Activity3D>(ListIn.Count * 2);
            int j = 0;
            for (int i = 0; i < ListIn.Count * 2; i++)
            {
                if(j == ListIn.Count) { break; }
                newList.Add(i % 2 == 1 ? new Activity3D
                    { ActivityDescription = "Moving crane from " + ListIn[j-1].sequence.ToString() + " to " + ListIn[j].sequence.ToString(), StartX = ListIn[j-1].EndX, StartY = ListIn[j-1].EndY, StartZ = ListIn[j-1].EndZ, EndX = ListIn[j].StartX, EndY = ListIn[j].StartY, EndZ = ListIn[j].StartZ, TotalDistance = CalculateDistance(ListIn[j-1].EndX, ListIn[j-1].EndY, ListIn[j].StartX, ListIn[j].StartY, ListIn[j-1].EndZ, ListIn[j].StartZ)
                } : ListIn[j++]);
            }
            return newList;
        }
        // This works and is generic:
        public static List<List<T>> GeneratePermutations<T>(List<T> items)
        {
            T[] current_permutation = new T[items.Count];
            bool[] in_selection = new bool[items.Count];
            List<List<T>> results = new List<List<T>>();
            PermuteItems<T>(items, in_selection, current_permutation, results, 0);
            return results;
        }
        public static void PermuteItems<T>(List<T> items, bool[] in_selection, T[] current_permutation, List<List<T>> results, int next_position)
        {
            if (next_position == items.Count)
            {
                results.Add(current_permutation.ToList());
            }
            else
            {
                for (int i = 0; i < items.Count; i++)
                {
                    if (!in_selection[i])
                    {
                        in_selection[i] = true;
                        current_permutation[next_position] = items[i];
                        PermuteItems<T>(items, in_selection,
                            current_permutation, results,
                            next_position + 1);
                        in_selection[i] = false;
                    }
                }
            }
        }
        public static int CalculateFixedPoints(List<Activity3D> ListIn)
        {
            int Derangements = 0;
            foreach (var x in globals.ActivityList3D)
            {
                if (!x.CanChangeSequence)
                {
                    Derangements = Derangements + 1;
                }
            }
            return Derangements;
        }
        public static int CalculatePermutations(List<Activity3D> ListIn, int Derangements)
        {
            int ret = 0;
            ret = Fact((ListIn.Count - Derangements));
            return ret;
        }
        static int Fact(int n)
        {
            if (n <= 1)
                return 1;
            return n * Fact(n - 1);
        }
        private int nSides = 6;

    }


}
