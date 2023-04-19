using Idaax.nanoFramework.CoAPSharp.Samples.Server;
using nanoFramework.Networking;
using System;
using System.Diagnostics;
using System.Net.NetworkInformation;
using System.Threading;

namespace Idaax.nanoFramework.CoAPSharp.Samples
{
    public class Program
    {
        public static void Main()
        {
            try
            {
                WiFiHelper.Connect("SSID", "PWD", 60_000);
                Debug.WriteLine("Connection to WiFi successful : " + GetMyIP());
                ICoAPSample coapSample = null;
                //Basic CoAP NON Server example
                coapSample = new BasicCoAPNONServer();
                coapSample.Run();
                WiFiHelper.Disconnect();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"{ex.ToString()}");
            }
        }

        private static string GetMyIP()
        {
            return NetworkInterface.GetAllNetworkInterfaces()[0].IPv4Address;
        }
    }
}
