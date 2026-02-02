using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using LumenRGB.Core;
using System.Diagnostics;
using System.Threading;

namespace LumenRGB.UI.Avalonia.Desktop.Views;

public partial class DevicesView : UserControl
{
    public DevicesView()
    {
        InitializeComponent();
        foreach (var device in LumenDiscoverer.GetLumenDevices())
        {
            Debug.WriteLine($"Found Lumen device: {device.Port} - {device.Product}");
            using var connection = LumenCom.Connect(device.Port);
            Debug.WriteLine($"Connected to Lumen device on port {device.Port}");
            connection.WriteLine("MODE SOLID");
            Thread.Sleep(2000);
            connection.WriteLine("SET PARAM colour 255 0 0");
            Thread.Sleep(2000);
            connection.WriteLine("MODE RAINBOW");
            Thread.Sleep(2000);
            connection.WriteLine("SET PARAM speed 128");
            Thread.Sleep(2000);
            connection.WriteLine("SET PARAM speed 64");
            Thread.Sleep(2000);
            connection.WriteLine("SET PARAM direction -1");
            Thread.Sleep(2000);
            connection.WriteLine("SET PARAM direction 1");
            Thread.Sleep(2000);
            connection.Close();
        }
    }
}
