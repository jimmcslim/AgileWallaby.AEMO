module AgileWallaby.AEMO.FSharp.NMI

open System

let calculateChecksum(nmi:string) =
    
    if (nmi.Length <> 10 && nmi.Length <> 11) then
        invalidArg "nmi" nmi
   
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

    seq { 10 .. -1 .. 1 }
    |> Seq.map (fun i -> (int)nmi.[i - 1])
    |> Seq.fold folderFunc (0, true)
    |> fun (v, _) -> (10 - v % 10) % 10
    
type T = NMI of string

let isNotNullOrWhitespace (s:string) =
    match String.IsNullOrWhiteSpace s with
    | true -> Error "Cannot be null or whitespace."
    | _ -> Ok s

let hasValidCharacters (s:string) =
    match s.ToUpper().IndexOfAny([|'I';'O'|]) with
    | x when x >= 0 -> Error "NMI cannot contain I or O, ambiguous with 1 and 0."
    | _ -> Ok s
    
let hasValidLength (s:string) =
    match s.Length with
    | 10 -> Ok s
    | 11 -> Ok s
    | _ -> Error "NMI should be 10 or 11 characters in length."
    
let hasValidChecksum (s:string) =
    match hasValidLength s with
    | Ok _ ->
        match s.Length with
        | 11 ->
            let expectedChecksum = calculateChecksum s
            let actualChecksum = Int32.Parse(s.[10].ToString())
            match expectedChecksum with
            | x when x = actualChecksum -> Ok s
            | _ -> Error (sprintf "Checksum is incorrect, expected %d, was %d." expectedChecksum actualChecksum)
        | _ -> Ok s
    | Error x -> Error x    
    
let validateNmi s =
    s
    |> Result.bind isNotNullOrWhitespace
    |> Result.bind hasValidLength
    |> Result.bind hasValidCharacters
    |> Result.bind hasValidChecksum

let createNMI nmi =
    match validateNmi (Ok nmi) with
    | Ok x -> Ok (NMI x)
    | Error s -> Error s

let checksum (NMI s) = calculateChecksum s
let baseValue (NMI s) = s.Substring(0, 10)
let fullValue (NMI s) =
        let baseNmi = s.Substring(0, 10)
        sprintf "%s%d" baseNmi (calculateChecksum baseNmi)
