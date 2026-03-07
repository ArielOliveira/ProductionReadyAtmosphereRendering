#ifndef MATH
#define MATH

#ifndef PI
    #define PI 3.1415926535897932384626433832795
#endif

#ifndef RAD_2_DEG 
    #define RAD_2_DEG 57.29578
#endif

#define TAU 6.283185307179586476925286766559
#define MAX_FLOAT 3.402823466e+38

float3 rotateAboutAxisDegrees(float3 In, float3 Axis, float Rotation) {
    Rotation = radians(Rotation);
    float s = sin(Rotation);
    float c = cos(Rotation);
    float one_minus_c = 1.0 - c;

    Axis = normalize(Axis);
    float3x3 rot_mat = 
    {   one_minus_c * Axis.x * Axis.x + c, one_minus_c * Axis.x * Axis.y - Axis.z * s, one_minus_c * Axis.z * Axis.x + Axis.y * s,
        one_minus_c * Axis.x * Axis.y + Axis.z * s, one_minus_c * Axis.y * Axis.y + c, one_minus_c * Axis.y * Axis.z - Axis.x * s,
        one_minus_c * Axis.z * Axis.x - Axis.y * s, one_minus_c * Axis.y * Axis.z + Axis.x * s, one_minus_c * Axis.z * Axis.z + c
    };
    
    return mul(rot_mat,  In);
}

float3 getSphericalDir(float theta, float phi) {
    float cosPhi = cos(phi);
    float sinPhi = sin(phi);
    float cosTheta = cos(theta);
    float sinTheta = sin(theta);
     
    return float3(sinPhi*sinTheta, cosPhi, sinPhi*cosTheta);
}

float safeacos(const float x) {
    return acos(clamp(x, -1.0, 1.0));
}

// Returns vector (dstToSphere, dstThroughSphere)
// If ray origin is inside sphere, dstToSphere = 0
// If ray misses sphere, dstToSphere = maxValue; dstThroughSphere = 0
float2 raySphere(float3 sphereCentre, float sphereRadius, float3 rayOrigin, float3 rayDir) {
	float3 offset = rayOrigin - sphereCentre;
	float a = 1; // Set to dot(rayDir, rayDir) if rayDir might not be normalized
	float b = 2 * dot(offset, rayDir);
	float c = dot (offset, offset) - sphereRadius * sphereRadius;
	float d = b * b - 4 * a * c; // Discriminant from quadratic formula

	// Number of intersections: 0 when d < 0; 1 when d = 0; 2 when d > 0
	if (d > 0) {
		float s = sqrt(d);
		float dstToSphereNear = max(0, (-b - s) / (2 * a));
		float dstToSphereFar = (-b + s) / (2 * a);

		// Ignore intersections that occur behind the ray
		if (dstToSphereFar >= 0) {
			return float2(dstToSphereNear, dstToSphereFar - dstToSphereNear);
		}
	}
	
    // Ray did not intersect sphere		
    return float2(MAX_FLOAT, 0);
}

float rayIntersectSphere(float3 ro, float3 rd, float rad) {
    float b = dot(ro, rd);
    float c = dot(ro, ro) - rad*rad;
    if (c > 0.0f && b > 0.0) return -1.0;
    float discr = b*b - c;
    if (discr < 0.0) return -1.0;
    // Special case: inside sphere, use far discriminant
    if (discr > b*b) return (-b + sqrt(discr));
    return -b - sqrt(discr);
}

float floatToSrgb(float value) {
    const float inv_12_92 = 0.0773993808;
    return value <= 0.04045
       ? value * inv_12_92 
       : pow((value + 0.055) / 1.055, 2.4);
}

float3 float3ToSRGB(float3 value) {
    return float3(floatToSrgb(value.x), floatToSrgb(value.y), floatToSrgb(value.z));
}

float3 ACESFilm(float3 x)
{
float a = 2.51;
float b = 0.03;
float c = 2.43;
float d = 0.59;
float e = 0.14;
return saturate((x*(a*x+b))/(x*(c*x+d)+e));
}

#endif