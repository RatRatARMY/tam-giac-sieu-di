using Silk.NET.Core.Contexts;
using Silk.NET.Windowing;
using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using System.Drawing;
namespace Pro.A;

public class Program {
    private static IWindow _window;
    private static GL _gl;
    private static Random _random = new Random();
    private static uint _vao;
    private static uint _vbo;
    private static uint _ebo;
    private static uint _program;
    public static void Main(string[] args) {
        WindowOptions opt = WindowOptions.Default with {
            Size = new Vector2D<int>(800, 600),
            Title = "Hello world!",
            API = GraphicsAPI.Default with {
                API = ContextAPI.OpenGL,
                Version = new APIVersion(4, 6)
            }
        };
        _window = Window.Create(opt);
        _window.Load += OnLoad;
        _window.Update += OnUpdate;
        _window.Render += OnRender;
        _window.Run();
    }

    private static unsafe void OnLoad() {
        IInputContext input = _window.CreateInput();
        for (int i = 0; i < input.Keyboards.Count; i++) input.Keyboards[i].KeyDown += KeyDown;
        _gl = _window.CreateOpenGL();
        _vao = _gl.GenVertexArray();
        _gl.ClearColor(Color.FromArgb(_random.Next(256), _random.Next(256), _random.Next(256), _random.Next(256)));
        _gl.BindVertexArray(_vao);
        float[] vertices = [
            -0.981f, 0.998f, 0.0f,
            0.9f, -0.9f, 0.0f,
            0.1f, -0.89f, 0.0f
        ];
        _vbo = _gl.GenBuffer();
        _gl.BindBuffer(BufferTargetARB.ArrayBuffer, _vbo);
        fixed (float* buf = vertices)
            _gl.BufferData(BufferTargetARB.ArrayBuffer, (nuint)(vertices.Length * sizeof(float)), buf,
                BufferUsageARB.StaticDraw);
        uint[] indices = [
            0u, 1u, 2u
        ];
        _ebo = _gl.GenBuffer();
        _gl.BindBuffer(BufferTargetARB.ElementArrayBuffer, _ebo);
        fixed (uint* buf = indices)
            _gl.BufferData(BufferTargetARB.ElementArrayBuffer, (nuint)(indices.Length * sizeof(uint)), buf,
                BufferUsageARB.StaticDraw);
        const string vertexCode = @"
#version 460
layout (location = 0) in vec3 aPos;
void main() {
    gl_Position = vec4(aPos, 1.0);
}
";
        const string fragmentCode = @"
#version 460
out vec4 out_color;
void main() {
    out_color = vec4(1.0, 0.5, 0.2, 1.0);
}";
        uint vertexSrc = _gl.CreateShader(ShaderType.VertexShader);
        _gl.ShaderSource(vertexSrc, vertexCode);
        _gl.CompileShader(vertexSrc);
        _gl.GetShader(vertexSrc, ShaderParameterName.CompileStatus, out int vStatus);
        if (vStatus != (int)GLEnum.True) throw new Exception($"Vertex shader failed to compile: {_gl.GetShaderInfoLog(vertexSrc)}");
        uint fragmentSrc = _gl.CreateShader(ShaderType.FragmentShader);
        _gl.ShaderSource(fragmentSrc, fragmentCode);
        _gl.CompileShader(fragmentSrc);
        _gl.GetShader(fragmentSrc, ShaderParameterName.CompileStatus, out int fStatus);
        if (fStatus != (int)GLEnum.True) throw new Exception($"Fragment shader failed to compile: {_gl.GetShaderInfoLog(fragmentSrc)}");
        _program = _gl.CreateProgram();
        _gl.AttachShader(_program, vertexSrc);
        _gl.AttachShader(_program, fragmentSrc);
        _gl.LinkProgram(_program);
        _gl.GetProgram(_program, ProgramPropertyARB.LinkStatus, out int lStatus);
        if (lStatus != (int)GLEnum.True) throw new Exception($"Program link failed to link: {_gl.GetProgramInfoLog(_program)}");
        _gl.DetachShader(_program, vertexSrc);
        _gl.DetachShader(_program, fragmentSrc);
        _gl.DeleteShader(vertexSrc);
        _gl.DeleteShader(fragmentSrc);
        const uint positionLoc = 0;
        _gl.EnableVertexAttribArray(positionLoc);
        _gl.VertexAttribPointer(positionLoc, 3, VertexAttribPointerType.Float, false, 3*sizeof(float), (void*)0);
        _gl.BindVertexArray(0);
        _gl.BindBuffer(BufferTargetARB.ArrayBuffer, 0);
        _gl.BindBuffer(BufferTargetARB.ElementArrayBuffer, 0);
    }

    private static void KeyDown(IKeyboard keyboard, Key key, int keyCode) {
        if (key == Key.Escape) _window.Close();
    }

    private static void OnUpdate(double deltaTime) {
        
    }

    private static unsafe void OnRender(double deltaTime) {
        _gl.Clear(ClearBufferMask.ColorBufferBit);
        _gl.BindVertexArray(_vao);
        _gl.UseProgram(_program);
        _gl.DrawElements(PrimitiveType.Triangles, 3, DrawElementsType.UnsignedInt, (void*)0);
    }
}
