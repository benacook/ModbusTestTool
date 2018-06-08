using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Modbus;

namespace ModbusTestTool
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private byte[] ModbusResponse { get; set; }

        public MainWindow()
        {
            InitializeComponent();
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            uint ModbusRegister = Convert.ToUInt16(Register.Text);
            int ModbusValue = Convert.ToInt16(Value.Text);
            int ModbusFunct = Convert.ToInt16(Function.Text);
            ModbusTcp ModbusDevice = new ModbusTcp(502, IpAddr.Text);

            try
            {
                ModbusResponse = await Task.Run(() =>
                ModbusDevice.WriteAsync(ModbusRegister, ModbusValue,
                ModbusFunct));

                Response.Text = BitConverter.ToString(ModbusResponse);
            }
            catch (Exception ex)
            {
                Response.Text = ex.Message;
                MessageBox.Show(ex.HResult + ": " + ex.Message, "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}