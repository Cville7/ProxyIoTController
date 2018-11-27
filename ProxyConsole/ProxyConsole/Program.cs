using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Net.Http;

namespace ProxyConsole {
    class Program {
        static void Main(string[] args) {
            List<IService> services = new List<IService> {
                new LightService(new List<ILightHub> { new LifXHub(), new HueBridge()})
            };
            
            string info = "Services:";
            foreach(IService service in services) {
                info += "\n\t" + service.Name + ":";
                foreach(IDevice device in service.Devices) {
                    info += "\n\t\t" + device.Name + ":";
                    foreach(Operation operation in device.GetAvailableOperations()) {
                        info += "\n\t\t\t" + operation.Name;
                    }
                }
            }
            Console.WriteLine("{0}\n",info);
            //Console.Read();

            int serviceNum = -1;
            int deviceNum = -1;
            int operationNum = -1;
            int cnt = 0;
            IService selService;
            IDevice selDevice;
            Operation selOperation;
            object[] operationInput;
            string stringInput;
            do {
                cnt = 0;
                Console.Clear();
                Console.WriteLine("Services Available:\n\t[-1] To Quit");
                foreach(IService service in services) {
                    Console.WriteLine("\t[{0}] {1}", cnt++, service.Name);
                }
                Console.Write("Select Service: ");
                serviceNum = Convert.ToInt32(Console.ReadLine());
                if(serviceNum >= 0) {
                    selService = services[serviceNum];
                    do {
                        cnt = 0;
                        Console.Clear();
                        Console.WriteLine("<{0}>", selService.Name);
                        Console.WriteLine("Devices Available:\n\t[-1] To Quit");
                        foreach(IDevice device in selService.Devices) {
                            Console.WriteLine("\t[{0}] {1}", cnt++, device.Name);
                        }
                        Console.Write("Select Device: ");
                        deviceNum = Convert.ToInt32(Console.ReadLine());
                        if(deviceNum >= 0) {
                            selDevice = selService.Devices[deviceNum];
                            do {
                                cnt = 0;
                                Console.Clear();
                                Console.WriteLine("<{0}>", selDevice.Name);
                                Console.WriteLine("Operations Available:\n\t[-1] To Quit");
                                foreach(Operation operation in selDevice.GetAvailableOperations()) {
                                    Console.WriteLine("\t[{0}] {1}", cnt++, operation.Name);
                                }
                                Console.Write("Select Operation: ");
                                operationNum = Convert.ToInt32(Console.ReadLine());
                                if(operationNum >= 0) {
                                    selOperation = selDevice.GetAvailableOperations()[operationNum];
                                    operationInput = new object[selOperation.ParamTypes.Length];
                                    for(int paramCnt = 0; paramCnt < operationInput.Length; paramCnt++) {
                                        if(selOperation.ParamTypes[paramCnt] is Color) {
                                            Console.Write("Enter Color Name: ");
                                            stringInput = Console.ReadLine();
                                            operationInput[paramCnt] = Color.FromName(stringInput);
                                        } else if(selOperation.ParamTypes[paramCnt] is ushort) {
                                            Console.Write("Enter Color Temp: ");
                                            stringInput = Console.ReadLine();
                                            operationInput[paramCnt] = Convert.ToUInt16(stringInput);
                                        } else if(selOperation.ParamTypes[paramCnt] is double) {
                                            Console.Write("Enter Brightness: ");
                                            stringInput = Console.ReadLine();
                                            operationInput[paramCnt] = Convert.ToDouble(stringInput);
                                        }
                                    }
                                    selOperation.Run(operationInput);
                                }
                            } while(operationNum >= 0);
                        }   
                    } while(deviceNum >= 0);
                }
            } while(serviceNum >= 0);










            /*
            List<ILight> lights = new List<ILight>();

            LifXHub hub = new LifXHub();
            hub.InitializeHub();
            lights.AddRange(hub.FindLights());

            HueBridge bridge = new HueBridge();
            bridge.InitializeHub();
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
                            lights[lightNum].ToggleLight(new object[] { });
                            break;
                        case ('C'):
                            Console.Write("Enter Color Name: ");
                            stringInput = Console.ReadLine();
                            lights[lightNum].SetColor(new object[] { Color.FromName(stringInput) });
                            break;
                        case ('T'):
                            Console.Write("Enter Color Temp: ");
                            stringInput = Console.ReadLine();
                            lights[lightNum].SetTemperature(new object[] { Convert.ToUInt16(stringInput) });
                            break;
                        case ('B'):
                            Console.Write("Enter Brightness: ");
                            stringInput = Console.ReadLine();
                            lights[lightNum].SetBrightness(new object[]{ Convert.ToByte(stringInput) });
                            break;
                        case ('E'):
                            Console.Write("Enter New Name: ");
                            stringInput = Console.ReadLine();
                            //lights[lightNum].SetDeviceName(stringInput);
                            Console.WriteLine("Light Name Changed!");
                            break;
                    }
                }
            } while(lightNum >= 0);
            
            bridge.Client.Dispose();
            hub.Client.Dispose();
            Console.Read();
            */
        }
    }
}
