using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using VelcroPhysics.Dynamics;
using VelcroPhysics.Dynamics.Joints;
using VelcroPhysics.Extensions.Controllers.Wind;
using VelcroPhysics.Factories;
using VelcroPhysics.Utilities;

namespace VelcroPhysics.Samples.HelloWorld
{
    public class Game1 : Game
    {
        private readonly GraphicsDeviceManager _graphics;
        private SpriteBatch _batch;
        private KeyboardState _oldKeyState;
        private GamePadState _oldPadState;
        private SpriteFont _font;
        private float _moveMod;

        private readonly World _world;

        private Body _circleBody;
        private Body _groundBody;

        private Texture2D _circleSprite;
        private Texture2D _groundSprite;

        // Simple camera controls
        private Matrix _view;

        private Vector2 _cameraPosition;
        private Vector2 _screenCenter;
        private Vector2 _groundOrigin;
        private Vector2 _circleOrigin;

        private FrictionJoint _joint;

#if !XBOX360

        private const string Text = "Press A or D to rotate the ball\n" +
                                    "Press Space to jump\n" +
                                    "Use arrow keys to move the camera";

#else
                const string Text = "Use left stick to move\n" +
                                    "Use right stick to move camera\n" +
                                    "Press A to jump\n";
#endif

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            _graphics.PreferredBackBufferWidth = 1920;
            _graphics.PreferredBackBufferHeight = 1080;

            _moveMod = 1f;

            Content.RootDirectory = "Content";

            //Create a world with gravity.
            _world = new World(new Vector2(0, 0));
        }

        protected override void LoadContent()
        {
            // Initialize camera controls
            _view = Matrix.Identity;
            _cameraPosition = Vector2.Zero;
            _screenCenter = new Vector2(_graphics.GraphicsDevice.Viewport.Width / 2f, _graphics.GraphicsDevice.Viewport.Height / 2f);
            _batch = new SpriteBatch(_graphics.GraphicsDevice);

            _font = Content.Load<SpriteFont>("font");

            // Load sprites
            _circleSprite = Content.Load<Texture2D>("CircleSprite"); //  96px x 96px => 1.5m x 1.5m
                                                                     //            _groundSprite = Content.Load<Texture2D>("GroundSprite"); // 512px x 64px =>   8m x 1m

            /* We need XNA to draw the ground and circle at the center of the shapes */
            //            _groundOrigin = new Vector2(_groundSprite.Width / 2f, _groundSprite.Height / 2f);
            _circleOrigin = new Vector2(_circleSprite.Width / 2f, _circleSprite.Height / 2f);

            // Velcro Physics expects objects to be scaled to MKS (meters, kilos, seconds)
            // 1 meters equals 64 pixels here
            ConvertUnits.SetDisplayUnitToSimUnitRatio(64f);

            /* Circle */
            // Convert screen center from pixels to meters
            Vector2 circlePosition = ConvertUnits.ToSimUnits(_screenCenter) + new Vector2(0, -1.5f);

            // Create the circle fixture
            _circleBody = BodyFactory.CreateCircle(_world, ConvertUnits.ToSimUnits(96 / 2f), 1f, circlePosition, BodyType.Dynamic);

            // Give it some bounce and friction
            _circleBody.Restitution = 0.3f;
            _circleBody.Friction = 0.5f;


            /* Ground */
            //            Vector2 groundPosition = ConvertUnits.ToSimUnits(_screenCenter) + new Vector2(0, 1.25f);

            // Create the ground fixture
            //            _groundBody = BodyFactory.CreateRectangle(_world, ConvertUnits.ToSimUnits(512f), ConvertUnits.ToSimUnits(64f), 1f, groundPosition);
            //            _groundBody.BodyType = BodyType.Static;
            //            _groundBody.Restitution = 0.3f;
            //            _groundBody.Friction = 0.5f;

            var floor = BodyFactory.CreateBody(_world);

            _joint = JointFactory.CreateFrictionJoint(_world, floor, _circleBody);
            _joint.MaxForce = 5;
            _joint.MaxTorque = 5;
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            HandleGamePad();
            HandleKeyboard();

            //We update the world
            _world.Step((float)gameTime.ElapsedGameTime.TotalMilliseconds * 0.001f);

            base.Update(gameTime);
        }

        private void HandleGamePad()
        {
            GamePadState padState = GamePad.GetState(0);

            if (padState.IsConnected)
            {
                if (padState.Buttons.A == ButtonState.Pressed && _oldPadState.Buttons.A == ButtonState.Released)
                    _moveMod += 0.5f;

                if (padState.Buttons.B == ButtonState.Pressed && _oldPadState.Buttons.B == ButtonState.Released)
                    _moveMod -= 0.5f;

                if (padState.Buttons.X == ButtonState.Pressed && _oldPadState.Buttons.X == ButtonState.Released)
                    _joint.MaxForce += 0.5f;

                if (padState.Buttons.Y == ButtonState.Pressed && _oldPadState.Buttons.Y == ButtonState.Released)
                    _joint.MaxForce -= 0.5f;




                if (padState.Buttons.Back == ButtonState.Pressed)
                    Exit();

                //                if (padState.Buttons.A == ButtonState.Pressed && _oldPadState.Buttons.A == ButtonState.Released)
                //                    _circleBody.ApplyLinearImpulse(new Vector2(0, -10));

                _circleBody.ApplyForce(padState.ThumbSticks.Left * _moveMod);
                _cameraPosition.X -= padState.ThumbSticks.Right.X;
                _cameraPosition.Y += padState.ThumbSticks.Right.Y;

                _view = Matrix.CreateTranslation(new Vector3(_cameraPosition - _screenCenter, 0f)) * Matrix.CreateTranslation(new Vector3(_screenCenter, 0f));

                _oldPadState = padState;
            }
        }

        private void HandleKeyboard()
        {
            KeyboardState state = Keyboard.GetState();
            if (state.IsKeyUp(Keys.D1) && _oldKeyState.IsKeyDown(Keys.D1))
            {
                _moveMod = 100f;
                _circleBody.Mass = 2.5f;
                _joint.MaxForce = 80f;
            }



            if (state.IsKeyUp(Keys.I) && _oldKeyState.IsKeyDown(Keys.I))
                _moveMod += 0.5f;

            if (state.IsKeyUp(Keys.U) && _oldKeyState.IsKeyDown(Keys.U))
                _moveMod -= 0.5f;

            if (state.IsKeyUp(Keys.Y) && _oldKeyState.IsKeyDown(Keys.Y))
                _moveMod += 5f;

            if (state.IsKeyUp(Keys.T) && _oldKeyState.IsKeyDown(Keys.T))
                _moveMod -= 5f;

            if (state.IsKeyUp(Keys.K) && _oldKeyState.IsKeyDown(Keys.K))
                _joint.MaxForce += 0.5f;

            if (state.IsKeyUp(Keys.J) && _oldKeyState.IsKeyDown(Keys.J))
                _joint.MaxForce -= 0.5f;

            if (state.IsKeyUp(Keys.H) && _oldKeyState.IsKeyDown(Keys.H))
                _joint.MaxForce += 5f;

            if (state.IsKeyUp(Keys.G) && _oldKeyState.IsKeyDown(Keys.G))
                _joint.MaxForce -= 5f;

            if (state.IsKeyUp(Keys.M) && _oldKeyState.IsKeyDown(Keys.M))
                _circleBody.Mass += 0.1f;

            if (state.IsKeyUp(Keys.N) && _oldKeyState.IsKeyDown(Keys.N))
                _circleBody.Mass -= 0.1f;


            //            // Move camera
            //            if (state.IsKeyDown(Keys.Left))
            //                _cameraPosition.X += 1.5f;
            //
            //            if (state.IsKeyDown(Keys.Right))
            //                _cameraPosition.X -= 1.5f;
            //
            //            if (state.IsKeyDown(Keys.I))
            //                _joint.MaxForce += 0.5f;
            //
            //            if (state.IsKeyDown(Keys.K))
            //                _joint.MaxForce -= 0.5f;

            _view = Matrix.CreateTranslation(new Vector3(_cameraPosition - _screenCenter, 0f)) * Matrix.CreateTranslation(new Vector3(_screenCenter, 0f));

            var force = new Vector2(0, 0);
            // We make it possible to rotate the circle body
            if (state.IsKeyDown(Keys.A))
                force = Vector2.Add(force, Vector2.UnitX * -1);
            //                _circleBody.ApplyForce(new Vector2(-1,0) * _moveMod);

            if (state.IsKeyDown(Keys.D))
                force = Vector2.Add(force, Vector2.UnitX);
            //            _circleBody.ApplyForce(new Vector2(1,0) * _moveMod);

            if (state.IsKeyDown(Keys.W))
                force = Vector2.Add(force, Vector2.UnitY * -1);

            //                _circleBody.ApplyForce(new Vector2(0,-1) * _moveMod);

            if (state.IsKeyDown(Keys.S))
                force = Vector2.Add(force, Vector2.UnitY);
            //            _circleBody.ApplyForce(new Vector2(0, 1) * _moveMod);

            force /= force.Length() == 0 ? 1 : force.Length();
            if (force != Vector2.Zero)
                _circleBody.ApplyForce(force * _moveMod);

            if (state.IsKeyDown(Keys.Escape))
                Exit();

            _oldKeyState = state;
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            //Draw circle and ground
            _batch.Begin(SpriteSortMode.Deferred, null, null, null, null, null, _view);
            _batch.Draw(_circleSprite, ConvertUnits.ToDisplayUnits(_circleBody.Position), null, Color.White, _circleBody.Rotation, _circleOrigin, 1f, SpriteEffects.None, 0f);
            //            _batch.Draw(_groundSprite, ConvertUnits.ToDisplayUnits(_groundBody.Position), null, Color.White, 0f, _groundOrigin, 1f, SpriteEffects.None, 0f);
            _batch.End();

            // Display instructions
            _batch.Begin();
            _batch.DrawString(_font, Text, new Vector2(14f, 14f), Color.Black);
            _batch.DrawString(_font, Text, new Vector2(12f, 12f), Color.White);
            _batch.DrawString(_font, $"Joint MaxForce: {_joint.MaxForce}", new Vector2(50f, 220f), Color.White);
            _batch.DrawString(_font, $"Move Multiplier: {_moveMod}", new Vector2(50f, 200f), Color.Black);
            _batch.DrawString(_font, $"Mass: {_circleBody.Mass}", new Vector2(50f, 260f), Color.Black);

            _batch.DrawString(_font, $"Velocity: {_circleBody.LinearVelocity}", new Vector2(50f, 240f), Color.White);

            _batch.End();

            base.Draw(gameTime);
        }
    }
}