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
    public float rotationSpeed;
    public float mass;
    public float drag;
    public bool isRed;

    public Rock(Vector3 position, Vector3 rotation, Vector3 velocity, float mass, bool isRed) {
        this.position = position;
        this.rotation = rotation;
        this.velocity = velocity;
        this.mass = mass;
        this.isRed = isRed;
        this.rotationSpeed = 0;
        this.drag = 0.1f;
    }
    public object Clone() {
        return new Rock(this.position, this.rotation, this.velocity, this.mass, this.isRed);
    }
}

class Window : GameWindow {
    Object3D ice;
    Object3D redRock;
    Object3D blueRock;
    Object3D broom;
    Object3D arena;
    Object3D cube;

    Rock[] rocks = new Rock[16];
    float[] icePositions = {0, -4.5f, 4.5f + 3.27f, 4.5f + 4.5f + 3.27f};

    ObjectUI[] objectUIs = new ObjectUI[2];
    double t = 0;

    Camera camera;
    float speed = 3;
    string phase = "title";

    int currentRock = 2;

    public Window(GameWindowSettings gameWindowSettings, NativeWindowSettings nativeWindowSettings) : base(gameWindowSettings, nativeWindowSettings) {
        camera = new Camera(new Vector3(0, 3, 0), new Vector3(0, 0, 0), (float)Size.X / Size.Y, 1.04f, 0.01f, 1000);
        for (int j = 0; j < 16; j++) {
            int i = j / 2;
            if (j % 2 == 0) {
                rocks[j] = new Rock(new Vector3(0.4f * (i/2) + 21, 0, -1.5f - 0.4f * (i%2)), new Vector3(0, 0, 0), new Vector3(0, 0, 0), 1, true);
            }
            else {
                rocks[j] = new Rock(new Vector3(0.4f * (i/2) + 21, 0, 0.4f * (i%2) + 1.5f), new Vector3(0, 0, 0), new Vector3(0, 0, 0), 1, false);
            }
        }
        rocks[0] = new Rock(new Vector3(16.5f, 0, 0), new Vector3(0, 0, 0), new Vector3(0, 0, 0), 1, true);
        rocks[1] = new Rock(new Vector3(-20, 0, 0.1f), new Vector3(0, 0, 0), new Vector3(5, 0, 0), 1, false);
    }

    protected override void OnLoad() {
        base.OnLoad();

        GL.ClearColor(0.5f, 0.7f, 1.0f, 1.0f);
        GL.Enable(EnableCap.DepthTest);
        GL.Enable(EnableCap.Multisample);

        redRock = new Object3D("models/rock-austin.obj", "shaders/shader-smooth.vert", "shaders/shader-smooth.frag", new Vector3(0, 0, 0), new Vector3(0, 0, 0));
        blueRock = new Object3D("models/rock-blue.obj", "shaders/shader-smooth.vert", "shaders/shader-smooth.frag", new Vector3(0, 0, 0), new Vector3(0, 0, 0));
        ice = new Object3D("models/ice.obj", "shaders/shader.vert", "shaders/shader.frag", new Vector3(0, 0, 0), new Vector3(0, 0, 0));
        broom = new Object3D("models/broom.obj", "shaders/shader.vert", "shaders/shader.frag", new Vector3(0, 0, 0), new Vector3(0, 0, 0));
        arena = new Object3D("models/Arena.obj", "shaders/shader-smooth.vert", "shaders/shader-smooth.frag", new Vector3(0, 0, 0), new Vector3(0, 0, 0));
        cube = new Object3D("models/cube.obj", "shaders/shader-vertex.vert", "shaders/shader-vertex.frag", new Vector3(0, 10, 0), new Vector3(0, 0, 0));
        objectUIs[0] = new ObjectUI("models/title.obj", new Vector3(0.5f, 0, 0), new Vector3(0.5f, 0.5f, 0), new Vector3(0, 0, 0), new Vector3(0.5f, 0.5f, 0.5f));
        objectUIs[1] = new ObjectUI("models/space.obj", new Vector3(0, 0, 0.5f), new Vector3(0.5f, 0, 0), new Vector3(0, 0, 0), new Vector3(0.25f, 0.25f, 0.25f));
    }

    protected override void OnRenderFrame(FrameEventArgs e) {
        base.OnRenderFrame(e);
        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
        for (int i = 0; i < icePositions.Length; i++) {
            ice.position.Z = icePositions[i];
            ice.updateMatrix();
            ice.render(camera);
        }
        arena.render(camera);
        broom.render(camera);
        cube.render(camera);
        if (phase == "title") {
            redRock.render(camera);
            blueRock.render(camera);
            for (int i = 0; i < objectUIs.Length; i++) {
                objectUIs[i].render();
            }
        }
        else {
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
        SwapBuffers();
    }

    protected override void OnUpdateFrame(FrameEventArgs e) {
        base.OnUpdateFrame(e);

        if (phase == "freecam") {
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
            if (KeyboardState.IsKeyDown(Keys.LeftShift)) {
                camera.position.Y -= speed * (float)e.Time;
                camera.updateMatrix();
            }
            if (KeyboardState.IsKeyDown(Keys.Space)) {
                camera.position.Y += speed * (float)e.Time;
                camera.updateMatrix();
            }
            if (KeyboardState.IsKeyDown(Keys.N)) {
                phase = "broom";
                camera.position = new Vector3(20f, 1, 0);
                camera.rotation = new Vector3(0, (float)Math.PI/2, 0);
                camera.updateMatrix();
                broom.position = new Vector3(16.5f, 0, 0);
                broom.rotation = new Vector3(0, (float)-Math.PI/2, 0);
                broom.updateMatrix();
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
        else if (phase == "broom") {
            if (KeyboardState.IsKeyDown(Keys.Left)) {
                camera.position.Z += speed * (float)e.Time;
                camera.updateMatrix();
                broom.position.Z = camera.position.Z;
                broom.updateMatrix();
            }
            if (KeyboardState.IsKeyDown(Keys.Right)) {
                camera.position.Z -= speed * (float)e.Time;
                camera.updateMatrix();
                broom.position.Z = camera.position.Z;
                broom.updateMatrix();
            }
            if (KeyboardState.IsKeyDown(Keys.Up)) {
                broom.position.X -= speed * (float)e.Time;
                broom.updateMatrix();
            }
            if (KeyboardState.IsKeyDown(Keys.Down)) {
                broom.position.X += speed * (float)e.Time;
                broom.updateMatrix();
            }
            if (KeyboardState.IsKeyDown(Keys.Space)) {
                phase = "aim";
                camera.position = new Vector3(-20, 0.5f, 0);
                camera.rotation = new Vector3(0, (float)-Math.PI/2, 0);
                camera.updateMatrix();
            }
        }
        else if (phase == "aim") {
            if (KeyboardState.IsKeyDown(Keys.Left)) {
                camera.rotation.Y += speed/5 * (float)e.Time;
                camera.updateMatrix();
            }
            if (KeyboardState.IsKeyDown(Keys.Right)) {
                camera.rotation.Y -= speed/5 * (float)e.Time;
                camera.updateMatrix();
            }
            if (KeyboardState.IsKeyDown(Keys.N)) {
                phase = "sweep";
                rocks[currentRock].rotationSpeed = -(camera.rotation.Y + (float)Math.PI/2) * 10;
                rocks[currentRock].position = new Vector3(-20, 0, 0);
                rocks[currentRock].velocity = new Vector3((float)-Math.Sin(camera.rotation.Y) * 5, 0, (float)-Math.Cos(camera.rotation.Y) * 5);
                camera.position = new Vector3(-20, 3, 3);
                camera.rotation = new Vector3(-0.78f, 0, 0);
                camera.updateMatrix();
            }
        }
        else if (phase == "sweep") {
            broom.position.X = rocks[currentRock].position.X + 0.5f;
            if (KeyboardState.IsKeyDown(Keys.Space)) {
                rocks[currentRock].drag = 0.1f;
                broom.position.Z = rocks[currentRock].position.Z + (float)Math.Sin(t*20)/5;
            }
            else {
                rocks[currentRock].drag = 0.5f;
                broom.position.Z = rocks[currentRock].position.Z + 2;
            }
            broom.updateMatrix();

            camera.position.X = rocks[currentRock].position.X;
            camera.position.Z = rocks[currentRock].position.Z + 3;
            if (KeyboardState.IsKeyDown(Keys.Up)) {
                camera.rotation.X += speed/2 * (float)e.Time;
            }
            if (KeyboardState.IsKeyDown(Keys.Down)) {
                camera.rotation.X -= speed/2 * (float)e.Time;
            }
            camera.updateMatrix();

            bool allRocksStopped = true;
            for (int i = 0; i < 16; i++) {
                if (rocks[i].velocity.Length > 0.01f) {
                    allRocksStopped = false;
                    break;
                }
            }
            if (allRocksStopped) {
                if (currentRock == 15) {
                    Console.WriteLine("All done");
                    Environment.Exit(0);
                }
                phase = "broom";
                camera.position = new Vector3(20f, 1, 0);
                camera.rotation = new Vector3(0, (float)Math.PI/2, 0);
                camera.updateMatrix();
                broom.position = new Vector3(16.5f, 0, 0);
                broom.rotation = new Vector3(0, (float)-Math.PI/2, 0);
                broom.updateMatrix();
                currentRock++;
            }
        }
        else if (phase == "title") {
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
            if (KeyboardState.IsKeyDown(Keys.Space)) {
                phase = "freecam";
                camera.position = new Vector3(16.5f, 1, 0);
                camera.rotation = new Vector3(0, 0, 0);
                camera.updateMatrix();
            }
        }

        if (phase != "title") {
            
            // make deep copy of rocks
            Rock[] newRocks = rocks.Select (a =>(Rock)a.Clone()).ToArray();
            for (int i = 0; i < rocks.Length; i++) {
                // add drag
                rocks[i].velocity -= rocks[i].drag * rocks[i].velocity * (float)e.Time/rocks[i].mass;
                // add curl
                if (rocks[i].rotationSpeed > 0) {
                    rocks[i].rotationSpeed -= rocks[i].drag * 0.1f * (float)e.Time;
                }
                if (rocks[i].rotationSpeed < 0) {
                    rocks[i].rotationSpeed += rocks[i].drag * 0.1f * (float)e.Time;
                }
                rocks[i].velocity.Z -= rocks[i].rotationSpeed * 0.03f * (float)e.Time;

                for (int j = 0; j < rocks.Length; j++) {
                    if (i == j) {
                        continue;
                    }
                    
                    // apply collisions if they exist
                    CollisionResult result = Collision.Collide(newRocks[i], newRocks[j], 0.3f);
                    if (result.didCollide) {
                        Console.WriteLine("rock " + i + " collided with " + j);
                        rocks[i].velocity = result.velocity;
                    }
                }

                // update position and rotation
                rocks[i].position += rocks[i].velocity * (float)e.Time;
                rocks[i].rotation.Y += rocks[i].rotationSpeed * (float)e.Time;
            }
        }

        t += e.Time;
        blueRock.dir = t;
        redRock.dir = t;
        cube.rotation = new Vector3((float)t, (float)t, 0);
        cube.updateMatrix();
    }

    protected override void OnResize(ResizeEventArgs e) {
        base.OnResize(e);
        camera.aspectRatio = (float)Size.X / Size.Y;
        camera.updateMatrix();
        if (phase == "title") {
                objectUIs[0].scale = new Vector3(1/camera.aspectRatio * 0.5f, camera.aspectRatio * 0.5f, 0.5f);
                objectUIs[0].updateMatrix();
                objectUIs[1].scale = new Vector3(1/camera.aspectRatio * 0.25f, camera.aspectRatio * 0.25f, 0.25f);
                objectUIs[1].updateMatrix();
        }
        GL.Viewport(0, 0, Size.X, Size.Y);
    }
}