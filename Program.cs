using OpenTK.Windowing.Desktop;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Graphics.OpenGL4;
using ObjLoader.Loader.Loaders;
using ObjLoader.Loader.Data.VertexData;
using ObjLoader.Loader.Data.Elements;

NativeWindowSettings nativeWindowSettings = new NativeWindowSettings();
nativeWindowSettings.ClientSize = new Vector2i(640, 480);
nativeWindowSettings.Title = "Curling 3D";
nativeWindowSettings.Flags = ContextFlags.ForwardCompatible;
Window window = new Window(GameWindowSettings.Default, nativeWindowSettings);
window.Run();

class Object3D
{
    Matrix4 modelMatrix;
    public Vector3 position;
    public Vector3 rotation;
    Shader shader;

    float[] vertices;
    uint[] indices;

    int elementBufferObject;
    int vertexBufferObject;
    int vertexArrayObject;

    public Object3D(string objFile, string vertShaderFile, string fragShaderFile, Vector3 position, Vector3 rotation) {
        ObjLoaderFactory objLoaderFactory = new ObjLoaderFactory();
        IObjLoader objLoader = objLoaderFactory.Create();
        FileStream fileStream = new FileStream(objFile, FileMode.Open);
        LoadResult result = objLoader.Load(fileStream);
        Vertex[] loaderVertices = result.Vertices.ToArray();
        vertices = new float[loaderVertices.Length * 3];
        for (int i = 0; i < loaderVertices.Length; i++) {
            vertices[i * 3] = loaderVertices[i].X;
            vertices[i * 3 + 1] = loaderVertices[i].Y;
            vertices[i * 3 + 2] = loaderVertices[i].Z;
        }

        int indexCount = 0;

        for (int i = 0; i < result.Groups.Count; i++) {
            for (int j = 0; j < result.Groups[i].Faces.Count; j++) {
                for (int k = 0; k < result.Groups[i].Faces[j].Count; k++) {
                    indexCount++;
                }
            }
        }

        indices = new uint[indexCount];
        indexCount = 0;

        for (int i = 0; i < result.Groups.Count; i++) {
            for (int j = 0; j < result.Groups[i].Faces.Count; j++) {
                for (int k = 0; k < result.Groups[i].Faces[j].Count; k++) {
                    indices[indexCount] = Convert.ToUInt32(result.Groups[i].Faces[j][k].VertexIndex);
                    indexCount++;
                }
            }
        }

        vertexArrayObject = GL.GenVertexArray();
        GL.BindVertexArray(vertexArrayObject);

        vertexBufferObject = GL.GenBuffer();
        GL.BindBuffer(BufferTarget.ArrayBuffer, vertexBufferObject);
        GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.StaticDraw);

        elementBufferObject = GL.GenBuffer();
        GL.BindBuffer(BufferTarget.ElementArrayBuffer, elementBufferObject);
        GL.BufferData(BufferTarget.ElementArrayBuffer, indices.Length * sizeof(uint), vertices, BufferUsageHint.StaticDraw);

        shader = new Shader(vertShaderFile, fragShaderFile);
        shader.Use();

        int vertexLocation = shader.GetAttribLocation("aPosition");
        GL.VertexAttribPointer(vertexLocation, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);
        GL.EnableVertexAttribArray(vertexLocation);

        this.position = position;
        this.rotation = rotation;
        updateMatrix();
    }

    public void updateMatrix() {
        modelMatrix = Matrix4.CreateTranslation(this.position) * Matrix4.CreateRotationX(rotation.X) * Matrix4.CreateRotationX(rotation.Y) * Matrix4.CreateRotationX(rotation.Z);
    }

    public void render() {
        GL.BindVertexArray(vertexArrayObject);

        shader.Use();
        shader.SetMatrix4("model", modelMatrix);

        GL.DrawElements(PrimitiveType.Triangles, indices.Length, DrawElementsType.UnsignedInt, 0);
    }
}

class Camera
{
    Matrix4 cameraMatrix;
    Vector3 position;
    Vector3 rotation;
}

class Window : GameWindow
{
    private string[] objs = {"rock.obj"/*, "ice.obj"*/};
    private Object3D[] object3Ds = new Object3D[1];

    public Window(GameWindowSettings gameWindowSettings, NativeWindowSettings nativeWindowSettings) : base(gameWindowSettings, nativeWindowSettings) {}

    protected override void OnLoad() {
        base.OnLoad();

        GL.ClearColor(0.0f, 0.0f, 0.0f, 1.0f);
        GL.Enable(EnableCap.DepthTest);

        for (int i = 0; i < objs.Length; i++) {
            object3Ds[i] = new Object3D(objs[i], "shader.vert", "shader.frag", new Vector3(0, 0, 0), new Vector3(0, 0, 0));
        }
    }

    protected override void OnRenderFrame(FrameEventArgs e) {
        base.OnRenderFrame(e);
        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
        for (int i = 0; i < objs.Length; i++) {
            object3Ds[i].render();
        }
        SwapBuffers();
    }

    protected override void OnUpdateFrame(FrameEventArgs e) {
        base.OnUpdateFrame(e);
        object3Ds[0].rotation.X += 0.001f;
        object3Ds[0].updateMatrix();
    }

    protected override void OnResize(ResizeEventArgs e) {
        base.OnResize(e);
        
        GL.Viewport(0, 0, Size.X, Size.Y);
    }
}