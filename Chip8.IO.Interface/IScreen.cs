namespace Chip8.IO.Interface
{
    public interface IScreen
    {        
        int W { get; }
        int H { get; }
        int PixelSize { get; }
        int ScreenBPP { get; }

        void Init(int w, int h, int pixelsize, int screenBPP);
        void Draw(byte[] graphics);

        bool WaitPoll();

        Keys GetKeys();
    }
}
