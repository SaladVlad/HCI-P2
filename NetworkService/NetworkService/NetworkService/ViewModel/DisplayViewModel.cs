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
        public ObservableCollection<Category> Categories { get; set; }
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

        public MyICommand TreeViewItemMouseMoveCommand { get; set; }
        public MyICommand TreeViewSelectedItemChangedCommand { get; set; }

        #endregion


        public DisplayViewModel()
        {
            InitializeCategories();
            InitializeCollections();

            DrawExistingLines();
            
            DropEntityOnCanvasCommand = new MyICommand<string>(OnDrop);
            MouseLeftButtonDownCommand = new MyICommand<string>(OnLeftMouseButtonDown);
            MouseLeftButtonUpCommand = new MyICommand(OnLeftMouseButtonUp);
            FreeCanvas = new MyICommand<string>(ResetCanvas);
            SelectionChangedCommand = new MyICommand<object>(OnSelectionChanged);

            //setting up an update timer to update the display visuals
            _timer = new Timer(UpdateEntitiesOnCanvas, null, TimeSpan.Zero, TimeSpan.FromMilliseconds(500));

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
                    if (category.Name.Equals(flowMeter.EntityType.Name))
                    {
                        category.FlowMeters.Add(flowMeter);
                    }
                }
            }
        }
        private void InitializeCollections()
        {
            CanvasCollection = CanvasCollection ?? new ObservableCollection<Canvas>();

            EntityInfo = EntityInfo ?? new ObservableCollection<FlowMeter>();

            BorderBrushCollection = BorderBrushCollection ?? new ObservableCollection<Brush>();

            AddedToGrid = AddedToGrid ?? new Dictionary<int, FlowMeter>();

            Lines = Lines ?? new Dictionary<Pair<int, int>, Line>();

            if (CanvasCollection.Count == 0) //if this is the first initialization, fill with default values
            {
                for (int i = 0; i < 12; i++)
                {
                    CanvasCollection.Add(new Canvas { Background = Brushes.PeachPuff, AllowDrop = true });
                    EntityInfo.Add(new FlowMeter { ID = -1, Name = "", EntityType = new EntityType("", ""), Value = 0 });
                    BorderBrushCollection.Add(Brushes.Black);
                }
            }
            foreach(FlowMeter f in AddedToGrid.Values)
                RemoveFromCategory(f);

        }

        private void DrawExistingLines()
        {
            throw new NotImplementedException();
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
                EntityInfo[index] = new FlowMeter { ID = -1, Name = "", EntityType = new EntityType("", ""), Value = 0 };
            }
        }

        private void OnDrop(string indexString)
        {
            int index = int.Parse(indexString);
            if (_draggedItem != null && !CanvasCollection[index].Resources.Contains("taken"))
            {
                var logo = new BitmapImage(new Uri(_draggedItem.EntityType.ImagePath, UriKind.Relative));
                CanvasCollection[index].Background = new ImageBrush(logo);
                CanvasCollection[index].Resources.Add("taken", true);
                CanvasCollection[index].Resources.Add("data", _draggedItem);
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

                SaveState();

            }
            //if the Drag&Drop action happened in between two taken canvases, draw a line
            else if(_draggedItem!= null  && CanvasCollection[index].Resources.Contains("taken"))
            {
                //draw a line, if it's not already drawn

                if (!IsLineAlreadyDrawn(_draggingSourceIndex,index))
                {
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
            CanvasCollection[index].Background = Brushes.PeachPuff;

            CanvasCollection[index].Resources.Remove("taken");
            FlowMeter removedMeter = CanvasCollection[index].Resources["data"] as FlowMeter;
            CanvasCollection[index].Resources.Remove("data");
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
        public static int GetCanvasIndexForEntityId(int entityId)
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
            foreach(FlowMeter entity in FlowMeters)
            {
                int canvasIndex = GetCanvasIndexForEntityId(entity.ID);

                if (canvasIndex != -1)
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


        private void SaveState()
        {
            throw new NotImplementedException();
        }
    }
}
