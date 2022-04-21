// MIT License - Copyright (C) The Mono.Xna Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;

namespace Duality
{
	[StructLayout(LayoutKind.Sequential)]
	public struct Ray : IEquatable<Ray>
    {
        #region Public Fields

        public Vector3 Direction;
      
        public Vector3 Position;

        #endregion


        #region Public Constructors

        public Ray(Vector3 position, Vector3 direction)
        {
            this.Position = position;
            this.Direction = direction;
        }

        #endregion


        #region Public Methods

        public override bool Equals(object obj)
        {
            return (obj is Ray) ? this.Equals((Ray)obj) : false;
        }

        
        public bool Equals(Ray other)
        {
            return this.Position.Equals(other.Position) && this.Direction.Equals(other.Direction);
        }

        
        public override int GetHashCode()
        {
            return this.Position.GetHashCode() ^ this.Direction.GetHashCode();
        }

        // adapted from http://www.scratchapixel.com/lessons/3d-basic-lessons/lesson-7-intersecting-simple-shapes/ray-box-intersection/
        public double? Intersects(BoundingBox box)
        {
            const double Epsilon = 1e-6;

            double? tMin = null, tMax = null;

            if (Math.Abs(this.Direction.X) < Epsilon)
            {
                if (this.Position.X < box.Min.X || this.Position.X > box.Max.X)
                    return null;
            }
            else
            {
                tMin = (box.Min.X - this.Position.X) / this.Direction.X;
                tMax = (box.Max.X - this.Position.X) / this.Direction.X;

                if (tMin > tMax)
                {
                    double? temp = tMin;
                    tMin = tMax;
                    tMax = temp;
                }
            }

            if (Math.Abs(this.Direction.Y) < Epsilon)
            {
                if (this.Position.Y < box.Min.Y || this.Position.Y > box.Max.Y)
                    return null;
            }
            else
            {
				double tMinY = (box.Min.Y - this.Position.Y) / this.Direction.Y;
				double tMaxY = (box.Max.Y - this.Position.Y) / this.Direction.Y;

                if (tMinY > tMaxY)
                {
					double temp = tMinY;
                    tMinY = tMaxY;
                    tMaxY = temp;
                }

                if ((tMin.HasValue && tMin > tMaxY) || (tMax.HasValue && tMinY > tMax))
                    return null;

                if (!tMin.HasValue || tMinY > tMin) tMin = tMinY;
                if (!tMax.HasValue || tMaxY < tMax) tMax = tMaxY;
            }

            if (Math.Abs(this.Direction.Z) < Epsilon)
            {
                if (this.Position.Z < box.Min.Z || this.Position.Z > box.Max.Z)
                    return null;
            }
            else
            {
				double tMinZ = (box.Min.Z - this.Position.Z) / this.Direction.Z;
				double tMaxZ = (box.Max.Z - this.Position.Z) / this.Direction.Z;

                if (tMinZ > tMaxZ)
                {
					double temp = tMinZ;
                    tMinZ = tMaxZ;
                    tMaxZ = temp;
                }

                if ((tMin.HasValue && tMin > tMaxZ) || (tMax.HasValue && tMinZ > tMax))
                    return null;

                if (!tMin.HasValue || tMinZ > tMin) tMin = tMinZ;
                if (!tMax.HasValue || tMaxZ < tMax) tMax = tMaxZ;
            }

            // having a positive tMin and a negative tMax means the ray is inside the box
            // we expect the intesection distance to be 0 in that case
            if ((tMin.HasValue && tMin < 0) && tMax > 0) return 0;

            // a negative tMin means that the intersection point is behind the ray's origin
            // we discard these as not hitting the AABB
            if (tMin < 0) return null;

            return tMin;
        }


        public void Intersects(ref BoundingBox box, out double? result)
        {
			result = this.Intersects(box);
        }


        public double? Intersects(BoundingSphere sphere)
        {
            double? result;
			this.Intersects(ref sphere, out result);
            return result;
        }

        public double? Intersects(Plane plane)
        {
            double? result;
			this.Intersects(ref plane, out result);
            return result;
        }

        public void Intersects(ref Plane plane, out double? result)
        {
			double den = Vector3.Dot(this.Direction, plane.Normal);
            if (Math.Abs(den) < 0.00001f)
            {
                result = null;
                return;
            }

            result = (-plane.D - Vector3.Dot(plane.Normal, this.Position)) / den;

            if (result < 0.0f)
            {
                if (result < -0.00001f)
                {
                    result = null;
                    return;
                }

                result = 0.0;
            }
        }

        public void Intersects(ref BoundingSphere sphere, out double? result)
        {
            // Find the vector between where the ray starts the the sphere's centre
            Vector3 difference = sphere.Center - this.Position;

            double differenceLengthSquared = difference.LengthSquared;
            double sphereRadiusSquared = sphere.Radius * sphere.Radius;

            double distanceAlongRay;

            // If the distance between the ray start and the sphere's centre is less than
            // the radius of the sphere, it means we've intersected. N.B. checking the LengthSquared is faster.
            if (differenceLengthSquared < sphereRadiusSquared)
            {
                result = 0.0;
                return;
            }

            Vector3.Dot(ref this.Direction, ref difference, out distanceAlongRay);
            // If the ray is pointing away from the sphere then we don't ever intersect
            if (distanceAlongRay < 0)
            {
                result = null;
                return;
            }

            // Next we kinda use Pythagoras to check if we are within the bounds of the sphere
            // if x = radius of sphere
            // if y = distance between ray position and sphere centre
            // if z = the distance we've travelled along the ray
            // if x^2 + z^2 - y^2 < 0, we do not intersect
            double dist = sphereRadiusSquared + distanceAlongRay * distanceAlongRay - differenceLengthSquared;

            result = (dist < 0) ? null : distanceAlongRay - (double?)Math.Sqrt(dist);
        }


        public static bool operator !=(Ray a, Ray b)
        {
            return !a.Equals(b);
        }


        public static bool operator ==(Ray a, Ray b)
        {
            return a.Equals(b);
		}

		public static implicit operator THREE.Math.Ray(Ray s)
		{
			return new THREE.Math.Ray(s.Position, s.Direction);
		}

		public static implicit operator Ray(THREE.Math.Ray s)
		{
			return new Ray(s.origin, s.direction);
		}

		internal string DebugDisplayString
        {
            get
            {
                return string.Concat(
                    "Pos( ", this.Position.DebugDisplayString, " )  \r\n",
                    "Dir( ", this.Direction.DebugDisplayString, " )"
                );
            }
        }

        public override string ToString()
        {
            return "{{Position:" + this.Position.ToString() + " Direction:" + this.Direction.ToString() + "}}";
        }
		
		#endregion
    }
}