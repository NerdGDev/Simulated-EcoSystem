using UnityEngine;

namespace Kit
{
    /// <summary>
    /// Spherical coordinates.
    /// </summary>
    /// <see cref="http://wiki.unity3d.com/index.php/SphericalCoordinates"/>
    [System.Serializable]
    public class SphericalCoordinates
    {
        [HideInInspector] public float _radius, _polar, _elevation;
        private float _minRadius, _maxRadius, _minPolar, _maxPolar, _minElevation, _maxElevation;
        public bool loopPolar = true;
        public bool loopElevation = true;

        /// <summary>
        /// the radial distance of that point from a fixed origin.
        /// Radius must be >= 0
        /// </summary>
        public float radius
        {
            get { return _radius; }
            set {
                float _tmp = Mathf.Clamp(value, _minRadius, _maxRadius);
                if (!float.IsNaN(_tmp)) _radius = _tmp;
            }
        }
		
        /// <summary>
        /// azimuth angle (in radian) of its orthogonal projection on 
        /// a reference plane that passes through the origin and is orthogonal to the zenith
        /// </summary>
        public float polar
        {
            get { return _polar; }
            set {
				float _tmp = loopPolar
					? Mathf.Repeat(value, _maxPolar - _minPolar)
					: Mathf.Clamp(value, _minPolar, _maxPolar);
                if (!float.IsNaN(_tmp)) _polar = _tmp;
            }
        }
        public float polarDegree
        {
            get { return polar * Mathf.Rad2Deg; }
            set { polar = value * Mathf.Deg2Rad; }
        }
        /// <summary>
        /// elevation angle (in radian) from the reference plane 
        /// </summary>
        public float elevation
        {
            get { return _elevation; }
            set {
                float _tmp = loopElevation
                    ? Mathf.Repeat(value, _maxElevation - _minElevation)
                    : Mathf.Clamp(value, _minElevation, _maxElevation);
                if (!float.IsNaN(_tmp)) _elevation = _tmp;
            }
        }
        public float elevationDegree
        {
            get { return elevation * Mathf.Rad2Deg; }
            set { elevation = value * Mathf.Deg2Rad; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Kit.Coordinates.SphericalCoordinates"/> class.
        /// </summary>
        /// <param name="radius">Radius.</param>
        /// <param name="polar">Polar.</param>
        /// <param name="elevation">Elevation.</param>
        /// <param name="minRadius">Minimum radius.</param>
        /// <param name="maxRadius">Max radius.</param>
        /// <param name="minPolar">Minimum polar.</param>
        /// <param name="maxPolar">Max polar.</param>
        /// <param name="minElevation">Minimum elevation.</param>
        /// <param name="maxElevation">Max elevation.</param>
        public SphericalCoordinates(float radius = 0f, float polar = 0f, float elevation = 0f,
                                    float minRadius = float.MinValue, float maxRadius = float.MaxValue,
                                    float minPolar = 0f, float maxPolar = (Mathf.PI * 2f), bool loopPolar = true,
                                    float minElevation = 0f, float maxElevation = (Mathf.PI * 2f), bool loopElevation = true)
        {
            _minRadius = minRadius;
            _maxRadius = maxRadius;
            _minPolar = minPolar;
            _maxPolar = maxPolar;
            this.loopPolar = loopPolar;
            _minElevation = minElevation;
            _maxElevation = maxElevation;
            this.loopElevation = loopElevation;
            SetRadius(radius);
            SetRotation(polar, elevation);
        }

        public SphericalCoordinates(Transform T,
                                    float minRadius = float.MinValue, float maxRadius = float.MaxValue,
                                    float minPolar = 0f, float maxPolar = (Mathf.PI * 2f), bool loopPolar = true,
                                    float minElevation = 0f, float maxElevation = (Mathf.PI * 2f), bool loopElevation = true)
            : this(T.position, minRadius, maxRadius, minPolar, maxPolar, loopPolar, minElevation, maxElevation, loopElevation) { }

        public SphericalCoordinates(Vector3 cartesianCoordinate,
                                    float minRadius = float.MinValue, float maxRadius = float.MaxValue,
                                    float minPolar = 0f, float maxPolar = (Mathf.PI * 2f), bool loopPolar = true,
                                    float minElevation = 0f, float maxElevation = (Mathf.PI * 2f), bool loopElevation = true)
        {
            _minRadius = minRadius;
            _maxRadius = maxRadius;
            _minPolar = minPolar;
            _maxPolar = maxPolar;
            this.loopPolar = loopPolar;
            _minElevation = minElevation;
            _maxElevation = maxElevation;
            this.loopElevation = loopElevation;
            FromCartesian(cartesianCoordinate);
        }

        public SphericalCoordinates SetUnlimit()
        {
            return this.SetPolarLimit(0f, Mathf.PI * 2f, true).SetElevationLimit(0f, Mathf.PI * 2f, true);
        }

        public SphericalCoordinates SetPolarLimit(float min, float max, bool loop = false)
        {
            _minPolar = Mathf.Clamp(min, 0f, Mathf.PI * 2f);
            _maxPolar = Mathf.Clamp(max, _minPolar, Mathf.PI * 2f);
            loopPolar = loop;
            return this;
        }

        public SphericalCoordinates SetElevationLimit(float min, float max, bool loop = false)
        {
            _minElevation = Mathf.Clamp(min, -Mathf.PI, Mathf.PI);
            _maxElevation = Mathf.Clamp(max, _minElevation, Mathf.PI * 2f);
            loopElevation = loop;
            return this;
        }



        /// <summary>
        /// Converts a point from Spherical coordinates to Cartesian (using positive * Y as up)
        /// </summary>
        /// <see cref="http://en.wikipedia.org/wiki/Spherical_coordinate_system"/>
        /// <seealso cref="http://blog.nobel-joergensen.com/2010/10/22/spherical-coordinates-in-unity/"/>
        public Vector3 ToCartesian()
        {
            float a = radius * Mathf.Cos(elevation);
            return new Vector3(a * Mathf.Cos(polar), radius * Mathf.Sin(elevation), a * Mathf.Sin(polar));
        }
        /// <summary>
        /// Convert from cartesian coordinates to spherical coordinates.
        /// </summary>
        /// <see cref="http://en.wikipedia.org/wiki/Spherical_coordinate_system"/>
        /// <seealso cref="http://blog.nobel-joergensen.com/2010/10/22/spherical-coordinates-in-unity/"/>
        public SphericalCoordinates FromCartesian(Vector3 cartesianCoordinate)
		{
			if (cartesianCoordinate.x == 0f)
				cartesianCoordinate.x = Mathf.Epsilon;
			radius = cartesianCoordinate.magnitude;
			polar = Mathf.Atan(cartesianCoordinate.z / cartesianCoordinate.x);
			if (cartesianCoordinate.x < 0f)
				polar += Mathf.PI;
			elevation = Mathf.Asin(cartesianCoordinate.y / radius);
			return this;
		}

        public Quaternion ToLocalRotation(Vector3 upward = default(Vector3))
        {
            if (upward == default(Vector3))
            {
                return Quaternion.AngleAxis(polar * -Mathf.Rad2Deg, Vector3.up) *
                    Quaternion.AngleAxis(elevation * -Mathf.Rad2Deg, Vector3.right);
            }
            else
            {
                return Quaternion.LookRotation(ToCartesian().normalized, upward);
            }
        }
        public Quaternion ToPolarRotation()
        {
            return Quaternion.AngleAxis(polar * -Mathf.Rad2Deg, Vector3.up);
        }
        public Quaternion ToElevationRotation()
        {
            return Quaternion.AngleAxis(elevation * -Mathf.Rad2Deg, Vector3.right);
        }


        /// <summary>Calculate the relative polar, elevation, radius between two coordinates.</summary>
        /// <param name="Relative"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        public static SphericalCoordinates RelativeToTarget(Vector3 Relative, Vector3 target)
        {
			return new SphericalCoordinates().FromCartesian(Relative - target);
        }

		public static SphericalCoordinates Zero
        {
            get { return new SphericalCoordinates(); }
        }

		public SphericalCoordinates RotatePolarAngle(float x) { return Rotate(x, 0f); }
		public SphericalCoordinates RotateElevationAngle(float x) { return Rotate(0f, x); }
		public SphericalCoordinates Rotate(float newPolar, float newElevation){ return SetRotation( polar + newPolar, elevation + newElevation ); }
		public SphericalCoordinates SetPolarAngle(float x) { return SetRotation(x, elevation); }
		public SphericalCoordinates SetElevationAngle(float x) { return SetRotation(x, elevation); }
		public SphericalCoordinates SetRotation(float newPolar, float newElevation)
		{
			polar = newPolar;		
			elevation = newElevation;
			
			return this;
		}
		
		public SphericalCoordinates TranslateRadius(float x) { return SetRadius(radius + x); }
		public SphericalCoordinates SetRadius(float rad)
		{
			radius = rad;
			return this;
		}

		public override string ToString ()
		{
			return string.Format ("[Spherical Coordinates Radius={0:F2}, polar={1:F2}|{3:F2}, elevation={2:F2}|{4:F2}]",radius,polar,elevation,Mathf.Repeat(polarDegree,360f),elevationDegree);
		}
        public SphericalCoordinates Clone()
        {
            return new SphericalCoordinates(radius, polar, elevation,
                _minRadius, _maxRadius,
                _minPolar, _maxPolar, loopPolar,
                _minElevation, _maxElevation, loopElevation);
        }
	}
    
}