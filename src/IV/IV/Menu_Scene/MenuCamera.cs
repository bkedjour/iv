using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace IV.Menu_Scene
{
    class MenuCamera
    {
        public Vector3 Position { get; set; }

        private float yaw;
        private float pitch;

        public float Yaw
        {
            get { return yaw; }
            set { yaw = MathHelper.WrapAngle(value); }
        }

        public float Pitch
        {
            get { return pitch; }
            set { pitch = MathHelper.Clamp(value, -MathHelper.PiOver2, MathHelper.PiOver2); }
        }


        public float Speed { get; set; }

        public Matrix WorldMatrix { get; set; }
        public Matrix ViewMatrix { get; protected set; }
        public Matrix ProjectionMatrix { get; set; }

        public bool UseMovementControls = true;


        public float aspectRatio { get; private set; }

        public MenuCamera(Vector3 position, float yaw, float pitch, float speed, float aspectRation)
        {
            Position = position;
            Yaw = yaw;
            Pitch = pitch;
            Speed = speed;
            aspectRatio = aspectRation;
            ProjectionMatrix = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4,
                                                                   aspectRation, 1, 2000);

        }
        public void Update(GameTime gameTime, KeyboardState keyboardState, MouseState mouseState)
        {
            var dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
            Yaw += (200 - mouseState.X) * dt * .12f;
            Pitch += (200 - mouseState.Y) * dt * .12f;

            if (keyboardState.IsKeyDown(Keys.R))
                Yaw = Pitch = 0;

            WorldMatrix = Matrix.CreateFromAxisAngle(Vector3.Right, Pitch) * Matrix.CreateFromAxisAngle(Vector3.Up, Yaw);
            WorldMatrix *= Matrix.CreateTranslation(Position);

            ViewMatrix = Matrix.Invert(WorldMatrix);

            var distance = Speed * dt;

            if (keyboardState.IsKeyDown(Keys.E))
                MoveForward(distance);
            if (keyboardState.IsKeyDown(Keys.D))
                MoveForward(-distance);
            if (keyboardState.IsKeyDown(Keys.F))
                MoveRight(distance);
            if (keyboardState.IsKeyDown(Keys.S))
                MoveRight(-distance);
            if (keyboardState.IsKeyDown(Keys.Z))
                MoveUp(-distance);
            if (keyboardState.IsKeyDown(Keys.A))
                MoveUp(distance);
        }
        void MoveForward(float distance)
        {
            Position += WorldMatrix.Forward * distance;
        }

        void MoveRight(float distance)
        {
            Position += WorldMatrix.Right * distance;
        }

        void MoveUp(float distance)
        {
            Position += new Vector3(0, distance, 0);
        }
    }
}
