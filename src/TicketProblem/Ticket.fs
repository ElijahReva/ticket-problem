namespace TicketProblem

open Processor
open Model
open System
open System.Collections.Generic
open TicketProblem
open System.Reactive.Linq

type ITicketChecker = 
    abstract IsLucky : string -> int -> IEnumerable<string>
    abstract IsLuckyObs : string -> int -> IObservable<string>
    abstract TotalCombinations : string -> int
    abstract TotalCombinationsObs : string -> IObservable<int>

    abstract GetAllExpressions : string -> IEnumerable<string>
    abstract EvalAndCheck : string -> int -> bool

    abstract Operations : IEnumerable<OperatorMapping>
    abstract Process : int -> IEnumerable<OperatorMapping> -> IEnumerable<Result>
    

type TicketChecker() = 
    
    interface ITicketChecker with
        member this.Operations = Model.operatorsSeq
        member this.Process number choosedOperations = failwith "Not implemented yet" 
        
        member this.EvalAndCheck expression expected = Parser.eval expression |> Parser.filter (float expected)
        member this.GetAllExpressions number = Processor.getall number

        member x.TotalCombinations length = Processor.getcomb length
        member x.IsLucky number expected = proc (float expected) number
        member x.IsLuckyObs number expected = (proc (float expected) number).ToObservable()
        member x.TotalCombinationsObs n = failwith "Not implemented yet"
