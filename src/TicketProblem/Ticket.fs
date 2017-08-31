namespace TicketProblem

open Processor
open System
open System.Collections.Generic
open System.Threading
open TicketProblem

type Ticket() = 
    let mutable results = []
    
    let eval = 
        lazy (results <- proc 100. "123123" |> Seq.toList
              if results.Length > 0 then true
              else false)
    
    member this.Results = 
        this.Eval() |> ignore
        results
    
    member this.Eval() = eval.Value

type ITicketChecker = 
    abstract IsLucky : string -> int -> IEnumerable<string>
    abstract IsLuckyAsync : string -> int -> CancellationToken -> IEnumerable<string>
    abstract IsLuckyObs : string -> int -> IObservable<string>

type TicketChecker() = 
    interface ITicketChecker with
        
        member x.IsLucky number expected = 
            let f = float expected
            proc f number
        
        member x.IsLuckyAsync number expected token = procAsync token (float expected) number
        member x.IsLuckyObs number expected = failwith "Not implemented yet"
