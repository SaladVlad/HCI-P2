using NetworkService.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace NetworkService.ViewModel
{
    public class DisplayViewModel : BindableBase
    {

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

        public static ObservableCollection<Canvas> CanvasCollection { get; set; }
        public static ObservableCollection<FlowMeter> EntityInfo { get; set; }
        public static ObservableCollection<Brush> BorderBrushCollection { get; set; }


        #region Commands

        public MyICommand<object> DropEntityOnCanvasCommand { get; set; }
        public MyICommand<object> MouseLeftButtonDownCommand { get; set; }
        public MyICommand MouseLeftButtonUpCommand { get; set; }
        public MyICommand<object> FreeCanvas { get; set; }
        public MyICommand<object> SelectionChangedCommand { get; set; }

        #endregion


        public DisplayViewModel()
        {
            if (AddedToGrid == null)
            {
                AddedToGrid = new Dictionary<int, FlowMeter>();
            }
            if (CanvasCollection == null)
            {
                CanvasCollection = new ObservableCollection<Canvas>();
                for (int i = 0; i < 12; i++)
                {
                    CanvasCollection.Add(new Canvas()
                    {
                        Background = Brushes.PeachPuff,
                        AllowDrop = true
                    });
                }
            }

            if (EntityInfo == null)
            {
                EntityInfo = new ObservableCollection<FlowMeter>();

                for (int i = 0; i < 12; i++)
                {
                    FlowMeter newInfo = new FlowMeter
                    {
                        ID = -1,
                        Name = "",
                        EntityType = new EntityType("",""),
                        Value = 0
                    };

                    EntityInfo.Add(newInfo);
                }
            }

            if (BorderBrushCollection == null)
            {
                BorderBrushCollection = new ObservableCollection<Brush>();
                for (int i = 0; i < 12; i++)
                {
                    BorderBrushCollection.Add(Brushes.Black);
                }
            }

            DropEntityOnCanvasCommand = new MyICommand<object>(OnDrop);
            MouseLeftButtonDownCommand = new MyICommand<object>(OnLeftMouseButtonDown);
            MouseLeftButtonUpCommand = new MyICommand(OnLeftMouseButtonUp);
            FreeCanvas = new MyICommand<object>(OnFreeCanvas);
            SelectionChangedCommand = new MyICommand<object>(OnSelectionChanged);


            Categories = new ObservableCollection<Category>()
            {
                new Category("Volume"),
                new Category("Turbine"),
                new Category("Electronic")
            };

            foreach(FlowMeter flowMeter in FlowMeters)
            {
                foreach(Category category in Categories)
                {
                    if (category.Name.Equals(flowMeter.EntityType.Name))
                    {
                        category.FlowMeters.Add(flowMeter);
                    }
                }
            }

            


        }

        private void OnFreeCanvas(object obj)
        {
            throw new NotImplementedException();
        }

        private void OnLeftMouseButtonUp()
        {
            _draggedItem = null;
            SelectedEntity = null;
            _dragging = false;
            _draggingSourceIndex = -1;
        }

        private void OnSelectionChanged(object parameter)
        {
            if (!_dragging)
            {
                _dragging = true;
                _draggedItem = SelectedEntity;
                DragDrop.DoDragDrop((ListView)parameter, _draggedItem, DragDropEffects.Move);
            }
        }

        private void OnLeftMouseButtonDown(object parameter)
        {
            if (!_dragging)
            {
                int index = Convert.ToInt32(parameter);

                if (CanvasCollection[index].Resources["taken"] != null)
                {
                    _dragging = true;
                    _draggedItem = (FlowMeter)(CanvasCollection[index].Resources["data"]);
                    _draggingSourceIndex = index;
                    DragDrop.DoDragDrop(CanvasCollection[index],_draggedItem,DragDropEffects.Move);

                    EntityInfo[index] = new FlowMeter()
                    {
                        ID = -1,
                        Name = "",
                        EntityType = new EntityType("",""),
                        Value = 0
                    };
                }
            }
        }

        private void OnDrop(object parameter)
        {
            if (_draggedItem != null)
            {
                int index = Convert.ToInt32(parameter);

                if (CanvasCollection[index].Resources["taken"] == null)
                {
                    BitmapImage logo = new BitmapImage();
                    logo.BeginInit();
                    logo.UriSource = new Uri(_draggedItem.EntityType.ImagePath, UriKind.Relative);
                    logo.EndInit();

                    CanvasCollection[index].Background = new ImageBrush(logo);
                    CanvasCollection[index].Resources.Add("taken", true);
                    CanvasCollection[index].Resources.Add("data", _draggedItem);
                    BorderBrushCollection[index] = 
                        _draggedItem.ValueState==ValueState.Normal 
                        ? Brushes.GreenYellow : Brushes.Crimson;

                    AddedToGrid.Add(index, _draggedItem);
                    EntityInfo[index] = AddedToGrid[index];

                    // Premeštanje iz jednog u drugi
                    if (_draggingSourceIndex != -1)
                    {
                        CanvasCollection[_draggingSourceIndex].Background = (Brush)(new BrushConverter().ConvertFrom("#8B9DC3"));
                        CanvasCollection[_draggingSourceIndex].Resources.Remove("taken");
                        CanvasCollection[_draggingSourceIndex].Resources.Remove("data");
                        BorderBrushCollection[_draggingSourceIndex] = (Brush)(new BrushConverter().ConvertFrom("#282B30"));

                        //UpdateLinesForCanvas(draggingSourceIndex, index);

                        // Prekid linije ako se pomeri
                        //if (sourceCanvasIndex != -1)
                        //{
                        //    isLineSourceSelected = false;
                        //    sourceCanvasIndex = -1;
                        //    linePoint1 = new Point();
                        //    linePoint2 = new Point();
                        //    currentLine = new MyLine();
                        //}

                        _draggingSourceIndex = -1;
                    }

                    foreach(Category category in Categories)
                    {
                        if (category.FlowMeters.Contains(_draggedItem))
                        {
                            category.FlowMeters.Remove(_draggedItem);
                            break;
                        }
                    }
                }
            }
        }
    }
}
