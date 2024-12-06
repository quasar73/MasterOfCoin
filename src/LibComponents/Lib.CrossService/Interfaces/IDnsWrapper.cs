namespace Lib.CrossService.Interfaces;

public interface IDnsWrapper
{

    /// <summary>
    /// Resolve host IP addresses
    /// </summary>
    /// <param name="hostName">Host name. Null for local machine.</param>
    /// <returns></returns>
    string[] GetHostAddresses(string? hostName = null);
}
