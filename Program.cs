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

class Rock {
    public Vector3 position;
    public Vector3 rotation;
    public Vector3 velocity;
    public float mass;
    public bool isRed;

    public Rock(Vector3 position, Vector3 rotation, Vector3 velocity, float mass, bool isRed) {
        this.position = position;
        this.rotation = rotation;
        this.velocity = velocity;
        this.mass = mass;
        this.isRed = isRed;
    }
    public object Clone() {
        return new Rock(this.position, this.rotation, this.velocity, this.mass, this.isRed);
    }
}

class Window : GameWindow {
    Object3D ice = null;
    Object3D redRock = null;
    Object3D blueRock = null;
    Rock[] rocks = new Rock[16];

    ObjectUI[] objectUIs = new ObjectUI[2];
    double t = 0;

    Camera camera;
    float speed = 1.5f;
    bool controllable = false;

    float drag = 0.1f;

    public Window(GameWindowSettings gameWindowSettings, NativeWindowSettings nativeWindowSettings) : base(gameWindowSettings, nativeWindowSettings) {
        camera = new Camera(new Vector3(0, 3, 0), new Vector3(0, 0, 0), (float)Size.X / Size.Y, 1.04f, 0.01f, 1000);
        rocks[0] = new Rock(new Vector3(16.5f, 0.05f, 0), new Vector3(0, 0, 0), new Vector3(0, 0, 0), 1, true);
        rocks[8] = new Rock(new Vector3(0, 0.05f, 0.1f), new Vector3(0, 0, 0), new Vector3(3, 0, 0), 1, false);
        for (int i = 1; i < 8; i++) {
            rocks[i] = new Rock(new Vector3(0.4f * (i%4) + 21, 0.05f, -1.5f - 0.4f * (i/4)), new Vector3(0, 0, 0), new Vector3(0, 0, 0), 1, true);
        }
        for (int i = 9; i < 16; i++) {
            rocks[i] = new Rock(new Vector3(0.4f * (i%4) + 21, 0.05f, 0.4f * (i/4) + 1.5f), new Vector3(0, 0, 0), new Vector3(0, 0, 0), 1, false);
        }
    }

    protected override void OnLoad() {
        base.OnLoad();

        GL.ClearColor(0.5f, 0.5f, 0.5f, 1.0f);
        GL.Enable(EnableCap.DepthTest);
        GL.Enable(EnableCap.Multisample);

        redRock = new Object3D("models/rock-red.obj", "shaders/shader.vert", "shaders/shader.frag", new Vector3(0, 0.05f, 0), new Vector3(0, 0, 0));
        blueRock = new Object3D("models/rock-blue.obj", "shaders/shader.vert", "shaders/shader.frag", new Vector3(0, 0.05f, 0), new Vector3(0, 0, 0));
        ice = new Object3D("models/ice.obj", "shaders/shader.vert", "shaders/shader.frag", new Vector3(0, 0, 0), new Vector3(0, 0, 0));
        //object3Ds[3] = new Object3D("models/cube.obj", "shaders/shader.vert", "shaders/shader.frag", new Vector3(0, 1, 0), new Vector3(0, 0, 0));
        objectUIs[0] = new ObjectUI("models/title.obj", new Vector3(0.5f, 0, 0), new Vector3(0.5f, 0.5f, 0), new Vector3(0, 0, 0), new Vector3(0.5f, 0.5f, 0.5f));
        objectUIs[1] = new ObjectUI("models/space.obj", new Vector3(0, 0, 0.5f), new Vector3(0.5f, 0, 0), new Vector3(0, 0, 0), new Vector3(0.25f, 0.25f, 0.25f));
    }

    protected override void OnRenderFrame(FrameEventArgs e) {
        base.OnRenderFrame(e);
        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
        ice.render(camera);
        if (controllable) {
            for (int i = 0; i < rocks.Length; i++) {
                if (rocks[i].isRed) {
                    redRock.position = rocks[i].position;
                    redRock.rotation = rocks[i].rotation;
                    redRock.updateMatrix();
                    redRock.render(camera);
                }
                else {
                    blueRock.position = rocks[i].position;
                    blueRock.rotation = rocks[i].rotation;
                    blueRock.updateMatrix();
                    blueRock.render(camera);
                }
            }
        }
        else {
            redRock.render(camera);
            blueRock.render(camera);
            for (int i = 0; i < objectUIs.Length; i++) {
                objectUIs[i].render();
            }
        }
        SwapBuffers();
    }

    protected override void OnUpdateFrame(FrameEventArgs e) {
        base.OnUpdateFrame(e);

        if (KeyboardState.IsKeyDown(Keys.Space) && !controllable) {
            controllable = true;
            camera.position = new Vector3(16.5f, 1, 0);
            camera.rotation = new Vector3(0, 0, 0);
            camera.updateMatrix();
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
            if (KeyboardState.IsKeyDown(Keys.C)) {
                Console.WriteLine("camera position: (" + camera.position.X + ", " + camera.position.Y + ", " + camera.position.Z + ")");
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
            // update position
            for (int i = 0; i < 16; i++) {
                rocks[i].position += rocks[i].velocity * (float)e.Time;
            }
            // apply collisions if they exist
            Rock[] newRocks = rocks.Select (a =>(Rock)a.Clone()).ToArray();
            for (int i = 0; i < rocks.Length; i++) {
                for (int j = 0; j < rocks.Length; j++) {
                    if (i == j) {
                        continue;
                    }
                    CollisionResult result = Collision.Collide(newRocks[i], newRocks[j], 0.3f);
                    if (result.didCollide) {
                        Console.WriteLine("rock " + i + " collided with " + j);
                        Console.WriteLine("velocity: (" + result.velocity1.X + ", " + result.velocity1.Y + ", " + result.velocity1.Z + ")");
                        rocks[i].velocity = result.velocity1;
                    }
                }
            }
            // apply drag
            for (int i = 0; i < 16; i++) {
                rocks[i].velocity -= drag*rocks[i].velocity*(float)e.Time/rocks[i].mass;
            }
        }
        else {
            camera.rotation.Y = (float)(Math.PI - t)/2;
            camera.position.X = (float)Math.Cos(t/2) * 10;
            camera.position.Z = (float)Math.Sin(t/2) * 10;
            camera.updateMatrix();
            redRock.position.X = (float)Math.Cos(t) * 2;
            redRock.position.Z = (float)Math.Sin(t) * 2;
            redRock.updateMatrix();
            blueRock.position.X = -(float)Math.Cos(t) * 2;
            blueRock.position.Z = -(float)Math.Sin(t) * 2;
            blueRock.updateMatrix();
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