using System;
using System.Drawing;

namespace Terrain {
	public static class ColorExtensions {
		public static int ToRGBA(this Color color) {
			byte[] bytes = new byte[] { color.R, color.G, color.B, color.A };
			return BitConverter.ToInt32(bytes, 0);
		}
	}
}

