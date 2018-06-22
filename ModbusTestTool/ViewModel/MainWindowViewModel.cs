using System;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows;
using System.Net.NetworkInformation;
using System.Net;
using System.Collections.ObjectModel;
using Commands;
using System.Windows.Threading;
using Modbus;
using System.Collections.Generic;
using Utils;

namespace ViewModels
{
    internal class MainWindowViewModel : ViewModelBase
    {
        #region decVar

        #region Timers

        private DispatcherTimer cyclicWrite;
        private DispatcherTimer cyclicRead;

        #endregion Timers

        #endregion decVar

        #region Binding Properties

        ///////////////////////////////////////////////////////////////////////
        //Properties for bindings
        ///////////////////////////////////////////////////////////////////////

        #region RegisterValues

        ///////////////////////////////////////////////////////////////////////
        //Register Value Bindins
        ///////////////////////////////////////////////////////////////////////

        private ObservableCollection<int> _registerValues =
            new ObservableCollection<int>
            {0,0,0,0,0,0,0,0,0,0,
            0,0,0,0,0,0,0,0,0,0};

        public ObservableCollection<int> RegisterValues
        {
            get { return _registerValues; }
            set
            {
                if (_registerValues == value) return;
                _registerValues = value;
                RaisePropertyChanged("RegisterValues");
            }
        }

        #endregion RegisterValues

        #region pages

        ///////////////////////////////////////////////////////////////////////
        //Page Bindings
        ///////////////////////////////////////////////////////////////////////
        private Page _currentPage;

        public Page currentPage
        {
            get { return _currentPage; }
            set
            {
                if (_currentPage == value) return;
                _currentPage = value;
                RaisePropertyChanged("currentPage");
            }
        }

        #endregion pages

        #region User Input

        ///////////////////////////////////////////////////////////////////////
        //User text input bindings
        ///////////////////////////////////////////////////////////////////////
        private string _ipAddr;

        public string IpAddr
        {
            get { return _ipAddr; }
            set
            {
                if (_ipAddr == value) return;
                _ipAddr = value;
                RaisePropertyChanged("IpAddr");
            }
        }

        private int _nodeID;

        public int NodeId
        {
            get { return _nodeID; }
            set
            {
                if (_nodeID == value) return;
                _nodeID = value;
                RaisePropertyChanged("NodeID");
            }
        }

        private int _functionCode;

        public int FunctionCode
        {
            get { return _functionCode; }
            set
            {
                if (_functionCode == value) return;
                _functionCode = value;
                RaisePropertyChanged("FunctionCode");
            }
        }

        private int _startReg;

        public int StartReg
        {
            get { return _startReg; }
            set
            {
                if (_startReg == value) return;
                _startReg = value;
                RaisePropertyChanged("StartReg");
            }
        }

        private int _regQty;

        public int RegQty
        {
            get { return _regQty; }
            set
            {
                if (_regQty == value) return;
                _regQty = value;
                RaisePropertyChanged("RegQty");
            }
        }

        private string _response;

        public string Response
        {
            get { return _response; }
            set
            {
                if (_response == value) return;
                _response = value;
                RaisePropertyChanged("Response");
            }
        }

        #endregion User Input

        #region CommandBase Properties

        ///////////////////////////////////////////////////////////////////////
        //Command bindings
        ///////////////////////////////////////////////////////////////////////
        public CommandBase Write { get; set; }

        public CommandBase Read { get; set; }
        public CommandBase WriteCyclically { get; set; }
        public CommandBase ReadCyclically { get; set; }

        #endregion CommandBase Properties

        #endregion Binding Properties

        #region Init

        ///////////////////////////////////////////////////////////////////////
        //Initialisation
        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Initialisation of the main application.
        /// </summary>
        public MainWindowViewModel()
        {
            InitialiseCommands();
            InitialiseDispatch();
        }

        /// <summary>
        /// Initialises commands for the MVVM.
        /// </summary>
        private void InitialiseCommands()
        {
            #region Main Window Page Control

            Write =
                new CommandBase(Write_Executed);
            Read =
                new CommandBase(Read_Executed);
            WriteCyclically =
                new CommandBase(WriteCyclically_Executed);
            ReadCyclically =
                new CommandBase(ReadCyclically_Executed);

            #endregion Main Window Page Control
        }

        /// <summary>
        /// Initialises the timed events.
        /// </summary>
        private void InitialiseDispatch()
        {
            cyclicWrite =
                new DispatcherTimer();
            cyclicWrite.Tick += new EventHandler(Write_Executed);
            cyclicWrite.Interval = new TimeSpan(0, 0, 1);

            cyclicRead =
                new DispatcherTimer();
            cyclicRead.Tick += new EventHandler(Read_Executed);
            cyclicRead.Interval = new TimeSpan(0, 0, 1);
        }

        #endregion Init

        #region UI Commands

        ///////////////////////////////////////////////////////////////////////
        //User Interface Commands
        ///////////////////////////////////////////////////////////////////////

        #region Page Selection

        ///////////////////////////////////////////////////////////////////////
        //Page selection
        ///////////////////////////////////////////////////////////////////////

        private async void Write_Executed(object sender,
            EventArgs e)
        {
            await ModbusWriteAsync();
        }

        private async void Read_Executed(object sender,
            EventArgs e)
        {
            await ModbusReadAsync();
        }

        private void WriteCyclically_Executed(object sender,
            EventArgs e)
        {
            if (!cyclicWrite.IsEnabled)
            { cyclicWrite.Start(); }
            else { cyclicWrite.Stop(); }
        }

        private void ReadCyclically_Executed(object sender,
            EventArgs e)
        {
            if (!cyclicRead.IsEnabled)
            { cyclicRead.Start(); }
            else { cyclicRead.Stop(); }
        }

        #endregion Page Selection

        #endregion UI Commands

        #region Network Commands

        public bool NetworkingCheck()
        {
            bool pingOK = false;
            bool subnetOK = false;
            if (!SubnetCheck())
            {
                MessageBox.Show("Device IP not in" +
                    "Subnet range of this PC", "Error", MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
            else { subnetOK = true; }
            if (!PingCheck())
            {
                MessageBox.Show("Cannot ping Device",
                    "Error", MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
            else { pingOK = true; }

            return pingOK & subnetOK;
        }

        ///////////////////////////////////////////////////////////////////////
        //TCP network related commands
        ///////////////////////////////////////////////////////////////////////
        /// <summary>
        /// Checks if the IP address of the TM5
        /// is in the subnet of the computer.
        /// </summary>
        /// <returns></returns>
        private bool SubnetCheck()
        {
            string IP = IpAddr;
            Char splitAt = '.';
            String[] deviceIP = IP.Split(splitAt);
            bool match = false;
            var hostIpAddress = GetHostAddress();

            for (int i = 0; i < hostIpAddress.Length; i++)
            {
                if (hostIpAddress[i] != null)
                {
                    String ipAddrStr = hostIpAddress[i].ToString();
                    Char delimiter = '.';
                    String[] substrings = ipAddrStr.Split(delimiter);
                    for (int j = 0; j < substrings.Length - 1; j++)
                    {
                        if (substrings[j] != deviceIP[j])
                        {
                            match = false;
                            break;
                        }
                        else { match = true; }
                    }
                    if (match == true)
                    { break; }
                }
            }
            if (match)
            {
                return true;
            }
            else { return false; }
        }

        /// <summary>
        /// Checks is the IP address of the TM5 is accessible.
        /// </summary>
        /// <returns></returns>
        private bool PingCheck()
        {
            // Pings the machine on local network.
            try
            {
                IPAddress userInIpConvert = IPAddress.Parse(IpAddr);
                Ping pingSender = new Ping();
                IPAddress address = userInIpConvert;
                PingReply reply = pingSender.Send(address);

                if (reply.Status == IPStatus.Success)
                {
                    return true;
                }
                else { return false; }
            }
            catch { return false; }
        }

        /// <summary>
        /// Gets the IP addresses of the computer.
        /// </summary>
        private IPAddress[] GetHostAddress()
        {
            var hostName = Dns.GetHostName();
            IPHostEntry hostIpAddressList = Dns.GetHostEntry(hostName);
            var hostIpAddress = hostIpAddressList.AddressList;
            for (int i = 0; i < hostIpAddressList.AddressList.Length; i++)
            {
                byte[] IpLengthChecker =
                    hostIpAddressList.AddressList[i].GetAddressBytes();
                if (IpLengthChecker.Length > 4)
                {
                    hostIpAddress[i] = null;
                }
            }
            return hostIpAddress;
        }

        #endregion Network Commands

        #region Modbus Commands

        public async Task ModbusWriteAsync()
        {
            try
            {
                ModbusTcp ModbusDevice = new ModbusTcp(502, IpAddr);

                byte[] ModbusResponse = await Task.Run(() =>
                ModbusDevice.WriteAsync(StartReg, RegQty, RegisterValues,
                FunctionCode));

                Response = BitConverter.ToString(ModbusResponse);
            }
            catch (Exception ex)
            {
                Response = ex.Message;
                MessageBox.Show("0x" + ex.HResult.ToString("X") + ": " +
                    ex.Message, "Error", MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        public async Task ModbusReadAsync()
        {
            try
            {
                ModbusTcp ModbusDevice = new ModbusTcp(502, IpAddr);

                RegisterValues = await Task.Run(() =>
                ModbusDevice.ReadAsync(StartReg, RegQty, FunctionCode));
                int[] RegValInts = new List<int>(RegisterValues).ToArray();
                byte[] RegValBytes = IntArray.ToByteArray(RegValInts);

                Response = BitConverter.ToString(RegValBytes);
            }
            catch (Exception ex)
            {
                Response = ex.Message;
                MessageBox.Show("0x" + ex.HResult.ToString("X") + ": " +
                    ex.Message, "Error", MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        #endregion Modbus Commands
    }
}