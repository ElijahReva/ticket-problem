module Entry

open TicketProblem
open Parser
open System
open System.Diagnostics

[<EntryPoint>]
let main args = 
    
    let sw = Stopwatch.StartNew()
    let result = Processor.proc 100 "123456789"
    sw.Stop()
    List.iter (fun (x,y) -> printfn "%s = %i" x y) <| result

    printfn "Total time spent(ms): %i" sw.ElapsedMilliseconds
    printfn "Total possibilities: %i" result.Length
    Console.ReadKey() |> ignore
    0

