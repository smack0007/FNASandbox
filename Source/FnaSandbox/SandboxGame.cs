using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FnaSandbox
{
    public class SandboxGame : Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        public SandboxGame()
        {
            this.graphics = new GraphicsDeviceManager(this);
            this.Content.RootDirectory = "Content";
        }

        protected override void Initialize()
        {
            base.Initialize();
        }

        protected override void LoadContent()
        {
            this.spriteBatch = new SpriteBatch(GraphicsDevice);
        }

        protected override void UnloadContent()
        {
        }
        
        protected override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            this.GraphicsDevice.Clear(Color.CornflowerBlue);

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
