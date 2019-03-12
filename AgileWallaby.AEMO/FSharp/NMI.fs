module AgileWallaby.AEMO.FSharp.NMI

open System
open AgileWallaby.AEMO.FSharp
open Utilities

let calculateChecksum(nmi:string) =
   
    let state = (0, true)
   
    // TODO: Not very functional - rewrite!
    let folder state i =
        let x = (int)nmi.[i - 1]
        let mutable d =
            match snd state with
            | true -> x * 2
            | _ -> x
        let multiply = not (snd state)
        let mutable v = fst state
        while (d > 0) do
            v <- v + d % 10
            d <- d / 10
        (v, multiply)
  
    let result = seq { nmi.Length .. -1 .. 1 } |> Seq.fold folder state
    (10 - fst result % 10) % 10   
    
type T = NMI of string

let createNMI success failure (s:string) =
        match trimmedOrNone s with
        | Some s ->
            let s = s.ToUpper()
            
            if s.ToUpper().IndexOfAny([|'I'; 'O'|]) >= 0 then
                failure "NMI cannot contain I or O, ambiguous with 1 and 0."
            else
                match s.Length with
                | 10 -> success (NMI s)
                | 11 ->
                    let nmiWithoutChecksum = s.Substring(0, 10)
                    let expectedChecksum = calculateChecksum nmiWithoutChecksum
                    let actualChecksum = Int32.Parse(s.[10].ToString())
                    if expectedChecksum <> actualChecksum then
                        failure (sprintf "Checksum is incorrect, expected %d, was %d" expectedChecksum actualChecksum)
                    else success (NMI s)
                | _ -> failure "NMI should be 10 or 11 characters in length."
        | None -> failure "NMI is blank or null."
    
let successOption e = Some e
let failure _ = None

let successValue e = e
let error e = failwith e

let createNMIOrNone = createNMI successOption failure
let createNMIOrError = createNMI successValue error

let checksum (NMI s) = calculateChecksum s
let baseValue (NMI s) = s
let fullValue (NMI s) = s

let nmi = createNMIOrNone "1234567890"

