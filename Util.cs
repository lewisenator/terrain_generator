using System;
using System.Diagnostics;
using OpenTK.Graphics.OpenGL;
using System.Drawing;
using System.Drawing.Imaging;
using System.Reflection;

namespace Terrain {
	public static class Util {

		private static int depth = 0;
		public delegate void Profileable();
		public static void Profile(string task, Profileable p) {
			Stopwatch stopwatch = new Stopwatch();
			stopwatch.Start();
			Debug("Started " + task);
			depth++;
			p();
			stopwatch.Stop();
			depth--;
			Debug(string.Format("Finished {0} in {1} milliseconds", task, stopwatch.Elapsed.TotalMilliseconds));
		}

		public static void Debug(string message, params Object[] tokens) {
			message = string.Format(message, tokens);
			for (int i = 0; i < depth; i++) {
				message = "  " + message;
			}
			Console.WriteLine(message);
		}

		public static int LoadTexture(string path) {
			int textureId = 0;
			GL.Hint(HintTarget.PerspectiveCorrectionHint, HintMode.Nicest);
			GL.GenTextures(1, out textureId);
			GL.BindTexture(TextureTarget.Texture2D, textureId);
			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.MirroredRepeat);
			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.MirroredRepeat);
			Bitmap bitmap = new Bitmap(Assembly.GetEntryAssembly().GetManifestResourceStream(path));
			BitmapData data = bitmap.LockBits(
				new Rectangle(0, 0, bitmap.Width, bitmap.Height),
				ImageLockMode.ReadOnly, 
				System.Drawing.Imaging.PixelFormat.Format32bppArgb);
			GL.TexImage2D(
				TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, data.Width, data.Height, 0,
				OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, data.Scan0);
			bitmap.UnlockBits(data);
			return textureId;
		}

	}
}

