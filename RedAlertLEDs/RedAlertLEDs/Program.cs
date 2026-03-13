using System.Drawing;
using RedAlertLEDs;

var ledStrip = new ArduinoLedStrip(86, "COM5");
ledStrip.Reset();

double t = 0;

while (true)
{
    for (int i = 0; i <= 85; i++)
    {
        double angle = i * 0.15 + t;

        int r = (int)((Math.Sin(angle) + 1) * 120);
        int g = (int)((Math.Sin(angle + 2) + 1) * 120);
        int b = (int)((Math.Sin(angle + 4) + 1) * 120);

        ledStrip.SetColor(Color.FromArgb(r, g, b), i);
    }

    t += 0.08;

    ledStrip.Update();
    Thread.Sleep(15);
}