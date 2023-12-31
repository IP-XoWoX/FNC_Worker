﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;

namespace FunctionGraphics
{
    public class Window : Game
    {
        GraphicsDeviceManager graphics;
        Camera camera;
        BasicEffect effect;

        Point mousePreviousFrame;
        int mousePreviousScrollValue;
        float mouseMoveFriction = 0.004f;
        float mouseScrollFriction = 0.004f;

        VertexPositionColor[] axes = new VertexPositionColor[]
        {
            new VertexPositionColor(new Vector3(-10000, 0, 0), Color.Gray),
            new VertexPositionColor(new Vector3(10000, 0, 0), Color.Gray),
            new VertexPositionColor(new Vector3(0,-10000, 0), Color.Gray),
            new VertexPositionColor(new Vector3(0, 10000,  0), Color.Gray),
        };
        List<VertexPositionColor[]> graphs = new List<VertexPositionColor[]>();

        public Window()
        {
            graphics = new GraphicsDeviceManager(this);
            graphics.PreferredBackBufferWidth = 1280;
            graphics.PreferredBackBufferHeight = 720;
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            effect = new BasicEffect(graphics.GraphicsDevice);
            effect.VertexColorEnabled = true;
            camera = new Camera(effect, new Vector3(0, 0, 10), graphics.PreferredBackBufferWidth / (float)graphics.PreferredBackBufferHeight, MathHelper.Pi / 2, 0.1f, 1000f); 
            base.Initialize();
        }

        protected override void Update(GameTime gameTime)
        {
            MouseState mouse = Mouse.GetState();
            if (mouse.LeftButton == ButtonState.Pressed && IsActive)
            {
                camera.TargetPosition = new Vector3(
                    camera.TargetPosition.X + ((mousePreviousFrame.X - mouse.Position.X) * mouseMoveFriction * camera.TargetPosition.Z),
                    camera.TargetPosition.Y + ((mouse.Position.Y - mousePreviousFrame.Y) * mouseMoveFriction * camera.TargetPosition.Z),
                    camera.TargetPosition.Z);
            }
            if (mouse.ScrollWheelValue != mousePreviousScrollValue)
            {
                double z = camera.TargetPosition.Z + ((mousePreviousScrollValue - mouse.ScrollWheelValue) * mouseScrollFriction);
                if (z < 1)
                    z = 1;
                if (z > 1000)
                    z = 1000;
                camera.TargetPosition = new Vector3(camera.TargetPosition.X, camera.TargetPosition.Y, (float)z);
            }
            camera.Update(gameTime.ElapsedGameTime.Ticks);
            mousePreviousFrame = mouse.Position;
            mousePreviousScrollValue = mouse.ScrollWheelValue;
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);
            foreach (var pass in effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                graphics.GraphicsDevice.DrawUserPrimitives(PrimitiveType.LineList, axes, 0, axes.Length / 2);
                foreach (var graph in graphs)
                {
                    if (graph.Length >= 2)
                        graphics.GraphicsDevice.DrawUserPrimitives(PrimitiveType.LineList, graph, 0, graph.Length / 2);
                }
            }
            base.Draw(gameTime);
        }

        public int Draw2DGraphDekart(Func<double, double> function, double step, double minBound, double maxBound, Color color)
        {
            var result = new List<VertexPositionColor>();
            double prevX = minBound;
            double prevY = function(minBound);
            double y;
            for (double x = minBound + step; x <= maxBound; x += step)
            {
                y = function(x);
                if (Math.Abs(y - prevY) < 100f)
                {
                    result.Add(new VertexPositionColor(new Vector3((float)prevX, (float)prevY, 0), color));
                    result.Add(new VertexPositionColor(new Vector3((float)x, (float)function(x), 0), color));
                }
                prevX = x;
                prevY = function(x);
                //Console.WriteLine(x + " " +prevY);
            }
            graphs.Add(result.ToArray());
            return graphs.Count - 1;
        }

        public int Draw2DGraphPolar(Func<double, double> function, double step, double minBound, double maxBound, Color color)
        {
            var result = new List<VertexPositionColor>();
            ToDekart(function(minBound), minBound, out double x, out double y);
            double prevX = x;
            double prevY = y;
            for (double f = minBound + step; f <= maxBound; f += step)
            {
                ToDekart(function(f), f, out x, out y);
                result.Add(new VertexPositionColor(new Vector3((float)prevX, (float)prevY, 0), color));
                result.Add(new VertexPositionColor(new Vector3((float)x, (float)y, 0), color));
                prevX = x;
                prevY = y;
            }
            graphs.Add(result.ToArray());
            return graphs.Count - 1;
        }

        private void ToDekart(double r, double f, out double x, out double y)
        {
            x = r * (double)Math.Cos(f);
            y = r * (double)Math.Sin(f);
        }

        public void RemoveGraph(int index)
        {
            
            if (graphs.Count > index)
                graphs.RemoveAt(index);
        }
    }
}
