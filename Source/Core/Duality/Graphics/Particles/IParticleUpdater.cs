using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Duality.Graphics.Particles
{
    public interface IParticleUpdater
    {
        void Update(float deltaTime, ParticleData particles);
    }
}
