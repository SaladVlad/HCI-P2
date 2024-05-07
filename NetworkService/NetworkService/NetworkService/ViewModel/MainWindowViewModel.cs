using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using NetworkService.Model;
using NetworkService.Views;

namespace NetworkService.ViewModel
{
    public class MainWindowViewModel : BindableBase
    {
        public ObservableCollection<FlowMeter> FlowMeters {  get; set; }


        public ICommand ChangeViewCommand { get; private set; }

        private object selectedContent;

        public object SelectedContent { get => selectedContent; set => SetProperty(ref selectedContent, value); }

        public MainWindowViewModel()
        {
            createListener(); //creating a listener for gathering info about network entities

            FlowMeters = new ObservableCollection<FlowMeter>();
            FlowMeters.Add(new FlowMeter { ID = 1, Name = "Naziv1", EntityType = new EntityType("volume", "volume.png") });
            FlowMeters.Add(new FlowMeter { ID = 15, Name = "Naziv2", EntityType = new EntityType("electronic", "electronic.png") });

            SelectedContent = new DisplayView(); //setting the net display view as a default

            #region Commands

            ChangeViewCommand = new MyICommand<string>(ChangeView);

            #endregion

        }

        private void ChangeView(string viewName)
        {
            if (viewName == "Table" && SelectedContent.GetType()!= typeof(EntitiesView))
            {
                SelectedContent = new EntitiesView();
            }
            else if (viewName == "Network" && SelectedContent.GetType() != typeof(DisplayView))
            {
                SelectedContent = new DisplayView();
            }
            else if (viewName == "Graph" && SelectedContent.GetType() != typeof(GraphView))
            {
                SelectedContent = new GraphView();
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

                            string[] parts = incomming.Split(':');
                            int id = int.Parse(parts[0].Split('_')[1]);
                            int value = int.Parse(parts[1]);
                            FlowMeters[id].Value = value;

                            //################ IMPLEMENTACIJA ####################
                            // Obraditi poruku kako bi se dobile informacije o izmeni
                            // Azuriranje potrebnih stvari u aplikaciji

                        }
                    }, null);
                }
            });

            listeningThread.IsBackground = true;
            listeningThread.Start();
        }

        
    }
}
