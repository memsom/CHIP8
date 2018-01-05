using System;
using SDL2;
using Chip8.IO.Interface;

namespace Chip8.IO.SDL2
{
    public class Screen : IScreen
    {
        IntPtr window;

        public int W { get; set; }

        public int H { get; set; }

        public int PixelSize { get; set; }

        public int ScreenBPP { get; set; }

        public Screen()
        {

        }

        public void Init(int w, int h, int pixelsize, int screenBPP)
        {

            W = w;
            H = h;
            PixelSize = pixelsize;
            ScreenBPP = screenBPP;

            SDL.SDL_Init(SDL.SDL_INIT_VIDEO);
            window = SDL.SDL_CreateWindow("Chip8", 50, 50, W * PixelSize, H * PixelSize, SDL.SDL_WindowFlags.SDL_WINDOW_RESIZABLE | SDL.SDL_WindowFlags.SDL_WINDOW_INPUT_FOCUS);
        }

        public void Draw(byte[] graphics)
        {
            var surfacep = SDL.SDL_GetWindowSurface(window);
            SDL.SDL_LockSurface(surfacep);

            var point = new SDL.SDL_Rect
            {
                w = 10,
                h = 10
            };
            for (var i = 0; i < (H * PixelSize); i += 10)
            {
                for (var j = 0; j < (W * PixelSize); j += 10)
                {
                    //screen[j + i * surface.w] = graphics[(j / 10) + (i / 10) * 64] ? 0xFFFFFFFF : 0;
                    var value = graphics[(j / 10) + (i / 10) * 64] != 0 ? 0xFFFFFFFF : 0;
                    point.x = j;
                    point.y = i;
                    SDL.SDL_FillRect(surfacep, ref point, value); //surface.Fill(point, value);
                }
            }

            SDL.SDL_UnlockSurface(surfacep);
            SDL.SDL_UpdateWindowSurface(window);
            SDL.SDL_Delay(15);
        }

        public bool WaitPoll()
        {
            return SDL.SDL_PollEvent(out SDL.SDL_Event ev) == 1;
        }

        public Keys GetKeys()
        {
            unsafe
            {
                var keysp = SDL.SDL_GetKeyboardState(out int numkeys);
                var keys = (byte*)keysp;


                return new Keys
                {
                    KeyCount = numkeys,
                    R1K1 = keys[(byte)SDL.SDL_Scancode.SDL_SCANCODE_1] != 0,
                    R1K2 = keys[(byte)SDL.SDL_Scancode.SDL_SCANCODE_2] != 0,
                    R1K3 = keys[(byte)SDL.SDL_Scancode.SDL_SCANCODE_3] != 0,
                    R1K4 = keys[(byte)SDL.SDL_Scancode.SDL_SCANCODE_4] != 0,
                    R2K1 = keys[(byte)SDL.SDL_Scancode.SDL_SCANCODE_Q] != 0,
                    R2K2 = keys[(byte)SDL.SDL_Scancode.SDL_SCANCODE_W] != 0,
                    R2K3 = keys[(byte)SDL.SDL_Scancode.SDL_SCANCODE_E] != 0,
                    R2K4 = keys[(byte)SDL.SDL_Scancode.SDL_SCANCODE_R] != 0,
                    R3K1 = keys[(byte)SDL.SDL_Scancode.SDL_SCANCODE_A] != 0,
                    R3K2 = keys[(byte)SDL.SDL_Scancode.SDL_SCANCODE_S] != 0,
                    R3K3 = keys[(byte)SDL.SDL_Scancode.SDL_SCANCODE_D] != 0,
                    R3K4 = keys[(byte)SDL.SDL_Scancode.SDL_SCANCODE_F] != 0,
                    R4K1 = keys[(byte)SDL.SDL_Scancode.SDL_SCANCODE_Z] != 0,
                    R4K2 = keys[(byte)SDL.SDL_Scancode.SDL_SCANCODE_X] != 0,
                    R4K3 = keys[(byte)SDL.SDL_Scancode.SDL_SCANCODE_C] != 0,
                    R4K4 = keys[(byte)SDL.SDL_Scancode.SDL_SCANCODE_V] != 0,
                    EK1 = keys[(byte)SDL.SDL_Scancode.SDL_SCANCODE_0] != 0,
                    EK2 = keys[(byte)SDL.SDL_Scancode.SDL_SCANCODE_9] != 0,
                    EK3 = keys[(byte)SDL.SDL_Scancode.SDL_SCANCODE_8] != 0,
                    EK4 = keys[(byte)SDL.SDL_Scancode.SDL_SCANCODE_U] != 0,
                    EK5 = keys[(byte)SDL.SDL_Scancode.SDL_SCANCODE_ESCAPE] != 0,

                    KeyMap = new bool[]
                    {
                        keys[(byte)SDL.SDL_Scancode.SDL_SCANCODE_1] != 0,
                        keys[(byte)SDL.SDL_Scancode.SDL_SCANCODE_2] != 0,
                        keys[(byte)SDL.SDL_Scancode.SDL_SCANCODE_3] != 0,
                        keys[(byte)SDL.SDL_Scancode.SDL_SCANCODE_4] != 0,
                        keys[(byte)SDL.SDL_Scancode.SDL_SCANCODE_Q] != 0,
                        keys[(byte)SDL.SDL_Scancode.SDL_SCANCODE_W] != 0,
                        keys[(byte)SDL.SDL_Scancode.SDL_SCANCODE_E] != 0,
                        keys[(byte)SDL.SDL_Scancode.SDL_SCANCODE_R] != 0,
                        keys[(byte)SDL.SDL_Scancode.SDL_SCANCODE_A] != 0,
                        keys[(byte)SDL.SDL_Scancode.SDL_SCANCODE_S] != 0,
                        keys[(byte)SDL.SDL_Scancode.SDL_SCANCODE_D] != 0,
                        keys[(byte)SDL.SDL_Scancode.SDL_SCANCODE_F] != 0,
                        keys[(byte)SDL.SDL_Scancode.SDL_SCANCODE_Z] != 0,
                        keys[(byte)SDL.SDL_Scancode.SDL_SCANCODE_X] != 0,
                        keys[(byte)SDL.SDL_Scancode.SDL_SCANCODE_C] != 0,
                        keys[(byte)SDL.SDL_Scancode.SDL_SCANCODE_V] != 0
                    }
                };
            }
        }
    }
}
