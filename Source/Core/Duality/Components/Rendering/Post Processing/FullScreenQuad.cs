using System;
using THREE.Cameras;
using THREE.Geometries;
using THREE.Materials;
using THREE.Objects;
using THREE.Renderers;
using THREE.Scenes;

namespace Duality.Postprocessing
{
	// Helper for passes that need to fill the viewport with a single quad.
	public class FullScreenQuad : IDisposable
	{
		private OrthographicCamera camera = new OrthographicCamera(-1, 1, 1, -1, 0, 1);
		private PlaneBufferGeometry geometry = new PlaneBufferGeometry(2, 2);

		private Mesh _mesh = null;
		private Scene scene = new Scene();
		public event EventHandler<EventArgs> Disposed;

		public Material material
		{
			get
			{
				if (_mesh == null) return null;
				else return _mesh.Material;
			}
			set
			{
				if (_mesh == null)
				{
					_mesh = new Mesh(geometry, value);
					scene.Add(_mesh);
				}
				else
				{
					_mesh.Material = value;
				}
			}
		}
		public FullScreenQuad()
		{

		}

		~FullScreenQuad()
		{
			Dispose(false);
		}
		public FullScreenQuad(Material material)
		{
			_mesh = new Mesh(geometry, material);

			scene.Add(_mesh);
		}

		public void Render(GLRenderer renderer)
		{
			renderer.Render(scene, camera);
		}
		public virtual void Dispose()
		{
			Dispose(disposed);
		}
		protected virtual void RaiseDisposed()
		{
			var handler = this.Disposed;
			if (handler != null)
				handler(this, new EventArgs());
		}
		private bool disposed;
		protected virtual void Dispose(bool disposing)
		{
			if (this.disposed) return;
			try
			{
				if (_mesh != null)
					_mesh.Geometry.Dispose();
				this.RaiseDisposed();
				this.disposed = true;
			}
			finally
			{

			}
			this.disposed = true;
		}

	}
}
