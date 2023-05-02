using Contract;
using System;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;
using Point = System.Windows.Point;

namespace Line2D;

public class Line2D : IShape
{
    public string Name => "Line";
    public string Icon => "Images/line.png";

    public Point Start { get; set; }
    public Point End { get; set; }

    public SolidColorBrush BrushColor { get; set; }
    public int BrushThickness { get; set; }
    public DoubleCollection BrushStyle { get; set; }

    public void HandleStart(double x, double y)
    {
        Start = new Point(x, y);
    }

    public void HandleEnd(double x, double y)
    {
        End = new Point(x, y);
    }

    public UIElement Draw(SolidColorBrush brush, double thickness, DoubleCollection stroke)
    {
        BrushColor = brush;
        BrushStyle = stroke;
        BrushThickness = (int)thickness;

        return Draw();
    }

    public UIElement Draw()
    {
        var line = new Line()
        {
            X1 = Start.X,
            Y1 = Start.Y,
            X2 = End.X,
            Y2 = End.Y,
            StrokeThickness = BrushThickness,
            Stroke = BrushColor,
            StrokeDashArray = BrushStyle
        };
        return line;
    }

    public byte[] Serialize()
    {
        try
        {
            using var detailData = new MemoryStream();
            using var writerDetail = new BinaryWriter(detailData);

            writerDetail.Write(Start.Serialize());
            writerDetail.Write(End.Serialize());
            writerDetail.Write(BrushColor.ToString());
            writerDetail.Write(BrushThickness);
            writerDetail.Write(BrushStyle.ToString());

            using var shapeData = new MemoryStream();
            using var writerShape = new BinaryWriter(shapeData);

            writerShape.Write(Name);
            writerShape.Write(detailData.Length);
            writerShape.Write(detailData.ToArray());

            return shapeData.ToArray();
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
        var result = new Line2D();
        using var dataStream = new MemoryStream(data);
        using var reader = new BinaryReader(dataStream);

        // Start Point
        reader.ReadString();
        var szDetailStart = reader.ReadInt64();
        result.Start = result.Start.Deserialize(reader.ReadBytes((int)szDetailStart));

        // End Point
        reader.ReadString();
        var szDetailEnd = reader.ReadInt64();
        result.End = result.End.Deserialize(reader.ReadBytes((int)szDetailEnd));

        // Brush Color
        var brushConverter = new BrushConverter();
        result.BrushColor = brushConverter.ConvertFromString(reader.ReadString()) as SolidColorBrush;

        // Brush Thickness
        result.BrushThickness = reader.ReadInt32();

        // Brush Style
        var converter = new DoubleCollectionConverter();
        result.BrushStyle = converter.ConvertFromString(reader.ReadString()) as DoubleCollection;

        return result;
    }

    public IShape DeepClone()
    {
        return new Line2D();
    }
}