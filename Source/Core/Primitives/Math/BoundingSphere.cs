// MIT License - Copyright (C) The Mono.Xna Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.Serialization;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Duality
{

	[StructLayout(LayoutKind.Sequential)]
	public struct BoundingSphere : IEquatable<BoundingSphere>
    {
        #region Public Fields

        public Vector3 Center;

        public double Radius;

        #endregion Public Fields


        #region Constructors

        public BoundingSphere(Vector3 center, double radius)
        {
            this.Center = center;
            this.Radius = radius;
        }

        #endregion Constructors


        #region Public Methods

        public BoundingSphere Transform(Matrix4 matrix)
        {
            BoundingSphere sphere = new BoundingSphere();
            sphere.Center = Vector3.Transform(this.Center, matrix);
            sphere.Radius = this.Radius * ((double)Math.Sqrt((double)Math.Max(((matrix.M11 * matrix.M11) + (matrix.M12 * matrix.M12)) + (matrix.M13 * matrix.M13), Math.Max(((matrix.M21 * matrix.M21) + (matrix.M22 * matrix.M22)) + (matrix.M23 * matrix.M23), ((matrix.M31 * matrix.M31) + (matrix.M32 * matrix.M32)) + (matrix.M33 * matrix.M33)))));
            return sphere;
        }

        public void Transform(ref Matrix4 matrix, out BoundingSphere result)
        {
            result.Center = Vector3.Transform(this.Center, matrix);
            result.Radius = this.Radius * ((double)Math.Sqrt((double)Math.Max(((matrix.M11 * matrix.M11) + (matrix.M12 * matrix.M12)) + (matrix.M13 * matrix.M13), Math.Max(((matrix.M21 * matrix.M21) + (matrix.M22 * matrix.M22)) + (matrix.M23 * matrix.M23), ((matrix.M31 * matrix.M31) + (matrix.M32 * matrix.M32)) + (matrix.M33 * matrix.M33)))));
        }

        public ContainmentType Contains(BoundingBox box)
        {
            //check if all corner is in sphere
            bool inside = true;
            foreach (Vector3 corner in box.GetCorners())
            {
                if (this.Contains(corner) == ContainmentType.Disjoint)
                {
                    inside = false;
                    break;
                }
            }

            if (inside)
                return ContainmentType.Contains;

            //check if the distance from sphere center to cube face < radius
            double dmin = 0;

            if (this.Center.X < box.Min.X)
				dmin += (this.Center.X - box.Min.X) * (this.Center.X - box.Min.X);

			else if (this.Center.X > box.Max.X)
					dmin += (this.Center.X - box.Max.X) * (this.Center.X - box.Max.X);

			if (this.Center.Y < box.Min.Y)
				dmin += (this.Center.Y - box.Min.Y) * (this.Center.Y - box.Min.Y);

			else if (this.Center.Y > box.Max.Y)
				dmin += (this.Center.Y - box.Max.Y) * (this.Center.Y - box.Max.Y);

			if (this.Center.Z < box.Min.Z)
				dmin += (this.Center.Z - box.Min.Z) * (this.Center.Z - box.Min.Z);

			else if (this.Center.Z > box.Max.Z)
				dmin += (this.Center.Z - box.Max.Z) * (this.Center.Z - box.Max.Z);

			if (dmin <= this.Radius * this.Radius) 
				return ContainmentType.Intersects;
            
            //else disjoint
            return ContainmentType.Disjoint;
        }

        public void Contains(ref BoundingBox box, out ContainmentType result)
        {
            result = this.Contains(box);
        }

        public ContainmentType Contains(BoundingFrustum frustum)
        {
            //check if all corner is in sphere
            bool inside = true;

            Vector3[] corners = frustum.GetCorners();
            foreach (Vector3 corner in corners)
            {
                if (this.Contains(corner) == ContainmentType.Disjoint)
                {
                    inside = false;
                    break;
                }
            }
            if (inside)
                return ContainmentType.Contains;

            //check if the distance from sphere center to frustrum face < radius
            double dmin = 0;
            //TODO : calcul dmin

            if (dmin <= this.Radius * this.Radius)
                return ContainmentType.Intersects;

            //else disjoint
            return ContainmentType.Disjoint;
        }

        public ContainmentType Contains(BoundingSphere sphere)
        {
            ContainmentType result;
			this.Contains(ref sphere, out result);
            return result;
        }

        public void Contains(ref BoundingSphere sphere, out ContainmentType result)
        {
            double sqDistance;
            Vector3.DistanceSquared(ref sphere.Center, ref this.Center, out sqDistance);

            if (sqDistance > (sphere.Radius + this.Radius) * (sphere.Radius + this.Radius))
                result = ContainmentType.Disjoint;

            else if (sqDistance <= (this.Radius - sphere.Radius) * (this.Radius - sphere.Radius))
                result = ContainmentType.Contains;

            else
                result = ContainmentType.Intersects;
        }

        public ContainmentType Contains(Vector3 point)
        {
            ContainmentType result;
			this.Contains(ref point, out result);
            return result;
        }

        public void Contains(ref Vector3 point, out ContainmentType result)
        {
            double sqRadius = this.Radius * this.Radius;
            double sqDistance;
            Vector3.DistanceSquared(ref point, ref this.Center, out sqDistance);
            
            if (sqDistance > sqRadius)
                result = ContainmentType.Disjoint;

            else if (sqDistance < sqRadius)
                result = ContainmentType.Contains;

            else 
                result = ContainmentType.Intersects;
        }

        public static BoundingSphere CreateFromBoundingBox(BoundingBox box)
        {
            BoundingSphere result;
            CreateFromBoundingBox(ref box, out result);
            return result;
        }

        public static void CreateFromBoundingBox(ref BoundingBox box, out BoundingSphere result)
        {
            // Find the center of the box.
            Vector3 center = new Vector3((box.Min.X + box.Max.X) / 2.0,
                                         (box.Min.Y + box.Max.Y) / 2.0,
                                         (box.Min.Z + box.Max.Z) / 2.0);

            // Find the distance between the center and one of the corners of the box.
            double radius = Vector3.Distance(center, box.Max);

            result = new BoundingSphere(center, radius);
        }

        public static BoundingSphere CreateFromFrustum(BoundingFrustum frustum)
        {
            return BoundingSphere.CreateFromPoints(frustum.GetCorners());
        }

        public static BoundingSphere CreateFromPoints(IEnumerable<Vector3> points)
        {
            if (points == null )
                throw new ArgumentNullException("points");

            // From "Real-Time Collision Detection" (Page 89)

            var minx = new Vector3(double.MaxValue, double.MaxValue, double.MaxValue);
            var maxx = -minx;
            var miny = minx;
            var maxy = -minx;
            var minz = minx;
            var maxz = -minx;

			// Find the most extreme points along the principle axis.
			int numPoints = 0;           
            foreach (var pt in points)
            {
                ++numPoints;

                if (pt.X < minx.X) 
                    minx = pt;
                if (pt.X > maxx.X) 
                    maxx = pt;
                if (pt.Y < miny.Y) 
                    miny = pt;
                if (pt.Y > maxy.Y) 
                    maxy = pt;
                if (pt.Z < minz.Z) 
                    minz = pt;
                if (pt.Z > maxz.Z) 
                    maxz = pt;
            }

            if (numPoints == 0)
                throw new ArgumentException("You should have at least one point in points.");

            double sqDistX = Vector3.DistanceSquared(maxx, minx);
            double sqDistY = Vector3.DistanceSquared(maxy, miny);
			double sqDistZ = Vector3.DistanceSquared(maxz, minz);

            // Pick the pair of most distant points.
            var min = minx;
            var max = maxx;
            if (sqDistY > sqDistX && sqDistY > sqDistZ) 
            {
                max = maxy;
                min = miny;
            }
            if (sqDistZ > sqDistX && sqDistZ > sqDistY) 
            {
                max = maxz;
                min = minz;
            }
            
            var center = (min + max) * 0.5;
			double radius = Vector3.Distance(max, center);
            
            // Test every point and expand the sphere.
            // The current bounding sphere is just a good approximation and may not enclose all points.            
            // From: Mathematics for 3D Game Programming and Computer Graphics, Eric Lengyel, Third Edition.
            // Page 218
            double sqRadius = radius * radius;
            foreach (var pt in points)
            {
                Vector3 diff = (pt-center);
                double sqDist = diff.LengthSquared;
                if (sqDist > sqRadius)
                {
                    double distance = (double)Math.Sqrt(sqDist); // equal to diff.Length();
                    Vector3 direction = diff / distance;
                    Vector3 G = center - radius * direction;
                    center = (G + pt) / 2;
                    radius = Vector3.Distance(pt, center);
                    sqRadius = radius * radius;
                }
            }

            return new BoundingSphere(center, radius);
        }

        public static BoundingSphere CreateMerged(BoundingSphere original, BoundingSphere additional)
        {
            BoundingSphere result;
            CreateMerged(ref original, ref additional, out result);
            return result;
        }

        public static void CreateMerged(ref BoundingSphere original, ref BoundingSphere additional, out BoundingSphere result)
        {
            Vector3 ocenterToaCenter = Vector3.Subtract(additional.Center, original.Center);
            double distance = ocenterToaCenter.Length;
            if (distance <= original.Radius + additional.Radius)//intersect
            {
                if (distance <= original.Radius - additional.Radius)//original contain additional
                {
                    result = original;
                    return;
                }
                if (distance <= additional.Radius - original.Radius)//additional contain original
                {
                    result = additional;
                    return;
                }
            }
            //else find center of new sphere and radius
            double leftRadius = Math.Max(original.Radius - distance, additional.Radius);
            double Rightradius = Math.Max(original.Radius + distance, additional.Radius);
            ocenterToaCenter = ocenterToaCenter + (((leftRadius - Rightradius) / (2 * ocenterToaCenter.Length)) * ocenterToaCenter);//oCenterToResultCenter

            result = new BoundingSphere();
            result.Center = original.Center + ocenterToaCenter;
            result.Radius = (leftRadius + Rightradius) / 2;
        }

        public bool Equals(BoundingSphere other)
        {
            return this.Center == other.Center && this.Radius == other.Radius;
        }

        public override bool Equals(object obj)
        {
            if (obj is BoundingSphere)
                return this.Equals((BoundingSphere)obj);

            return false;
        }

        public override int GetHashCode()
        {
            return this.Center.GetHashCode() + this.Radius.GetHashCode();
        }

        public bool Intersects(BoundingBox box)
        {
			return box.Intersects(this);
        }

        public void Intersects(ref BoundingBox box, out bool result)
        {
            box.Intersects(ref this, out result);
        }

        public bool Intersects(BoundingSphere sphere)
        {
            bool result;
			this.Intersects(ref sphere, out result);
            return result;
        }

        public void Intersects(ref BoundingSphere sphere, out bool result)
        {
            double sqDistance;
            Vector3.DistanceSquared(ref sphere.Center, ref this.Center, out sqDistance);

            if (sqDistance > (sphere.Radius + this.Radius) * (sphere.Radius + this.Radius))
                result = false;
            else
                result = true;
        }

        public PlaneIntersectionType Intersects(Plane plane)
        {
            var result = default(PlaneIntersectionType);
            // TODO: we might want to inline this for performance reasons
            this.Intersects(ref plane, out result);
            return result;
        }

        public void Intersects(ref Plane plane, out PlaneIntersectionType result)
        {
			double distance = default(double);
            // TODO: we might want to inline this for performance reasons
            Vector3.Dot(ref plane.Normal, ref this.Center, out distance);
            distance += plane.D;
            if (distance > this.Radius)
                result = PlaneIntersectionType.Front;
            else if (distance < -this.Radius)
                result = PlaneIntersectionType.Back;
            else
                result = PlaneIntersectionType.Intersecting;
        }

        public Nullable<double> Intersects(Ray ray)
        {
            return ray.Intersects(this);
        }

        public void Intersects(ref Ray ray, out Nullable<double> result)
        {
            ray.Intersects(ref this, out result);
        }

        public static bool operator == (BoundingSphere a, BoundingSphere b)
        {
            return a.Equals(b);
        }

        public static bool operator != (BoundingSphere a, BoundingSphere b)
        {
            return !a.Equals(b);
		}

		public static implicit operator THREE.Math.Sphere(BoundingSphere s)
		{
			return new THREE.Math.Sphere(s.Center, (float)s.Radius);
		}

		public static implicit operator BoundingSphere(THREE.Math.Sphere s)
		{
			return new BoundingSphere(s.Center, s.Radius);
		}

		internal string DebugDisplayString
        {
            get
            {
                return string.Concat(
                    "Pos( ", this.Center.DebugDisplayString, " )  \r\n",
                    "Radius( ", this.Radius.ToString(), " )"
                    );
            }
        }

        public override string ToString()
        {
            return "{{Center:" + this.Center.ToString() + " Radius:" + this.Radius.ToString() + "}}";
        }

        #endregion Public Methods
    }
}
