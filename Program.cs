using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.Common;
using OpenTK.Mathematics;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Windowing.GraphicsLibraryFramework;

NativeWindowSettings nativeWindowSettings = new NativeWindowSettings();
nativeWindowSettings.ClientSize = new Vector2i(640, 480);
nativeWindowSettings.Title = "Curling 3D";
nativeWindowSettings.Flags = ContextFlags.ForwardCompatible;
nativeWindowSettings.NumberOfSamples = 8;
Window window = new Window(GameWindowSettings.Default, nativeWindowSettings);
window.Run();

class Window : GameWindow
{
    Object3D[] object3Ds = new Object3D[4];
    double t = 0;

    Camera camera;
    float speed = 1.5f;

    public Window(GameWindowSettings gameWindowSettings, NativeWindowSettings nativeWindowSettings) : base(gameWindowSettings, nativeWindowSettings) {
        camera = new Camera(new Vector3(0, 1, 0), new Vector3(0, 0, 0), (float)Size.X / Size.Y, 1.04f, 0.01f, 1000);
    }

    protected override void OnLoad() {
        base.OnLoad();

        GL.ClearColor(0.5f, 0.5f, 0.5f, 1.0f);
        GL.Enable(EnableCap.DepthTest);
        GL.Enable(EnableCap.Multisample);

        object3Ds[0] = new Object3D("models/rock-red.obj", "shaders/shader.vert", "shaders/shader.frag", new Vector3(0, 0.05f, 0), new Vector3(0, 0, 0));
        object3Ds[1] = new Object3D("models/rock-blue.obj", "shaders/shader.vert", "shaders/shader.frag", new Vector3(0, 0.05f, 0), new Vector3(0, 0, 0));
        object3Ds[2] = new Object3D("models/ice.obj", "shaders/shader.vert", "shaders/shader.frag", new Vector3(0, 0, 0), new Vector3(0, 0, 0));
        object3Ds[3] = new Object3D("models/cube.obj", "shaders/shader.vert", "shaders/shader.frag", new Vector3(0, 1, 0), new Vector3(0, 0, 0));
    }

    protected override void OnRenderFrame(FrameEventArgs e) {
        base.OnRenderFrame(e);
        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
        for (int i = 0; i < object3Ds.Length; i++) {
            object3Ds[i].render(camera);
        }
        SwapBuffers();
    }

    protected override void OnUpdateFrame(FrameEventArgs e) {
        base.OnUpdateFrame(e);
        
        object3Ds[0].position.X = (float)Math.Cos(t) * 2;
        object3Ds[0].position.Z = (float)Math.Sin(t) * 2;
        object3Ds[0].updateMatrix();
        object3Ds[1].position.X = -(float)Math.Cos(t) * 2;
        object3Ds[1].position.Z = -(float)Math.Sin(t) * 2;
        object3Ds[1].updateMatrix();
        object3Ds[3].rotation.X = (float)t;
        object3Ds[3].rotation.Y = (float)t;
        object3Ds[3].updateMatrix();
        t += e.Time;

        if (KeyboardState.IsKeyDown(Keys.W)) {
            camera.position.X -= speed * (float)e.Time * (float)Math.Sin(camera.rotation.Y);
            camera.position.Z -= speed * (float)e.Time * (float)Math.Cos(camera.rotation.Y);
            camera.updateMatrix();
        }
        if (KeyboardState.IsKeyDown(Keys.S)) {
            camera.position.X += speed * (float)e.Time * (float)Math.Sin(camera.rotation.Y);
            camera.position.Z += speed * (float)e.Time * (float)Math.Cos(camera.rotation.Y);
            camera.updateMatrix();
        }
        if (KeyboardState.IsKeyDown(Keys.D)) {
            camera.position.X += speed * (float)e.Time * (float)Math.Cos(camera.rotation.Y);
            camera.position.Z -= speed * (float)e.Time * (float)Math.Sin(camera.rotation.Y);
            camera.updateMatrix();
        }
        if (KeyboardState.IsKeyDown(Keys.A)) {
            camera.position.X -= speed * (float)e.Time * (float)Math.Cos(camera.rotation.Y);
            camera.position.Z += speed * (float)e.Time * (float)Math.Sin(camera.rotation.Y);
            camera.updateMatrix();
        }
        if (KeyboardState.IsKeyDown(Keys.Left)) {
            camera.rotation.Y += speed * (float)e.Time;
            camera.updateMatrix();
        }
        if (KeyboardState.IsKeyDown(Keys.Right)) {
            camera.rotation.Y -= speed * (float)e.Time;
            camera.updateMatrix();
        }
        if (KeyboardState.IsKeyDown(Keys.Up)) {
            camera.rotation.X += speed * (float)e.Time;
            camera.updateMatrix();
        }
        if (KeyboardState.IsKeyDown(Keys.Down)) {
            camera.rotation.X -= speed * (float)e.Time;
            camera.updateMatrix();
        }
    }

    protected override void OnResize(ResizeEventArgs e) {
        base.OnResize(e);
        camera.aspectRatio = (float)Size.X / Size.Y;
        camera.updateMatrix();
        GL.Viewport(0, 0, Size.X, Size.Y);
    }
}