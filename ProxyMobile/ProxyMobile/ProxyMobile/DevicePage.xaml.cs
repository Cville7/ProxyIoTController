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
	public partial class DevicePage : ContentPage
	{
        StackLayout stack;
        ScrollView scroll;
        IDevice device;

        Dictionary<string, System.Drawing.Color> nameToColor = new Dictionary<string, System.Drawing.Color>
        {
            { "Aqua", System.Drawing.Color.Aqua }, { "Black", System.Drawing.Color.Black },
            { "Blue", System.Drawing.Color.Blue }, { "Fucshia", System.Drawing.Color.Fuchsia },
            { "Gray", System.Drawing.Color.Gray }, { "Green", System.Drawing.Color.Green },
            { "Lime", System.Drawing.Color.Lime }, { "Maroon", System.Drawing.Color.Maroon },
            { "Navy", System.Drawing.Color.Navy }, { "Olive", System.Drawing.Color.Olive },
            { "Purple", System.Drawing.Color.Purple }, { "Red", System.Drawing.Color.Red },
            { "Silver", System.Drawing.Color.Silver }, { "Teal", System.Drawing.Color.Teal },
            { "White", System.Drawing.Color.White }, { "Yellow", System.Drawing.Color.Yellow }
        };

        public DevicePage(IDevice activeDevice)
		{
            device = activeDevice;

            stack = new StackLayout {
                Orientation = StackOrientation.Vertical,
                Padding = new Thickness(20, 0, 20, 0)
            };
            InitializeComponent();
            
            stack.Children.Add(new Label() {
                Text = device.Name.ToString() + " Operations",
                FontSize = 26
            });

            foreach(Operation operation in device.GetAvailableOperations()) {
                if(operation.ParamTypes.Length > 0) {
                    if(operation.ParamTypes[0] is System.Drawing.Color) {
                        System.Drawing.Color param;
                        Picker picker = new Picker {
                            Title = operation.Name,
                            VerticalOptions = LayoutOptions.CenterAndExpand
                        };

                        foreach(string colorName in nameToColor.Keys) {
                            picker.Items.Add(colorName);
                        }

                        picker.SelectedIndexChanged += (sender, args) =>
                        {
                            if(picker.SelectedIndex == -1) {
                                param = System.Drawing.Color.White;
                            } else {
                                string colorName = picker.Items[picker.SelectedIndex];
                                param = nameToColor[colorName];
                            }

                            operation.Run(param);
                        };

                        stack.Children.Add(picker);

                    } else if(operation.ParamTypes[0] is double) {
                        var operationLabel = new Label() {
                            Text = operation.Name.ToString(),
                        };

                        stack.Children.Add(operationLabel);

                        Slider slider = new Slider {
                            Minimum = 0,
                            Maximum = 1,
                            Value = (operation.Name.Equals("Set Temperature")) ? ((ILight)device).Temperature : ((ILight)device).Brightness,
                            VerticalOptions = LayoutOptions.CenterAndExpand
                        };
                        slider.ValueChanged += (sender, args) => {
                            operation.Run(slider.Value);
                        };
                        stack.Children.Add(slider);
                    }
                } else {
                    var operationBtn = new Button() {
                        Text = operation.Name.ToString()
                    };
                    operationBtn.Pressed += (sender, e) => { operation.Run(); };
                    stack.Children.Add(operationBtn);
                }
            }


            stack.Children.Add(new Label() {
                Text = "Device Details",
                FontSize = 22
            });
            stack.Children.Add(new Label() {
                Text = device.ToString(),
                FontSize = 16
            });

            scroll = new ScrollView { Content = stack };
            Content = scroll;
        }
	}
}