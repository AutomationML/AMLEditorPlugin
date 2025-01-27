using Aml.Editor.MVVMBase;
using HelixToolkit.SharpDX.Core;
using HelixToolkit.SharpDX.Core.Assimp;
using HelixToolkit.Wpf.SharpDX;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;

using Camera = HelixToolkit.Wpf.SharpDX.Camera;
using Matrix = SharpDX.Matrix;
using OrthographicCamera = HelixToolkit.Wpf.SharpDX.OrthographicCamera;

namespace Aml.Editor.Plugin.Collada.ViewModels
{
    internal class PluginViewModel: ViewModelBase
    {

        public PluginViewModel ()
        {
            Camera = new OrthographicCamera
            {
                LookDirection = new Vector3D(0, -10, -10),
                Position = new Point3D(0, 10, 10),
                UpDirection = new Vector3D(0, 1, 0),
                FarPlaneDistance = 5000,
                NearPlaneDistance = 0.1f
            };
        }

        private double _zoomFactor = 1;

        public Camera Camera { get; }

        public EffectsManager EffectsManager { get; } = new DefaultEffectsManager();

        public TextureModel EnvironmentMap { get; }

        private HelixToolkitScene _scene;

        public SceneNodeGroupModel3D GroupModel { get; } = new SceneNodeGroupModel3D();

        public double ZoomFactor
        {
            get => _zoomFactor; 
            set => Set(ref _zoomFactor, value);
        }


        private void ResetCamera()
        {
            (Camera as OrthographicCamera).Reset();
            (Camera as OrthographicCamera).FarPlaneDistance = 5000;
            (Camera as OrthographicCamera).NearPlaneDistance = 0.1f;
        }

        public void ImportGeometry(string filename, Viewport3DX view3D)
        {
            _ = Task.Run(() =>
            {
                var loader = new Importer(); // {Configuration = {IsSourceMatrixColumnMajor = false}};
                return loader.Load(filename);
            }).ContinueWith(result =>
            {
                if (result.IsCompleted)
                {
                    _scene = result.Result;                    
                    GroupModel.Clear();
                    if (_scene?.Root != null)
                    {
                        _ = GroupModel.AddNode(_scene.Root);
                        //_scene.Root.ModelMatrix *= matrix;
                    }
                }               
                RaisePropertyChanged(nameof(GroupModel));
                ResetCamera();
                view3D.Dispatcher?.BeginInvoke(new Action(() => view3D.ZoomExtents()));
            }, TaskScheduler.FromCurrentSynchronizationContext());
        }

    }
}
