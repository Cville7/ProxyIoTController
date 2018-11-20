using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;

namespace ProxyConsole {
    public class HueState {
        public bool on { get; set; }
        public int bri { get; set; }
        public int ct { get; set; }
        public int sat { get; set; }
        public int hue { get; set; }
        public string alert { get; set; }
        public string colormode { get; set; }
        public string mode { get; set; }
        public bool reachable { get; set; }

        public HueState() {
            on = reachable = false;
            bri = ct = sat = hue = 0;
            alert = colormode = mode = "";
        }

        public override string ToString() {
            string output = String.Empty;
            output += "State:";
            output += "\n\tOn: " + on;
            output += "\n\tBrightness: " + bri;
            output += "\n\tCT: " + ct;
            output += "\n\tSaturation: " + sat;
            output += "\n\tHue: " + hue;
            output += "\n\tAlert: " + alert;
            output += "\n\tColor Mode: " + colormode;
            output += "\n\tMode: " + mode;
            output += "\n\tReachable: " + reachable;
            return output;
        }
    }

    public class HueLight {
        public HueState state { get; set; }
        public string type { get; set; }
        public string name { get; set; }
        public string modelid { get; set; }
        public string manufacturername { get; set; }
        public string productname { get; set; }
        public string uniqueid { get; set; }
        public string swversion { get; set; }
        public string configid { get; set; }
        public string productid { get; set; }

        public HueLight() {
            state = new HueState();
            type = name = modelid = manufacturername = productname = uniqueid = swversion = configid = productid = "";
        }

        public override string ToString() {
            string output = String.Empty;
            output += state;
            output += "\nType: " + type;
            output += "\nName: " + name;
            output += "\nModel ID: " + modelid;
            output += "\nManufacturer Name: " + manufacturername;
            output += "\nProduct Name: " + productname;
            output += "\nUnique ID: " + uniqueid;
            output += "\nSoftware Ver: " + swversion;
            output += "\nConfig ID: " + configid;
            output += "\nProduct ID: " + productid;
            return output;
        }
    }

    public class HueBridge {
        public string id { get; set; }
        public string internalipaddress { get; set; }

        public List<HueLight> lights { get; set; }

        public HueBridge() {
            id = internalipaddress = "";
            lights = new List<HueLight>();
        }

        public override string ToString() {
            return "ID: " + id + "\nInternal IP Address: " + internalipaddress + "\n";
        }
    }

    class Program {
        static void Main(string[] args) {
            HueBridge bridge = new HueBridge();

            string html = String.Empty;
            using(StreamReader sr = new StreamReader(WebRequest.Create("https://discovery.meethue.com/").GetResponse().GetResponseStream())) {
                html = sr.ReadToEnd();
            }
            bridge = (JsonConvert.DeserializeObject<List<HueBridge>>(html))[0];
            Console.WriteLine("{0}", bridge);
            string URL = "http://" + bridge.internalipaddress + "/api/ge4zFs2pzOSF2T7XdvBOVk0ujRPqSc2KlC69VGJn/";

            //StringContent content = new StringContent(JsonConvert.SerializeObject(bridge.lights[0].state));

            HttpClient client = new HttpClient {
                BaseAddress = new Uri(URL)
            };

            // Add an Accept header for JSON format.
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            HttpResponseMessage response = client.GetAsync("lights/").Result;
            dynamic dynJson = JsonConvert.DeserializeObject(response.Content.ReadAsStringAsync().Result);
            int lightCnt = 0;
            foreach(var light in dynJson) {
                foreach(var thing in light) {
                    bridge.lights.Add(new HueLight());
                    bridge.lights[lightCnt] = JsonConvert.DeserializeObject<HueLight>(Convert.ToString(thing));
                    lightCnt++; 
                }
            }

            int lightNum = -1;
            char input = '!';
            do {
                Console.Write("Enter Light #: ");
                lightNum = Convert.ToInt32(Console.ReadLine());

                if(lightNum >= 0 && lightNum < bridge.lights.Count) {
                    Console.WriteLine("{0}", bridge.lights[lightNum]);
                    Console.Write("\nToggle? [y/n]: ");
                    input = Convert.ToChar(Console.ReadLine());
                    if(input == 'y') {
                        bridge.lights[lightNum].state.on = !(bridge.lights[lightNum].state.on);
                        StringContent content = new StringContent(JsonConvert.SerializeObject(bridge.lights[lightNum].state));
                        client.PutAsync("lights/" + lightNum + "/state/", content);
                    }
                }
            } while(lightNum != -1);

            /*
            bridge.lights.Add(new HueLight());
            bridge.lights[0].state.on = true;
            bridge.lights[0].state.ct = 50;
            bridge.lights[0].state.sat = 0;
            bridge.lights[0].state.hue = 00000;
            bridge.lights[0].state.bri = 254;   
            */

            // List data response.
            //HttpResponseMessage response = client.GetAsync(urlParameters).Result;  // Blocking call! Program will wait here until a response is received or a timeout occurs.
            
            //response = client.PutAsync("lights/5/state/", content).Result;
            /*
            if(response.IsSuccessStatusCode) {
                // Parse the response body.
                string dataObjects = response.Content.ReadAsStringAsync().Result;  //Make sure to add a reference to System.Net.Http.Formatting.dll
                Console.WriteLine("{0}", JsonConvert.DeserializeObject(dataObjects));
            } else {
                Console.WriteLine("{0} ({1})", (int)response.StatusCode, response.ReasonPhrase);
            }
            */

            //Make any other calls using HttpClient here.
            /*
            while(true) {
                Console.Write("What # light to change: ");
                int lightNum = Convert.ToInt32(Console.ReadLine());
                bridge.lights[lightNum].state.on = !bridge.lights[lightNum].state.on;
                content = new StringContent(JsonConvert.SerializeObject(bridge.lights[0].state));
                response = client.PutAsync("lights/" + lightNum + "/state/", content).Result;
            }
            */

            //Dispose once all HttpClient calls are complete. This is not necessary if the containing object will be disposed of; for example in this case the HttpClient instance will be disposed automatically when the application terminates so the following call is superfluous.
            client.Dispose();
        }
    }
}
