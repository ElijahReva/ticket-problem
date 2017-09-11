namespace TicketProblem

module String = 
    let explode (s : string) = 
        [ for c in s -> c.ToString() ]
    
    let implode (xs : string list) = 
        let sb = System.Text.StringBuilder(xs.Length)
        xs |> List.iter (sb.Append >> ignore)
        sb.ToString()


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

    let getcomb (str: string) = (str |> Seq.length ) - 1 |> pown operations.Length  |> (*) 2
    

    