using NetworkService;
using NetworkService.Model;
using NetworkService.ViewModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
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

        public ObservableCollection<FlowMeter> FlowMeters = MainWindowViewModel.FlowMeters;

        public ObservableCollection<FlowMeter> FilteredMeteres { get; set; }

        public MyICommand<string> InputKeyCommand
        {
            get; set;
        }

        public MyICommand<object> TextBoxGotFocusCommand
        {
            get; set;
        }

        public MyICommand BackspaceCommand
        {
            get; set;
        }

        public MyICommand<string> InputNumberCommand
        {
            get; set;
        }

        public MyICommand AddEntityCommand
        {
            get; set;
        }

        public MyICommand<object> RemoveEntityCommand
        {
            get;set;
        }

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

        #endregion

        public EntitiesViewModel()
        {
            
            //creating commands for keyboard
            InputKeyCommand = new MyICommand<string>(InputKey);
            InputNumberCommand = new MyICommand<string>(InputNumber);
            TextBoxGotFocusCommand = new MyICommand<object>(TextBoxGotFocus);
            BackspaceCommand = new MyICommand(Backspace);

            //creating commands for adding and removing entities
            AddEntityCommand = new MyICommand(OnAddEntity, CanAddEntity);
            RemoveEntityCommand = new MyICommand<object>(OnRemoveEntity, CanRemoveEntity);

            Types = new List<string>();
            Types.Add("Volume");
            Types.Add("Turbine");
            Types.Add("Electronic");

            IDText = "";
            NameText = "";
            SelectedType = Types[0];
        }

        private bool CanRemoveEntity(object arg)
        {
            throw new NotImplementedException();
        }

        private void OnRemoveEntity(object obj)
        {
            throw new NotImplementedException();
        }

        private void OnAddEntity()
        {
            FlowMeter newFlowMeter = new FlowMeter();
            newFlowMeter.ID = int.Parse(IDText);
            newFlowMeter.Name = NameText;
            string type = (SelectedType as string).ToLower();
            newFlowMeter.EntityType = new EntityType(type, $"/Resources/Images/{type}.png");

            FlowMeters.Add(newFlowMeter);
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
                SelectedTextBox.Text.Remove(SelectedTextBox.Text.Length - 1, 1);
            }
        }

        private void TextBoxGotFocus(object obj)
        {
            if (obj is TextBox textBox)
            {
                SelectedTextBox = textBox;
                SelectedTextBox.Focus();
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
