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
    using System.Windows.Threading;
    using MahApps.Metro.Controls.Dialogs;
    using ReactiveUI;

    public class MainViewModel : ReactiveObject
    {
        private readonly ITicketChecker checker = new TicketChecker();
        private readonly IDialogCoordinator dialogs = DialogCoordinator.Instance;
        private readonly ObservableAsPropertyHelper<string> operationHeader;
        private HashSet<OperatorMapping> selectedOperator = new HashSet<OperatorMapping>();
        private readonly ObservableAsPropertyHelper<int> count;
        private readonly ObservableAsPropertyHelper<int> totalCombintaions;
        private readonly ObservableAsPropertyHelper<TimeSpan> elapsed;
        private event EventHandler CheckStopwatch;
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
                ReactiveCommand.Create(() => { this.cts.Cancel(); });

            this.count = this.Output.CountChanged.ToProperty(this, vm => vm.Count);
            this.elapsed = this.CreateTimer().ToProperty(this, vm => vm.Elapsed);

            this.totalCombintaions = this.WhenAnyValue(vm => vm.Number)
                .Throttle(TimeSpan.FromMilliseconds(350))
                .Select(n => this.checker.TotalCombinations(n.ToString()))
                .ToProperty(this, vm => vm.TotalCombinations);

            this.Operations = new ReactiveList<OperationViewModel>()
            {
                ChangeTrackingEnabled = true
            };

            this.operationHeader =
                this.Operations.ItemChanged
                .Where(i => i.PropertyName == nameof(OperationViewModel.Selected))
                .Select(i => i.Sender)
                .Merge(this.Operations.ItemsAdded)
                .Do(v =>
                    {
                        if (v.Selected)
                        {
                            this.selectedOperator.Add(v.Model);
                        }
                        else
                        {
                            this.selectedOperator.Remove(v.Model);
                        }
                    })
                .Select(_ =>
                    {
                        switch (this.selectedOperator.Count)
                        {
                            case 0:
                                return "<none>";
                            case 1:
                                return this.selectedOperator.First().description;
                            default:
                                return string.Join(" ", this.selectedOperator.Select(o => o.trigger));

                        }
                    })
                .ToProperty(this, vm => vm.OperationsHeader);

            this.Operations.AddRange(this.checker.Operations.Select(m => new OperationViewModel(m)));
        }

        public ReactiveCommand ComputeCommandSync { get; }

        public ReactiveCommand ComputeCommandAsync { get; }

        public ReactiveCommand ComputeParallel { get; }

        public ReactiveCommand ComputeAsObservable { get; }

        public ReactiveCommand CancelCommand { get; }

        public ReactiveList<string> Output { get; } = new ReactiveList<string>();

        public ReactiveList<OperationViewModel> Operations { get; }

        public string OperationsHeader => this.operationHeader.Value;

        public TimeSpan Elapsed => this.elapsed.Value;

        public int TotalCombinations => this.totalCombintaions.Value;

        public int Count => this.count.Value;

        public int Number
        {
            get { return this.number; }
            set { this.RaiseAndSetIfChanged(ref this.number, value); }
        }

        public int Expected
        {
            get { return this.expected; }
            set { this.RaiseAndSetIfChanged(ref this.expected, value); }
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

        private async Task LoadAsync()
        {
            var mySettings = new MetroDialogSettings()
            {
                NegativeButtonText = "Stop",
                AnimateShow = false,
                AnimateHide = false,
                ColorScheme = MetroDialogColorScheme.Accented
            };

            var controller = await this.dialogs.ShowProgressAsync(this, "Processing", "Generating all expressions", true, mySettings);
            controller.Canceled += (sender, args) => this.cts.Cancel();
            controller.SetIndeterminate();

            this.Output.Clear();

            this.timer.Start();
            this.sw.Restart();

            var expressions = await Task.Run(() => this.checker.GetAllExpressions(this.Number.ToString()).TakeWhile(_ => !this.cts.IsCancellationRequested).ToArray());

            controller.Minimum = 0;
            controller.Maximum = expressions.Length;
            var i = 0;
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
                                controller.SetProgress(i++);
                                controller.SetMessage($"Expressions founded: {i}");
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

            await controller.CloseAsync();
        }

        private async Task LoadParallel()
        {

            var mySettings = new MetroDialogSettings()
            {
                NegativeButtonText = "Stop",
                AnimateShow = false,
                AnimateHide = false,
                ColorScheme = MetroDialogColorScheme.Accented
            };

            var controller = await this.dialogs.ShowProgressAsync(this, "Processing", "Generating all expressions", true, mySettings);
            controller.Canceled += (sender, args) => this.cts.Cancel();
            controller.SetIndeterminate();

            this.Output.Clear();


            await Task.Run(async () =>
            {
                controller.Minimum = 0;
                controller.Maximum = this.TotalCombinations;
                var i = 0;
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
                                        controller.SetProgress(i++);
                                        controller.SetMessage($"Expressions founded: {i}");
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

            await controller.CloseAsync();
        }

        private async Task LoadObservable()
        {
            var mySettings = new MetroDialogSettings()
            {
                NegativeButtonText = "Stop",
                AnimateShow = false,
                AnimateHide = false,
                ColorScheme = MetroDialogColorScheme.Accented
            };

            var controller = await this.dialogs.ShowProgressAsync(this, "Processing", "Generating all expressions", true, mySettings);
            controller.Canceled += (sender, args) => this.cts.Cancel();
            controller.Minimum = 0;
            controller.Maximum = this.TotalCombinations;
            var j = 0;

            this.Output.Clear();

            this.timer.Start();
            this.sw.Restart();


            this.checker
                .IsLuckyObs(this.Number.ToString(), this.Expected)
                .ObserveOn(RxApp.MainThreadScheduler)
                .SubscribeOn(RxApp.TaskpoolScheduler)
                .TakeWhile(_ => !this.cts.Token.IsCancellationRequested)
                .Subscribe(
                    i =>
                    {
                        this.Output.Add(i);
                        controller.SetProgress(j++);
                        controller.SetMessage($"Expressions founded: {j}");
                    },
                    async () =>
                    {
                        this.sw.Stop();
                        this.timer.Stop();
                        this.cts = new CancellationTokenSource();
                        await controller.CloseAsync();
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