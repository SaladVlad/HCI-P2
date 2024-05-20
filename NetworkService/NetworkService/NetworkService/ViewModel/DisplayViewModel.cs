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

        public ObservableCollection<FlowMeter> FlowMeters = MainWindowViewModel.FlowMeters;
        public static ObservableCollection<Category> Categories { get; set; }
        public static Dictionary<int, FlowMeter> AddedToGrid { get; set; }
        public static Dictionary<Pair<int,int>,Line> Lines { get; set; }
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
            InitializeCollections();
            InitializeCategories();
            
            DrawExistingLines();
            
            DropEntityOnCanvasCommand = new MyICommand<string>(OnDrop);
            MouseLeftButtonDownCommand = new MyICommand<string>(OnLeftMouseButtonDown);
            MouseLeftButtonUpCommand = new MyICommand(OnLeftMouseButtonUp);
            FreeCanvas = new MyICommand<string>(ResetCanvas);
            SelectionChangedCommand = new MyICommand<object>(OnSelectionChanged);
            ClearCanvasCommand = new MyICommand<string>(ResetCanvas);

            //setting up an update timer to update the display visuals
            _timer = new Timer(UpdateEntitiesOnCanvas, null, TimeSpan.Zero, TimeSpan.FromMilliseconds(500));

        }

        private void InitializeCollections()
        {
            CanvasCollection = CanvasCollection ?? new ObservableCollection<Canvas>();
            EntityInfo = EntityInfo ?? new ObservableCollection<FlowMeter>();
            BorderBrushCollection = BorderBrushCollection ?? new ObservableCollection<Brush>();
            AddedToGrid = AddedToGrid ?? new Dictionary<int, FlowMeter>();
            Lines = Lines ?? new Dictionary<Pair<int, int>, Line>();

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
                        canvas.Background = Brushes.PeachPuff;
                        EntityInfo.Add(new FlowMeter { ID = -1, Name = "", EntityType = new EntityType("", ""), Value = 0 });
                    }

                    canvas.AllowDrop = true;
                    CanvasCollection.Add(canvas);
                    BorderBrushCollection.Add(Brushes.Black);
                }
            }
            else
            {
                for(int i = 0; i < 12; i++)
                {
                    if (AddedToGrid.ContainsKey(i))
                    {
                        var logo = new BitmapImage(new Uri(AddedToGrid[i].EntityType.ImagePath, UriKind.Relative));
                        CanvasCollection[i].Background = new ImageBrush(logo);
                        CanvasCollection[i].Resources["taken"] = true;
                        CanvasCollection[i].Resources["data"] = AddedToGrid[i];
                    }
                    else
                    {
                        CanvasCollection[i].Background = Brushes.PeachPuff;
                        CanvasCollection[i].Resources["taken"] = null;
                        CanvasCollection[i].Resources["data"] = null;
                    }
                }
            }

            foreach(FlowMeter f in AddedToGrid.Values)
                RemoveFromCategory(f);

        }
        private void InitializeCategories()
        {
            Categories = new ObservableCollection<Category>
            {
                new Category("Volume"),
                new Category("Turbine"),
                new Category("Electronic")
            };

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

        private void DrawExistingLines()
        {
            //TODO draw lines
            Lines.Clear();
            
        }



        private void OnLeftMouseButtonUp()
        {
            _dragging = false;
            _draggedItem = null;
            _draggingSourceIndex = -1;
        }

        private void OnSelectionChanged(object parameter)
        {
            if (!_dragging && parameter!=null)
            {
                _dragging = true;
                _draggedItem = SelectedEntity;
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
                DragDrop.DoDragDrop(CanvasCollection[index], _draggedItem, DragDropEffects.Move);
                
            }
        }

        private void OnDrop(string indexString)
        {
            

            int index = int.Parse(indexString);
            if (_draggedItem != null && !CanvasCollection[index].Resources.Contains("taken"))
            {
                //save state to undo stack before anything happens
                SaveState();

                var logo = new BitmapImage(new Uri(_draggedItem.EntityType.ImagePath, UriKind.Relative));
                CanvasCollection[index].Background = new ImageBrush(logo);
                CanvasCollection[index].Resources.Add("taken", true);
                CanvasCollection[index].Resources.Add("data", _draggedItem);

                AddedToGrid.Add(index,_draggedItem);

                BorderBrushCollection[index] = _draggedItem.ValueState == ValueState.Normal ? Brushes.GreenYellow : Brushes.Crimson;
                EntityInfo[index] = _draggedItem;

                //if the dragged item is from a different canvas control, clear the previous one
                if (_draggingSourceIndex != -1)
                {
                    ResetCanvas(_draggingSourceIndex.ToString());
                    _draggingSourceIndex = -1;
                }

                //end the Drag&Drop action
                RemoveFromCategory(_draggedItem);
                _draggedItem = null;
                _dragging= false;

                

            }
            //if the Drag&Drop action happened in between two taken canvases, draw a line
            else if(_draggedItem!= null  && CanvasCollection[index].Resources.Contains("taken"))
            {
                //draw a line, if it's not already drawn

                if (!IsLineAlreadyDrawn(_draggingSourceIndex,index))
                {
                    //save state to undo stack before anything happens
                    SaveState();

                    Pair<int, int> coordinates = 
                        new Pair<int, int>(_draggingSourceIndex, index);

                    Line newLine = new Line();
                    //TODO draw the line on the canvas
                }
            }


        }

        private bool IsLineAlreadyDrawn(int sourceIndex, int destinationIndex)
        {
            foreach(Pair<int,int> coordinates in Lines.Keys)
            {
                if(sourceIndex == coordinates.Item1 && destinationIndex == coordinates.Item2)
                {
                    return true;
                }
            }
            return false;
        }

        //method for clearing the canvas of its entity, and placing the entity back into the list
        private void ResetCanvas(string indexString)
        {

            int index = int.Parse(indexString);

            if (!CanvasCollection[index].Resources.Contains("taken"))
            {
                return; //if nothing is inside the canvas, return
                //TODO notification: warning about nothing being there
            }
            if(_draggingSourceIndex == -1)
            {
                SaveState();
            }

            CanvasCollection[index].Background = Brushes.PeachPuff;

            CanvasCollection[index].Resources.Remove("taken");
            FlowMeter removedMeter = CanvasCollection[index].Resources["data"] as FlowMeter;
            CanvasCollection[index].Resources.Remove("data");

            AddedToGrid.Remove(index); //remove entity from the list of placed entities
            AddToCategory(removedMeter);

            BorderBrushCollection[index] = Brushes.Black;
            EntityInfo[index] = new FlowMeter { ID = -1, Name = "", EntityType = new EntityType("", ""), Value = 0 };

        }

        //add entity back to the list
        private void AddToCategory(FlowMeter flowMeter)
        {
            foreach(Category c in Categories)
            {
                if (c.Name.Equals(flowMeter.EntityType.Name))
                {
                    c.FlowMeters.Add(flowMeter);
                    break;
                }
            }
        }

        //method to remove entities that are already added to the grid
        private void RemoveFromCategory(FlowMeter flowMeter)
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

        //getting the right canvas based on the entity's ID
        public int GetCanvasIndexForEntityId(int entityId)
        {
            for (int i = 0; i < CanvasCollection.Count; i++)
            {
                FlowMeter entity = (CanvasCollection[i].Resources["data"]) as FlowMeter;

                if ((entity != null) && (entity.ID == entityId))
                {
                    return i;
                }
            }
            return -1;
        }

        //method for updating border color based on the value
        private void UpdateEntitiesOnCanvas(object state)
        {
            if(BorderBrushCollection.Count == 12) //if the viewModel is fully loaded
            {
                foreach (FlowMeter entity in FlowMeters)
                {
                    int canvasIndex = GetCanvasIndexForEntityId(entity.ID);

                    if (canvasIndex != -1 && AddedToGrid.ContainsKey(canvasIndex))
                    {
                        if (entity.ValueState == ValueState.Normal)
                        {
                            BorderBrushCollection[canvasIndex] = Brushes.GreenYellow;
                        }
                        else
                        {
                            BorderBrushCollection[canvasIndex] = Brushes.Crimson;
                        }
                    }
                }
            }
            
        }


        //method for saving state before an action
        private void SaveState()
        {
            CanvasState canvasState = new CanvasState();

            canvasState.AddedToGrid = new Dictionary<int, FlowMeter>(AddedToGrid);
            canvasState.Lines = new Dictionary<Pair<int, int>, Line>(Lines);
            canvasState.BorderBrushCollection = new ObservableCollection<Brush>(BorderBrushCollection);
            canvasState.EntityInfo = new ObservableCollection<FlowMeter>(EntityInfo);

            //pushing state onto an undo stack
            MainWindowViewModel.UndoStack.Push(
                new SaveState<CommandType,object>(CommandType.CanvasManipulation, canvasState));

        }
    }
}
