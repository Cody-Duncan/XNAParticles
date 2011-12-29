#region File Header
/*////////////////////////////////////////////////////////////////////////
 *  Camera.cs
 *  Project: Particle Test 2
 *  Date: December 28th, 2011
 *  Requirements: Visual Studio 2008, XNA 3.1, Shader Model 3.0
 *  Description: Camera to view the particles being drawn in 3D space.
 *  References:
 *  borrows entirely from: http://www.catalinzima.com/tutorials/4-uses-of-vtf/particle-systems/
 *  
 * Controls:
 * up,down,left,right, z x - camera controls, but using these throws off the mapping of clicks to attraction/repel points.
 *      probably need to fix that later, but its fine for a proof of concept. 
 * 
///////////////////////////////////////////////////////////////////////*/
#endregion

#region Using Statements
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
#endregion

namespace ParticleTest2
{
    /// <summary>
    /// This is a game component that implements IUpdateable.
    /// </summary>
    public class Camera : Microsoft.Xna.Framework.GameComponent
    {
        private float cameraArc = -90;

        public float CameraArc
        {
            get { return cameraArc; }
            set { cameraArc = value; }
        }

        private float cameraRotation = 180;

        public float CameraRotation
        {
            get { return cameraRotation; }
            set { cameraRotation = value; }
        }

        private float cameraDistance = 1940;

        public float CameraDistance
        {
            get { return cameraDistance; }
            set { cameraDistance = value; }
        }

        public Matrix View
        {
            get
            {
                Matrix view = Matrix.CreateTranslation(0, -10, 0) *
                      Matrix.CreateRotationY(MathHelper.ToRadians(cameraRotation)) *
                      Matrix.CreateRotationX(MathHelper.ToRadians(cameraArc)) *
                      Matrix.CreateLookAt(new Vector3(0, 0, -cameraDistance),
                                          new Vector3(0, 0, 0), Vector3.Up);
                return view;
            }
        }

        public Matrix Projection
        {
            get
            {

                float aspectRatio = (float)Game.Window.ClientBounds.Width /
                                (float)Game.Window.ClientBounds.Height;
                Matrix projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4,
                                                                        aspectRatio,
                                                                        1,
                                                                        10000);
                return projection;
            }
        }

        KeyboardState currentKeyboardState = new KeyboardState();
        GamePadState currentGamePadState = new GamePadState();

        public Camera(Game game)
            : base(game)
        {
            // TODO: Construct any child components here
        }

        /// <summary>
        /// Allows the game component to perform any initialization it needs to before starting
        /// to run.  This is where it can query for any required services and load content.
        /// </summary>
        public override void Initialize()
        {
            // TODO: Add your initialization code here

            base.Initialize();
        }


        /// <summary>
        /// Allows the game component to update itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public override void Update(GameTime gameTime)
        {

            currentKeyboardState = Keyboard.GetState();
            currentGamePadState = GamePad.GetState(PlayerIndex.One);

            // TODO: Add your update code here

            float time = (float)gameTime.ElapsedGameTime.TotalMilliseconds;

            // Check for input to rotate the camera up and down around the model.
            if (currentKeyboardState.IsKeyDown(Keys.Up) ||
                currentKeyboardState.IsKeyDown(Keys.W))
            {
                cameraArc += time * 0.1f;
            }

            if (currentKeyboardState.IsKeyDown(Keys.Down) ||
                currentKeyboardState.IsKeyDown(Keys.S))
            {
                cameraArc -= time * 0.1f;
            }

            cameraArc += currentGamePadState.ThumbSticks.Right.Y * time * 0.05f;

            // Limit the arc movement.
            if (cameraArc > 90.0f)
                cameraArc = 90.0f;
            else if (cameraArc < -90.0f)
                cameraArc = -90.0f;

            // Check for input to rotate the camera around the model.
            if (currentKeyboardState.IsKeyDown(Keys.Right) ||
                currentKeyboardState.IsKeyDown(Keys.D))
            {
                cameraRotation += time * 0.1f;
            }

            if (currentKeyboardState.IsKeyDown(Keys.Left) ||
                currentKeyboardState.IsKeyDown(Keys.A))
            {
                cameraRotation -= time * 0.1f;
            }

            cameraRotation += currentGamePadState.ThumbSticks.Right.X * time * 0.05f;

            // Check for input to zoom camera in and out.
            if (currentKeyboardState.IsKeyDown(Keys.Z))
                cameraDistance += time * 0.25f;

            if (currentKeyboardState.IsKeyDown(Keys.X))
                cameraDistance -= time * 0.25f;

            cameraDistance += currentGamePadState.Triggers.Left * time * 0.25f;
            cameraDistance -= currentGamePadState.Triggers.Right * time * 0.25f;

            // Limit the arc movement.
            if (cameraDistance > 2500.0f)
                cameraDistance = 2500.0f;
            else if (cameraDistance < 10.0f)
                cameraDistance = 10.0f;

            if (currentGamePadState.Buttons.RightStick == ButtonState.Pressed ||
                currentKeyboardState.IsKeyDown(Keys.R))
            {
                cameraArc = -30;
                cameraRotation = 0;
                cameraDistance = 100;
            }

            base.Update(gameTime);
        }
    }
}


