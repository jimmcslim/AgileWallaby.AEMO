module AgileWallaby.AEMO.FSharp.Utilities

open System

let trimmedOrNone str =
    match String.IsNullOrWhiteSpace str with
    | true -> None
    | false -> Some (str.Trim())