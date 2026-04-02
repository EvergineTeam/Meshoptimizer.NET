using System;

namespace Evergine.Bindings.MeshOptimizer
{
	/// <summary>
	/// Vertex buffer filter encoders
	/// These functions can be used to encode data in a format that meshopt_decodeFilter can decode
	/// meshopt_encodeFilterOct encodes unit vectors with K-bit (2 
	/// <
	/// = K 
	/// <
	/// = 16) signed X/Y as an output.
	/// Each component is stored as an 8-bit or 16-bit normalized integer; stride must be equal to 4 or 8. Z will store 1.0f, W is preserved as is.
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
	/// meshopt_encodeFilterColor encodes RGBA color data by converting RGB to YCoCg color space with K-bit (2 
	/// <
	/// = K 
	/// <
	/// = 16) component encoding; A is stored using K-1 bits.
	/// Each component is stored as an 8-bit or 16-bit integer; stride must be equal to 4 or 8.
	/// Input data must contain 4 floats for every color (count*4 total).
	/// </summary>
	public enum EncodeExpMode
	{

		/// <summary>
		/// When encoding exponents, use separate values for each component (maximum quality) 
		/// </summary>
		Separate = 0,

		/// <summary>
		/// When encoding exponents, use shared value for all components of each vector (better compression) 
		/// </summary>
		SharedVector = 1,

		/// <summary>
		/// When encoding exponents, use shared value for each component of all vectors (best compression) 
		/// </summary>
		SharedComponent = 2,

		/// <summary>
		/// When encoding exponents, use separate values for each component, but clamp to 0 (good quality if very small values are not important) 
		/// </summary>
		Clamped = 3,
	}

	/// <summary>
	/// Simplification options
	/// </summary>
	[Flags]
	public enum SimplifyOptions : uint
	{

		/// <summary>
		/// Do not move vertices that are located on the topological border (vertices on triangle edges that don't have a paired triangle). Useful for simplifying portions of the larger mesh. 
		/// </summary>
		LockBorder = 1,

		/// <summary>
		/// Improve simplification performance assuming input indices are a sparse subset of the mesh. Note that error becomes relative to subset extents. 
		/// </summary>
		Sparse = 2,

		/// <summary>
		/// Treat error limit and resulting error as absolute instead of relative to mesh extents. 
		/// </summary>
		ErrorAbsolute = 4,

		/// <summary>
		/// Remove disconnected parts of the mesh during simplification incrementally, regardless of the topological restrictions inside components. 
		/// </summary>
		Prune = 8,

		/// <summary>
		/// Produce more regular triangle sizes and shapes during simplification, at some cost to geometric and attribute quality. 
		/// </summary>
		Regularize = 16,

		/// <summary>
		/// Experimental: Allow collapses across attribute discontinuities, except for vertices that are tagged with meshopt_SimplifyVertex_Protect in vertex_lock. 
		/// </summary>
		Permissive = 32,
	}

	/// <summary>
	/// Experimental: Simplification vertex flags/locks, for use in `vertex_lock` arrays in simplification APIs
	/// </summary>
	[Flags]
	public enum SimplifyVertexOptions : uint
	{

		/// <summary>
		/// Do not move this vertex. 
		/// </summary>
		Lock = 1,

		/// <summary>
		/// Protect attribute discontinuity at this vertex; must be used together with meshopt_SimplifyPermissive option. 
		/// </summary>
		Protect = 2,
	}

}
