using System;
using System.Collections.Generic;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using System.Drawing;

namespace Terrain {
	public class VBO {
		public PrimitiveType PrimitiveType { get; set; }

		int arrayObject = -1;
		int indexObject = -1;
		int colorObject = -1;
		int colorVectorThreeObject = -1;
		int normalObject = -1;
		int texcoordsObject = -1;
		int numElements = 0;
		public int TextureId { get; set; }

		public VBO() {
			PrimitiveType = PrimitiveType.Triangles;
			TextureId = -1;
		}

		public void SetTexcoords(List<Vector2> texcoordsList) {
			Vector2[] texcoords = new Vector2[texcoordsList.Count];
			for (int x = 0; x < texcoordsList.Count; x++) {
				texcoords[x] = texcoordsList[x];
			}
			GL.GenBuffers(1, out texcoordsObject);
			GL.BindBuffer(BufferTarget.ArrayBuffer, texcoordsObject);
			GL.BufferData(
				BufferTarget.ArrayBuffer, 
				(IntPtr)(texcoords.Length * Vector2.SizeInBytes), 
				texcoords, 
				BufferUsageHint.StaticDraw
			);
		}

		public void SetNormals(List<Vector3> normalsList) {
			Vector3[] normals = new Vector3[normalsList.Count];
			for (int x = 0; x < normalsList.Count; x++) {
				normals[x] = normalsList[x];
			}
			GL.GenBuffers(1, out normalObject);
			GL.BindBuffer(BufferTarget.ArrayBuffer, normalObject);
			GL.BufferData(
				BufferTarget.ArrayBuffer, 
				(IntPtr)(normals.Length * Vector3.SizeInBytes), 
				normals, 
				BufferUsageHint.StaticDraw
			);
		}

		public void SetVerticies(List<Vector3> verticesList, int attrib = -1) {
			int arrayId;
			GL.GenVertexArrays(1, out arrayId);
			GL.BindVertexArray(arrayId);

			Vector3[] vertices = new Vector3[verticesList.Count];
			for (int x = 0; x < verticesList.Count; x++) {
				vertices[x] = verticesList[x];
			}
			GL.GenBuffers(1, out arrayObject);
			GL.BindBuffer(BufferTarget.ArrayBuffer, arrayObject);

			GL.BufferData(
				BufferTarget.ArrayBuffer, 
				(IntPtr)(vertices.Length * 3 * sizeof(float)), 
				vertices, 
				BufferUsageHint.StaticDraw
			);

			if (attrib > -1) {
				GL.EnableVertexAttribArray(attrib);
				GL.VertexAttribPointer(attrib, vertices.Length * 3 * sizeof(float), VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);
			}
		}

		public void SetIndices(List<uint> indicesList) {
			uint[] indices = new uint[indicesList.Count];
			for (int x = 0; x < indicesList.Count; x++) {
				indices[x] = indicesList[x];
			}
			GL.GenBuffers(1, out indexObject);
			GL.BindBuffer(BufferTarget.ElementArrayBuffer, indexObject);
			GL.BufferData(
				BufferTarget.ElementArrayBuffer, 
				(IntPtr)(indices.Length * sizeof(uint)), 
				indices, 
				BufferUsageHint.StaticDraw
			);
			numElements = indices.Length;
		}

		public void SetColors(List<int> colorList, int attrib = -1) {
			int[] colors = new int[colorList.Count];
			for (int x = 0; x < colorList.Count; x++) {
				colors[x] = colorList[x];
			}
			GL.GenBuffers(1, out colorObject);
			GL.BindBuffer(BufferTarget.ArrayBuffer, colorObject);
			GL.BufferData(
				BufferTarget.ArrayBuffer, 
				(IntPtr)(colors.Length * sizeof(int)), 
				colors, 
				BufferUsageHint.StaticDraw
			);
			if (attrib > -1) {
				GL.EnableVertexAttribArray(attrib);
				GL.VertexAttribPointer(attrib, colors.Length * sizeof(int), VertexAttribPointerType.Byte, false, sizeof(int), 0);
			}
		}

		public void SetColors(List<Vector3> colorVectorList, int attrib = -1) {
			Vector3[] colors = new Vector3[colorVectorList.Count];
			for (int x = 0; x < colorVectorList.Count; x++) {
				colors[x] = colorVectorList[x];
			}
			GL.GenBuffers(1, out colorVectorThreeObject);
			GL.BindBuffer(BufferTarget.ArrayBuffer, colorVectorThreeObject);
			GL.BufferData(
				BufferTarget.ArrayBuffer, 
				(IntPtr)(colors.Length * Vector3.SizeInBytes), 
				colors, 
				BufferUsageHint.StaticDraw
			);
			if (attrib > -1) {
				GL.EnableVertexAttribArray(attrib);
				GL.VertexAttribPointer(attrib, colors.Length * 3 * sizeof(float), VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);
			}
		}

		public void Render() {
			if (normalObject != -1) {
				GL.EnableClientState(ArrayCap.NormalArray);
				GL.BindBuffer(BufferTarget.ArrayBuffer, normalObject);
				GL.NormalPointer(NormalPointerType.Float, Vector3.SizeInBytes, IntPtr.Zero);
			}

			if (colorObject != -1) {
				GL.EnableClientState(ArrayCap.ColorArray);
				GL.BindBuffer(BufferTarget.ArrayBuffer, colorObject);
				GL.ColorPointer(4, ColorPointerType.UnsignedByte, sizeof(int), IntPtr.Zero);
			}

			if (colorVectorThreeObject != -1) {
				GL.EnableClientState(ArrayCap.ColorArray);
				GL.BindBuffer(BufferTarget.ArrayBuffer, colorObject);
				GL.ColorPointer(3, ColorPointerType.Float, Vector3.SizeInBytes, IntPtr.Zero);
			}

			if (texcoordsObject != -1) {
				GL.BindBuffer(BufferTarget.ArrayBuffer, texcoordsObject);
				GL.TexCoordPointer(2, TexCoordPointerType.Float, Vector2.SizeInBytes, IntPtr.Zero);
				GL.EnableClientState(ArrayCap.TextureCoordArray);
				GL.BindTexture(TextureTarget.Texture2D, TextureId);
			}

			if (arrayObject != -1) {
				GL.BindBuffer(BufferTarget.ArrayBuffer, arrayObject);
				GL.VertexPointer(3, VertexPointerType.Float, Vector3.SizeInBytes, IntPtr.Zero);
				GL.EnableClientState(ArrayCap.VertexArray);
			}

			if (indexObject != -1) {
				GL.BindBuffer(BufferTarget.ElementArrayBuffer, indexObject);
				GL.DrawElements(PrimitiveType, numElements, DrawElementsType.UnsignedInt, IntPtr.Zero);
			}
		}
	}
}

