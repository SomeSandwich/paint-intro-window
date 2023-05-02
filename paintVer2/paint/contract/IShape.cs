using System.Windows;
using System.Windows.Media;

namespace Contract;

public interface IShape : IDeepCopy<IShape>
{
    #region Basic Information

    string Name { get; }

    string Icon { get; }

    #endregion

    #region Region

    public System.Windows.Point Start { get; set; }
    public System.Windows.Point End { get; set; }

    #endregion

    #region Brush Information

    SolidColorBrush BrushColor { get; set; }

    int BrushThickness { get; set; }

    DoubleCollection BrushStyle { get; set; }

    #endregion


    void HandleStart(double x, double y);

    public void HandleStart(System.Windows.Point point)
    {
        HandleStart(point.X, point.Y);
    }

    void HandleEnd(double x, double y);

    public void HandleEnd(System.Windows.Point point)
    {
        HandleEnd(point.X, point.Y);
    }


    UIElement Draw(SolidColorBrush brush, double thickness, DoubleCollection line);
    UIElement Draw();

    byte[] Serialize();
    IShape Deserialize(byte[] data);
}