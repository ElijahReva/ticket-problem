namespace TicketProblem.Host
{
    using System;
    using System.Diagnostics;
    using System.Linq;
    using System.Reactive;
    using System.Reactive.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Windows.Threading;
    using ReactiveUI;

    public class MainViewModel : ReactiveObject
    {
        private readonly ITicketChecker checker = new TicketChecker();

        private readonly ObservableAsPropertyHelper<int> count;
        private readonly ObservableAsPropertyHelper<long> elapsed;
        private readonly DispatcherTimer timer = new DispatcherTimer();
        private readonly Stopwatch sw = new Stopwatch();

        private string number = "123456789";
        private CancellationTokenSource cts = new CancellationTokenSource();

        public MainViewModel()
        {

            var canProcess = this.WhenAnyValue(vm => vm.Number, n => !string.IsNullOrWhiteSpace(n) && n.Length < 10);

            this.ComputeCommandSync = ReactiveCommand.Create(this.Load, canProcess);
            this.ComputeCommandAsync = ReactiveCommand.CreateFromTask(this.LoadAsync, canProcess);

            this.ComputeCommandAsync.ThrownExceptions.Subscribe(this.HandleError);
            this.ComputeCommandSync.ThrownExceptions.Subscribe(this.HandleError);

            this.CancelCommand = ReactiveCommand.Create(() => { this.cts.Cancel(); }, this.ComputeCommandAsync.IsExecuting);

            this.count = this.Output.CountChanged.ToProperty(this, vm => vm.Count);

            this.elapsed = this.CreateTimer().ToProperty(this, vm => vm.Elapsed);
        }

        public ReactiveCommand ComputeCommandSync { get; }
        public ReactiveCommand ComputeCommandAsync { get; }

        public ReactiveCommand CancelCommand { get; }

        public ReactiveList<string> Output { get; } = new ReactiveList<string>();

        public int Count => this.count.Value;
        public long Elapsed => this.elapsed.Value;
        public string Number
        {
            get { return this.number; }
            set { this.RaiseAndSetIfChanged(ref this.number, value); }
        }

        private void Load()
        {

            var result = this.checker.IsLucky(this.number, 15).ToArray();

            using (this.Output.SuppressChangeNotifications())
            {
                this.Output.Clear();
                foreach (var expression in result)
                {
                    this.Output.Add(expression);
                }
            }
        }



        private IObservable<long> CreateTimer()
        {
            return Observable.FromEventPattern(ev => this.timer.Tick += ev, ev => this.timer.Tick -= ev).Select(args =>
            {
                if (!this.sw.IsRunning)
                {
                    return 0;
                }
                return this.sw.ElapsedMilliseconds / 1000;
            });
        }

        private async Task LoadAsync()
        {
            this.Output.Clear();

            this.timer.Start();
            this.sw.Restart();

            var result = await Task.Run(() =>
            {
                return this.checker.IsLucky(this.number, 15).TakeWhile(item => !this.cts.Token.IsCancellationRequested).ToList();
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
        private void HandleError(Exception ex)
        {
            this.Output.Add(ex.ToString());
        }
    }
}
