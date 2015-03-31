using System;
using System.Collections.Generic;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using System.Drawing;

namespace Terrain {
	public class VBO2D {
		public PrimitiveType PrimitiveType { get; set; }

		public int ArrayObject = 0;
		public int IndexObject = 0;
		public int ColorObject = 0;
		public int TexcoordsObject = 0;

		int numElements = 0;

		public VBO2D() {
			PrimitiveType = PrimitiveType.Triangles;
		}

		public void SetTexcoords(List<Vector2> texcoordsList) {
			Vector2[] texcoords = new Vector2[texcoordsList.Count];
			for (int x = 0; x < texcoordsList.Count; x++) {
				texcoords[x] = texcoordsList[x];
			}
			GL.GenBuffers(1, out TexcoordsObject);
			GL.BindBuffer(BufferTarget.ArrayBuffer, TexcoordsObject);
			GL.BufferData(
				BufferTarget.ArrayBuffer, 
				(IntPtr)(texcoords.Length * Vector2.SizeInBytes), 
				texcoords, 
				BufferUsageHint.StaticDraw
			);
		}

		public void SetVerticies(List<Vector3> verticesList) {
			Vector3[] vertices = new Vector3[verticesList.Count];
			for (int x = 0; x < verticesList.Count; x++) {
				vertices[x] = verticesList[x];
			}
			GL.GenBuffers(1, out ArrayObject);
			GL.BindBuffer(BufferTarget.ArrayBuffer, ArrayObject);
			GL.BufferData(
				BufferTarget.ArrayBuffer, 
				(IntPtr)(vertices.Length * 3 * sizeof(float)), 
				vertices, 
				BufferUsageHint.StaticDraw
			);
		}

		public void SetIndices(List<uint> indicesList) {
			uint[] indices = new uint[indicesList.Count];
			for (int x = 0; x < indicesList.Count; x++) {
				indices[x] = indicesList[x];
			}
			GL.GenBuffers(1, out IndexObject);
			GL.BindBuffer(BufferTarget.ElementArrayBuffer, IndexObject);
			GL.BufferData(
				BufferTarget.ElementArrayBuffer, 
				(IntPtr)(indices.Length * sizeof(uint)), 
				indices, 
				BufferUsageHint.StaticDraw
			);
			numElements = indices.Length;
		}

		public void SetColors(List<int> colorList) {
			int[] colors = new int[colorList.Count];
			for (int x = 0; x < colorList.Count; x++) {
				colors[x] = colorList[x];
			}
			GL.GenBuffers(1, out ColorObject);
			GL.BindBuffer(BufferTarget.ArrayBuffer, ColorObject);
			GL.BufferData(
				BufferTarget.ArrayBuffer, 
				(IntPtr)(colors.Length * sizeof(int)), 
				colors, 
				BufferUsageHint.StaticDraw
			);
		}

		public void Render() {
			if (ColorObject != 0) {
				GL.EnableClientState(ArrayCap.ColorArray);
				GL.BindBuffer(BufferTarget.ArrayBuffer, ColorObject);
				GL.ColorPointer(4, ColorPointerType.UnsignedByte, sizeof(int), IntPtr.Zero);
			}

			if (TexcoordsObject != 0) {
				GL.BindBuffer(BufferTarget.ArrayBuffer, TexcoordsObject);
				GL.TexCoordPointer(2, TexCoordPointerType.Float, Vector2.SizeInBytes, IntPtr.Zero);
				GL.EnableClientState(ArrayCap.TextureCoordArray);
			}

			if (ArrayObject != 0) {
				GL.BindBuffer(BufferTarget.ArrayBuffer, ArrayObject);
				GL.VertexPointer(3, VertexPointerType.Float, Vector3.SizeInBytes, IntPtr.Zero);
				GL.EnableClientState(ArrayCap.VertexArray);
			}

			if (IndexObject != 0) {
				GL.BindBuffer(BufferTarget.ElementArrayBuffer, IndexObject);
				GL.DrawElements(PrimitiveType, numElements, DrawElementsType.UnsignedInt, IntPtr.Zero);
			}
		}

	}
}

