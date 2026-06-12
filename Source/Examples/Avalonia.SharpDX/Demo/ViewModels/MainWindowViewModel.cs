using HalconDotNet;
using HelixToolkit;
using HelixToolkit.SharpDX;
using System.Numerics;

namespace Demo.ViewModels
{
    public partial class MainWindowViewModel : ViewModelBase
    {
        public MainWindowViewModel()
        {
            using var model = new HObjectModel3D("C:\\Users\\Admin\\Desktop\\1.ply", new HTuple("m"),new HTuple(),new HTuple(), out _);
            var xs = model.GetObjectModel3dParams("point_coord_x").ToFArr();
            var ys = model.GetObjectModel3dParams("point_coord_y").ToFArr();
            var zs = model.GetObjectModel3dParams("point_coord_z").ToFArr();
            var vectors = new Vector3Collection(xs.Length);
            for (int i = 0; i < xs.Length; i++)
            {
                vectors.Add(new Vector3(xs[i], ys[i], zs[i]));
            }
            PointCloud = new PointGeometry3D() { Positions = vectors };
        }

        public IEffectsManager EffectsManager { get; } = new DefaultEffectsManager();
        public PointGeometry3D PointCloud { get; set; }
        public string Greeting { get; } = "Welcome to Avalonia!";
    }
}
