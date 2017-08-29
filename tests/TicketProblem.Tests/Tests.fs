module TicketProblem.Tests

open TicketProblem
open NUnit.Framework
open System.Linq
open FParsec.CharParsers 

let is (expected: float) (result: ParserResult<float,unit>) = function
    | Success(value,_,_) -> Assert.AreEqual(expected, value)
    | Failure(error,_,_) -> Assert.Fail(error)

let failed result = function
    | Success(value,_,_) -> Assert.Fail(sprintf "Should not parsed, but was %f" value)
    | Failure(error,_,_) -> Assert.Pass(error)

[<Test>]
let ``eval 1 + 02 = -1`` () =
  let result = Parser.eval "1 + 0002"
  result |> failed

[<Test>]
let ``eval 1 + 0 + 2 = 3`` () =
  let result = Parser.eval "1 + 0 + 2"
  result |> is 3.
  

[<Test>]
let ``proc 2 1001 expect 4`` () =
  let result = Processor.procWithCustom 2. "1001" ["";"+";"-"] |> Seq.toArray
  Assert.AreEqual(4, result.Length)


[<Test>]
let ``proc -2 02 expect 1`` () =
  let result = Processor.proc -1. "0012" |> Seq.toArray
  Assert.AreEqual(6, result.Length)

[<Test>]
let ``proc 2001 in standart expect 2`` () =
  let result = Processor.proc 2001. "123456789" |> Seq.toArray
  let ``no -`` = result
                    |> Array.filter (fun x -> not <| x.StartsWith("-"))

  Assert.AreEqual(5, result.Length)
  Assert.AreEqual(2, ``no -``.Length)

[<Test>]
let ``proc 100 in standart expect 162`` () =
  let result = Processor.proc 100. "123456789"
  Assert.AreEqual(162, result.Count())


