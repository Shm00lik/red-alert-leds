using System.IO.Ports;
using System.Runtime.InteropServices;

namespace RedAlertLEDs.Hardware;

public class Arduino : IDisposable
{
    public const int DefaultBaudRate = 115200;

    public static readonly string DefaultPort = GetDefaultPort();

    private const byte StartByte = 0xAA;
    private const int ReadTimeout = 500;

    private readonly SerialPort _port;

    public Arduino(string port, int baudrate = DefaultBaudRate)
    {
        _port = new SerialPort(port, baudrate, Parity.None, 8, StopBits.One);
        _port.ReadTimeout = ReadTimeout;
        _port.Open();
    }

    public void Write(byte[] buffer)
    {
        _port.Write([StartByte], 0, 1);
        _port.ReadByte();

        _port.Write(buffer, 0, buffer.Length);
        _port.ReadByte();
    }

    private static string GetDefaultPort()
    {
        return RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "COM5" :
            RuntimeInformation.IsOSPlatform(OSPlatform.Linux) ? "/dev/ttyACM0" :
            RuntimeInformation.IsOSPlatform(OSPlatform.OSX) ? "/dev/tty.usbmodem" :
            throw new PlatformNotSupportedException();
    }

    public void Dispose()
    {
        _port.Close();
    }
}