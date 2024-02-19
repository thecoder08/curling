using OpenTK.Mathematics;
using JeremyAnsel.Media.WavefrontObj;
using OpenTK.Graphics.OpenGL4;

class ObjectUI
{
    Matrix4 modelMatrix;
    public Vector3 position;
    public Vector3 rotation;
    public Vector3 scale;
    Shader shader;

    int vertexBufferObject;
    int vertexArrayObject;
    int[] elementBufferObjects;
    int[] numIndices;
    Vector3 color;

    public ObjectUI(string objFilePath, Vector3 color, Vector3 position, Vector3 rotation, Vector3 scale) {
        ObjFile objFile = ObjFile.FromFile(objFilePath);

        // Load vertices from .obj
        float[] vertices = new float[objFile.Vertices.Count * 3];
        for (int i = 0; i < objFile.Vertices.Count; i++) {
            vertices[i * 3] = objFile.Vertices[i].Position.X;
            vertices[i * 3 + 1] = objFile.Vertices[i].Position.Y;
            vertices[i * 3 + 2] = objFile.Vertices[i].Position.Z;
        }

        vertexArrayObject = GL.GenVertexArray();
        GL.BindVertexArray(vertexArrayObject);

        vertexBufferObject = GL.GenBuffer();
        GL.BindBuffer(BufferTarget.ArrayBuffer, vertexBufferObject);
        GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.StaticDraw);

        elementBufferObjects = new int[1];
        numIndices = new int[1];
        numIndices[0] = objFile.Faces.Count * 3;

        // Retrieve indices that use this material
        uint[] indices = new uint[numIndices[0]];
        for (int j = 0; j < objFile.Faces.Count; j++) {
            indices[j * 3] = (uint)objFile.Faces[j].Vertices[0].Vertex - 1;
            indices[j * 3 + 1] = (uint)objFile.Faces[j].Vertices[1].Vertex - 1;
            indices[j * 3 + 2] = (uint)objFile.Faces[j].Vertices[2].Vertex - 1;
        }

        elementBufferObjects[0] = GL.GenBuffer();
        GL.BindBuffer(BufferTarget.ElementArrayBuffer, elementBufferObjects[0]);
        GL.BufferData(BufferTarget.ElementArrayBuffer, numIndices[0] * sizeof(uint), indices, BufferUsageHint.StaticDraw);

        shader = new Shader("shaders/shader.vert", "shaders/shader.frag");
        shader.Use();

        int vertexLocation = shader.GetAttribLocation("aPosition");
        
        GL.BindVertexArray(vertexArrayObject);
        GL.VertexAttribPointer(vertexLocation, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);
        GL.EnableVertexAttribArray(vertexLocation);
        
        this.position = position;
        this.rotation = rotation;
        this.color = color;
        this.scale = scale;
        updateMatrix();
    }

    public void updateMatrix() {
        modelMatrix = Matrix4.CreateScale(scale) * Matrix4.CreateRotationX(rotation.X) * Matrix4.CreateRotationY(rotation.Y) * Matrix4.CreateRotationZ(rotation.Z) * Matrix4.CreateTranslation(position);
    }

    public void render() {
        GL.BindVertexArray(vertexArrayObject);

        for (int i = 0; i < elementBufferObjects.Length; i++) {
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, elementBufferObjects[i]);

            shader.Use();
            shader.SetMatrix4("mvp", modelMatrix);
            shader.SetVector3("color", color);

            GL.DrawElements(PrimitiveType.Triangles, numIndices[i], DrawElementsType.UnsignedInt, 0);
        }
    }
}