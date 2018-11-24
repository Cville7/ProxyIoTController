using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;

public class HueState {
    [JsonProperty("on")] public bool On { get; set; }
    [JsonProperty("bri")] public int Bri { get; set; }
    [JsonProperty("ct")] public int Ct { get; set; }
    [JsonProperty("sat")] public int Sat { get; set; }
    [JsonProperty("hue")] public int Hue { get; set; }
    [JsonProperty("alert")] public string Alert { get; set; }
    [JsonProperty("colormode")] public string Colormode { get; set; }
    [JsonProperty("mode")] public string Mode { get; set; }
    [JsonProperty("reachable")] public bool Reachable { get; set; }

    public HueState() {
        On = Reachable = false;
        Bri = Ct = Sat = Hue = 0;
        Alert = Colormode = Mode = "";
    }

    public override string ToString() {
        return "\n\tOn: " + On
                + "\n\tBrightness: " + Bri
                + "\n\tCT: " + Ct
                + "\n\tSaturation: " + Sat
                + "\n\tHue: " + Hue
                + "\n\tAlert: " + Alert
                + "\n\tColor Mode: " + Colormode
                + "\n\tMode: " + Mode
                + "\n\tReachable: " + Reachable;
    }
}
public class HueLight : ILight {
    [JsonProperty("uniqueid")] public string ID { get; set; }
    [JsonProperty("name")] public string Name { get; set; }

    [JsonProperty("state")] public HueState State { get; set; }
    [JsonProperty("type")] public string Type { get; set; }
    [JsonProperty("modelid")] public string ModelID { get; set; }
    [JsonProperty("manufacturername")] public string ManufacturerName { get; set; }
    [JsonProperty("productname")] public string ProductName { get; set; }
    [JsonProperty("swversion")] public string SWVersion { get; set; }
    [JsonProperty("configid")] public string ConfigID { get; set; }
    [JsonProperty("productid")] public string ProductID { get; set; }
    public string BridgeID { get; set; }

    public HueBridge ParentBridge { get; set; }

    public HueLight() {
        State = new HueState();
        ID = BridgeID = Type = Name = ModelID = ManufacturerName = ProductName = SWVersion = ConfigID = ProductID = "";
    }

    public void ToggleLight() {
        State.On = !(State.On);
        StringContent content = new StringContent(JsonConvert.SerializeObject(State));
        ParentBridge.Client.PutAsync("lights/" + BridgeID + "/state/", content);
    }

    public override string ToString() {
        return "BridgeID: " + BridgeID
                + "\nState: " + State
                + "\nType: " + Type
                + "\nName: " + Name
                + "\nModel ID: " + ModelID
                + "\nManufacturer Name: " + ManufacturerName
                + "\nProduct Name: " + ProductName
                + "\nUnique ID: " + ID
                + "\nSoftware Ver: " + SWVersion
                + "\nConfig ID: " + ConfigID
                + "\nProduct ID: " + ProductID;
    }
}
public class HueBridge : ILightHub {
    [JsonProperty("internalipaddress")] public string InternalIPAddress { get; set; }
    [JsonProperty("id")] public string ID { get; set; }
    public string Name { get; set; }
    public List<ILight> Lights { get; set; }
    public HttpClient Client { get; set; }
    readonly string token = "ge4zFs2pzOSF2T7XdvBOVk0ujRPqSc2KlC69VGJn";

    public HueBridge() {
        ID = InternalIPAddress = Name = "";
        Lights = new List<ILight>();
        Client = new HttpClient();
    }

    public void InitializeClient() {
        Client.BaseAddress = new Uri("http://" + InternalIPAddress + "/api/" + token + "/");
        Client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        Name = "Hue Bridge " + ID;
    }

    public string GetDiscoveryString() {
        string html = String.Empty;
        using(StreamReader sr = new StreamReader(WebRequest.Create("https://discovery.meethue.com/").GetResponse().GetResponseStream())) {
            html = sr.ReadToEnd();
        }
        return html;
    }

    public List<ILight> FindLights() {
        HttpResponseMessage response = Client.GetAsync("lights/").Result;
        dynamic dynJson = JsonConvert.DeserializeObject(response.Content.ReadAsStringAsync().Result);
        int lightCnt = 0;
        foreach(var light in dynJson) {
            foreach(var thing in light) {
                Lights.Add(JsonConvert.DeserializeObject<HueLight>(Convert.ToString(thing)));
                ((HueLight)Lights[lightCnt]).BridgeID = Convert.ToString(lightCnt + 1);
                ((HueLight)Lights[lightCnt]).ParentBridge = this;
                lightCnt++;
            }
        }

        return Lights;
    }

    public override string ToString() {
        return "IoT Device Type: Hue Bridge\nID: " + ID + "\nInternal IP Address: " + InternalIPAddress + "\n";
    }
}