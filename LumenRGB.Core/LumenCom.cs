using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LumenRGB.Core
{
    public class LumenCom
    {
        public static SerialPort Connect(string port)
        {
            // Connection logic will be implemented here
            var connection = new SerialPort(port, 115200);
            connection.DtrEnable = false;
            connection.RtsEnable = false;
            connection.ReadTimeout = 1000;
            connection.WriteTimeout = 1000;
            connection.WriteBufferSize = 4096;
            connection.ReadBufferSize = 4096;
            connection.Open();

            return connection;
        }

        public void Disconnect(SerialPort connection)
        {
            // Disconnection logic will be implemented here
            connection.Close();
        }
    }
}
