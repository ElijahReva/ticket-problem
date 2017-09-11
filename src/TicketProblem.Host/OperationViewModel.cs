namespace TicketProblem.Host
{
    using ReactiveUI;

    public class OperationViewModel : ReactiveObject
    {
        private bool selected;

        public OperationViewModel(OperatorMapping model)
        {
            this.Model = model;
            this.selected = true;
        }

        public OperatorMapping Model { get; }

        public bool Selected
        {
            get { return this.selected; }
            set { this.RaiseAndSetIfChanged(ref this.selected, value); }
        }
    }
}
