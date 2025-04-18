using ShaderSama.Rendering;

namespace ShaderSama
{
    internal class Program
    {
        static void Main()
        {
            _ = new Window(1920, 1080);
            _ = new Renderer();
            _ = new Logic();
            //Console.WriteLine(Shaders.Basic);
            Logic.Singleton.Tick();
        }
    }
}