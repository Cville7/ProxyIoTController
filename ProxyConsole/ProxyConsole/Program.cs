using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
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
            char charInput = '!';
            string stringInput = "";

            do {
                Console.Write("\nEnter Light #: ");
                lightNum = Convert.ToInt32(Console.ReadLine());
                
                if(lightNum >= 0 && lightNum < lights.Count) {
                    Console.WriteLine("Device Name: {0}\tDevice ID: {1}", lights[lightNum].Name, lights[lightNum].ID);

                    string desc = "(O)n/Off | (E)dit Name";
                    if(lights[lightNum].ColorVariable) { desc += " | (C)olor"; }
                    if(lights[lightNum].TemperatureVariable) { desc += " | (T)emp"; }
                    if(lights[lightNum].BrightnessVariable) { desc += " | (B)rightness"; }
                    Console.Write(desc + ": ");

                    charInput = Convert.ToChar(Console.ReadLine());
                    switch(charInput) {
                        case ('O'):
                            lights[lightNum].ToggleLight();
                            break;
                        case ('C'):
                            Console.Write("Enter Color Name: ");
                            stringInput = Console.ReadLine();
                            lights[lightNum].SetColor(Color.FromName(stringInput));
                            break;
                        case ('T'):
                            Console.Write("Enter Color Temp: ");
                            stringInput = Console.ReadLine();
                            lights[lightNum].SetTemperature(Convert.ToUInt16(stringInput));
                            break;
                        case ('B'):
                            Console.Write("Enter Brightness: ");
                            stringInput = Console.ReadLine();
                            lights[lightNum].SetBrightness(Convert.ToByte(stringInput));
                            break;
                        case ('E'):
                            Console.Write("Enter New Name: ");
                            stringInput = Console.ReadLine();
                            lights[lightNum].SetDeviceName(stringInput);
                            Console.WriteLine("Light Name Changed!");
                            break;
                    }
                }
            } while(lightNum >= 0);
            
            bridge.Client.Dispose();
            hub.Client.Dispose();
            Console.Read();
        }
    }
}
