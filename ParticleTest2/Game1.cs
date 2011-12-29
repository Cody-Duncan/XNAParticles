#region File Header
/*////////////////////////////////////////////////////////////////////////
 *  Game1.cs
 *  Project: Particle Test 2
 *  Author: Cody Duncan
 *  Date: December 28th, 2011
 *  Requirements: Visual Studio 2008, XNA 3.1, Shader Model 3.0
 *  Description: Stateful particle system that includes collission with walls, not with other particles.
 *  References:
 *  http://www.gamasutra.com/view/feature/2122/building_a_millionparticle_system.php
 *  borrows some source: http://www.catalinzima.com/tutorials/4-uses-of-vtf/particle-systems/
 *  credit for ParticlePhysics.fx and Particle.fx shaders also goes to Catalin Zima, though several
 *  modifications were made to allow for attractors and gravitation.
 *  
 * Controls:
 *  1 - no gravity
 *  2 - downward gravity
 *  3 - gravity at center
 *  left-click - attract to cursor
 *  right-click - repel from cursor
 *  enter - reset simulation (will start particles on rotary trajectories if gravity is at center)
 *  
 * up,down,left,right, z x - camera controls, but using these throws off the mapping of clicks to attraction/repel points.
 *      probably need to fix that later, but its fine for a proof of concept. 
 *  inspired by : http://www.youtube.com/watch?v=ACHJ2rGyP10 (dhscaresme) and http://www.youtube.com/watch?v=CyAZ2Y7nOTw (GearGOD)
 * 
///////////////////////////////////////////////////////////////////////*/
#endregion

#region Using Statements
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Storage;
#endregion

namespace ParticleTest2
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        Camera camera;

        Texture2D randomTexture;    //used to store a series of random values, that will be accessed from various shaders
        Texture2D particleTexture;  //texture used to draw the particles

        RenderTarget2D positionRT;  // render target that will hold the positions of the particles
        RenderTarget2D velocityRT;  // render target that will hold the velocities of the particles
        RenderTarget2D temporaryRT; // temporary render target, needed when updating the other render targets
        DepthStencilBuffer simulationDepthBuffer; // depth buffer of the same size as the render targets, used when updating the particle system

        VertexBuffer particlesVB;  // vertex buffer that will hold the particle system's vertices.
        Effect renderParticleEffect; // effect file used to render the particles
        Effect physicsEffect;        // effect file used to update the physics (position and velocities)
        SpriteBatch spriteBatch;     // sprite batch used for 2D drawing
        Boolean isPhysicsReset = false;  // true if the physics was reset. If false, the velocity and position textures will be reset to initial values
        int particleCount = 512;     // dimension for render targets. We will have particleCount * particleCount particles.
        bool isSuperPullOn = false;
        int pullStrength = 100;
        bool isDownGravityOn = false; //toggle down gravity
        bool isCenterGravityOn = false; //toggle center gravity

        IInputHandler inputHandler;

        int worldHalfHeight;  // screenBorderFromCenterY in ParticlePhysicsfx
        int worldHalfWidth;   // screenBorderFromCenterX in ParticlePhysicsfx
        int worldHeight;
        int worldWidth;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            this.IsFixedTimeStep = true;
            //graphics.IsFullScreen = true;

            camera = new Camera(this);
            Components.Add(camera);

            InputHandler tempHandler = new InputHandler(this);
            inputHandler = tempHandler;
            Components.Add(tempHandler);
        }


        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // TODO: Add your initialization logic here
            InitGraphicsMode(1280, 720, false);
            base.Initialize();
        }

        /// <summary>
        /// Attempt to set the display mode to the desired resolution.  Itterates through the display
        /// capabilities of the default graphics adapter to determine if the graphics adapter supports the
        /// requested resolution.  If so, the resolution is set and the function returns true.  If not,
        /// no change is made and the function returns false.
        /// </summary>
        /// <param name="iWidth">Desired screen width.</param>
        /// <param name="iHeight">Desired screen height.</param>
        /// <param name="bFullScreen">True if you wish to go to Full Screen, false for Windowed Mode.</param>
        private bool InitGraphicsMode(int iWidth, int iHeight, bool bFullScreen)
        {
            // If we aren't using a full screen mode, the height and width of the window can
            // be set to anything equal to or smaller than the actual screen size.
            if (bFullScreen == false)
            {
                if ((iWidth <= GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width)
                    && (iHeight <= GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height))
                {
                    graphics.PreferredBackBufferWidth = iWidth;
                    graphics.PreferredBackBufferHeight = iHeight;
                    graphics.IsFullScreen = bFullScreen;
                    graphics.ApplyChanges();
                    return true;
                }
            }
            else
            {
                // If we are using full screen mode, we should check to make sure that the display
                // adapter can handle the video mode we are trying to set.  To do this, we will
                // iterate thorugh the display modes supported by the adapter and check them against
                // the mode we want to set.
                foreach (DisplayMode dm in GraphicsAdapter.DefaultAdapter.SupportedDisplayModes)
                {
                    // Check the width and height of each mode against the passed values
                    if ((dm.Width == iWidth) && (dm.Height == iHeight))
                    {
                        // The mode is supported, so set the buffer formats, apply changes and return
                        graphics.PreferredBackBufferWidth = iWidth;
                        graphics.PreferredBackBufferHeight = iHeight;
                        graphics.IsFullScreen = bFullScreen;
                        graphics.ApplyChanges();
                        return true;
                    }
                }
            }
            return false;
        }


        /// <summary>
        /// Load your graphics content.  If loadAllContent is true, you should
        /// load content from both ResourceManagementMode pools.  Otherwise, just
        /// load ResourceManagementMode.Manual content.
        /// </summary>
        /// <param name="loadAllContent">Which type of content to load.</param>
        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(graphics.GraphicsDevice);
            
            particleTexture = Content.Load<Texture2D>("Textures\\Flare");

            //load shaders
            physicsEffect = Content.Load<Effect>("Shaders\\ParticlePhysics");
            worldHalfHeight = physicsEffect.Parameters["screenBorderFromCenterY"].GetValueInt32();
            worldHalfWidth = physicsEffect.Parameters["screenBorderFromCenterX"].GetValueInt32();
            worldHeight = worldHalfHeight * 2;
            worldWidth = worldHalfWidth * 2;

            renderParticleEffect = Content.Load<Effect>("Shaders\\Particle");

            //initialize renderTargets
            temporaryRT = new RenderTarget2D(graphics.GraphicsDevice, particleCount, particleCount, 1, SurfaceFormat.Vector4, MultiSampleType.None, 0);
            positionRT = new RenderTarget2D(graphics.GraphicsDevice, particleCount, particleCount, 1, SurfaceFormat.Vector4, MultiSampleType.None, 0);
            velocityRT = new RenderTarget2D(graphics.GraphicsDevice, particleCount, particleCount, 1, SurfaceFormat.Vector4, MultiSampleType.None, 0);
            simulationDepthBuffer = new DepthStencilBuffer(graphics.GraphicsDevice, particleCount, particleCount, graphics.GraphicsDevice.DepthStencilBuffer.Format);

            isPhysicsReset = false;

            //generate initial particles for vertexbuffer
            //the data isn't really used because it gets overwritten by the Particle.fx shader
            VertexPositionColor[] vertices = new VertexPositionColor[particleCount * particleCount];
            Random rand = new Random();
            for (int i = 0; i < particleCount; i++)
            {
                for (int j = 0; j < particleCount; j++)
                {
                    VertexPositionColor vert = new VertexPositionColor();

                    vert.Color = new Color(150, 150, (byte)(200 + rand.Next(50)));
                    vert.Position = new Vector3();
                    vert.Position.X = (float)i / (float)particleCount;
                    vert.Position.Y = (float)j / (float)particleCount;
                    vertices[i * particleCount + j] = vert;
                }
            }
            particlesVB = new VertexBuffer(graphics.GraphicsDevice, typeof(VertexPositionColor), particleCount * particleCount, BufferUsage.Points);
            particlesVB.SetData<VertexPositionColor>(vertices);

            //generate a random texture for initial particle locations and colors
            int textureSize = particleCount;
            randomTexture = new Texture2D(graphics.GraphicsDevice, textureSize, textureSize, 1, TextureUsage.None, SurfaceFormat.Vector4);
            Vector4[] pointsarray = new Vector4[textureSize * textureSize];
            for (int i = 0; i < textureSize * textureSize; i++)
            {
                pointsarray[i] = new Vector4();
                pointsarray[i].X = (float)rand.NextDouble() - 0.5f;
                pointsarray[i].Y = (float)rand.NextDouble() - 0.5f;
                pointsarray[i].Z = (float)rand.NextDouble() - 0.5f;  
                pointsarray[i].W = (float)rand.NextDouble() - 0.5f;
            }
            randomTexture.SetData<Vector4>(pointsarray);

            physicsEffect.Parameters["randomMap"].SetValue(randomTexture);
        }


        /// <summary>
        /// Unload your graphics content.  If unloadAllContent is true, you should
        /// unload content from both ResourceManagementMode pools.  Otherwise, just
        /// unload ResourceManagementMode.Manual content.  Manual content will get
        /// Disposed by the GraphicsDevice during a Reset.
        /// </summary>
        /// <param name="unloadAllContent">Which type of content to unload.</param>
        protected override void UnloadContent()
        {
                // TODO: Unload any ResourceManagementMode.Automatic content
                Content.Unload();
            

            // TODO: Unload any ResourceManagementMode.Manual content
        }


        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            // Allows the game to exit
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                this.Exit();

            checkMouseInput();
            checkKeyboardInput();

            base.Update(gameTime);
        }

        /// <summary>
        /// Uses the ParticlePhysics.fx shader to do physics calculations for velocity and position
        /// on the gpu.
        /// </summary>
        /// <param name="technique">the shader technique to apply</param>
        /// <param name="resultTarget">the rendertarget to copy the resulting data to</param>
        private void DoPhysicsPass(string technique, RenderTarget2D resultTarget)
        {
            //store old rendertarget
            RenderTarget2D oldRT = graphics.GraphicsDevice.GetRenderTarget(0) as RenderTarget2D;
            DepthStencilBuffer oldDS = graphics.GraphicsDevice.DepthStencilBuffer;

            //set render targets, clear, and choose technique
            graphics.GraphicsDevice.SetRenderTarget(0, temporaryRT);
            graphics.GraphicsDevice.DepthStencilBuffer = simulationDepthBuffer;
            graphics.GraphicsDevice.Clear(Color.White);
            physicsEffect.CurrentTechnique = physicsEffect.Techniques[technique];

            if (isPhysicsReset) //set if not already set
            {
                physicsEffect.Parameters["positionMap"].SetValue(positionRT.GetTexture());
                physicsEffect.Parameters["velocityMap"].SetValue(velocityRT.GetTexture());
            }

            //first operation
            //perform the shader operations
            spriteBatch.Begin(SpriteBlendMode.None, SpriteSortMode.Immediate, SaveStateMode.None);
            physicsEffect.Begin();

            physicsEffect.CurrentTechnique.Passes[0].Begin();
                spriteBatch.Draw(randomTexture, new Rectangle(0, 0, particleCount, particleCount), Color.White); //must draw something to get shader to go
                spriteBatch.End();

            physicsEffect.CurrentTechnique.Passes[0].End();
            physicsEffect.End();
            
            //second operation to copy back to rendertarget
            //set render target
            graphics.GraphicsDevice.SetRenderTarget(0, resultTarget);

            //set effect parameters
            physicsEffect.Parameters["temporaryMap"].SetValue(temporaryRT.GetTexture());
            physicsEffect.CurrentTechnique = physicsEffect.Techniques["CopyTexture"];

            //draw with effect onto rendertarget
            spriteBatch.Begin(SpriteBlendMode.None, SpriteSortMode.Immediate, SaveStateMode.None);
            physicsEffect.Begin();
            physicsEffect.CurrentTechnique.Passes[0].Begin();
            spriteBatch.Draw(temporaryRT.GetTexture(), new Rectangle(0, 0, particleCount, particleCount), Color.White);
            spriteBatch.End();
            physicsEffect.CurrentTechnique.Passes[0].End();
            physicsEffect.End();
            
            //set back old rendertargets
            graphics.GraphicsDevice.SetRenderTarget(0, oldRT);
            graphics.GraphicsDevice.DepthStencilBuffer = oldDS;
        }

        private void SimulateParticles(GameTime gameTime)
        {
            physicsEffect.Parameters["elapsedTime"].SetValue((float)gameTime.ElapsedGameTime.TotalSeconds);

            if (!isPhysicsReset)
            {
                DoPhysicsPass("ResetPositions", positionRT);
                DoPhysicsPass("ResetVelocities", velocityRT);

                isPhysicsReset = true;
            }

            DoPhysicsPass("UpdateVelocities", velocityRT);
            DoPhysicsPass("UpdatePositions", positionRT);
        }

        private void checkMouseInput()
        {
            //get relative location  of mouse on window
            double xRelativeLocation = (double)inputHandler.MouseState.X / graphics.PreferredBackBufferWidth;
            double yRelativeLocation = (double)inputHandler.MouseState.Y / graphics.PreferredBackBufferHeight;

            //map the screen coordinate to the coordinates of the world
            float worldLocationX = (float)(worldWidth * xRelativeLocation) - worldHalfWidth;
            float worldLocationY = (float)(worldHeight * yRelativeLocation) - worldHalfHeight;
            Vector4 mousePosition = new Vector4(worldLocationX, 0, worldLocationY, 1);
            physicsEffect.Parameters["pullLocation"].SetValue(mousePosition);

            int pullStrengthValue = isSuperPullOn ? pullStrength * 5 : pullStrength; //if superpull is on, increase pull by 5 times.

            if (inputHandler.MouseState.LeftButton == ButtonState.Pressed) //left click - pull inward
            {
                physicsEffect.Parameters["pullStrength"].SetValue(pullStrengthValue);
            }
            else if (inputHandler.MouseState.RightButton == ButtonState.Pressed) //right click - push outward
            {
                physicsEffect.Parameters["pullStrength"].SetValue(-pullStrengthValue);
            }
            else //dont pull if no button pressed
            {
                physicsEffect.Parameters["pullStrength"].SetValue(0);
            }
        }

        KeyboardState previousState;
        KeyboardState currentState;
        private void checkKeyboardInput()
        {
            //manage initial previous state
            if (previousState == null)
            {
                previousState = currentState;
            }
            currentState = inputHandler.KeyboardState;      //get current state

            //do key checks
            //number keys = change gravity state
            if (currentState.IsKeyDown(Keys.D1) && !previousState.IsKeyDown(Keys.D1))  //1 - no gravity
            {
                isDownGravityOn = false;
                isCenterGravityOn = false;
            }
            if (currentState.IsKeyDown(Keys.D2) && !previousState.IsKeyDown(Keys.D2)) //2 - downward gravity
            {
                isDownGravityOn = true;
                isCenterGravityOn = false;
            }
            if (currentState.IsKeyDown(Keys.D3) && !previousState.IsKeyDown(Keys.D3)) //3 - gravity at center
            {
                isDownGravityOn = false;
                isCenterGravityOn = true;
            }
            physicsEffect.Parameters["isDownGravityOn"].SetValue(isDownGravityOn);
            physicsEffect.Parameters["isCoreGravityOn"].SetValue(isCenterGravityOn);

            //enter = reset 
            if (currentState.IsKeyDown(Keys.Enter) && !previousState.IsKeyDown(Keys.Enter))
            {
                isPhysicsReset = false;
            }

            //leftshift = increase pull strength by 5 times
            if (currentState.IsKeyDown(Keys.LeftShift) && !previousState.IsKeyDown(Keys.LeftShift))
            {
                isSuperPullOn = !isSuperPullOn;
            }

            //now current is the previous state.
            previousState = currentState;

            
            
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            SimulateParticles(gameTime);  //do particle simulation          
            graphics.GraphicsDevice.Clear(Color.Black); 

            //set Particle.fx variables
            renderParticleEffect.Parameters["world"].SetValue(Matrix.Identity);
            renderParticleEffect.Parameters["view"].SetValue(camera.View);
            renderParticleEffect.Parameters["proj"].SetValue(camera.Projection);
            renderParticleEffect.Parameters["textureMap"].SetValue(particleTexture);
            renderParticleEffect.Parameters["positionMap"].SetValue(positionRT.GetTexture());
            renderParticleEffect.CommitChanges();
            
            //render with alphablending and make it opaque (if I'm not mistaken)
            graphics.GraphicsDevice.RenderState.AlphaBlendEnable = true;
            graphics.GraphicsDevice.RenderState.AlphaBlendOperation = BlendFunction.Add;
            graphics.GraphicsDevice.RenderState.DepthBufferWriteEnable = false;
            graphics.GraphicsDevice.RenderState.PointSpriteEnable = true;
            graphics.GraphicsDevice.RenderState.SourceBlend = Blend.SourceAlpha;
            graphics.GraphicsDevice.RenderState.DestinationBlend = Blend.One;

            //draw all particles, let Particle.fx handle placing them on screen
            //using vertex texture fetch from the PositionMap
            using (VertexDeclaration decl = new VertexDeclaration(
                   graphics.GraphicsDevice, VertexPositionColor.VertexElements))
            {
                graphics.GraphicsDevice.VertexDeclaration = decl;

                renderParticleEffect.Begin();
                renderParticleEffect.CurrentTechnique.Passes[0].Begin();

                graphics.GraphicsDevice.Vertices[0].SetSource(particlesVB, 0, VertexPositionColor.SizeInBytes);
                graphics.GraphicsDevice.DrawPrimitives(PrimitiveType.PointList, 0, particleCount * particleCount);

                renderParticleEffect.CurrentTechnique.Passes[0].End();
                renderParticleEffect.End();
            }

            base.Draw(gameTime);
        }
    }
}
