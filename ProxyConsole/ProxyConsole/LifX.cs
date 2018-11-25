using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;

public class LifXColor {
    [JsonProperty("hue")] public double Hue { get; set; }
    [JsonProperty("saturation")] public double Saturation { get; set; }
    [JsonProperty("kelvin")] public ushort Kelvin { get; set; }

    public LifXColor() {
        Hue = Saturation = 0;
        Kelvin = 3885;
    }

    public override string ToString() {
        return "\n\tHue: " + Hue
                + "\n\tSaturation: " + Saturation
                + "\n\tKelivn: " + Kelvin;
    }
}

public class LifXGroup {
    [JsonProperty("id")] public string ID { get; set; }
    [JsonProperty("name")] public string Name { get; set; }

    public LifXGroup() {
        ID = Name = "";
    }

    public override string ToString() {
        return "\n\tID: " + ID
                + "\n\tName: " + Name;
    }
}

public class LifXCapabilities {
    [JsonProperty("has_color")] public bool HasColor { get; set; }
    [JsonProperty("has_variable_color_temp")] public bool HasVariableColorTemp { get; set; }
    [JsonProperty("has_ir")] public bool HasIR { get; set; }
    [JsonProperty("has_chain")] public bool HasChain { get; set; }
    [JsonProperty("has_multizone")] public bool HasMultizone { get; set; }

    public LifXCapabilities() {
        HasColor = HasVariableColorTemp = HasIR = HasChain = HasMultizone = false;
    }

    public override string ToString() {
        return "\n\t\tHas Color: " + HasColor
                + "\n\t\tHas Variable Color Temp: " + HasVariableColorTemp
                + "\n\t\tHas IR: " + HasIR
                + "\n\t\tHas Chain: " + HasChain
                + "\n\t\tHas Multizone: " + HasMultizone;
    }
}

public class LifXProduct {
    [JsonProperty("name")] public string Name { get; set; }
    [JsonProperty("identifier")] public string Identifier { get; set; }
    [JsonProperty("company")] public string Company { get; set; }
    [JsonProperty("capabilities")] public LifXCapabilities Capabilities { get; set; }

    public LifXProduct() {
        Name = Identifier = Company = "";
        Capabilities = new LifXCapabilities();
    }

    public override string ToString() {
        return "\n\tName: " + Name
                + "\n\tIdentifier: " + Identifier
                + "\n\tCompany: " + Company
                + "\n\tCapabilities: " + Capabilities;
    }
}

public class LifXLight : ILight {
    #region Properties
    [JsonProperty("uuid")] public string UUID { get; set; }
    [JsonProperty("connected")] public bool Connected { get; set; }
    [JsonProperty("power")] public string Power { get; set; }
    [JsonProperty("color")] public LifXColor Color { get; set; }
    [JsonProperty("group")] public LifXGroup Group { get; set; }
    [JsonProperty("location")] public LifXGroup Location { get; set; }
    [JsonProperty("product")] public LifXProduct Product { get; set; }
    [JsonProperty("last_seen")] public string LastSeen { get; set; }
    [JsonProperty("seconds_since_seen")] public int SecondsSinceSeen { get; set; }
    public LifXHub ParentHub { get; set; }
    #endregion

    #region IDevice Variables
    [JsonProperty("id")] public string ID { get; set; }
    [JsonProperty("label")] public string Name { get; set; }
    #endregion

    #region ILight Variables
    public bool On {
        get { return Power.Equals("on") ? true : false; }
        set { Power = value == true ? "on" : "false"; }
    }

    [JsonProperty("brightness")] public double Brightness { get; set; }

    public double Hue {
        get { return Color.Hue; }
        set { Color.Hue = value; }
    }

    public double Saturation {
        get { return Color.Saturation; }
        set { Color.Saturation = value; }
    }

    public ushort Temperature {
        get { return Color.Kelvin; }
        set { Color.Kelvin = value; }
    }
    #endregion


    public LifXLight() {
        ID = UUID = Name = Power = LastSeen = "";
        Connected = false;
        Brightness = SecondsSinceSeen = 0;
        Color = new LifXColor();
        Group = new LifXGroup();
        Location = new LifXGroup();
        Product = new LifXProduct();
        ParentHub = new LifXHub();
    }

    public override string ToString() {
        return "LifX Light: " + Name
                + "\nID: " + ID
                + "\nUUID: " + UUID
                + "\nConnected: " + Connected
                + "\nPower: " + Power
                + "\nColor: " + Color
                + "\nBrighness: " + Brightness
                + "\nGroup: " + Group
                + "\nLocation: " + Location
                + "\nProduct: " + Product
                + "\nLast Seen: " + LastSeen
                + "\nSeconds Since Seen: " + SecondsSinceSeen;
    }

    public void ToggleLight() {
        ParentHub.Client.PostAsync("lights/" + "id:" + ID + "/toggle", new StringContent(""));
    }

    public void SetState(bool on, double Brightness, double Hue, double Saturation, ushort Temperature) {
        ParentHub.Client.PutAsync("lights/" + "id:" + ID + "/state", new StringContent(""));
    }

    public void SetColor() {
        throw new NotImplementedException();
    }
}

public class LifXHub : ILightHub {
    public string ID { get; set; }
    public string Name { get; set; }
    public List<ILight> Lights { get; set; }
    public HttpClient Client { get; set; }
    readonly string token = "ce9bed580318cdfd50cccd1f12d110921f8353bb92cd61616137abf8f7865483";

    public LifXHub() {
        ID = Name = "LifX Virtual Hub";
        Lights = new List<ILight>();
        Client = new HttpClient();
        InitializeClient();
    }

    public void InitializeClient() {
        Client.BaseAddress = new Uri("https://api.lifx.com/v1/");
        Client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }

    public List<ILight> FindLights() {
        HttpResponseMessage response = Client.GetAsync("lights/all").Result;
        Lights.AddRange(JsonConvert.DeserializeObject<List<LifXLight>>(response.Content.ReadAsStringAsync().Result));
        foreach(LifXLight light in Lights) {
            light.ParentHub = this;
        }
        return Lights;
    }
}

