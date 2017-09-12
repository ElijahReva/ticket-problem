namespace TicketProblem

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
    
    type Parser() = 
        let parser = new OperatorPrecedenceParser<float, unit, unit>()
        member this.eval = 199
    
    let opp = new OperatorPrecedenceParser<float, unit, unit>()
    let expr = opp.ExpressionParser
    
    opp.TermParser <- number

    Model.avaliableOperators |> Seq.iter opp.AddOperator

    opp.AddOperator(PrefixOperator("-", ws, 4, true, fun x -> -x))
    
    let completeExpression = ws >>. expr .>> eof
    let eval input = run completeExpression input
    
    let filter expected result = 
        match result with
        | Success(value, _, _) when value = expected -> true
        | Success(_) -> false
        | Failure(_) -> false

    let getValue result = 
        match result with
        | Success(value, _, _) -> value
        | Failure(_) -> 0.