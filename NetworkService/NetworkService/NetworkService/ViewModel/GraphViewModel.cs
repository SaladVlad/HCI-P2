using NetworkService.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using NetworkService.Views;
using System.Threading;
using System.Windows.Media;

namespace NetworkService.ViewModel
{
    public class GraphViewModel : BindableBase
    {

        public ObservableCollection<FlowMeter> FlowMeters { get; set; }
        private FlowMeter selectedMeter;


        private readonly double graphCoefficient = 0.16;

        public MyICommand SelectCommand;
        public MyICommand SelectionChangedCommand { get; }


        private readonly Timer _timer;

        public GraphViewModel()
        {
            this.FlowMeters= MainWindowViewModel.FlowMeters;
            if(FlowMeters.Count!=0)
                SelectedMeter = FlowMeters[0];

            StartPoint = new Point(45, 210); //this always is the same (unless you move the graph lol)
            
            //setting the points to a default state before initialization
            LinePoint_1 = new Point(45,210); 
            LinePoint_2 = new Point(95, 210);
            LinePoint_3 = new Point(145, 210);
            LinePoint_4 = new Point(195,210);
            LinePoint_5 = new Point(245,210);

            MarginPoint_1 = new Thickness(30, 195, 0, 0);
            MarginPoint_2 = new Thickness(80, 195, 0, 0);
            MarginPoint_3 = new Thickness(130, 195, 0, 0);
            MarginPoint_4 = new Thickness(180, 195, 0, 0);
            MarginPoint_5 = new Thickness(230, 195, 0, 0);

            NodeColor_1 = new SolidColorBrush(Colors.Purple);
            NodeColor_2 = new SolidColorBrush(Colors.Purple);
            NodeColor_3 = new SolidColorBrush(Colors.Purple);
            NodeColor_4 = new SolidColorBrush(Colors.Purple);
            NodeColor_5 = new SolidColorBrush(Colors.Purple);

            NodeText_1 = "0";
            NodeText_2 = "0";
            NodeText_3 = "0";
            NodeText_4 = "0";
            NodeText_5 = "0";

            //setting up an update timer to update the graph visuals
            _timer = new Timer(UpdatePositions, null, TimeSpan.Zero, TimeSpan.FromSeconds(1));


        }
        #region Properties

        public FlowMeter SelectedMeter { get => selectedMeter; set => selectedMeter = value; }

        private Point startPoint;
        public Point StartPoint { get => startPoint; set => SetProperty(ref startPoint, value); }
        private Point linePoint_1;
        public Point LinePoint_1 { get => linePoint_1; set => SetProperty(ref linePoint_1, value); }
        private Point linePoint_2;
        public Point LinePoint_2 { get => linePoint_2; set => SetProperty(ref linePoint_2, value); }
        private Point linePoint_3;
        public Point LinePoint_3 { get => linePoint_3; set => SetProperty(ref linePoint_3, value); }
        private Point linePoint_4;
        public Point LinePoint_4 { get => linePoint_4; set => SetProperty(ref linePoint_4, value); }
        private Point linePoint_5;
        public Point LinePoint_5 { get => linePoint_5; set => SetProperty(ref linePoint_5, value); }
        private Thickness marginPoint_1;
        public Thickness MarginPoint_1 { get => marginPoint_1; set => SetProperty(ref marginPoint_1, value); }
        private Thickness marginPoint_2;
        public Thickness MarginPoint_2 { get => marginPoint_2; set => SetProperty(ref marginPoint_2, value); }
        private Thickness marginPoint_3;
        public Thickness MarginPoint_3 { get => marginPoint_3; set => SetProperty(ref marginPoint_3, value); }
        private Thickness marginPoint_4;
        public Thickness MarginPoint_4 { get => marginPoint_4; set => SetProperty(ref marginPoint_4, value); }
        private Thickness marginPoint_5;
        public Thickness MarginPoint_5 { get => marginPoint_5; set => SetProperty(ref marginPoint_5, value); }


        private SolidColorBrush nodeColor_1;
        public SolidColorBrush NodeColor_1 { get => nodeColor_1; set => SetProperty(ref nodeColor_1, value); }
        private SolidColorBrush nodeColor_2;
        public SolidColorBrush NodeColor_2 { get => nodeColor_2; set => SetProperty(ref nodeColor_2, value); }
        private SolidColorBrush nodeColor_3;
        public SolidColorBrush NodeColor_3 { get => nodeColor_3; set => SetProperty(ref nodeColor_3, value); }
        private SolidColorBrush nodeColor_4;
        public SolidColorBrush NodeColor_4 { get => nodeColor_4; set => SetProperty(ref nodeColor_4, value); }
        private SolidColorBrush nodeColor_5;
        public SolidColorBrush NodeColor_5 { get => nodeColor_5; set => SetProperty(ref nodeColor_5, value); }

        private string nodeText_1;
        public string NodeText_1 { get => nodeText_1; set => SetProperty(ref nodeText_1, value); }
        private string nodeText_2;
        public string NodeText_2 { get => nodeText_2; set => SetProperty(ref nodeText_2, value); }
        private string nodeText_3;
        public string NodeText_3 { get => nodeText_3; set => SetProperty(ref nodeText_3, value); }
        private string nodeText_4;
        public string NodeText_4 { get => nodeText_4; set => SetProperty(ref nodeText_4, value); }
        private string nodeText_5;
        public string NodeText_5 { get => nodeText_5; set => SetProperty(ref nodeText_5, value); }

        #endregion


        #region Actions



        private void UpdatePositions(object state)
        {
            if (SelectedMeter.Last_5_Values.Count == 5 && SelectedMeter!=null)
            {

                Application.Current.Dispatcher.Invoke(() =>
                {

                    

                    LinePoint_1 = new Point(LinePoint_1.X, (int)Math.Round(210 - SelectedMeter.Last_5_Values[0] * graphCoefficient));
                    StartPoint = LinePoint_1;
                    MarginPoint_1 = new Thickness(MarginPoint_1.Left, (int)Math.Round(195 - SelectedMeter.Last_5_Values[0] * graphCoefficient), 0, 0);
                    NodeText_1 = SelectedMeter.Last_5_Values[0].ToString();

                    if (SelectedMeter.Last_5_Values[0] < 670)
                    {
                        NodeColor_1 = new SolidColorBrush(Colors.Purple);
                    }
                    else if (SelectedMeter.Last_5_Values[0] > 735)
                    {
                        NodeColor_1 = new SolidColorBrush(Colors.Red);
                    }
                    else
                    {
                        NodeColor_1 = new SolidColorBrush(Colors.Green);
                    }



                    LinePoint_2 = new Point(LinePoint_2.X, (int)Math.Round(210 - SelectedMeter.Last_5_Values[1] * graphCoefficient));
                    MarginPoint_2 = new Thickness(MarginPoint_2.Left, (int)Math.Round(195 - SelectedMeter.Last_5_Values[1] * graphCoefficient), 0, 0);
                    NodeText_2 = SelectedMeter.Last_5_Values[1].ToString();

                    if (SelectedMeter.Last_5_Values[1] < 670)
                    {
                        NodeColor_2 = new SolidColorBrush(Colors.Purple);
                    }
                    else if (SelectedMeter.Last_5_Values[1] > 735)
                    {
                        NodeColor_2 = new SolidColorBrush(Colors.Red);
                    }
                    else
                    {
                        NodeColor_2 = new SolidColorBrush(Colors.Green);
                    }


                    LinePoint_3 = new Point(LinePoint_3.X, (int)Math.Round(210 - SelectedMeter.Last_5_Values[2] * graphCoefficient));
                    MarginPoint_3 = new Thickness(MarginPoint_3.Left, (int)Math.Round(195 - SelectedMeter.Last_5_Values[2] * graphCoefficient), 0, 0);
                    NodeText_3 = SelectedMeter.Last_5_Values[2].ToString();

                    if (SelectedMeter.Last_5_Values[2] < 670)
                    {
                        NodeColor_3 = new SolidColorBrush(Colors.Purple);
                    }
                    else if (SelectedMeter.Last_5_Values[2] > 735)
                    {
                        NodeColor_3 = new SolidColorBrush(Colors.Red);
                    }
                    else
                    {
                        NodeColor_3 = new SolidColorBrush(Colors.Green);
                    }


                    LinePoint_4 = new Point(LinePoint_4.X, (int)Math.Round(210 - SelectedMeter.Last_5_Values[3] * graphCoefficient));
                    MarginPoint_4 = new Thickness(MarginPoint_4.Left, (int)Math.Round(195 - SelectedMeter.Last_5_Values[3] * graphCoefficient), 0, 0);
                    NodeText_4 = SelectedMeter.Last_5_Values[3].ToString();

                    if (SelectedMeter.Last_5_Values[3] < 670)
                    {
                        NodeColor_4 = new SolidColorBrush(Colors.Purple);
                    }
                    else if (SelectedMeter.Last_5_Values[3] > 735)
                    {
                        NodeColor_4 = new SolidColorBrush(Colors.Red);
                    }
                    else
                    {
                        NodeColor_4 = new SolidColorBrush(Colors.Green);
                    }


                    LinePoint_5 = new Point(LinePoint_5.X, (int)Math.Round(210 - SelectedMeter.Last_5_Values[4] * graphCoefficient));
                    MarginPoint_5 = new Thickness(MarginPoint_5.Left, (int)Math.Round(195 - SelectedMeter.Last_5_Values[4] * graphCoefficient), 0, 0);
                    NodeText_5 = SelectedMeter.Last_5_Values[4].ToString();

                    if (SelectedMeter.Last_5_Values[4] < 670)
                    {
                        NodeColor_5 = new SolidColorBrush(Colors.Purple);
                    }
                    else if (SelectedMeter.Last_5_Values[4] > 735)
                    {
                        NodeColor_5 = new SolidColorBrush(Colors.Red);
                    }
                    else
                    {
                        NodeColor_5 = new SolidColorBrush(Colors.Green);
                    }

                });

            }
            else if (SelectedMeter == null)
            {
                 //do nothing because no entity is selected
            }
            else
            {
                Console.WriteLine($"[ WARNING ] Waiting on at least 5 values; Current: {SelectedMeter.Last_5_Values.Count}");
               
            }
        }

        #endregion

        public void Dispose()
        {
            _timer.Dispose();
        }
    }
}
