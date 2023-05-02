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
        public bool IsDrawing { get; set; }

        public bool IsNewDrawShape { get; set; }

        // public bool IsEditing { get; set; } = false;
        public bool IsChanged { get; set; } = false;

        // public bool IsZooming { get; set; } = false;
        // public bool IsZoomingIn { get; set; } = false;
        // public bool IsSelectingShape { get; set; }

        // Pen
        public int CurrentThickness { get; set; } = 1;
        public DoubleCollection _currentStrokeStyle { get; set; } = new();
        public SolidColorBrush CurrentColor { get; set; } = new(Colors.Black);

        // private Matrix _originalMatrix;

        // Shape Thing 
        public readonly ShapeFactory _shapeFactoryInstance = ShapeFactory.Instance;
        public List<IShape> LoadedShapePrototypes { get; set; } = new();

        public string CurrSelectShape { get; set; } = "";

        public Stack<IShape> _drawedShapes { get; set; } = new();
        public Stack<IShape> _redoShapeStack { get; set; } = new();

        // File
        private string? _filePathCurrent = null;
        private string? _fileNameCurrent = null;


        private IShape _preview = null;

        // size 
        public double ZoomFactor { get; set; } = 1.0;


        public event PropertyChangedEventHandler? PropertyChanged;

        #endregion

        #region FUNCTION UTILITY

        private void UpdateSelectedShape(int index)
        {
            Trace.WriteLine(index);
            if (index >= LoadedShapePrototypes.Count || index < 0)
            {
                return;
            }

            CurrSelectShape = LoadedShapePrototypes[index].Name;
            _preview = _shapeFactoryInstance.CreateShape(CurrSelectShape);
        }

        private void ImportButton_Click(object sender, RoutedEventArgs e)
        {
        }

        private void LoadShapedll()
        {
            LoadedShapePrototypes = _shapeFactoryInstance.GetPrototypes().Values.ToList();
            iconListView.ItemsSource = LoadedShapePrototypes;

            if (LoadedShapePrototypes.Count == 0)
            {
                return;
            }

            iconListView.SelectedIndex = 0;
        }

        private void _updateToggleAttribute()
        {
            redoButton.IsEnabled = _redoShapeStack.Count > 0;
            undoButton.IsEnabled = _drawedShapes.Count > 0;
            undoButton.ToolTip = undoButton.IsEnabled ? "Undo" : "No Shape to undo";
            redoButton.ToolTip = redoButton.IsEnabled ? "redo" : "No Shape to undo";
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

        #region WINDOW HANDLE

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            DataContext = this;

            LoadShapedll();
            ResetToDefault();
        }

        private void MainWindow_Closing(object? sender, CancelEventArgs cancelEventArgs)
        {
            if (IsChanged == false)
            {
                return;
            }

            if (_fileNameCurrent == null)
            {
                _fileNameCurrent = "Untitle";
            }

            String title = $"There are unsaved changes in \"{_fileNameCurrent}\".";

            var result = System.Windows.MessageBox.Show(title, "Do you want to save current work?",
                MessageBoxButton.YesNoCancel);

            if (MessageBoxResult.Yes == result)
            {
                try
                {
                    SaveFileBinary();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
            else if (MessageBoxResult.No == result)
            {
            }
            else if (MessageBoxResult.Cancel == result)
            {
                cancelEventArgs.Cancel = true;
            }
        }

        #endregion

        #region QUICK ACCESS

        private void OnUndoButtonClick(object sender, RoutedEventArgs e)
        {
            IShape stack;
            bool isAvailble = _drawedShapes.TryPop(out stack);
            if (isAvailble)
            {
                _redoShapeStack.Push(stack);
                drawingArea.Children.RemoveAt(drawingArea.Children.Count - 1);

                _updateToggleAttribute();
            }
        }

        private void OnRedoButtonClick(object sender, RoutedEventArgs e)
        {
            IShape stack;
            bool isAvailble = _redoShapeStack.TryPop(out stack);
            if (isAvailble)
            {
                _drawedShapes.Push(stack);
                drawingArea.Children.Add(stack.Draw());

                _updateToggleAttribute();
            }
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
            if ((_drawedShapes.Count == 0 && _redoShapeStack.Count == 0) || IsChanged == false)
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


        #region TOOL HANDLE

        private void OnChange_ToggleButton(object sender, RoutedEventArgs e)
        {
        }

        private void OnDelete(object sender, RoutedEventArgs e)
        {
        }

        #endregion


        #region SHAPE HANDLE

        private void iconListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var index = iconListView.SelectedIndex;

            UpdateSelectedShape(index);
        }

        #endregion

        // #TODO:

        #region ZOOM HANDLE

        private void ApplyZoom()
        {
            var scale = new ScaleTransform(ZoomFactor, ZoomFactor);

            drawingArea.LayoutTransform = scale;
        }

        private void OnMouseWheelZoom(object sender, MouseWheelEventArgs e)
        {
            if (e.Delta > 0) // scrolling up
            {
                ZoomFactor *= 1.1; // Increase zoom factor by 10%
            }
            else // scrolling down
            {
                ZoomFactor /= 1.1; // Decrease zoom factor by 10%
            }

            ApplyZoom();
        }

        private void OnZoomIn_ToggleButton(object sender, RoutedEventArgs e)
        {
            ZoomFactor *= 1.1; // Increase zoom factor by 10%

            ApplyZoom();
        }

        private void OnZoomOut_ToggleButton(object sender, RoutedEventArgs e)
        {
            ZoomFactor /= 1.1; // Increase zoom factor by 10%

            ApplyZoom();
        }

        #endregion


        #region STYLE HANDLE

        private void CbSizeBrush_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            CurrentThickness = CbSizeBrush.SelectedIndex switch
            {
                0 => (int)BrushSizeEnum.Size1,
                1 => (int)BrushSizeEnum.Size2,
                2 => (int)BrushSizeEnum.Size3,
                3 => (int)BrushSizeEnum.Size5,
                _ => CurrentThickness
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
            IsDrawing = true;
            IsNewDrawShape = true;

            _preview.HandleStart(e.GetPosition(drawingArea));
        }

        private void OnCanvasMouseMove(object sender, MouseEventArgs e)
        {
            if (!IsDrawing) return;

            if (_redoShapeStack.Count > 0)
            {
                _redoShapeStack.Clear();
            }

            _preview.HandleEnd(e.GetPosition(drawingArea));
            var uiElement = _preview.Draw(CurrentColor, CurrentThickness, _currentStrokeStyle);

            if (IsNewDrawShape)
            {
                DrawShape(uiElement, false);
                IsNewDrawShape = false;
            }
            else
            {
                DrawShape(uiElement, true);
            }
        }

        private void OnCanvasMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (IsDrawing)
            {
                IsChanged = true;
                IsDrawing = false;
                _preview.HandleEnd(e.GetPosition(drawingArea));
                _drawedShapes.Push(_preview);

                // Create new shape
                _preview = _shapeFactoryInstance.CreateShape(CurrSelectShape);
            }

            _updateToggleAttribute();
        }

        #endregion
    }
}