namespace TicketProblem

open Model
open System
open System.Collections.Generic
open TicketProblem
open System.Reactive.Linq

type ITicketChecker = 

    abstract IsLucky : string -> int -> IEnumerable<Result>
    abstract IsLuckyObs : string -> int -> IObservable<Result>

    abstract TotalCombinations : int * int-> int
    abstract TotalCombinationsObs : string -> IObservable<int>

    abstract GetAllExpressions : string -> IEnumerable<string>
    abstract EvalAndCheck : string -> int -> bool

    abstract Operations : IEnumerable<OperatorMapping>
    abstract Process : int -> IEnumerable<OperatorMapping> -> IEnumerable<Result>
    

type TicketChecker() = 
    
    interface ITicketChecker with
        member this.Operations = Model.operatorsSeq
        member this.Process number choosedOperations = Processor.getallbyop number choosedOperations
            
        
        member this.EvalAndCheck expression expected = Parser.eval expression |> Parser.filter (float expected)
        member this.GetAllExpressions number = Processor.getall number

        member this.TotalCombinations (length, count) = Processor.getcomb (length, count)
        member this.IsLucky number expected = Processor.proc (float expected) number
        member this.IsLuckyObs number expected = (Processor.proc (float expected) number).ToObservable()
        member this.TotalCombinationsObs n = failwith "Not implemented yet"
