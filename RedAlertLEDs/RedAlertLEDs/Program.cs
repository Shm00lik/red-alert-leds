// See https://aka.ms/new-console-template for more information

using RedAlertLEDs;

var ard = new Arduino("COM5");

var buffer = new byte[86 * 3 + 1];

buffer[0] = 0xAA;

var rand = new Random();

foreach (var i in Enumerable.Range(0, 86))
{
    buffer[i * 3 + 1] = (byte)rand.Next(0, 255);
    buffer[i * 3 + 1 + 1] = (byte)rand.Next(0, 255);
    buffer[i * 3 + 2 + 1] = (byte)rand.Next(0, 255);
}

ard.Write(buffer);

Task.Delay(1000).Wait();