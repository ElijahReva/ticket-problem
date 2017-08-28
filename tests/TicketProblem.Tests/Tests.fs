module TicketProblem.Tests

open TicketProblem
open NUnit.Framework
open System.Linq

[<Test>]
let ``proc 100 count`` () =
  let result = Processor.proc 100. "123456789"
  Assert.AreEqual(162, result.Count())

[<Test>]
let ``eval 1 + 02`` () =
  let result = Parser.eval "1 + 0002"
  Assert.AreEqual(3., result)
  
  
[<Test>]
let ``proc 10 count`` () =
  let result = Processor.proc 10. "010"
  Assert.AreEqual(3, result.Count())
