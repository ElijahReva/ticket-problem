module TicketProblem.Tests

open TicketProblem
open NUnit.Framework

[<Test>]
let ``hello returns 42`` () =
  let result = Processor.proc 100 "123456789"
  Assert.AreEqual(101, result.Length)
