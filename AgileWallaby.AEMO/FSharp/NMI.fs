module AgileWallaby.AEMO.FSharp.NMI

open System
open AgileWallaby.AEMO.FSharp
open Utilities

let calculateChecksum(nmi:string) =
   
    let folderFunc (v1, mult) x =
        let d = 
            match mult with
            | true -> x * 2
            | _ -> x
        
        let x (v, d) =
            match d with
            | d when d <= 0 -> None
            | _ ->
                let state = (v + d % 10, d / 10)
                Some (state, state)
            
        Seq.unfold x (v1, d)
        |> Seq.last
        |> fun (v, _) -> (v, not (mult))

    seq { nmi.Length .. -1 .. 1 }
    |> Seq.map (fun i -> (int)nmi.[i - 1])
    |> Seq.fold folderFunc (0, true)
    |> fun (v, _) -> (10 - v % 10) % 10
    
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

