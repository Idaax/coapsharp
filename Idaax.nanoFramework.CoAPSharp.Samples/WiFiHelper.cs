
using nanoFramework.Networking;
using System;
using System.Diagnostics;
using System.Text;
using System.Threading;

namespace Idaax.nanoFramework.CoAPSharp.Samples
{
    /// <summary>
    /// A simple helper class to manage WiFi connections on the ESP32
    /// </summary>
    public class WiFiHelper
    {
        /// <summary>
        /// Connect to the WiFi
        /// </summary>
        /// <param name="ssid">The WiFi SSID</param>
        /// <param name="password">The WiFi password</param>
        /// <param name="timeoutMillis">Wait for millis for a valid connection</param>
        public static void Connect(string ssid,string password,int timeoutMillis)
        {
            try
            {
                CancellationTokenSource cs = new(timeoutMillis);
                var success = WifiNetworkHelper.ScanAndConnectDhcp(ssid, password);
                if (!success)
                {
                    if (NetworkHelper.HelperException != null)
                    {
                        throw NetworkHelper.HelperException;
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        /// <summary>
        /// Disconnect from WiFi
        /// </summary>
        public static void Disconnect()
        {
            WifiNetworkHelper.Disconnect();
        }
    }
}
