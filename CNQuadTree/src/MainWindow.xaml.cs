using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
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
using System.Windows.Threading;

namespace CNQuadTree
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        CNQuadTree<TestNode> tree1 = new CNQuadTree<TestNode>();
        static Mutex myMutex = new Mutex();

        public MainWindow()
        {
            InitializeComponent();

            //attach handler for new quadrants
            tree1.NodeChangedEvent += Tree1_NodeChangedEvent;
        }


        private void DrawProbes()
        {
            foreach (var probe in tree1.Probes)
            {
                System.Windows.Shapes.Ellipse dot;
                dot = new System.Windows.Shapes.Ellipse();
                dot.Stroke = new SolidColorBrush(Colors.Black);
                dot.Fill = new SolidColorBrush(Colors.Red);
                dot.StrokeThickness = 1;
                dot.Width = probe.Bounds.Width;
                dot.Height = probe.Bounds.Height;
                Canvas.SetLeft(dot, probe.Position.X - (dot.Width / 2));
                Canvas.SetTop(dot, probe.Position.Y - (dot.Height / 2));
                cnv_main.Children.Add(dot);

                CNQuadTree<TestNode>.QuadNode quadnode = tree1.GetQuad(probe);
                if (quadnode != null)
                {
                    System.Windows.Shapes.Rectangle rect;
                    rect = new System.Windows.Shapes.Rectangle();
                    rect.Stroke = new SolidColorBrush(Colors.Black);
                    rect.Fill = new SolidColorBrush(Colors.Blue);
                    rect.StrokeThickness = 1;
                    rect.Width = quadnode.Bounds.Width;
                    rect.Height = quadnode.Bounds.Height;
                    Canvas.SetLeft(rect, quadnode.Bounds.Location.X);
                    Canvas.SetTop(rect, quadnode.Bounds.Location.Y);
                    cnv_main.Children.Add(rect);
                }

                //CNQuadTree<TestNode>.QuadNode quadnode2 = tree1.GetQuad(x, PointF.Add(quadnode.Bounds.Location, new System.Drawing.SizeF(10, -3)));
                CNQuadTree<TestNode>.QuadNode quadnode2 = tree1.GetQuad(probe, neighbor: 0);
                if (quadnode2 != null)
                {
                    System.Windows.Shapes.Rectangle rect;
                    rect = new System.Windows.Shapes.Rectangle();
                    rect.Stroke = new SolidColorBrush(Colors.Black);
                    rect.Fill = new SolidColorBrush(Colors.Green);
                    rect.StrokeThickness = 1;
                    rect.Width = quadnode2.Bounds.Width;
                    rect.Height = quadnode2.Bounds.Height;
                    Canvas.SetLeft(rect, quadnode2.Bounds.Location.X);
                    Canvas.SetTop(rect, quadnode2.Bounds.Location.Y);
                    cnv_main.Children.Add(rect);
                }
            }
        }

        private void OnWindowLoaded(object sender, RoutedEventArgs e)
        {
            tree1.Extent = new System.Drawing.RectangleF(new PointF(0, 0), new System.Drawing.SizeF(Convert.ToSingle(cnv_main.ActualWidth), Convert.ToSingle(cnv_main.ActualHeight)));

            //create and add probe
            CNQuadTree<TestNode>.Probe test = new CNQuadTree<TestNode>.Probe(
                new System.Drawing.PointF(10, 10),
                new System.Drawing.SizeF(10, 10)
            );
            test.DomainRadius = 80.0f;

            CNQuadTree<TestNode>.Probe test2 = new CNQuadTree<TestNode>.Probe(
                new System.Drawing.PointF(400, 40),
                new System.Drawing.SizeF(10, 10)
            );
            test2.DomainRadius = 10.0f;

            CNQuadTree<TestNode>.Probe test3 = new CNQuadTree<TestNode>.Probe(
                new System.Drawing.PointF(167, 200),
                new System.Drawing.SizeF(20, 20)
            );
            test3.DomainRadius = 20.0f;

            tree1.Probes.Add(test);
            tree1.Probes.Add(test2);
            tree1.Probes.Add(test3);

            DispatcherTimer redraw = new DispatcherTimer(new TimeSpan(0, 0, 0, 0, 50), DispatcherPriority.Background, (x, y) => {
                if (myMutex.WaitOne())
                {
                    cnv_main.Children.OfType<Ellipse>().ToList().ForEach(z => cnv_main.Children.Remove(z));
                    cnv_main.Children.OfType<System.Windows.Shapes.Rectangle>().ToList().ForEach(z => cnv_main.Children.Remove(z));
                    tree1.UpdateProbes();
                    DrawProbes();
                    myMutex.ReleaseMutex();
                }
            }
            , Dispatcher);
        }

        private void Tree1_NodeChangedEvent(object sender, CNQuadTree<TestNode>.NodeChangedEventArgs e)
        {
            //receive quadrant locations and create nodes
            Random rnd = new Random(5837296);
            foreach (var x in e.NodesModified)
            {
                if (x.Item2 == CNQuadTree<TestNode>.NodeChangeTypes.Add)
                {
                    x.Item1.Node = new TestNode() { Value = rnd.Next() };
                    Rect rect = new Rect();
                    rect.Width = x.Item1.Bounds.Width;
                    rect.Height = x.Item1.Bounds.Height;

                    DrawingVisual drawingVisual = new DrawingVisual();
                    DrawingContext drawingContext = drawingVisual.RenderOpen();
                    drawingContext.DrawRectangle((System.Windows.Media.Brush)null, new System.Windows.Media.Pen(System.Windows.Media.Brushes.Black, 1), rect);
                    drawingContext.Close();

                    VisualHost dv = new VisualHost() { Visual = drawingVisual };
                    Canvas.SetLeft(dv, x.Item1.Bounds.Location.X);
                    Canvas.SetTop(dv, x.Item1.Bounds.Location.Y);
                    x.Item1.Node.Element = dv;
                    cnv_main.Children.Add(dv);
                }
                else if (x.Item2 == CNQuadTree<TestNode>.NodeChangeTypes.Remove)
                {
                    cnv_main.Children.Remove(x.Item1.Node.Element);
                }
            }
        }

        private void cnv_main_MouseMove(object sender, MouseEventArgs e)
        {
            if (myMutex.WaitOne() && (e.LeftButton == MouseButtonState.Pressed))
            {
                tree1.Probes.FirstOrDefault().Position = new System.Drawing.PointF((float)e.GetPosition((Canvas)sender).X, (float)e.GetPosition((Canvas)sender).Y);
                myMutex.ReleaseMutex();
            }
        }
    }
}
