using System;
using System.Collections.Generic;
using System.Text;
using System.Numerics;
using Microsoft.Xna.Framework;

using Vector2 = System.Numerics.Vector2;
using Vector3 = System.Numerics.Vector3;

namespace FnaSandbox
{
    public struct Vertex
    {
        public Vector3 Position;

        public Vector2 UV;

        public Color Color;

        public Vertex(float x, float y, float z, float u, float v, Color color)
        {
            this.Position = new Vector3(x, y, z);
            this.UV = new Vector2(u, v);
            this.Color = color;
        }
    }
}
