using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApplication1
{
    class Program
    {
        struct Point
        {
            public double x;
            public double y;
            public Point(double x, double y)
            {
                this.x = x;
                this.y = y;
            }
            public Point Sub(Point A)                       // subtracts one point to another
            {
                return new Point(x - A.x, y - A.y);
            }
            public double Cross(Point b)                    // finds cross product of this and another vector
            {
                return x * b.y - y * b.x;
            }
            public override string ToString()
            {
                return "(" + x + ", " + y + ")";
            }

        }

        static double length(Point p1, Point p2)            // finds length between two points
        {
            return Math.Sqrt((p1.x-p2.x)*(p1.x - p2.x) + (p1.y - p2.y)*(p1.y - p2.y));
        }

        static double perimeter(List<Point> points)         // input convex hull ==> finds perimeter of shape
        {
            if(points.Count < 3) { return 0; }
            double perim = length(points[0], points[points.Count-1]);
            for (int i = 1; i < points.Count; i++)
            {
                perim += length(points[i], points[i - 1]);
            }
            return perim;
        }

        static double area(List<Point> points)              // input convex hull ==> finds area of shape
        {
            double a = 0;
            int j = points.Count - 1;

            for (int i = 0; i < points.Count; i++)
            {
                a += (points[j].x + points[i].x) * (points[j].y - points[i].y);
                j = i;
            }

            a *= .5;
            if(a<0) { a *= -1; }
            return a;
        }

        static Point CenterOfMass(List<Point> shape)        // finds center-of-mass of the shape
        {
            double x = 0, y = 0;
            foreach(Point p in shape)
            {
                x += p.x; y += p.y;
            }
            x /= shape.Count; y /= shape.Count;
            return new Point(x, y);
        }

        static List<Point> CreateHull(List<Point> points)   // input raw shape ==> creates a convex hull based on points
        {
            points.Sort((a, b) =>
              a.x == b.x ? a.y.CompareTo(b.y) : (a.x > b.x ? 1 : -1));

            // Importantly, DList provides O(1) insertion at beginning and end
            List<Point> hull = new List<Point>();
            int L = 0, U = 0; // size of lower and upper hulls

            // Builds a hull such that the output polygon starts at the leftmost point.
            for (int i = points.Count - 1; i >= 0; i--)
            {
                Point p = points[i], p1;

                // build lower hull (at end of output list)
                while (L >= 2 && (p1 = hull[hull.Count - 1]).Sub(hull[hull.Count - 2]).Cross(p.Sub(p1)) >= 0)
                {
                    hull.RemoveAt(hull.Count - 1);
                    L--;
                }
                hull.Add(p);
                L++;

                // build upper hull (at beginning of output list)
                while (U >= 2 && (p1 = hull[0]).Sub(hull[1]).Cross(p.Sub(p1)) <= 0)
                {
                    hull.RemoveAt(0);
                    U--;
                }
                if (U != 0) // when U=0, share the point added above
                    hull.Insert(0, p);
                U++;
            }
            hull.RemoveAt(hull.Count - 1);
            return hull;
        }

        static List<Point> LargestTriangle(List<Point> hull)    // input convex hull ==> outputs largest possible triangle
        {
            double maxArea = 0;
            List<Point> tri = new List<Point>(3);

            for (int i = 0; i < hull.Count - 2; i++)
            {
                for (int j = i + 1; j < hull.Count - 1; j++)
                {
                    for (int k = j + 1; k < hull.Count; k++)
                    {
                        List<Point> test = new List<Point>();
                        test.Add(hull[i]);
                        test.Add(hull[j]);
                        test.Add(hull[k]);
                        if (area(test) > maxArea)
                        {
                            tri = new List<Point>(test);
                            maxArea = area(tri);
                        }
                    }
                }
            }
            return tri;
        }

        static List<Point> SmallestRectangle(List<Point> hull)  // input convex hull ==> outputs smallest binding rectangle parallel to axes
        {
            if (hull.Count < 2) { return hull; }
            List<Point> rect = new List<Point>();
            double minx = hull[0].x;
            double maxx = hull[0].x;
            double miny = hull[0].y;
            double maxy = hull[0].y;
            foreach(Point p in hull)
            {
                if (p.x < minx) { minx = p.x; }
                if (p.x > maxx) { maxx = p.x; }
                if (p.y < miny) { miny = p.y; }
                if (p.y > maxy) { maxy = p.y; }
            }
            rect.Add(new Point(minx,maxy));
            rect.Add(new Point(maxx, maxy));
            rect.Add(new Point(maxx, miny));
            rect.Add(new Point(minx, miny));
            return rect;
        }

        static int identifyShape(List<Point> points)        // input convex hull ==> 0: circle; 2: line; 3: triangle; 4: rectangle; 8: diamond; -1: no shape
        {
            double are = area(points);
            double per = perimeter(points);
            double tri = area(LargestTriangle(points));
            double rect = area(SmallestRectangle(points));
           /* Console.WriteLine("Line Test: " + are + ", " + 0);
            Console.WriteLine("Circle Test: " + (per*per/are) + ", " + (4 * Math.PI));
            Console.WriteLine("Triangle Test: " + are + ", " + tri);
            Console.WriteLine("Rectangle Test: " + are + ", " + rect);
            Console.WriteLine("Diamond Test: " + are / rect + ", " + 0.5);
            Console.WriteLine("Diamond Test: " + are*are / (rect * tri) + ", " + 1);*/
            if (are < 1) { return 2; }
            if (Math.Abs((per * per / are) - 4 * Math.PI) <= 1) { return 0; }
            if (Math.Abs(are - tri) <= 1) { return 3; }
            if (Math.Abs(are - tri) <= 1) { return 4; }
            if ((Math.Abs(are / rect - 0.5) <= .075)&&(Math.Abs(are * are / (rect * tri) - 1) <= .2)) { return 8; }
            return -1;
        }

        static bool overlaying(List<Point> shape1, List<Point> shape2)  // input 2 convex hulls ==> outputs boolean: true = shapes are overlapping, false = shapes are not overlapping
        {
            if((area(shape1)/area(shape2)<.5)||(area(shape1) / area(shape2) > 2))
            {
                return false;
            }
            if (length(CenterOfMass(shape1), CenterOfMass(shape2)) > 1.0 / 10 * (perimeter(shape1) + perimeter(shape2)))
            {
                return false;
            }
            return true;
        }


        static void testing (string file)           // input txt of points and shape # ==> calculates shape (no output)
        {
            char[] separatingChars = { ',', ' ' };
            List<Point> points = new List<Point>();
            string[] lines = System.IO.File.ReadAllLines(file);
            foreach (string line in lines)
            {
                string[] words = line.Split(separatingChars);
                if (words.Length == 2)
                {
                    points.Add(new Point(Convert.ToDouble(words[0]), Convert.ToDouble(words[1])));
                }
            }
            string text = "";
            for(int i = 0; i < points.Count; i++)
            {
                text += points[i] + (i < points.Count - 1 ? ", " : "");
            }
            System.IO.File.WriteAllText(""+file+".points.txt", text);
            List<Point> hull = CreateHull(points);

            text = "";
            for (int i = 0; i < hull.Count; i++)
            {
                text += hull[i] + ", ";
            }
            text += hull[0];
            System.IO.File.WriteAllText("" + file + ".hull.txt", text);
        }

        static void Main(string[] args)
        {
            testing("circle.txt");
            testing("triangle1.txt");
            testing("triangle2.txt");
            testing("rectangle1.txt");
            testing("rectangle2.txt");
            testing("line1.txt");
            testing("line2.txt");
            testing("diamond1.txt");
            testing("diamond2.txt");
            testing("diamond3.txt");
            testing("diamondbad1.txt");
            testing("diamondbad2.txt");
            testing("diamondgood1.txt");
            testing("diamondgood2.txt");

            string s = Console.ReadLine();
        }
    }
}
