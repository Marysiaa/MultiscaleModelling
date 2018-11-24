using Microsoft.Win32;
using MultiscaleModelling.CellularAutomata;
using MultiscaleModelling.Common;
using MultiscaleModelling.File;
using MultiscaleModelling.Models;
using MultiscaleModelling.MonteCarlo;
using System;
using System.IO;
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
        private Scope previousScope, currentScope;
        private Random random;

        private SimulationProperties properties;
        private CA CA;

        private MCProperties MCproperties;
        private MC MC;
        private bool MCSelected = false;

        private SRX SRX;
        private SRXProperties SRXProperties;

        public MainWindow()
        {
            InitializeComponent();

            AterRadioButton.IsEnabled = (currentScope != null) ? currentScope.IsFull : false;
            AddInclusionsButton.IsEnabled = (bool)AterRadioButton.IsChecked;

            this.random = new Random();
            this.CA = new CA(random);

            dispatcherTimer = new System.Windows.Threading.DispatcherTimer();
            dispatcherTimer.Tick += dispatcherTimer_Tick;
            dispatcherTimer.Interval = new TimeSpan(0, 0, 0, 0, 2);
        }

        private void dispatcherTimer_Tick(object sender, EventArgs e)
        {
            if (!MCSelected && (currentScope == null || !currentScope.IsFull))
            {
                currentScope = CA.Grow(previousScope, properties.NeighbourhoodType, properties.GrowthProbability);
                StructureImage.Source = Converters.BitmapToImageSource(currentScope.StructureBitmap);
                previousScope = currentScope;
            }
            else if (MCSelected && (currentScope == null || MC.ItertionsPerformed < MCproperties.NumberOfSteps))
            {
                currentScope = MC.Grow(previousScope);
                StructureImage.Source = Converters.BitmapToImageSource(currentScope.StructureBitmap);
                previousScope = currentScope;
            }
            else
            {
                if (!MCSelected)
                {
                    EnableStructureChanges();
                    EnableMCBase();
                }
                if (MCSelected)
                {
                    EnableStructureChanges();
                    SubstructureRadioButton.IsEnabled = false;
                    EnableCABase();
                    EnableSRXCheckBox.IsEnabled = true;
                }
                DispatcherTimer timer = (DispatcherTimer)sender;
                timer.Stop();
            }
        }

        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            MCSelected = false;
            DisableMC();
            DisableCAStructureChanges();

            previousScope = null;
            currentScope = null;

            SetUpProperties();
            previousScope = StructureHelpers.InitCAStructure(properties, random);

            dispatcherTimer.Start();
        }

        private void SaveTxtButton_Click(object sender, RoutedEventArgs e)
        {
            OpenSaveDialogBox(FileType.Txt);
        }

        private void SaveBitmapButton_Click(object sender, RoutedEventArgs e)
        {
            OpenSaveDialogBox(FileType.Bitmap);
        }

        private void ReadTxtButton_Click(object sender, RoutedEventArgs e)
        {
            OpenReadDialog(FileType.Txt);
        }

        private void ReadBitmapButton_Click(object sender, RoutedEventArgs e)
        {
            OpenReadDialog(FileType.Bitmap);
        }

        private NeighbourhoodType ChooseNeighbourhoodType()
        {
            if (MooreRadioButton.IsChecked == true)
            {
                return NeighbourhoodType.Moore;
            }
            else if (NeumannRadioButton.IsChecked == true)
            {
                return NeighbourhoodType.Neumann;
            }
            else
            {
                return NeighbourhoodType.ExtendedMoore;
            }
        }

        private void OpenSaveDialogBox(FileType type)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.RestoreDirectory = true;
            saveFileDialog.FileName = FileHelper.DecideFileName(type);
            saveFileDialog.DefaultExt = FileHelper.DecideExtension(type);

            bool? result = saveFileDialog.ShowDialog();
            if (result == true)
            {
                var output = "";
                var filePath = Path.GetFullPath(saveFileDialog.FileName);
                switch (type)
                {
                    case FileType.Bitmap:
                        output = FileSaver.SaveBitmapFile(currentScope, filePath);
                        break;
                    case FileType.Txt:
                        output = FileSaver.SaveTxtFile(currentScope, filePath);
                        break;
                }
                ResultLabel.Content = string.Concat("File save result: ", output);
            }
        }

        private void OpenReadDialog(FileType type)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.DefaultExt = FileHelper.DecideExtension(type);

            bool? result = openFileDialog.ShowDialog();

            if (result == true)
            {
                string filePath = Path.GetFullPath(openFileDialog.FileName);
                switch (type)
                {
                    case FileType.Bitmap:
                        currentScope = FileReader.ReadBitmapFile(filePath);
                        previousScope = currentScope;
                        break;
                    case FileType.Txt:
                        currentScope = FileReader.ReadTxtFile(filePath);
                        previousScope = currentScope;
                        break;
                }

                if (currentScope != null)
                {
                    StructureImage.Source = Converters.BitmapToImageSource(currentScope.StructureBitmap);
                    ResultLabel.Content = "File read result: structure uploaded";
                    if (EnableInclusionsCheckBox.IsEnabled)
                    {
                        AterRadioButton.IsEnabled = currentScope.IsFull;
                    }
                    EnableStructureChanges();
                    EnableSRXCheckBox.IsEnabled = true;
                }
                else
                {
                    ResultLabel.Content = "File read result: file does not exist or is incorrect";
                }
            }
        }

        private InclusionsCreationTime ChooseCreationTime()
        {
            if (BeginningRadioButton.IsChecked == true)
            {
                return InclusionsCreationTime.Beginning;
            }
            else
            {
                return InclusionsCreationTime.After;
            }
        }

        private StructureType ChooseStructureType()
        {
            if (SubstructureRadioButton.IsEnabled)
            {
                if (SubstructureRadioButton.IsChecked == true)
                {
                    return StructureType.Substructure;
                }
                else
                {
                    return StructureType.Dualphase;
                }
            }
            return StructureType.Disabled;
        }

        private AdvancedMethodType ChooseAdvancedMethodType()
        {
            if (AdvancedCARadioButton.IsEnabled)
            {
                if (AdvancedCARadioButton.IsChecked == true)
                {
                    return AdvancedMethodType.AdvancedCA;
                }
                else
                {
                    return AdvancedMethodType.AdvancedMC;
                }
            }
            return AdvancedMethodType.Disabled;
        }

        private void AddInclusionsButton_Click(object sender, RoutedEventArgs e)
        {
            SetUpProperties();

            if (properties.Inclusions.AreEnable && (properties.Inclusions.CreationTime == InclusionsCreationTime.After))
            {
                var inclusions = new Inclusions(properties.Inclusions, random);

                var filePath = @"..\..\Structures\structureforinlusions.bmp";
                FileSaver.SaveBitmapFile(currentScope, filePath);
                currentScope = FileReader.ReadBitmapFile(filePath);
                previousScope = currentScope;

                currentScope = inclusions.AddInclusionsAfterGrainGrowth(currentScope);

                StructureHelpers.UpdateBitmap(currentScope);
                currentScope.IsFull = true;
                previousScope = currentScope;

                StructureImage.Source = Converters.BitmapToImageSource(currentScope.StructureBitmap);
            }
        }

        private void AterRadioButton_Click(object sender, RoutedEventArgs e)
        {
            AddInclusionsButton.IsEnabled = (bool)AterRadioButton.IsChecked;
        }

        private InclusionsType ChooseInclusionsType()
        {
            if (SquareRadioButton.IsChecked == true)
            {
                return InclusionsType.Square;
            }
            else
            {
                return InclusionsType.Circular;
            }
        }

        private void BeginningRadioButton_Click(object sender, RoutedEventArgs e)
        {
            AddInclusionsButton.IsEnabled = (bool)AterRadioButton.IsChecked;
        }

        private void EnableInclusionsCheckBox_Click(object sender, RoutedEventArgs e)
        {
            if (BeginningRadioButton.IsEnabled)
            {
                AterRadioButton.IsEnabled = (currentScope != null) ? currentScope.IsFull : false;
                AddInclusionsButton.IsEnabled = (bool)AterRadioButton.IsChecked;
            }
            else
            {
                AterRadioButton.IsEnabled = false;
                AddInclusionsButton.IsEnabled = false;
            }
        }

        private void GenerateButton_Click(object sender, RoutedEventArgs e)
        {
            SetUpProperties();
            SetUpMCProperties();
            var structures = new StructuresGrowth(random, currentScope, MCproperties);
            currentScope = structures.ChangeStructure(properties);

            previousScope = currentScope;
            StructureImage.Source = Converters.BitmapToImageSource(currentScope.StructureBitmap);

            EnableStructureChanges();
        }

        private GrainsBoundariesOption ChooseGrainBoundariesOption()
        {
            if (AllGrainsBoundariesRadioButton.IsEnabled)
            {
                if (AllGrainsBoundariesRadioButton.IsChecked == true)
                {
                    return GrainsBoundariesOption.AllGrains;
                }
                else
                {
                    return GrainsBoundariesOption.NGrains;
                }
            }
            return GrainsBoundariesOption.Disabled;
        }

        private void SetUpProperties()
        {
            properties = new SimulationProperties()
            {
                ScopeWidth = (int)StructureImage.Width,
                ScopeHeight = (int)StructureImage.Height,
                NumberOfGrains = Converters.StringToInt(NumberOfGrainsTextBox.Text),
                NeighbourhoodType = ChooseNeighbourhoodType(),
                Inclusions = new InclusionsProperties()
                {
                    AreEnable = (bool)EnableInclusionsCheckBox.IsChecked,
                    Amount = Converters.StringToInt(AmountOfInclusionsTextBox.Text),
                    Size = Converters.StringToInt(SizeOfInclusionsTextBox.Text),
                    CreationTime = ChooseCreationTime(),
                    InclusionsType = ChooseInclusionsType()
                },
                GrowthProbability = Converters.StringToInt(GrowthProbabilityTextBox.Text),
                StructureType = ChooseStructureType(),
                NumberOfRemainingGrains = Converters.StringToInt(NumberOfRemainingGringTextBox.Text),
                GrainBoundariesProperties = new GrainBoundariesProperties()
                {
                    GrainsBoundariesOption = ChooseGrainBoundariesOption(),
                    NumberOfGrainsToSelectBoundaries = Converters.StringToInt(NumberOfGrainsBoundariesTextBox.Text),
                    GrainBoundarySize = Converters.StringToInt(BoundarySizeTextBox.Text)
                },
                AdvancedMethodType = ChooseAdvancedMethodType()
            };
        }

        private void AllGrainsBoundariesRadioButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void NGrainsBoundariesRadioButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void ColorBoundariesButton_Click(object sender, RoutedEventArgs e)
        {
            SetUpProperties();

            var filePath = @"..\..\Structures\structureforinlusions.bmp";
            FileSaver.SaveBitmapFile(currentScope, filePath);
            currentScope = FileReader.ReadBitmapFile(filePath);

            var grainBoundaries = new GrainBoundaries(random, currentScope, properties.GrainBoundariesProperties);
            currentScope = grainBoundaries.SelectGrainBoundaries();

            previousScope = currentScope;
            StructureImage.Source = Converters.BitmapToImageSource(currentScope.StructureBitmap);

            ColorBoundariesButton.IsEnabled = false;
            ClearBackgroundButton.IsEnabled = true;
            DisableFirstPartOfStructureChanges();
        }

        private void ClearBackgroundButton_Click(object sender, RoutedEventArgs e)
        {
            var grainBoundaries = new GrainBoundaries(random, currentScope, properties.GrainBoundariesProperties);

            currentScope = grainBoundaries.ClearBackground();

            previousScope = currentScope;
            StructureImage.Source = Converters.BitmapToImageSource(currentScope.StructureBitmap);

            ClearBackgroundButton.IsEnabled = false;
        }
        
        private void SetUpMCProperties()
        {
            MCproperties = new MCProperties()
            {
                Neighbourhood = NeighbourhoodType.Moore,
                NumberOfInititalStates = Converters.StringToInt(NumberOfInitialStatesTextBox.Text),
                NumberOfSteps = Converters.StringToInt(NumberOfMCStepsTextBox.Text)
            };
        }

        private void MCStartButton_Click(object sender, RoutedEventArgs e)
        {
            MCSelected = true;
            DisableCA();

            previousScope = null;
            currentScope = null;
            
            SetUpMCProperties();
            this.MC = new MC(random, MCproperties);

            previousScope = StructureHelpers.GenerateEmptyStructure((int)StructureImage.Width, (int)StructureImage.Height);

            dispatcherTimer.Start();
        }


        private void DisableCA()
        {
            NumberOfGrainsTextBox.IsEnabled = false;
            NeumannRadioButton.IsEnabled = false;
            MooreRadioButton.IsEnabled = false;
            ExtendedMooreRadioButton.IsEnabled = false;
            //GrowthProbabilityTextBox.IsEnabled = false;
            EnableInclusionsCheckBox.IsEnabled = false;
            //AmountOfInclusionsTextBox.IsEnabled = false;
            //SizeOfInclusionsTextBox.IsEnabled = false;
            //SquareRadioButton.IsEnabled = false;
            //CircularRadioButton.IsEnabled = false;
            //BeginningRadioButton.IsEnabled = false;
            //AterRadioButton.IsEnabled = false;
            //AddInclusionsButton.IsEnabled = false;
            SubstructureRadioButton.IsEnabled = false;

            AdvancedCARadioButton.IsEnabled = false;
            //AdvancedMCRadioButton.IsEnabled = false;

            DualPhaseRadioButton.IsEnabled = false;
            NumberOfRemainingGringTextBox.IsEnabled = false;
            GenerateButton.IsEnabled = false;
            AllGrainsBoundariesRadioButton.IsEnabled = false;
            NGrainsBoundariesRadioButton.IsEnabled = false;
            //NumberOfGrainsBoundariesTextBox.IsEnabled = false;
            ColorBoundariesButton.IsEnabled = false;
            ClearBackgroundButton.IsEnabled = false; 
        }

        private void EnableCABase()
        {
            NumberOfGrainsTextBox.IsEnabled = true;
            NeumannRadioButton.IsEnabled = true;
            MooreRadioButton.IsEnabled = true;
            ExtendedMooreRadioButton.IsEnabled = true;
            //GrowthProbabilityTextBox.IsEnabled = true;
            EnableInclusionsCheckBox.IsEnabled = true;
        }

        private void EnableStructureChanges()
        {
            SubstructureRadioButton.IsEnabled = true;
            AdvancedCARadioButton.IsEnabled = true;
            //AdvancedMCRadioButton.IsEnabled = true;
            DualPhaseRadioButton.IsEnabled = true;
            NumberOfRemainingGringTextBox.IsEnabled = true;
            GenerateButton.IsEnabled = true;

            AllGrainsBoundariesRadioButton.IsEnabled = true;
            NGrainsBoundariesRadioButton.IsEnabled = true;
            ColorBoundariesButton.IsEnabled = true;
            BoundarySizeTextBox.IsEnabled = true;
        }

        private void DisableFirstPartOfStructureChanges()
        {
            SubstructureRadioButton.IsEnabled = false;
            AdvancedCARadioButton.IsEnabled = false;
            //AdvancedMCRadioButton.IsEnabled = false;
            DualPhaseRadioButton.IsEnabled = false;
            NumberOfRemainingGringTextBox.IsEnabled = false;
            GenerateButton.IsEnabled = false;
        }

        private void DisableCAStructureChanges()
        {
            SubstructureRadioButton.IsEnabled = false;
            AdvancedCARadioButton.IsEnabled = false;
            //AdvancedMCRadioButton.IsEnabled = false;
            DualPhaseRadioButton.IsEnabled = false;
            NumberOfRemainingGringTextBox.IsEnabled = false;
            GenerateButton.IsEnabled = false;

            AllGrainsBoundariesRadioButton.IsEnabled = false;
            NGrainsBoundariesRadioButton.IsEnabled = false;
            ColorBoundariesButton.IsEnabled = false;
            BoundarySizeTextBox.IsEnabled = false;
        }

        private void DisableMC()
        {
            NumberOfMCStepsTextBox.IsEnabled = false;
            MCMooreRadioButton.IsEnabled = false;
            NumberOfInitialStatesTextBox.IsEnabled = false;

            EnableSRXCheckBox.IsEnabled = false;
        }

        private void EnableMCBase()
        {
            NumberOfMCStepsTextBox.IsEnabled = true;
            MCMooreRadioButton.IsEnabled = true;
            NumberOfInitialStatesTextBox.IsEnabled = true;
        }

        private void EnableSRXCheckBox_Click(object sender, RoutedEventArgs e)
        {

        }

        private EnergyDistributionType ChooseEnergyDistributionType()
        {
            if (HeterogenousDistributionRadioButton.IsEnabled)
            {
                if (HeterogenousDistributionRadioButton.IsChecked == true)
                {
                    return EnergyDistributionType.Heterogenous;
                }
                else
                {
                    return EnergyDistributionType.Homogenous;
                }
            }
            return EnergyDistributionType.Disabled;
        }

        private NucleationPosition ChooseNucleationPosition()
        {
            if (AnywhereNucleationRadioButton.IsEnabled)
            {
                if (AnywhereNucleationRadioButton.IsChecked == true)
                {
                    return NucleationPosition.Anywhere;
                }
                else
                {
                    return NucleationPosition.BC;
                }
            }
            return NucleationPosition.Disabled;
        }

        private NucleationAmount ChooseNucleationAmount()
        {
            if (BeginingNucleationRadioButton.IsEnabled)
            {
                if (BeginingNucleationRadioButton.IsChecked == true)
                {
                    return NucleationAmount.Beginning;
                }
                else
                {
                    return NucleationAmount.Increasing;
                }
            }
            return NucleationAmount.Disabled;
        }

        private void SetUpSRXPropertirs()
        {
            SRXProperties = new SRXProperties()
            {
                EnergyDistributionType = ChooseEnergyDistributionType(),
                NucleationPosition = ChooseNucleationPosition(),
                NucleationAmount = ChooseNucleationAmount()  
            };
        }

        private void VisualizeEnerdyButton_Click(object sender, RoutedEventArgs e)
        {
            SetUpSRXPropertirs();
            SRX = new SRX(SRXProperties, currentScope);
            currentScope = SRX.VisualizeEnergy();
            StructureImage.Source = Converters.BitmapToImageSource(currentScope.StructureBitmap);
            previousScope = currentScope;
        }

        private void StartSRXButton_Click(object sender, RoutedEventArgs e)
        {
            SetUpSRXPropertirs();
            SRX = new SRX(SRXProperties, currentScope);

        }
    }
}
