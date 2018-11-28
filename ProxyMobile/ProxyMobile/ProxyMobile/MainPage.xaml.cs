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
        List<IService> services;
        StackLayout stack;
        ScrollView scroll;

        public MainPage()
        {
            stack = new StackLayout {
                Orientation = StackOrientation.Vertical,
                Padding = new Thickness(20,0,20,0)
            };
            InitializeComponent();

            services = new List<IService> {
                new LightService(new List<ILightHub> { new LifXHub(), new HueBridge()})
            };

            var header = new Label() {
                Text = "Services Available",
                FontSize = 26
            };
            stack.Children.Add(header);

            foreach(IService service in services) {
                var serviceButton = new Button() {
                    Text = service.Name.ToString(),
                    FontSize = 20
                };

                serviceButton.Pressed += (sender, args) => {
                    Navigation.PushModalAsync(new ServicePage(service));
                };

                stack.Children.Add(serviceButton);
            }

            scroll = new ScrollView { Content = stack };

            Content = scroll;
        }
    }
}
