using OpenTK.Mathematics;

class Camera
{
    public Matrix4 cameraMatrix;
    public Vector3 position;
    public Vector3 rotation;
    public float aspectRatio;
    public float fov;
    public float depthNear;
    public float depthFar;

    public Camera(Vector3 position, Vector3 rotation, float aspectRatio, float fov, float depthNear, float depthFar) {
        this.position = position;
        this.rotation = rotation;
        this.aspectRatio = aspectRatio;
        this.fov = fov;
        this.depthNear = depthNear;
        this.depthFar = depthFar;
        updateMatrix();
    }

    public void updateMatrix() {
        cameraMatrix = Matrix4.CreateRotationX(rotation.X) * Matrix4.CreateRotationY(rotation.Y) * Matrix4.CreateRotationZ(rotation.Z) * Matrix4.CreateTranslation(this.position) * Matrix4.CreatePerspectiveFieldOfView(fov, aspectRatio, depthNear, depthFar);
    }
}