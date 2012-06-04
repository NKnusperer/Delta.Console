using Delta.Engine.Dynamic;
using Delta.Engine;
using System.Reflection;
using Delta.InputSystem;

namespace Delta.Console
{
    /// <summary>
    /// The main module you need to load with the Factory to use the console and graph
    /// </summary>
    public class Module : DynamicModule
    {
        #region Private
        /// <summary>
        /// Holds an instance of the Console class
        /// </summary>
        private Delta.Console.Console console;
        /// <summary>
        /// Holds an instance of the GraphManager class
        /// </summary>
        private GraphManager graph;
        /// <summary>
        /// Save if the graph should be drawn or not
        /// </summary>
        private bool isGraphEnabled;
        #endregion

        #region ctor
        /// <summary>
        /// Constructor for this module, will be child of the Application module to
        /// be executed as part of the Application.Run.
        /// </summary>
        /// <param name="bindConsole">The button to open and close the console</param>
        public Module(InputButton bindConsoleButton)
            : base("Delta.Console", typeof(Application))
        {
            // Initialize
            console = new Delta.Console.Console(bindConsoleButton);
            graph = new GraphManager();

            // Add command to the console to enable or disable the graph
            console.AddCmdToConsole(typeof(Module).GetMethod("SetGraphState"), this);
        }
        #endregion

        #region AddCmdToConsole
        /// <summary>
        /// Add a method to the console so it's collable from the console
        /// </summary>
        /// <param name="method">The method you want to add</param>
        /// <param name="target">The instance of the object where the method is running. Can be NULL of you dont deal with any context related stuff.</param>
        public void AddCmdToConsole(MethodInfo method, object target)
        {
            console.AddCmdToConsole(method, target);
        }
        #endregion

        #region AddPlotToGraph
        /// <summary>
        /// Add a custom plot to the graph to draw in it
        /// </summary>
        /// <param name="plot">A object that implements the IPlottable interface</param>
        public void AddPlotToGraph(IPlottable plot)
        {
            graph.AddPlotToGraph(plot);
        }
        #endregion

        #region EnableGraph
        /// <summary>
        /// Show or hide the graph.
        /// If enabled and then disabled, the graph will be freezed and continue when you enable it again.
        /// </summary>
        /// <param name="state">The state to set</param>
        /// <returns>The new state of the graph</returns>
        [ConsoleCommand("Render.GraphOverlayEnable")]
        public bool SetGraphState(bool state)
        {
            isGraphEnabled = state;
            return isGraphEnabled;
        }
        #endregion

        #region Run
        /// <summary>
        /// Run function of the module. Gets called every update tick.
        /// </summary>
        public override void Run()
        {
            console.Draw();

            if (isGraphEnabled)
            {
                graph.Draw();
            }
        }
        #endregion
    }
}