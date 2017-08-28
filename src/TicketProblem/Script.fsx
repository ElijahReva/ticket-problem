// Learn more about F# at http://fsharp.org. See the 'F# Tutorial' project
// for more guidance on F# programming.
#I @"../../packages/"
#r @"FParsec\lib\net40-client\FParsecCS.dll"
#r @"FParsec\lib\net40-client\FParsec.dll"
#load "Library.fs"
#load "Ticket.fs"

open FParsec
open FParsec.CharParsers

run pfloat "123"
run pfloat "001"

