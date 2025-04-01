using ILNumerics;
using ILNumerics.Drawing;
using ILNumerics.Drawing.Plotting;
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
                                colormap: Colormaps.Jet) { new Colorbar() }
                }
            };
            ilPanel.Scene.Configure();
        }
    }
}
