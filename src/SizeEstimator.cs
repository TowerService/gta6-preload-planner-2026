namespace GTA6PreloadPlanner
{
    /// <summary>
    /// Provides storage size estimates for GTA 6 preload.
    /// </summary>
    public class SizeEstimator
    {
        /// <summary>
        /// Represents a storage estimate range in GB.
        /// </summary>
        public class Estimate
        {
            public int MinGB { get; set; }
            public int MaxGB { get; set; }
        }

        /// <summary>
        /// Edition of the game.
        /// </summary>
        public enum Edition
        {
            Standard,
            Ultimate
        }

        /// <summary>
        /// Target platform.
        /// </summary>
        public enum Platform
        {
            PC,
            PlayStation5,
            XboxSeriesX
        }

        /// <summary>
        /// Gets the estimated storage range for the given edition and platform.
        /// Includes preload overhead (+10%) for temporary files and unpacking.
        /// </summary>
        /// <param name="edition">Game edition.</param>
        /// <param name="platform">Target platform.</param>
        /// <returns>Estimate with MinGB and MaxGB.</returns>
        public Estimate GetEstimate(Edition edition, Platform platform)
        {
            int minGB;
            int maxGB;

            switch (edition)
            {
                case Edition.Ultimate:
                    minGB = 180;
                    maxGB = 200;
                    break;
                case Edition.Standard:
                default:
                    minGB = 150;
                    maxGB = 180;
                    break;
            }

            // Add 10% overhead for preload temporary files and unpacking
            minGB = (int)(minGB * 1.1);
            maxGB = (int)(maxGB * 1.1);

            return new Estimate { MinGB = minGB, MaxGB = maxGB };
        }
    }
}