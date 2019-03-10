module AgileWallaby.AEMO.Tests.Main

[<EntryPoint>]
let main args =
    //TODO: Replace with xunit.runner.utility
    let r = 0 //runTestsInAssembly defaultConfig args
    if System.Diagnostics.Debugger.IsAttached then System.Console.ReadLine() |> ignore
    r
