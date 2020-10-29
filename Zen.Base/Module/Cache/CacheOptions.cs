using System;

namespace Zen.Base.Module.Cache
{
    /// <summary>
    ///     Defines the caching behavior for a given item.
    /// </summary>
    public class CacheOptions
    {
        private TimeSpan? _lifeTimeSpan = TimeSpan.FromSeconds(600);

        public CacheOptions() { }

        public CacheOptions(int seconds) => _lifeTimeSpan = TimeSpan.FromSeconds(seconds);

        /// <summary>
        ///     Gets or sets an absolute expiration time, relative to now.
        /// </summary>
        public TimeSpan? LifeTimeSpan
        {
            get => _lifeTimeSpan;
            set
            {
                if (value <= TimeSpan.Zero)
                    throw new ArgumentOutOfRangeException(nameof(LifeTimeSpan), value, @"The relative expiration value must be positive.");

                _lifeTimeSpan = value;
            }
        }
    }
}