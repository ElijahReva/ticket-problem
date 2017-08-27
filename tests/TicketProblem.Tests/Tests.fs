module TicketProblem.Tests

open TicketProblem
open NUnit.Framework

[<Test>]
let ``hello returns 42`` () =
  let result = Processor.proc 100 "0100000"
  result
  |> List.iter (fun x -> printfn "%s" x)
  Assert.AreEqual(100, result)
