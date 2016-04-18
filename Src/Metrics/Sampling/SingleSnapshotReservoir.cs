using System;
using System.Linq;
using Metrics.Utils;

namespace Metrics.Sampling
{
    public sealed class SingleSnapshotReservoir : Reservoir
    {
        private const int DefaultSize = 1028;

        private Reservoir reservoir;

        public SingleSnapshotReservoir()
            : this(DefaultSize) { }

        public SingleSnapshotReservoir(int size)
        {
            reservoir = new SlidingWindowReservoir(size);
        }


        public long Count { get { return reservoir.Count; } }

        public int Size { get { return reservoir.Size; } }

        public void Update(long value, string userValue = null)
        {
            reservoir.Update(value, userValue);
        }

        public Snapshot GetSnapshot(bool resetReservoir = false)
        {
            return reservoir.GetSnapshot(true);
        }

        public void Reset()
        {
            reservoir.Reset();
        }
    }
}
