namespace PowerManagerConfig.Tests
{
    internal abstract class AbstractMockTextReader : TextReader
    {
        public abstract Task InitializeAsync();

        protected readonly Queue<string> queue = new Queue<string>();

        public override async Task<string?> ReadLineAsync()
        {
            return await Task.FromResult(queue.Count > 0 ? queue.Dequeue() : null);
        }
    }
}
