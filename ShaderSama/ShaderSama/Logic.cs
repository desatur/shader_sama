using System.Diagnostics;
using ShaderSama.Rendering;

namespace ShaderSama
{
    public class Logic
    {
        public static Logic Singleton { get; private set; }
        public float DeltaTime { get; private set; }
        public float Time { get; private set; }
        private readonly Stopwatch _sw;

        public Logic()
        {
            Singleton ??= this;
            _sw = new Stopwatch();
        }

        public void Tick()
        {
            _sw.Start();
            long lastTime = _sw.ElapsedMilliseconds;

            while (Window.Singleton.Base.Exists)
            {
                long currentTime = _sw.ElapsedMilliseconds;
                DeltaTime = (currentTime - lastTime) / 1000f;
                Time += DeltaTime;
                lastTime = currentTime;
                var events = Window.Singleton.Base.PumpEvents();
                Renderer.Singleton.Draw();
            }
        }
    }
}
