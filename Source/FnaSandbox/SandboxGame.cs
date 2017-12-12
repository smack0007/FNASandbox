using System.IO;
using System.Reflection;
using FnaSandbox;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace DrawingTriangles
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class SandboxGame : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;

        VertexBuffer vertexBuffer;

        Effect effect;
        
        Matrix world = Matrix.Identity;

        Matrix view = new Matrix(
            1.0f, 0.0f, 0.0f, 0.0f,
            0.0f, -1.0f, 0.0f, 0.0f,
            0.0f, 0.0f, -1.0f, 0.0f,
            0.0f, 0.0f, 0.0f, 1.0f);

        Matrix projection = Matrix.CreateOrthographicOffCenter(0, 1280, -720, 0, 0, 1);

        Texture2D texture;

        SpriteRenderer spriteRenderer;

        public SandboxGame()
        {
            this.graphics = new GraphicsDeviceManager(this);
            this.graphics.PreferredBackBufferWidth = 1280;
            this.graphics.PreferredBackBufferHeight = 720;
            this.graphics.ApplyChanges();

            this.Content.RootDirectory = "Content";

            this.spriteRenderer = new SpriteRenderer(this.GraphicsDevice);
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            var assemblyDir = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);

            this.texture = Texture2D.FromStream(this.GraphicsDevice, File.OpenRead(Path.Combine(assemblyDir, "Logo.png")));
            
            this.effect = new Effect(this.GraphicsDevice, File.ReadAllBytes(Path.Combine(assemblyDir, "BasicEffect.fxb")));

            var centerX = graphics.PreferredBackBufferWidth / 2.0f;
            var centerY = graphics.PreferredBackBufferHeight / 2.0f;
            var size = 300.0f;

            var vertices = new Vertex[3];
            vertices[0] = new Vertex(centerX, centerY - size, 0.5f, 0.5f, 0, Color.Red);
            vertices[1] = new Vertex(centerX + size, centerY + size, 0.5f, 1, 1, Color.Green);
            vertices[2] = new Vertex(centerX - size, centerY + size, 0.5f, 0, 1, Color.Blue);

            var vertexDeclaration = new VertexDeclaration(new VertexElement[]
            {
                new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0),
                new VertexElement(12, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 0),
                new VertexElement(20, VertexElementFormat.Color, VertexElementUsage.Color, 0)
            });

            vertexBuffer = new VertexBuffer(GraphicsDevice, vertexDeclaration, 3, BufferUsage.WriteOnly);
            vertexBuffer.SetData(vertices);
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            // Allows the game to exit
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                this.Exit();

            this.rotation += 0.01f;

            base.Update(gameTime);
        }

        private float rotation = 0.0f;

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            this.GraphicsDevice.Clear(Color.CornflowerBlue);

            this.spriteRenderer.Begin();

            var viewport = this.GraphicsDevice.Viewport;
            var origin = new Vector2(this.texture.Width / 2, this.texture.Height / 2);

            this.spriteRenderer.Draw(
                this.texture,
                new Rectangle(
                    (viewport.Width / 2),
                    (viewport.Height / 2),
                    this.texture.Width,
                    this.texture.Height),
                origin: origin,
                rotation: this.rotation);

            this.spriteRenderer.End();

            //this.effect.Parameters["Transform"].SetValue(world * view * projection);
            //this.effect.Parameters["Texture"].SetValue(this.texture);

            //this.GraphicsDevice.SetVertexBuffer(vertexBuffer);

            //RasterizerState rasterizerState = new RasterizerState();
            //rasterizerState.CullMode = CullMode.None;
            //this.GraphicsDevice.RasterizerState = rasterizerState;

            //foreach (EffectPass pass in effect.CurrentTechnique.Passes)
            //{
            //    pass.Apply();
            //    this.GraphicsDevice.DrawPrimitives(PrimitiveType.TriangleList, 0, 1);
            //}

            base.Draw(gameTime);
        }

        public static void Main(string[] args)
        {
            new FNAAssemblyLoadContext().Init();

            using (var game = new SandboxGame())
                game.Run();
        }
    }
}