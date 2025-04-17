using System.Numerics;
using System.Text;
using Veldrid;
using Veldrid.SPIRV;
using Veldrid.StartupUtilities;

namespace ShaderSama
{
    public class Renderer
    {
        public static Renderer Singleton { get; private set; }
        public GraphicsDevice GraphicsDeviceInstance { get; private set; }

        private ResourceFactory _factory;
        private CommandList _commandList;
        private DeviceBuffer _paramBuffer;
        private Shader[] _shaders;
        private Pipeline _pipeline;
        private ResourceLayout _resourceLayout;
        private ResourceSet _resourceSet;
        private float _time;

        public Renderer()
        {
            Singleton ??= this;

            GraphicsDeviceOptions options = new()
            {
                Debug = true,
                PreferStandardClipSpaceYDirection = true,
                PreferDepthRangeZeroToOne = true
            };
            GraphicsDeviceInstance = VeldridStartup.CreateGraphicsDevice(Window.Singleton.Base, options);
            _factory = GraphicsDeviceInstance.ResourceFactory;

            CreateResources();
        }

        private void CreateResources()
        {
            // Uniform buffer
            _paramBuffer = _factory.CreateBuffer(new BufferDescription(16, BufferUsage.UniformBuffer | BufferUsage.Dynamic));

            // Shader loading
            _shaders = _factory.CreateFromSpirv(
                new ShaderDescription(ShaderStages.Vertex, Encoding.UTF8.GetBytes(VertexCode), "main"),
                new ShaderDescription(ShaderStages.Fragment, Encoding.UTF8.GetBytes(Shaders.Basic), "main"));

            // Resource layout and set
            _resourceLayout = _factory.CreateResourceLayout(new ResourceLayoutDescription(
                new ResourceLayoutElementDescription("Params", ResourceKind.UniformBuffer, ShaderStages.Fragment)));

            _resourceSet = _factory.CreateResourceSet(new ResourceSetDescription(
                _resourceLayout, _paramBuffer));

            // Pipeline
            var pipelineDesc = new GraphicsPipelineDescription
            {
                BlendState = BlendStateDescription.SingleOverrideBlend,
                DepthStencilState = DepthStencilStateDescription.Disabled,
                RasterizerState = new RasterizerStateDescription(FaceCullMode.None, PolygonFillMode.Solid, FrontFace.Clockwise, true, false),
                PrimitiveTopology = PrimitiveTopology.TriangleStrip,
                ResourceLayouts = new[] { _resourceLayout },
                ShaderSet = new ShaderSetDescription(
                    vertexLayouts: Array.Empty<VertexLayoutDescription>(),
                    shaders: _shaders),
                Outputs = GraphicsDeviceInstance.SwapchainFramebuffer.OutputDescription
            };
            _pipeline = _factory.CreateGraphicsPipeline(pipelineDesc);
            _commandList = _factory.CreateCommandList();
        }

        public void Draw()
        {
            _time += Logic.Singleton.DeltaTime;

            Vector2 resolution = new Vector2(
                GraphicsDeviceInstance.SwapchainFramebuffer.Width,
                GraphicsDeviceInstance.SwapchainFramebuffer.Height);

            // Update uniform buffer
            GraphicsDeviceInstance.UpdateBuffer(_paramBuffer, 0, _time);
            GraphicsDeviceInstance.UpdateBuffer(_paramBuffer, 4, resolution);

            _commandList.Begin();
            _commandList.SetFramebuffer(GraphicsDeviceInstance.SwapchainFramebuffer);
            _commandList.ClearColorTarget(0, RgbaFloat.Black);

            _commandList.SetPipeline(_pipeline);
            _commandList.SetGraphicsResourceSet(0, _resourceSet);
            _commandList.Draw(4);

            _commandList.End();
            GraphicsDeviceInstance.SubmitCommands(_commandList);
            GraphicsDeviceInstance.SwapBuffers();
        }

        // Vertex shader (fullscreen triangle)
        private const string VertexCode = @"
        #version 450
        void main()
        {
            vec2 pos[4] = vec2[](
                vec2(-1.0, -1.0),
                vec2( 1.0, -1.0),
                vec2(-1.0,  1.0),
                vec2( 1.0,  1.0)
            );
            gl_Position = vec4(pos[gl_VertexIndex], 0.0, 1.0);
        }";
    }
}
