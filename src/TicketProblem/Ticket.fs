namespace TicketProblem

open Processor
open TicketProblem

type Ticket(expected : float, number : string) = 
    let mutable results = []
    let expected = expected
    
    let eval = 
        lazy (results <- proc expected number |> Seq.toList
              if results.Length > 0 then true
              else false)
    
    member this.Results = 
        this.Eval() |> ignore
        results
    
    member this.Eval() = eval.Value
