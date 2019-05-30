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


    [<Theory>]
    [<InlineData(null, "Cannot be null or whitespace.")>]
    [<InlineData("", "Cannot be null or whitespace.")>]
    [<InlineData("111111111", "NMI should be 10 or 11 characters in length.")>]
    [<InlineData("111111111111", "NMI should be 10 or 11 characters in length.")>]
    [<InlineData("I111111111", "NMI cannot contain I or O, ambiguous with 1 and 0.")>]
    [<InlineData("O111111111", "NMI cannot contain I or O, ambiguous with 1 and 0.")>]
    [<InlineData("VKTS8765101", "Checksum is incorrect, expected 8, was 1.")>]
    let ``Guards against invalid input`` exampleNmi expectedError =
        match NMI.createNMI exampleNmi with
        | Error x -> x |> should equal expectedError
        | Ok _ -> failwith "Should not have validated"
        
