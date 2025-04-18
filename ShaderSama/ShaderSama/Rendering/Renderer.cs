using System.Drawing;
using System.Text;
using Veldrid;
using Veldrid.SPIRV;
using Veldrid.StartupUtilities;

namespace ShaderSama.Rendering
{
    public class Renderer
    {
        public static Renderer Singleton { get; private set; }
        public GraphicsDevice GraphicsDeviceInstance { get; private set; }
        public RgbaFloat ClearColor = RgbaFloat.Black;

        private ResourceFactory _factory;
        private CommandList _commandList;
        private DeviceBuffer _paramBuffer;
        private Shader[] _shaders;
        private Pipeline _pipeline;
        private ResourceLayout _resourceLayout;
        private ResourceSet _resourceSet;

        private event Action Resized;
        private bool _windowResized = false;

        public Renderer()
        {
            Singleton ??= this;

            GraphicsDeviceOptions options = new()
            {
                Debug = true,
                PreferStandardClipSpaceYDirection = true,
                PreferDepthRangeZeroToOne = true
            };
            GraphicsDeviceInstance = VeldridStartup.CreateGraphicsDevice(Window.Singleton.Base, options, GraphicsBackend.OpenGL);

            Window.Singleton.Base.Resized += () =>
            {
                _windowResized = true;
                Resized?.Invoke();
            };

            _factory = GraphicsDeviceInstance.ResourceFactory;

            CreateResources();
        }

        private void CreateResources()
        {
            _paramBuffer = _factory.CreateBuffer(new BufferDescription(16, BufferUsage.UniformBuffer | BufferUsage.Dynamic)); // Uniform buffer

            // Shader loading
            _shaders = _factory.CreateFromSpirv(
                new ShaderDescription(ShaderStages.Vertex, Encoding.UTF8.GetBytes(VertexCode), "main"),
                new ShaderDescription(ShaderStages.Fragment, Encoding.UTF8.GetBytes(Shaders.CyberFuji), "main"));

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
                ResourceLayouts = [_resourceLayout],
                ShaderSet = new ShaderSetDescription(
                    vertexLayouts: [],
                    shaders: _shaders),
                Outputs = GraphicsDeviceInstance.SwapchainFramebuffer.OutputDescription
            };
            _pipeline = _factory.CreateGraphicsPipeline(pipelineDesc);
            _commandList = _factory.CreateCommandList();
            GraphicsDeviceInstance.UpdateBuffer(_paramBuffer, 8, Window.Singleton.GetSize);
        }

        public void Draw()
        {
            ResizeGraphicsDeviceCheck();
            GraphicsDeviceInstance.UpdateBuffer(_paramBuffer, 0, Logic.Singleton.Time);

            _commandList.Begin();
            _commandList.SetFramebuffer(GraphicsDeviceInstance.SwapchainFramebuffer);
            _commandList.ClearColorTarget(0, ClearColor);

            _commandList.SetPipeline(_pipeline);
            _commandList.SetGraphicsResourceSet(0, _resourceSet);
            _commandList.Draw(4);

            _commandList.End();
            GraphicsDeviceInstance.SubmitCommands(_commandList);
            GraphicsDeviceInstance.SwapBuffers();
        }
        private void ResizeGraphicsDeviceCheck()
        {
            if (_windowResized)
            {
                _windowResized = false;
                var size = Window.Singleton.GetSize;
                GraphicsDeviceInstance.ResizeMainWindow((uint)size.X, (uint)size.Y);
                GraphicsDeviceInstance.UpdateBuffer(_paramBuffer, 8, size);
            }
        }

        // Vertex shader (fullscreen triangle)
        private const string VertexCode = @"
#version 450

vec2 positions[3] = vec2[](
    vec2(-1.0, -1.0),
    vec2( 3.0, -1.0),
    vec2(-1.0,  3.0)
);

void main() {
    gl_Position = vec4(positions[gl_VertexIndex], 0.0, 1.0);
}
    ";
    }
}
