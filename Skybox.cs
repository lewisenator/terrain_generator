using System;
using OpenTK.Graphics.OpenGL;
using OpenTK;
using System.Collections.Generic;

namespace Terrain {
	public class Skybox : IRenderable, ILoadable, IUpdateable {

		private VBO vbo;
		private float angle;

		public Skybox() {
		}

		public void Update() {
			angle += 0.005f;
			angle %= 360f;
		}

		public void Load() {
			vbo = new VBO();
			vbo.TextureId = Util.LoadTexture("Terrain.Assets.SKYBOX.jpg");
			List<Vector3> verticesList = new List<Vector3>();
			List<uint> indicesList = new List<uint>();
			List<Vector2> texcoordsList = new List<Vector2>();

			//forward
			verticesList.Add(new Vector3(-1, -1, 1));
			verticesList.Add(new Vector3(-1, 1, 1));
			verticesList.Add(new Vector3(1, 1, 1));
			verticesList.Add(new Vector3(1, -1, 1));

			texcoordsList.Add(new Vector2(0.25f, 2f / 3f));
			texcoordsList.Add(new Vector2(0.25f, 1f / 3f));
			texcoordsList.Add(new Vector2(0.5f, 1f / 3f));
			texcoordsList.Add(new Vector2(0.5f, 2f / 3f));

			indicesList.Add(0);
			indicesList.Add(1);
			indicesList.Add(2);

			indicesList.Add(2);
			indicesList.Add(3);
			indicesList.Add(0);

			//top
			verticesList.Add(new Vector3(-1, 0.98f, 1));
			verticesList.Add(new Vector3(-1, 0.98f, -1));
			verticesList.Add(new Vector3(1, 0.98f, -1));
			verticesList.Add(new Vector3(1, 0.98f, 1));

			texcoordsList.Add(new Vector2(0.255f, 1f / 3f));
			texcoordsList.Add(new Vector2(0.255f, 0f / 3f));
			texcoordsList.Add(new Vector2(0.495f, 0f / 3f));
			texcoordsList.Add(new Vector2(0.495f, 1f / 3f));

			indicesList.Add(4);
			indicesList.Add(5);
			indicesList.Add(6);

			indicesList.Add(6);
			indicesList.Add(7);
			indicesList.Add(4);

			//left
			verticesList.Add(new Vector3(-1, -1, -1));
			verticesList.Add(new Vector3(-1, 1, -1));
			verticesList.Add(new Vector3(-1, 1, 1));
			verticesList.Add(new Vector3(-1, -1, 1));

			texcoordsList.Add(new Vector2(0.00f, 2f / 3f));
			texcoordsList.Add(new Vector2(0.00f, 1f / 3f));
			texcoordsList.Add(new Vector2(0.25f, 1f / 3f));
			texcoordsList.Add(new Vector2(0.25f, 2f / 3f));

			indicesList.Add(8);
			indicesList.Add(9);
			indicesList.Add(10);

			indicesList.Add(10);
			indicesList.Add(11);
			indicesList.Add(8);

			//right
			verticesList.Add(new Vector3(1, -1, 1));
			verticesList.Add(new Vector3(1, 1, 1));
			verticesList.Add(new Vector3(1, 1, -1));
			verticesList.Add(new Vector3(1, -1, -1));

			texcoordsList.Add(new Vector2(0.50f, 2f / 3f));
			texcoordsList.Add(new Vector2(0.50f, 1f / 3f));
			texcoordsList.Add(new Vector2(0.75f, 1f / 3f));
			texcoordsList.Add(new Vector2(0.75f, 2f / 3f));

			indicesList.Add(12);
			indicesList.Add(13);
			indicesList.Add(14);

			indicesList.Add(14);
			indicesList.Add(15);
			indicesList.Add(12);

			//back
			verticesList.Add(new Vector3(1, -1, -1));
			verticesList.Add(new Vector3(1, 1, -1));
			verticesList.Add(new Vector3(-1, 1, -1));
			verticesList.Add(new Vector3(-1, -1, -1));

			texcoordsList.Add(new Vector2(0.75f, 2f / 3f));
			texcoordsList.Add(new Vector2(0.75f, 1f / 3f));
			texcoordsList.Add(new Vector2(1f, 1f / 3f));
			texcoordsList.Add(new Vector2(1f, 2f / 3f));

			indicesList.Add(16);
			indicesList.Add(17);
			indicesList.Add(18);

			indicesList.Add(18);
			indicesList.Add(19);
			indicesList.Add(16);

			vbo.SetIndices(indicesList);
			vbo.SetVerticies(verticesList);
			vbo.SetTexcoords(texcoordsList);
		}

		public void Render() {
			GL.Disable(EnableCap.Lighting);
			GL.Disable(EnableCap.CullFace);
			GL.Enable(EnableCap.Texture2D);
			GL.Disable(EnableCap.Fog);
			GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);
			GL.PushMatrix();
			float scale = 20000f;
			GL.Scale(new Vector3(scale, scale, scale));
			GL.Translate(new Vector3(0f, 0.2f, 0f));
			GL.Rotate(angle, new Vector3(0, 1, 0));

			vbo.Render();
			GL.PopMatrix();
			GL.Enable(EnableCap.Fog);
		}


	}
}

