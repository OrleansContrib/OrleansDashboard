namespace OrleansDashboard.Metrics.History
{
    public sealed class RingBuffer<T>
    {
        private readonly T[] items;
        private int startIndex;
        private int count;

        public int Count => count;

        public T this[int index]
        {
            get
            {
                var finalIndex = (startIndex + index) % items.Length;

                return items[finalIndex];
            }
        }

        public RingBuffer(int capacity)
        {
            items = new T[capacity];
        }

        public void Add(T item)
        {
            var newIndex = (startIndex + count) % items.Length;

            items[newIndex] = item;

            if (count < items.Length)
            {
                count++;
            }
            else
            {
                startIndex++;
            }
        }
    }
}
