using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace IV.Action_Scene.Effects
{

    public struct Quad
    {
        public Vector3 Origin;
        public Vector3 UpperLeft;
        public Vector3 LowerLeft;
        public Vector3 UpperRight;
        public Vector3 LowerRight;
        public Vector3 Normal;
        public Vector3 Up;
        public Vector3 Left;

        public VertexPositionNormalTexture[] Vertices;
        public short[] Indexes;

        public Quad(Vector3 origin, Vector3 normal, Vector3 up, float width, float height)
        {
            Vertices = new VertexPositionNormalTexture[4];
            
            Indexes = new short[6];
            Origin = origin;
            Normal = normal;
            Up = up;

            // Calculate the quad corners
            Left = Vector3.Cross(normal, Up);
            Vector3 uppercenter = (Up * height / 2) + origin;
            UpperLeft = uppercenter + (Left * width / 2);
            UpperRight = uppercenter - (Left * width / 2);
            LowerLeft = UpperLeft - (Up * height);
            LowerRight = UpperRight - (Up * height);

            FillVertices(ref Vertices, 0, ref Indexes, 0, Normal);
        }

        private void FillVertices(ref VertexPositionNormalTexture[] vertices, 
            int vertexOffset,
            ref short[] Indices,
            int indexOffset, 
            Vector3 normal)
        {
            // Fill in texture coordinates to display full texture
            // on quad
            var textureUpperLeft = new Vector2(0.0f, 0.0f);
            var textureUpperRight = new Vector2(1.0f, 0.0f);
            var textureLowerLeft = new Vector2(0.0f, 1.0f);
            var textureLowerRight = new Vector2(1.0f, 1.0f);

            // Provide a normal for each vertex
            for (int i = 0; i < vertices.Length; i++)
            {
                vertices[i].Normal = normal;
            }

            // Set the position and texture coordinate for each
            // vertex
            vertices[vertexOffset].Position = LowerLeft;
            vertices[vertexOffset].TextureCoordinate = textureLowerLeft;
            vertices[vertexOffset + 1].Position = UpperLeft;
            vertices[vertexOffset + 1].TextureCoordinate = textureUpperLeft;
            vertices[vertexOffset + 2].Position = LowerRight;
            vertices[vertexOffset + 2].TextureCoordinate = textureLowerRight;
            vertices[vertexOffset + 3].Position = UpperRight;
            vertices[vertexOffset + 3].TextureCoordinate = textureUpperRight;

            // Set the index buffer for each vertex, using
            // clockwise winding
            Indices[indexOffset] = 0;
            Indices[indexOffset+1] = 1;
            Indices[indexOffset+2] = 2;
            Indices[indexOffset+3] = 2;
            Indices[indexOffset+4] = 1;
            Indices[indexOffset+5] = 3;
        }
    }

}
