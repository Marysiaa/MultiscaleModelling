using MultiscaleModelling.CellularAutomata;
using MultiscaleModelling.Common;
using MultiscaleModelling.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace MultiscaleModelling
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private DispatcherTimer dispatcherTimer;
        private SimulationProperties properties;
        private Scope previousScope, currentScope;
        private Random random;
        private CA CA;

        public MainWindow()
        {
            InitializeComponent();

            // select method if needed
            this.CA = new CA();
            this.random = new Random();

            dispatcherTimer = new System.Windows.Threading.DispatcherTimer();
            dispatcherTimer.Tick += dispatcherTimer_Tick;
            dispatcherTimer.Interval = new TimeSpan(0, 0, 0, 0, 50);
        }

        private void dispatcherTimer_Tick(object sender, EventArgs e)
        {
            currentScope = CA.Grow(previousScope, properties.NeighbourhoodType);
            StructureImage.Source = Converters.BitmapToImageSource(currentScope.StructureBitmap);
            previousScope = currentScope;            
        }

        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            properties = new SimulationProperties()
            {
                ScopeWidth = (int)StructureImage.Width, // 302,
                ScopeHeight = (int)StructureImage.Height, // 302,
                NumberOfGrains = Converters.StringToInt(NumberOfGrainsTextBox.Text),
                NeighbourhoodType = chooseNeighbourhoodType()
            };
            previousScope = StructureHelpers.InitStructure(properties, random);

            dispatcherTimer.Start();
        }

        private NeighbourhoodType chooseNeighbourhoodType()
        {
            if (MooreRadioButton.IsChecked == true)
            {
                return NeighbourhoodType.Moore;
            }
            else
            {
                return NeighbourhoodType.Neumann;
            }
        }
    }
}
