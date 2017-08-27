module Entry

open TicketProblem
open Parser
open System

[<EntryPoint>]
let main args = 
    
    List.iter (fun x -> printfn "%s" x) <| Processor.proc 100 "01000110"
    Console.ReadKey() |> ignore
    0

