using System;
using System.Drawing;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Audio;
using OpenTK.Audio.OpenAL;
using OpenTK.Input;
using System.Drawing.Imaging;
using System.Diagnostics;
using System.Reflection;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using OpenTK.Platform;

namespace Terrain {

	public class Terrain : ILoadable, IRenderable {
		public int subdivisions = 5;
		public float gridSpacing = 5f;

		public int MapSize { get; set; }
		public float[,] RandomMap { get; set; }
		public float[,] Heightmap { get; set; }
		public Vector3 [,] NormalMap { get; set; }
		public int PageSideLength { get; set; }

		public float minHeight = float.MaxValue;
		public float maxHeight = float.MinValue;
		public float deltaHeight = 0;
		public int DARK_GRASS = 0;
		public int MUD = 1;
		public int LIGHT_GRASS = 2;
		public int ROCK = 3;
		public int SNOW = 4;
		public int NOISE = 5;
		public int SAND = 6;
		public int WATER = 7;

		private List<TerrainPage> pages = new List<TerrainPage>();
		public int[,] Texturemap { get; set; }


		public static Terrain Instance { 
			get { return instance == null ? (instance = new Terrain()) : instance; } 
		}
		private static Terrain instance = null;
		private Terrain() {
			PageSideLength = 0;
		}

		public void Load() {
			Util.Profile("BuildRandomMap", () => BuildRandomMap());
			Util.Profile("LoadTextures", () => LoadTextures());
			Util.Profile("BuildHeightMap", () => BuildHeightmap());
			Util.Profile("BuildNormalMap", () => BuildNormalmap());
			Util.Profile("BuildTextureMap", () => BuildTexturemap());
			Util.Profile("BuildTerrainPages", () => BuildTerrainPages());
		}

		private void LoadTextures() {
			DARK_GRASS = Util.LoadTexture("Terrain.Assets.DARK_GRASS_2.jpg");
			MUD = Util.LoadTexture("Terrain.Assets.MUD_2.jpg");
			LIGHT_GRASS = Util.LoadTexture("Terrain.Assets.LIGHT_GRASS.jpg");
			ROCK = Util.LoadTexture("Terrain.Assets.ROCK.jpg");
			SNOW = Util.LoadTexture("Terrain.Assets.SNOW.jpg");
			NOISE = Util.LoadTexture("Terrain.Assets.NOISE.jpg");
			SAND = Util.LoadTexture("Terrain.Assets.SAND.jpg");
			WATER = Util.LoadTexture("Terrain.Assets.WATER.jpg");
		}

		private void BuildRandomMap() {
			Bitmap bmp = new Bitmap(Assembly.GetEntryAssembly().GetManifestResourceStream("Terrain.Assets.NOISE.jpg"));
			RandomMap = new float[bmp.Width, bmp.Height];
			for (int x = 0; x < bmp.Width; x++) {
				for (int y = 0; y < bmp.Height; y++) {
					Color color = bmp.GetPixel(x, y);
					float merged = (color.R + color.G + color.B) / 765f;
					RandomMap[x, y] = merged;
				}
			}
		}

		private void BuildHeightmap() {
			//Bitmap bmp = new Bitmap(Assembly.GetEntryAssembly().GetManifestResourceStream("Terrain.Assets.HEIGHTMAP_2_1024.jpg"));
			////Bitmap bmp = new Bitmap(Assembly.GetEntryAssembly().GetManifestResourceStream("Terrain.Assets.HEIGHTMAP_3_1024.jpg"));
			Noise noise = new Noise();
			//Heightmap = new float[bmp.Width, bmp.Height];
			int width = 1024;
			int height = width;
			float halfWidth = width / 2f;
			Heightmap = new float[width, height];
			Vector2 center = new Vector2(Heightmap.GetLength(0) / 2f, Heightmap.GetLength(1) / 2f);
			for (int x = 0; x < Heightmap.GetLength(0); x++) {
				for (int y = 0; y < Heightmap.GetLength(1); y++) {
					float distance = (float)Math.Abs(Math.Sqrt(Math.Pow(center.X - x, 2) + Math.Pow(center.Y - y, 2)));
					float attenuation = 0.3f;
					float influence = 1f - (distance / (width / 2f)) * attenuation;
					//Color color = bmp.GetPixel(x % width, y % height);
					//Heightmap[x, y] = (color.R + color.B + color.G) * 1.2f * influence;
					int passes = 10;
					float detail = 40f;
					float scale = 20f;
					Heightmap[x, y] = noise.NoiseAt((x / (float)width * detail), (y / (float)height * detail), passes) * influence * scale + 8f * scale;
					//if (Heightmap[x, y] < minHeight) minHeight = Heightmap[x, y];
					//if (Heightmap[x, y] > maxHeight) maxHeight = Heightmap[x, y];

					float affect = -1f * (float)Math.Log(distance / halfWidth + .1f);
					Heightmap[x, y] *= affect;
					if (Heightmap[x, y] < minHeight) minHeight = Heightmap[x, y];
					if (Heightmap[x, y] > maxHeight) maxHeight = Heightmap[x, y];
				}
			}

			MapSize = Heightmap.GetLength(0);
			deltaHeight = maxHeight - minHeight;
		}

		private void BuildTexturemap() {
			Texturemap = new int[Heightmap.GetLength(0), Heightmap.GetLength(1)];
			float middleHeight = minHeight + (deltaHeight / 2f);
			//float snowHeight = minHeight + (deltaHeight * 8.5f / 10f);
			float sandHeight = minHeight + (deltaHeight * 2.1f / 10f);
			float waterHeight = minHeight + (deltaHeight * 2f / 10f);
			for (int x = 0; x < Texturemap.GetLength(0); x++) {
				for (int z = 0; z < Texturemap.GetLength(1); z++) {
					Vector3 normal = NormalAt(x, z);
					double angle = Math.Abs(Math.Acos(normal.Y / normal.Length));
					float avgHeight = HeightAt(x, z);

					float randomInfluence = 0.20f;
					bool low = avgHeight * (1f - randomInfluence) + avgHeight * randomInfluence * RandomAt(x, z) * 2f < middleHeight;
					randomInfluence = 0.25f;
					bool steep = angle * (1f - randomInfluence) + angle * randomInfluence * RandomAt(x, z) * 2f > MathHelper.PiOver4;
					randomInfluence = 0.10f;
					bool sand = avgHeight * (1f - randomInfluence) + avgHeight * randomInfluence * RandomAt(x, z) * 2f < sandHeight;
					randomInfluence = 0.001f;
					bool water = avgHeight * (1f - randomInfluence) + avgHeight * randomInfluence * RandomAt(x, z) * 2f < waterHeight;
					randomInfluence = 0.025f;
					bool snow = false;//avgHeight * (1f - randomInfluence) + avgHeight * randomInfluence * RandomAt(x, z) * 2f > snowHeight;

					if (snow) {
						Texturemap[x, z] = SNOW;
					} else if (water) {
						Texturemap[x, z] = WATER;
					} else if (sand) {
						Texturemap[x, z] = SAND;
					} else {
						if (low) {
							Texturemap[x, z] = steep ? MUD : DARK_GRASS;
						} else {
							Texturemap[x, z] = steep ? ROCK : LIGHT_GRASS;
						}
					}
				}
			}
		}

		private void BuildNormalmap() {
			NormalMap = new Vector3[Heightmap.GetLength(0), Heightmap.GetLength(1)];
			for (int x = 0; x < NormalMap.GetLength(0); x++) {
				for (int z = 0; z < NormalMap.GetLength(1); z++) {
					var v1 = HeightVectorAt(x, z);
					var v2 = HeightVectorAt(x - 1, z);
					var v3 = HeightVectorAt(x, z + 1);
					var v4 = HeightVectorAt(x + 1, z + 1);
					var v5 = HeightVectorAt(x + 1, z);
					var v6 = HeightVectorAt(x, z - 1);
					var v7 = HeightVectorAt(x - 1, z - 1);
					var normal = CalculateNormal(v1, v2, v3, v4, v5, v6, v7, v2, v3, v5, v6, v1, v1, v1, v1, v1);
					NormalMap[x, z] = normal;
				}
			}
		}

		private void BuildTerrainPages() {
			int pagesPerDimension = subdivisions;
			createTerrainPage(0, 0, Heightmap.GetLength(0), Heightmap.GetLength(1), pagesPerDimension);
			PageSideLength = (int)(Heightmap.GetLength(0) / Math.Sqrt(pages.Count));
			Util.Debug("Built {0} x {1} = {2} terrain pages", Math.Sqrt(pages.Count), Math.Sqrt(pages.Count), pages.Count);
			Util.Debug("Each page is {0} verts per side", PageSideLength);
		}

		private Vector3 CalculateNormal(params Vector3[] points) {
			Vector3 normal = new Vector3(0f, 1f, 0f);
			for (int n = 0; n < points.Length; n++) {
				Vector3 current = points[n];
				Vector3 next = points[(n + 1) % points.Length];
				normal.X = normal.X + ((current.Y - next.Y) * (current.Z + next.Z));
				normal.Y = normal.Y + ((current.Z - next.Z) * (current.X + next.X));
				normal.Z = normal.Z + ((current.X - next.X) * (current.Y + next.Y));
			}
			normal.Normalize();
			return normal;
		}

		private void createTerrainPage(int startX, int startZ, int width, int height, int pagesPerDimensions) {
			if (pagesPerDimensions > 1) { //recurse to subdivide further
				int newPagesPerDimension = pagesPerDimensions - 1;
				int newWidth = width / 2;
				int newHeight = height / 2;
				createTerrainPage(startX, 					 startZ, 						 newWidth, newHeight, newPagesPerDimension);
				createTerrainPage(startX + newWidth, startZ, 						 newWidth, newHeight, newPagesPerDimension);
				createTerrainPage(startX + newWidth, startZ + newHeight, newWidth, newHeight, newPagesPerDimension);
				createTerrainPage(startX, 					 startZ + newHeight, newWidth, newHeight, newPagesPerDimension);
			} else { //actually create page
				//Console.WriteLine("Creating terrain page: (x: {0}, z: {1}, width: {2}, height: {3})", startX, startZ, width, height);
				pages.Add(new TerrainPage(startX, startZ, width, height));
			}
		}

		public void Render() {
			GL.Color3(Color.White);
			GL.PushClientAttrib(ClientAttribMask.ClientVertexArrayBit);
			GL.Enable(EnableCap.CullFace);
			GL.Enable(EnableCap.DepthTest);
			GL.Enable(EnableCap.Lighting);
			GL.Enable(EnableCap.Light0);
			//GL.Enable(EnableCap.ColorMaterial); //necessary to use color array
			GL.LightModel(LightModelParameter.LightModelAmbient, new float[] { 0.2f, 0.2f, 0.2f, 1.0f });
			GL.Light(LightName.Light0, LightParameter.Position, new float[] { -1000, 800, 0 });
			GL.ColorMaterial(MaterialFace.Front, ColorMaterialParameter.AmbientAndDiffuse);
			GL.Enable(EnableCap.Texture2D);
//			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
//			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
//			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToBorder);
//			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToBorder);

			GL.Material(MaterialFace.Front, MaterialParameter.Diffuse, new float[] { 1, 1, 1, 1 });
			GL.Material(MaterialFace.Front, MaterialParameter.Ambient, new float[] { 0.5f, 0.5f, 0.5f, 1 });

//			GL.Material(MaterialFace.Front, MaterialParameter.Emission, new float[] { 0, 0, 0, 1 });
//			GL.Material(MaterialFace.Front, MaterialParameter.Specular, new float[] { 0, 0, 0, 1 });
//			GL.ColorMaterial(MaterialFace.FrontAndBack, ColorMaterialParameter.AmbientAndDiffuse);
//
//			GL.MatrixMode(MatrixMode.Modelview);
//			GL.PushMatrix();
//				GL.LoadIdentity();
//				GL.Rotate(-90, Vector3.UnitX);
//				GL.Light(LightName.Light0, LightParameter.Position, new float[] { -1f, -1f, -0f, 0f });
//			GL.PopMatrix();
//			GL.Light(LightName.Light0, LightParameter.Diffuse, new float[] { .8f, .8f, .8f, 1 });
//			GL.Light(LightName.Light0, LightParameter.Ambient, new float[] { .2f, .2f, .2f, 1 });


			GL.PolygonMode(MaterialFace.Front, PolygonMode.Fill);
			int pagesCount = 0;
			int triangles = 0;
			int totalTriangles = 0;
			int bytesOfMemory = 0;
			foreach (TerrainPage page in pages) {
				if (page.Visible()) {
					page.VBO.Render();
					triangles += page.Triangles;
					pagesCount++;
				}
				totalTriangles += page.Triangles;
				bytesOfMemory += page.TextureSize * page.TextureSize * 4;
			}
			HUD.Instance.Triangles = String.Format("Triangles: {0} of {1}", triangles, totalTriangles);
			HUD.Instance.Pages = String.Format("Pages: {0} of {1}", pagesCount, pages.Count);
			HUD.Instance.TextureMemory = String.Format("Texture memory: {0} MB", bytesOfMemory / 1024 / 1024);

			GL.Disable(EnableCap.CullFace);
			GL.PopClientAttrib();
		}

		public Vector3 NormalAt(int x, int y) {
			if (x >= NormalMap.GetLength(0)) x = NormalMap.GetLength(0) - 1;
			if (y >= NormalMap.GetLength(1)) y = NormalMap.GetLength(1) - 1;
			if (x < 0) x = 0;
			if (y < 0) y = 0;
			return NormalMap[x, y];
		}

		public Vector3 HeightVectorAt(int x, int z) {
			float y = HeightAt(x, z);
			return new Vector3(gridSpacing * x, y, gridSpacing * z);
		}

		public float RandomAt(int x, int y) {
			x = x % RandomMap.GetLength(0);
			y = y % RandomMap.GetLength(1);
			return RandomMap[x, y];
		}

		public float HeightAt(int x, int y) {
			if (x >= Heightmap.GetLength(0)) x = Heightmap.GetLength(0) - 1;
			if (y >= Heightmap.GetLength(1)) y = Heightmap.GetLength(1) - 1;
			if (x < 0) x = 0;
			if (y < 0) y = 0;
			return Heightmap[x, y];
		}

		public int TextureAt(int x, int y) {
			if (x >= Texturemap.GetLength(0)) x = Texturemap.GetLength(0) - 1;
			if (y >= Texturemap.GetLength(1)) y = Texturemap.GetLength(1) - 1;
			if (x < 0) x = 0;
			if (y < 0) y = 0;
			return Texturemap[x, y];
		}
	}
}

