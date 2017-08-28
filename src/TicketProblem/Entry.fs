open System
open System.Diagnostics
open TicketProblem

[<EntryPoint>]
let main args = 

    let tic = Ticket(100., "123456789")

    let sw = Stopwatch.StartNew()
    let res = tic.Eval()
    sw.Stop()

    match res with
    | true -> printfn "Lucky you!"
    | false -> printfn "Buy another!"

    printfn "Total time spent(ms): %i" sw.ElapsedMilliseconds
    printfn "Total possibilities: %i" <| tic.Results.Length
    
    Console.ReadKey() |> ignore
    0
