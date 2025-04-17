using Veldrid.Sdl2;
using Veldrid.StartupUtilities;

namespace ShaderSama
{
    public class Window
    {
        public static Window Singleton { get; private set; }
        public Sdl2Window Base {  get; private set; }
        public Window(int width, int height)
        {
            Singleton ??= this;

            WindowCreateInfo windowCI = new()
            {
                X = 100,
                Y = 100,
                WindowWidth = width,
                WindowHeight = height,
                WindowTitle = "Shader Sama"
            };
            Base = VeldridStartup.CreateWindow(ref windowCI);
        }
    }
}
