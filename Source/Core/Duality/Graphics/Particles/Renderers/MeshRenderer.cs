﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Duality.Graphics.Particles.Renderers
{
    public class MeshRenderer : IParticleRenderer
    {
        public Resources.Mesh Mesh { get; set; }

        public void PrepareRenderOperations(ParticleSystem particleSystem, RenderOperations operations, Matrix4 worldOffset)
        {
            if (particleSystem == null) throw new ArgumentNullException(nameof(particleSystem));
            if (operations == null) throw new ArgumentNullException(nameof(operations));

            if (Mesh == null)
                return;

            // TODO: this is very efficent ...
            var particles = particleSystem.Particles;
            for (var i = 0; i < particles.AliveCount; i++)
            {
                Matrix4.CreateTranslation(ref particles.Position[i], out var translation);
                Matrix4.Multiply(ref worldOffset, ref translation, out var world);

                foreach (var subMesh in Mesh.SubMeshes)
				{
					if (subMesh.Material.IsAvailable == false) continue;
					operations.Add(subMesh.Handle, world, subMesh.Material.Res, null);
                }
            }
        }

        public void Update(ParticleSystem particleSystem, Stage stage, float deltaTime)
        {
            // NOP
        }
    }
}
