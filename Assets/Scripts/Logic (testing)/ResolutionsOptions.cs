using System;
using System.Collections.Generic;

namespace Mayhem.Logic
{
    public struct ResolutionInfo
    {
        public int Width { get; private set; }
        public int Height { get; private set; }
        public double RefreshRate { get; private set; }

        public ResolutionInfo(int width, int height, double refreshRate)
        {
            Width = width;
            Height = height;
            RefreshRate = refreshRate;
        }
    }

    public class ResolutionOptions
    {
        private const double RefreshTolerance = 0.01;
        private readonly List<ResolutionInfo> options = new List<ResolutionInfo>();

        public int Count => options.Count;

        public ResolutionOptions(IEnumerable<ResolutionInfo> all, double currentRefreshRate)
        {
            if (all == null) return;

            foreach (var r in all)
            {
                if (Math.Abs(r.RefreshRate - currentRefreshRate) < RefreshTolerance)
                    options.Add(r);
            }
        }

        public ResolutionInfo Get(int index) { return options[index]; }

        public List<string> Labels()
        {
            var labels = new List<string>();
            foreach (var r in options)
                labels.Add($"{r.Width} x {r.Height} {(int)Math.Round(r.RefreshRate)} Hz");
            return labels;
        }

        public int IndexOf(int width, int height)
        {
            for (int i = 0; i < options.Count; i++)
                if (options[i].Width == width && options[i].Height == height)
                    return i;
            return 0;
        }

        public bool TryGet(int index, out ResolutionInfo info)
        {
            if (index < 0 || index >= options.Count)
            {
                info = default(ResolutionInfo);
                return false;
            }
            info = options[index];
            return true;
        }
    }
}