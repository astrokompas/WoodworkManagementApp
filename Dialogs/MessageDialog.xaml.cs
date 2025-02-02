using System.Windows.Input;
using System.Windows;


namespace WoodworkManagementApp.Dialogs
{
    public partial class MessageDialog : Window
    {
        public string Message { get; set; }
        public new string Title { get; set; }

        public MessageDialog(string message, string title = "Potwierdzenie")
        {
            InitializeComponent();
            Message = message;
            Title = title;
            DataContext = this;
        }

        private void Border_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
                Close();
            else
                DragMove();
        }

        private void YesButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        private void NoButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        public static bool Show(string message, string title = "Potwierdzenie")
        {
            var dialog = new MessageDialog(message, title);
            return dialog.ShowDialog() == true;
        }
    }
}