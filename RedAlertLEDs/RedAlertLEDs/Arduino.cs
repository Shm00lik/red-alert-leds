namespace RedAlertLEDs;
using System.IO.Ports;

public class Arduino : IDisposable
{
    private readonly SerialPort _port;

    public Arduino(string port, int baudrate = 9600)
    {
        _port = new SerialPort(port, baudrate, Parity.None, 8, StopBits.One);
        _port.Open();
    }
    
    public void Write(byte[] buffer)
    {
        _port.Write(buffer, 0, buffer.Length);
    }

    public void Dispose()
    {
        _port.Close();
    }
}