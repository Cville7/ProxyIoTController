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
        public MainPage()
        {
            InitializeComponent();
            iot.Children.Clear();

            List<int> testList = new List<int>() { 1, 2, 3, 4, 5, 6, 7, 8, 9 };

            foreach (int item in testList)
            {

                var btn = new Button()
                {
                    Text = item.ToString(),
                    StyleId = item.ToString()
                };

                iot.Children.Add(btn);
            }
        }
    }
}
