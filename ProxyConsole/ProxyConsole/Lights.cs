using System;
using System.Collections.Generic;
using System.Drawing;

public class LightService : IService {
    #region IService Properties
    public string Name { get; set; }
    public List<IDevice> Devices { get; set; }
    #endregion

    #region LightService Properties
    public List<ILightHub> LightHubs { get; set; }
    #endregion
    #region LightService Methods
    public LightService(List<ILightHub> lightHubs) {
        Name = "Lighting";
        Devices = new List<IDevice>();
        LightHubs = lightHubs;

        foreach(ILightHub lightHub in LightHubs) {
            lightHub.InitializeHub();
            Devices.AddRange(lightHub.FindLights());
        }
    }
    #endregion
}

public abstract class ILight : IDevice {
    #region IDevice Properties
    public abstract string ID { get; set; }
    public abstract string Name { get; set; }
    public List<Operation> Operations { get; set; }
    #endregion

    public abstract bool On { get; set; }
    public abstract double Brightness { get; set; }
    public abstract double Hue { get; set; }
    public abstract double Saturation { get; set; }
    public abstract ushort Temperature { get; set; }
    public abstract bool ColorVariable { get; }
    public abstract bool TemperatureVariable { get; }
    public abstract bool BrightnessVariable { get; }

    public abstract void ToggleLight(object[] args);
    public abstract void SetColor(object[] args);
    public abstract void SetTemperature(object[] args);
    public abstract void SetBrightness(object[] args);

    public ILight() {
        Operations = new List<Operation> {
            new Operation { Name = "Toggle Light", Run = ToggleLight, ParamTypes = new object[]{ } },
            new Operation { Name = "Set Color", Run = SetColor, ParamTypes = new object[]{ new Color() } },
            new Operation { Name = "Set Temperature", Run = SetTemperature, ParamTypes = new object[]{ new ushort() } },
            new Operation { Name = "Set Brightness", Run = SetBrightness, ParamTypes = new object[]{ new double() } }
        };
    }

    public List<Operation> GetAvailableOperations() {
        List<Operation> AvailableOperations = new List<Operation> { Operations[0] };
        if(ColorVariable) { AvailableOperations.Add(Operations[1]); }
        if(TemperatureVariable) { AvailableOperations.Add(Operations[2]); }
        if(BrightnessVariable) { AvailableOperations.Add(Operations[3]); }
        return AvailableOperations;
    }
}

public interface ILightHub : IDevice {
    List<ILight> Lights { get; set; }

    List<ILight> FindLights();
    void InitializeHub();
}