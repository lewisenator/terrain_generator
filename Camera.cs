using System;
using System.Drawing;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Audio;
using OpenTK.Audio.OpenAL;
using OpenTK.Input;

namespace Terrain {
	public class Camera : IUpdateable {
		public Frustum Frustum { get; private set; }
		public float Pitch { get; set; }
		public float Facing { get; set; }
		public Vector3 Location { get { return location; } set {location = value; } }
		public Matrix4 Matrix { get { return matrix; } }

		private Matrix4 matrix;
		private Vector3 location;
		private Vector3 up = Vector3.UnitY;
		private MouseState previous;

		public static Camera Instance { 
			get { return instance == null ? (instance = new Camera()) : instance; } 
		}
		private static Camera instance = null;

		private Camera() {
			matrix = Matrix4.Identity;
			Pitch = 0f;
			Facing = MathHelper.PiOver6;
			location = new Vector3(0f, 10f, 0f);
		}

		public void LoadMatrix() {
			Matrix4 cameraMatrix = Matrix;
			GL.LoadMatrix(ref cameraMatrix);
		}

		public void Update() {
			if (!Game.Instance.Focused) return;
			KeyboardState Keyboard = OpenTK.Input.Keyboard.GetState();
			float speed = 5f;
			if (Keyboard[Key.W] || Keyboard[Key.Up]) {
				location.X += (float)Math.Cos(Facing) * speed;
				location.Z += (float)Math.Sin(Facing) * speed;
			}

			if (Keyboard[Key.S] || Keyboard[Key.Down]) {
				location.X -= (float)Math.Cos(Facing) * speed;
				location.Z -= (float)Math.Sin(Facing) * speed;
			}

			if (Keyboard[Key.A] || Keyboard[Key.Left]) {
				location.X -= (float)Math.Cos(Facing + Math.PI / 2) * speed;
				location.Z -= (float)Math.Sin(Facing + Math.PI / 2) * speed;
			}

			if (Keyboard[Key.D] || Keyboard[Key.Right]) {
				location.X += (float)Math.Cos(Facing + Math.PI / 2) * speed;
				location.Z += (float)Math.Sin(Facing + Math.PI / 2) * speed;
			}
			if (Keyboard[Key.R]) {
				location.Y += speed;
			}
			if (Keyboard[Key.F]) {
				location.Y -= speed;
			}
			if (Keyboard[Key.Q]) {
				Facing -= (float)Math.PI * 0.01f;
			}
			if (Keyboard[Key.E]) {
				Facing += (float)Math.PI * 0.01f;
			}

			float sensitivity = 0.0075f;
			int xdelta = 0, ydelta = 0, zdelta = 0;
			var current = OpenTK.Input.Mouse.GetState();
			//if (current[MouseButton.Middle] || current[MouseButton.Right] || current[MouseButton.Left]) {
			//if (current[MouseButton.Middle]) { // normal mouse right button
			if (current[MouseButton.Left]) { // track pad right button
			//if (false) {
				if (!current.Equals(previous)) {
					xdelta = current.X - previous.X;
					ydelta = current.Y - previous.Y;
					zdelta = current.Wheel - previous.Wheel;
					//Console.WriteLine("{0}, {1}, {2}", xdelta, ydelta, zdelta);
					Pitch -= ydelta * sensitivity;
					if (Pitch > Math.PI) Pitch = (float)Math.PI;
					if (Pitch < -Math.PI) Pitch = -(float)Math.PI;
					Facing += xdelta * sensitivity;
					if (Facing > Math.PI) Facing -= (float)(Math.PI * 2f);
					if (Facing < Math.PI) Facing += (float)(Math.PI * 2f);
					location.Y += zdelta * 0.5f;
					//Console.WriteLine("pitch: {0}, facing: {1}", Pitch, Facing);
				}
			}
			previous = current;

			Vector3 lookatPoint = new Vector3((float)Math.Cos(Facing), (float)Math.Sin(Pitch / 2), (float)Math.Sin(Facing));
			Vector3 target = Location + lookatPoint;
			matrix = Matrix4.LookAt(Location, target, up); // eye, target, up
			Frustum = null;

			Facing = Facing % (2f * (float)Math.PI);
			if (Facing > Math.PI * 2f) Facing -= (float)(Math.PI * 2f);
			if (Facing < 0) Facing += (float)(Math.PI * 2f);

			Frustum = new Frustum(Location, target, Facing);

//			Console.WriteLine("In frustum: {0}", Frustum.Contains(new Vector3(0f, 10f, 0f)));
//			Console.WriteLine("Camera Location: {0}", Location);
		}
	}
}