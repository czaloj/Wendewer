using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BlisterUI;
using BlisterUI.Input;
using BlisterUI.Widgets;
using Wdw.GLView;
using Wdw.Common.Data;
using Wdw.Common.Events;
using Wdw.Common.Property;
using OpenTK;
using EGL;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Graphics;
using OpenTK.Input;
using System.Diagnostics;
using EGL.Helpers;
using System.Drawing;
using Wdw.GLView.UI;
using Wdw.Common.Operators;

namespace Wdw {
    public class AppScreen : GameScreen<App> {
        public override int Next {
            get { return -1; }
            protected set { }
        }
        public override int Previous {
            get { return -1; }
            protected set { }
        }

        MasterData data;

        SpriteBatch sb;
        SpriteFont sf;

        WidgetRenderer wr;
        Renderer renderer;
        RenderPanel rPanel;
        MenuOptions menu;
        OperatorPanel opPanel;

        public override void Build() {
        }
        public override void Destroy(GameTime gameTime) {
        }

        public override void OnEntry(GameTime gameTime) {
            sb = new SpriteBatch();
            sf = new SpriteFont("Times New Roman", 120, (char)32, (char)126);
            wr = new WidgetRenderer(sf);

            data = new MasterData();
            OperatorList.InitializeAll(data);
            renderer = new Renderer();

            CreateMenu();
            CreateRenderPanel();

            CreateRandomObjects();
            rPanel.OnPick += (sender, e) => {
                for(int i = 0; i < MasterData.Instance.Objects.Count; i++) {
                    var od = MasterData.Instance.Objects[i];
                    if(i == e.UUID) {
                        od.Material = data.Materials[1];
                    }
                    else od.Material = data.Materials[0];
                }
            };

            KeyboardEventDispatcher.OnKeyPressed += OnKP;
            MouseEventDispatcher.OnMousePress += OnMP;
            MouseEventDispatcher.OnMouseRelease += OnMR;
        }
        public void CreateMenu() {
            string[] ops = {
                "File.New",
                "Options.About",
                "Exit"
            };
            menu = new MenuOptions(wr, game.Width, 24, wr.DefaultFont, 24, ops.Union(from op in OperatorList.Operators select op.Name));
            menu.Hook();
            menu.OnMenuPick += menu_OnMenuPick;
        }
        public void CreateRenderPanel() {
            rPanel = new RenderPanel(wr, 500, 500, renderer, 500, 500);
            rPanel.Offset = new Point(game.Width / 4, 0);
            rPanel.OffsetAlignY = Alignment.BOTTOM;
            rPanel.LayerOffset = 0f;
            rPanel.Parent = menu;
            rPanel.Hook();
        }
        public override void OnExit(GameTime gameTime) {
            KeyboardEventDispatcher.OnKeyPressed -= OnKP;
            MouseEventDispatcher.OnMousePress -= OnMP;
            MouseEventDispatcher.OnMouseRelease -= OnMR;

            sf.Dispose();
            renderer.Dispose();
            rPanel.Dispose();
            menu.Dispose();
            wr.Dispose();
        }

        public override void Update(GameTime gameTime) {
            DataEvent e = MasterData.GetEvent();
            while(e != null) {
                renderer.ApplyEvent(e);
                e = MasterData.GetEvent();
            }
        }
        public override void Draw(GameTime gameTime) {
            rPanel.Render();

            GL.Viewport(0, 0, game.Width, game.Height);
            GL.ClearColor(0.1f, 0.1f, 0.1f, 1f);
            GL.ClearDepth(1);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            wr.Draw(sb, game.Width, game.Height);
        }

        public void OnKP(object sender, KeyboardKeyEventArgs args) {
            switch(args.Key) {
                case Key.F4:
                    break;
                case Key.Q:
                    break;
            }
        }
        public void OnMP(object sender, MouseButtonEventArgs e) {
        }
        public void OnMR(object sender, MouseButtonEventArgs e) {
        }

        private void CreateRandomObjects() {
            data.RemoveAll();

            var mesh = data.CreateMesh();
            mesh.ReadData(@"data\meshes\Cube.obj");

            var tex = data.CreateTexture();
            tex.FileLocation = @"data\textures\Cube.png";

            var matNorm = data.CreateMaterial();
            matNorm.VShaderFile = @"data\shaders\texture.vert";
            matNorm.FShaderFile = @"data\shaders\texture.frag";
            matNorm.AddTexture(0, "Texture", tex);
            matNorm.AddUniform("Tint", PropertyType.Vec3);
            matNorm.ChangeUniform("Tint", Vector3.One);
            matNorm.AddUniform("Texture", PropertyType.Int);
            matNorm.ChangeUniform("Texture", 0);

            var matHigh = data.CreateMaterial();
            matHigh.VShaderFile = @"data\shaders\texture.vert";
            matHigh.FShaderFile = @"data\shaders\texture.frag";
            matHigh.AddTexture(0, "Texture", tex);
            matHigh.AddUniform("Tint", PropertyType.Vec3);
            matHigh.ChangeUniform("Tint", Vector3.One * 10);
            matHigh.AddUniform("Texture", PropertyType.Int);
            matHigh.ChangeUniform("Texture", 0);

            Random r = new Random();
            for(int i = 0; i < 20; i++) {
                var obj = data.CreateObject();
                obj.Transform =
                    Matrix4.CreateRotationX(r.Next(630) / 100f) *
                    Matrix4.CreateRotationY(r.Next(630) / 100f) *
                    Matrix4.CreateScale(4f);
                obj.Mesh = MasterData.Instance.Box;
                obj.Material = matNorm;
            }
        }

        void menu_OnMenuPick(object sender, MenuPickArgs e) {
            switch(e.Operation) {
                case "File.New":
                    CreateRandomObjects();
                    break;
                case "Exit":
                    State = ScreenState.ExitApplication;
                    break;
                default:
                    menu.Unhook();
                    var op = OperatorList.Get(e.Operation);
                    opPanel = new OperatorPanel(wr, 200, 24, op, data);
                    opPanel.AlignX = Alignment.RIGHT;
                    opPanel.AlignY = Alignment.BOTTOM;
                    opPanel.Anchor = new Point(game.Width, game.Height);
                    opPanel.LayerDepth = 0.5f;
                    opPanel.OnOperationFinished += (a1, a2) => {
                        opPanel.Dispose();
                        opPanel = null;
                        menu.Hook();
                    };
                    break;
            }
        }
    }
}