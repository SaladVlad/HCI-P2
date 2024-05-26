using NetworkService.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using NetworkService.Helpers;
using System.Threading;
using System.Windows.Shapes;
using System.Windows.Navigation;
using NetworkService.Views;
using System.ComponentModel;
using System.Linq;
using System.Diagnostics;

namespace NetworkService.ViewModel
{
    public class DisplayViewModel : BindableBase
    {

        private readonly Timer _timer;

        private FlowMeter _selectedEntity;
        public FlowMeter SelectedEntity
        {
            get => _selectedEntity;
            set
            {
                SetProperty(ref _selectedEntity, value);
            }
        }

        //dragging item info
        private FlowMeter _draggedItem = null;
        private bool _dragging = false;
        public int _draggingSourceIndex = -1;

        public static ObservableCollection<FlowMeter> FlowMeters { get; set; }
        public static ObservableCollection<Category> Categories { get; set; }
        public static Dictionary<int, FlowMeter> AddedToGrid { get; set; }

        public static Dictionary<string, Line> Lines { get; set; }

        public static ObservableCollection<Line> LinesToDisplay { get; set; }

        public static ObservableCollection<Canvas> CanvasCollection { get; set; }
        public static ObservableCollection<FlowMeter> EntityInfo { get; set; }
        public static ObservableCollection<Brush> BorderBrushCollection { get; set; }


        #region Commands

        public MyICommand<string> DropEntityOnCanvasCommand { get; set; }
        public MyICommand<string> MouseLeftButtonDownCommand { get; set; }
        public MyICommand MouseLeftButtonUpCommand { get; set; }
        public MyICommand<string> FreeCanvas { get; set; }
        public MyICommand<object> SelectionChangedCommand { get; set; }
        public MyICommand<string> ClearCanvasCommand { get; set; }


        #endregion


        public DisplayViewModel()
        {
            MainWindowViewModel.Mutex.WaitOne();

            InitializeCollections();
            InitializeCategories();

            MainWindowViewModel.Mutex.ReleaseMutex();

            DrawExistingLines();

            

            DropEntityOnCanvasCommand = new MyICommand<string>(OnDrop);
            MouseLeftButtonDownCommand = new MyICommand<string>(OnLeftMouseButtonDown);
            MouseLeftButtonUpCommand = new MyICommand(OnLeftMouseButtonUp);
            FreeCanvas = new MyICommand<string>(ResetCanvas);
            SelectionChangedCommand = new MyICommand<object>(OnSelectionChanged);
            ClearCanvasCommand = new MyICommand<string>(ResetCanvas);


        }

        public static void InitializeCollections()
        {
            lock (_lock)
            {

                CanvasCollection = CanvasCollection ?? new ObservableCollection<Canvas>();
                EntityInfo = EntityInfo ?? new ObservableCollection<FlowMeter>();
                BorderBrushCollection = BorderBrushCollection ?? new ObservableCollection<Brush>();
                AddedToGrid = AddedToGrid ?? new Dictionary<int, FlowMeter>();
                Lines = Lines ?? new Dictionary<string, Line>();
                LinesToDisplay = LinesToDisplay ?? new ObservableCollection<Line>();

                if (CanvasCollection.Count == 0)
                {
                    for (int i = 0; i < 12; i++)
                    {
                        Canvas canvas = new Canvas();
                        if (AddedToGrid.ContainsKey(i))
                        {
                            var logo = new BitmapImage(new Uri(AddedToGrid[i].EntityType.ImagePath, UriKind.Relative));
                            canvas.Background = new ImageBrush(logo);
                            EntityInfo.Add(AddedToGrid[i]);
                            canvas.Resources["taken"] = true;
                            canvas.Resources["data"] = AddedToGrid[i];
                        }
                        else
                        {
                            canvas.Background = Brushes.Transparent;
                            EntityInfo.Add(null);
                        }

                        canvas.AllowDrop = true;
                        CanvasCollection.Add(canvas);
                        BorderBrushCollection.Add(Brushes.Transparent);
                    }
                }
                else
                {
                    for (int i = 0; i < 12; i++)
                    {
                        if (AddedToGrid.ContainsKey(i))
                        {
                            var logo = new BitmapImage(new Uri(AddedToGrid[i].EntityType.ImagePath, UriKind.Relative));
                            CanvasCollection[i].Background = new ImageBrush(logo);
                            CanvasCollection[i].Resources["taken"] = true;
                            CanvasCollection[i].Resources["data"] = AddedToGrid[i];
                            EntityInfo[i] = AddedToGrid[i];
                        }
                        else
                        {
                            CanvasCollection[i].Background = Brushes.Transparent;
                            if (CanvasCollection[i].Resources.Contains("taken"))
                                CanvasCollection[i].Resources.Remove("taken");
                            if (CanvasCollection[i].Resources.Contains("data"))
                                CanvasCollection[i].Resources.Remove("data");
                            BorderBrushCollection[i] = Brushes.Transparent;
                            EntityInfo[i] = null;
                        }
                    }
                }
                foreach (FlowMeter f in AddedToGrid.Values)
                    RemoveFromCategory(f);

            }

        }
        public static void InitializeCategories()
        {
            lock (_lock)
            {
                FlowMeters = MainWindowViewModel.FlowMeters;

                Categories = Categories ?? new ObservableCollection<Category>
            {
                new Category("Volume"),
                new Category("Turbine"),
                new Category("Electronic")
            };
                Categories[0].FlowMeters.Clear();
                Categories[1].FlowMeters.Clear();
                Categories[2].FlowMeters.Clear();

                foreach (var flowMeter in FlowMeters)
                {
                    foreach (var category in Categories)
                    {
                        if (category.Name.Equals(flowMeter.EntityType.Name) && !AddedToGrid.ContainsValue(flowMeter))
                        {
                            category.FlowMeters.Add(flowMeter);
                        }
                    }
                }
            }
        }


        private static readonly object _lock = new object();
        public static void DrawExistingLines()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                lock (_lock)
                {
                    try
                    {
                        var linesToRemove = LinesToDisplay.Except(Lines.Values).ToList();
                        foreach (var line in linesToRemove)
                        {
                            LinesToDisplay.Remove(line);
                        }

                        var linesToAdd = Lines.Values.Except(LinesToDisplay).ToList();
                        foreach (var line in linesToAdd)
                        {
                            LinesToDisplay.Add(line);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Exception in DrawExistingLines: {ex.Message}");
                    }
                }
            });

        }

        private void OnLeftMouseButtonUp()
        {
            _dragging = false;
            _draggedItem = null;
            _draggingSourceIndex = -1;
        }
        private void OnSelectionChanged(object parameter)
        {
            if (!_dragging && parameter != null)
            {
                _dragging = true;
                _draggedItem = SelectedEntity;
                if (_draggedItem != null)
                    DragDrop.DoDragDrop((ListView)parameter, _draggedItem, DragDropEffects.Move);
            }
        }
        private void OnLeftMouseButtonDown(string indexString)
        {
            int index = int.Parse(indexString);
            if (!_dragging && CanvasCollection[index].Resources.Contains("taken"))
            {
                _dragging = true;
                _draggedItem = CanvasCollection[index].Resources["data"] as FlowMeter;
                _draggingSourceIndex = index;

                if (_draggedItem != null)
                    DragDrop.DoDragDrop(CanvasCollection[index], _draggedItem, DragDropEffects.Move);

            }
        }
        private void OnDrop(string indexString)
        {

            int index = int.Parse(indexString);

            if (CanvasCollection[index].Resources.Contains("data"))
            {
                if (_draggedItem != null && (CanvasCollection[index].Resources["data"] as FlowMeter).ID == _draggedItem.ID)
                {
                    //dragged and dropped over itself, end the action
                    _draggedItem = null;
                    _draggingSourceIndex = -1;
                    _dragging = false;
                    return;
                }
            }



            if (_draggedItem != null && !CanvasCollection[index].Resources.Contains("taken"))
            {
                //save state to undo stack before anything happens
                SaveState();

                /*--------populating the dropped canvas---------*/
                var logo = new BitmapImage(new Uri(_draggedItem.EntityType.ImagePath, UriKind.Relative));
                CanvasCollection[index].Background = new ImageBrush(logo);
                CanvasCollection[index].Resources.Add("taken", true);
                CanvasCollection[index].Resources.Add("data", _draggedItem);
                AddedToGrid.Add(index, _draggedItem);
                BorderBrushCollection[index] = _draggedItem.ValueState == ValueState.Normal ? Brushes.GreenYellow : Brushes.Crimson;
                EntityInfo[index] = _draggedItem;
                /*----------------------------------------------*/


                //if the dragged item is from a different canvas control, clear the previous one and redraw lines
                if (_draggingSourceIndex != -1)
                {
                    ResetCanvas(_draggingSourceIndex.ToString());
                    List<int> connections = FindAllConnections(_draggingSourceIndex);

                    if (connections.Count != 0) //if the source had any lines, we remove and redraw them
                    {
                        foreach (int connectedTo in connections)
                        {
                            int source = Math.Min(_draggingSourceIndex, connectedTo);
                            int destination = Math.Max(_draggingSourceIndex, connectedTo);
                            DeleteLine(source, destination);

                            source = Math.Min(index, connectedTo);
                            destination = Math.Max(index, connectedTo);
                            Lines.Add($"{source},{destination}", CreateNewLine(source, destination));
                        }
                        DrawExistingLines(); //repopulate the presentation collection
                    }
                    //end the drag and drop action
                }

                //end the Drag&Drop action
                _draggingSourceIndex = -1;
                RemoveFromCategory(_draggedItem);
                _draggedItem = null;
                _dragging = false;

            }
            //if the Drag&Drop action happened in between two taken canvases, draw a line
            else if (_draggedItem != null && CanvasCollection[index].Resources.Contains("taken"))
            {
                //draw a line, if it's not already drawn
                if (IsLineAlreadyDrawn(_draggingSourceIndex, index) == 0)
                {
                    //save state to undo stack before anything happens
                    SaveState();


                    int source = Math.Min(_draggingSourceIndex, index);
                    int destination = Math.Max(_draggingSourceIndex, index);
                    Lines.Add($"{source},{destination}", CreateNewLine(source, destination));
                    DrawExistingLines();
                }
            }


        }
        private Line CreateNewLine(int sourceIndex, int destinationIndex)
        {
            Line newLine = new Line
            {
                X1 = ConvertToAbsoluteX(sourceIndex),
                Y1 = ConvertToAbsoluteY(sourceIndex),
                X2 = ConvertToAbsoluteX(destinationIndex),
                Y2 = ConvertToAbsoluteY(destinationIndex),
                Stroke = Brushes.White,
                StrokeThickness = 3,
                StrokeStartLineCap = PenLineCap.Round,
                StrokeEndLineCap = PenLineCap.Round
            };
            return newLine;
        }
        private double ConvertToAbsoluteY(int index)
        {
            index = index / 3;

            return Math.Round(index * 121.375 + 60.937);
        }
        private double ConvertToAbsoluteX(int index)
        {
            index = index % 3;

            return Math.Round(index * 148.333 + 74.167);
        }
        public static List<int> FindAllConnections(int index)
        {
            return Lines.Keys.Select(c =>
            {
                var parts = c.Split(',');
                int index1 = int.Parse(parts[0]);
                int index2 = int.Parse(parts[1]);
                return index == index1 ? index2 : index == index2 ? index1 : (int?)null;
            })
            .Where(connectedIndex => connectedIndex.HasValue)
            .Select(connectedIndex => connectedIndex.Value)
            .ToList();

        }
        public static void DeleteLine(int index1, int index2)
        {
            string key = $"{index1},{index2}";
            if (!Lines.Remove(key))
            {
                key = $"{index2},{index1}";
                Lines.Remove(key);
            }
        }

        private int IsLineAlreadyDrawn(int sourceIndex, int destinationIndex)
        {
            return Lines.Keys.Cast<string>().Any(c =>
            {
                var parts = c.Split(',');
                int index1 = int.Parse(parts[0]);
                int index2 = int.Parse(parts[1]);
                return sourceIndex == index1 && destinationIndex == index2;
            }) ? 1 : 0;
        }

        //method for removing content and lines from canvas
        private void ResetCanvas(string indexString)
        {

            int index = int.Parse(indexString);

            if (!CanvasCollection[index].Resources.Contains("taken"))
            {
                ToastNotify.RaiseToast("Error", "There is nothing to remove!", Notification.Wpf.NotificationType.Warning);
                return; //if nothing is inside the canvas, return

            }
            if (_draggingSourceIndex == -1) //if the action came from the clear button, delete lines
            {
                SaveState();

                List<int> connections = FindAllConnections(index);

                foreach (int connectedTo in connections) //always go from lower to higher indexes
                {
                    if (connectedTo > index)
                    {
                        DeleteLine(index, connectedTo);
                    }
                    else
                    {
                        DeleteLine(connectedTo, index);
                    }
                }
                DrawExistingLines();
            }

            CanvasCollection[index].Background = Brushes.Transparent;
            CanvasCollection[index].Resources.Remove("taken");
            FlowMeter removedMeter = CanvasCollection[index].Resources["data"] as FlowMeter;
            CanvasCollection[index].Resources.Remove("data");

            AddedToGrid.Remove(index); //remove entity from the list of placed entities
            AddToCategory(removedMeter);

            BorderBrushCollection[index] = Brushes.Transparent;
            EntityInfo[index] = null;

        }

        //add entity back to the list
        private void AddToCategory(FlowMeter flowMeter)
        {
            foreach (Category c in Categories)
            {
                if (c.Name.Equals(flowMeter.EntityType.Name))
                {
                    c.FlowMeters.Add(flowMeter);
                    break;
                }
            }
        }

        //method to remove entities that are already added to the grid
        private static void RemoveFromCategory(FlowMeter flowMeter)
        {
            foreach (var category in Categories) //going through all the categories
            {
                if (category.FlowMeters.Contains(flowMeter)) //if the added meter is in the list, remove it
                {
                    category.FlowMeters.Remove(flowMeter);
                    break;
                }
            }
        }

        //method for updating border color based on the value
        public static void UpdateEntitiesOnCanvas()
        {
            if (CanvasCollection != null)
            {
                for (int i = 0; i < 12; i++)
                {
                    if (CanvasCollection[i].Resources.Contains("taken"))
                    {
                        if (AddedToGrid.TryGetValue(i, out FlowMeter flowMeter))
                        {
                            BorderBrushCollection[i] = flowMeter.ValueState == ValueState.Normal ? Brushes.GreenYellow : Brushes.Crimson;
                        }
                    }
                }
            }
        }

        //method for saving state before an action
        public static void SaveState()
        {

            Dictionary<int, FlowMeter> entityState = new Dictionary<int, FlowMeter>();
            foreach (var entry in AddedToGrid)
            {
                entityState.Add(entry.Key, entry.Value);
            }
            Dictionary<string, Line> lineState = new Dictionary<string, Line>();
            foreach (var entry in Lines)
            {
                lineState.Add(entry.Key, entry.Value);
            }

            List<object> state = new List<object>() { entityState, lineState };
            //pushing state onto an undo stack
            MainWindowViewModel.UndoStack.Push(
                new SaveState<CommandType, object>(CommandType.CanvasManipulation, state));

        }
    }
}
