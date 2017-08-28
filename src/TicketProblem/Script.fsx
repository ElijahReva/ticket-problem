// Learn more about F# at http://fsharp.org. See the 'F# Tutorial' project
// for more guidance on F# programming.
#I @"../../packages/"
#r @"FParsec\lib\net40-client\FParsecCS.dll"
#r @"FParsec\lib\net40-client\FParsec.dll"
#load "Library.fs"
#load "Ticket.fs"

open TicketProblem

let num =  "123+34" |> Parser.eval
printfn "%f" num

let a = TicketProblem.Ticket(100.,"12334")

a.Eval()

