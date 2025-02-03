using System.Windows;
using System.Windows.Input;

namespace WoodworkManagementApp.Dialogs
{
    public partial class ConfirmationDialog : Window
    {
        public string Message { get; set; }
        public new string Title { get; set; }

        public ConfirmationDialog(string message, string title = "Informacja")
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

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        public static void Show(string message, string title = "Informacja")
        {
            var dialog = new ConfirmationDialog(message, title);
            dialog.ShowDialog();
        }
    }
}