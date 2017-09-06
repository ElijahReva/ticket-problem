namespace TicketProblem.Host
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Reactive.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Threading;
    using ReactiveUI;

    public class MainViewModel : ReactiveObject
    {
        private readonly ITicketChecker checker = new TicketChecker();
        private readonly ObservableAsPropertyHelper<int> count;
        private readonly ObservableAsPropertyHelper<int> totalCombintaions;
        private readonly ObservableAsPropertyHelper<TimeSpan> elapsed;
        public event EventHandler CheckStopwatch;
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

            this.ComputeParallel = ReactiveCommand.CreateFromTask(this.LoadParallel, canProcess);
            this.ComputeParallel.ThrownExceptions.Subscribe(this.HandleError);

            this.ComputeAsObservable = ReactiveCommand.Create(this.LoadObservable, canProcess);
            this.ComputeAsObservable.ThrownExceptions.Subscribe(this.HandleError);

            this.CancelCommand =
                ReactiveCommand.Create(() =>
                    {
                        this.cts.Cancel();
                    });

            this.count = this.Output.CountChanged.ToProperty(this, vm => vm.Count);
            this.elapsed = this.CreateTimer().ToProperty(this, vm => vm.Elapsed);

            this.totalCombintaions = this.WhenAnyValue(vm => vm.Number)
                .Throttle(TimeSpan.FromMilliseconds(350))
                .Select(n => this.checker.TotalCombinations(n.ToString().Length))
                .ToProperty(this, vm => vm.TotalCombinations);
        }

        public ReactiveCommand ComputeCommandSync { get; }

        public ReactiveCommand ComputeCommandAsync { get; }

        public ReactiveCommand ComputeParallel { get; }

        public ReactiveCommand ComputeAsObservable { get; }

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
            this.CheckStopwatch?.Invoke(null, null);

            var result = this.checker.IsLucky(this.number.ToString(), this.expected).ToArray();



            using (this.Output.SuppressChangeNotifications())
            {
                this.Output.Clear();
                foreach (var expression in result)
                {
                    this.Output.Add(expression);
                }
            }

            this.sw.Stop();
            this.CheckStopwatch?.Invoke(null, null);
        }

        private async Task LoadParallel()
        {
            this.Output.Clear();


            await Task.Run(async () =>
            {
                this.timer.Start();
                this.sw.Restart();
                var tasks = new List<Task>();
                using (var enumerator = this.checker.GetAllExpressions(this.Number.ToString()).GetEnumerator())
                {
                    while (enumerator.MoveNext() && !this.cts.IsCancellationRequested)
                    {
                        var expr = enumerator.Current;
                        var task = Task.Factory.StartNew(() =>
                            {
                                if (!this.cts.IsCancellationRequested && this.checker.EvalAndCheck(expr, this.Expected))
                                {
                                    DispatchService.Invoke(() =>
                                    {
                                        this.Output.Add(expr);
                                    });
                                }
                            },
                            this.cts.Token);
                        tasks.Add(task);
                    }
                }

                if (this.cts.IsCancellationRequested)
                {
                    this.sw.Stop();
                    this.timer.Stop();
                    this.cts = new CancellationTokenSource();
                }
                else
                {

                    await Task.Factory.ContinueWhenAll(tasks.ToArray(), r =>
                    {
                        this.sw.Stop();
                        this.timer.Stop();

                        this.cts = new CancellationTokenSource();
                    });
                }
            }, this.cts.Token);
        }

        private async Task LoadAsync()
        {
            this.Output.Clear();

            this.timer.Start();
            this.sw.Restart();

            var expressions = await Task.Run(() => this.checker.GetAllExpressions(this.Number.ToString()).ToArray());

            var tasks = new List<Task>();
            foreach (var expr in expressions)
            {
                var task = Task.Factory.StartNew(() =>
                {
                    if (!this.cts.IsCancellationRequested && this.checker.EvalAndCheck(expr, this.Expected))
                    {
                        DispatchService.Invoke(() =>
                        {
                            this.Output.Add(expr);
                        });
                    }
                },
                this.cts.Token);
                tasks.Add(task);
            }

            await Task.Factory.ContinueWhenAll(tasks.ToArray(), r =>
            {
                this.sw.Stop();
                this.timer.Stop();

                this.cts = new CancellationTokenSource();
            });
        }

        private void LoadObservable()
        {
            this.Output.Clear();

            this.timer.Start();
            this.sw.Restart();


            this.checker
                .IsLuckyObs(this.Number.ToString(), this.Expected)
                .ObserveOn(RxApp.MainThreadScheduler)
                .SubscribeOn(RxApp.TaskpoolScheduler)
                .TakeWhile(_ => !this.cts.Token.IsCancellationRequested)
                .Subscribe(
                i => this.Output.Add(i),
                () =>
                {
                    this.sw.Stop();
                    this.timer.Stop();
                    this.cts = new CancellationTokenSource();
                });
        }

        private void HandleError(Exception ex) => this.Output.Add(ex.ToString());

        private IObservable<TimeSpan> CreateTimer() => Observable
            .FromEventPattern(
                ev => this.timer.Tick += ev,
                ev => this.timer.Tick -= ev)
            .Merge(Observable.FromEventPattern(
                ev => this.CheckStopwatch += ev,
                ev => this.CheckStopwatch -= ev
                ))
            .Select(args => this.sw.Elapsed);
    }

    public static class DispatchService
    {
        public static void Invoke(Action action)
        {
            Dispatcher dispatchObject = Application.Current.Dispatcher;
            if (dispatchObject == null || dispatchObject.CheckAccess())
            {
                action();
            }
            else
            {
                dispatchObject.Invoke(action);
            }
        }
    }
}