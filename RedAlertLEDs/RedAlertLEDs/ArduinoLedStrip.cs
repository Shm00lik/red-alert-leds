using System.Drawing;

namespace RedAlertLEDs;

public class ArduinoLedStrip(Arduino arduino, int numLeds)
{
    private readonly int _numLeds = numLeds;
    private readonly byte[] _buffer = new byte[numLeds * 3];

    public ArduinoLedStrip(int numLeds, string port = Arduino.DefaultPort, int baudrate = Arduino.DefaultBaudRate)
        : this(new Arduino(port, baudrate), numLeds)
    {
    }

    public void Reset(bool update = false)
    {
        SetColor(Color.Black, update);
    }

    public void SetColor(Color color, int index)
    {
        if (!IsValidIndex(index))
        {
            throw new ArgumentOutOfRangeException(nameof(index));
        }
        
        _buffer[index * 3] = color.R;
        _buffer[index * 3 + 1] = color.G;
        _buffer[index * 3 + 2] = color.B;
    }

    public void SetColor(Color colors, bool update = true)
    {
        for (var i = 0; i < _numLeds; i++)
        {
            SetColor(colors, i);
        }
    }

    public Color GetColor(int index)
    {
        if (!IsValidIndex(index))
        {
            throw new ArgumentOutOfRangeException(nameof(index));
        }
        
        var r = _buffer[index * 3];
        var g = _buffer[index * 3 + 1];
        var b = _buffer[index * 3 + 2];

        return Color.FromArgb(r, g, b);
    }

    private bool IsValidIndex(int index)
    {
        return index >= 0  && index < _numLeds;
    }
    
    public void Update()
    {
        arduino.Write(_buffer);
    }
}