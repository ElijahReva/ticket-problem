module TicketProblem.Tests

open TicketProblem
open NUnit.Framework
open System.Linq

[<Test>]
let ``proc 100 in standart expect 162`` () =
  let result = Processor.proc 100. "123456789"
  Assert.AreEqual(162, result.Count())

[<Test>]
let ``eval 1 + 02 = -1`` () =
  let result = Parser.eval "1 + 0002"
  Assert.AreEqual(-1., result) 

[<Test>]
let ``eval 1 + 0 + 2 = 3`` () =
  let result = Parser.eval "1 + 0 + 2"
  Assert.AreEqual(3., result) 
  

[<Test>]
let ``proc 2 1001 expect 4`` () =
  let result = Processor.procWithCustom 2. "1001" ["";"+";"-"] |> Seq.toArray
  Assert.AreEqual(4, result.Length)

[<Test>]
let ``proc 2001 in standart expect 2`` () =
  let result = Processor.proc 2001. "123456789" |> Seq.toArray
  let ``no -`` = result
                    |> Array.filter (fun x -> not <| x.StartsWith("-"))

  Assert.AreEqual(5, result.Length)
  Assert.AreEqual(2, ``no -``.Length)
