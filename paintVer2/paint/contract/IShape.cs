using System.Windows;
using System.Windows.Media;

namespace Contract;

public interface IShape : IDeepCopy<IShape>
{
    #region VARIABLE

    #region Basic Information

    string Name { get; }

    string Icon { get; }

    #endregion

    #region Brush Information

    SolidColorBrush BrushColor { get; set; }

    int BrushThickness { get; set; }

    DoubleCollection BrushStyle { get; set; }

    #endregion

    #endregion

    #region METHOD

    #region Handle Point

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

    #endregion

    #region Draw Handle

    UIElement Draw(SolidColorBrush brush, double thickness, DoubleCollection line);
    UIElement Draw();

    #endregion

    #region Serialize and Deserilize

    byte[] Serialize();
    IShape Deserialize(byte[] data);

    #endregion

    #endregion
}