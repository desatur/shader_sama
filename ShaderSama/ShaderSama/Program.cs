namespace ShaderSama
{
    internal class Program
    {
        static void Main()
        {
            new Window(1920, 1080);
            new Renderer();
            new Logic();
            Console.WriteLine(Shaders.Basic);
            Logic.Singleton.Tick();
        }
    }
}