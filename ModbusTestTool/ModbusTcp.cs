using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Modbus
{
    internal class ModbusTcp
    {
        ///////////////////////////////////////////////////////////////////////
        //DecVar
        ///////////////////////////////////////////////////////////////////////
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
            client = new TcpClient(IpAddr, 502);
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
            client = new TcpClient(IpAddr, 502);
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

        ///////////////////////////////////////////////////////////////////////
        //Write to Modbus
        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Write to a device over Modbus
        /// </summary>
        /// <param name="register"></param>
        /// <param name="value"></param>
        /// <param name="modbusFunct"></param>
        public async Task<byte[]> WriteAsync(int register, int value,
            int modbusFunct)
        {
            ///////////////////////////////////////////////////////////////////
            //DecVar
            ///////////////////////////////////////////////////////////////////
            byte[] registerAddr = BitConverter.GetBytes(register);
            byte[] sendValue = BitConverter.GetBytes(value);

            ///////////////////////////////////////////////////////////////////
            //Construct data and send
            ///////////////////////////////////////////////////////////////////
            Byte[] data = { 0x00, transactionID, 0x00, 0x00, 0x00, 0x09,
                Convert.ToByte(UnitID), Convert.ToByte(modbusFunct),
                registerAddr[1], registerAddr[0], 0x00, 0x01, 0x02,
                sendValue[1], sendValue[0] };

            NetworkStream stream = client.GetStream();
            stream.Write(data, 0, data.Length);
            byte sentID = transactionID;
            transactionID++;

            ///////////////////////////////////////////////////////////////////
            //Get the response
            ///////////////////////////////////////////////////////////////////
            byte[] response = new byte[32];
            int bytes = await stream.ReadAsync(response, 0, response.Length);
            //stream.Close(100);
            return response;
        }

        ///////////////////////////////////////////////////////////////////////
        //Read Modbus
        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Request data over Modbus and get the response asynchronously.
        /// </summary>
        /// <param name="register"></param>
        /// <param name="quantity"></param>
        /// <param name="modbusFunct"></param>
        /// <returns></returns>
        public async Task<byte[]> ReadAsync(int register, int quantity,
            int modbusFunct)
        {
            ///////////////////////////////////////////////////////////////////
            //DecVar
            ///////////////////////////////////////////////////////////////////
            byte[] registerAddr = BitConverter.GetBytes(register);
            byte[] receiveQty = BitConverter.GetBytes(quantity);
            TcpClient client = new TcpClient(IpAddr, Port);
            NetworkStream stream = client.GetStream();

            ///////////////////////////////////////////////////////////////////
            //Construct data and send
            ///////////////////////////////////////////////////////////////////
            Byte[] data = { 0x00, transactionID, 0x00, 0x00, 0x00, 0x06,
                Convert.ToByte(UnitID), Convert.ToByte(modbusFunct),
                registerAddr[1], registerAddr[0], receiveQty[1],
                receiveQty[0] };

            stream.Write(data, 0, data.Length);
            byte sentID = transactionID;
            transactionID++;

            ///////////////////////////////////////////////////////////////////
            //Get the response
            ///////////////////////////////////////////////////////////////////
            var response = new Byte[256];
            int bytes = await stream.ReadAsync(response, 0, response.Length);
            if (response.Length > 0)
            {
                if (response[1] == sentID)
                {
                    ReadResponse[0] = response[8];
                    for (int i = 1; i <= response[8]; i++)
                    {
                        ReadResponse[i] = response[8 + i];
                    }
                }
            }
            stream.Close(100);
            return ReadResponse;
        }
    }
}