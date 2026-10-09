using System.Collections.Generic;
using NUnit.Framework;
using Mayhem.Logic;
namespace Mayhem.Tests
{
    public class ResolutionsTests
    {
        // A small fake list of screen modes so that no real and specific monitor is needed
        private static List<ResolutionInfo> Sample()
        {
            return new List<ResolutionInfo>
            {
                new ResolutionInfo(1280, 720, 60),
                new ResolutionInfo(1280, 720, 144),
                new ResolutionInfo(1920, 1080, 60),
                new ResolutionInfo(1920, 1080, 144),
                new ResolutionInfo(2560, 1440, 60),
            };
        }

        //Tests for the resolutions
        [Test]
        public void FilterByRefreshRate()
        {
            var options = new ResolutionOptions(Sample(), 60);
            Assert.AreEqual(3, options.Count);
            Assert.AreEqual(1280, options.Get(0).Width);
            Assert.AreEqual(2560, options.Get(2).Width);
        }

        [Test]
        public void FilterIgnoresTinyRateDifference()
        {
            var all = new List<ResolutionInfo> { new ResolutionInfo(1920, 1080, 59.9400001) };
            var options = new ResolutionOptions(all, 59.94);
            Assert.AreEqual(1, options.Count);
        }

        [Test]
        public void LabelFormat()
        {
            var options = new ResolutionOptions(Sample(), 60);
            Assert.AreEqual("1920 x 1080 60 Hz", options.Labels()[1]);
        }

        [Test]
        public void LabelRoundsHz()
        {
            var all = new List<ResolutionInfo> { new ResolutionInfo(1920, 1080, 59.94) };
            var options = new ResolutionOptions(all, 59.94);
            Assert.AreEqual("1920 x 1080 60 Hz", options.Labels()[0]);
        }

        [Test]
        public void IndexOfFindsCurrent()
        {
            var options = new ResolutionOptions(Sample(), 60);
            Assert.AreEqual(1, options.IndexOf(1920, 1080));
        }

        [Test]
        public void IndexOfUnknownIsZero()
        {
            var options = new ResolutionOptions(Sample(), 60);
            Assert.AreEqual(0, options.IndexOf(800, 600));
        }

        [Test]
        public void TryGetValidIndex()
        {
            var options = new ResolutionOptions(Sample(), 60);
            bool ok = options.TryGet(2, out ResolutionInfo info);
            Assert.IsTrue(ok);
            Assert.AreEqual(2560, info.Width);
            Assert.AreEqual(1440, info.Height);
        }

        [Test]
        public void TryGetNegativeIndex()
        {
            var options = new ResolutionOptions(Sample(), 60);
            Assert.IsFalse(options.TryGet(-1, out ResolutionInfo _));
        }

        [Test]
        public void TryGetIndexTooHigh()
        {
            var options = new ResolutionOptions(Sample(), 60);
            Assert.IsFalse(options.TryGet(3, out ResolutionInfo _));
        }

        [Test]
        public void NoMatchGivesEmptyList()
        {
            var options = new ResolutionOptions(Sample(), 75);
            Assert.AreEqual(0, options.Count);
            Assert.AreEqual(0, options.IndexOf(1920, 1080));
            Assert.IsFalse(options.TryGet(0, out ResolutionInfo _));
        }

        [Test]
        public void NullListGivesEmptyList()
        {
            var options = new ResolutionOptions(null, 60);
            Assert.AreEqual(0, options.Count);
        }
    }
}