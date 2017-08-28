namespace TicketProblem.Inter
{
    using System;
    using System.Diagnostics;

    public static class EntryPoint
    {
        public static void Main(string[] args)
        {
            var ticket = new Ticket(100, "123456789");

            var sw = Stopwatch.StartNew();
            var result = ticket.Eval();
            sw.Stop();
            Console.WriteLine($"Elapsed ms: {sw.ElapsedMilliseconds}");

            if (result)
            {
                Console.WriteLine("Lucky you!");
            }
            else
            {
                Console.WriteLine("Try more!");
            }

            Console.ReadKey();
        }
    }
}
