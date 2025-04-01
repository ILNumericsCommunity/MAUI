using System;
using ILNumerics;
using ILNumerics.Drawing;
using ILNumerics.Drawing.Plotting;
using Microsoft.Maui.Accessibility;
using Microsoft.Maui.Controls;
using static ILNumerics.ILMath;

namespace MAUIDemoApp
{
    public partial class MainPage : ContentPage
    {
        int count = 0;

        public MainPage()
        {
            InitializeComponent();

            Array<double> B = SpecialData.sinc(50, 60);

            ilPanel.Scene = new Scene
            {
                new PlotCube(twoDMode: false)
                {
                    new Surface(tosingle(B),
                                colormap: Colormaps.Hot) { new Colorbar() }
                }
            };
            ilPanel.Scene.Configure();
        }

        private void OnCounterClicked(object sender, EventArgs e)
        {
            count++;

            if (count == 1)
                CounterBtn.Text = $"Clicked {count} time";
            else
                CounterBtn.Text = $"Clicked {count} times";

            SemanticScreenReader.Announce(CounterBtn.Text);
        }
    }

}
