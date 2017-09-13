using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SDL2;
using OpenGL;

namespace GraphicsTest
{
    class Program
    {
        public static string vertexShader2Source = @"
uniform mat4 projection_matrix;
uniform mat4 view_matrix;
uniform mat4 modelview_matrix;
uniform float time;

in vec3 in_position;
in vec2 in_uv;

out vec2 uv;

void main(void)
{
    uv = in_uv;
    gl_Position = projection_matrix * view_matrix * modelview_matrix * vec4(in_position, 1);
}";

        public static string fragmentShader2Source = @"
uniform sampler2D texture;
in vec2 uv;
out vec4 fragment;

void main(void)
{
  vec4 color = texture2D(texture, uv);  
  fragment = color;
}";

        public static int width = 400;
        public static int height = 400;
        static Matrix4 view_matrix = Matrix4.CreateTranslation(new Vector3(0, 0, -5));
        public static bool focused = false;

        public static TetrisGame game;

        static void Main(string[] args)
        {
            System.Threading.Thread t = new System.Threading.Thread(new System.Threading.ThreadStart(
                () => 
                { 
                    // Gamethread.
                    game = new TetrisGame(12, 14);
                    game.Reset();

                    while (true)
                    {
                        lock (game)
                        {
                            game.Tick();
                            TetrisConsoleView.DrawTetrisToConsole(game.Data);
                        }
                        System.Threading.Thread.Sleep(500);
                    }
                }
                ));
            t.Start();

            SDL.SDL_Init(SDL.SDL_INIT_EVERYTHING);
            IntPtr window = SDL.SDL_CreateWindow("tommi",
                SDL.SDL_WINDOWPOS_CENTERED,
                SDL.SDL_WINDOWPOS_CENTERED,
                400, 400, 
                SDL.SDL_WindowFlags.SDL_WINDOW_OPENGL
                | SDL.SDL_WindowFlags.SDL_WINDOW_RESIZABLE);

            IntPtr context = SDL.SDL_GL_CreateContext(window);
            //var renderer = SDL.SDL_CreateRenderer(window, -1, SDL.SDL_RendererFlags.SDL_RENDERER_ACCELERATED | SDL.SDL_RendererFlags.SDL_RENDERER_PRESENTVSYNC);

            var texture = new Texture("a.jpg");
            var texture2 = new Texture("b.png");

            ShaderProgram program = new ShaderProgram(vertexShader2Source, fragmentShader2Source);
            view_matrix = Matrix4.CreateTranslation(new Vector3(-5, 5, -5));
            program["view_matrix"].SetValue(view_matrix);
            //program["projection_matrix"].SetValue(Matrix4.CreatePerspectiveFieldOfView(0.45f, (float)width / height, 0.1f, 1000f));
            //program["projection_matrix"].SetValue(Matrix4.CreatePerspectiveFieldOfView(0.45f, (float)width / height, 0.1f, 1000f));
            program["projection_matrix"].SetValue(Matrix4.CreateOrthographic(20, 20, 0.1f, 1000f));
            program["modelview_matrix"].SetValue(Matrix4.CreateTranslation(Vector3.Zero));
            var time = 0.0f;
            program["time"].SetValue(time);
          
            var sprite = Geometry.CreateQuad(program, new Vector2(-0.5, -0.5), new Vector2(1, 1));            

            var watch = System.Diagnostics.Stopwatch.StartNew();
            bool run = true;
            while (run)
            {
                watch.Restart();
                SDL.SDL_Event e;
                while (SDL.SDL_PollEvent(out e) != 0)
                {
                    switch (e.type)
                    {
                        case SDL.SDL_EventType.SDL_QUIT:
                            run = false;
                            break;

                        case SDL.SDL_EventType.SDL_WINDOWEVENT:
                            switch (e.window.windowEvent)
                            {
                                case SDL.SDL_WindowEventID.SDL_WINDOWEVENT_FOCUS_GAINED:
                                    focused = true;
                                    break;

                                case SDL.SDL_WindowEventID.SDL_WINDOWEVENT_FOCUS_LOST:
                                    focused = false;
                                    break;

                                case SDL.SDL_WindowEventID.SDL_WINDOWEVENT_RESIZED:
                                    width = e.window.data1;
                                    height = e.window.data2;
                                    program["projection_matrix"].SetValue(Matrix4.CreatePerspectiveFieldOfView(0.45f, (float)width / height, 0.1f, 1000f));
                                    break;
                            }
                            break;

                        case SDL.SDL_EventType.SDL_MOUSEBUTTONDOWN:
                            SDL.SDL_CaptureMouse(SDL.SDL_bool.SDL_TRUE);
                            break;

                        case SDL.SDL_EventType.SDL_MOUSEBUTTONUP:
                            SDL.SDL_CaptureMouse(SDL.SDL_bool.SDL_FALSE);
                            break;

                        case SDL.SDL_EventType.SDL_MOUSEMOTION:
                            //program["modelview_matrix"].SetValue(Matrix4.CreateRotationY(e.button.x / 20.0f)
                            //    * Matrix4.CreateRotationX(e.button.y / 20.0f)
                            //    * Matrix4.CreateTranslation(new Vector3(0, 0, -2)) 
                            //    );
                            break;

                        case SDL.SDL_EventType.SDL_KEYDOWN:
                            switch (e.key.keysym.sym)
                            {
                                case SDL.SDL_Keycode.SDLK_UP:
                                    //view_matrix *= Matrix4.CreateTranslation(new Vector3(0, 0, 1));
                                    //program["view_matrix"].SetValue(view_matrix);

                                    lock (game)
                                    {
                                        game.Input(TetrisGame.EInputs.ROTATE_CW);
                                        TetrisConsoleView.DrawTetrisToConsole(game.Data);
                                    }

                                    break;
                                case SDL.SDL_Keycode.SDLK_DOWN:
                                    //view_matrix *= Matrix4.CreateTranslation(new Vector3(0, 0, -1));
                                    //program["view_matrix"].SetValue(view_matrix);

                                    lock (game)
                                    {
                                        game.Input(TetrisGame.EInputs.ACCELERATE);
                                        TetrisConsoleView.DrawTetrisToConsole(game.Data);
                                    }

                                    break;
                                case SDL.SDL_Keycode.SDLK_RIGHT:
                                    lock (game)
                                    {
                                        game.Input(TetrisGame.EInputs.SHIFT_RIGHT);
                                        TetrisConsoleView.DrawTetrisToConsole(game.Data);
                                    }
                                    break;
                                case SDL.SDL_Keycode.SDLK_LEFT:
                                    lock (game)
                                    {
                                        game.Input(TetrisGame.EInputs.SHIFT_LEFT);
                                        TetrisConsoleView.DrawTetrisToConsole(game.Data);
                                    }
                                    break;
                            }
                            break;
                    }
                }

                Gl.Viewport(0, 0, width, height);
                Gl.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

                //Gl.BindTexture(texture);
                //Gl.Enable(EnableCap.Blend);
                //Gl.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);

                //Gl.ClearColor(0, focused ? 1.0f : 0.6f, 0, 0);

                //sprite.Program.Use();
                //sprite.Draw();


                lock (game)
                {
                    var table = (Tetris.ESquare[,])game.Data.Table.Clone();
                    if (game.Data.CurrentBlock != null)
                    {
                        for (int j = 0; j < game.Data.CurrentBlock.Item.Height; ++j)
                            for (int i = 0; i < game.Data.CurrentBlock.Item.Width; ++i)
                            {
                                table[game.Data.CurrentBlock.Y + j, game.Data.CurrentBlock.X + i] = table[game.Data.CurrentBlock.Y + j, game.Data.CurrentBlock.X + i] == Tetris.ESquare.EMPTY
                                    ? game.Data.CurrentBlock.Item.Table[j, i]
                                    : Tetris.ESquare.FILLED;
                            }
                    }


                    for (int j = 0; j < game.Data.TableHeight; ++j)
                    {
                        for (int i = 0; i < game.Data.TableWidth; ++i)
                        {
                            var mvm = Matrix4.CreateTranslation(new Vector3(i, -j, 0));
                            sprite.Program["modelview_matrix"].SetValue(mvm);

                            if (table[j, i] == Tetris.ESquare.EMPTY)
                            {
                                Gl.BindTexture(texture2);
                            }
                            else
                            {
                                Gl.BindTexture(texture);
                            }

                            sprite.Program.Use();
                            sprite.Draw();
                        }                        
                    }

                }


                //cube.Program.Use();
                //cube.Draw();

                SDL.SDL_GL_SwapWindow(window);

                //time += 0.025f;
                program["time"].SetValue(time);

                watch.Stop();
                var fps = 60;
                var mspf = 1000 / fps;
                int timeToSleep = (int)(mspf - watch.ElapsedMilliseconds);
                if (timeToSleep > 0)
                    System.Threading.Thread.Sleep(timeToSleep);
            }

            program.Dispose();
            sprite.DisposeChildren = true;
            sprite.Dispose();
            texture.Dispose();
            texture2.Dispose();

            SDL.SDL_GL_DeleteContext(context);
            //SDL.SDL_RenderClear(renderer);
            SDL.SDL_DestroyWindow(window);
        }
    }
}
