namespace TicketProblem

open Parser
open Processor
open System
open System.Collections.Generic
open System.Threading
open System.Threading.Tasks
open TicketProblem
open System.Reactive.Linq

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
    abstract IsLuckyObs : string -> int -> IObservable<string>
    abstract TotalCombinations : int -> int
    abstract TotalCombinationsObs : int -> IObservable<int>

    abstract GetAllExpressions : string -> IEnumerable<string>
    abstract EvalAndCheck : string -> int -> bool
    

type TicketChecker() = 
    
    interface ITicketChecker with
        member this.EvalAndCheck expression expected = Parser.eval expression |> Parser.filter (float expected)
        member this.GetAllExpressions number = Processor.getall number

        member x.TotalCombinations length = Processor.getcomb length
        member x.IsLucky number expected = proc (float expected) number
        member x.IsLuckyObs number expected = (proc (float expected) number).ToObservable()
        member x.TotalCombinationsObs n = failwith "Not implemented yet"
