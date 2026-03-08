namespace WeatherApp.Services
{
    public class WeatherVisitCounter
    {
        private int _count = 0;

        public int Increment() => Interlocked.Increment(ref _count);
        public int Count => _count;
    }
}
