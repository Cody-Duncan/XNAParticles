#region File Header
/*////////////////////////////////////////////////////////////////////////
 *  InputHandler.cs
 *  Project: Particle Test 2
 *  Date: December 28th, 2011
 *  Requirements: Visual Studio 2008, XNA 3.1, Shader Model 3.0
 *  Description: Handler to get keyboard, mouse, and gamepad states.
 *  References:
 *  borrowed entirely from: Microsoft XNA Game Studio 3.0 Unleashed by Chad Carter
///////////////////////////////////////////////////////////////////////*/
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Net;
using Microsoft.Xna.Framework.Storage;

//Borrowed from XNA Unleashed by Chad Carter. Really good book :D .
namespace ParticleTest2
{
    public interface IInputHandler 
    {
        KeyboardState KeyboardState { get; }
        GamePadState[] GamePads { get; }

        #if !XBOX360
        MouseState MouseState { get; }
        MouseState PreviousMouseState { get; }
        #endif
    }

    /// <summary>
    /// This is a game component that implements IUpdateable.
    /// </summary>
    public class InputHandler : Microsoft.Xna.Framework.GameComponent, IInputHandler
    {

        #region Fields


        private KeyboardState keyboardState;
        private GamePadState[] gamePads = new GamePadState[4];

        #if !XBOX360
        private MouseState mouseState;
        private MouseState previousMouseState;
        #endif

        

        #endregion

        #region Properties

        public KeyboardState KeyboardState
        {
            get { return(keyboardState); }
        }

        public GamePadState[] GamePads 
        {
            get { return (gamePads); } 
        }
        
        #if !XBOX360
        public MouseState MouseState
        {
            get { return (mouseState); }
        }

        public MouseState PreviousMouseState
        {
            get { return (previousMouseState); }
        }
        #endif

        #endregion

        public InputHandler(Game game)
            : base(game)
        {
            // TODO: Construct any child components here
            game.Services.AddService(typeof(IInputHandler), this);

            #if !XBOX360
                Game.IsMouseVisible = true;
                previousMouseState = Mouse.GetState();
            #endif
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
            // TODO: Add your update code here

            base.Update(gameTime);

            keyboardState = Keyboard.GetState();

            gamePads[0] = GamePad.GetState(PlayerIndex.One);
            gamePads[1] = GamePad.GetState(PlayerIndex.Two);
            gamePads[2] = GamePad.GetState(PlayerIndex.Three);
            gamePads[3] = GamePad.GetState(PlayerIndex.Four);

            if (keyboardState.IsKeyDown(Keys.Escape))
            {
                Game.Exit();
            }

            if (gamePads[0].Buttons.Back == ButtonState.Pressed)
            {
                Game.Exit();
            }

            #if !XBOX360
            previousMouseState = mouseState;
            mouseState = Mouse.GetState();
            #endif
        }
    }
}