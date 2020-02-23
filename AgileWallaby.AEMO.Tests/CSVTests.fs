module AgileWallaby.AEMO.Tests.CSVTests

open System.IO
open FsUnit
open Xunit
open Xunit.Abstractions
open AgileWallaby.AEMO.FSharp.CSV    
    
type Tests(output: ITestOutputHelper) =

    [<Fact>]
    member t.``Parse Files``() =
        let fullFilename = Path.Combine(Directory.GetCurrentDirectory(), "aemo-csv", "PUBLIC_DISPATCHIS_202002180915_0000000319228251.csv")
        let reader = File.OpenText fullFilename
        
        let data = parseCSVFile reader
        
        for key in data.Reports do
            let v = key.Value
            output.WriteLine(sprintf "Report: %O Count: %d" key.Key v.Data.Length)
        
        true