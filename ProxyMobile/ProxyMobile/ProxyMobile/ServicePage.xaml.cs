using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace ProxyMobile
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class ServicePage : ContentPage
	{
        StackLayout stack;
        ScrollView scroll;
        IService service;

		public ServicePage(IService activeService)
		{
            service = activeService;

            stack = new StackLayout {
                Orientation = StackOrientation.Vertical,
                Padding = new Thickness(20, 0, 20, 0)
            };
            InitializeComponent();

            var header = new Label() {
                Text = service.Name.ToString() + " Devices",
                FontSize = 26
            };
            stack.Children.Add(header);

            foreach(IDevice device in service.Devices) {
                var deviceButton = new Button() {
                    WidthRequest = 200,
                    Text = device.Name.ToString(),
                };

                deviceButton.Clicked += (sender, args) => {
                    Navigation.PushModalAsync(new DevicePage(device));
                };

                stack.Children.Add(deviceButton);
            }

            scroll = new ScrollView { Content = stack };
            Content = scroll;
        }
	}
}