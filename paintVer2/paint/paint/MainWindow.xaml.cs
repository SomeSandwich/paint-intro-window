using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using Contract;
using paint.Constant;
using Button = System.Windows.Controls.Button;
using MessageBox = System.Windows.MessageBox;
using MouseEventArgs = System.Windows.Input.MouseEventArgs;


namespace paint
{
    public partial class MainWindow : Fluent.RibbonWindow, INotifyPropertyChanged
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        #region VARIABLE

        // Status
        private bool _isDrawing = false;
        private bool _isNewDrawShape = false;
        private bool _isEditing = false;
        private bool _isChanged = false;

        private bool _isZooming = false;
        private bool _isZoomingIn = false;
        private bool _isSelectingShape;

        // Pen
        private int _currentThickness = 1;
        private DoubleCollection _currentStrokeStyle = new DoubleCollection();
        public SolidColorBrush CurrentColor { get; set; } = new(Colors.Black);

        private Matrix _originalMatrix;

        // Shape Thing 
        private readonly ShapeFactory _shapeFactoryInstance = ShapeFactory.Instance;
        private List<IShape> _loadedShapePrototypes = new();

        private string _currSelectShape = "";

        private readonly Stack<IShape> _drawedShapes = new();
        private readonly Stack<IShape> _redoShapeStack = new();

        // File
        private string? _filePathCurrent = null;
        private string? _fileNameCurrent = null;


        private IShape _preview = null;

        public event PropertyChangedEventHandler? PropertyChanged;

        #endregion

        #region FUNCTION UTILITY

        private void UpdateSelectedShape(int index)
        {
            Trace.WriteLine(index);
            if (index >= _loadedShapePrototypes.Count || index < 0)
            {
                return;
            }

            _currSelectShape = _loadedShapePrototypes[index].Name;
            _preview = _shapeFactoryInstance.CreateShape(_currSelectShape);
        }

        private void ImportButton_Click(object sender, RoutedEventArgs e)
        {
        }

        private void LoadShapedll()
        {
            _loadedShapePrototypes = _shapeFactoryInstance.GetPrototypes().Values.ToList();
            iconListView.ItemsSource = _loadedShapePrototypes;

            if (_loadedShapePrototypes.Count == 0)
            {
                return;
            }

            iconListView.SelectedIndex = 0;
        }

        private void _updateToggleAttribute()
        {
            redoButton.IsEnabled = _redoShapeStack.Count > 0;
            undoButton.IsEnabled = _drawedShapes.Count > 0;
        }

        private void ResetToDefault()
        {
            Title = "Paint - Untitle";
            //_isChanged = false;
            //_isDrawing = false;
            //_isNewDrawShape = false;

            _filePathCurrent = null;
            _fileNameCurrent = null;

            //CurrentColor = new SolidColorBrush(Colors.Black);

            //_updateToggleAttribute();
            UpdateSelectedShape(0);
            iconListView.SelectedIndex = 0;
            CbStyleBrush.SelectedIndex = 0;
            CbSizeBrush.SelectedIndex = 0;

            _drawedShapes.Clear();
            _redoShapeStack.Clear();

            drawingArea.Children.Clear();
            drawingArea.Background = new SolidColorBrush(Colors.White);

            //_updateToggleAttribute();
        }

        #endregion

        // #TODO:

        #region WINDOW HANDLE

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            DataContext = this;

            LoadShapedll();
            ResetToDefault();
        }

        private void MainWindow_Closing(object? sender, CancelEventArgs cancelEventArgs)
        {
        }

        #endregion

        #region QUICK ACCESS

        private void OnUndoButtonClick(object sender, RoutedEventArgs e)
        {
        }

        private void OnRedoButtonClick(object sender, RoutedEventArgs e)
        {
        }

        #endregion

        #region FILE HANDLE

        private void SaveFileBinary()
        {
            if (_drawedShapes.Count == 0) return;

            if (_filePathCurrent == null)
            {
                var dialog = new Microsoft.Win32.SaveFileDialog
                {
                    Filter = "BIN (*.bin)|*.bin|PPF (*.ppf)|*.ppf",
                    FileName = "Untitled"
                };

                if (dialog.ShowDialog() != true) return;

                var path = dialog.FileName;
                _filePathCurrent = path;

                var file = new FileInfo(path);
                _fileNameCurrent = file.Name;

                Title = $"Paint - {_fileNameCurrent}";

                using var binaryWriter = new BinaryWriter(File.Open(path, FileMode.Create));
                foreach (var shape in _drawedShapes)
                {
                    binaryWriter.Write(shape.Serialize());
                }
            }
            else
            {
                using var binaryWriter = new BinaryWriter(File.Open(_filePathCurrent, FileMode.OpenOrCreate));
                foreach (var shape in _drawedShapes)
                {
                    binaryWriter.Write(shape.Serialize());
                }
            }
            //_isChanged = false;
        }

        private void CreateNewButton_Click(object sender, RoutedEventArgs e)
        {
            if ((_drawedShapes.Count == 0 && _redoShapeStack.Count == 0) || _isChanged == false)
            {
                ResetToDefault();
                e.Handled = true;
                return;
            }

            var result = MessageBox.Show("Do you want to save current file?", "Unsaved changes detected",
                MessageBoxButton.YesNoCancel);

            switch (result)
            {
                case MessageBoxResult.Yes:
                    SaveFileButton_Click(sender, e);

                    ResetToDefault();
                    e.Handled = true;
                    break;
                case MessageBoxResult.No:
                    ResetToDefault();
                    e.Handled = true;
                    break;
                case MessageBoxResult.Cancel:
                    e.Handled = false;
                    break;
            }
        }

        private void OpenFileButton_Click(object sender, RoutedEventArgs e)
        {
        }

        private void SaveFileButton_Click(object sender, RoutedEventArgs e)
        {
            SaveFileBinary();
            e.Handled = true;
        }

        private void SaveAsPngButton_Click(object sender, RoutedEventArgs e)
        {
        }

        #endregion

        #region TAB HOME

        // #TODO:

        #region CLIPBOARD HANDLE

        private void OnPaste(object sender, RoutedEventArgs e)
        {
        }

        private void OnCopy(object sender, RoutedEventArgs e)
        {
        }

        private void OnCut(object sender, RoutedEventArgs e)
        {
        }

        #endregion

        // #TODO:

        #region TOOL HANDLE

        private void OnChange_ToggleButton(object sender, RoutedEventArgs e)
        {
        }

        private void OnDelete(object sender, RoutedEventArgs e)
        {
        }

        #endregion

        // #TODO:

        #region SHAPE HANDLE

        private void iconListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var index = iconListView.SelectedIndex;

            UpdateSelectedShape(index);
        }

        #endregion

        // #TODO:

        #region ZOOM HANDLE

        private void OnMouseWheelZoom(object sender, MouseWheelEventArgs e)
        {
        }

        private void OnZoom_ToggleButton(object sender, RoutedEventArgs e)
        {
        }

        #endregion

        // #TODO:

        #region STYLE HANDLE

        private void CbSizeBrush_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _currentThickness = CbSizeBrush.SelectedIndex switch
            {
                0 => (int)BrushSizeEnum.Size1,
                1 => (int)BrushSizeEnum.Size2,
                2 => (int)BrushSizeEnum.Size3,
                3 => (int)BrushSizeEnum.Size5,
                _ => _currentThickness
            };
        }

        private void CbStyleBrush_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _currentStrokeStyle = CbStyleBrush.SelectedIndex switch
            {
                0 => new DoubleCollection(),
                1 => new DoubleCollection() { 4, 1, 1, 1, 1, 1 },
                2 => new DoubleCollection() { 1, 1 },
                3 => new DoubleCollection() { 6, 1 },
                _ => _currentStrokeStyle
            };
        }

        #endregion

        // #TODO: Change background color of current selected btn

        #region COLOR HANDLE

        private void BtnCurrSelColor_OnClick(object sender, RoutedEventArgs e)
        {
            /*System.Windows.Forms.ColorDialog picker*/
            var picker = new ColorDialog();

            if (picker.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                CurrentColor = new SolidColorBrush(Color.FromRgb(picker.Color.R, picker.Color.G, picker.Color.B));
            }
        }

        private void OnSelectedColor_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button sel)
                CurrentColor = (SolidColorBrush)sel.Background;
        }

        #endregion

        #endregion

        #region MOUSE DRAW CANVAS HANDLE

        private void DrawShape(UIElement uIElement, bool isRemoveEnd)
        {
            if (isRemoveEnd && drawingArea.Children.Count > 0)
            {
                // Remove last item
                drawingArea.Children.RemoveAt((drawingArea.Children.Count - 1));
            }

            drawingArea.Children.Add(uIElement);
        }

        private void RedrawCanvas()
        {
            foreach (var shape in _drawedShapes)
            {
                drawingArea.Children.Add(shape.Draw());
            }
        }

        private void OnCanvasMouseDown(object sender, MouseButtonEventArgs e)
        {
            _isDrawing = true;
            _isNewDrawShape = true;

            _preview.HandleStart(e.GetPosition(drawingArea));
        }

        private void OnCanvasMouseMove(object sender, MouseEventArgs e)
        {
            if (!_isDrawing) return;

            if (_redoShapeStack.Count > 0)
            {
                _redoShapeStack.Clear();
            }

            _preview.HandleEnd(e.GetPosition(drawingArea));
            var uiElement = _preview.Draw(CurrentColor, _currentThickness, _currentStrokeStyle);

            if (_isNewDrawShape)
            {
                DrawShape(uiElement, false);
                _isNewDrawShape = false;
            }
            else
            {
                DrawShape(uiElement, true);
            }
        }

        private void OnCanvasMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (_isDrawing)
            {
                _isChanged = true;
                _isDrawing = false;
                _preview.HandleEnd(e.GetPosition(drawingArea));
                _drawedShapes.Push(_preview);

                // Create new shape
                _preview = _shapeFactoryInstance.CreateShape(_currSelectShape);
            }

            _updateToggleAttribute();
        }

        #endregion
    }
}