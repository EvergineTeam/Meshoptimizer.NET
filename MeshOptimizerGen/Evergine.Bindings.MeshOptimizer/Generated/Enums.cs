using System;

namespace Evergine.Bindings.MeshOptimizer
{
	/// <summary>
	/// Vertex buffer filter encoders
	/// These functions can be used to encode data in a format that meshopt_decodeFilter can decode
	/// meshopt_encodeFilterOct encodes unit vectors with K-bit (K 
	/// <
	/// = 16) signed X/Y as an output.
	/// Each component is stored as an 8-bit or 16-bit normalized integer; stride must be equal to 4 or 8. W is preserved as is.
	/// Input data must contain 4 floats for every vector (count*4 total).
	/// meshopt_encodeFilterQuat encodes unit quaternions with K-bit (4 
	/// <
	/// = K 
	/// <
	/// = 16) component encoding.
	/// Each component is stored as an 16-bit integer; stride must be equal to 8.
	/// Input data must contain 4 floats for every quaternion (count*4 total).
	/// meshopt_encodeFilterExp encodes arbitrary (finite) floating-point data with 8-bit exponent and K-bit integer mantissa (1 
	/// <
	/// = K 
	/// <
	/// = 24).
	/// Exponent can be shared between all components of a given vector as defined by stride or all values of a given component; stride must be divisible by 4.
	/// Input data must contain stride/4 floats for every vector (count*stride/4 total).
	/// </summary>
	public enum meshopt_EncodeExpMode
	{

		/// <summary>
		/// When encoding exponents, use separate values for each component (maximum quality) 
		/// </summary>
		meshopt_EncodeExpSeparate = 0,

		/// <summary>
		/// When encoding exponents, use shared value for all components of each vector (better compression) 
		/// </summary>
		meshopt_EncodeExpSharedVector = 1,

		/// <summary>
		/// When encoding exponents, use shared value for each component of all vectors (best compression) 
		/// </summary>
		meshopt_EncodeExpSharedComponent = 2,

		/// <summary>
		/// When encoding exponents, use separate values for each component, but clamp to 0 (good quality if very small values are not important) 
		/// </summary>
		meshopt_EncodeExpClamped = 3,
	}

}
