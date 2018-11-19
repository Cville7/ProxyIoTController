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
    }

    public class HueLight {
        public HueState state { get; set; }

        public HueLight() {
            state = new HueState();
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

            bridge.lights.Add(new HueLight());
            bridge.lights[0].state.on = false;
            bridge.lights[0].state.ct = 5;
            bridge.lights[0].state.sat = 100;
            bridge.lights[0].state.hue = 15151;
            bridge.lights[0].state.bri = 5;

            StringContent content = new StringContent(JsonConvert.SerializeObject(bridge.lights[0].state));

            HttpClient client = new HttpClient {
                BaseAddress = new Uri(URL)
            };

            // Add an Accept header for JSON format.
            client.DefaultRequestHeaders.Accept.Add(
            new MediaTypeWithQualityHeaderValue("application/json"));
        
            
            
            // List data response.
            //HttpResponseMessage response = client.GetAsync(urlParameters).Result;  // Blocking call! Program will wait here until a response is received or a timeout occurs.
            HttpResponseMessage response = client.PutAsync("lights/5/state/", content).Result;
            if(response.IsSuccessStatusCode) {
                // Parse the response body.
                string dataObjects = response.Content.ReadAsStringAsync().Result;  //Make sure to add a reference to System.Net.Http.Formatting.dll
                Console.WriteLine("{0}", JsonConvert.DeserializeObject(dataObjects));
            } else {
                Console.WriteLine("{0} ({1})", (int)response.StatusCode, response.ReasonPhrase);
            }

            //Make any other calls using HttpClient here.

            while(true) {
                Console.Read();
                bridge.lights[0].state.on = !bridge.lights[0].state.on;
                content = new StringContent(JsonConvert.SerializeObject(bridge.lights[0].state));
                response = client.PutAsync("lights/5/state/", content).Result;
            }
            


            //Dispose once all HttpClient calls are complete. This is not necessary if the containing object will be disposed of; for example in this case the HttpClient instance will be disposed automatically when the application terminates so the following call is superfluous.
            client.Dispose();
        }
    }
}
