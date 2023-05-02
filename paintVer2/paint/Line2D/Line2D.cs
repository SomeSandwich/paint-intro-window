using Contract;
using System;
using System.ComponentModel;
using System.IO;
using System.Reflection.Metadata;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;
using Point = Contract.Point;

namespace Line2D;

public class Line2D : IShape
{
    public string Name => "Line";
    public string Icon => "Images/line.png";

    public SolidColorBrush BrushColor { get; set; }
    public int BrushThickness { get; set; }
    public DoubleCollection BrushStyle { get; set; }

    private Contract.Point _start = new Contract.Point();
    private Contract.Point _end = new Point();

    public IShape Clone()
    {
        return new Line2D();
    }

    public UIElement Draw()
    {
        Line line = new Line()
        {
            X1 = _start.X,
            Y1 = _start.Y,
            X2 = _end.X,
            Y2 = _end.Y,
            StrokeThickness = BrushThickness,
            Stroke = BrushColor,
            StrokeDashArray = BrushStyle
        };
        return line;
    }

    public UIElement Draw(SolidColorBrush brush, double thickness, DoubleCollection stroke)
    {
        this.BrushColor = brush;
        this.BrushStyle = stroke;
        this.BrushThickness = (int)thickness;

        return Draw();
    }

    public void HandleEnd(double x, double y)
    {
        _end.X = x;
        _end.Y = y;
    }

    public void HandleStart(double x, double y)
    {
        _start.X = x;
        _start.Y = y;
    }

    public byte[] Serialize()
    {
        try
        {
            using (MemoryStream data = new MemoryStream())
            {
                using (BinaryWriter writer = new BinaryWriter(data))
                {
                    writer.Write(_start.Serialize());
                    writer.Write(_end.Serialize());
                    writer.Write(BrushColor.ToString());
                    writer.Write(BrushThickness);
                    writer.Write(BrushStyle.ToString());

                    using (MemoryStream content = new MemoryStream())
                    {
                        using (BinaryWriter writer1 = new BinaryWriter(content))
                        {
                            writer1.Write(Name);
                            writer1.Write(data.Length);
                            writer1.Write(data.ToArray());
                            return content.ToArray();
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            Console.WriteLine(ex.StackTrace);
        }

        return null;
    }

    public IShape Deserialize(byte[] data)
    {
        Line2D result = new Line2D();
        using (MemoryStream dataStream = new MemoryStream(data))
        {
            using (BinaryReader reader = new BinaryReader(dataStream))
            {
                reader.ReadString(); // Read name point
                long sizeStart = reader.ReadInt64();
                result._start = result._start.Deserialize(reader.ReadBytes((int)sizeStart)) as Contract.Point;

                reader.ReadString(); // Read name point
                long sizeEnd = reader.ReadInt64();
                result._end = result._start.Deserialize(reader.ReadBytes((int)sizeEnd)) as Contract.Point;

                BrushConverter brushConverter = new BrushConverter();
                result.BrushColor = brushConverter.ConvertFromString(reader.ReadString()) as SolidColorBrush;

                result.BrushThickness = reader.ReadInt32();

                DoubleCollectionConverter converter = new DoubleCollectionConverter();
                result.BrushStyle = converter.ConvertFromString(reader.ReadString()) as DoubleCollection;

                return result;
            }
        }
    }

    public IShape DeepClone()
    {
        return new Line2D();
    }
}