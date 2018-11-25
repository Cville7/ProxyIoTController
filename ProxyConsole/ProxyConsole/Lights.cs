using System.Collections.Generic;

public interface ILight : IDevice {
    void ToggleLight();
    void SetState(bool on, double Brightness, double Hue, double Saturation, ushort Temperature);
    void SetColor();
    //void SetBrightness();
    
    bool On { get; set; }
    double Brightness { get; set; }
    double Hue { get; set; }
    double Saturation { get; set; }
    ushort Temperature { get; set; }
}

public interface ILightHub : IDevice {
    List<ILight> Lights { get; set; }
    List<ILight> FindLights();
}