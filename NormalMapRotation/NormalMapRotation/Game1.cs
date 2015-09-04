using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace NormalMapRotation
{
    public class Sprite
    {
        public Vector2 Position;
        public float Angle;
        public Color Color;
    }

    public class Game1 : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        RenderTarget2D normalRT;
        RenderTarget2D resultRT;
        Texture2D normal;
        Effect normalEffect1;
        Effect normalEffect2;
        Effect lightEffect;
        Vector3 lightDirection;
        EffectParameter lightDirectionParam;
        EffectParameter normalEffect2RotationParam;
        Texture2D pixel;
        KeyboardState keyState;
        KeyboardState prevKeyState;
        bool showNormals;
        int renderMethod = 1;
        SpriteFont font;
        List<Sprite> sprites;
        Stopwatch stopwatch;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        protected override void Initialize()
        {
            // TODO: Add your initialization logic here

            base.Initialize();
        }

        protected override void LoadContent()
        {
            graphics.PreferredBackBufferWidth = 300;
            graphics.PreferredBackBufferHeight = 200;
            graphics.ApplyChanges();

            spriteBatch = new SpriteBatch(GraphicsDevice);
            normalRT = new RenderTarget2D(GraphicsDevice, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height);
            resultRT = new RenderTarget2D(GraphicsDevice, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height);
            normal = Content.Load<Texture2D>("SampleNormal");
            normalEffect1 = Content.Load<Effect>("NormalMapRotate1");
            normalEffect2 = Content.Load<Effect>("NormalMapRotate2");
            normalEffect2RotationParam = normalEffect2.Parameters["SpriteRotation"];
            font = Content.Load<SpriteFont>("Font");
            lightEffect = Content.Load<Effect>("Lighting");
            lightDirectionParam = lightEffect.Parameters["LightDirection"];

            pixel = new Texture2D(GraphicsDevice, 1, 1);
            pixel.SetData<Color>(new[] { Color.White });

            Random rng = new Random();
            sprites = new List<Sprite>();
            for (int i = 0; i < 20000; i++)
            {
                Sprite sprite = new Sprite();

                sprite.Position = new Vector2((float)rng.NextDouble() * GraphicsDevice.Viewport.Width, (float)rng.NextDouble() * GraphicsDevice.Viewport.Height);
                sprite.Angle = (float)rng.NextDouble() * MathHelper.TwoPi;
                sprites.Add(sprite);
            }

            stopwatch = new Stopwatch();
        }

        protected override void UnloadContent()
        {
        }

        public float EncodeAngle(float angleRads)
        {
            angleRads = MathHelper.WrapAngle(angleRads);

            // Range will be [0, 2*pi]
            if (angleRads < 0)
                angleRads += MathHelper.TwoPi;

            // Convert to [0, 1]
            angleRads /= MathHelper.TwoPi;

            return angleRads;
        }

        protected override void Update(GameTime gameTime)
        {
            prevKeyState = keyState;
            keyState = Keyboard.GetState();

            MouseState mouseState = Mouse.GetState();
            Vector2 center = new Vector2(GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height) * 0.5f;
            Vector2 mouse = new Vector2(mouseState.X, mouseState.Y);
            Vector2 rel = mouse - center;

            if (keyState.IsKeyDown(Keys.N) && prevKeyState.IsKeyUp(Keys.N))
                showNormals = !showNormals;
            if (keyState.IsKeyDown(Keys.D1) && prevKeyState.IsKeyUp(Keys.D1))
                renderMethod = 1;
            if (keyState.IsKeyDown(Keys.D2) && prevKeyState.IsKeyUp(Keys.D2))
                renderMethod = 2;
            if (keyState.IsKeyDown(Keys.D3) && prevKeyState.IsKeyUp(Keys.D3))
                renderMethod = 3;

            //spriteAngle += 0.01f;
            //normalColor = Color.White;
            //if (!capture)
                //spriteAngle = (float)Math.Atan2(rel.Y, rel.X);

            for (int i = 0; i < sprites.Count; i++)
            {
                sprites[i].Angle += 0.01f;
            }

            if (renderMethod == 1)
            {
                for (int i = 0; i < sprites.Count; i++)
                {
                    Sprite sprite = sprites[i];

                    sprite.Color = new Color(EncodeAngle(sprite.Angle), 0f, 0f);
                }
            }


            lightDirection = new Vector3(1f, 1f, 0.3f);

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            lightDirectionParam.SetValue(lightDirection);

            GraphicsDevice.SetRenderTarget(normalRT);
            GraphicsDevice.Clear(new Color(0.5f, 0.5f, 1f));

            stopwatch.Reset();
            stopwatch.Start();

            if (renderMethod == 1)
            {
                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearWrap, null, null, normalEffect1);
                for (int i = 0; i < sprites.Count; i++)
                {
                    Sprite sprite = sprites[i];

                    spriteBatch.Draw(normal, sprite.Position, null, sprite.Color, sprite.Angle, new Vector2(normal.Width, normal.Height) * 0.5f, 1f, SpriteEffects.None, 0f);
                }
                spriteBatch.End();
            }
            else if (renderMethod == 2)
            {
                for (int i = 0; i < sprites.Count; i++)
                {
                    Sprite sprite = sprites[i];

                    normalEffect2RotationParam.SetValue(sprite.Angle);
                    spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearWrap, null, null, normalEffect2);
                    spriteBatch.Draw(normal, sprite.Position, null, Color.White, sprite.Angle, new Vector2(normal.Width, normal.Height) * 0.5f, 1f, SpriteEffects.None, 0f);
                    spriteBatch.End();
                }
            }
            else if (renderMethod == 3)
            {
                spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearWrap, null, null, normalEffect2);
                for (int i = 0; i < sprites.Count; i++)
                {
                    Sprite sprite = sprites[i];

                    normalEffect2RotationParam.SetValue(sprite.Angle);
                    spriteBatch.Draw(normal, sprite.Position, null, Color.White, sprite.Angle, new Vector2(normal.Width, normal.Height) * 0.5f, 1f, SpriteEffects.None, 0f);
                }
                spriteBatch.End();
            }

            stopwatch.Stop();

            GraphicsDevice.SetRenderTarget(resultRT);
            GraphicsDevice.Clear(Color.Gray);

            if (showNormals)
            {
                spriteBatch.Begin();
                spriteBatch.Draw(normalRT, normalRT.Bounds, Color.White);
                spriteBatch.End();
            }
            else
            {
                GraphicsDevice.Textures[1] = normalRT;
                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearWrap, null, null, lightEffect);
                spriteBatch.Draw(pixel, new Rectangle(0, 0, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height), Color.White);
                spriteBatch.End();
            }

            GraphicsDevice.SetRenderTarget(null);
            spriteBatch.Begin();
            spriteBatch.Draw(resultRT, resultRT.Bounds, Color.White);
            spriteBatch.End();

            string time = "Render method: " + renderMethod + "\nTime to render: " + stopwatch.ElapsedMilliseconds + "ms";
            spriteBatch.Begin();
            spriteBatch.DrawString(font, time, new Vector2(11, 11), Color.Black);
            spriteBatch.DrawString(font, time, new Vector2(9, 9), Color.Black);
            spriteBatch.DrawString(font, time, new Vector2(10, 10), Color.Yellow);
            spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
