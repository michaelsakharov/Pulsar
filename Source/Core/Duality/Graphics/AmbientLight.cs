﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Duality.Graphics.Resources;

namespace Duality.Graphics
{
    public class AmbientLight
    {
        public Duality.Resources.Texture Irradiance { get; set; }
        public float IrradianceStrength { get; set; } = 1.0f;
        public Duality.Resources.Texture Specular { get; set; }
        public float SpecularStrength { get; set; } = 1.0f;
    }
}