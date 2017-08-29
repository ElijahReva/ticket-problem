namespace TicketProblem.Host
{
    using System;
    using System.Reactive.Disposables;
    using System.Reactive.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using ReactiveUI;

    public class MainViewModel : ReactiveObject
    {
        public ReactiveList<string> Output { get; } = new ReactiveList<string>();

        public MainViewModel()
        {
            this.ComputeCommand = ReactiveCommand.Create(this.Load);
            this.CancelCommand = ReactiveCommand.Create(() => { });
        }

        public ReactiveCommand ComputeCommand { get; private set; }

        public ReactiveCommand CancelCommand { get; private set; }

        private void Load()
        {
            this.Output.Add("Test");
        }
    }
}
