using System;
using System.Runtime.InteropServices;

namespace Evergine.Bindings.MeshOptimizer
{
	/// <summary>
	/// Vertex attribute stream
	/// Each element takes size bytes, beginning at data, with stride controlling the spacing between successive elements (stride >= size).
	/// </summary>
	[StructLayout(LayoutKind.Sequential)]
	public unsafe struct Stream
	{
		public void* Data;
		public nuint Size;
		public nuint Stride;
	}

	[StructLayout(LayoutKind.Sequential)]
	public unsafe struct VertexCacheStatistics
	{
		public uint VerticesTransformed;
		public uint WarpsExecuted;

		/// <summary>
		/// transformed vertices / triangle count; best case 0.5, worst case 3.0, optimum depends on topology 
		/// </summary>
		public float Acmr;

		/// <summary>
		/// transformed vertices / vertex count; best case 1.0, worst case 6.0, optimum is 1.0 (each vertex is transformed once) 
		/// </summary>
		public float Atvr;
	}

	[StructLayout(LayoutKind.Sequential)]
	public unsafe struct VertexFetchStatistics
	{
		public uint BytesFetched;

		/// <summary>
		/// fetched bytes / vertex buffer size; best case 1.0 (each byte is fetched once) 
		/// </summary>
		public float Overfetch;
	}

	[StructLayout(LayoutKind.Sequential)]
	public unsafe struct OverdrawStatistics
	{
		public uint PixelsCovered;
		public uint PixelsShaded;

		/// <summary>
		/// shaded pixels / covered pixels; best case 1.0 
		/// </summary>
		public float Overdraw;
	}

	[StructLayout(LayoutKind.Sequential)]
	public unsafe struct CoverageStatistics
	{
		public fixed float Coverage[3];

		/// <summary>
		/// viewport size in mesh coordinates 
		/// </summary>
		public float Extent;
	}

	/// <summary>
	/// Meshlet is a small mesh cluster (subset) that consists of:
	/// - triangles, an 8-bit micro triangle (index) buffer, that for each triangle specifies three local vertices to use;
	/// - vertices, a 32-bit vertex indirection buffer, that for each local vertex specifies which mesh vertex to fetch vertex attributes from.
	/// For efficiency, meshlet triangles and vertices are packed into two large arrays; this structure contains offsets and counts to access the data.
	/// </summary>
	[StructLayout(LayoutKind.Sequential)]
	public unsafe struct Meshlet
	{

		/// <summary>
		/// offsets within meshlet_vertices and meshlet_triangles arrays with meshlet data 
		/// </summary>
		public uint VertexOffset;
		public uint TriangleOffset;

		/// <summary>
		/// number of vertices and triangles used in the meshlet; data is stored in consecutive range [offset..offset+count) for vertices and [offset..offset+count*3) for triangles 
		/// </summary>
		public uint VertexCount;
		public uint TriangleCount;
	}

	[StructLayout(LayoutKind.Sequential)]
	public unsafe struct Bounds
	{

		/// <summary>
		/// bounding sphere, useful for frustum and occlusion culling 
		/// </summary>
		public fixed float Center[3];
		public float Radius;

		/// <summary>
		/// normal cone, useful for backface culling 
		/// </summary>
		public fixed float ConeApex[3];
		public fixed float ConeAxis[3];

		/// <summary>
		/// = cos(angle/2) 
		/// </summary>
		public float ConeCutoff;

		/// <summary>
		/// normal cone axis and cutoff, stored in 8-bit SNORM format; decode using x/127.0 
		/// </summary>
		public fixed byte ConeAxisS8[3];
		public byte ConeCutoffS8;
	}

}

