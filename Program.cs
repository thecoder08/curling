using OpenTK.Windowing.Desktop;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Graphics.OpenGL4;
using JeremyAnsel.Media.WavefrontObj;

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
    float[] colors;

    int elementBufferObject;
    int vertexBufferObject;
    int vertexArrayObject;
    int colorBufferObject;
    int colorArrayObject;

    public Object3D(string objFilePath, string vertShaderFile, string fragShaderFile, Vector3 position, Vector3 rotation) {
        ObjFile objFile = ObjFile.FromFile(objFilePath);
        ObjMaterial[] materials = new ObjMaterial[objFile.MaterialLibraries.Count];
        for (int i = 0; i < objFile.MaterialLibraries.Count; i++) {
            ObjMaterialFile mtlFile = ObjMaterialFile.FromFile(objFile.MaterialLibraries[i]);
            for (int j = 0; j < mtlFile.Materials.Count; j++) {
                materials[i * mtlFile.Materials.Count + j] = mtlFile.Materials[j];
            }
        }
        ObjMaterialFile 
        vertices = new float[objFile.Vertices.Count * 3];
        for (int i = 0; i < objFile.Vertices.Count; i++) {
            vertices[i * 3] = objFile.Vertices[i].Position.X;
            vertices[i * 3 + 1] = objFile.Vertices[i].Position.Y;
            vertices[i * 3 + 2] = objFile.Vertices[i].Position.Z;
        }

        indices = new uint[objFile.Faces.Count * 3];
        colors = new float[objFile.Faces.Count * 4];
        for (int i = 0; i < objFile.Faces.Count; i++) {
            for (int j = 0; j < objFile.Faces[i].Vertices.Count; j++) {
                indices[i * 3 + j] = (uint)objFile.Faces[i].Vertices[j].Vertex;
            }
            Console.WriteLine(objFile.Faces[i].MaterialName);

            ObjMaterial material = objFile.Materials.FirstOrDefault(m => m.Name == materialName);

            if (material != null)
            {
                // Retrieve the diffuse color
                Color diffuseColor = material.DiffuseColor;
                // Do something with the diffuse color
            }

            //colors[i * 4] = objFile.Faces[i].Material.DiffuseColor.R;
            //colors[i * 4 + 1] = objFile.Faces[i].Material.DiffuseColor.G;
            //colors[i * 4 + 2] = objFile.Faces[i].Material.DiffuseColor.B;
            //colors[i * 4 + 3] = objFile.Faces[i].Material.DiffuseColor.A;
        }

        vertexArrayObject = GL.GenVertexArray();
        GL.BindVertexArray(vertexArrayObject);

        vertexBufferObject = GL.GenBuffer();
        GL.BindBuffer(BufferTarget.ArrayBuffer, vertexBufferObject);
        GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.StaticDraw);

        colorArrayObject = GL.GenVertexArray();
        GL.BindVertexArray(colorArrayObject);

        colorBufferObject = GL.GenBuffer();
        GL.BindBuffer(BufferTarget.ArrayBuffer, colorBufferObject);
        GL.BufferData(BufferTarget.ArrayBuffer, colors.Length * sizeof(float), colors, BufferUsageHint.StaticDraw);

        elementBufferObject = GL.GenBuffer();
        GL.BindBuffer(BufferTarget.ElementArrayBuffer, elementBufferObject);
        GL.BufferData(BufferTarget.ElementArrayBuffer, objFile.Faces.Count * 3 * sizeof(uint), indices, BufferUsageHint.StaticDraw);

        shader = new Shader(vertShaderFile, fragShaderFile);
        shader.Use();

        int vertexLocation = shader.GetAttribLocation("aPosition");
        int vertexColor = shader.GetAttribLocation("aColor");
        
        GL.BindVertexArray(vertexArrayObject);
        GL.VertexAttribPointer(vertexLocation, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);
        GL.EnableVertexAttribArray(vertexLocation);

        GL.BindVertexArray(colorArrayObject);
        GL.VertexAttribPointer(vertexColor, 4, VertexAttribPointerType.Float, false, 4 * sizeof(float), 0);
        GL.EnableVertexAttribArray(vertexColor);
        
        this.position = position;
        this.rotation = rotation;
        updateMatrix();
    }

    public void updateMatrix() {
        modelMatrix = Matrix4.CreateTranslation(this.position) * Matrix4.CreateRotationX(rotation.X) * Matrix4.CreateRotationY(rotation.Y) * Matrix4.CreateRotationY(rotation.Z);
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
    private Object3D[] object3Ds = new Object3D[1];

    public Window(GameWindowSettings gameWindowSettings, NativeWindowSettings nativeWindowSettings) : base(gameWindowSettings, nativeWindowSettings) {}

    protected override void OnLoad() {
        base.OnLoad();

        GL.ClearColor(0.0f, 0.0f, 0.0f, 1.0f);
        GL.Enable(EnableCap.DepthTest);

        object3Ds[0] = new Object3D("models/rock-blue.obj", "shaders/shader.vert", "shaders/shader.frag", new Vector3(0, 0, 0), new Vector3(0, 0, 0));
        //object3Ds[1] = new Object3D("rock-red.obj", "shader.vert", "shader.frag", new Vector3(0, 0, 0), new Vector3(0, 0, 0));
    }

    protected override void OnRenderFrame(FrameEventArgs e) {
        base.OnRenderFrame(e);
        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
        for (int i = 0; i < object3Ds.Length; i++) {
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