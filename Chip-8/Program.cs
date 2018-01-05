using Chip8.Core;

namespace Chip8.Demo
{
    class Program
    {
        static int Main(string[] args)
        {
            try
            {
                CreateEngine(args)?.Start(@"games\pong.c8");
            }
            catch (System.Exception ex)
            {
                System.Console.WriteLine(ex.Message);
            }
            

            return 0;
        }

        static Engine CreateEngine(string[] args)
        {
            if (args.Length > 0 && string.Compare(args[0], "-sdl1") == 0)
            {
                return EngineFactory.CreateSDL1();
            }
            else
            {
                return EngineFactory.CreateSDL2();
            }
        }
    }
}
