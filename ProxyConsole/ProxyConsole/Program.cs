using Newtonsoft.Json;
using System;
using System.Collections.Generic;

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

            int lightNum = -1;
            char input = '!';
            do {
                Console.Write("Enter Light #: ");
                lightNum = Convert.ToInt32(Console.ReadLine());

                if(lightNum >= 0 && lightNum < lights.Count) {
                    Console.WriteLine("{0}", lights[lightNum]);
                    Console.Write("\nToggle? [y/n]: ");
                    input = Convert.ToChar(Console.ReadLine());
                    if(input == 'y') {
                        lights[lightNum].ToggleLight(); 
                    }
                }
            } while(lightNum != -1);
            
            bridge.Client.Dispose();
            hub.Client.Dispose();
            Console.Read();
        }
    }
}
