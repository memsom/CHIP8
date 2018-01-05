using System;
using SdlDotNet.Graphics;
using SdlDotNet;
using Tao.Sdl;
using System.Drawing;
using Chip8.IO.Interface;
using System.Runtime.InteropServices;

namespace Chip8.IO.SDL1
{

    public class Screen : IScreen
    {
        Surface surface;

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

            Sdl.SDL_Init(Sdl.SDL_INIT_VIDEO);
            surface = Video.SetVideoMode(W * PixelSize, H * PixelSize, ScreenBPP, true, false, false, true, true);
        }

        public void Draw(byte[] graphics)
        {
            //surface.Lock();
            Sdl.SDL_LockSurface(surface.Handle);
            var point = new Sdl.SDL_Rect
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
                    point.x = (short)j;
                    point.y = (short)i;
                    Sdl.SDL_FillRect(surface.Handle, ref point, value); //directly calling surface,Fill(...) here fails
                }
            }

            Sdl.SDL_UnlockSurface(surface.Handle); //surface.Unlock();
            surface.Update();
            Sdl.SDL_Delay(15);
        }

        //see:: http://jturner.tapetrade.net/docs/C_Sharp_SDL_Framebuffer.html

        //[DllImport("msvcrt.dll", EntryPoint = "memset", CallingConvention = CallingConvention.Cdecl, SetLastError = false)]
        //public static extern IntPtr MemSet(IntPtr dest, int c, int count);

        //public void Draw(byte[] graphics)
        //{
        //    unsafe
        //    {
        //        var surfacep = Sdl.SDL_GetVideoSurface();
        //        Sdl.SDL_LockSurface(surfacep);
        //        var surface = (Sdl.SDL_Surface)Marshal.PtrToStructure(surfacep, typeof(Sdl.SDL_Surface));
        //        var size = surface.w * surface.h * sizeof(UInt32);
        //        MemSet(surface.pixels, 0, size);
        //        int* screen = (int*)(surface.pixels);

        //        for (var i = 0; i < (H * PixelSize); i++)
        //            for (var j = 0; j < (W * PixelSize); j++)
        //            {
        //                var index = (j / 10) + (i / 10) * 64;
        //                int value = graphics[index] > 0 ? 0xFFFFFFF : 0;
        //                var sindex = j + i * surface.w;
        //                screen[sindex] = value;
        //            }

        //        Sdl.SDL_UnlockSurface(surfacep);
        //        Sdl.SDL_Flip(surfacep);
        //        Sdl.SDL_Delay(15);
        //    }
        //}

        public bool WaitPoll()
        {
            return Sdl.SDL_PollEvent(out Sdl.SDL_Event ev) == 1;
        }

        public Keys GetKeys()
        {
            var keys = Sdl.SDL_GetKeyState(out int numkeys);
            return new Keys
            {
                KeyCount = numkeys,
                R1K1 = keys[(byte)Sdl.SDLK_1] != 0,
                R1K2 = keys[(byte)Sdl.SDLK_2] != 0,
                R1K3 = keys[(byte)Sdl.SDLK_3] != 0,
                R1K4 = keys[(byte)Sdl.SDLK_4] != 0,
                R2K1 = keys[(byte)Sdl.SDLK_q] != 0,
                R2K2 = keys[(byte)Sdl.SDLK_w] != 0,
                R2K3 = keys[(byte)Sdl.SDLK_e] != 0,
                R2K4 = keys[(byte)Sdl.SDLK_r] != 0,
                R3K1 = keys[(byte)Sdl.SDLK_a] != 0,
                R3K2 = keys[(byte)Sdl.SDLK_s] != 0,
                R3K3 = keys[(byte)Sdl.SDLK_d] != 0,
                R3K4 = keys[(byte)Sdl.SDLK_f] != 0,
                R4K1 = keys[(byte)Sdl.SDLK_z] != 0,
                R4K2 = keys[(byte)Sdl.SDLK_x] != 0,
                R4K3 = keys[(byte)Sdl.SDLK_c] != 0,
                R4K4 = keys[(byte)Sdl.SDLK_v] != 0,
                EK1 = keys[(byte)Sdl.SDLK_0] != 0,
                EK2 = keys[(byte)Sdl.SDLK_9] != 0,
                EK3 = keys[(byte)Sdl.SDLK_8] != 0,
                EK4 = keys[(byte)Sdl.SDLK_u] != 0,
                EK5 = keys[(byte)Sdl.SDLK_ESCAPE] != 0,

                KeyMap = new bool[]
                {
                    keys[(byte)Sdl.SDLK_1] != 0,
                    keys[(byte)Sdl.SDLK_2] != 0,
                    keys[(byte)Sdl.SDLK_3] != 0,
                    keys[(byte)Sdl.SDLK_4] != 0,
                    keys[(byte)Sdl.SDLK_q] != 0,
                    keys[(byte)Sdl.SDLK_w] != 0,
                    keys[(byte)Sdl.SDLK_e] != 0,
                    keys[(byte)Sdl.SDLK_r] != 0,
                    keys[(byte)Sdl.SDLK_a] != 0,
                    keys[(byte)Sdl.SDLK_s] != 0,
                    keys[(byte)Sdl.SDLK_d] != 0,
                    keys[(byte)Sdl.SDLK_f] != 0,
                    keys[(byte)Sdl.SDLK_z] != 0,
                    keys[(byte)Sdl.SDLK_x] != 0,
                    keys[(byte)Sdl.SDLK_c] != 0,
                    keys[(byte)Sdl.SDLK_v] != 0
                }
            };
        }
    }
}
