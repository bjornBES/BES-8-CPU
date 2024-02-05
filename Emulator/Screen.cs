using System;
using System.Diagnostics;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;
using OpenTK.Mathematics;
using System.Collections.Generic;

namespace emu
{
    public class Screen : GameWindow
    {
        /*
        bin databus IXXXXXXXRRRRAAAA
        0x8000/I = instr enable
        0x00F0/R = args mode
        0x000F/A = instr
        */

        public static MEM MEM;

        const char Pixel = '█'; // 219 dec
        const char HalvPixel = '▄'; // 220 dec

        const int ScreenW = 192;
        const int ScreenH = 144;

        Color4[] Pallet =
        {
            new Color4( 0,  0,  0,  255),
            new Color4( 255,255,255,255)
        };

        public Screen(int width, int height, string title) : base(GameWindowSettings.Default, new NativeWindowSettings() { Size = (width, height), Title = title })
        {
        }

        public static void RESET()
        {
            using (Screen game = new Screen(ScreenW, ScreenH, "BES-8-CPU"))
            {
                game.Run();
            }
            //Console.SetWindowSize(192, 60);
            //CursorPosX = 0;
            //CursorPosY = 0;
        }

        private float[] _vertices;
        private List<float> vertices = new List<float>();

        // So we're going make the triangle pulsate between a color range.
        // In order to do that, we'll need a constantly changing value.
        // The stopwatch is perfect for this as it is constantly going up.
        private Stopwatch _timer;

        private int _vertexBufferObject;

        private int _vertexArrayObject;

        private Shader _shader;

        void SetPixel(int width, int height, int color)
        {
            float X = width / ScreenW;
            float Y = height / ScreenH;

            Color4 color4 = Pallet[color];

            vertices.Add(X);
            vertices.Add(Y);
            vertices.Add(0);

            vertices.Add(color4.R / 255);
            vertices.Add(color4.G / 255);
            vertices.Add(color4.B / 255);
        }
        void SetPixel(int width, int height, int Red, int Green, int Blue)
        {
            float X = width / ScreenW;
            float Y = height / ScreenH;

            Color4 color4 = new Color4(Red, Green, Blue, 255);

            vertices.Add(X);
            vertices.Add(Y);
            vertices.Add(0);

            vertices.Add(color4.R / 255);
            vertices.Add(color4.G / 255);
            vertices.Add(color4.B / 255);
        }
        void SetPixel(int width, int height, int Layer, int Red, int Green, int Blue)
        {
            float X = width / ScreenW;
            float Y = height / ScreenH;

            Color4 color4 = new Color4(Red, Green, Blue, 255);

            vertices.Add(X);
            vertices.Add(Y);
            vertices.Add(Layer);

            vertices.Add(color4.R / 255);
            vertices.Add(color4.G / 255);
            vertices.Add(color4.B / 255);
        }
        void SetPixel(float width, float height, int color)
        {
            Color4 color4 = Pallet[color];

            vertices.Add(width);
            vertices.Add(height);
            vertices.Add(0);

            vertices.Add(color4.R);
            vertices.Add(color4.G);
            vertices.Add(color4.B);
        }
        void SetPixel(float width, float height, float Red, float Green, float Blue)
        {
            Color4 color4 = new Color4(Red, Green, Blue, 1);

            vertices.Add(width);
            vertices.Add(height);
            vertices.Add(0);

            vertices.Add(color4.R);
            vertices.Add(color4.G);
            vertices.Add(color4.B);
        }
        void SetPixel(float width, float height, float Layer, float Red, float Green, float Blue)
        {
            Color4 color4 = new Color4(Red, Green, Blue, 1);

            vertices.Add(width);
            vertices.Add(height);
            vertices.Add(Layer);

            vertices.Add(color4.R);
            vertices.Add(color4.G);
            vertices.Add(color4.B);
        }

        protected override void OnLoad()
        {
            base.OnLoad();

            /*
            VRAM
            XX-YY-CC-L0

            X = cursor pos X
            Y = cursor pos Y
            C = color Index
            S = Layer Index

             // positions        // colors
             0.5f, -0.5f, 0.0f,  1.0f, 0.0f, 0.0f,   // bottom right
            -0.5f, -0.5f, 0.0f,  0.0f, 1.0f, 0.0f,   // bottom left
             0.0f,  0.5f, 0.0f,  0.0f, 0.0f, 1.0f    // top 
             */
            //           X       Y      Z       R       G       B
            SetPixel(   000,    000,   000,    000,    000,    255);
            SetPixel(   001,    000,   000,    000,    000,    255);
            SetPixel(   002,    000,   000,    000,    000,    255);
            SetPixel(   003,    000,   000,    000,    000,    255);
            SetPixel(   004,    000,   000,    000,    000,    255);
            SetPixel(   005,    000,   000,    000,    000,    255);
            SetPixel(   006,    000,   000,    000,    000,    255);
            SetPixel(   007,    000,   000,    000,    000,    255);
            SetPixel(   008,    000,   000,    000,    000,    255);
            SetPixel(   009,    000,   000,    000,    000,    255);
            SetPixel(   010,    000,   000,    000,    000,    255);

            _vertices = new float[vertices.Count];

            Array.Copy(vertices.ToArray(), _vertices, vertices.Count);

            GL.ClearColor(1.0f, 0.0f, 0.0f, 1.0f);

            _vertexBufferObject = GL.GenBuffer();

            GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBufferObject);
            GL.BufferData(BufferTarget.ArrayBuffer, _vertices.Length * sizeof(float), _vertices, BufferUsageHint.StaticDraw);

            _vertexArrayObject = GL.GenVertexArray();
            GL.BindVertexArray(_vertexArrayObject);

            // Just like before, we create a pointer for the 3 position components of our vertices.
            // The only difference here is that we need to account for the 3 color values in the stride variable.
            // Therefore, the stride contains the size of 6 floats instead of 3.
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 6 * sizeof(float), 0);
            GL.EnableVertexAttribArray(0);

            // We create a new pointer for the color values.
            // Much like the previous pointer, we assign 6 in the stride value.
            // We also need to correctly set the offset to get the color values.
            // The color data starts after the position data, so the offset is the size of 3 floats.
            GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, 6 * sizeof(float), 3 * sizeof(float));
            // We then enable color attribute (location=1) so it is available to the shader.
            GL.EnableVertexAttribArray(1);

            GL.GetInteger(GetPName.MaxVertexAttribs, out int maxAttributeCount);
            Debug.WriteLine($"Maximum number of vertex attributes supported: {maxAttributeCount}");

            _shader = new Shader("Shaders/shader.vert", "Shaders/shader.frag");
            _shader.Use();

            // We start the stopwatch here as this method is only called once.
            _timer = new Stopwatch();
            _timer.Start();
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);

            GL.Clear(ClearBufferMask.ColorBufferBit);

            _shader.Use();

            GL.BindVertexArray(_vertexArrayObject);

            GL.DrawArrays(PrimitiveType.Points, 0, _vertices.Length);

            SwapBuffers();
        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            base.OnUpdateFrame(e);

            var input = KeyboardState;

            if (input.IsKeyDown(Keys.Escape))
            {
                Close();
            }
        }

        protected override void OnResize(ResizeEventArgs e)
        {
            base.OnResize(e);

            GL.Viewport(0, 0, Size.X, Size.Y);
        }
    }
}
