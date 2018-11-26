using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;

public class HueLight : ILight {
    #region IDevice Properties
    [JsonProperty("uniqueid")] public override string ID { get; set; }
    [JsonProperty("name")] public override string Name { get; set; }
    #endregion
    #region IDevice Methods
    public void SetDeviceName(string name) {
        Name = name;
        ParentBridge.Client.PutAsync("lights/" + BridgeID, new StringContent("{\"name\":\"" + Name + "\"}"));
    }
    #endregion

    #region ILight Properties
    public override bool On {
        get { return State.On; }
        set { State.On = value; }
    }

    public override double Brightness {
        get { return State.Bri; }
        set { State.Bri = Convert.ToByte(value > byte.MaxValue ? byte.MaxValue : value < byte.MinValue ? byte.MinValue : value); }
    }

    public override double Hue {
        get { return State.Hue; }
        set { State.Hue = Convert.ToUInt16(value > ushort.MaxValue ? ushort.MaxValue : value < ushort.MinValue ? ushort.MinValue : value); }
    }

    public override double Saturation {
        get { return State.Sat; }
        set { State.Sat = Convert.ToByte(value > byte.MaxValue ? byte.MaxValue : value < byte.MinValue ? byte.MinValue : value); }
    }

    public override ushort Temperature {
        get { return State.CT; }
        set { State.CT = Convert.ToUInt16(value > 500 ? 500 : value < 153 ? 153 : value); }
    }

    public override bool ColorVariable {
        get { return !Capabilities.Control.ColorGamutType.Equals(""); }
    }

    public override bool TemperatureVariable {
        get { return !Capabilities.Control.CT.Max.Equals(0); }
    }

    public override bool BrightnessVariable {
        get { return !Capabilities.Control.MinDimLevel.Equals(0); }
    }
    #endregion
    #region ILight Methods
    public override void ToggleLight(object[] args) {
        State.On = !(State.On);
        ParentBridge.Client.PutAsync("lights/" + BridgeID + "/state/", new StringContent(JsonConvert.SerializeObject(State)));
    }

    public void SetState(bool on, double brightness, double hue, double saturation, ushort temperature) {
        On = on;
        Brightness = brightness;
        Hue = hue;
        Saturation = saturation;
        Temperature = temperature;

        Console.WriteLine(JsonConvert.SerializeObject(State.Colormode));
        Console.WriteLine(ParentBridge.Client.PutAsync("lights/" + BridgeID, new StringContent("{\"colormode\":\"hs\"}")).Result.Content.ReadAsStringAsync().Result);
        Console.WriteLine(ParentBridge.Client.PutAsync("lights/" + BridgeID + "/state/", new StringContent("{\"hue\":" + 21845 + ",\"sat\":" + State.Sat + "}")).Result.Content.ReadAsStringAsync().Result);
    }

    public override void SetColor(object[] args) {
        Hue = ((Color)args[0]).GetHue() / 360.0 * ushort.MaxValue;
        Saturation = ((Color)args[0]).GetSaturation() * byte.MaxValue;
        ParentBridge.Client.PutAsync("lights/" + BridgeID + "/state/", new StringContent("{\"hue\":" + Hue + ",\"sat\":" + Saturation + "}"));
    }

    public override void SetTemperature(object[] args) {
        Temperature = (ushort)args[0];
        ParentBridge.Client.PutAsync("lights/" + BridgeID + "/state/", new StringContent("{\"ct\":" + Temperature + "}"));
    }

    public override void SetBrightness(object[] args) {
        Brightness = (double)args[0];
        ParentBridge.Client.PutAsync("lights/" + BridgeID + "/state/", new StringContent("{\"bri\":" + Brightness + "}"));
    }
    #endregion

    #region HueLight Properties
    [JsonProperty("state")] public HueState State { get; set; }
    [JsonProperty("swupdate")] public HueSoftwareUpdate SoftwareUpdate;
    [JsonProperty("type")] public string Type { get; set; }
    [JsonProperty("modelid")] public string ModelID { get; set; }
    [JsonProperty("manufacturername")] public string ManufacturerName { get; set; }
    [JsonProperty("productname")] public string ProductName { get; set; }
    [JsonProperty("capabilities")] public HueCapabilities Capabilities;
    [JsonProperty("config")] public HueConfig Config;
    [JsonProperty("swversion")] public string SWVersion { get; set; }
    [JsonProperty("configid")] public string ConfigID { get; set; }
    [JsonProperty("productid")] public string ProductID { get; set; }
    public string BridgeID { get; set; }
    public HueBridge ParentBridge { get; set; }
    #endregion
    #region HueLight Methods
    public HueLight() {
        State = new HueState();
        ID = BridgeID = Type = Name = ModelID = ManufacturerName = ProductName = SWVersion = ConfigID = ProductID = "";
    }

    public override string ToString() {
        return "BridgeID: " + BridgeID
                + "\nState: " + State
                + "\nSoftwareUpdate: " + SoftwareUpdate
                + "\nType: " + Type
                + "\nName: " + Name
                + "\nModel ID: " + ModelID
                + "\nManufacturer Name: " + ManufacturerName
                + "\nProduct Name: " + ProductName
                + "\nCapabilities: " + Capabilities
                + "\nConfig: " + Config
                + "\nUnique ID: " + ID
                + "\nSoftware Ver: " + SWVersion
                + "\nConfig ID: " + ConfigID
                + "\nProduct ID: " + ProductID;
    }
    #endregion
}

public class HueBridge : ILightHub {
    #region IDevice Properties
    [JsonProperty("id")] public string ID { get; set; }
    public string Name { get; set; }
    public List<Operation> Operations { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    #endregion
    #region IDevice Methods
    public void SetDeviceName(string name) {
        Name = name;
    }
    #endregion

    #region ILightHub Properties
    public List<ILight> Lights { get; set; }
    #endregion
    #region ILightHub Methods
    public List<ILight> FindLights() {
        HttpResponseMessage response = Client.GetAsync("lights/").Result;
        dynamic dynJson = JsonConvert.DeserializeObject(response.Content.ReadAsStringAsync().Result);
        HueLight hueLight;
        int lightCnt = 0;

        foreach(var lightContainer in dynJson) {
            foreach(var light in lightContainer) {
                hueLight = JsonConvert.DeserializeObject<HueLight>(Convert.ToString(light));
                hueLight.BridgeID = Convert.ToString(++lightCnt);
                hueLight.ParentBridge = this;
                Lights.Add(hueLight);
            }
        }
        return Lights;
    }

    public void InitializeHub() {
        ID = JsonConvert.DeserializeObject<List<HueBridge>>(GetDiscoveryString())[0].ID;
        InternalIPAddress = JsonConvert.DeserializeObject<List<HueBridge>>(GetDiscoveryString())[0].InternalIPAddress;
        Name = "Hue Bridge " + ID;
        Client.BaseAddress = new Uri("http://" + InternalIPAddress + "/api/" + token + "/");
        Client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
    }
    #endregion

    #region HueBridge Properties & Variables
    [JsonProperty("internalipaddress")] public string InternalIPAddress { get; set; }
    public HttpClient Client { get; set; }
    readonly string token = "ge4zFs2pzOSF2T7XdvBOVk0ujRPqSc2KlC69VGJn";
    #endregion
    #region HueBridge Methods
    public HueBridge() {
        ID = InternalIPAddress = Name = "";
        Lights = new List<ILight>();
        Client = new HttpClient();
    }

    public string GetDiscoveryString() {
        string html = String.Empty;
        using(StreamReader sr = new StreamReader(WebRequest.Create("https://discovery.meethue.com/").GetResponse().GetResponseStream())) {
            html = sr.ReadToEnd();
        }
        return html;
    }

    public override string ToString() {
        return "IoT Device Type: Hue Bridge\nID: " + ID 
                + "\nInternal IP Address: " + InternalIPAddress 
                + "\n";
    }

    public List<Operation> GetAvailableOperations() {
        throw new NotImplementedException();
    }
    #endregion
}

#region HueLight JSON Helper Classes
public class HueState {
    [JsonProperty("on")] public bool On { get; set; }
    [JsonProperty("bri")] public byte Bri { get; set; }
    [JsonProperty("ct")] public ushort CT { get; set; }
    [JsonProperty("sat")] public byte Sat { get; set; }
    [JsonProperty("hue")] public ushort Hue { get; set; }
    [JsonProperty("alert")] public string Alert { get; set; }
    [JsonProperty("colormode")] public string Colormode { get; set; }
    [JsonProperty("mode")] public string Mode { get; set; }
    [JsonProperty("reachable")] public bool Reachable { get; set; }
    [JsonProperty("effect")] public string Effect { get; set; }
    [JsonProperty("xy")] public double[] XY { get; set; }

    public HueState() {
        On = Reachable = false;
        Bri = Sat = 0;
        CT = Hue = 0;
        Effect = Alert = Colormode = Mode = "";
        XY = new double[2];
    }

    public override string ToString() {
        return "\n\tOn: " + On
                + "\n\tBrightness: " + Bri
                + "\n\tHue: " + Hue
                + "\n\tSaturation: " + Sat
                + "\n\tEffect: " + Effect
                + "\n\tXY: " + XY
                + "\n\tColor Temp: " + CT
                + "\n\tAlert: " + Alert
                + "\n\tColor Mode: " + Colormode
                + "\n\tMode: " + Mode
                + "\n\tReachable: " + Reachable;
    }
}

public class HueSoftwareUpdate {
    [JsonProperty("state")] public string State;
    [JsonProperty("lastinstall")] public string LastInstall;

    public HueSoftwareUpdate() {
        State = LastInstall = "";
    }

    public override string ToString() {
        return "\n\tState: " + State
                + "\n\tLastInstall: " + LastInstall;
    }
}

public class HueCT {
    [JsonProperty("min")] public int Min;
    [JsonProperty("max")] public int Max;

    public HueCT() {
        Min = Max = 0;
    }

    public override string ToString() {
        return "\n\t\t\tMin: " + Min
                + "\n\t\t\tMax: " + Max;
    }
}

public class HueControl {
    [JsonProperty("mindimlevel")] public int MinDimLevel;
    [JsonProperty("maxlumen")] public int MaxLumen;
    [JsonProperty("colorgamuttype")] public string ColorGamutType;
    [JsonProperty("colorgamut")] public double[,] ColorGamut;
    [JsonProperty("ct")] public HueCT CT;

    public HueControl() {
        MinDimLevel = MaxLumen = 0;
        ColorGamutType = "";
        ColorGamut = new double[3, 2];
        CT = new HueCT();
    }

    public override string ToString() {
        return "\n\t\tMin Dim Level: " + MinDimLevel
                + "\n\t\tMaxLume: " + MaxLumen
                + "\n\t\tColor Gamut Type: " + ColorGamutType
                + "\n\t\tColor Gamut: " + ColorGamut
                + "\n\t\tColor Temp: " + CT;
    }
}

public class HueStreaming {
    [JsonProperty("renderer")] public bool Renderer;
    [JsonProperty("proxy")] public bool Proxy;

    public HueStreaming() {
        Renderer = Proxy = false;
    }

    public override string ToString() {
        return "\n\t\tRenderer: " + Renderer
                + "\n\t\tProxy: " + Proxy;
    }
}

public class HueCapabilities {
    [JsonProperty("certified")] public string Certified;
    [JsonProperty("control")] public HueControl Control;
    [JsonProperty("streaming")] public HueStreaming Streaming;

    public HueCapabilities() {
        Certified = "";
        Control = new HueControl();
        Streaming = new HueStreaming();
    }

    public override string ToString() {
        return "\n\tCertified: " + Certified
                + "\n\tControl: " + Control
                + "\n\tStreaming: " + Streaming;
    }
}

public class HueConfig {
    [JsonProperty("archetype")] public string Archetype;
    [JsonProperty("function")] public string Function;
    [JsonProperty("direction")] public string Direction;

    public HueConfig() {
        Archetype = Function = Direction = "";
    }

    public override string ToString() {
        return "\n\tArchetype: " + Archetype
                + "\n\tFunction: " + Function
                + "\n\tDirection: " + Direction;
    }
}
#endregion
