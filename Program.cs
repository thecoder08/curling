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
    Object3D[] object3Ds = new Object3D[3];
    ObjectUI[] objectUIs = new ObjectUI[2];
    double t = 0;

    Camera camera;
    float speed = 1.5f;
    bool controllable = false;
    Vector2 redVelocity = new Vector2(0, 0);
    Vector2 blueVelocity = new Vector2(0, 0);

    float drag = 0.0001f;

    public Window(GameWindowSettings gameWindowSettings, NativeWindowSettings nativeWindowSettings) : base(gameWindowSettings, nativeWindowSettings) {
        camera = new Camera(new Vector3(0, 3, 0), new Vector3(0, 0, 0), (float)Size.X / Size.Y, 1.04f, 0.01f, 1000);
    }

    protected override void OnLoad() {
        base.OnLoad();

        GL.ClearColor(0.5f, 0.5f, 0.5f, 1.0f);
        GL.Enable(EnableCap.DepthTest);
        GL.Enable(EnableCap.Multisample);

        object3Ds[0] = new Object3D("models/rock-red.obj", "shaders/shader.vert", "shaders/shader.frag", new Vector3(0, 0.05f, 0), new Vector3(0, 0, 0));
        object3Ds[1] = new Object3D("models/rock-blue.obj", "shaders/shader.vert", "shaders/shader.frag", new Vector3(0, 0.05f, 0), new Vector3(0, 0, 0));
        object3Ds[2] = new Object3D("models/ice.obj", "shaders/shader.vert", "shaders/shader.frag", new Vector3(0, 0, 0), new Vector3(0, 0, 0));
        //object3Ds[3] = new Object3D("models/cube.obj", "shaders/shader.vert", "shaders/shader.frag", new Vector3(0, 1, 0), new Vector3(0, 0, 0));
        objectUIs[0] = new ObjectUI("models/title.obj", new Vector3(0.5f, 0, 0), new Vector3(0.5f, 0.5f, 0), new Vector3(0, 0, 0), new Vector3(0.5f, 0.5f, 0.5f));
        objectUIs[1] = new ObjectUI("models/space.obj", new Vector3(0, 0, 0.5f), new Vector3(0.5f, 0, 0), new Vector3(0, 0, 0), new Vector3(0.25f, 0.25f, 0.25f));
    }

    protected override void OnRenderFrame(FrameEventArgs e) {
        base.OnRenderFrame(e);
        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
        for (int i = 0; i < object3Ds.Length; i++) {
            object3Ds[i].render(camera);
        }
        for (int i = 0; i < objectUIs.Length; i++) {
            objectUIs[i].render();
        }
        SwapBuffers();
    }

    protected override void OnUpdateFrame(FrameEventArgs e) {
        base.OnUpdateFrame(e);

        if (KeyboardState.IsKeyDown(Keys.Space) && !controllable) {
            controllable = true;
            camera.position = new Vector3(0, 1, 0);
            camera.rotation = new Vector3(0, 0, 0);
            camera.updateMatrix();
            object3Ds[0].position = new Vector3(0, 0.05f, 0.1f);
            object3Ds[0].updateMatrix();
            redVelocity = new Vector2(1, 0);
            object3Ds[1].position = new Vector3(10, 0.05f, 0);
            object3Ds[1].updateMatrix();
            objectUIs[0].rotation.Y = (float)Math.PI/2;
            objectUIs[0].updateMatrix();
            objectUIs[1].rotation.Y = (float)Math.PI/2;
            objectUIs[1].updateMatrix();            
        }

        if (controllable) {
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
            object3Ds[0].position.X += redVelocity.X * (float)e.Time;
            object3Ds[0].position.Z += redVelocity.Y * (float)e.Time;
            object3Ds[1].position.X += blueVelocity.X * (float)e.Time;
            object3Ds[1].position.Z += blueVelocity.Y * (float)e.Time;
            if (Collision.HaveCollided(new Vector2(object3Ds[0].position.X, object3Ds[0].position.Z), new Vector2(object3Ds[1].position.X, object3Ds[1].position.Z), 0.3f)) {
                CollisionResult result = Collision.Collide(new Vector2(object3Ds[0].position.X, object3Ds[0].position.Z), redVelocity, object3Ds[0].mass, new Vector2(object3Ds[1].position.X, object3Ds[1].position.Z), blueVelocity, object3Ds[1].mass);
                redVelocity = result.velocity1;
                blueVelocity = result.velocity2;
            }
            redVelocity.X -= drag*redVelocity.X/object3Ds[0].mass;
            redVelocity.Y -= drag*redVelocity.Y/object3Ds[0].mass;
            blueVelocity.X -= drag*blueVelocity.X/object3Ds[0].mass;
            blueVelocity.Y -= drag*blueVelocity.Y/object3Ds[0].mass;
            object3Ds[0].updateMatrix();
            object3Ds[1].updateMatrix();
        }
        else {
            camera.rotation.Y = (float)(Math.PI - t)/2;
            camera.position.X = (float)Math.Cos(t/2) * 10;
            camera.position.Z = (float)Math.Sin(t/2) * 10;
            camera.updateMatrix();
            object3Ds[0].position.X = (float)Math.Cos(t) * 2;
            object3Ds[0].position.Z = (float)Math.Sin(t) * 2;
            object3Ds[0].updateMatrix();
            object3Ds[1].position.X = -(float)Math.Cos(t) * 2;
            object3Ds[1].position.Z = -(float)Math.Sin(t) * 2;
            object3Ds[1].updateMatrix();
            //object3Ds[3].rotation.X = (float)t;
            //object3Ds[3].rotation.Y = (float)t;
            //object3Ds[3].updateMatrix();
            objectUIs[0].rotation.Y = (float)t;
            objectUIs[0].updateMatrix();
        }

        t += e.Time;
    }

    protected override void OnResize(ResizeEventArgs e) {
        base.OnResize(e);
        camera.aspectRatio = (float)Size.X / Size.Y;
        camera.updateMatrix();
        if (!controllable) {
                objectUIs[0].scale = new Vector3(1/camera.aspectRatio * 0.5f, camera.aspectRatio * 0.5f, 0.5f);
                objectUIs[0].updateMatrix();
                objectUIs[1].scale = new Vector3(1/camera.aspectRatio * 0.25f, camera.aspectRatio * 0.25f, 0.25f);
                objectUIs[1].updateMatrix();
        }
        GL.Viewport(0, 0, Size.X, Size.Y);
    }
}