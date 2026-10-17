using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace AvaloniaFileTransfer.Services;

public static class NetworkService
{
    public static List<string> GetLocalIpAddresses()
    {
        var ips = new List<string>();

        try
        {
            foreach (var netInterface in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (netInterface.OperationalStatus != OperationalStatus.Up ||
                    netInterface.NetworkInterfaceType == NetworkInterfaceType.Loopback)
                {
                    continue;
                }

                var ipProps = netInterface.GetIPProperties();
                foreach (var addr in ipProps.UnicastAddresses)
                {
                    if (addr.Address.AddressFamily == AddressFamily.InterNetwork &&
                        !IPAddress.IsLoopback(addr.Address))
                    {
                        ips.Add(addr.Address.ToString());
                    }
                }
            }
        }
        catch
        {
            // Fallback using Dns
            try
            {
                var host = Dns.GetHostEntry(Dns.GetHostName());
                foreach (var ip in host.AddressList)
                {
                    if (ip.AddressFamily == AddressFamily.InterNetwork && !IPAddress.IsLoopback(ip))
                    {
                        ips.Add(ip.ToString());
                    }
                }
            }
            catch
            {
                // Silently ignore
            }
        }

        if (ips.Count == 0)
        {
            ips.Add("127.0.0.1");
        }

        return ips.Distinct().ToList();
    }

    public static string GetPrimaryLocalIp()
    {
        var ips = GetLocalIpAddresses();
        // Prefer typical private LAN addresses (192.168.x.x, 10.x.x.x, 172.16-31.x.x)
        var lanIp = ips.FirstOrDefault(ip => ip.StartsWith("192.168.") || ip.StartsWith("10.") || ip.StartsWith("172."));
        return lanIp ?? ips.FirstOrDefault() ?? "127.0.0.1";
    }
}
