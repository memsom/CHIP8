using Chip8.IO.Interface;
using System;
using System.IO;

/// <summary>
/// This core emulation is a C# translation of:
///     https://github.com/arnsa/Chip-8-Emulator
///     
/// With some parts of: (which to be honest is an uncredited C++ port of the 
/// other one.. totes. Code is way, way too similar.)
///     https://github.com/JamesGriffin/CHIP-8-Emulator
///  
/// A few issues were fixed by looking at this one (mainly the size of I was wrong... 
/// typo.. I could kick myself!!):
///     https://blog.dantup.com/2016/06/building-a-chip-8-interpreter-in-csharp/
///     https://github.com/DanTup/DaChip8
/// 
/// And a good read through this article:
///     http://www.multigesture.net/articles/how-to-write-an-emulator-chip-8-interpreter/
///     
/// Originally, like the original source, this was one monolithic file that included
/// the SDL1 code directly. But I've now split it out and made it work with both SDL1 
/// and SDL2. I guess, these could all be improved. The flicker is massive, but I 
/// think that was how the original CHIP8 was(?)
/// </summary>
namespace Chip8.Core
{
    public class Engine
    {
        const int memsize = 0x1000; //4096
        const int SCREEN_W = 640;
        const int SCREEN_H = 320;
        const int SCREEN_BPP = 32;
        const int W = 64;
        const int H = 32;

        bool shouldDraw = false;

        ushort opcode;
        byte[] memory = new byte[Engine.memsize];
        byte[] V = new byte[0x10];
        ushort I = 0;        //byte I = 0; //aaagh!!!
        ushort pc = 0;
        ushort sp = 0;
        byte delay_timer;
        byte sound_timer;
        ushort[] stack = new ushort[0x10];

        byte[] graphics = new byte[W * H];

        byte[] key = new byte[0x10];

        byte[] chip8_fontset =
        {
          0xF0, 0x90, 0x90, 0x90, 0xF0, // 0
          0x20, 0x60, 0x20, 0x20, 0x70, // 1
          0xF0, 0x10, 0xF0, 0x80, 0xF0, // 2
          0xF0, 0x10, 0xF0, 0x10, 0xF0, // 3
          0x90, 0x90, 0xF0, 0x10, 0x10, // 4
          0xF0, 0x80, 0xF0, 0x10, 0xF0, // 5
          0xF0, 0x80, 0xF0, 0x90, 0xF0, // 6
          0xF0, 0x10, 0x20, 0x40, 0x40, // 7
          0xF0, 0x90, 0xF0, 0x90, 0xF0, // 8
          0xF0, 0x90, 0xF0, 0x10, 0xF0, // 9
          0xF0, 0x90, 0xF0, 0x90, 0x90, // A
          0xE0, 0x90, 0xE0, 0x90, 0xE0, // B
          0xF0, 0x80, 0x80, 0x80, 0xF0, // C
          0xE0, 0x90, 0x90, 0x90, 0xE0, // D
          0xF0, 0x80, 0xF0, 0x80, 0xF0, // E
          0xF0, 0x80, 0xF0, 0x80, 0x80  // F
        };

        Random rand = new Random();

        IScreen screen;

        public Engine(IScreen screen)
        {
            this.screen = screen;
        }

        //0x000-0x1FF - Chip 8 interpreter (contains font set in emu)
        //0x050-0x0A0 - Used for the built in 4x5 pixel font set (0-F)
        //0x200-0xFFF - Program ROM and work RAM

        public void Start(string name)
        {
            //Console.WriteLine("Enter the name of the game: ");
            //var name = new string(Console.ReadLine().Take(100).ToArray());

            Prepare(name);
        }

       
        void Prepare(string name)
        {
            Initialize();
            Load(name);

            screen.Init(W, H, 10, SCREEN_BPP);

            while (true)
            {
                if (screen.WaitPoll())
                    continue;

                Execute();
                Draw();
                Prec(name);
            }
        }

        void Load(string name)
        {
            if (!File.Exists(name))
            {
                Console.WriteLine("Wrong game name!");
                Console.ReadKey();
                Environment.Exit(1);
            }

            var game = File.Open(name, FileMode.Open);
           
            for (int f = 0x200; f < memsize; f++)
            {
                memory[f] = (byte)game.ReadByte();
            }
        }

        void Initialize()
        {
            pc = 0x200;      // Set program counter to 0x200
            opcode = 0;      // Reset op code
            I = 0;           // Reset I
            sp = 0;          // Reset stack pointer

            // Clear the display
            for (int i = 0; i < 2048; ++i)
            {
                graphics[i] = 0;
            }

            // Clear the stack, keypad, and V registers
            for (int i = 0; i < 16; ++i)
            {
                stack[i] = 0;
                key[i] = 0;
                V[i] = 0;
            }

            // Clear memory
            for (int i = 0; i < 4096; ++i)
            {
                memory[i] = 0;
            }

            // Load font set into memory
            for (int i = 0; i < 80; ++i)
            {
                memory[i] = chip8_fontset[i];
            }

            // Reset timers
            delay_timer = 0;
            sound_timer = 0;

            rand = new Random(DateTime.Now.Millisecond);
        }

        void Draw()
        {
            if (shouldDraw)
            {
                shouldDraw = false;
                screen.Draw(graphics);
            }
        }

        void Timers()
        {
            if (delay_timer > 0)
            {
                delay_timer--;
            }
            if (sound_timer > 0)
                sound_timer--;
        }

        void Prec(string name)
        {
            var keys = screen.GetKeys(); 
            if (keys.EK5)
                Environment.Exit(1);

            if (keys.EK1)
                Prepare(name);

            if (keys.EK2)
                Start(name);
            if (keys.EK3)
            {
                while (true)
                {
                    if (screen.WaitPoll())
                    {
                        keys = screen.GetKeys();
                        if (keys.EK5)
                            Environment.Exit(1);
                        if (keys.EK4)
                            break;
                    }
                }
            }
        }


        void Execute()
        {
            for (var times = 0; times < 10; times++)
            {
                opcode = (ushort)(memory[pc] << 8 | memory[pc + 1]);
#if DEBUG_OPCODES
                Console.Write($"Executing {opcode:X} at {pc:X}, I:{I:X} SP:{sp:X}\n");
#endif
                switch (opcode & 0xF000)
                {
                    case 0x0000:
                        Execute0000();
                        break;

                    case 0x1000: // 1NNN: Jumps to address NNN
                        Execute1000();
                        break;

                    case 0x2000: // 2NNN: Calls subroutine at NNN
                        Execute2000();
                        break;

                    case 0x3000: // 3XNN: Skips the next instruction if VX equals NN
                        Execute3000();
                        break;

                    case 0x4000: // 4XNN: Skips the next instruction if VX doesn't equal NN
                        Execute4000();

                        break;

                    case 0x5000: // 5XY0: Skips the next instruction if VX equals VY
                        Execute5000();

                        break;

                    case 0x6000: // 6XNN: Sets VX to NN
                        Execute6000();
                        break;

                    case 0x7000: // 7XNN: Adds NN to VX
                        Execute7000();
                        break;

                    case 0x8000:
                        Execute8000();
                        break;

                    case 0x9000: // 9XY0: Skips the next instruction if VX doesn't equal VY
                        Execute9000();

                        break;

                    case 0xA000: // ANNN: Sets I to the address NNN
                        ExecuteA000();
                        break;

                    case 0xB000: // BNNN: Jumps to the address NNN plus V0
                        ExecuteB000();
                        break;

                    case 0xC000: // CXNN: Sets VX to a random number and NN
                        ExecuteC000();
                        break;

                    case 0xD000: // DXYN: Draws a sprite at coordinate (VX, VY) that has a width of 8 pixels and a height of N pixels
                        ExecuteD000();
                        break;

                    case 0xE000:
                        ExecuteE000();
                        break;

                    case 0xF000:
                        ExecuteF000();
                        break;

                    default:
                        Console.Write($"Wrong opcode: {opcode:X}\n");
                        Console.ReadKey();
                        break;

                }
                Timers();
            }
        }

        void ExecuteF000()
        {
            switch (opcode & 0x00FF)
            {
                case 0x0007: // FX07: Sets VX to the value of the delay timer
                    V[(opcode & 0x0F00) >> 8] = delay_timer;
                    pc += 2;
                    break;

                case 0x000A: // FX0A: A key press is awaited, and then stored in VX
                    var keys = screen.GetKeys();
                    for (var i = 0; i < 0x10; i++)
                        if (keys.KeyMap[i])
                        {
                            V[(opcode & 0x0F00) >> 8] = (byte)i;
                            pc += 2;
                        }
                    break;

                case 0x0015: // FX15: Sets the delay timer to VX
                    delay_timer = V[(opcode & 0x0F00) >> 8];
                    pc += 2;
                    break;

                case 0x0018: // FX18: Sets the sound timer to VX
                    sound_timer = V[(opcode & 0x0F00) >> 8];
                    pc += 2;
                    break;

                case 0x001E: // FX1E: Adds VX to I
                    I += V[(opcode & 0x0F00) >> 8];
                    pc += 2;
                    break;

                case 0x0029: // FX29: Sets I to the location of the sprite for the character in VX. Characters 0-F (in hexadecimal) are represented by a 4x5 font
                    I = (ushort)(V[(opcode & 0x0F00) >> 8] * 5);
                    pc += 2;
                    break;

                case 0x0033: // FX33: Stores the Binary-coded decimal representation of VX, with the most significant of three digits at the address in I, the middle digit at I plus 1, and the least significant digit at I plus 2
                    memory[I] = (byte)(V[(opcode & 0x0F00) >> 8] / 100);
                    memory[I + 1] = (byte)((V[(opcode & 0x0F00) >> 8] / 10) % 10);
                    memory[I + 2] = (byte)(V[(opcode & 0x0F00) >> 8] % 10);
                    pc += 2;
                    break;

                case 0x0055: // FX55: Stores V0 to VX in memory starting at address I
                    for (var i = 0; i <= ((opcode & 0x0F00) >> 8); i++)
                    {
                        memory[I + i] = V[i];
                    }

                    pc += 2;
                    break;

                case 0x0065: //FX65: Fills V0 to VX with values from memory starting at address I
                    for (var i = 0; i <= ((opcode & 0x0F00) >> 8); i++)
                    {
                        V[i] = memory[I + i];
                    }

                    pc += 2;
                    break;

                default:
                    Console.Write($"Wrong opcode: {opcode:X}\n");
                    Console.ReadKey();
                    break;
            }
        }

        void ExecuteE000()
        {
            switch (opcode & 0x000F)
            {
                case 0x000E: // EX9E: Skips the next instruction if the key stored in VX is pressed
                    var keysE = screen.GetKeys();
                    if (keysE.KeyMap[V[(opcode & 0x0F00) >> 8]])
                    {
                        pc += 4;
                    }
                    else
                    {
                        pc += 2;
                    }

                    break;

                case 0x0001: // EXA1: Skips the next instruction if the key stored in VX isn't pressed
                    var keys1 = screen.GetKeys();
                    if (!keys1.KeyMap[V[(opcode & 0x0F00) >> 8]])
                        pc += 4;
                    else
                        pc += 2;
                    break;

                default:
                    Console.Write($"Wrong opcode: {opcode:X}\n");
                    Console.ReadKey();
                    break;

            }
        }

        void ExecuteD000()
        {
            ushort x = V[(opcode & 0x0F00) >> 8];
            ushort y = V[(opcode & 0x00F0) >> 4];
            int height = opcode & 0x000F; // opcode & 0x000F;

            V[0xF] = 0;
            for (int yline = 0; yline < height; yline++)
            {
                ushort pixel = memory[I + yline];
                for (int xline = 0; xline < 8; xline++)
                {
                    if ((pixel & (0x80 >> xline)) != 0)
                    {
                        var index = (x + xline + ((y + yline) * 64));
                        if (index >= 0 && index < graphics.Length)
                        {
                            if (graphics[index] == 1)
                            {
                                V[0xF] = 1;
                            }

                            graphics[x + xline + ((y + yline) * 64)] ^= 1;
                            shouldDraw = true;
                        }                        
                    }
                }
            }

            pc += 2;
        }

        void ExecuteC000()
        {
            //V[(opcode & 0x0F00) >> 8] = (byte)(rand.Next() & (opcode & 0x00FF));
            //pc += 2;
            V[(opcode & 0x0F00) >> 8] = (byte)((rand.Next(0, 256) % (0xFF + 1)) & (opcode & 0x00FF));
            pc += 2;
        }

        void ExecuteB000()
        {
            pc = (ushort)((opcode & 0x0FFF) + V[0]);
        }

        void ExecuteA000()
        {
            I = (ushort)(opcode & 0x0FFF);
            pc += 2;
        }

        void Execute9000()
        {
            if (V[(opcode & 0x0F00) >> 8] != V[(opcode & 0x00F0) >> 4])
            {
                pc += 4;
            }
            else
            {
                pc += 2;
            }
        }

        void Execute8000()
        {
            switch (opcode & 0x000F)
            {
                case 0x0000: // 8XY0: Sets VX to the value of VY
                    V[(opcode & 0x0F00) >> 8] = V[(opcode & 0x00F0) >> 4];
                    pc += 2;
                    break;

                case 0x0001: // 8XY1: Sets VX to VX or VY
                    V[(opcode & 0x0F00) >> 8] = (byte)(V[(opcode & 0x0F00) >> 8] | V[(opcode & 0x00F0) >> 4]);
                    pc += 2;
                    break;

                case 0x0002: // 8XY2: Sets VX to VX and VY
                    V[(opcode & 0x0F00) >> 8] = (byte)(V[(opcode & 0x0F00) >> 8] & V[(opcode & 0x00F0) >> 4]);
                    pc += 2;
                    break;

                case 0x0003: // 8XY3: Sets VX to VX xor VY
                    V[(opcode & 0x0F00) >> 8] = (byte)(V[(opcode & 0x0F00) >> 8] ^ V[(opcode & 0x00F0) >> 4]);
                    pc += 2;
                    break;

                case 0x0004: // 8XY4: Adds VY to VX. VF is set to 1 when there's a carry, and to 0 when there isn't
                    //if (V[(opcode & 0x0F00) >> 8] + V[(opcode & 0x00F0) >> 4] < 256)
                    //{
                    //    V[0xF] &= 0;
                    //}
                    //else
                    //{
                    //    V[0xF] = 1;
                    //}

                    //V[(opcode & 0x0F00) >> 8] += V[(opcode & 0x00F0) >> 4];
                    //pc += 2;
                    //break;
                    V[(opcode & 0x0F00) >> 8] += V[(opcode & 0x00F0) >> 4];
                    if (V[(opcode & 0x00F0) >> 4] > (0xFF - V[(opcode & 0x0F00) >> 8]))
                    {
                        V[0xF] = 1; //carry
                    }
                    else
                    {
                        V[0xF] = 0;
                    }

                    pc += 2;
                    break;

                case 0x0005: // 8XY5: VY is subtracted from VX. VF is set to 0 when there's a borrow, and 1 when there isn't
                    //if (((int)V[(opcode & 0x0F00) >> 8] - (int)V[(opcode & 0x00F0) >> 4]) >= 0)
                    //    V[0xF] = 1;
                    //else
                    //    V[0xF] &= 0;

                    //V[(opcode & 0x0F00) >> 8] -= V[(opcode & 0x00F0) >> 4];
                    //pc += 2;
                    //break;
                    if (V[(opcode & 0x00F0) >> 4] > V[(opcode & 0x0F00) >> 8])
                    {
                        V[0xF] = 0; // there is a borrow
                    }
                    else
                    {
                        V[0xF] = 1;
                    }

                    V[(opcode & 0x0F00) >> 8] -= V[(opcode & 0x00F0) >> 4];
                    pc += 2;
                    break;

                case 0x0006: // 8XY6: Shifts VX right by one. VF is set to the value of the least significant bit of VX before the shift
                    //V[0xF] = (byte)(V[(opcode & 0x0F00) >> 8] & 7);
                    //V[(opcode & 0x0F00) >> 8] = (byte)(V[(opcode & 0x0F00) >> 8] >> 1);
                    //pc += 2;
                    //break;
                    V[0xF] = (byte)(V[(opcode & 0x0F00) >> 8] & 0x1);
                    V[(opcode & 0x0F00) >> 8] >>= 1;
                    pc += 2;
                    break;

                case 0x0007: // 8XY7: Sets VX to VY minus VX. VF is set to 0 when there's a borrow, and 1 when there isn't
                    //if (((int)V[(opcode & 0x0F00) >> 8] - (int)V[(opcode & 0x00F0) >> 4]) > 0)
                    //    V[0xF] = 1;
                    //else
                    //    V[0xF] &= 0;

                    //V[(opcode & 0x0F00) >> 8] = (byte)(V[(opcode & 0x00F0) >> 4] - V[(opcode & 0x0F00) >> 8]);
                    //pc += 2;
                    //break;
                    if (V[(opcode & 0x0F00) >> 8] > V[(opcode & 0x00F0) >> 4])	// VY-VX
                        V[0xF] = 0; // there is a borrow
                    else
                        V[0xF] = 1;
                    V[(opcode & 0x0F00) >> 8] = (byte)(V[(opcode & 0x00F0) >> 4] - V[(opcode & 0x0F00) >> 8]);
                    pc += 2;
                    break;

                case 0x000E: // 8XYE: Shifts VX left by one. VF is set to the value of the most significant bit of VX before the shift
                    V[0xF] = (byte)(V[(opcode & 0x0F00) >> 8] >> 7);
                    V[(opcode & 0x0F00) >> 8] = (byte)(V[(opcode & 0x0F00) >> 8] << 1);
                    pc += 2;
                    break;

                default:
                    Console.Write($"Wrong opcode: {opcode:X}\n");
                    Console.ReadKey();
                    break;

            }
        }

        void Execute7000()
        {
            V[(opcode & 0x0F00) >> 8] += (byte)(opcode & 0x00FF); //this seems a bit off
            pc += 2;
        }

        void Execute6000()
        {
            V[(opcode & 0x0F00) >> 8] = (byte)(opcode & 0x00FF); //this seems a bit off
            pc += 2;
        }

        void Execute5000()
        {
            if (V[(opcode & 0x0F00) >> 8] == V[(opcode & 0x00F0) >> 4])
            {
                pc += 4;
            }
            else
            {
                pc += 2;
            }
        }

        void Execute4000()
        {
            if (V[(opcode & 0x0F00) >> 8] != (opcode & 0x00FF))
            {
                pc += 4;
            }
            else
            {
                pc += 2;
            }
        }

        void Execute3000()
        {
            if (V[(opcode & 0x0F00) >> 8] == (opcode & 0x00FF))
            {
                pc += 4;
            }
            else
            {
                pc += 2;
            }
        }

        void Execute2000()
        {
            stack[sp] = pc;
            ++sp;
            pc = (ushort)(opcode & (ushort)0x0FFF);
        }

        void Execute1000()
        {
            pc = (ushort)(opcode & (ushort)0x0FFF);
        }

        void Execute0000()
        {
            switch (opcode & 0x000F)
            {
                case 0x0000: // 00E0: Clears the screen  
                    for (var i = 0; i < 2048; ++i)
                    {
                        graphics[i] = 0;
                    }
                    pc += 2;
                    break;

                case 0x000E: // 00EE: Returns from subroutine      
                    --sp;
                    pc = stack[sp];
                    pc += 2;
                    break;

                default:
                    Console.Write($"Wrong opcode: {opcode:X}\n");
                    Console.ReadKey();
                    break;

            }
        }
    }
}
