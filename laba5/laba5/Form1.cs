using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

/*
___
160
140
240
320
___
260
140
200
140
160
180
160
240
180
260
240
260
___
*/

/*
___
20
80
280
340
___
220
120
80
120
80
240
220
240
___
*/

namespace laba5
{
    public partial class Form1 : Form
    {
        Graphics drawArea;
        SolidBrush brush;
        Pen pen;

        //координаты отрезка
        List<Point> segment = new List<Point>() { };

        //направляющий вектор отрезка
        Point direction = new Point();

        //массив точек многоугольника
        List<Point> polygon = new List<Point>() { };

        //массив точек пересечений t
        List<double> crossroads = new List<double>() { };

        //массив потенциально входящих точек
        List<double> incoming = new List<double>() { };

        //массив потенциально выходящих точек
        List<double> outgoing = new List<double>() { };

        //ответ
        List<double> inner = new List<double>() { };

        //координаты нормалей
        List<Point> normals = new List<Point>() { };

        int Xmin, Xmax, Ymin, Ymax;

        public Form1()
        {
            InitializeComponent();
            drawArea = pictureBox1.CreateGraphics();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            CyrusBeck();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            MidPoint();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            CohenSutherland();
        }

        public void Initialize()
        {
            brush = new SolidBrush(Color.White);
            drawArea.FillRectangle(brush, 0, 0, pictureBox1.Width, pictureBox1.Height);

            segment = new List<Point>() { };
            polygon = new List<Point>() { };

            direction = new Point();
            crossroads = new List<double>() { };
            incoming = new List<double>() { };
            outgoing = new List<double>() { };
            inner = new List<double>() { };
            normals = new List<Point>() { };

            Xmax = 0;
            Xmin = pictureBox1.Width;
            Ymax = 0;
            Ymin = pictureBox1.Height;

            string[] lines_segment = textBox1.Lines;
            for (int i = 0; i < lines_segment.Length; i += 2)
            {
                Point current_point = new Point(int.Parse(lines_segment[i]), int.Parse(lines_segment[i + 1]));
                segment.Add(current_point);
            }

            string[] lines_polygon = textBox2.Lines;
            for (int i = 0; i < lines_polygon.Length; i += 2)
            {
                //точки вводят для обхода многоугольника против часовой стрелки
                Point current_point = new Point(int.Parse(lines_polygon[i]), int.Parse(lines_polygon[i + 1]));

                polygon.Add(current_point);
            }
            polygon.Add(new Point(polygon[0].X, polygon[0].Y));
        }

        public void DrawSegment()
        {
            pen = new Pen(Color.Blue, 2);
            drawArea.DrawLine(pen, segment[0].X, segment[0].Y, segment[1].X, segment[1].Y);
        }

        public void DrawPolygon()
        {
            for (int i = 1; i < polygon.Count(); i++)
            {
                if (polygon[i].X < Xmin)
                    Xmin = polygon[i].X;

                if (polygon[i].Y < Ymin)
                    Ymin = polygon[i].Y;

                if (polygon[i].X > Xmax)
                    Xmax = polygon[i].X;

                if (polygon[i].Y > Ymax)
                    Ymax = polygon[i].Y;

                pen = new Pen(Color.Red, 2);
                drawArea.DrawLine(pen, polygon[i - 1].X, polygon[i - 1].Y, polygon[i].X, polygon[i].Y);
            }
        }

        public void GetNormals()
        {
            Point current_normal = new Point();
            for (int i = 1; i < polygon.Count(); i++)
            {
                current_normal.X = polygon[i].Y - polygon[i - 1].Y;
                current_normal.Y = polygon[i - 1].X - polygon[i].X;
                normals.Add(current_normal);
            }
        }

        public void GetDirection()
        {
            direction.X = segment[1].X - segment[0].X;
            direction.Y = (segment[1].Y - segment[0].Y);
        }

        public void FindCrossT()
        {
            double current_t;
            for (int i = 0; i < polygon.Count() - 1; i++)
            {
                Point current_normal = normals[i];
                Point current_point = polygon[i];
                Point P1 = segment[0];               
                double numerator = 1.0 * ((P1.X - current_point.X) * current_normal.X + (P1.Y - current_point.Y) * current_normal.Y);
                double denominator = 1.0 * (direction.X * current_normal.X + direction.Y * current_normal.Y);
                current_t = -(numerator / denominator);
                if (current_t >= 0 && current_t <= 1)
                {
                    crossroads.Add(current_t);
                }
            }
        }

        public void FindIncomingOutgoing()
        {
            double min_in = 2.0;
            double max_out = -1.0;
            Point P1 = segment[0];
            for (int i = 0; i < crossroads.Count(); i++)
            {
                Point current_normal = normals[i];
                double current_crossroad = 1.0 * crossroads[i];
                double tmp_debug = current_normal.X * direction.X + current_normal.Y * direction.Y;
                
                //потенциально входящие точки
                if (tmp_debug < 0) 
                {
                    incoming.Add(current_crossroad);

                    pen = new Pen(Color.LightGreen, 5);
                    int x_point = (int)(P1.X + direction.X * current_crossroad - 1);
                    int y_point = (int)(P1.Y + direction.Y * current_crossroad - 1);
                    drawArea.DrawEllipse(pen, x_point, y_point, 3, 3);
                   
                    if (current_crossroad < min_in)
                    {
                        min_in = current_crossroad;
                    }
                }
                //потенциально покидающие точки
                else if (tmp_debug > 0) 
                {
                    outgoing.Add(current_crossroad);

                    pen = new Pen(Color.DarkGreen, 5);
                    int x_point = (int)(P1.X + direction.X * current_crossroad - 1);
                    int y_point = (int)(P1.Y + direction.Y * current_crossroad - 1);
                    drawArea.DrawEllipse(pen, x_point, y_point, 3, 3);
                    
                    if (current_crossroad > max_out)
                    {
                        max_out = current_crossroad;
                    }
                }
            }
            if (min_in > max_out &&
                min_in >= 0 && min_in <= 1 &&
                max_out >= 0 && max_out <= 1) 
            {
                inner.Add(max_out);
                inner.Add(min_in);
            }
        }

        public void DrawInner()
        {
            if (inner.Count() > 0)
            {
                Point P3 = new Point();
                Point P4 = new Point();
                if (0 <= inner[0] && inner[1] <= 1)
                {
                    P3.X = (int)(segment[0].X + direction.X * inner[0]);
                    P3.Y = (int)(segment[0].Y + direction.Y * inner[0]);
                    P4.X = (int)(segment[0].X + direction.X * inner[1]);
                    P4.Y = (int)(segment[0].Y + direction.Y * inner[1]);
                }
                else
                {
                    if (inner[0] < 0)
                    {
                        P3 = segment[0];
                        P4.X = (int)(segment[0].X + direction.X * inner[1]);
                        P4.Y = (int)(segment[0].Y + direction.Y * inner[1]);
                    }
                    else
                    {
                        P3.X = (int)(segment[0].X + direction.X * inner[0]);
                        P3.Y = (int)(segment[0].Y + direction.Y * inner[0]);
                        P4 = segment[1];
                    }
                }
                pen = new Pen(Color.Gold, 2);
                drawArea.DrawLine(pen, P3.X, P3.Y, P4.X, P4.Y);
            }
            else
            {
                MessageBox.Show("Нет решений");
            }
        }

        public void CyrusBeck()
        {
            Initialize();
            DrawSegment();
            DrawPolygon();
            GetNormals();
            GetDirection();
            FindCrossT();
            FindIncomingOutgoing();
            DrawInner();
        }

        public void CohenSutherland()
        {
            byte[] perfect = { 0, 0, 0, 0 };

            Initialize();
            DrawSegment();
            DrawPolygon();
            DrawLines();

            byte[] byteA = RewriteBytes(segment[0]);
            byte[] byteB = RewriteBytes(segment[1]);

            //если отрезок пересекает область
            if (CheckCross(byteA, byteB))
            {
                double dx, dy;
                int x1 = segment[0].X;
                int y1 = segment[0].Y;
                int x2 = segment[1].X;
                int y2 = segment[1].Y;
                //пока отрезок не лежит в области
                while (((byteA[0] != perfect[0] ||
                        byteA[1] != perfect[1] ||
                        byteA[2] != perfect[2] ||
                        byteA[3] != perfect[3])
                        ||
                       (byteB[0] != perfect[0] ||
                        byteB[1] != perfect[1] ||
                        byteB[2] != perfect[2] ||
                        byteB[3] != perfect[3]))
                        &&
                        CheckCross(byteA, byteB))
                {
                    dx = x2 - x1;
                    dy = y2 - y1;

                    if (byteA[0] != perfect[0] ||
                        byteA[1] != perfect[1] ||
                        byteA[2] != perfect[2] ||
                        byteA[3] != perfect[3])
                    {
                        if (x1 < Xmin)
                        {
                            y1 += (int)(dy * (Xmin - x1) / dx);
                            x1 = Xmin;
                            continue;
                        }
                        if (x1 > Xmax)
                        {
                            y1 += (int)(dy * (Xmax - x1) / dx);
                            x1 = Xmax;
                            continue;
                        }
                        if (y1 < Ymin)
                        {
                            x1 += (int)(dx * (Ymin - y1) / dy);
                            y1 = Ymin;
                            continue;
                        }
                        if (y1 > Ymax)
                        {
                            x1 += (int)(dx * (Ymax - y1) / dy);
                            y1 = Ymax;
                            continue;
                        }
                        segment[0] = new Point(x1, y1);
                        byteA = RewriteBytes(segment[0]);
                    }
                    else
                    {
                        if (x2 < Xmin)
                        {
                            y2 += (int)(dy * (Xmin - x2) / dx);
                            x2 = Xmin;
                            continue;
                        }
                        if (x2 > Xmax)
                        {
                            y2 += (int)(dy * (Xmax - x2) / dx);
                            x2 = Xmax;
                            continue;
                        }
                        if (y2 < Ymin)
                        {
                            x2 += (int)(dx * (Ymin - y2) / dy);
                            y2 = Ymin;
                            continue;
                        }
                        if (y2 > Ymax)
                        {
                            x2 += (int)(dx * (Ymax - y2) / dy);
                            y2 = Ymax;
                            continue;
                        }
                        segment[1] = new Point(x2, y2);
                        byteB = RewriteBytes(segment[1]);
                    }
                }
                //рисуем ответ
                pen = new Pen(Color.Gold, 2);
                drawArea.DrawLine(pen, segment[0].X, segment[0].Y, segment[1].X, segment[1].Y);
            }
            else
            {
                MessageBox.Show("Нет решений");
            }
        }

        byte[] RewriteBytes(Point current_point)
        {
            byte[] current_bytes = { 0, 0, 0, 0 };
            //x1
            if (current_point.X < Xmin)
                current_bytes[0] = 1;
            else
                current_bytes[0] = 0;
            //y1
            if (current_point.Y < Ymin)
                current_bytes[1] = 1;
            else
                current_bytes[1] = 0;
            //x2
            if (current_point.X > Xmax)
                current_bytes[2] = 1;
            else
                current_bytes[2] = 0;
            //y2
            if (current_point.Y > Ymax)
                current_bytes[3] = 1;
            else
                current_bytes[3] = 0;
            //записали новое местоположение точки отрезка
            return current_bytes;
        }

        public void DrawLines()
        {
            pen = new Pen(Color.Black, 1);
            //x1
            drawArea.DrawLine(pen, Xmin, 0, Xmin, pictureBox1.Height);
            //x2
            drawArea.DrawLine(pen, Xmax, 0, Xmax, pictureBox1.Height);
            //y1
            drawArea.DrawLine(pen, 0, Ymin, pictureBox1.Width, Ymin);
            //y2
            drawArea.DrawLine(pen, 0, Ymax, pictureBox1.Width, Ymax);
        }

        bool CheckCross(byte[] A, byte[] B)
        {
            //отрезок полностью слева от x1
            if (A[0] == B[0] && B[0] == 1)
            {
                return false;
            }
            //отрезок полностью справа от x2
            if (A[2] == B[2] && B[2] == 1)
            {
                return false;
            }
            //отрезок полностью сверху от y1
            if (A[3] == B[3] && B[3] == 1)
            {
                return false;
            }
            //отрезок полностью снизу от y2
            if (A[1] == B[1] && B[1] == 1)
            {
                return false;
            }
            return true;
        }

        public void MidPoint()
        {
            Initialize();
            DrawSegment();
            DrawPolygon();
            DrawLines();
            FoundAndDrawMidPoint(segment[0], segment[1]);
        }

        public void FoundAndDrawMidPoint(Point A, Point B)
        {
            byte[] perfect = { 0, 0, 0, 0 };

            byte[] byteA = RewriteBytes(A);
            byte[] byteB = RewriteBytes(B);

            //если отрезок пересекает область
            if (CheckCross(byteA, byteB))
            {
                int x1 = A.X;
                int y1 = A.Y;
                int x2 = B.X;
                int y2 = B.Y;
                //если ненулевой отрезок не лежит целиком в области
                if (((byteA[0] != perfect[0] ||
                      byteA[1] != perfect[1] ||
                      byteA[2] != perfect[2] ||
                      byteA[3] != perfect[3])
                      ||
                     (byteB[0] != perfect[0] ||
                      byteB[1] != perfect[1] ||
                      byteB[2] != perfect[2] ||
                      byteB[3] != perfect[3]))
                      &&
                      (int)Math.Sqrt((x1 - x2) * (x1 - x2) + (y1 - y2) * (y1 - y2)) > 1)
                {
                    //считаем координаты середины отрезка
                    Point mid = new Point((int)((x1 + x2) / 2), (int)((y1 + y2) / 2));
                    //выполняем поиск для двух половинок
                    FoundAndDrawMidPoint(A, mid);
                    FoundAndDrawMidPoint(mid, B);
                }
                else
                {
                    //рисуем ответ
                    pen = new Pen(Color.Gold, 2);
                    drawArea.DrawLine(pen, x1, y1, x2, y2);
                }
            }
        }
    }
}
