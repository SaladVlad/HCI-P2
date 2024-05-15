using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;


namespace NetworkService.Model
{
    public static class CanvasBehavior
    {
        public static readonly DependencyProperty MouseLeftButtonDownCommandProperty =
            DependencyProperty.RegisterAttached("MouseLeftButtonDownCommand", typeof(ICommand), typeof(CanvasBehavior), new PropertyMetadata(null, OnMouseLeftButtonDownCommandChanged));

        public static readonly DependencyProperty MouseLeftButtonUpCommandProperty =
            DependencyProperty.RegisterAttached("MouseLeftButtonUpCommand", typeof(ICommand), typeof(CanvasBehavior), new PropertyMetadata(null, OnMouseLeftButtonUpCommandChanged));

        public static readonly DependencyProperty DropCommandProperty =
            DependencyProperty.RegisterAttached("DropCommand", typeof(ICommand), typeof(CanvasBehavior), new PropertyMetadata(null, OnDropCommandChanged));

        public static ICommand GetMouseLeftButtonDownCommand(DependencyObject obj)
        {
            return (ICommand)obj.GetValue(MouseLeftButtonDownCommandProperty);
        }

        public static void SetMouseLeftButtonDownCommand(DependencyObject obj, ICommand value)
        {
            obj.SetValue(MouseLeftButtonDownCommandProperty, value);
        }

        public static ICommand GetMouseLeftButtonUpCommand(DependencyObject obj)
        {
            return (ICommand)obj.GetValue(MouseLeftButtonUpCommandProperty);
        }

        public static void SetMouseLeftButtonUpCommand(DependencyObject obj, ICommand value)
        {
            obj.SetValue(MouseLeftButtonUpCommandProperty, value);
        }

        public static ICommand GetDropCommand(DependencyObject obj)
        {
            return (ICommand)obj.GetValue(DropCommandProperty);
        }

        public static void SetDropCommand(DependencyObject obj, ICommand value)
        {
            obj.SetValue(DropCommandProperty, value);
        }

        private static void OnMouseLeftButtonDownCommandChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is UIElement element)
            {
                element.MouseLeftButtonDown -= MouseLeftButtonDown;
                element.MouseLeftButtonDown += MouseLeftButtonDown;
            }
        }

        private static void OnMouseLeftButtonUpCommandChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is UIElement element)
            {
                element.MouseLeftButtonUp -= MouseLeftButtonUp;
                element.MouseLeftButtonUp += MouseLeftButtonUp;
            }
        }

        private static void OnDropCommandChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is UIElement element)
            {
                element.Drop -= Drop;
                element.Drop += Drop;
                element.AllowDrop = true;
            }
        }

        private static void MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var element = sender as UIElement;
            var command = GetMouseLeftButtonDownCommand(element);
            command?.Execute(e);
        }

        private static void MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var element = sender as UIElement;
            var command = GetMouseLeftButtonUpCommand(element);
            command?.Execute(e);
        }

        private static void Drop(object sender, DragEventArgs e)
        {
            var element = sender as UIElement;
            var command = GetDropCommand(element);
            command?.Execute(e);
        }
    }
}

