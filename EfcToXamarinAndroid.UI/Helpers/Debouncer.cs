namespace EfcToXamarinAndroid.UI.Helpers
{
    /// <summary>
    /// Debouncer для предотвращения частых вызовов тяжелых операций.
    /// Используется для оптимизации пересчета статистики.
    /// </summary>
    public class Debouncer : IDisposable
    {
        private CancellationTokenSource? _cts;
        private readonly int _milliseconds;

        /// <summary>
        /// Создает новый Debouncer с указанной задержкой
        /// </summary>
        /// <param name="milliseconds">Задержка в миллисекундах (по умолчанию 300ms)</param>
        public Debouncer(int milliseconds = 300)
        {
            _milliseconds = milliseconds;
        }

        /// <summary>
        /// Выполняет действие с debouncing - отменяет предыдущие вызовы,
        /// ждет указанное время, затем выполняет действие
        /// </summary>
        public async Task DebounceAsync(Func<Task> action)
        {
            // Отменяем предыдущий вызов, если он был
            _cts?.Cancel();
            _cts = new CancellationTokenSource();

            try
            {
                await Task.Delay(_milliseconds, _cts.Token);
                await action();
            }
            catch (TaskCanceledException)
            {
                // Ожидаемое исключение при debounce
            }
        }

        public void Dispose()
        {
            _cts?.Cancel();
            _cts?.Dispose();
        }
    }
}
