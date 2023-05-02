using System;
using System.IO;

namespace Contract;

public static class PointHelper
{
    public static byte[] Serialize(this System.Windows.Point point)
    {
        try
        {
            using var detailData = new MemoryStream();
            using var writerDetail = new BinaryWriter(detailData);

            writerDetail.Write(point.X);
            writerDetail.Write(point.Y);

            using var shapeData = new MemoryStream();
            using var writerShape = new BinaryWriter(shapeData);

            writerShape.Write("Point");
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

    public static System.Windows.Point Deserialize(this System.Windows.Point point, byte[] detailData)
    {
        using var stream = new MemoryStream(detailData);
        using var reader = new BinaryReader(stream);

        return new System.Windows.Point(reader.ReadDouble(), reader.ReadDouble());
    }
}