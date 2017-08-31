namespace TicketProblem.Host
{
    using System;
    using System.Linq;
    using System.Reactive.Linq;
    using ReactiveUI;

    public class MainViewModel : ReactiveObject
    {
        private readonly ITicketChecker checker = new TicketChecker();

        private readonly ObservableAsPropertyHelper<int> count;
        private string number;

        public MainViewModel()
        {

            var canProcess = this.WhenAnyValue(vm => vm.Number, n => !string.IsNullOrWhiteSpace(n));
            this.ComputeCommand = ReactiveCommand.Create(this.Load, canProcess);
            this.ComputeCommand.ThrownExceptions.Subscribe(
                x => this.Output.Add($"OnNext: {x}"));

            this.CancelCommand = ReactiveCommand.Create(() => this.Number = string.Empty);

            this.count = this.Output.CountChanged.ToProperty(this, vm => vm.Count);
        }


        public ReactiveCommand ComputeCommand { get; }

        public ReactiveCommand CancelCommand { get;  }

        public ReactiveList<string> Output { get; } = new ReactiveList<string>();
        
        public int Count => this.count.Value;
        public string Number
        {
            get {return this.number; }
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
