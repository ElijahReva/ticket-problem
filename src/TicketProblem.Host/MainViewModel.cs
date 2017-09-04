namespace TicketProblem.Host
{
    using System;
    using System.Diagnostics;
    using System.Linq;
    using System.Reactive.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Windows.Threading;
    using ReactiveUI;

    public class MainViewModel : ReactiveObject
    {
        private readonly ITicketChecker checker = new TicketChecker();
        private readonly ObservableAsPropertyHelper<int> count;
        private readonly ObservableAsPropertyHelper<int> totalCombintaions;
        private readonly ObservableAsPropertyHelper<TimeSpan> elapsed;
        private readonly DispatcherTimer timer = new DispatcherTimer();
        private readonly Stopwatch sw = new Stopwatch();

        private int number = 123456789;
        private int expected = 100;

        private CancellationTokenSource cts = new CancellationTokenSource();

        public MainViewModel()
        {
            var canProcess = this.WhenAnyValue(vm => vm.Number, n => n != 0 && n >= this.expected);

            this.ComputeCommandSync = ReactiveCommand.Create(this.Load, canProcess);
            this.ComputeCommandSync.ThrownExceptions.Subscribe(this.HandleError);

            this.ComputeCommandAsync = ReactiveCommand.CreateFromTask(this.LoadAsync, canProcess);
            this.ComputeCommandAsync.ThrownExceptions.Subscribe(this.HandleError);

            this.CancelCommand =
                ReactiveCommand.Create(() => { this.cts.Cancel(); }, this.ComputeCommandAsync.IsExecuting);

            this.count = this.Output.CountChanged.ToProperty(this, vm => vm.Count);
            this.elapsed = this.CreateTimer().ToProperty(this, vm => vm.Elapsed);

            this.totalCombintaions = this.WhenAnyValue(vm => vm.Number)
                .Throttle(TimeSpan.FromMilliseconds(350))
                .Select(n => this.checker.TotalCombinations(n.ToString().Length))
                .ToProperty(this, vm => vm.TotalCombinations);
        }

        public ReactiveCommand ComputeCommandSync { get; }

        public ReactiveCommand ComputeCommandAsync { get; }

        public ReactiveCommand CancelCommand { get; }

        public ReactiveList<string> Output { get; } = new ReactiveList<string>();

        public TimeSpan Elapsed => this.elapsed.Value;

        public int TotalCombinations => this.totalCombintaions.Value;

        public int Count => this.count.Value;

        public int Number
        {
            get => this.number;
            set => this.RaiseAndSetIfChanged(ref this.number, value);
        }

        public int Expected
        {
            get => this.expected;
            set => this.RaiseAndSetIfChanged(ref this.expected, value);
        }

        private void Load()
        {
            this.sw.Restart();

            var result = this.checker.IsLucky(this.number.ToString(), this.expected).ToArray();

            this.sw.Stop();

            using (this.Output.SuppressChangeNotifications())
            {
                this.Output.Clear();
                foreach (var expression in result)
                {
                    this.Output.Add(expression);
                }
            }
        }

        private async Task LoadAsync()
        {
            this.Output.Clear();

            this.timer.Start();
            this.sw.Restart();

            var result = await Task.Run(() =>
            {
                return this.checker.IsLucky(this.number.ToString(), this.expected).TakeWhile(item => !this.cts.Token.IsCancellationRequested).ToList();
            });

            this.sw.Stop();
            this.timer.Stop();

            using (this.Output.SuppressChangeNotifications())
            {
                foreach (var expression in result.ToArray())
                {
                    this.Output.Add(expression);
                }
            }

            this.cts = new CancellationTokenSource();
        }

        private void HandleError(Exception ex) => this.Output.Add(ex.ToString());

        private IObservable<TimeSpan> CreateTimer() => Observable
            .FromEventPattern(
                ev => this.timer.Tick += ev,
                ev => this.timer.Tick -= ev)
            .Select(args => !this.sw.IsRunning ? TimeSpan.Zero : this.sw.Elapsed);
    }
}
