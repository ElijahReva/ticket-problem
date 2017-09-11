namespace TicketProblem

open System.Collections.Generic

type OperatorMapping = 
    { trigger : string
      description : string }

type OperatorWithPosition = 
    { operator : OperatorMapping
      position : int }

type Result = 
    { operators : IEnumerable<OperatorWithPosition>
      expression : string
      evaluated : float }

module Model = 
    open FParsec
    
    let avaliableOperators : IEnumerable<Operator<float, unit, unit>> = 
        seq { 
            yield upcast InfixOperator("+", spaces, 1, Associativity.Left, (+))
            yield upcast InfixOperator("-", spaces, 1, Associativity.Left, (-))
            yield upcast InfixOperator("*", spaces, 2, Associativity.Left, (*))
            yield upcast InfixOperator("/", spaces, 2, Associativity.Left, (/))
        }
    
    let operatorMap = 
        [ "+", "Addition"
          "-", "Subtraction"
          "*", "Multiplication"
          "/", "Division" ]
        |> Map.ofList
    
    let operatorsSeq = 
        avaliableOperators |> Seq.map (fun x -> 
                                  { trigger = x.String
                                    description = operatorMap.[x.String] })
