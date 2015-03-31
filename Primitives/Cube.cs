using System;
using OpenTK.Graphics.OpenGL;
using System.Drawing;
using OpenTK;
using System.Collections.Generic;

namespace Terrain {
	public class Cube : IRenderable, ILoadable {
		public Vector3 Position {get; set;}
		private VBO vbo;

		public Cube() {
			Position = new Vector3(0f, 0f, 0f);
		}

		public void Load() {
			vbo = new VBO();
			vbo.PrimitiveType = PrimitiveType.Triangles;
			List<Vector3> verticesList = new List<Vector3>(){
				new Vector3(-0.5f,  0.5f,  0.5f), // vertex[0]
				new Vector3(0.5f,  0.5f,  0.5f), // vertex[1]
				new Vector3(0.5f, -0.5f,  0.5f), // vertex[2]
				new Vector3(-0.5f, -0.5f,  0.5f), // vertex[3]
				new Vector3(-0.5f,  0.5f, -0.5f), // vertex[4]
				new Vector3(0.5f,  0.5f, -0.5f), // vertex[5]
				new Vector3(0.5f, -0.5f, -0.5f), // vertex[6]
				new Vector3(-0.5f, -0.5f, -0.5f), // vertex[7]
			};
			List<uint> indicesList = new List<uint>(){
				1, 0, 2, // front
				3, 2, 0,
				6, 4, 5, // back
				4, 6, 7,
				4, 7, 0, // left
				7, 3, 0,
				1, 2, 5, //right
				2, 6, 5,
				0, 1, 5, // top
				0, 5, 4,
				2, 3, 6, // bottom
				3, 7, 6
			};
			List<int> colorList = new List<int>(){
				FromRGBA(1.0f, 0.0f, 0.0f, 1.0f).ToRGBA(),
				FromRGBA(0.0f, 1.0f, 0.0f, 1.0f).ToRGBA(),
				FromRGBA(0.0f, 0.0f, 1.0f, 1.0f).ToRGBA(),
				FromRGBA(0.0f, 1.0f, 1.0f, 1.0f).ToRGBA(),
				FromRGBA(1.0f, 0.0f, 0.0f, 1.0f).ToRGBA(),
				FromRGBA(0.0f, 1.0f, 0.0f, 1.0f).ToRGBA(),
				FromRGBA(0.0f, 0.0f, 1.0f, 1.0f).ToRGBA(),
				FromRGBA(0.0f, 1.0f, 1.0f, 1.0f).ToRGBA()
			};
			vbo.SetVerticies(verticesList);
			vbo.SetIndices(indicesList);
			vbo.SetColors(colorList);
		}

		public static Color FromRGBA(float red, float green, float blue, float alpha) {
			return Color.FromArgb(
				(int)(alpha * 255), 
				(int)(red * 255), 
				(int)(green * 255), 
				(int)(blue * 255));
		}

		public void Render() {
			GL.PushMatrix();
			Matrix4 pos = Matrix4.CreateTranslation(Position);
			GL.LoadMatrix(ref pos);
//			Matrix4 scale = Matrix4.CreateScale(20f);
//			GL.LoadMatrix(ref scale);
			GL.Color3(Color.White);
			GL.PushClientAttrib(ClientAttribMask.ClientVertexArrayBit);
			//GL.Enable(EnableCap.CullFace);
			GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);
			vbo.Render();

			//GL.Disable(EnableCap.CullFace);
			GL.PopClientAttrib();
			GL.PopMatrix();
		}
	}
}

