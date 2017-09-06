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
    open FParsec.Error
    open FParsec.Primitives
    
    let pNoZeroFloat : Parser<float, unit> = 
        let parser = numberLiteral NumberLiteralOptions.DefaultFloat "number"
        fun stream -> 
            let reply = parser stream
            if reply.Status = Ok then 
                let nl = reply.Result
                if nl.String = "0" then Reply(0.)
                else if nl.String.[0] = '0' then Reply(Error, messageError "Leading Zero")
                else Reply(float nl.String)
            else Reply(reply.Status, reply.Error)
    
    let ws = spaces
    let str_ws s = pstring s >>. ws
    let number = pNoZeroFloat .>> ws
    let opp = new OperatorPrecedenceParser<float, unit, unit>()
    let expr = opp.ExpressionParser
    
    opp.TermParser <- number
    opp.AddOperator(InfixOperator("+", ws, 1, Associativity.Left, (+)))
    opp.AddOperator(InfixOperator("-", ws, 1, Associativity.Left, (-)))
    opp.AddOperator(InfixOperator("*", ws, 2, Associativity.Left, (*)))
    opp.AddOperator(InfixOperator("/", ws, 2, Associativity.Left, (/)))
    opp.AddOperator(PrefixOperator("-", ws, 4, true, fun x -> -x))
    
    let completeExpression = ws >>. expr .>> eof
    let eval input = run completeExpression input
    
    let filter expected result = 
        match result with
        | Success(value, _, _) when value = expected -> true
        | Success(_) -> false
        | Failure(_) -> false

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
    
    let flat (input : (string * string) seq) = 
        input |> Seq.fold (fun acc i -> 
                     let x, y = i
                     (acc @ [ x; y ])) []
    
    let append xs ys = 
        seq { 
            for x in xs do
                for y in ys -> Seq.append [ x ] y
        }
    
    let getallexpr input operations = 
        let temp = String.explode input
        operations
        |> permutationsWithRep (input.Length - 1)
        |> append [ ""; "-" ]
        |> Seq.map (fun x -> 
               temp
               |> Seq.zip x
               |> flat
               |> String.implode)
  
    let getall number = getallexpr number operations
    
    let evalAll (input : string) operations = 
        getallexpr input operations
        |> Seq.map (fun x -> (x, Parser.eval x))  
    
    let procWithCustom (expected : float) (input : string) operations = 
        evalAll input operations
        |> Seq.filter (fun (_, result) -> Parser.filter expected result)
        |> Seq.map (fun (expression, _) -> expression)
    
    let proc (res : float) (input : string) = procWithCustom res input operations

    let (./.) x y = 
        (x |> double) / (y |> double) |> int

    let choose n k = 
        let rec choose' n k = 
            match k with 
            | 0 -> 1
            | k when k > n ./. 2 -> choose' n (n-k)
            | k -> n * (choose' (n-1) (k-1)) ./. k
        choose' (max n k) (min n k)

    let combWithRep n k = 
        choose (n + k - 1) k     

    let getcomb n = combWithRep n operations.Length
    

    