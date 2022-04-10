﻿using System;
using System.Collections.Generic;
using System.Linq;

using Duality.Drawing;
using Duality.Properties;
using Duality.Editor;
using Duality.Cloning;
using Duality.Components;

using THREE.Materials;

namespace Duality.Resources
{
	[EditorHintCategory(CoreResNames.CategoryGraphics)]
	[EditorHintImage(CoreResNames.ImageMaterial)]
	[ExplicitResourceReference()]
	public abstract class Material : Resource
	{
		public abstract THREE.Materials.Material GetThreeMaterial();

		public ContentRef<Texture> Map;
		public ContentRef<Texture> NormalMap;
		public Vector2 NormalScale;
		public ContentRef<Texture> SpecularMap;
		public ContentRef<Texture> AlphaMap;
		public ContentRef<Texture> MetalnessMap;
		public ContentRef<Texture> RoughnessMap;
		public ContentRef<Texture> BumpMap;
		public ContentRef<Texture> EmissiveMap;
		public ContentRef<Texture> DisplacementMap;
		public float DisplacementScale;
		
		public ContentRef<Texture> AoMap;
		public ContentRef<Texture> EnvMap;

		public ColorRgba Color = ColorRgba.White;
		public ColorRgba Specular = ColorRgba.Black;
		public ColorRgba Sheen = ColorRgba.Black;
		public ColorRgba Emissive = ColorRgba.Black;

		public float Opacity = 1f;
		public float Reflectivity = 1f;
		public float AlphaTest = 0f;
		public float Metalness = 0.5f;
		public float Roughness = 0.5f;
		public float Shininess = 0f;
		public float EmissiveIntensity = 1f;
		public float BumpScale = 1f;

		public float AoMapIntensity = 1f;
		public float EnvMapIntensity = 1f;
		public float LightMapIntensity = 1f;

		public float WireframeLineWidth = 1f;

		public bool Transparent = false;
		public bool Skinning = false;
		public bool FlatShading = false;
		public bool Fog = true;
		public bool Dithering = false;
		public bool PremultipliedAlpha = false;
		public bool VertexColors = false;
		public bool VertexTangents = false;
		public bool Wireframe = false;
		public bool ColorWrite = true;
		public bool DepthWrite = true;
		public bool DepthTest = true;

		/// <summary>
		/// The Material type
		/// </summary>
		protected abstract MaterialType Type { get; }

		protected void SetupBaseMaterialSettings(THREE.Materials.Material mat)
		{
			mat.Map = Map.IsAvailable ? Map.Res.THREE : null;
			mat.NormalMap = NormalMap.IsAvailable ? NormalMap.Res.THREE : null;
			mat.SpecularMap = SpecularMap.IsAvailable ? SpecularMap.Res.THREE : null;
			mat.AlphaMap = AlphaMap.IsAvailable ? AlphaMap.Res.THREE : null;
			mat.MetalnessMap = MetalnessMap.IsAvailable ? MetalnessMap.Res.THREE : null;
			mat.RoughnessMap = RoughnessMap.IsAvailable ? RoughnessMap.Res.THREE : null;
			mat.BumpMap = BumpMap.IsAvailable ? BumpMap.Res.THREE : null;
			mat.EmissiveMap = EmissiveMap.IsAvailable ? EmissiveMap.Res.THREE : null;
			mat.DisplacementMap = DisplacementMap.IsAvailable ? DisplacementMap.Res.THREE : null;
			mat.AoMap = AoMap.IsAvailable ? AoMap.Res.THREE : null;
			mat.EnvMap = EnvMap.IsAvailable ? EnvMap.Res.THREE : null;

			mat.Color = new THREE.Math.Color(Color.R / 255f, Color.G / 255f, Color.B / 255f);
			mat.Specular = new THREE.Math.Color(Specular.R / 255f, Specular.G / 255f, Specular.B / 255f);
			mat.Sheen = new THREE.Math.Color(Sheen.R / 255f, Sheen.G / 255f, Sheen.B / 255f);
			mat.Emissive = new THREE.Math.Color(Emissive.R / 255f, Emissive.G / 255f, Emissive.B / 255f);

			mat.Opacity = Opacity;
			mat.Reflectivity = Reflectivity;
			mat.AlphaTest = AlphaTest;
			mat.Metalness = Metalness;
			mat.Roughness = Roughness;
			mat.Shininess = Shininess;
			mat.EmissiveIntensity = EmissiveIntensity;
			mat.BumpScale = BumpScale;

			mat.AoMapIntensity = AoMapIntensity;
			mat.EnvMapIntensity = EnvMapIntensity;
			mat.LightMapIntensity = LightMapIntensity;

			mat.WireframeLineWidth = WireframeLineWidth;

			mat.Transparent = Transparent;
			mat.Skinning = Skinning;
			mat.FlatShading = FlatShading;
			mat.Fog = Fog;
			mat.Dithering = Dithering;
			mat.PremultipliedAlpha = PremultipliedAlpha;
			mat.VertexColors = VertexColors;
			mat.VertexTangents = VertexTangents;
			mat.Wireframe = Wireframe;
			mat.ColorWrite = ColorWrite;
			mat.DepthWrite = DepthWrite;
			mat.DepthTest = DepthTest;
		}

		/// <summary>
		/// Creates a new Material
		/// </summary>
		public Material()
		{
		}
		
	}
}
