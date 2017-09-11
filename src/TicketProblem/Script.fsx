// Learn more about F# at http://fsharp.org. See the 'F# Tutorial' project
// for more guidance on F# programming.
#I @"../../packages/"
#r @"FParsec\lib\net40-client\FParsecCS.dll"
#r @"FParsec\lib\net40-client\FParsec.dll"
#r @"Rx-Core\lib\net45\System.Reactive.Core.dll"
#r @"Rx-Linq\lib\net45\System.Reactive.Linq.dll"
#r @"Rx-Interfaces\lib\net45\System.Reactive.Interfaces.dll"
#load "Library.fs"
#load "Ticket.fs"

open FParsec
open FParsec.CharParsers
open System
open TicketProblem.Processor



run pfloat "123"
run pfloat "001"

getcomb "123456789"

("123456789" |> Seq.length ) - 1 |> pown operations.Length  |> (*) 2
