using NetworkService;
using NetworkService.Model;
using NetworkService.ViewModel;
using System.Windows;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using NetworkService.Helpers;
using System.Windows.Media.Animation;
using static System.Net.Mime.MediaTypeNames;
using System.Text.RegularExpressions;
using System.Reflection;
using System.Windows.Shapes;

namespace NetworkService.Views
{
    public class EntitiesViewModel : BindableBase
    {
        #region Properties and Commands

        #region Properties

        private SolidColorBrush _idBorderBrush;
        public SolidColorBrush IDBorderBrush { get => _idBorderBrush; set => SetProperty(ref _idBorderBrush, value); }
        public List<string> Types { get; set; }

        private object _selectedType;
        public object SelectedType
        {

            get => _selectedType;
            set
            {
                SetProperty(ref _selectedType, value);
                AddEntityCommand.RaiseCanExecuteChanged();
            }
        }

        private Visibility _keyboardVisibility;
        public Visibility KeyboardVisibility { get => _keyboardVisibility; set => SetProperty(ref _keyboardVisibility, value); }

        private bool _isKeyboardEnabled;
        public bool IsKeyboardEnabled { get => _isKeyboardEnabled; set => SetProperty(ref _isKeyboardEnabled, value); }

        private TextBox _selectedTextBox;
        public TextBox SelectedTextBox
        {
            get => _selectedTextBox;
            set
            {
                SetProperty(ref _selectedTextBox, value);
            }
        }

        private FlowMeter _selectedEntity;
        public FlowMeter SelectedEntity
        {
            get => _selectedEntity;
            set
            {
                SetProperty(ref _selectedEntity, value);
                RemoveEntityCommand.RaiseCanExecuteChanged();
            }
        }
        public ObservableCollection<FlowMeter> FlowMeters { get; set; }
        public ObservableCollection<FlowMeter> FilteredMeters { get; set; }

        private string _idText;
        public string IDText
        {
            get { return _idText; }
            set
            {
                SetProperty(ref _idText, value);
                AddEntityCommand.RaiseCanExecuteChanged();
            }
        }
        private string _nameText;
        public string NameText
        {
            get { return _nameText; }
            set
            {
                SetProperty(ref _nameText, value);
                AddEntityCommand.RaiseCanExecuteChanged();
            }
        }
        private string _filterText;
        public string FilterText
        {
            get { return _filterText; }
            set
            {
                SetProperty(ref _filterText, value);
                FilterCommand.RaiseCanExecuteChanged();
            }
        }
        private string _filterType;
        public string FilterType
        {
            get => _filterType;
            set
            {
                SetProperty(ref _filterType, value);
                FilterCommand.RaiseCanExecuteChanged();
            }
        }
        private bool _isLowerThanChecked;
        public bool IsLowerThanChecked
        {
            get => _isLowerThanChecked;
            set
            {
                SetProperty(ref _isLowerThanChecked, value);
                if (_isLowerThanChecked)
                {
                    IsEqualChecked = false;
                    IsGreaterThanChecked = false;
                }
                FilterCommand.RaiseCanExecuteChanged();
            }
        }
        private bool _isEqualChecked;
        public bool IsEqualChecked
        {
            get => _isEqualChecked;
            set
            {
                SetProperty(ref _isEqualChecked, value);
                if (_isEqualChecked)
                {
                    IsLowerThanChecked = false;
                    IsGreaterThanChecked = false;
                }
                FilterCommand.RaiseCanExecuteChanged();
            }
        }
        private bool _isGreaterThanChecked;
        public bool IsGreaterThanChecked
        {
            get => _isGreaterThanChecked;
            set
            {
                SetProperty(ref _isGreaterThanChecked, value);
                if (_isGreaterThanChecked)
                {
                    IsLowerThanChecked = false;
                    IsEqualChecked = false;
                }
                FilterCommand.RaiseCanExecuteChanged();
            }
        }

        #endregion

        #region Command Definitions
        public MyICommand<string> InputKeyCommand { get; set; }
        public MyICommand<object> TextBoxGotFocusCommand { get; set; }
        public MyICommand<object> TextBoxLostFocusCommand { get; set; }
        public MyICommand HideKeyboardCommand { get; set; }
        public MyICommand BackspaceCommand { get; set; }
        public MyICommand<string> InputNumberCommand { get; set; }
        public MyICommand<TextBox> TextChangedCommand { get; set; }
        public MyICommand AddEntityCommand { get; set; }
        public MyICommand RemoveEntityCommand { get; set; }
        public MyICommand FilterCommand { get; set; }
        public MyICommand ClearFiltersCommand { get; set; }

        #endregion

        #endregion

        #region Constructor
        public EntitiesViewModel()
        {

            FlowMeters = MainWindowViewModel.FlowMeters;
            FilteredMeters = new ObservableCollection<FlowMeter>();

            foreach (FlowMeter f in FlowMeters)
            {
                FilteredMeters.Add(f);
            }

            //creating commands for keyboard
            InputKeyCommand = new MyICommand<string>(InputKey);
            InputNumberCommand = new MyICommand<string>(InputNumber);

            TextBoxGotFocusCommand = new MyICommand<object>(TextBoxGotFocus);
            TextBoxLostFocusCommand = new MyICommand<object>(TextBoxLostFocus);

            BackspaceCommand = new MyICommand(Backspace);
            TextChangedCommand = new MyICommand<TextBox>(OnTextChanged);

            HideKeyboardCommand = new MyICommand(HideKeyboard);

            //creating commands for adding and removing entities
            AddEntityCommand = new MyICommand(OnAddEntity, CanAddEntity);
            RemoveEntityCommand = new MyICommand(OnRemoveEntity, CanRemoveEntity);

            FilterCommand = new MyICommand(Filter, CanFilter);
            ClearFiltersCommand = new MyICommand(ClearFilters);

            Types = new List<string>
            {
                "Volume",
                "Turbine",
                "Electronic"
            };

            IDBorderBrush = new SolidColorBrush(Colors.Transparent);

            IDText = "";
            NameText = "";
            SelectedType = Types[0];

            KeyboardVisibility = Visibility.Hidden;
            IsKeyboardEnabled = false;

        }

        #endregion

        #region Filter Actions
        private bool CanFilter()
        {
            // Enable filtering if there is only a selected filter type and no other criteria
            if (!string.IsNullOrEmpty(FilterType) &&
                !IsEqualChecked &&
                !IsGreaterThanChecked &&
                !IsLowerThanChecked &&
                string.IsNullOrWhiteSpace(FilterText))
            {
                return true;
            }

            bool isFilterTextValid = int.TryParse(FilterText, out _);

            // Enable filtering if FilterText is a valid number and at least one of the ID criteria is checked
            if (isFilterTextValid &&
                (IsEqualChecked || IsGreaterThanChecked || IsLowerThanChecked))
            {
                return true;
            }

            return false;
        }
        private void ClearFilters()
        {
            //resetting UI elements
            IsEqualChecked = false;
            IsGreaterThanChecked = false;
            IsLowerThanChecked = false;
            FilterText = string.Empty;
            FilterType = null;

            //repopulating the table
            FilteredMeters.Clear();
            foreach (FlowMeter f in FlowMeters)
                FilteredMeters.Add(f);
        }
        private void Filter()
        {
            HideKeyboard();
            FilteredMeters.Clear();

            var filteredByType = new List<FlowMeter>();

            // First pass: Filter by type
            if (!string.IsNullOrEmpty(FilterType))
            {
                foreach (FlowMeter flowMeter in FlowMeters)
                {
                    if (flowMeter.EntityType.Name.Equals(FilterType))
                    {
                        filteredByType.Add(flowMeter);
                    }
                }
            }
            else
            {
                filteredByType.AddRange(FlowMeters);
            }

            // Second pass: Filter by ID criteria
            foreach (FlowMeter flowMeter in filteredByType)
            {
                bool matches = true;
                if (!string.IsNullOrWhiteSpace(FilterText))
                {
                    if (int.TryParse(FilterText, out int filterValue))
                    {
                        if (IsLowerThanChecked && flowMeter.ID >= filterValue)
                        {
                            matches = false;
                        }
                        if (IsEqualChecked && flowMeter.ID != filterValue)
                        {
                            matches = false;
                        }
                        if (IsGreaterThanChecked && flowMeter.ID <= filterValue)
                        {
                            matches = false;
                        }
                    }
                    else
                    {
                        matches = false; // Invalid FilterText means no match
                    }
                }

                if (matches)
                {
                    FilteredMeters.Add(flowMeter);
                }
            }

            ToastNotify.RaiseToast("Filtered", "Successful filtering!", Notification.Wpf.NotificationType.Notification);
        }

        #endregion

        #region Text Changed Action
        private void OnTextChanged(TextBox textBox)
        {
            if (textBox.Name.Equals("IDTextBox") || textBox.Name.Equals("FilterTextBox"))
            {
                if (Regex.IsMatch(textBox.Text, @"^\d+$"))
                {
                    return;
                }
                else
                {
                    if (!string.IsNullOrEmpty(textBox.Text))
                    {
                        // Remove the last character
                        textBox.Text = textBox.Text.Remove(textBox.Text.Length - 1);
                        textBox.CaretIndex = textBox.Text.Length;
                    }

                    // Ensure the background is a SolidColorBrush
                    if (!(textBox.Background is SolidColorBrush))
                    {
                        textBox.Background = new SolidColorBrush(Colors.Transparent);
                    }

                    // Create a color animation
                    var colorAnimation = new ColorAnimation
                    {
                        From = Colors.Red,
                        To = (Color)System.Windows.Application.Current.Resources["PrimaryColorDark"],
                        Duration = TimeSpan.FromSeconds(0.3),
                        AutoReverse = false
                    };

                    var storyboard = new Storyboard();
                    storyboard.Children.Add(colorAnimation);

                    Storyboard.SetTargetProperty(colorAnimation, new PropertyPath("(TextBox.Background).(SolidColorBrush.Color)"));

                    Storyboard.SetTarget(colorAnimation, textBox);

                    storyboard.Begin();
                }
            }
        }

        #endregion

        #region Creating/Removing
        private bool CanRemoveEntity()
        {
            if (SelectedEntity != null)
            {
                return true;
            }
            return false;
        }
        private void OnRemoveEntity()
        {
            if (MessageBox.Show(
                "Are you sure you want to remove the selected entity?",
                "Confirmation Dialog",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                SaveState();
                FlowMeters.Remove(SelectedEntity);

                if (DisplayViewModel.AddedToGrid != null)
                {
                    var keyToRemove = DisplayViewModel.AddedToGrid.FirstOrDefault(
                    x => EqualityComparer<FlowMeter>.Default.Equals(x.Value, SelectedEntity)).Key;

                    if (!EqualityComparer<int>.Default.Equals(keyToRemove, default))
                    {
                        DisplayViewModel.AddedToGrid.Remove(keyToRemove);
                        List<int> connections = DisplayViewModel.FindAllConnections(keyToRemove);
                        foreach (int connectedTo in connections)
                        {
                            int source = Math.Min(keyToRemove, connectedTo);
                            int destination = Math.Max(keyToRemove, connectedTo);
                            DisplayViewModel.DeleteLine(source, destination);
                        }
                    }
                }

                ToastNotify.RaiseToast(
                    "Deletion Successful",
                    $"Entity with ID:{SelectedEntity.ID}",
                    Notification.Wpf.NotificationType.Information);

                SelectedEntity = null;
                ClearFilters();
            }


        }
        private bool CanAddEntity()
        {
            bool allGood = true;

            if (IDText.Trim().Length > 0)
            {
                int intID;
                try
                {
                    intID = int.Parse(IDText);

                    foreach (FlowMeter f in FlowMeters)
                    {
                        if (f.ID == intID)
                        {
                            IDBorderBrush = new SolidColorBrush(Colors.Red);
                            allGood = false;
                            break;
                        }
                    }
                    if (allGood) IDBorderBrush = new SolidColorBrush(Colors.Transparent);
                }
                catch
                {
                    allGood = false;
                }

            }
            else
            {
                allGood = false;
            }

            if (NameText.Trim().Length == 0)
            {
                allGood = false;
            }

            return allGood;
        }
        private void OnAddEntity()
        {
            SaveState();

            FlowMeter newFlowMeter = new FlowMeter
            {
                ID = int.Parse(IDText),
                Name = NameText.Trim()
            };
            string type = (SelectedType as string);
            newFlowMeter.EntityType = new EntityType(type, $"../../Resources/Images/{type.ToLower()}.png");

            FlowMeters.Add(newFlowMeter);

            IDText = string.Empty;
            NameText = string.Empty;
            SelectedType = Types[0];

            HideKeyboard();

            ClearFilters();

            AddEntityCommand.RaiseCanExecuteChanged();

            ToastNotify.RaiseToast(
                    "Successful",
                    $"Created entity!:{newFlowMeter.ID}",
                    Notification.Wpf.NotificationType.Success);


        }

        #endregion

        #region Undo
        private void SaveState()
        {
            MainWindowViewModel.UndoStack.Push(new SaveState<CommandType, object>
                (CommandType.EntityManipulation,
                new ObservableCollection<FlowMeter>(FlowMeters)));
        }

        #endregion

        #region Keyboard Actions

        private void HideKeyboard()
        {
            KeyboardVisibility = Visibility.Hidden;
            IsKeyboardEnabled = false;
        }
        private void Backspace()
        {
            if (!string.IsNullOrEmpty(SelectedTextBox.SelectedText))
            {
                int selectionStart = SelectedTextBox.SelectionStart;
                SelectedTextBox.Text = SelectedTextBox.Text.Remove(selectionStart, SelectedTextBox.SelectionLength);
                SelectedTextBox.CaretIndex = selectionStart;
                return;
            }
            if (SelectedTextBox.Text.Length > 0)
            {


                SelectedTextBox.Text = SelectedTextBox.Text.Remove(SelectedTextBox.Text.Length - 1, 1);
            }
        }

        private void TextBoxGotFocus(object obj)
        {
            if (obj is TextBox textBox)
            {
                SelectedTextBox = textBox;
                SelectedTextBox.Focus();
                KeyboardVisibility = Visibility.Visible;
                IsKeyboardEnabled = true;
            }
        }
        private void TextBoxLostFocus(object obj)
        {
            if (obj is TextBox)
            {
                KeyboardVisibility = Visibility.Hidden;
                IsKeyboardEnabled = false;
            }
        }
        private void InputKey(string keyPressed)
        {
            if (SelectedTextBox != null && !SelectedTextBox.Name.Equals("IDTextBox"))
            {
                SelectedTextBox.Text += keyPressed;
            }
            else if (SelectedTextBox.Name.Equals("IDTextBox"))
            {
                // Create a color animation
                var colorAnimation = new ColorAnimation
                {
                    From = Colors.Red,
                    To = (Color)System.Windows.Application.Current.Resources["PrimaryColorDark"],
                    Duration = TimeSpan.FromSeconds(0.3),
                    AutoReverse = false,
                    RepeatBehavior = new RepeatBehavior(1) // Repeat 3 times
                };

                // Create a storyboard and add the animation to it
                var storyboard = new Storyboard();
                storyboard.Children.Add(colorAnimation);

                // Set the target property to the background color
                Storyboard.SetTargetProperty(colorAnimation, new PropertyPath("(TextBox.Background).(SolidColorBrush.Color)"));

                // Set the target object to the selected TextBox
                Storyboard.SetTarget(colorAnimation, SelectedTextBox);

                // Begin the animation
                storyboard.Begin();
            }

        }
        private void InputNumber(string keyPressed)
        {
            if (SelectedTextBox != null)
            {
                SelectedTextBox.Text += keyPressed;
            }
        }
        #endregion

    }

}
