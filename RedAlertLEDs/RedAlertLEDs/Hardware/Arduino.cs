namespace RedAlertLEDs;

using System.IO.Ports;

public class Arduino : IDisposable
{    
    public const int DefaultBaudRate = 230400;
    public const string DefaultPort = "COM5";
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

    public void Dispose()
    {
        _port.Close();
    }
}