using OpenTK.Mathematics;
using JeremyAnsel.Media.WavefrontObj;
using OpenTK.Graphics.OpenGL4;

class Object3D
{
    Matrix4 modelMatrix;
    public Vector3 position;
    public Vector3 rotation;
    Shader shader;

    public float mass = 4;

    int vertexBufferObject;
    int vertexArrayObject;
    int[] elementBufferObjects;
    int[] numIndices;
    Vector3[] colors;

    public Object3D(string objFilePath, string vertShaderFile, string fragShaderFile, Vector3 position, Vector3 rotation) {
        ObjFile objFile = ObjFile.FromFile(objFilePath);
        // Count materials
        int materialCount = 0;
        for (int i = 0; i < objFile.MaterialLibraries.Count; i++) {
            ObjMaterialFile mtlFile = ObjMaterialFile.FromFile(objFile.MaterialLibraries[i]);
            materialCount += mtlFile.Materials.Count;
        }
        ObjMaterial[] materials = new ObjMaterial[materialCount];
        // Load materials from .obj
        for (int i = 0; i < objFile.MaterialLibraries.Count; i++) {
            ObjMaterialFile mtlFile = ObjMaterialFile.FromFile(objFile.MaterialLibraries[i]);
            for (int j = 0; j < mtlFile.Materials.Count; j++) {
                materials[i * mtlFile.Materials.Count + j] = mtlFile.Materials[j];
            }
        }

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

        elementBufferObjects = new int[materials.Length];
        colors = new Vector3[materials.Length];
        numIndices = new int[materials.Length];

        //Iterate through materials
        for (int i = 0; i < materials.Length; i++)
        {
            // Retrieve the diffuse color
            colors[i].X = materials[i].DiffuseColor.Color.X;
            colors[i].Y = materials[i].DiffuseColor.Color.Y;
            colors[i].Z = materials[i].DiffuseColor.Color.Z;

            // Count faces that use this material
            numIndices[i] = 0;
            for (int j = 0; j < objFile.Faces.Count; j++) {
                if (objFile.Faces[j].MaterialName == materials[i].Name) {
                    numIndices[i] += 3;
                }
            }

            // Retrieve indices that use this material
            uint[] indices = new uint[numIndices[i]];
            int foundIndex = 0;
            for (int j = 0; j < objFile.Faces.Count; j++) {
                if (objFile.Faces[j].MaterialName == materials[i].Name) {
                    indices[foundIndex * 3] = (uint)objFile.Faces[j].Vertices[0].Vertex - 1;
                    indices[foundIndex * 3 + 1] = (uint)objFile.Faces[j].Vertices[1].Vertex - 1;
                    indices[foundIndex * 3 + 2] = (uint)objFile.Faces[j].Vertices[2].Vertex - 1;
                    foundIndex++;
                }
            }

            elementBufferObjects[i] = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, elementBufferObjects[i]);
            GL.BufferData(BufferTarget.ElementArrayBuffer, numIndices[i] * sizeof(uint), indices, BufferUsageHint.StaticDraw);
        }

        shader = new Shader(vertShaderFile, fragShaderFile);
        shader.Use();

        int vertexLocation = shader.GetAttribLocation("aPosition");
        
        GL.BindVertexArray(vertexArrayObject);
        GL.VertexAttribPointer(vertexLocation, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);
        GL.EnableVertexAttribArray(vertexLocation);
        
        this.position = position;
        this.rotation = rotation;
        updateMatrix();
    }

    public void updateMatrix() {
        modelMatrix = Matrix4.CreateRotationX(rotation.X) * Matrix4.CreateRotationY(rotation.Y) * Matrix4.CreateRotationZ(rotation.Z) * Matrix4.CreateTranslation(position);
    }

    public void render(Camera camera) {
        GL.BindVertexArray(vertexArrayObject);

        for (int i = 0; i < elementBufferObjects.Length; i++) {
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, elementBufferObjects[i]);

            shader.Use();
            shader.SetMatrix4("mvp", modelMatrix * camera.cameraMatrix);
            shader.SetVector3("color", colors[i]);

            GL.DrawElements(PrimitiveType.Triangles, numIndices[i], DrawElementsType.UnsignedInt, 0);
        }
    }
}