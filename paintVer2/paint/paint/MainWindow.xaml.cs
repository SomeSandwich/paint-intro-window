using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Contract;
using paint.Constant;
using Button = System.Windows.Controls.Button;
using MessageBox = System.Windows.MessageBox;
using MouseEventArgs = System.Windows.Input.MouseEventArgs;


namespace paint;

public partial class MainWindow : Fluent.RibbonWindow, INotifyPropertyChanged
{
    public MainWindow()
    {
        InitializeComponent();
    }

    #region VARIABLE

    // State
    public bool IsDrawing { get; set; }
    public bool IsNewDrawShape { get; set; }
    public bool IsChanged { get; set; }

    // Brush Info
    public SolidColorBrush CurrentColor { get; set; } = new(Colors.Black);
    public int CurrentThickness { get; set; } = 1;
    public DoubleCollection CurrentStrokeStyle { get; set; } = new();

    // Shape Thing 
    public ShapeFactory ShapeFactoryInstance { get; set; } = ShapeFactory.GetInstance();
    public List<IShape> LoadedShapePrototypes { get; set; } = new();
    public string CurrSelectShape { get; set; } = "";
    public Stack<IShape> DrawedShapes { get; set; } = new();
    public Stack<IShape> RedoShapeStack { get; set; } = new();

    // File
    public string? FilePathCurrent { get; set; }
    public string? FileNameCurrent { get; set; }


    public IShape CurrShapeDrawing { get; set; } = null;

    // size 
    public double ZoomFactor { get; set; } = 1.0;


    public event PropertyChangedEventHandler? PropertyChanged;

    #endregion

    #region FUNCTION UTILITY

    private void LoadShapesToListView()
    {
        LoadedShapePrototypes = ShapeFactoryInstance.GetPrototypes().Values.ToList();
        LvShape.ItemsSource = LoadedShapePrototypes;

        if (LoadedShapePrototypes.Count == 0)
        {
            return;
        }

        LvShape.SelectedIndex = 2;
    }

    private void UpdateSelectedShape(int index)
    {
        if (index >= LoadedShapePrototypes.Count || index < 0)
        {
            return;
        }

        CurrSelectShape = LoadedShapePrototypes[index].Name;

        CurrShapeDrawing = ShapeFactoryInstance.CreateShape(CurrSelectShape);
    }

    private void UpdateToggleAttribute()
    {
        redoButton.IsEnabled = RedoShapeStack.Count > 0;
        undoButton.IsEnabled = DrawedShapes.Count > 0;

        undoButton.ToolTip = undoButton.IsEnabled ? "Undo" : "No Shape to undo";
        redoButton.ToolTip = redoButton.IsEnabled ? "redo" : "No Shape to undo";
    }

    private void ResetAllDefault()
    {
        Title = "Paint - Untitled";

        IsChanged = false;
        IsDrawing = false;
        IsNewDrawShape = false;

        CurrentColor = new SolidColorBrush(Colors.Black);
        CurrentThickness = 1;
        CurrentStrokeStyle = new DoubleCollection();

        FilePathCurrent = null;
        FileNameCurrent = null;

        UpdateSelectedShape(0);
        LvShape.SelectedIndex = 0;
        CbStyleBrush.SelectedIndex = 0;
        CbSizeBrush.SelectedIndex = 0;

        DrawedShapes.Clear();
        RedoShapeStack.Clear();

        CvDrawing.Children.Clear();
        CvDrawing.Background = new SolidColorBrush(Colors.White);
    }

    #endregion

    #region WINDOW HANDLE

    private void MainWindow_Loaded(object sender, RoutedEventArgs e)
    {
        DataContext = this;

        LoadShapesToListView();
        ResetAllDefault();
    }

    private void MainWindow_Closing(object? sender, CancelEventArgs cancelEventArgs)
    {
        if (IsChanged == false)
        {
            return;
        }

        FileNameCurrent ??= "Untitled";

        var title = $"There are unsaved changes in \"{FileNameCurrent}\".";

        var result = MessageBox.Show(title, "Do you want to save current work?",
            MessageBoxButton.YesNoCancel);

        switch (result)
        {
            case MessageBoxResult.Yes:
                try
                {
                    SaveFileBinary();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }

                break;
            case MessageBoxResult.No:
                break;
            case MessageBoxResult.Cancel:
                cancelEventArgs.Cancel = true;
                break;
        }
    }

    #endregion

    #region QUICK ACCESS

    private void OnUndoButtonClick(object sender, RoutedEventArgs e)
    {
        var success = DrawedShapes.TryPop(out var shape);

        if (!success) return;

        RedoShapeStack.Push(shape!);

        CvDrawing.Children.RemoveAt(CvDrawing.Children.Count - 1);

        UpdateToggleAttribute();
    }

    private void OnRedoButtonClick(object sender, RoutedEventArgs e)
    {
        var success = RedoShapeStack.TryPop(out var shape);

        if (!success) return;

        DrawedShapes.Push(shape!);

        CvDrawing.Children.Add(shape!.Draw());

        UpdateToggleAttribute();
    }

    #endregion

    #region FILE HANDLE

    private void SaveFileBinary()
    {
        if (DrawedShapes.Count == 0) return;

        if (FilePathCurrent == null)
        {
            var dialog = new Microsoft.Win32.SaveFileDialog
            {
                Filter = "BIN (*.bin)|*.bin|PPF (*.ppf)|*.ppf",
                FileName = "Untitled"
            };

            if (dialog.ShowDialog() != true) return;

            var path = dialog.FileName;
            FilePathCurrent = path;

            var file = new FileInfo(path);
            FileNameCurrent = file.Name;

            Title = $"Paint - {FileNameCurrent}";

            using var binaryWriter = new BinaryWriter(File.Open(path, FileMode.Create));
            foreach (var shape in DrawedShapes)
            {
                binaryWriter.Write(shape.Serialize());
            }
        }
        else
        {
            using var binaryWriter = new BinaryWriter(File.Open(FilePathCurrent, FileMode.OpenOrCreate));
            foreach (var shape in DrawedShapes)
            {
                binaryWriter.Write(shape.Serialize());
            }
        }

        IsChanged = false;
    }

    private void CreateNewButton_Click(object sender, RoutedEventArgs e)
    {
        if ((DrawedShapes.Count == 0) || IsChanged == false)
        {
            ResetAllDefault();
            e.Handled = true;
            return;
        }

        var result = MessageBox.Show("Do you want to save current file?", "Unsaved changes detected",
            MessageBoxButton.YesNoCancel);

        switch (result)
        {
            case MessageBoxResult.Yes:
                SaveFileButton_Click(sender, e);
                ResetAllDefault();
                e.Handled = true;
                break;
            case MessageBoxResult.No:
                ResetAllDefault();
                e.Handled = true;
                break;
            case MessageBoxResult.Cancel:
                e.Handled = false;
                break;
        }
    }

    private void OpenFileButton_Click(object sender, RoutedEventArgs e)
    {
        CreateNewButton_Click(sender, e);
        if (!e.Handled)
            return;

        var openFile = new OpenFileDialog();
        openFile.Filter = "BIN (*.bin)|*.bin|PPF (*.ppf)|*.ppf";

        if (openFile.ShowDialog() != System.Windows.Forms.DialogResult.OK) return;

        var file = File.Open(openFile.FileName, FileMode.Open);
        FilePathCurrent = openFile.FileName;
        FileNameCurrent = openFile.SafeFileName;
        Title = $"Paint - {FileNameCurrent}";

        using (var binaryReader = new BinaryReader(file))
        {
            // Đọc đến khi hết file.
            while (binaryReader.BaseStream.Position != binaryReader.BaseStream.Length)
            {
                var name = binaryReader.ReadString();
                var size = binaryReader.ReadInt64();
                var data = binaryReader.ReadBytes((int)size);

                var shape = ShapeFactoryInstance?.CreateShape(name)?.Deserialize(data);

                if (shape != null) DrawedShapes.Push(shape);
            }

            RedrawCanvas();
        }

        IsChanged = false;
    }

    private void SaveFileButton_Click(object sender, RoutedEventArgs e)
    {
        SaveFileBinary();
        e.Handled = true;
    }

    private void SaveAsBmpButton_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new Microsoft.Win32.SaveFileDialog
        {
            Filter = "BMP (*.bmp)|*.bmp ",
            FileName = "Untitled.bmp"
        };

        if (dialog.ShowDialog() != true) return;

        var path = dialog.FileName;

        var rect = new Rect(CvDrawing.RenderSize);
        var renderTargetBitmap =
            new RenderTargetBitmap((int)rect.Width, (int)rect.Height, 96d, 96d, PixelFormats.Pbgra32);
        renderTargetBitmap.Render(CvDrawing);

        var bitmapEncoder = new BmpBitmapEncoder();
        bitmapEncoder.Frames.Add(BitmapFrame.Create(renderTargetBitmap));

        using var file = File.Create(path);
        bitmapEncoder.Save(file);
        //BitmapEncoder pngEncoder = new PngBitmapEncoder();
        //pngEncoder.Frames.Add(BitmapFrame.Create(renderTargetBitmap));

        //MemoryStream memoryStream = new MemoryStream();

        //pngEncoder.Save(memoryStream);
        //memoryStream.Close();

        //File.WriteAllBytes(path, memoryStream.ToArray());
    }

    private void ImportButton_Click(object sender, RoutedEventArgs e)
    {
        CreateNewButton_Click(sender, e);
        if (!e.Handled)
        {
            return;
        }

        var dialog = new System.Windows.Forms.OpenFileDialog();
        dialog.Filter = "PNG (*.png)|*.png| JPEG (*.jpeg)|*.jpeg| BMP (*.bmp)|*.bmp";

        if (dialog.ShowDialog() != System.Windows.Forms.DialogResult.OK) return;
        var path = dialog.FileName;
        FilePathCurrent = path;

        var file = new FileInfo(path);
        FileNameCurrent = file.Name;


        var brush = new ImageBrush
        {
            ImageSource = new BitmapImage(new Uri(path, UriKind.Absolute))
        };

        CvDrawing.Background = brush;
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

    private void LvShape_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        var index = LvShape.SelectedIndex;

        UpdateSelectedShape(index);
    }

    #endregion


    #region ZOOM HANDLE

    private void ApplyZoom()
    {
        var scale = new ScaleTransform(ZoomFactor, ZoomFactor);

        CvDrawing.LayoutTransform = scale;
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
        CurrentStrokeStyle = CbStyleBrush.SelectedIndex switch
        {
            0 => new DoubleCollection(),
            1 => new DoubleCollection() { 4, 1, 1, 1, 1, 1 },
            2 => new DoubleCollection() { 1, 1 },
            3 => new DoubleCollection() { 6, 1 },
            _ => CurrentStrokeStyle
        };
    }

    #endregion


    #region COLOR HANDLE

    private void BtnCurrSelColor_OnClick(object sender, RoutedEventArgs e)
    {
        var picker = new ColorDialog();

        if (picker.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            CurrentColor = new SolidColorBrush(Color.FromRgb(picker.Color.R, picker.Color.G, picker.Color.B));
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
        if (isRemoveEnd && CvDrawing.Children.Count > 0)
        {
            // Remove last item
            CvDrawing.Children.RemoveAt((CvDrawing.Children.Count - 1));
        }

        CvDrawing.Children.Add(uIElement);
    }

    private void RedrawCanvas()
    {
        foreach (var shape in DrawedShapes)
        {
            CvDrawing.Children.Add(shape.Draw());
        }
    }

    private void OnCvDrawingMouseDown(object sender, MouseButtonEventArgs e)
    {
        IsDrawing = true;
        IsNewDrawShape = true;

        CurrShapeDrawing.HandleStart(e.GetPosition(CvDrawing));
    }

    private void OnCvDrawingMouseMove(object sender, MouseEventArgs e)
    {
        if (!IsDrawing) return;

        if (RedoShapeStack.Count > 0)
        {
            RedoShapeStack.Clear();
        }

        CurrShapeDrawing.HandleEnd(e.GetPosition(CvDrawing));
        var uiElement = CurrShapeDrawing.Draw(CurrentColor, CurrentThickness, CurrentStrokeStyle);

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

    private void OnCvDrawingMouseUp(object sender, MouseButtonEventArgs e)
    {
        if (IsDrawing)
        {
            IsChanged = true;
            IsDrawing = false;
            CurrShapeDrawing.HandleEnd(e.GetPosition(CvDrawing));
            DrawedShapes.Push(CurrShapeDrawing);

            CurrShapeDrawing = ShapeFactoryInstance.CreateShape(CurrSelectShape);
        }

        UpdateToggleAttribute();
    }

    #endregion
}