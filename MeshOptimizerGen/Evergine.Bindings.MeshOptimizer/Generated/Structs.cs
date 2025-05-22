using System;
using System.Runtime.InteropServices;

namespace Evergine.Bindings.MeshOptimizer
{
	/// <summary>
	/// Vertex attribute stream
	/// Each element takes size bytes, beginning at data, with stride controlling the spacing between successive elements (stride >= size).
	/// </summary>
	[StructLayout(LayoutKind.Sequential)]
	public unsafe struct meshopt_Stream
	{
		public void* data;
		public UIntPtr size;
		public UIntPtr stride;
	}

	[StructLayout(LayoutKind.Sequential)]
	public unsafe struct meshopt_VertexCacheStatistics
	{
		public uint vertices_transformed;
		public uint warps_executed;

		/// <summary>
		/// transformed vertices / triangle count; best case 0.5, worst case 3.0, optimum depends on topology 
		/// </summary>
		public float acmr;

		/// <summary>
		/// transformed vertices / vertex count; best case 1.0, worst case 6.0, optimum is 1.0 (each vertex is transformed once) 
		/// </summary>
		public float atvr;
	}

	[StructLayout(LayoutKind.Sequential)]
	public unsafe struct meshopt_OverdrawStatistics
	{
		public uint pixels_covered;
		public uint pixels_shaded;

		/// <summary>
		/// shaded pixels / covered pixels; best case 1.0 
		/// </summary>
		public float overdraw;
	}

	[StructLayout(LayoutKind.Sequential)]
	public unsafe struct meshopt_VertexFetchStatistics
	{
		public uint bytes_fetched;

		/// <summary>
		/// fetched bytes / vertex buffer size; best case 1.0 (each byte is fetched once) 
		/// </summary>
		public float overfetch;
	}

	/// <summary>
	/// Meshlet is a small mesh cluster (subset) that consists of:
	/// - triangles, an 8-bit micro triangle (index) buffer, that for each triangle specifies three local vertices to use;
	/// - vertices, a 32-bit vertex indirection buffer, that for each local vertex specifies which mesh vertex to fetch vertex attributes from.
	/// For efficiency, meshlet triangles and vertices are packed into two large arrays; this structure contains offsets and counts to access the data.
	/// </summary>
	[StructLayout(LayoutKind.Sequential)]
	public unsafe struct meshopt_Meshlet
	{

		/// <summary>
		/// offsets within meshlet_vertices and meshlet_triangles arrays with meshlet data 
		/// </summary>
		public uint vertex_offset;
		public uint triangle_offset;

		/// <summary>
		/// number of vertices and triangles used in the meshlet; data is stored in consecutive range defined by offset and count 
		/// </summary>
		public uint vertex_count;
		public uint triangle_count;
	}

	[StructLayout(LayoutKind.Sequential)]
	public unsafe struct meshopt_Bounds
	{

		/// <summary>
		/// bounding sphere, useful for frustum and occlusion culling 
		/// </summary>
		public fixed float center[3];
		public float radius;

		/// <summary>
		/// normal cone, useful for backface culling 
		/// </summary>
		public fixed float cone_apex[3];
		public fixed float cone_axis[3];

		/// <summary>
		/// = cos(angle/2) 
		/// </summary>
		public float cone_cutoff;

		/// <summary>
		/// normal cone axis and cutoff, stored in 8-bit SNORM format; decode using x/127.0 
		/// </summary>
		public fixed byte cone_axis_s8[3];
		public byte cone_cutoff_s8;
	}

}

