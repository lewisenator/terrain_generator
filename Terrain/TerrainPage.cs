using System;
using OpenTK;
using System.Collections.Generic;
using OpenTK.Graphics.OpenGL;
using System.Drawing;

namespace Terrain {
	public class TerrainPage {
		public static int WHITE_RGBA = Color.FromArgb(255, 255, 255).ToRGBA();
		private static int instanceCount = 0;
		private int instanceNumber = 0;
		public int Triangles {get; set;}
		public VBO VBO { get; set; }
		public int X { get; set; }
		public int Z { get; set; }
		public int WIDTH { get; set; }
		public int HEIGHT {get; set;}

		private Terrain terrain;

		public int TextureSize = 512;

		public int ColorTexture = 0;
		public int FBOHandle = 0;


		public TerrainPage(int startX, int startZ, int width, int height) {
			instanceNumber = instanceCount++;
			terrain = Terrain.Instance;

			X = startX;
			Z = startZ;
			WIDTH = width;
			HEIGHT = height;

			if (instanceNumber == 0) Util.Profile("VBO", () => BuildVBO()); else BuildVBO();
			if (instanceNumber == 0) Util.Profile("FBO", () => GenTexture()); else GenTexture();
		}

		public void BuildVBO() {
			VBO vbo = new VBO(){
				PrimitiveType = PrimitiveType.Triangles,
				TextureId = terrain.DARK_GRASS
			};
			List<Vector3> verticesList = new List<Vector3>();
			List<uint> indicesList = new List<uint>();
			List<Vector3> normalsList = new List<Vector3>();
			List<Vector2> texcoordsList = new List<Vector2>();
			for (int x = X; x <= (X + WIDTH); x++) {
				for (int z = Z; z <= (Z + HEIGHT); z++) {
					verticesList.Add(terrain.HeightVectorAt(x, z));
					normalsList.Add(terrain.NormalAt(x, z));
					texcoordsList.Add(new Vector2((x - X) / (float)WIDTH, (z - Z) / (float)HEIGHT));
				}
			}

			int stride = WIDTH + 1;
			for (int x = 0; x < WIDTH; x++) {
				for (int z = 0; z < HEIGHT; z++) {
					indicesList.Add((uint)((x) * stride + (z + 1)));
					indicesList.Add((uint)((x + 1) * stride + (z + 1)));
					indicesList.Add((uint)((x) * stride + (z)));

					indicesList.Add((uint)((x + 1) * stride + (z)));
					indicesList.Add((uint)((x) * stride + (z)));
					indicesList.Add((uint)((x + 1) * stride + (z + 1)));
				}
			}
			vbo.SetVerticies(verticesList);
			vbo.SetIndices(indicesList);
			vbo.SetNormals(normalsList);
			vbo.SetTexcoords(texcoordsList);

			VBO = vbo;
			Triangles = indicesList.Count / 3;
		}

		public void GenTexture() {
			//Console.WriteLine("Generating texture from {0}x{1} FBO", TextureSize, TextureSize);
			GL.GenTextures(1, out ColorTexture);
			GL.BindTexture(TextureTarget.Texture2D, ColorTexture);
			GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba8, TextureSize, TextureSize, 0, PixelFormat.Rgba, PixelType.UnsignedByte, IntPtr.Zero);
			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.MirroredRepeat);
			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.MirroredRepeat);

			GL.Ext.GenFramebuffers(1, out FBOHandle);
			GL.Ext.BindFramebuffer(FramebufferTarget.FramebufferExt, FBOHandle);
			GL.Ext.FramebufferTexture2D(FramebufferTarget.FramebufferExt, FramebufferAttachment.ColorAttachment0Ext, TextureTarget.Texture2D, ColorTexture, 0);
			//GL.Ext.FramebufferTexture2D(FramebufferTarget.FramebufferExt, FramebufferAttachment.DepthAttachmentExt, TextureTarget.Texture2D, DepthTexture, 0);

			GL.Viewport(0, 0, TextureSize, TextureSize);
			GL.ClearColor(1f, 0f, 0f, 0f);
			GL.Clear(ClearBufferMask.ColorBufferBit);

			int[] textures = new int[]{
				terrain.DARK_GRASS,
				terrain.MUD,
				terrain.LIGHT_GRASS,
				terrain.ROCK,
				terrain.SNOW,
				terrain.SAND,
				terrain.WATER
			};

			GL.Enable(EnableCap.Texture2D);
			GL.Disable(EnableCap.Lighting);
			GL.Disable(EnableCap.DepthTest);
			GL.Enable(EnableCap.ColorMaterial);

			VBO2D shared = new VBO2D();
			List<Vector3> verticesList = new List<Vector3>();
			List<Vector2> texcoordsList = new List<Vector2>();
			for (int x = X; x <= (X + WIDTH); x++) {
				for (int z = Z; z <= (Z + HEIGHT); z++) {
					texcoordsList.Add(new Vector2(((x - X) / (float)WIDTH) * 1f, ((z - Z) / (float)HEIGHT) * 1f));	
					float fbox = (x - X) / (float)WIDTH * (float)TextureSize;
					float fboz = (z - Z) / (float)HEIGHT * (float)TextureSize;
					verticesList.Add(new Vector3(-1f + fbox / (float)TextureSize * 2f, -1f + fboz / (float)TextureSize * 2f, 0f));
				}
			}
			List<uint> indicesList = new List<uint>();
			int stride = WIDTH + 1;
			for (int x = 0; x < WIDTH; x++) {
				for (int z = 0; z < HEIGHT; z++) {
					indicesList.Add((uint)((x) * stride + (z + 1)));
					indicesList.Add((uint)((x + 1) * stride + (z + 1)));
					indicesList.Add((uint)((x) * stride + (z)));

					indicesList.Add((uint)((x + 1) * stride + (z)));
					indicesList.Add((uint)((x) * stride + (z)));
					indicesList.Add((uint)((x + 1) * stride + (z + 1)));
				}
			}
			shared.SetVerticies(verticesList);
			shared.SetTexcoords(texcoordsList);
			shared.SetIndices(indicesList);

			for (int i = 0; i < textures.Length; i++) {
				bool firstPass = i == 0;
				int currentTexture = textures[i];
				GL.BindTexture(TextureTarget.Texture2D, currentTexture);
				if (firstPass) GL.Disable(EnableCap.Blend);
				else {
					GL.Enable(EnableCap.Blend);
					GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);
				}

				List<int> colorList = new List<int>();
				byte alphaColor = firstPass ? (byte)255 : (byte)0;
				Color color = Color.White;
				int currentTextureColor = BitConverter.ToInt32(new byte[] { color.R, color.G, color.B, (byte)255 }, 0);
				int notCurrentTextureColor = BitConverter.ToInt32(new byte[] { color.R, color.G, color.B, alphaColor }, 0);
				for (int x = X; x <= (X + WIDTH); x++) {
					for (int z = Z; z <= (Z + HEIGHT); z++) {
						colorList.Add(terrain.TextureAt(x, z) == currentTexture ? currentTextureColor : notCurrentTextureColor);
					}
				}
				shared.SetColors(colorList);
				shared.Render();
			}

			GL.Ext.BindFramebuffer(FramebufferTarget.FramebufferExt, 0);
			VBO.TextureId = ColorTexture; 
		}

		public bool Visible() {	 
			return Camera.Instance.Frustum.Contains(
				new Vector3(X * terrain.gridSpacing, terrain.HeightAt(X, Z), Z * terrain.gridSpacing),
				new Vector3(X * terrain.gridSpacing + WIDTH, terrain.HeightAt(X + WIDTH, Z), Z * terrain.gridSpacing),
				new Vector3(X * terrain.gridSpacing, terrain.HeightAt(X, Z + HEIGHT), Z * terrain.gridSpacing + HEIGHT),
				new Vector3(X * terrain.gridSpacing + WIDTH, terrain.HeightAt(X + WIDTH, Z + HEIGHT), Z * terrain.gridSpacing + HEIGHT)
			);
		}
	}
}

