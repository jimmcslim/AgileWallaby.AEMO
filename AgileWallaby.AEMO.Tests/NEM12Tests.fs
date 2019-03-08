module AgileWallaby.AEMO.Tests.NEM12Tests

open System.IO

open AgileWallaby.AEMO.FSharp.NEM12

open FsUnit
open Xunit
open Xunit.Abstractions

type Tests(output: ITestOutputHelper) =
    
    [<Fact>]
    member t.Adhoc() =
        
        let path = Path.Combine(Directory.GetCurrentDirectory(), "test-data", "NEM12#000000000000001#CNRGYMDP#NEMMCO.csv")
        let reader = File.OpenText path
        
        let result = parseNem12File reader
        let list = result |> Seq.toList
        true
    
      
    [<Theory>]
    [<MemberData("files")>]
    member t.All (filename:string) =
        let reader = File.OpenText filename
        let result = parseNem12File reader
        let list = result |> Seq.toList
        true
    
    static member files() =
        let testDataDirectory = Path.Combine(Directory.GetCurrentDirectory(), "test-data")
        let x = Directory.GetFiles(testDataDirectory)
                |> Seq.map (fun file -> [|file|])
                |> Array.ofSeq
        x
