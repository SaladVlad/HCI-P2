using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using NetworkService.Helpers;
using NetworkService.Model;
using NetworkService.Views;

namespace NetworkService.ViewModel
{
    public class MainWindowViewModel : BindableBase
    {
        public static ObservableCollection<FlowMeter> FlowMeters { get; set; }

        public static Stack<SaveState<CommandType, object>> UndoStack { get; set; }

        #region Commands
        public MyICommand<string> ChangeViewCommand { get; set; }
        public MyICommand UndoCommand { get; set; }
        public MyICommand QuitCommand { get; set; }

        #endregion

        private object _selectedContent;
        public object SelectedContent
        {
            get => _selectedContent;
            set
            {
                SetProperty(ref _selectedContent, value);
                UndoCommand.RaiseCanExecuteChanged();
            }
        }

        public MainWindowViewModel()
        {
            createListener(); //creating a listener for gathering info about network entities

            FlowMeters = new ObservableCollection<FlowMeter>();
            FlowMeters.Add(new FlowMeter { ID = 1, Name = "Naziv1", EntityType = new EntityType("Volume", "../../Resources/Images/volume.png") });
            //FlowMeters.Add(new FlowMeter { ID = 15, Name = "Naziv2", EntityType = new EntityType("electronic", "electronic.png") });
            UndoStack = new Stack<SaveState<CommandType, object>>();


            #region Commands

            ChangeViewCommand = new MyICommand<string>(ChangeView);
            UndoCommand = new MyICommand(OnUndo, CanUndo);
            QuitCommand = new MyICommand(OnQuit);

            #endregion

            SelectedContent = new HomeView(); //setting the net display view as a default
            
        }


        private void ChangeView(string viewName)
        {
            if (viewName == "Table" && SelectedContent.GetType() != typeof(EntitiesView))
            {
                UndoStack.Push(new SaveState<CommandType, object>(CommandType.SwitchViews, SelectedContent.GetType()));
                SelectedContent = new EntitiesView();


            }
            else if (viewName == "Network" && SelectedContent.GetType() != typeof(DisplayView))
            {
                UndoStack.Push(new SaveState<CommandType, object>(CommandType.SwitchViews, SelectedContent.GetType()));
                SelectedContent = new DisplayView();

            }
            else if (viewName == "Graph" && SelectedContent.GetType() != typeof(GraphView))
            {
                UndoStack.Push(new SaveState<CommandType, object>(CommandType.SwitchViews, SelectedContent.GetType()));
                SelectedContent = new GraphView();

            }
            else if (viewName == "Home" && SelectedContent.GetType() != typeof(HomeView))
            {
                UndoStack.Push(new SaveState<CommandType, object>(CommandType.SwitchViews, SelectedContent.GetType()));
                SelectedContent = new HomeView();

            }
        }

        private void createListener()
        {
            var tcp = new TcpListener(IPAddress.Loopback, 25657);
            tcp.Start();

            var listeningThread = new Thread(() =>
            {
                while (true)
                {
                    var tcpClient = tcp.AcceptTcpClient();
                    ThreadPool.QueueUserWorkItem(param =>
                    {
                        //Prijem poruke
                        NetworkStream stream = tcpClient.GetStream();
                        string incomming;
                        byte[] bytes = new byte[1024];
                        int i = stream.Read(bytes, 0, bytes.Length);
                        //Primljena poruka je sacuvana u incomming stringu
                        incomming = System.Text.Encoding.ASCII.GetString(bytes, 0, i);

                        //Ukoliko je primljena poruka pitanje koliko objekata ima u sistemu -> odgovor
                        if (incomming.Equals("Need object count"))
                        {
                            Byte[] data = System.Text.Encoding.ASCII.GetBytes(FlowMeters.Count.ToString());
                            stream.Write(data, 0, data.Length);
                        }
                        else
                        {
                            //U suprotnom, server je poslao promenu stanja nekog objekta u sistemu
                            Console.WriteLine(incomming); //Na primer: "Entitet_1:272"

                            Helpers.Logging.AppendToFile("log.txt", incomming);

                            string[] parts = incomming.Split(':');
                            int id = int.Parse(parts[0].Split('_')[1]);
                            int value = int.Parse(parts[1]);
                            if (FlowMeters[id] != null)
                            {
                                FlowMeters[id].Value = value;
                                AddValueToList(FlowMeters[id]);
                            }
                            else
                            {
                                //TODO small toast about unknown recieved value
                            }

                        }
                    }, null);
                }
            });

            listeningThread.IsBackground = true;
            listeningThread.Start();
        }

        private void AddValueToList(FlowMeter flowMeter)
        {
            if (flowMeter.Last_5_Values.Count == 5)
            {
                flowMeter.Last_5_Values.RemoveAt(0);
                flowMeter.Last_5_Values.Add(new Pair<DateTime, int>(DateTime.Now, flowMeter.Value));
            }
            else
            {
                flowMeter.Last_5_Values.Add(new Pair<DateTime, int>(DateTime.Now, flowMeter.Value));
            }
        }

        public bool CanUndo()
        {
            return UndoStack.Count != 0;
        }
        public void OnUndo()
        {
            SaveState<CommandType, object> saveState = UndoStack.Pop();
            if (saveState.CommandType == CommandType.SwitchViews)
            {
                Type viewType = saveState.SavedState as Type;

                if (viewType == typeof(EntitiesView))
                {
                    SelectedContent = new EntitiesView();
                }
                else if (viewType == typeof(DisplayView))
                {
                    SelectedContent = new DisplayView();
                }
                else if (viewType == typeof(GraphView))
                {
                    SelectedContent = new GraphView();
                }
                else
                {
                    SelectedContent = new HomeView();
                }
            }
            else if (saveState.CommandType == CommandType.EntityManipulation)
            {
                FlowMeters = saveState.SavedState as ObservableCollection<FlowMeter>;
                //refreshing the list
                SelectedContent = new EntitiesView();
            }
            else if (saveState.CommandType == CommandType.CanvasManipulation)
            {
                
                DisplayViewModel.AddedToGrid.Clear();
                foreach(var entry in saveState.SavedState as Dictionary<int,FlowMeter>)
                {
                    DisplayViewModel.AddedToGrid.Add(entry.Key, entry.Value);
                }

                DisplayViewModel.InitializeCollections();
                DisplayViewModel.InitializeCategories();


            }

            GC.Collect();
            UndoCommand.RaiseCanExecuteChanged();
        }
        private void OnQuit()
        {
            Application.Current.MainWindow.Close();
        }

    }
}
