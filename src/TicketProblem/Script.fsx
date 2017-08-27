// Learn more about F# at http://fsharp.org. See the 'F# Tutorial' project
// for more guidance on F# programming.
#r @"E:\work\ticket\packages\FParsec\lib\net40-client\FParsec.dll"
#r @"E:\work\ticket\packages\FParsec\lib\net40-client\FParsecCS.dll"
#load "Library.fs"

open TicketProblem
open FParsec

let num =  "123+34" |> Library.calculate |> Library.value
printfn "%i" num
