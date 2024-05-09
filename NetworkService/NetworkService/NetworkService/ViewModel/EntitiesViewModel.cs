using NetworkService.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;

namespace NetworkService.Views
{
    public class EntitiesViewModel : BindableBase
    {

        public List<string> Types { get; set; }

        private TextBox _selectedTextBox;
        public TextBox SelectedTextBox
        {
            get { return _selectedTextBox; }
            set
            {
                _selectedTextBox = value;
                OnPropertyChanged(nameof(SelectedTextBox));
            }
        }

        public ObservableCollection<FlowMeter> FilteredMeteres { get; set; }

        private ICommand _inputKeyCommand;
        public ICommand InputKeyCommand
        {
            get { return _inputKeyCommand; }
            set
            {
                _inputKeyCommand = value;
                OnPropertyChanged(nameof(InputKeyCommand));
            }
        }

        private ICommand _textBoxGotFocusCommand;
        public ICommand TextBoxGotFocusCommand
        {
            get { return _textBoxGotFocusCommand; }
            set
            {
                _textBoxGotFocusCommand = value;
                OnPropertyChanged(nameof(TextBoxGotFocusCommand));
            }
        }

        private ICommand _backspaceCommand;

        public ICommand BackspaceCommand
        {
            get { return _backspaceCommand; }
            set
            {
                _backspaceCommand = value;
                OnPropertyChanged(nameof(BackspaceCommand));
            }
        }

        public EntitiesViewModel()
        {
            Types = new List<string>();
            Types.Add("Volume");
            Types.Add("Turbine");
            Types.Add("Electronic");

            InputKeyCommand = new MyICommand<string>(InputKey);
            TextBoxGotFocusCommand = new MyICommand<object>(TextBoxGotFocus);
            BackspaceCommand = new MyICommand(Backspace);
            
        }

        private void Backspace()
        {
            if (SelectedTextBox.Text.Length > 0)
            {
                SelectedTextBox.Text.Remove(SelectedTextBox.Text.Length - 1, 1);
            }
        }

        private void TextBoxGotFocus(object obj)
        {
            Console.WriteLine("Changed focus");
            
            if(obj is TextBox textBox)
            {
                SelectedTextBox = textBox;
                SelectedTextBox.Focus();
            }
        }

        private void InputKey(string keyPressed)
        {

            //TODO find selected textbox in view and append the appropriate key to it
            SelectedTextBox.Text += keyPressed;
            
            
        }
    }


}
