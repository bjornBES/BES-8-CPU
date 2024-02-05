using System;
using System.Diagnostics;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;
using OpenTK.Mathematics;
using System.Collections.Generic;
using emu;

namespace emulator
{
    public class Engine : GameWindow
    {
        Color4[] Pallet =
        {
            new Color4( 0,  0,  0,  255),
            new Color4( 255,255,255,255)
        };

        const int ScreenW = 192;
        const int ScreenH = 144;

        static CPU CPU = new CPU();
        static MEM MEM = new MEM();

        public Engine(int width, int height, string title) : base(GameWindowSettings.Default, new NativeWindowSettings() { Size = (width, height), Title = title })
        {

        }
        public static void RESET(uint[] program)
        {
            //MEM.Reset(program);
            //CPU.RESET(ref MEM);
            using (Engine game = new Engine(192, 144, "BES-8-CPU"))
            {
                game.Run();
            }
        }

        private byte[] _vertices;
        private List<byte> vertices = new List<byte>();

        private int _vertexBufferObject;

        private int _vertexArrayObject;

        private Shader _shader;

        #region SetPixel
        void SetPixel(int width, int height, int color)
        {
            Color4 color4 = Pallet[color];

            vertices.Add((byte)width);
            vertices.Add((byte)height);
            vertices.Add(0);

            vertices.Add((byte)color4.R);
            vertices.Add((byte)color4.G);
            vertices.Add((byte)color4.B);
        }
        void SetPixel(int width, int height, int Red, int Green, int Blue)
        {
            Color4 color4 = new Color4(Red, Green, Blue, 255);

            vertices.Add((byte)width);
            vertices.Add((byte)height);
            vertices.Add(0);

            vertices.Add((byte)color4.R);
            vertices.Add((byte)color4.G);
            vertices.Add((byte)color4.B);
        }
        void SetPixel(int width, int height, int Layer, int Red, int Green, int Blue)
        {
            Color4 color4 = new Color4(Red, Green, Blue, 255);

            vertices.Add((byte)width);
            vertices.Add((byte)height);
            vertices.Add((byte)Layer);

            vertices.Add((byte)color4.R);
            vertices.Add((byte)color4.G);
            vertices.Add((byte)color4.B);
        }
        #endregion

        protected override void OnLoad()
        {
            base.OnLoad();

            /*
            for (int Y = 0; Y < ScreenH; Y++)
            {
                for (int X = 0; X < ScreenW; X++)
                {
                    SetPixel(X, Y, 0, 000, 000, 255);
                }
            }
            */
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

            SetPixel( 192, 000, 000, 000, 255, 000);
            SetPixel( 192, 144, 000, 000, 000, 255);
            SetPixel( 000, 000, 000, 255, 000, 000);

            _vertices = new byte[vertices.Count];

            Array.Copy(vertices.ToArray(), _vertices, vertices.Count);

            GL.ClearColor(0.2f, 0.3f, 0.3f, 1.0f);

            _vertexBufferObject = GL.GenBuffer();

            GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBufferObject);
            GL.BufferData(BufferTarget.ArrayBuffer, _vertices.Length * sizeof(byte), _vertices, BufferUsageHint.StaticDraw);

            _vertexArrayObject = GL.GenVertexArray();
            GL.BindVertexArray(_vertexArrayObject);

            // Just like before, we create a pointer for the 3 position components of our vertices.
            // The only difference here is that we need to account for the 3 color values in the stride variable.
            // Therefore, the stride contains the size of 6 floats instead of 3.
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Byte, false, 6 * sizeof(byte), 0);
            GL.EnableVertexAttribArray(0);

            // We create a new pointer for the color values.
            // Much like the previous pointer, we assign 6 in the stride value.
            // We also need to correctly set the offset to get the color values.
            // The color data starts after the position data, so the offset is the size of 3 floats.
            GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Byte, false, 6 * sizeof(byte), 3 * sizeof(byte));
            // We then enable color attribute (location=1) so it is available to the shader.
            GL.EnableVertexAttribArray(1);

            GL.GetInteger(GetPName.MaxVertexAttribs, out int maxAttributeCount);
            Debug.WriteLine($"Maximum number of vertex attributes supported: {maxAttributeCount}");

            _shader = new Shader("Shaders/shader.vert", "Shaders/shader.frag");
            _shader.Use();
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

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);

            GL.Clear(ClearBufferMask.ColorBufferBit);

            _shader.Use();

            GL.BindVertexArray(_vertexArrayObject);

            GL.DrawArrays(PrimitiveType.Triangles, 0, _vertices.Length);

            SwapBuffers();
        }

        protected override void OnResize(ResizeEventArgs e)
        {
            base.OnResize(e);

            GL.Viewport(0, 0, Size.X, Size.Y);
        }

    }
}
