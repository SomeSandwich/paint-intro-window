using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Contract;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;

namespace paint
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Fluent.RibbonWindow
    {
        //shape 
        ShapeFactory shapeFactoryInstance = ShapeFactory.Instance;
        List<IShape> _loadedShapePrototypes = new List<IShape>();
        private string _selectedShapePrototypeName = "";
        //  
        Stack<IShape> _drawedShapes = new Stack<IShape>();
        Stack<IShape> _redoShapeStack = new Stack<IShape>();
        //file
        private String? filePathCurrent = null;
        private String? fileNameCurrent = null;
        private bool _isChanged = false;

        private IShape preview = null;
        public MainWindow()
        {
            InitializeComponent();
        }

        private void onUndoButtonClick(object sender, RoutedEventArgs e)
        {

        }

        private void onRedoButtonClick(object sender, RoutedEventArgs e)
        {

        }

        #region File 
        private void _saveFileBinary()
        {
            if (_drawedShapes.Count != 0) {
                if (filePathCurrent == null)
                {
                    var dialog = new Microsoft.Win32.SaveFileDialog();

                    dialog.Filter = "BIN (*.bin)|*.bin|PPF (*.ppf)|*.ppf";
                    dialog.FileName = "Untitle";

                    if (dialog.ShowDialog() == true)
                    {
                        string path = dialog.FileName;
                        filePathCurrent = path;

                        FileInfo file = new FileInfo(path);
                        fileNameCurrent = file.Name;

                        Title = $"Paint - {fileNameCurrent}";

                        using (BinaryWriter binaryWriter = new BinaryWriter(File.Open(path, FileMode.Create)))
                        {
                            foreach (IShape shape in _drawedShapes)
                            {
                                binaryWriter.Write(shape.Serialize());
                            }
                        }
                    }
                }
                else
                {
                    using (BinaryWriter binaryWriter = new BinaryWriter(File.Open(filePathCurrent, FileMode.OpenOrCreate)))
                    {
                        foreach (IShape shape in _drawedShapes)
                        {
                            binaryWriter.Write(shape.Serialize());
                        }
                    }
                }
                //_isChanged = false;
            }
        }
        private void createNewButton_Click(object sender, RoutedEventArgs e)
        {
            if ((_drawedShapes.Count == 0 && _redoShapeStack.Count == 0) || _isChanged == false)
            {
                _resetToDefault();
                e.Handled = true;
                return;
            }

            var result = MessageBox.Show("Do you want to save current file?", "Unsaved changes detected", MessageBoxButton.YesNoCancel);

            if (MessageBoxResult.Yes == result)
            {
                saveFileButton_Click(sender, e);

                _resetToDefault();
                e.Handled = true;
            }
            else if (MessageBoxResult.No == result)
            {
                _resetToDefault();
                e.Handled = true;
            }
            else if (MessageBoxResult.Cancel == result)
            {
                e.Handled = false;
            }
        }

        private void openFileButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void saveFileButton_Click(object sender, RoutedEventArgs e)
        {
            _saveFileBinary();
            e.Handled = true;
        }

        private void saveAsPngButton_Click(object sender, RoutedEventArgs e)
        {

        }
        private void _resetToDefault()
        {
            Title = "Paint - Untitle";
            //_isChanged = false;
            //_isDrawing = false;
            //_isNewDrawShape = false;

            filePathCurrent = null;
            fileNameCurrent = null;

            //CurrentColor = new SolidColorBrush(Colors.Black);

            //_updateToggleAttribute();
            updateSelectedShape(0);
            iconListView.SelectedIndex = 0;
            dashComboBox.SelectedIndex = 0;
            sizeComboBox.SelectedIndex = 0;

            _drawedShapes.Clear();
            _redoShapeStack.Clear();

            drawingArea.Children.Clear();
            drawingArea.Background = new SolidColorBrush(Colors.White);

            //_updateToggleAttribute();
        }
        #endregion


        private void updateSelectedShape(int index)
        {
            Trace.WriteLine(index);
            if (index >= _loadedShapePrototypes.Count || index < 0) { return; }

            _selectedShapePrototypeName = _loadedShapePrototypes[index].Name;
            preview = shapeFactoryInstance.CreateShape(_selectedShapePrototypeName);
        }


        private void onCanvasMouseMove(object sender, MouseEventArgs e)
        {

        }

        private void onPaste(object sender, RoutedEventArgs e)
        {

        }

        private void onCopy(object sender, RoutedEventArgs e)
        {

        }

        private void onCut(object sender, RoutedEventArgs e)
        {

        }

        private void onMouseWheelZoom(object sender, MouseWheelEventArgs e)
        {

        }

        private void onCanvasMouseDown(object sender, MouseButtonEventArgs e)
        {

        }

        private void onCanvasMouseUp(object sender, MouseButtonEventArgs e)
        {

        }

        private void importButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void iconListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var index = iconListView.SelectedIndex;
            
            updateSelectedShape(index); 
        }

        private void dashComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void sizeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void btnBasicBlack_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnBasicGray_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnBasicRed_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnBasicOrange_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnBasicYellow_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnBasicBlue_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnBasicGreen_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnBasicPurple_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnBasicPink_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnBasicBrown_Click(object sender, RoutedEventArgs e)
        {

        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            loadShapedll();
            DataContext = this;
         /*   _resetToDefault();*/
        }
        private void loadShapedll()
        {
            _loadedShapePrototypes = shapeFactoryInstance.GetPrototypes().Values.ToList();
            iconListView.ItemsSource = _loadedShapePrototypes;

            if (_loadedShapePrototypes.Count == 0)
            {
                return;
            }

            iconListView.SelectedIndex = 0;
        }

        private void onZoom_ToggleButton(object sender, RoutedEventArgs e)
        {

        }

        private void onChange_ToggleButton(object sender, RoutedEventArgs e)
        {

        }

        private void onDelete(object sender, RoutedEventArgs e)
        {

        }
    }
}
