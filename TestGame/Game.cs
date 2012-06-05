using System.Collections.Generic;
using Delta.ContentSystem;
using Delta.ContentSystem.Rendering;
using Delta.Engine;
using Delta.Engine.Dynamic;
using Delta.PhysicsEngines;
using Delta.PhysicsEngines.VisualShapes;
using Delta.Rendering.Basics.Fonts;
using Delta.Rendering.Cameras;
using Delta.Rendering.Models;
using Delta.Utilities.Datatypes;
using Delta.InputSystem;
using Delta.Console;

namespace TestGame
{
    /// <summary>
    /// Game class, which is the entry point for this game. Manages all the game
    /// logic and displays everything. More complex games obviously have more
    /// classes and will do things in a more organized and structured matter.
    /// This example game is about a 3D Pyramid of boxes, which can be pushed
    /// around (via Mouse, Touch or other input devices) to collapse the Pyramid
    /// with 3D Physics.
    /// </summary>
    class Game : DynamicModule
    {
        #region Constants
        /// <summary>
        /// Size of the pyramid. Bullet is really slow for this test so only use
        /// 10 * 10 pyramid. Use 20 * 20 for Jitter (handles this much quicker),
        /// 50 * 50 or more is also certainly possible, but slower!
        /// </summary>
        private static readonly int PyramidSize =
            Settings.Modules.PhysicsModule == "Bullet"
                ? 10
                : 20;

        /// <summary>
        /// Size of each box for the Pyramid.
        /// </summary>
        private const float BoxSize = 2.0f;
        #endregion

        #region Private
        /// <summary>
        /// Mesh for drawing the ground plane
        /// </summary>
        private Mesh plane =
            Mesh.CreateSegmentedPlane("GroundPlane", 75, 75, 5,
                Content.Exists("Ground", ContentType.Image)
                    ? new MaterialData()
                    {
                        DiffuseMapName = "Ground",
                    }
                    : MaterialData.Default);

        /// <summary>
        /// And list of shapes to draw for the pyramid in the 3D scene.
        /// </summary>
        private readonly List<BaseVisualPhysicsShape> shapesToDraw =
            new List<BaseVisualPhysicsShape>();
        #endregion

        #region Constructor
        /// <summary>
        /// Constructor, do additional initialization code here if needed.
        /// </summary>
        public Game()
            : base("Advanced Game", typeof(Application))
        {
            // Unable to continue if we have no physics (warnings are already in log)
            if (Physics.Instance == null)
            {
                Application.Quit();
                return;
            }

            Physics.Instance.SetGroundPlane(true, 0.0f);
            // Useful for debugging:
            //Physics.DebugEnabled = true;
            // Increase gravity to be less moon-like (not perfect, just tweaking)
            Physics.Gravity = Physics.DefaultGravity * 10.0f;
            // Create a camera for the 3D world.
            new LookAtCamera(new Vector(10, -25, 25));
            // And finally setup the pyramid boxes
            SetupPyramid();

            // Setup a few commands for some interaction (pushing boxes and reseting)
            Input.Commands[Command.UIClick].Add(delegate(CommandTrigger command)
            {
                // Create a ray from the input position (mouse, touch, whatever)
                Ray ray = ScreenSpace.GetRayFromScreenPoint(command.Position);
                // And try to grab the box we hit
                PhysicsBody grabBody;
                Vector hitNormal;
                float fraction;
                if (Physics.FindRayCast(ray, false, out grabBody, out hitNormal,
                    out fraction) &&
                    grabBody != null)
                {
                    // Activate that body and push it in the direction we are looking
                    grabBody.ApplyLinearImpulse(ray.Direction / 3.0f);
                }
            });
            Input.Commands[Command.UIRightClick].Add(delegate
            {
                SetupPyramid();
            });
            Input.Commands[Command.QuitTest].Add(delegate
            {
                Application.Quit();
            });

            // Load the module
            var module = Factory.Create<Delta.Console.Module>(new object[] { InputButton.Pipe, null, null, null, null });
            // Add our two test plots to the graph
            module.AddPlotToGraph(new TestPlotCpu());
            module.AddPlotToGraph(new TestPlotFps());
            // Add the test methods to the console
            module.AddCmdToConsole(typeof(Game).GetMethod("AddFloats"), null);


        }
        #endregion

        #region AddFloats
        [ConsoleCommand("Render.AddFloats")]
        public float AddFloats(float a, float b)
        {
            return a + b;
        }
        #endregion

        #region SetupPyramid
        /// <summary>
        /// Setup the pyramid of physics boxes for this example game
        /// </summary>
        private void SetupPyramid()
        {
            // If we already had boxes, just restore them!
            if (shapesToDraw.Count > 0)
            {
                foreach (BaseVisualPhysicsShape shape in shapesToDraw)
                {
                    if (shape.Body != null)
                    {
                        shape.Body.AngularVelocity = Vector.Zero;
                        shape.Body.LinearVelocity = Vector.Zero;
                        shape.Body.RotationMatrix = Matrix.Identity;
                        shape.Body.Position = shape.Body.InitialPosition;
                        shape.Body.Restitution = 0.0f;
                        shape.Body.IsActive = true;
                    }
                }
            }
            else
            {
                // Otherwise create a new pyramid
                for (int height = 0; height < PyramidSize; height++)
                {
                    for (int width = height; width < PyramidSize; width++)
                    {
                        // Very slightly increase height at the upper levels. This will cause
                        // the pyramid to collapse and bounce of the top parts in the
                        // beginning while the rest stays solid :)
                        VisualPhysicsBox box = new VisualPhysicsBox(
                            new Vector((width - height * 0.5f) * BoxSize * 1.1f -
                                BoxSize * 1.1f * PyramidSize / 2.0f + 1.1f, 0.0f,
                                1.0f + height * BoxSize * 1.1f),
                            BoxSize, BoxSize, BoxSize, Color.White,
                            new MaterialData()
                            {
                                DiffuseMapName =
                                    Content.Exists("Box", ContentType.Image)
                                        ? "Box"
                                        : "DeltaEngineLogo"
                            });

                        shapesToDraw.Add(box);

                        if (box.Body != null)
                        {
                            box.Body.Restitution = 0.0f;
                        }
                    }
                }
            }
        }
        #endregion

        #region Run
        /// <summary>
        /// Run game loop, called every frame to do all game logic updating.
        /// </summary>
        public override void Run()
        {

            // Draw the boxes of the pyramid
            foreach (BaseVisualPhysicsShape shape in shapesToDraw)
            {
                shape.Draw();
            }

            // Draw the ground plane (a little below the physical ground to avoid
            // overlapping and boxes going slightly into the ground).
            Matrix groundTransform = Matrix.CreateTranslation(0.0f, 0.0f, -0.01f);
            plane.Draw(ref groundTransform);
        }
        #endregion
    }
}
