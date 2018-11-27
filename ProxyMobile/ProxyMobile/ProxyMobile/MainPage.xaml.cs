using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace ProxyMobile
{
    public partial class MainPage : ContentPage
    {
        enum State { Services, Devices, Operations, Inputs };
        List<IService> services;
        State state;
        IService activeService;
        IDevice activeDevice;
        Operation activeOperation;
        StackLayout iot;

        public MainPage()
        {
            iot = new StackLayout {
                Orientation = StackOrientation.Vertical,
                Padding = 0
            };

            InitializeComponent();
            iot.Children.Clear();
            state = State.Services;

            services = new List<IService> {
                new LightService(new List<ILightHub> { new LifXHub(), new HueBridge()})
            };

            foreach(IService service in services) {
                var serviceLabel = new Label() {
                    Text = service.Name.ToString(),
                    FontSize = 20
                };

                iot.Children.Add(serviceLabel);

                foreach(IDevice device in service.Devices) {
                    var deviceLabel = new Label() {
                        Text = device.Name.ToString(),
                    };

                    iot.Children.Add(deviceLabel);

                    foreach(Operation operation in device.GetAvailableOperations()) {
                        var operationBtn = new Button() {
                            Text = operation.Name.ToString()
                        };
                        operationBtn.Pressed += (sender, e) => DeviceEvent(sender, e, device);
                        iot.Children.Add(operationBtn);
                    }
                }
            }

            var scrollView = new ScrollView { Content = iot };

            Content = scrollView;
        }

        public void DoThing(object sender, IDevice device) {
            ((Button)sender).Text = "boop";
        }

        static void DeviceEvent(object sender, EventArgs e, IDevice device) {
            device.Operations[0].Run();
        }

        void ProxyEvent(object sender, EventArgs e, params object[] objects) {
            switch(state) {
                case State.Services:
                    state = State.Devices;
                    activeService = (IService)objects[0];
                    break;
            }
            RenderApp();
        }

        void RenderApp() {
            switch(state) {
                case State.Services:
                    iot.Children.Clear();
                    foreach(IService service in services) {
                        var btn = new Button() {
                            Text = service.Name.ToString()
                        };

                        btn.Pressed += (sender, e) => ProxyEvent(sender, e, service);

                        iot.Children.Add(btn);
                    }
                    break;

                case State.Devices:
                    iot.Children.Clear();
                    foreach(IDevice device in activeService.Devices) {
                        var btn = new Button() {
                            Text = device.Name.ToString()
                        };

                        btn.Pressed += (sender, e) => DeviceEvent(sender, e, device);

                        iot.Children.Add(btn);
                    }
                    break;

                case State.Operations:
                    break;

                case State.Inputs:
                    break;
            }
        }
    }
}
