namespace TicketProblem

open Processor
open TicketProblem
open System.Threading.Tasks
open System

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
    
    abstract member IsLucky : string -> int -> System.Collections.Generic.IEnumerable<string>    

    abstract member IsLuckyAsync : string -> int -> Task<Ticket>

    abstract member IsLuckyObs : string -> int -> IObservable<Ticket>

type TicketChecker() = 

    interface ITicketChecker with

        member x.IsLucky(number: string) (expected: int): System.Collections.Generic.IEnumerable<string>  = 
            let f = float expected
            proc f number


        
        member x.IsLuckyAsync arg1 arg2 = failwith "Not implemented yet"
        member x.IsLuckyObs arg1 arg2 = failwith "Not implemented yet"