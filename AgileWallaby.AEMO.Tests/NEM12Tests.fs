module AgileWallaby.AEMO.Tests.NEM12Tests

open System
open System.IO

open AgileWallaby.AEMO.FSharp.NEM12

open FsUnit
open Xunit

module Tests =
    
    //[<Fact>]
    let Adhoc() =
        
        let path = Path.Combine(Directory.GetCurrentDirectory(), "test-data", "NEM12#000000000000001#CNRGYMDP#NEMMCO.csv")
        let reader = File.OpenText path
        
        let result = parseNem12File reader
        let list = result |> Seq.toList
        true
    