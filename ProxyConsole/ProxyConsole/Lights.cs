using System.Collections.Generic;
using System.Drawing;

public interface ILight : IDevice {
    bool On { get; set; }
    double Brightness { get; set; }
    double Hue { get; set; }
    double Saturation { get; set; }
    ushort Temperature { get; set; }
    bool ColorVariable { get; }
    bool TemperatureVariable { get; }
    bool BrightnessVariable { get; }

    void ToggleLight();
    void SetColor(Color color);
    void SetTemperature(ushort temperature);
    void SetBrightness(double brightness);
}

public interface ILightHub : IDevice {
    List<ILight> Lights { get; set; }
    List<ILight> FindLights();
}