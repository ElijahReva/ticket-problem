namespace TicketProblem

module String =
    /// Converts a string into a list of characters.
    let explode (s:string) =
        [for c in s -> c.ToString()]

    /// Converts a list of characters into a string.
    let implode (xs:string list) =
        let sb = System.Text.StringBuilder(xs.Length)
        xs |> List.iter (sb.Append >> ignore)
        sb.ToString()

module Parser = 
  
  open FParsec

  let ws = spaces 

  let str_ws s = pstring s >>. ws

  let number = pint32 .>> ws

  
  let opp = new OperatorPrecedenceParser<int,unit,unit>()

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
      | Success (v, _, _)     -> v
      | Failure(_) -> -1


  let equals expectedValue r =
      match r with
      | Success (v, _, _) when v = expectedValue -> ()
      | Success (v, _, _)     -> failwith "Math is hard, let's go shopping!"
      | Failure (msg, err, _) -> printf "%s" msg; failwith msg

  let eval = calculate >> value

module Processor = 
    let rec insertions x = function
        | []             -> [[x]]
        | (y :: ys) as l -> (x::l)::(List.map (fun x -> y::x) (insertions x ys))

    let rec permutations = function
        | []      -> seq [ [] ]
        | x :: xs -> Seq.concat (Seq.map (insertions x) (permutations xs))

    let operations = ["*"; "/";"+";"-";""]

    let extendArray length =
        operations @ [for _ in operations.Length..length-1 -> ""]
    
    let flat (input : (string * string) list) = 
        input |> List.fold (fun acc i -> 
            let x,y = i
            (acc @ [x; y])) []


        
    let proc (res: int) (input: string) = 
        let all = extendArray input.Length
        let temp = String.explode input

        all
        |> permutations  
        |> Seq.toList
        |> List.map (fun x ->        
            temp
            |> List.zip x  
            |> flat 
            |> String.implode)
        |> List.map (fun x -> (x, Parser.eval x))
        |> List.filter (fun (_,d) -> d > 80 && d < 120)


        

        

