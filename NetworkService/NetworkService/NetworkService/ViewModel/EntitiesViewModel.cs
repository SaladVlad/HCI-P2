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

namespace NetworkService.Views
{
    public class EntitiesViewModel : BindableBase
    {
        #region Properties and Commands
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
            set {
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
            set {
                SetProperty(ref _isLowerThanChecked, value);
                if (_isLowerThanChecked)
                {
                    IsEqualChecked = false;
                    IsGreaterThanChecked = false;
                }
            }
        }
        private bool _isEqualChecked;
        public bool IsEqualChecked
        {
            get => _isEqualChecked;
            set {
                SetProperty(ref _isEqualChecked, value);
                if (_isEqualChecked)
                {
                    IsLowerThanChecked = false;
                    IsGreaterThanChecked = false;
                }
                
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
                    IsLowerThanChecked= false;
                    IsEqualChecked = false;
                }
            }
        }

        #endregion

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

            FilterCommand = new MyICommand(Filter);
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
            //TODO filter out data and display it inside the listview

            char selectedOption = ' ';
            if (IsEqualChecked)
                selectedOption = '=';
            else if (IsLowerThanChecked)
                selectedOption = '<';
            else if(IsGreaterThanChecked)
                selectedOption = '>';


            if(!string.IsNullOrWhiteSpace(FilterText) && selectedOption != ' ')
            {
                // NEEDS TO SELECT A FILTER OPTION,

                //TODO feedback
                return;
            }

            int filterID = FilterText != null && FilterText!= "" ? int.Parse(FilterText) : -1; ;

            FilteredMeters.Clear();
            foreach(FlowMeter flowMeter in FlowMeters)
            {
                if (selectedOption == '=')
                {
                    if (FilterType != null)
                    {
                        if (flowMeter.ID == filterID && flowMeter.EntityType.Name.Equals(FilterType))
                        {
                            FilteredMeters.Add(flowMeter);
                        }
                    }
                    else
                    {
                        if(flowMeter.ID == filterID)
                        {
                            FilteredMeters.Add(flowMeter);
                        }
                    }

                    
                }
                if(selectedOption == '<')
                {
                    if (FilterType != null)
                    {
                        if (flowMeter.ID < filterID && flowMeter.EntityType.Name.Equals(FilterType))
                        {
                            FilteredMeters.Add(flowMeter);
                        }
                    }
                    else
                    {
                        if (flowMeter.ID < filterID)
                        {
                            FilteredMeters.Add(flowMeter);
                        }
                    }
                }
                if (selectedOption == '>')
                {
                    if (FilterType != null)
                    {
                        if (flowMeter.ID > filterID && flowMeter.EntityType.Name.Equals(FilterType))
                        {
                            FilteredMeters.Add(flowMeter);
                        }
                    }
                    else
                    {
                        if (flowMeter.ID > filterID)
                        {
                            FilteredMeters.Add(flowMeter);
                        }
                    }
                }
                //nothing is selected, filter only by the category
                if (selectedOption == ' ')
                {
                    if (FilterType != null)
                    {
                        if (flowMeter.EntityType.Name.Equals(FilterType))
                        {
                            FilteredMeters.Add(flowMeter);
                        }
                    }
                }
            }

        }

        private void HideKeyboard()
        {
            KeyboardVisibility = Visibility.Hidden;
            IsKeyboardEnabled = false;
        }

        private void TextChanged()
        {

        }

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
            FlowMeters.Remove(SelectedEntity);
            SelectedEntity = null;
        }

        private void OnAddEntity()
        {
            FlowMeter newFlowMeter = new FlowMeter();
            newFlowMeter.ID = int.Parse(IDText);
            newFlowMeter.Name = NameText;
            string type = (SelectedType as string);
            newFlowMeter.EntityType = new EntityType(type, $"../../Resources/Images/{type.ToLower()}.png");

            FlowMeters.Add(newFlowMeter);

            ClearFilters();
            //raise some toast or something, tell the user it was successful
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



        #region Keyboard Actions
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
            if (SelectedTextBox != null)
            {
                SelectedTextBox.Text += keyPressed;
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
