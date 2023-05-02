using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Contract;

public class ShapeFactory
{
    private static ShapeFactory? _instance;
    private static readonly Dictionary<string, IShape> Prototypes = new();

    private ShapeFactory()
    {
        LoadShapePrototypes();
    }

    public static ShapeFactory GetInstance()
    {
        return _instance ??= new ShapeFactory();
    }

    public Dictionary<string, IShape> GetPrototypes()
    {
        return Prototypes;
    }

    public void Reload()
    {
        Prototypes.Clear();

        LoadShapePrototypes();
    }

    public IShape? CreateShape(string name)
    {
        return Prototypes.TryGetValue(name, out var shape) ? shape.DeepClone() : null;
    }

    private static void LoadShapePrototypes()
    {
        var programFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

        if (programFolder == null) return;

        var folder = new DirectoryInfo(programFolder);
        if (!folder.Exists)
        {
            folder.Create();
            return;
        }

        var dllFiles = folder.GetFiles("*.dll");
        foreach (var dll in dllFiles)
        {
            var shape = GetPaintShapeFromDll(dll);

            if (shape == null) continue;

            try
            {
                Prototypes.TryAdd(shape.Name, shape);
            }
            catch
            {
                // ignored
            }
        }
    }

    private static IShape? GetPaintShapeFromDll(FileSystemInfo fileInfo)
    {
        var assembly = Assembly.Load(AssemblyName.GetAssemblyName(fileInfo.FullName));
        var types = assembly.GetTypes();

        return types
            .Where(t =>
                t.IsClass && typeof(IShape).IsAssignableFrom(t))
            .Select(t => Activator.CreateInstance(t) as IShape)
            .FirstOrDefault();
    }
}