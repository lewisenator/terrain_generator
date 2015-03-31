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

namespace Terrain {
	public class HUD : ILoadable, IUpdateable, IRenderable, IResizeable {

		private Bitmap textBMP;
		private int textBMPID;
		readonly Font TextFont = new Font(FontFamily.GenericSansSerif, 11, FontStyle.Bold);
		private Stopwatch stopWatch = Stopwatch.StartNew();
		private double previousFrameElapsedTime = 0;
		private double fps = 60.0d;
		public int FPSWindow { get; set; }
		public string Triangles { get; set; }
		public string Pages { get; set; }
		public string TextureMemory { get; set; }
		public int Width { get; set; }
		public int Height { get; set; }

		public static HUD Instance { 
			get { return instance == null ? (instance = new HUD()) : instance; } 
		}
		private static HUD instance = null;

		private HUD() {
			FPSWindow = 10;
			Triangles = "0 Triangles";
			Pages = "0 Pages";
			TextureMemory = "0 MB";
			Width = 200;
			Height = 200;
		}

		public void Load() {
			CreateTexture();
		}

		public void Update() {
			UpdateTextureText();
		}

		public void Render() {
			DoRender();
		}

		public void Resize(int width, int height) {
			Width = width;
			Height = height;
			RecreateTexture();
		}

		int ticks = 0;
		private void Tick() {
			ticks++;
			double totalSeconds = stopWatch.Elapsed.TotalSeconds;
			double deltaSeconds = totalSeconds - previousFrameElapsedTime;
			previousFrameElapsedTime = totalSeconds;
			double currentFPS = 1d / deltaSeconds;
			fps = fps * (((double)FPSWindow - 1d) / (double)FPSWindow) + currentFPS * (1d / (double)FPSWindow);
			if (ticks > FPSWindow) {
				ticks = 0;
				FPSWindow = (int)(fps * 4f);
			}
		}

		private void CreateTexture() {
			textBMP = new Bitmap(Width, Height); // match window size
			textBMPID = GL.GenTexture();
			GL.BindTexture(TextureTarget.Texture2D, textBMPID);
			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)All.Nearest);
			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)All.Nearest);
			GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, textBMP.Width, textBMP.Height, 0,
				OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, IntPtr.Zero); // just allocate memory, so we can update efficiently using TexSubImage2D
			UpdateTextureText();
		}

		private void RecreateTexture() {
			if (textBMP == null) {
				CreateTexture();
				textBMP.Dispose();
			}
			textBMP = new Bitmap(Width, Height);
			GL.BindTexture(TextureTarget.Texture2D, textBMPID);
			GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, textBMP.Width, textBMP.Height, 0,
				OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, IntPtr.Zero); // just allocate memory, so we can update efficiently using TexSubImage2D
			UpdateTextureText();
		}

		private void DoRender() {
			if (textBMP == null) CreateTexture();
			Tick();

			GL.PushAttrib(AttribMask.LightingBit);
			GL.Disable(EnableCap.DepthTest);
			GL.Disable(EnableCap.Lighting);
			GL.Enable(EnableCap.Texture2D);
			GL.Enable(EnableCap.Blend);
			GL.BlendFunc(BlendingFactorSrc.One, BlendingFactorDest.OneMinusSrcAlpha);
			GL.PolygonMode(MaterialFace.Front, PolygonMode.Fill);
			GL.PolygonMode(MaterialFace.Back, PolygonMode.Fill);

			GL.MatrixMode(MatrixMode.Projection);
			GL.PushMatrix();
			GL.LoadIdentity();
			GL.Ortho(0, textBMP.Width, 0, textBMP.Height, -1, 1);
			GL.MatrixMode(MatrixMode.Modelview);
			GL.LoadIdentity();
			GL.Color3(Color.White);
			GL.BindTexture(TextureTarget.Texture2D, textBMPID);
			GL.Begin(PrimitiveType.Quads);
			GL.TexCoord2(0f, 1f); GL.Vertex2(0f, 0f);
			GL.TexCoord2(1f, 1f); GL.Vertex2(textBMP.Width, 0f);
			GL.TexCoord2(1f, 0f); GL.Vertex2(textBMP.Width, textBMP.Height);
			GL.TexCoord2(0f, 0f); GL.Vertex2(0f, textBMP.Height);
			GL.End();

			GL.MatrixMode(MatrixMode.Projection);
			GL.PopMatrix();

			GL.Disable(EnableCap.Texture2D);
			GL.Disable(EnableCap.Blend);
			GL.Enable(EnableCap.DepthTest);
			GL.PopAttrib();
		}

		private void UpdateTextureText() {
			int fpsInt = (int)(fps * 10);
			float fpsFloat = fpsInt / 10f;
			var text = String.Format("FPS: {0}", fpsFloat);
			using (Graphics gfx = Graphics.FromImage(textBMP)) {
				int line = 0;
				SolidBrush drawBrush = new SolidBrush(Color.White);
				gfx.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
				gfx.Clear(Color.Transparent);
				gfx.DrawString(text, TextFont, drawBrush, new PointF(0, line++ * TextFont.Height));
				gfx.DrawString(Triangles, TextFont, drawBrush, new PointF(0, line++ * TextFont.Height));
				gfx.DrawString(Pages, TextFont, drawBrush, new PointF(0, line++ * TextFont.Height));
				gfx.DrawString(TextureMemory, TextFont, drawBrush, new PointF(0, line++ * TextFont.Height));
			}
			BitmapData data = textBMP.LockBits(
				new Rectangle(0, 0, textBMP.Width, textBMP.Height),
				ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
			GL.TexSubImage2D(TextureTarget.Texture2D, 0, 0, 0, textBMP.Width,
				textBMP.Height, OpenTK.Graphics.OpenGL.PixelFormat.Bgra,
				PixelType.UnsignedByte, data.Scan0);
			textBMP.UnlockBits(data);
		}
	}
}