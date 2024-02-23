using OpenTK.Mathematics;
using JeremyAnsel.Media.WavefrontObj;
using OpenTK.Graphics.OpenGL4;

class Object3D {
    Matrix4 modelMatrix;
    public Vector3 position;
    public Vector3 rotation;
    Shader shader;

    int[] vertexArrayObjects;
    int[] numVertices;
    Vector3[] colors;

    public double dir;
    Vector3 lightDir = new Vector3(0, 1, 0);

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

        vertexArrayObjects = new int[materials.Length];
        colors = new Vector3[materials.Length];
        numVertices = new int[materials.Length];

        shader = new Shader(vertShaderFile, fragShaderFile);
        shader.Use();

        int vertexLocation = shader.GetAttribLocation("aPosition");
        int vertexNormal = shader.GetAttribLocation("aNormal");

        //Iterate through materials
        for (int i = 0; i < materials.Length; i++) {

            // Retrieve the diffuse color
            colors[i].X = materials[i].DiffuseColor.Color.X;
            colors[i].Y = materials[i].DiffuseColor.Color.Y;
            colors[i].Z = materials[i].DiffuseColor.Color.Z;

            // Count vertices that use this material
            int verticesUsing = 0;
            for (int j = 0; j < objFile.Faces.Count; j++) {
                if (objFile.Faces[j].MaterialName == materials[i].Name) {
                    for (int k = 0; k < objFile.Faces[i].Vertices.Count; k++) {
                        verticesUsing++;
                    }
                }
            }
            numVertices[i] = verticesUsing;

            // Load vertices from .obj
            int foundIndex = 0;
            float[] vertices = new float[verticesUsing * 6];
            for (int j = 0; j < objFile.Faces.Count; j++) {
                if (objFile.Faces[j].MaterialName == materials[i].Name) {
                    for (int k = 0; k < 3; k++) {
                        vertices[foundIndex * 6] = objFile.Vertices[objFile.Faces[j].Vertices[k].Vertex - 1].Position.X;
                        vertices[foundIndex * 6 + 1] = objFile.Vertices[objFile.Faces[j].Vertices[k].Vertex - 1].Position.Y;
                        vertices[foundIndex * 6 + 2] = objFile.Vertices[objFile.Faces[j].Vertices[k].Vertex - 1].Position.Z;
                        vertices[foundIndex * 6 + 3] = objFile.VertexNormals[objFile.Faces[j].Vertices[k].Normal - 1].X;
                        vertices[foundIndex * 6 + 4] = objFile.VertexNormals[objFile.Faces[j].Vertices[k].Normal - 1].Y;
                        vertices[foundIndex * 6 + 5] = objFile.VertexNormals[objFile.Faces[j].Vertices[k].Normal - 1].Z;
                        foundIndex++;
                    }
                }
            }

            if (objFilePath == "models/cube.obj") {
            for (int j = 0; j < 6; j++) {
                Console.WriteLine(vertices[j*6] + " " + vertices[j*6+1] + " " + vertices[j*6+2] + " " + vertices[j*6+3] + " " + vertices[j*6+4] + " " + vertices[j*6+5]);
            }
            }

            vertexArrayObjects[i] = GL.GenVertexArray();
            GL.BindVertexArray(vertexArrayObjects[i]);
            int vertexBufferObject = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, vertexBufferObject);
            GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.StaticDraw);

        GL.VertexAttribPointer(vertexLocation, 3, VertexAttribPointerType.Float, false, 6 * sizeof(float), 0);
        GL.EnableVertexAttribArray(vertexLocation);
        GL.VertexAttribPointer(vertexNormal, 3, VertexAttribPointerType.Float, false, 6 * sizeof(float), 3);
        GL.EnableVertexAttribArray(vertexNormal);
        }
        
        this.position = position;
        this.rotation = rotation;
        updateMatrix();
    }

    public void updateMatrix() {
        modelMatrix = Matrix4.CreateRotationX(rotation.X) * Matrix4.CreateRotationY(rotation.Y) * Matrix4.CreateRotationZ(rotation.Z) * Matrix4.CreateTranslation(position);
    }

    public void render(Camera camera) {

        for (int i = 0; i < vertexArrayObjects.Length; i++) {  
            GL.BindVertexArray(vertexArrayObjects[i]);

            shader.Use();
            shader.SetMatrix4("mvp", modelMatrix * camera.cameraMatrix);
            //shader.SetVector3("color", colors[i]);
            //shader.SetVector3("lightDir", lightDir);

            GL.DrawArrays(PrimitiveType.Triangles, 0, numVertices[i]);

            lightDir.X = (float)Math.Sin(dir);
            lightDir.Z = (float)Math.Cos(dir);
        }
    }
    static Vector3 Average(ObjVector3 a, ObjVector3 b, ObjVector3 c) {
        Vector3 sum;
        sum.X = a.X + b.X + c.X;
        sum.Y = a.Y + b.Y + c.Y;
        sum.Z = a.Z + b.Z + c.Z;
        return sum / 3;
    }
}