using Chip8.Core;
using Chip8.IO.Interface;

namespace Chip8.Demo
{
    public class EngineFactory
    {
        static Engine GetEngine(IScreen screen)
        {
            return new Engine(screen);
        }

        public static Engine CreateSDL1()
        {
            return GetEngine(new IO.SDL1.Screen());
        }

        public static Engine CreateSDL2()
        {
            return GetEngine(new IO.SDL2.Screen());
        }
    }
}
