using System;
using System.Collections.Generic;
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

namespace paint
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Fluent.RibbonWindow
    {
        //shape 
        ShapeFactory _shapeFactoryIns = ShapeFactory.Instance;
        List<IShape> _loadedShapePrototypes = new List<IShape>();
        private string _selectedShapePrototypeName = "";

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

        private void createNewButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void openFileButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void saveFileButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void saveAsPngButton_Click(object sender, RoutedEventArgs e)
        {

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
            _loadedShapePrototypes = _shapeFactoryIns.GetPrototypes().Values.ToList();
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
