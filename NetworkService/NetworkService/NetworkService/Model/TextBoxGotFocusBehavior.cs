using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace NetworkService.Model
{
    public static class TextBoxGotFocusBehavior
    {
        public static readonly DependencyProperty GotFocusCommandProperty =
            DependencyProperty.RegisterAttached("GotFocusCommand", typeof(ICommand), typeof(TextBoxGotFocusBehavior), new PropertyMetadata(null, OnGotFocusCommandChanged));

        public static ICommand GetGotFocusCommand(DependencyObject obj)
        {
            return (ICommand)obj.GetValue(GotFocusCommandProperty);
        }

        public static void SetGotFocusCommand(DependencyObject obj, ICommand value)
        {
            obj.SetValue(GotFocusCommandProperty, value);
        }

        private static void OnGotFocusCommandChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is TextBox textBox)
            {
                textBox.GotFocus -= TextBox_GotFocus;
                textBox.GotFocus += TextBox_GotFocus;
            }
        }

        private static void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            var textBox = sender as TextBox;
            var command = GetGotFocusCommand(textBox);
            command?.Execute(textBox);
        }
    }
}
