namespace TicketProblem.Host
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reactive;
    using System.Reactive.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Reactive.Concurrency;
    using System.Reactive.Disposables;
    using System.Reactive.Subjects;
    using ReactiveUI;

    public class MainViewModel : ReactiveObject
    {
        private readonly ITicketChecker checker = new TicketChecker();

        private readonly ObservableAsPropertyHelper<int> count;
        private string number;

        public MainViewModel()
        {

            var canProcess = this.WhenAnyValue(vm => vm.Number, n => !string.IsNullOrWhiteSpace(n) && n.Length < 10);
            this.ComputeCommandSync = ReactiveCommand.Create(this.Load, canProcess);
            this.ComputeCommandAsync = ReactiveCommand.CreateFromObservable(
                () => Observable
                    .StartAsync(this.LoadAsync)
                    .TakeUntil((IObservable<string>)this.CancelCommand), canProcess);

            this.ComputeCommandAsync.ThrownExceptions.Subscribe(this.HandleError);
            this.ComputeCommandSync.ThrownExceptions.Subscribe(this.HandleError);

            this.CancelCommand = ReactiveCommand.Create(() => this.Number = string.Empty, this.ComputeCommandAsync.IsExecuting);

            this.count = this.Output.CountChanged.ToProperty(this, vm => vm.Count);
        }

        private async Task LoadAsync(CancellationToken ct)
        {
            this.Output.Clear();
            var result = await Task.Run(() => this.checker.IsLuckyAsync(this.number, 15, ct));

            using (this.Output.SuppressChangeNotifications())
            {
                
                foreach (var expression in result.ToArray())
                {
                    this.Output.Add(expression);
                }
            }
        }

        public void HandleError(Exception ex)
        {
            this.Output.Add(ex.ToString());
        }


        public ReactiveCommand ComputeCommandSync { get; }
        public ReactiveCommand ComputeCommandAsync { get; }

        public ReactiveCommand CancelCommand { get; }

        public ReactiveList<string> Output { get; } = new ReactiveList<string>();

        public int Count => this.count.Value;
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
    }
}
