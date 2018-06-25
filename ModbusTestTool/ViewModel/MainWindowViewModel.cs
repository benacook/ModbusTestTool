using System;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows;
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

        //=====================================================================
        //Properties for bindings
        //=====================================================================

        #region RegisterValues

        //=====================================================================
        //Register Value Bindins
        //=====================================================================

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

        //=====================================================================
        //Page Bindings
        //=====================================================================
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

        //=====================================================================
        //User text input bindings
        //=====================================================================
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

        private bool _writeCyclicChecked;

        public bool WriteCyclicChecked
        {
            get { return _writeCyclicChecked; }
            set
            {
                if (_writeCyclicChecked == value) return;
                _writeCyclicChecked = value;
                RaisePropertyChanged("WriteCyclicChecked");
            }
        }

        private bool _readCyclicChecked;

        public bool ReadCyclicChecked
        {
            get { return _readCyclicChecked; }
            set
            {
                if (_readCyclicChecked == value) return;
                _readCyclicChecked = value;
                RaisePropertyChanged("ReadCyclicChecked");
            }
        }

        #endregion User Input

        #region CommandBase Properties

        //=====================================================================
        //Command bindings
        //=====================================================================
        public CommandBase Write { get; set; }

        public CommandBase Read { get; set; }
        public CommandBase WriteCyclically { get; set; }
        public CommandBase ReadCyclically { get; set; }

        #endregion CommandBase Properties

        #endregion Binding Properties

        #region Init

        //=====================================================================
        //Initialisation
        //=====================================================================

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

        //=====================================================================
        //User Interface Commands
        //=====================================================================

        private async void Write_Executed(object sender,
            EventArgs e)
        {
            try { await ModbusWriteAsync(); }
            catch (Exception ex)
            {
                StopCyclicWrite();
                MessageBox.Show(ex.Message, "Error",
                MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void Read_Executed(object sender,
            EventArgs e)
        {
            try { await ModbusReadAsync(); }
            catch (Exception ex)
            {
                StopCyclicRead();
                MessageBox.Show(ex.Message, "Error",
                MessageBoxButton.OK, MessageBoxImage.Error);
            }
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

        #endregion UI Commands

        #region Modbus Commands

        public async Task ModbusWriteAsync()
        {
            if (Networking.NetworkCheck(IpAddr))
            {
                try
                {
                    ModbusTcp ModbusDevice =
                        new ModbusTcp(502, IpAddr, NodeId);

                    byte[] ModbusResponse = await Task.Run(() =>
                    ModbusDevice.WriteAsync(StartReg, RegQty, RegisterValues,
                    FunctionCode));

                    Response = BitConverter.ToString(ModbusResponse);
                }
                catch (Exception ex)
                {
                    Response = ex.Message;
                    StopCyclicWrite();
                    MessageBox.Show("0x" + ex.HResult.ToString("X") + ": " +
                        ex.Message, "Error", MessageBoxButton.OK,
                        MessageBoxImage.Error);
                }
            }
            else { StopCyclicWrite(); }
        }

        public async Task ModbusReadAsync()
        {
            if (Networking.NetworkCheck(IpAddr))
            {
                try
                {
                    ModbusTcp ModbusDevice =
                        new ModbusTcp(502, IpAddr, NodeId);

                    RegisterValues = await Task.Run(() =>
                    ModbusDevice.ReadAsync(StartReg, RegQty, FunctionCode));
                    int[] RegValInts = new List<int>(RegisterValues).ToArray();
                    byte[] RegValBytes = IntArray.ToByteArray(RegValInts);

                    Response = BitConverter.ToString(RegValBytes);
                }
                catch (Exception ex)
                {
                    Response = ex.Message;
                    StopCyclicRead();
                    MessageBox.Show("0x" + ex.HResult.ToString("X") + ": " +
                        ex.Message, "Error", MessageBoxButton.OK,
                        MessageBoxImage.Error);
                }
            }
            else { StopCyclicRead(); }
        }

        private void StopCyclicWrite()
        {
            cyclicWrite.Stop();
            WriteCyclicChecked = false;
        }

        private void StopCyclicRead()
        {
            cyclicRead.Stop();
            ReadCyclicChecked = false;
        }

        #endregion Modbus Commands
    }
}