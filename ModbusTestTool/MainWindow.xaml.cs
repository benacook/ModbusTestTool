using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using Modbus;

namespace ModbusTestTool
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private byte[] ModbusResponse { get; set; }
        private DispatcherTimer dt = new DispatcherTimer();
        private ModbusTcp ModbusDevice = new ModbusTcp(502, "192.168.25.220");
        private int value = 0;

        public MainWindow()
        {
            InitializeComponent();
        }

        private async void OnTimerTickAsync(object sender, EventArgs e)
        {
            try
            {
                int ModbusRegister = Convert.ToInt16(Register.Text);
                int ModbusValue = value;
                int ModbusFunct = Convert.ToInt16(Function.Text);

                ModbusResponse = await Task.Run(() =>
                ModbusDevice.WriteAsync(ModbusRegister, ModbusValue,
                ModbusFunct));

                Response.Text = BitConverter.ToString(ModbusResponse);
                value++;
            }
            catch (Exception ex)
            {
                Response.Text = ex.Message;
                MessageBox.Show("0x" + ex.HResult.ToString("X") + ": " +
                    ex.Message, "Error", MessageBoxButton.OK,
                    MessageBoxImage.Error);
                RunCyc.IsChecked = false;
            }
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                int ModbusRegister = Convert.ToInt16(Register.Text);
                int ModbusValue = Convert.ToInt16(Value.Text);
                int ModbusFunct = Convert.ToInt16(Function.Text);
                //ModbusTcp ModbusDevice = new ModbusTcp(502, IpAddr.Text);

                ModbusResponse = await Task.Run(() =>
                ModbusDevice.WriteAsync(ModbusRegister,
                ModbusValue, ModbusFunct));

                Response.Text = BitConverter.ToString(ModbusResponse);
            }
            catch (Exception ex)
            {
                Response.Text = ex.Message;
                MessageBox.Show("0x" + ex.HResult.ToString("X") + ": " +
                    ex.Message, "Error", MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            dt.IsEnabled = false;
            dt.Interval = TimeSpan.FromMilliseconds(500);
            dt.Tick += OnTimerTickAsync;
            dt.Start();
            SendBtn.IsEnabled = false;
        }

        private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            dt.Stop();
            SendBtn.IsEnabled = true;
        }
    }
}