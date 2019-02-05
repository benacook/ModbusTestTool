using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Sockets;
using System.Threading.Tasks;
using Utils;

namespace Modbus
{
    internal class ModbusTcp
    {
        //=====================================================================
        //DecVar
        //=====================================================================
        public int Port { get; set; }

        public string IpAddr { get; set; }
        public int UnitID { get; set; }
        public byte[] ReadResponse = new byte[16];
        public static byte transactionID = 0x00;
        public TcpClient client;

        /// <summary>
        /// Creates an object for Modbus TCP communication.
        /// </summary>
        /// <param name="port"></param>
        /// <param name="ipAddr"></param>
        public ModbusTcp(int port, string ipAddr)
        {
            Port = port;
            IpAddr = ipAddr;
            UnitID = 1;
            client = new TcpClient(IpAddr, port);
        }

        /// <summary>
        /// Creates an object for Modbus TCP communication.
        /// +1 Overload.
        /// </summary>
        /// <param name="port"></param>
        /// <param name="ipAddr"></param>
        /// <param name="unitID"></param>
        public ModbusTcp(int port, string ipAddr, int unitID)
        {
            Port = port;
            IpAddr = ipAddr;
            UnitID = unitID;
            client = new TcpClient(IpAddr, port);
        }

        ~ModbusTcp()
        {
            try
            {
                client.GetStream().Close();
                client.Close();
            }
            catch { }
        }

        //=====================================================================
        //Write to Modbus
        //=====================================================================

        /// <summary>
        /// Write to a device over Modbus
        /// </summary>
        /// <param name="register"></param>
        /// <param name="value"></param>
        /// <param name="modbusFunct"></param>
        public async Task<byte[]> WriteAsync(int register, int value,
            int modbusFunct)
        {
            //=================================================================
            //DecVar
            //=================================================================
            byte[] registerAddr = BitConverter.GetBytes(register);
            byte[] sendValue = BitConverter.GetBytes(value);

            //=================================================================
            //Construct data and send
            //=================================================================
            Byte[] data = { 0x00, transactionID, 0x00, 0x00, 0x00, 0x09,
                Convert.ToByte(UnitID), Convert.ToByte(modbusFunct),
                registerAddr[1], registerAddr[0], 0x00, 0x01, 0x02,
                sendValue[1], sendValue[0] };

            NetworkStream stream = client.GetStream();
            stream.Write(data, 0, data.Length);
            byte sentID = transactionID;
            transactionID++;

            //=================================================================
            //Get the response
            //=================================================================
            byte[] response = new byte[32];
            int bytes = await stream.ReadAsync(response, 0, response.Length);
            stream.Close(100);
            return response;
        }

        public async Task<byte[]> WriteAsync(int startReg, int regQty,
            ObservableCollection<int> registerValues, int functionCode)
        {
            //=================================================================
            //DecVar
            //=================================================================
            byte[] registerAddr = BitConverter.GetBytes(startReg);
            byte[,] sendValues = new byte[regQty, 2];
            byte[] qty = BitConverter.GetBytes(regQty);
            byte[] byteQtyValues = BitConverter.GetBytes(regQty * 2);
            byte[] byteQtyTotal = BitConverter.GetBytes((regQty * 2) + 7);

            for (int i = 0; i < regQty; i++)
            {
                byte[] values = BitConverter.GetBytes(registerValues[i]);
                sendValues[i, 0] = values[0];
                sendValues[i, 1] = values[1];
            }

            //=================================================================
            //Construct data and send
            //=================================================================
            byte[] data1 = { 0x00, transactionID, 0x00, 0x00, byteQtyTotal[1],
                byteQtyTotal[0],
                Convert.ToByte(UnitID), Convert.ToByte(functionCode),
                registerAddr[1], registerAddr[0], qty[1],
                qty[0], byteQtyValues[0]};

            byte[] data2 = new byte[regQty * 2];
            int j = 0;
            for (int i = 0; i < regQty; i++)
            {
                data2[j] = sendValues[i, 1];
                data2[j + 1] = sendValues[i, 0];
                j += 2;
            }

            byte[] data = data1.Concat(data2).ToArray();

            NetworkStream stream = client.GetStream();
            stream.Write(data, 0, data.Length);
            byte sentID = transactionID;
            transactionID++;

            //=================================================================
            //Get the response
            //=================================================================
            byte[] response = new byte[64];
            int bytes = await stream.ReadAsync(response, 0, response.Length);
            stream.Close(100);
            return response;
        }

        public async Task<ObservableCollection<int>> ReadAsync(int startReg,
            int regQty, int functionCode)
        {
            //=================================================================
            //DecVar
            //=================================================================
            byte[] registerAddr = BitConverter.GetBytes(startReg);
            byte[] qty = BitConverter.GetBytes(regQty);
            byte[] byteQtyTotal = BitConverter.GetBytes((regQty * 2) + 7);

            //=================================================================
            //Construct data and send
            //=================================================================
            byte[] data = { 0x00, transactionID, 0x00, 0x00, byteQtyTotal[1],
                byteQtyTotal[0],
                Convert.ToByte(UnitID), Convert.ToByte(functionCode),
                registerAddr[1], registerAddr[0], qty[1],
                qty[0]};

            NetworkStream stream = client.GetStream();
            stream.Write(data, 0, data.Length);
            byte sentID = transactionID++;

            //=================================================================
            //Get the response
            //=================================================================
            byte[] response = new byte[64];
            int bytes = await stream.ReadAsync(response, 0, response.Length);
            stream.Close(100);

            var responseBytes = ByteArray.ToIntArray(response);
            var responseCollection =
                new ObservableCollection<int>(responseBytes);
            return responseCollection;
        }
    }
}