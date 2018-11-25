using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;

namespace ProxyConsole {
    class Program {
        static void Main(string[] args) {
            List<ILight> lights = new List<ILight>();

            LifXHub hub = new LifXHub();
            lights.AddRange(hub.FindLights());

            HueBridge bridge = new HueBridge();
            bridge = JsonConvert.DeserializeObject<List<HueBridge>>(bridge.GetDiscoveryString())[0];
            bridge.InitializeClient();
            lights.AddRange(bridge.FindLights());

            Console.WriteLine(hub.Client.PutAsync("lights/" + "id: " + lights[0].ID + "/state", new StringContent("")).Result.Content.ReadAsStringAsync().Result);

            int lightNum = -1;
            char input = '!';
            do {
                Console.Write("Enter Light #: ");
                lightNum = Convert.ToInt32(Console.ReadLine());

                if(lightNum >= 0 && lightNum < lights.Count) {
                    Console.WriteLine("{0}\n", lights[lightNum]);
                    Console.Write("Toggle? [y/n]: ");
                    input = Convert.ToChar(Console.ReadLine());
                    if(input == 'y') {
                        lights[lightNum].SetState(lights[lightNum].On, 254, ushort.MaxValue*(0.0/360.0), 254, 0);
                    }
                }
            } while(lightNum != -1);
            
            bridge.Client.Dispose();
            hub.Client.Dispose();
            Console.Read();
        }
    }
}
