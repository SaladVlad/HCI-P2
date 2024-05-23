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

namespace NetworkService.Views
{
    public class EntitiesViewModel : BindableBase
    {
        #region Properties and Commands

        #region Properties
        public List<string> Types { get; set; }

        private object _selectedType;
        public object SelectedType {

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
            set => SetProperty(ref _filterType, value);
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
        public MyICommand<string> InputKeyCommand
        {
            get; set;
        }
        public MyICommand<object> TextBoxGotFocusCommand
        {
            get; set;
        }
        public MyICommand<object> TextBoxLostFocusCommand { get; set; }
        public MyICommand HideKeyboardCommand { get; set; }
        public MyICommand BackspaceCommand
        {
            get; set;
        }
        public MyICommand<string> InputNumberCommand
        {
            get; set;
        }
        public MyICommand TextChangedCommand
        {
            get;set;
        }
        public MyICommand AddEntityCommand
        {
            get; set;
        }
        public MyICommand RemoveEntityCommand
        {
            get;set;
        }
        public MyICommand FilterCommand { get; set; }
        public MyICommand ClearFiltersCommand { get; set; }

        #endregion

        #endregion


        #region Constructor
        public EntitiesViewModel()
        {
            
            FlowMeters = MainWindowViewModel.FlowMeters;
            FilteredMeters = new ObservableCollection<FlowMeter>();

            foreach(FlowMeter f in FlowMeters)
            {
                FilteredMeters.Add(f);
            }

            //creating commands for keyboard
            InputKeyCommand = new MyICommand<string>(InputKey);
            InputNumberCommand = new MyICommand<string>(InputNumber);

            TextBoxGotFocusCommand = new MyICommand<object>(TextBoxGotFocus);
            TextBoxLostFocusCommand = new MyICommand<object>(TextBoxLostFocus);

            BackspaceCommand = new MyICommand(Backspace);
            TextChangedCommand = new MyICommand(TextChanged);

            HideKeyboardCommand = new MyICommand(HideKeyboard);

            //creating commands for adding and removing entities
            AddEntityCommand = new MyICommand(OnAddEntity, CanAddEntity);
            RemoveEntityCommand = new MyICommand(OnRemoveEntity, CanRemoveEntity);

            FilterCommand = new MyICommand(Filter,CanFilter);
            ClearFiltersCommand = new MyICommand(ClearFilters);

            Types = new List<string>();
            Types.Add("Volume");
            Types.Add("Turbine");
            Types.Add("Electronic");

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
            if (int.TryParse(FilterText, out _)&&
                (IsEqualChecked || IsGreaterThanChecked || IsLowerThanChecked))
            {
                return true;
            }
            else if(FilterType!=null && (int.TryParse(FilterText, out _) &&
                !(IsEqualChecked || IsGreaterThanChecked || IsLowerThanChecked))){
                return false;
            }
            else if (FilterType != null && (int.TryParse(FilterText, out _) &&
                (IsEqualChecked || IsGreaterThanChecked || IsLowerThanChecked)))
            {
                return true;
            }
            else if (FilterType == null && (int.TryParse(FilterText, out _) &&
                !(IsEqualChecked || IsGreaterThanChecked || IsLowerThanChecked)))
            {
                return false;
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
            //TODO warning about not selecting anything


            FilteredMeters.Clear();
            foreach (FlowMeter flowMeter in FlowMeters)
            {
                //filter by type
                if(FilterType!=null || !string.IsNullOrEmpty(FilterType))
                {
                    if (flowMeter.EntityType.Name.Equals(FilterType))
                    {
                        FilteredMeters.Add(flowMeter);
                    }
                }
                else
                {
                    FilteredMeters.Add(flowMeter);
                }

                //second filter pass
                if((IsEqualChecked||IsGreaterThanChecked||IsLowerThanChecked) && !string.IsNullOrWhiteSpace(FilterText))
                {
                    if (IsLowerThanChecked && flowMeter.ID<int.Parse(FilterText))
                    {
                        FilteredMeters.Add(flowMeter);
                    }
                    if (IsEqualChecked && flowMeter.ID == int.Parse(FilterText))
                    {
                        FilteredMeters.Add(flowMeter);
                    }
                    if (IsGreaterThanChecked && flowMeter.ID < int.Parse(FilterText))
                    {
                        FilteredMeters.Add(flowMeter);
                    }
                }
                else
                {
                    FilteredMeters.Add(flowMeter);
                }
            }
            //TODO toast of successful filtering
        }

        #endregion

        private void TextChanged()
        {

        }

        #region Creating/Removing
        private bool CanRemoveEntity()
        {
            if(SelectedEntity!=null)
            {
                return true;
            }
            return false;
        }
        private void OnRemoveEntity()
        {
            if(MessageBox.Show(
                "Are you sure you want to remove the selected entity?",
                "Confirmation Dialog",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                SaveState();
                FlowMeters.Remove(SelectedEntity);

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
                            allGood = false;
                            break;
                        }
                    }

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

            FlowMeter newFlowMeter = new FlowMeter();
            newFlowMeter.ID = int.Parse(IDText);
            newFlowMeter.Name = NameText;
            string type = (SelectedType as string);
            newFlowMeter.EntityType = new EntityType(type, $"../../Resources/Images/{type.ToLower()}.png");

            FlowMeters.Add(newFlowMeter);

            HideKeyboard();

            ClearFilters();

            ToastNotify.RaiseToast(
                    "Successful",
                    $"Created entity!:{newFlowMeter.ID}",
                    Notification.Wpf.NotificationType.Success);

        }
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
            if (obj is TextBox textBox)
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
            else if(SelectedTextBox.Name.Equals("IDTextBox"))
            {
                //TODO warning about using only numbers
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
