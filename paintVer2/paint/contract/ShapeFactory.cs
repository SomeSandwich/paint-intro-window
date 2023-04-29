using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Contract
{
    public class ShapeFactory
    {
        private static ShapeFactory instance = null;
        private Dictionary<string, IShape> prototypes = new Dictionary<string, IShape>();

        public static ShapeFactory Instance
        {
            get {
                if(instance== null)
                {
                    instance = new ShapeFactory();
                }
                return instance;
            } 
        }
        private ShapeFactory()
        {
            LoadShapePrototypes();
        }
        public IShape CreateShape(string name)
        {
            IShape shape = null;
            if (prototypes.ContainsKey(name))
                shape = prototypes[name].Clone();
            return (IShape)shape;
        }
        public void Reload()
        {
            prototypes.Clear();
            LoadShapePrototypes();
        }
        private IShape? getPaintShapeFromDll(FileInfo fileInfo)
        {
            Assembly assembly = Assembly.Load(AssemblyName.GetAssemblyName(fileInfo.FullName));
            //Assembly assembly = Assembly.LoadFile(fileInfo.FullName);
            Type[] types = assembly.GetTypes();

            IEnumerable<IShape?> results = types.Where(type =>
            {
                bool check = type.IsClass && typeof(IShape).IsAssignableFrom(type)
                                    && !typeof(Point).Equals(type);

                return type.IsClass && typeof(IShape).IsAssignableFrom(type)
                                    && !typeof(Point).Equals(type);
            })
             .Select(type => Activator.CreateInstance(type) as IShape);

            foreach (var result in results)
            {
                return result;
            }

            return null;
        }
        public Dictionary<string, IShape> GetPrototypes()
        {
            return prototypes;
        }

        public void LoadShapePrototypes()
        {
            string exePath = Assembly.GetExecutingAssembly().Location;
            //string _folder = Path.GetDirectoryName(exePath) + "/shapes";
            string _folder = Path.GetDirectoryName(exePath);

            DirectoryInfo shapesDir = new DirectoryInfo(_folder);
            if (!shapesDir.Exists)
            {
                shapesDir.Create();
                return;
            }

            FileInfo[] fis = new DirectoryInfo(_folder).GetFiles("*.dll");
            foreach (FileInfo fileInfo in fis)
            {
                IShape? shape = getPaintShapeFromDll(fileInfo);
                if (shape != null)
                {
                    Console.WriteLine(shape.Name);
                }

                if (shape != null && !prototypes.ContainsKey(shape.Name))
                {
                    prototypes.Add(shape.Name, shape);
                }
            }
        }


    }
}
