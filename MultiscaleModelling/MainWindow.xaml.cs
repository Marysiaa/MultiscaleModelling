using MultiscaleModelling.CellularAutomata;
using MultiscaleModelling.Common;
using MultiscaleModelling.File;
using MultiscaleModelling.Models;
using System;
using System.Windows;
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
            if (currentScope == null || !currentScope.IsFull)
            {
                currentScope = CA.Grow(previousScope, properties.NeighbourhoodType);
                StructureImage.Source = Converters.BitmapToImageSource(currentScope.StructureBitmap);
                previousScope = currentScope;
            }
        }

        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            previousScope = null;
            currentScope = null;

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

        private void SaveTxtButton_Click(object sender, RoutedEventArgs e)
        {
            var result = FileSaver.SaveTxtFile(currentScope);
            ResultLabel.Content = string.Concat("File save result: ", result);
        }

        private void SaveBitmapButton_Click(object sender, RoutedEventArgs e)
        {
            var result = FileSaver.SaveBitmapFile(currentScope);
            ResultLabel.Content = string.Concat("File save result: ", result);
        }

        private void ReadTxtButton_Click(object sender, RoutedEventArgs e)
        {
            string fileName = ReadFileNameTextBox.Text;
            currentScope = FileReader.ReadTxtFile(fileName);

            if (currentScope != null)
            {
                StructureImage.Source = Converters.BitmapToImageSource(currentScope.StructureBitmap);
                ResultLabel.Content = "File read result: structure uploaded";
            }
            else
            {
                ResultLabel.Content = "File read result: file does not exist or is incorrect";
            }
        }

        private void ReadBitmapButton_Click(object sender, RoutedEventArgs e)
        {
            string fileName = ReadFileNameTextBox.Text;
            currentScope = FileReader.ReadBitmapFile(fileName);

            if (currentScope != null)
            {
                StructureImage.Source = Converters.BitmapToImageSource(currentScope.StructureBitmap);
                ResultLabel.Content = "File read result: structure uploaded";
            }
            else
            {
                ResultLabel.Content = "File read result: file does not exist or is incorrect";
            }
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
