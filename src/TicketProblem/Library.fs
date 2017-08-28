namespace TicketProblem

module String = 

    let explode (s : string) = 
        [ for c in s -> c.ToString() ]
    
    let implode (xs : string list) = 
        let sb = System.Text.StringBuilder(xs.Length)
        xs |> List.iter (sb.Append >> ignore)
        sb.ToString()

module Parser = 
    open FParsec
    
    let ws = spaces
    let str_ws s = pstring s >>. ws
    let number = pint32 .>> ws
    let opp = new OperatorPrecedenceParser<int, unit, unit>()
    let expr = opp.ExpressionParser
    
    opp.TermParser <- number
    opp.AddOperator(InfixOperator("+", ws, 1, Associativity.Left, (+)))
    opp.AddOperator(InfixOperator("-", ws, 1, Associativity.Left, (-)))
    opp.AddOperator(InfixOperator("*", ws, 2, Associativity.Left, (*)))
    opp.AddOperator(InfixOperator("/", ws, 2, Associativity.Left, (/)))
    opp.AddOperator(PrefixOperator("-", ws, 4, true, fun x -> -x))
    
    let completeExpression = ws >>. expr .>> eof
    let calculate s = run completeExpression s
    
    let value r = 
        match r with
        | Success(v, _, _) -> v
        | Failure(_) -> -1
    
    let equals expectedValue r = 
        match r with
        | Success(v, _, _) when v = expectedValue -> ()
        | Success(v, _, _) -> failwith "Math is hard, let's go shopping!"
        | Failure(msg, err, _) -> 
            printf "%s" msg
            failwith msg
    
    let eval = calculate >> value

module Processor = 

    let rec permutationsWithRep m l = 
        seq { 
            if m = 1 then 
                for v in l do
                    yield [ v ]
            else 
                for s in permutationsWithRep (m - 1) l do
                    for v in l do
                        yield v :: s
        }
    
    let operations = [ "*"; "/"; "+"; "-"; "" ]
    
    let flat (input : (string * string) list) = 
        input |> List.fold (fun acc i -> 
                     let x, y = i
                     (acc @ [ x; y ])) []
    
    let proc (res : int) (input : string) = 
        let temp = String.explode input
        operations
        |> permutationsWithRep (input.Length - 1)
        |> Seq.map (fun x -> "" :: x)
        |> Seq.map (fun x -> 
               temp
               |> List.zip x
               |> flat
               |> String.implode)
        |> Seq.distinct
        |> Seq.map (fun x -> (x, Parser.eval x))
        |> Seq.filter (fun (_, d) -> d = res)
    
