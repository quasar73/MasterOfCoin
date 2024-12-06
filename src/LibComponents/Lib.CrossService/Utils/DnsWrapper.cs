using Lib.CrossService.Interfaces;
using System.Net;

namespace Lib.CrossService.Utils;

public class DnsWrapper : IDnsWrapper
{
    public string[] GetHostAddresses(string? hostName = null) =>
        Dns.GetHostAddresses(hostName ?? Dns.GetHostName()).Select(a => a.ToString()).ToArray();
}
