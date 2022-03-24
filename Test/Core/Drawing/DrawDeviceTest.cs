using System;
using System.Collections.Generic;
using System.Linq;

using Duality;
using Duality.Drawing;
using Duality.Tests.Properties;

using NUnit.Framework;

namespace Duality.Tests.Drawing
{
	public class DrawDeviceTest
	{
		[Test] public void IsSphereInViewScreenSpace()
		{
			//Vector2 viewportSize = new Vector2(800, 600);
			//using (DrawDevice device = new DrawDevice())
			//{
			//	device.TargetSize = viewportSize;
			//	device.ViewportRect = new Rect(viewportSize);
			//	device.Projection = ProjectionMode.Screen;
			//
			//	// Screen space mode is supposed to ignore view dependent settings
			//	device.ViewerAngle = MathF.DegToRad(90.0f);
			//	device.ViewerPos = new Vector3(7000, 8000, -500);
			//	device.FocusDist = 500;
			//	device.NearZ = 100;
			//	device.FarZ = 10000;
			//
			//	// Viewport center
			//	Assert.IsTrue(device.IsSphereInView(new Vector3(viewportSize.X * 0.5f, viewportSize.Y * 0.5f, 0), 150));
			//
			//	// Just inside each of the viewports sides
			//	Assert.IsTrue(device.IsSphereInView(new Vector3(-100, 0, 0), 150));
			//	Assert.IsTrue(device.IsSphereInView(new Vector3(0, -100, 0), 150));
			//	Assert.IsTrue(device.IsSphereInView(new Vector3(viewportSize.X + 100, 0, 0), 150));
			//	Assert.IsTrue(device.IsSphereInView(new Vector3(0, viewportSize.Y + 100, 0), 150));
			//	Assert.IsTrue(device.IsSphereInView(new Vector3(0, 0, 10000), 150));
			//	Assert.IsTrue(device.IsSphereInView(new Vector3(0, 0, 50), 150));
			//
			//	// Just outside each of the viewports sides
			//	Assert.IsFalse(device.IsSphereInView(new Vector3(-200, 0, 0), 150));
			//	Assert.IsFalse(device.IsSphereInView(new Vector3(0, -200, 0), 150));
			//	Assert.IsFalse(device.IsSphereInView(new Vector3(viewportSize.X + 200, 0, 0), 150));
			//	Assert.IsFalse(device.IsSphereInView(new Vector3(0, viewportSize.Y + 200, 0), 150));
			//	Assert.IsFalse(device.IsSphereInView(new Vector3(0, 0, 1000000000), 150));
			//	Assert.IsFalse(device.IsSphereInView(new Vector3(0, 0, -50), 150));
			//}
		}
		[Test] public void IsSphereInViewOrthographic()
		{
			Vector2 viewportSize = new Vector2(800, 600);
			using (DrawDevice device = new DrawDevice())
			{
				device.TargetSize = viewportSize;
				device.ViewportRect = new Rect(viewportSize);
				device.ViewerPos = new Vector3(0, 0, 0);
				device.ViewerAngle = new Vector3(0, 0, 0);
				device.FocusDist = 0;
				device.NearZ = 100;
				device.FarZ = 10000;
				device.Projection = ProjectionMode.Orthographic;

				float sphereDist = -500;

				// Viewport center
				Assert.IsTrue(device.IsSphereInView(new Vector3(0, 0, sphereDist), 150));

				// Just inside each of the viewports sides
				Assert.IsTrue(device.IsSphereInView(new Vector3(-viewportSize.X * 0.5f - 100, 0, sphereDist), 150));
				Assert.IsTrue(device.IsSphereInView(new Vector3(0, -viewportSize.Y * 0.5f - 100, sphereDist), 150));
				Assert.IsTrue(device.IsSphereInView(new Vector3(viewportSize.X * 0.5f + 100, 0, sphereDist), 150));
				Assert.IsTrue(device.IsSphereInView(new Vector3(0, viewportSize.Y * 0.5f + 100, sphereDist), 150));
				Assert.IsTrue(device.IsSphereInView(new Vector3(0, 0, (-device.FarZ) - 50), 150));
				Assert.IsTrue(device.IsSphereInView(new Vector3(0, 0, (-device.NearZ) + 50), 150));
				
				// Just outside each of the viewports sides
				Assert.IsFalse(device.IsSphereInView(new Vector3(-viewportSize.X * 0.5f - 200, 0, sphereDist), 150));
				Assert.IsFalse(device.IsSphereInView(new Vector3(0, -viewportSize.Y * 0.5f - 200, sphereDist), 150));
				Assert.IsFalse(device.IsSphereInView(new Vector3(viewportSize.X * 0.5f + 200, 0, sphereDist), 150));
				Assert.IsFalse(device.IsSphereInView(new Vector3(0, viewportSize.Y * 0.5f + 200, sphereDist), 150));
				Assert.IsFalse(device.IsSphereInView(new Vector3(0, 0, (-device.FarZ) - 151), 150));
				Assert.IsFalse(device.IsSphereInView(new Vector3(0, 0, (-device.NearZ) + 151), 150));
			}
		}
		[Test] public void IsSphereInViewPerspective()
		{
			Vector2 viewportSize = new Vector2(800, 600);
			using (DrawDevice device = new DrawDevice())
			{
				device.TargetSize = viewportSize;
				device.ViewportRect = new Rect(viewportSize);
				device.ViewerPos = new Vector3(0, 0, 0);
				device.ViewerAngle = new Vector3(0, 0, 0);
				device.FocusDist = 0f;
				device.NearZ = 100;
				device.FarZ = 10000;
				device.Projection = ProjectionMode.Perspective;

				float sphereDist = -500;

				// Viewport center
				Assert.IsTrue(device.IsSphereInView(new Vector3(0, 0, sphereDist), 150));

				// Just inside each of the viewports sides
				Assert.IsTrue(device.IsSphereInView(new Vector3(-viewportSize.X * 0.5f - 100, 0, sphereDist), 150));
				Assert.IsTrue(device.IsSphereInView(new Vector3(0, -viewportSize.Y * 0.5f - 100, sphereDist), 150));
				Assert.IsTrue(device.IsSphereInView(new Vector3(viewportSize.X * 0.5f + 100, 0, sphereDist), 150));
				Assert.IsTrue(device.IsSphereInView(new Vector3(0, viewportSize.Y * 0.5f + 100, sphereDist), 150));
				Assert.IsTrue(device.IsSphereInView(new Vector3(0, 0, (-device.FarZ) - 50), 150));
				Assert.IsTrue(device.IsSphereInView(new Vector3(0, 0, (-device.NearZ) + 50), 150));
				
				// Just outside each of the viewports sides
				Assert.IsFalse(device.IsSphereInView(new Vector3(-viewportSize.X * 0.5f - 300, 0, sphereDist), 150));
				Assert.IsFalse(device.IsSphereInView(new Vector3(0, -viewportSize.Y * 0.5f - 300, sphereDist), 150));
				Assert.IsFalse(device.IsSphereInView(new Vector3(viewportSize.X * 0.5f + 300, 0, sphereDist), 150));
				Assert.IsFalse(device.IsSphereInView(new Vector3(0, viewportSize.Y * 0.5f + 300, sphereDist), 150));
				Assert.IsFalse(device.IsSphereInView(new Vector3(0, 0, (-device.FarZ) - 151), 150));
				Assert.IsFalse(device.IsSphereInView(new Vector3(0, 0, (-device.NearZ) + 151), 150));

				// Things that are in/visible because of perspective projection
				Assert.IsFalse(device.IsSphereInView(new Vector3(-viewportSize.X * 0.5f - 100, 0, sphereDist * 0.5f), 150));
				Assert.IsTrue(device.IsSphereInView(new Vector3(-viewportSize.X * 0.5f - 200, 0, sphereDist * 2.0f), 150));
			}
		}

		[Test] public void GetScreenPos()
		{
			using (DrawDevice device = new DrawDevice())
			{
				Vector2 targetSize = new Vector2(800, 600);
				Vector2 viewportCenter = targetSize * 0.5f;

				// We'll check twice the focus distance to make sure orthographic
				// scaling is working as expected.
				device.FocusDist = DrawDevice.DefaultFocusDist * 2.0f;
				device.NearZ = 100;
				device.FarZ = 10000;
				device.TargetSize = targetSize;
				device.ViewportRect = new Rect(targetSize);
				device.ViewerPos = new Vector3(0, 0, -device.FocusDist);

				// Screen space rendering
				device.Projection = ProjectionMode.Screen;

				// 1:1 screen coordinate output in all cases
				Assert.AreEqual(new Vector2(0.0f, 0.0f), device.GetScreenPos(new Vector3(0.0f, 0.0f, 0.0f)));
				Assert.AreEqual(new Vector2(400.0f, 300.0f), device.GetScreenPos(new Vector3(400.0f, 300.0f, 0.0f)));
				Assert.AreEqual(new Vector2(800.0f, 600.0f), device.GetScreenPos(new Vector3(800.0f, 600.0f, 0.0f)));
				Assert.AreEqual(new Vector2(0.0f, 0.0f), device.GetScreenPos(new Vector3(0.0f, 0.0f, 1000.0f)));
				Assert.AreEqual(new Vector2(400.0f, 0.0f), device.GetScreenPos(new Vector3(400.0f, 0.0f, 1000.0f)));
				Assert.AreEqual(new Vector2(800.0f, 0.0f), device.GetScreenPos(new Vector3(800.0f, 0.0f, 1000.0f)));
				Assert.AreEqual(new Vector2(0.0f, 300.0f), device.GetScreenPos(new Vector3(0.0f, 300.0f, 1000.0f)));
				Assert.AreEqual(new Vector2(0.0f, 600.0f), device.GetScreenPos(new Vector3(0.0f, 600.0f, 1000.0f)));

				// World space rendering with orthographic projection
				device.Projection = ProjectionMode.Orthographic;

				// Scaled up 2:1 due to focus distance scaling factor
				Assert.AreEqual(viewportCenter, device.GetScreenPos(new Vector3(0.0f, 0.0f, 0.0f)));
				Assert.AreEqual(viewportCenter + new Vector2(-400.0f, -300.0f), device.GetScreenPos(new Vector3(-200.0f, -150.0f, 0.0f)));
				Assert.AreEqual(viewportCenter + new Vector2(400.0f, 300.0f), device.GetScreenPos(new Vector3(200.0f, 150.0f, 0.0f)));

				// No scale changes at other distances
				Assert.AreEqual(viewportCenter, device.GetScreenPos(new Vector3(0.0f, 0.0f, 1000.0f)));
				Assert.AreEqual(viewportCenter + new Vector2(-400.0f, 0.0f), device.GetScreenPos(new Vector3(-200.0f, 0.0f, 1000.0f)));
				Assert.AreEqual(viewportCenter + new Vector2(400.0f, 0.0f), device.GetScreenPos(new Vector3(200.0f, 0.0f, 1000.0f)));
				Assert.AreEqual(viewportCenter + new Vector2(0.0f, -300.0f), device.GetScreenPos(new Vector3(0.0f, -150.0f, 1000.0f)));
				Assert.AreEqual(viewportCenter + new Vector2(0.0f, 300.0f), device.GetScreenPos(new Vector3(0.0f, 150.0f, 1000.0f)));

				// World space rendering with perspective projection
				device.Projection = ProjectionMode.Perspective;
				
				// 1:1 scaling at focus distance
				Assert.AreEqual(viewportCenter, device.GetScreenPos(new Vector3(0.0f, 0.0f, 0.0f)));
				Assert.AreEqual(viewportCenter + new Vector2(-400.0f, -300.0f), device.GetScreenPos(new Vector3(-400.0f, -300.0f, 0.0f)));
				Assert.AreEqual(viewportCenter + new Vector2(400.0f, 300.0f), device.GetScreenPos(new Vector3(400.0f, 300.0f, 0.0f)));

				// Scaled down 1:2 at double the focus distance
				Assert.AreEqual(viewportCenter, device.GetScreenPos(new Vector3(0.0f, 0.0f, 1000.0f)));
				Assert.AreEqual(viewportCenter + new Vector2(-200.0f, 0.0f), device.GetScreenPos(new Vector3(-400.0f, 0.0f, 1000.0f)));
				Assert.AreEqual(viewportCenter + new Vector2(200.0f, 0.0f), device.GetScreenPos(new Vector3(400.0f, 0.0f, 1000.0f)));
				Assert.AreEqual(viewportCenter + new Vector2(0.0f, -150.0f), device.GetScreenPos(new Vector3(0.0f, -300.0f, 1000.0f)));
				Assert.AreEqual(viewportCenter + new Vector2(0.0f, 150.0f), device.GetScreenPos(new Vector3(0.0f, 300.0f, 1000.0f)));
			}
		}
		[Test] public void GetWorldPos()
		{
			// Could be more Thorough
			using (DrawDevice device = new DrawDevice())
			{
				Vector2 targetSize = new Vector2(800, 600);
				Vector2 viewportCenter = targetSize * 0.5f;

				// We'll check twice the focus distance to make sure orthographic
				// scaling is working as expected.
				device.FocusDist = 0.0f;
				device.NearZ = 100;
				device.FarZ = 10000;
				device.TargetSize = targetSize;
				device.ViewportRect = new Rect(targetSize);
				device.ViewerPos = new Vector3(0, 0, 0);

				// Screen space rendering
				device.Projection = ProjectionMode.Screen;

				// 1:1 world coordinate output in all cases
				AssertRoughlyEqual(new Vector3(0.0f, 0.0f, 0.0f), device.GetWorldPos(new Vector3(0.0f, 0.0f, 0.0f)));
				AssertRoughlyEqual(new Vector3(400.0f, 300.0f, 0.0f), device.GetWorldPos(new Vector3(400.0f, 300.0f, 0.0f)));
				AssertRoughlyEqual(new Vector3(800.0f, 600.0f, 0.0f), device.GetWorldPos(new Vector3(800.0f, 600.0f, 0.0f)));

				// World space rendering with orthographic projection
				device.Projection = ProjectionMode.Orthographic;

				// Near Clip
				AssertRoughlyEqual(new Vector3(0.0f, 0.0f, -100), device.GetWorldPos(new Vector3(viewportCenter, 0.0f)));
				AssertRoughlyEqual(new Vector3(0.0f, 300f, -100), device.GetWorldPos(new Vector3(viewportCenter.X, 0, 0.0f)));
				AssertRoughlyEqual(new Vector3(-400f, 0.0f, -100), device.GetWorldPos(new Vector3(0, viewportCenter.Y, 0.0f)));
				// Far Clip
				AssertRoughlyEqual(new Vector3(0.0f, 0.0f, -10000), device.GetWorldPos(new Vector3(viewportCenter, 1.0f)));
				AssertRoughlyEqual(new Vector3(0.0f, 300f, -10000), device.GetWorldPos(new Vector3(viewportCenter.X, 0, 1.0f)));
				AssertRoughlyEqual(new Vector3(-400f, 0.0f, -10000), device.GetWorldPos(new Vector3(0, viewportCenter.Y, 1.0f)));

				// World space rendering with perspective projection
				device.Projection = ProjectionMode.Perspective;

				// Near Clip
				AssertRoughlyEqual(new Vector3(0.0f, 0.0f, -100), device.GetWorldPos(new Vector3(viewportCenter, 0.0f)));
				AssertRoughlyEqual(new Vector3(0.0f, 70.02075f, -100), device.GetWorldPos(new Vector3(viewportCenter.X, 0, 0.0f)));
				AssertRoughlyEqual(new Vector3(0.0f, -70.02075f, -100), device.GetWorldPos(new Vector3(viewportCenter.X, targetSize.Y, 0.0f)));
				AssertRoughlyEqual(new Vector3(-93.36101f, 0.0f, -100), device.GetWorldPos(new Vector3(0, viewportCenter.Y, 0.0f)));
				AssertRoughlyEqual(new Vector3(93.36101f, 0.0f, -100), device.GetWorldPos(new Vector3(targetSize.X, viewportCenter.Y, 0.0f)));
				AssertRoughlyEqual(new Vector3(-93.36101f, 70.02075f, -100), device.GetWorldPos(new Vector3(0, 0, 0.0f)));
				AssertRoughlyEqual(new Vector3(93.36101f, -70.02075f, -100), device.GetWorldPos(new Vector3(targetSize.X, targetSize.Y, 0.0f)));

				// Far Clip
				AssertRoughlyEqual(new Vector3(0.0f, 0.0f, -10000), device.GetWorldPos(new Vector3(viewportCenter, 1.0f)));
				AssertRoughlyEqual(new Vector3(0.0f, 70.02075f, -10000), device.GetWorldPos(new Vector3(viewportCenter.X, 0, 1.0f)));
				AssertRoughlyEqual(new Vector3(0.0f, -70.02075f, -10000), device.GetWorldPos(new Vector3(viewportCenter.X, targetSize.Y, 1.0f)));
				AssertRoughlyEqual(new Vector3(-93.36101f, 0.0f, -10000), device.GetWorldPos(new Vector3(0, viewportCenter.Y, 1.0f)));
				AssertRoughlyEqual(new Vector3(93.36101f, 0.0f, -10000), device.GetWorldPos(new Vector3(targetSize.X, viewportCenter.Y, 1.0f)));
				AssertRoughlyEqual(new Vector3(-93.36101f, 70.02075f, -10000), device.GetWorldPos(new Vector3(0, 0, 1.0f)));
				AssertRoughlyEqual(new Vector3(93.36101f, -70.02075f, -10000), device.GetWorldPos(new Vector3(targetSize.X, targetSize.Y, 1.0f)));
			}
		}

		private static void AssertRoughlyEqual(Vector3 expected, Vector3 actual)
		{
			float threshold = 0.001f;
			Assert.IsTrue(
				(expected - actual).Length < threshold,
				string.Format(
					"{0} is equal to {1} within a threshold of {2}.",
					actual,
					expected,
					threshold));
		}
	}
}
