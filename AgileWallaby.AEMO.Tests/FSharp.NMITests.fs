module AgileWallaby.AEMO.Tests.FSharp.NMI

open FsUnit
open Xunit

open AgileWallaby.AEMO.FSharp

module Tests =
    
    [<Theory>]
    // Sample NMIs from https://bit.ly/2GKxM9E
    [<InlineData("2001985732", 8)>]
    [<InlineData("QAAAVZZZZZ", 3)>]
    [<InlineData("2001985733", 6)>]
    [<InlineData("QCDWW00010", 2)>]
    [<InlineData("3075621875", 8)>]
    [<InlineData("SMVEW00085", 8)>]
    [<InlineData("3075621876", 6)>]
    [<InlineData("VAAA000065", 7)>]
    [<InlineData("4316854005", 9)>]
    [<InlineData("VAAA000066", 5)>]
    [<InlineData("4316854006", 7)>]
    [<InlineData("VAAA000067", 2)>]
    [<InlineData("6305888444", 6)>]
    [<InlineData("VAAASTY576", 8)>]
    [<InlineData("6350888444", 2)>]
    [<InlineData("VCCCX00009", 1)>]
    [<InlineData("7001888333", 8)>]
    [<InlineData("VEEEX00009", 1)>]
    [<InlineData("7102000001", 7)>]
    [<InlineData("VKTS786150", 2)>]
    [<InlineData("NAAAMYS582", 6)>]
    [<InlineData("VKTS867150", 5)>]
    [<InlineData("NBBBX11110", 0)>]
    [<InlineData("VKTS871650", 7)>]
    [<InlineData("NBBBX11111", 8)>]
    [<InlineData("VKTS876105", 7)>]
    [<InlineData("NCCC519495", 5)>]
    [<InlineData("VKTS876150", 3)>]
    [<InlineData("NGGG000055", 4)>]
    [<InlineData("VKTS876510", 8)>]
    let ``Accepts 11 digit NMI with valid checksum`` exampleNmi exampleChecksum =
        NMI.calculateChecksum exampleNmi |> should equal exampleChecksum

    type NMIOrError =
        | NMI of NMI.T
        | Error of string
        
    [<Fact>]
    let ``Can receive NMI or error via continuation`` () =
        
        let success = (fun x -> NMI x)
        let error = (fun x -> Error x)
        
        match NMI.createNMI success error "1234567890" with
        | NMI nmi -> nmi |> NMI.baseValue |> should equal "1234567890"
        | Error err -> failwith err
        
        match NMI.createNMI success error "12345" with
        | NMI nmi -> failwith "Expected an error"
        | Error err -> err |> should equal "NMI should be 10 or 11 characters in length."
        
    [<Fact>]
    let ``Can receive NMI or None via createNMIOrNone`` () =
        
        match NMI.createNMIOrNone "1234567890" with
        | Some nmi -> nmi |> NMI.baseValue |> should equal "1234567890"
        | None _ -> failwith "Should not have received None"
        
        match NMI.createNMIOrNone "12345" with
        | Some nmi -> failwith "Should not have received a NMI"
        | None _ -> ignore //Expected
        |> ignore
        
    [<Fact>]
    let ``Create receive NMI or raise exception via createNMIOrError`` () =
        
        let nmi = NMI.createNMIOrError "1234567890"
        nmi |> NMI.baseValue |> should equal "1234567890"
        
        shouldFail (fun () -> NMI.createNMIOrError "12345" |> ignore)
        
        // How to test the message?
        //(fun () -> NMI.createNMIOrError "12345" |> ignore) |> should (throwWithMessage "Invalid number of characters") typeof<System.Exception>
