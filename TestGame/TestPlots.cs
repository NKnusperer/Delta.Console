using Delta.Utilities.Datatypes;
using Delta.Engine;
using System.Diagnostics;
using Delta.Console;

namespace TestGame
{
    /// <summary>
    /// Show the current Frames/Second
    /// </summary>
    class TestPlotFps : IPlottable
    {
        public TestPlotFps()
        {
            this.PlotName = "FPS";
            this.PlotColor = Color.Red;
            // We dont know a fixed range of FPS so we enable autoscale
            this.UseAutoScale = true;
        }

        public bool UseAutoScale { get; set; }

        public float MinValue { get; set; }

        public float MaxValue { get; set; }

        public string PlotName { get; set; }

        public Color PlotColor { get; set; }

        public float UpdatePlot()
        {
            // Return the current FPS
            // Will be auto updated every second so we dont need to cache
            return Time.Fps;
        }
    }

    /// <summary>
    /// Show the current CPU load
    /// </summary>
    class TestPlotCpu : IPlottable
    {
        /// <summary>
        /// PerformanceCounter instance
        /// </summary>
        private PerformanceCounter counter;
        /// <summary>
        /// Helper to cache the current value
        /// </summary>
        private float cache;

        public TestPlotCpu()
        {
            this.PlotName = "CPU";
            this.PlotColor = Color.Green;
            // CPU load is from 0% to 100%, autoscale would be confuse us and make no sense
            this.MinValue = 0f;
            this.MaxValue = 100f;

            // Initialize
            counter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
        }

        public bool UseAutoScale { get; set; }

        public float MinValue { get; set; }

        public float MaxValue { get; set; }

        public string PlotName { get; set; }

        public Color PlotColor { get; set; }

        public float UpdatePlot()
        {
            // Update cache every second (could also be done every 0.25f seconds or something like that....)
            if (Time.EverySecond)
            {
                // Set new value
                cache = counter.NextValue();
            }

            // Return the CPU load
            return cache;
        }
    }
}