using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using Modbus;
using ViewModels;

namespace ModbusTestTool
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private byte[] ModbusResponse { get; set; }
        private DispatcherTimer dt = new DispatcherTimer();

        public MainWindow()
        {
            try
            {
                InitializeComponent();
                // create and link view model
                MainWindowViewModel vm = new MainWindowViewModel();
                DataContext = vm;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Fatal Error: " +
                    ex.ToString(), "Error", MessageBoxButton.OK,
                    MessageBoxImage.Error);
                this.Close();
            }
        }
    }
}