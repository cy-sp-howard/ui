﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SharpDX.Direct3D9;
using System;
using System.Threading;
using System.Windows.Forms;
using ui.Services;
using ui.Services.Overlay;
using static ui.WindowUtil;

namespace ui
{
    public class ReadHelper : Game
    {
        public GraphicsDeviceManager Graphics => _graphics;
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        public IntPtr FormHandle { get; private set; }
        public Form Form { get; private set; }
        private static readonly IService[] _services = new IService[] {
            Overlay        = new Overlay()
        };
        public static readonly Overlay Overlay;
        internal static ReadHelper Instance;
        static public Texture2D PixelTexture;
        private System.Drawing.Point location_bak;
        private bool drawStarted = false;

        public ReadHelper()
        {
            ReadHelper.Instance = this;
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            // TODO: Add your initialization logic here
            FormHandle = this.Window.Handle;
            Form = Control.FromHandle(FormHandle).FindForm();

            Form.BackColor = System.Drawing.Color.Yellow;
            // Avoid the flash the window shows when the application launches (-32000x-32000 is where windows places minimized windows)
            location_bak = Form.Location;
            Form.Location = new System.Drawing.Point(-30000, -30000);
            Window.IsBorderless = true;

            SetWindowLong(FormHandle, GWL_EXSTYLE, WS_EX_TOPMOST | WS_EX_TRANSPARENT | WS_EX_LAYERED);
            SetLayeredWindowAttributes(FormHandle, 0, 255, 2);

            Graphics.PreferredBackBufferWidth = 1000;
            Graphics.PreferredBackBufferHeight = 500;
            Graphics.ApplyChanges();

            base.Initialize();
        }

        protected override void LoadContent()
        {
            PixelTexture = new Texture2D(GraphicsDevice, 1, 1);
            PixelTexture.SetData(new[] { Color.White });
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            foreach (var item in _services)
            {
                item.Load();
            }

        }
        protected override void BeginRun()
        {
            base.BeginRun();

        }

        protected override void Update(GameTime gameTime)
        {
            var marg = new Margins
            {
                cxLeftWidth = 0,
                cyTopHeight = 0,
                cxRightWidth = Graphics.PreferredBackBufferWidth,
                cyBottomHeight = Graphics.PreferredBackBufferHeight
            };
            // 無邊框
            //DwmExtendFrameIntoClientArea(FormHandle, ref marg);
            var screenPoint = System.Drawing.Point.Empty;
            ClientToScreen(FormHandle, ref screenPoint);
            SetWindowPos(FormHandle, HWND_TOPMOST, screenPoint.X, screenPoint.Y, marg.cxRightWidth, marg.cyBottomHeight, 0);

            foreach (var item in _services)
            {
                item.Update(gameTime);
            }
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Transparent);
            _spriteBatch.Begin();
            foreach (var item in _services)
            {
                item.Draw(_spriteBatch, GraphicsDevice);
            }
            _spriteBatch.End();
            if(!drawStarted)
            {
                Form.Location = location_bak;
                drawStarted = true;
            }
            base.Draw(gameTime);
        }
    }
}
