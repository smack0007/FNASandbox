using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FnaSandbox
{
    public sealed class SpriteRenderer : IDisposable
    {
        [StructLayout(LayoutKind.Sequential)]
        struct Vertex
        {
            public Vector3 Position;
            public Vector2 UV;
            public Color Color;
        }

        GraphicsDevice graphics;

        Vertex[] vertices;
        int vertexCount;
        Texture2D texture;

        VertexBuffer vertexBuffer;
        IndexBuffer indexBuffer;

        Effect shader;
        bool drawInProgress;

        Matrix viewMatrix;
        Matrix projectionMatrix;

        public SpriteRenderer(GraphicsDevice graphics)
            : this(graphics, 1024)
        {
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="graphics"></param>
        /// <param name="maxSprites">The maximum number of sprites which can be batched.</param>
		public SpriteRenderer(GraphicsDevice graphics, int maxSprites)
        {
            if (graphics == null)
                throw new ArgumentNullException("graphics");

            if (maxSprites <= 0)
                throw new ArgumentOutOfRangeException("maxSprites", "MaxSprites must be >= 1.");

            this.graphics = graphics;

            this.vertices = new Vertex[maxSprites * 4];

            var vertexDeclaration = new VertexDeclaration(new VertexElement[]
            {
                new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0),
                new VertexElement(12, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 0),
                new VertexElement(20, VertexElementFormat.Color, VertexElementUsage.Color, 0)
            });

            this.vertexBuffer = new VertexBuffer(this.graphics, vertexDeclaration, this.vertices.Length, BufferUsage.WriteOnly);

            ushort[] indices = new ushort[maxSprites * 6];
            for (ushort i = 0, vertex = 0; i < indices.Length; i += 6, vertex += 4)
            {
                indices[i] = vertex;
                indices[i + 1] = (ushort)(vertex + 1);
                indices[i + 2] = (ushort)(vertex + 3);
                indices[i + 3] = (ushort)(vertex + 1);
                indices[i + 4] = (ushort)(vertex + 2);
                indices[i + 5] = (ushort)(vertex + 3);
            }

            this.indexBuffer = new IndexBuffer(this.graphics, IndexElementSize.SixteenBits, indices.Length, BufferUsage.WriteOnly);
            this.indexBuffer.SetData(indices);

            this.viewMatrix = new Matrix(
                1.0f, 0.0f, 0.0f, 0.0f,
                0.0f, -1.0f, 0.0f, 0.0f,
                0.0f, 0.0f, -1.0f, 0.0f,
                0.0f, 0.0f, 0.0f, 1.0f);

            var assemblyDir = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            this.shader = new Effect(this.graphics, File.ReadAllBytes(Path.Combine(assemblyDir, "SpriteRenderer.fxb")));
        }

        public void Dispose()
        {
            this.vertexBuffer.Dispose();
            this.indexBuffer.Dispose();
        }

        private void EnsureDrawInProgress()
        {
            if (!this.drawInProgress)
                throw new InvalidOperationException("Draw not currently in progress.");
        }

        public void Begin(/*ISpriteShaderProgram shader*/)
        {
            //if (shader == null)
            //    throw new ArgumentNullException("shader");

            if (this.drawInProgress)
                throw new InvalidOperationException("Draw already in progress.");

            //this.shader = shader;

            var blendState = new BlendState();
            this.graphics.BlendState = BlendState.AlphaBlend;
            this.graphics.SamplerStates[0] = SamplerState.PointWrap;
            this.graphics.DepthStencilState = DepthStencilState.Default;
            this.graphics.RasterizerState = RasterizerState.CullNone;
                        
            this.drawInProgress = true;
        }

        public void End()
        {
            this.EnsureDrawInProgress();

            this.Flush();

            //this.shader = null;
            this.drawInProgress = false;
        }

        private Vector2 CalculateUV(float x, float y)
        {
            Vector2 uv = Vector2.Zero;

            if (this.texture.Width != 1 || this.texture.Height != 1)
            {
                uv = new Vector2(x / (float)this.texture.Width, y / (float)this.texture.Height);
            }

            return uv;
        }

        private void AddQuad(
            ref Vector2 topLeft,
            ref Vector2 topRight,
            ref Vector2 bottomRight,
            ref Vector2 bottomLeft,
            Rectangle source,
            Color color,
            float layerDepth)
        {
            if (this.vertexCount == this.vertices.Length)
                this.Flush();

            this.vertices[this.vertexCount].Position = new Vector3(topLeft, layerDepth);
            this.vertices[this.vertexCount + 1].Position = new Vector3(topRight, layerDepth);
            this.vertices[this.vertexCount + 2].Position = new Vector3(bottomRight, layerDepth);
            this.vertices[this.vertexCount + 3].Position = new Vector3(bottomLeft, layerDepth);

            this.vertices[this.vertexCount].UV = this.CalculateUV(source.Left, source.Top);
            this.vertices[this.vertexCount + 1].UV = this.CalculateUV(source.Right, source.Top);
            this.vertices[this.vertexCount + 2].UV = this.CalculateUV(source.Right, source.Bottom);
            this.vertices[this.vertexCount + 3].UV = this.CalculateUV(source.Left, source.Bottom);

            this.vertices[this.vertexCount].Color = color;
            this.vertices[this.vertexCount + 1].Color = color;
            this.vertices[this.vertexCount + 2].Color = color;
            this.vertices[this.vertexCount + 3].Color = color;

            this.vertexCount += 4;
        }

        public void Draw(
            Texture2D texture,
            Vector2 destination,
            Rectangle? source = null,
            Color? tint = null,
            Vector2? origin = null,
            Vector2? scale = null,
            float rotation = 0.0f,
            float layerDepth = 0.5f)
        {
            if (texture == null)
                throw new ArgumentNullException("texture");

            this.DrawInternal(
                texture,
                destination,
                source != null ? source.Value.Width : texture.Width,
                source != null ? source.Value.Height : texture.Height,
                source,
                tint,
                origin,
                scale,
                rotation,
                layerDepth);
        }

        public void Draw(
            Texture2D texture,
            Rectangle destination,
            Rectangle? source = null,
            Color? tint = null,
            Vector2? origin = null,
            Vector2? scale = null,
            float rotation = 0.0f,
            float layerDepth = 0.5f)
        {
            this.DrawInternal(
                texture,
                new Vector2(destination.X, destination.Y),
                destination.Width,
                destination.Height,
                source,
                tint,
                origin,
                scale,
                rotation,
                layerDepth);
        }

        //public void Draw(
        //    SpriteSheet spriteSheet,
        //    int frame,
        //    Vector2 position,
        //    Color? tint = null,
        //    Vector2? origin = null,
        //    Vector2? scale = null,
        //    float rotation = 0.0f,
        //    float layerDepth = 0.0f)
        //{
        //    if (spriteSheet == null)
        //        throw new ArgumentNullException("spriteSheet");

        //    Rectangle frameRect = spriteSheet[frame];

        //    this.DrawInternal(
        //        spriteSheet.Texture,
        //        position,
        //        frameRect.Width,
        //        frameRect.Height,
        //        frameRect,
        //        tint,
        //        origin,
        //        scale,
        //        rotation,
        //        layerDepth);
        //}

        private void DrawInternal(
            Texture2D texture,
            Vector2 destination,
            int width,
            int height,
            Rectangle? source,
            Color? tint,
            Vector2? origin,
            Vector2? scale,
            float rotation,
            float layerDepth)
        {
            this.EnsureDrawInProgress();

            if (texture == null)
                throw new ArgumentNullException("texture");

            if (texture != this.texture)
                this.Flush();

            this.texture = texture;

            if (source == null)
                source = new Rectangle(0, 0, texture.Width, texture.Height);

            if (tint == null)
                tint = Color.White;

            if (origin == null)
                origin = Vector2.Zero;

            if (scale == null)
                scale = Vector2.One;

            Vector2 topLeft = new Vector2(-origin.Value.X, -origin.Value.Y);
            Vector2 topRight = new Vector2(width - origin.Value.X, -origin.Value.Y);
            Vector2 bottomRight = new Vector2(width - origin.Value.X, height - origin.Value.Y);
            Vector2 bottomLeft = new Vector2(-origin.Value.X, height - origin.Value.Y);

            Matrix rotationMatrix;
            Matrix.CreateRotationZ(rotation, out rotationMatrix);

            Matrix scaleMatrix;
            Matrix.CreateScale(scale.Value.X, scale.Value.Y, 1.0f, out scaleMatrix);

            Matrix translationMatrix;
            Matrix.CreateTranslation(destination.X, destination.Y, layerDepth, out translationMatrix);

            Matrix identity = Matrix.Identity;

            Matrix transform1;
            Matrix.Multiply(ref identity, ref scaleMatrix, out transform1);

            Matrix transform2;
            Matrix.Multiply(ref transform1, ref rotationMatrix, out transform2);

            Matrix transform3;
            Matrix.Multiply(ref transform2, ref translationMatrix, out transform3);

            Vector2.Transform(ref topLeft, ref transform3, out var topLeft2);
            Vector2.Transform(ref topRight, ref transform3, out var topRight2);
            Vector2.Transform(ref bottomRight, ref transform3, out var bottomRight2);
            Vector2.Transform(ref bottomLeft, ref transform3, out var bottomLeft2);

            this.AddQuad(
                ref topLeft2,
                ref topRight2,
                ref bottomRight2,
                ref bottomLeft2,
                source.Value,
                tint.Value,
                layerDepth);
        }

        //public Vector2 DrawString(
        //    TextureFont font,
        //    string text,
        //    Vector2 position,
        //    Color4? tint = null,
        //    Vector2? origin = null,
        //    Vector2? scale = null,
        //    float rotation = 0.0f,
        //    float layerDepth = 0.0f)
        //{
        //    if (font == null)
        //        throw new ArgumentNullException("font");

        //    if (text == null)
        //        throw new ArgumentNullException("text");

        //    if (text.Length == 0)
        //        return position;

        //    Size textSize = font.MeasureString(text);

        //    return this.DrawString(font, text, new Rectangle((int)position.X, (int)position.Y, textSize.Width, textSize.Height), tint, origin, scale, rotation, layerDepth);
        //}

        //public Vector2 DrawString(
        //    TextureFont font,
        //    string text,
        //    Rectangle destination,
        //    Color4? tint = null,
        //    Vector2? origin = null,
        //    Vector2? scale = null,
        //    float rotation = 0.0f,
        //    float layerDepth = 0.0f)
        //{
        //    if (font == null)
        //        throw new ArgumentNullException("font");

        //    if (text == null)
        //        throw new ArgumentNullException("text");

        //    if (text.Length == 0)
        //        return new Vector2(destination.X, destination.Y);

        //    if (tint == null)
        //        tint = Color4.White;

        //    if (origin == null)
        //        origin = Vector2.Zero;

        //    if (scale == null)
        //        scale = Vector2.One;

        //    float heightOfSingleLine = font.LineHeight * scale.Value.Y;

        //    if (heightOfSingleLine > destination.Height) // We can't draw anything
        //        return new Vector2(destination.X, destination.Y);

        //    Vector2 cursor = new Vector2(destination.X, destination.Y);

        //    for (int i = 0; i < text.Length; i++)
        //    {
        //        // Skip characters we can't render.
        //        if (text[i] == '\r')
        //            continue;

        //        float widthOfChar = 0;

        //        if (text[i] == '\n' || cursor.X + (widthOfChar = font[text[i]].Width * scale.Value.X) > destination.Right)
        //        {
        //            cursor.X = destination.X;
        //            cursor.Y += heightOfSingleLine + font.LineSpacing;

        //            // If the next line extends past the destination, quit.
        //            if (cursor.Y + heightOfSingleLine > destination.Bottom)
        //                return cursor;

        //            // We can't render a new line.
        //            if (text[i] == '\n')
        //                continue;
        //        }

        //        Vector2 characterOrigin = origin.Value;
        //        characterOrigin.X -= cursor.X - destination.X;
        //        characterOrigin.Y -= cursor.Y - destination.Y;

        //        Rectangle letterSource = font[text[i]];
        //        Rectangle letterDestination = new Rectangle((int)cursor.X + (int)characterOrigin.X, (int)cursor.Y + (int)characterOrigin.Y, (int)widthOfChar, (int)heightOfSingleLine);

        //        this.Draw(
        //            font.Texture,
        //            letterDestination,
        //            letterSource,
        //            tint,
        //            characterOrigin,
        //            scale,
        //            rotation,
        //            layerDepth);

        //        cursor.X += widthOfChar + font.CharacterSpacing;
        //    }

        //    return cursor;
        //}

        private void Flush()
        {
            if (this.vertexCount > 0)
            {
                var viewport = this.graphics.Viewport;
                this.projectionMatrix = Matrix.CreateOrthographicOffCenter(0, viewport.Width, -viewport.Height, 0, 0, 1);

                this.vertexBuffer.SetData(this.vertices, 0, this.vertexCount);

                this.shader.Parameters["Transform"].SetValue(Matrix.Identity * this.viewMatrix * this.projectionMatrix);
                this.shader.Parameters["Texture"].SetValue(this.texture);

                this.graphics.SetVertexBuffer(this.vertexBuffer);
                this.graphics.Indices = this.indexBuffer;

                RasterizerState rasterizerState = new RasterizerState();
                rasterizerState.CullMode = CullMode.None;
                this.graphics.RasterizerState = rasterizerState;

                foreach (var pass in this.shader.CurrentTechnique.Passes)
                {
                    pass.Apply();

                    this.graphics.DrawIndexedPrimitives(
                        PrimitiveType.TriangleList,
                        0,
                        0,
                        this.vertexCount,
                        0,
                        (this.vertexCount / 4) * 2);
                }

                this.vertexCount = 0;
            }
        }
    }
}
