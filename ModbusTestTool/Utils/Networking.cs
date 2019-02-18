using System.Net;
using System.Net.NetworkInformation;
using System.Windows;

namespace Utils
{
    internal class Networking
    {
        //=====================================================================
        //Network Check
        //=====================================================================
        //Checks if the IP is valid
        //Checks if IP in range
        //Checks if can ping IP
        //---------------------------------------------------------------------

        /// <summary>
        /// </summary>
        /// <param name="ipAddr"></param>
        /// <returns></returns>
        public static bool NetworkCheck(string ipAddr)
        {
            if (!IsIPValid(ipAddr))
            {
                MessageBox.Show("Invalid IP Address", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            if (!SubnetCheck(ipAddr))
            {
                MessageBox.Show("Device IP not in " +
                    "subnet range of this PC", "Warning", MessageBoxButton.OK,
                    MessageBoxImage.Warning);
            }

            if (!PingCheck(ipAddr))
            {
                MessageBox.Show("Cannot ping Device",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            return true;
        }

        /// <summary>
        /// </summary>
        /// <param name="ipAddr"></param>
        /// <returns></returns>
        public static bool NetworkCheck(IPAddress ipAddr)
        {
            if (!IsIPValid(ipAddr))
            {
                MessageBox.Show("Invalid IP Address", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            if (!SubnetCheck(ipAddr))
            {
                MessageBox.Show("Device IP not in " +
                    "subnet range of this PC", "Error", MessageBoxButton.OK,
                    MessageBoxImage.Error);
                return false;
            }

            if (!PingCheck(ipAddr))
            {
                MessageBox.Show("Cannot ping Device",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            return true;
        }

        //=====================================================================
        //Is Ip Address Valid
        //=====================================================================

        /// <summary>
        /// </summary>
        /// <param name="ipAddr"></param>
        /// <returns></returns>
        public static bool IsIPValid(string ipAddr)
        {
            IPAddress deviceIP = new IPAddress(0);
            IPAddress.TryParse(ipAddr, out deviceIP);
            byte[] deviceIPBytes = deviceIP.GetAddressBytes();
            for (int i = 0; i < deviceIPBytes.Length; i++)
            {
                if (deviceIPBytes[i] > 255)
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// </summary>
        /// <param name="ipAddr"></param>
        /// <returns></returns>
        public static bool IsIPValid(IPAddress ipAddr)
        {
            byte[] deviceIPBytes = ipAddr.GetAddressBytes();
            for (int i = 0; i < deviceIPBytes.Length; i++)
            {
                if (deviceIPBytes[i] > 255)
                {
                    return false;
                }
            }
            return true;
        }

        //=====================================================================
        //Is Ip Address in subnet range
        //=====================================================================

        /// <summary>
        /// Checks if the IP address of the TM5 is in the subnet of the computer.
        /// </summary>
        /// <returns></returns>
        public static bool SubnetCheck(string ipAddr)
        {
            IPAddress deviceIP = new IPAddress(0);
            IPAddress.TryParse(ipAddr, out deviceIP);
            byte[] deviceIPBytes = deviceIP.GetAddressBytes();

            //Check if ip address is interal (127.x.x.x)
            if (deviceIPBytes[0] == 127)
            { return true; }

            bool match = false;
            var hostIpAddress = GetHostAddress();

            for (int i = 0; i < hostIpAddress.Length; i++)
            {
                if (hostIpAddress[i] != null)
                {
                    byte[] ipAddrByte = hostIpAddress[i].GetAddressBytes();
                    for (int j = 0; j < ipAddrByte.Length - 1; j++)
                    {
                        if (ipAddrByte[j] != deviceIPBytes[j])
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
        /// </summary>
        /// <param name="ipAddr"></param>
        /// <returns></returns>
        public static bool SubnetCheck(IPAddress ipAddr)
        {
            byte[] deviceIPBytes = ipAddr.GetAddressBytes();

            //Check if ip address is interal (127.x.x.x)
            if (deviceIPBytes[0] == 127)
            { return true; }

            bool match = false;
            var hostIpAddress = GetHostAddress();

            for (int i = 0; i < hostIpAddress.Length; i++)
            {
                if (hostIpAddress[i] != null)
                {
                    byte[] ipAddrByte = hostIpAddress[i].GetAddressBytes();
                    for (int j = 0; j < ipAddrByte.Length - 1; j++)
                    {
                        if (ipAddrByte[j] != deviceIPBytes[j])
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

        //=====================================================================
        //Can ping IP Address
        //=====================================================================

        /// <summary>
        /// Checks is the IP address of the TM5 is accessible.
        /// </summary>
        /// <returns></returns>
        public static bool PingCheck(string ipAddr)
        {
            try
            {
                Ping pingSender = new Ping();
                IPAddress address = IPAddress.Parse(ipAddr);
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
        /// </summary>
        /// <param name="ipAddr"></param>
        /// <returns></returns>
        public static bool PingCheck(IPAddress ipAddr)
        {
            try
            {
                Ping pingSender = new Ping();
                IPAddress address = ipAddr;
                PingReply reply = pingSender.Send(address);

                if (reply.Status == IPStatus.Success)
                {
                    return true;
                }
                else { return false; }
            }
            catch { return false; }
        }

        //=====================================================================
        //Get IP Address of Host PC
        //=====================================================================

        /// <summary>
        /// Gets the IP addresses of the computer.
        /// </summary>
        public static IPAddress[] GetHostAddress()
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
    }
}