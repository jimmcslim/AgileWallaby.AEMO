module AgileWallaby.AEMO.Tests.NEM12Tests

open System.IO

open AgileWallaby.AEMO.FSharp.NEM12

open System.IO
open FsUnit
open Xunit
open Xunit.Abstractions

type Tests(output: ITestOutputHelper) =
    
    [<Fact>]
    member t.Adhoc() =
        
        let path = Path.Combine(Directory.GetCurrentDirectory(), "valid-test-data", "NEM12#000000000000001#CNRGYMDP#NEMMCO.csv")
        let reader = File.OpenText path
        
        let result = parseNem12File reader
        let list = result |> Seq.toList
        true
        
    [<Theory>]
    [<MemberData("files", "valid-test-data")>]
    member t.All (filename:string) =
        let fullFilename = Path.Combine(Directory.GetCurrentDirectory(), "valid-test-data", filename)
        let reader = File.OpenText fullFilename
        let result = parseNem12File reader
        let list = result |> Seq.toList
        true
    
    static member files(directory:string) =
        let testDataDirectory = Path.Combine(Directory.GetCurrentDirectory(), directory)
        let x = Directory.GetFiles(testDataDirectory)
                |> Seq.map (fun file -> [|Path.GetFileName(file)|])
                |> Array.ofSeq
        x
