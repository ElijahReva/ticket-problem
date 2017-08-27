module Entry

open TicketProblem
open Parser
open System

[<EntryPoint>]
let main args = 
    
    List.iter (fun (x,y) -> printfn "%s = %i" x y) <| Processor.proc 100 "123456789"
    Console.ReadKey() |> ignore
    0

