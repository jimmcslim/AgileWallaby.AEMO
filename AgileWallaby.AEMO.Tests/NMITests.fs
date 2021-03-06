module AgileWallaby.AEMO.Tests.NMITests

open System
open AgileWallaby.AEMO
open FSharp.Data.AEMO
//open Samples.

open FsUnit
open Xunit

module Tests =

    type snowman = Samples.StringTypeProvider.StringTyped<value = "abc">
    
    [<Fact>]
    let ``Encapsulates a NMI`` () =
        let nmi = NMI "1234567890"
        nmi.Base |> should equal "1234567890"
        
    [<Fact>]
    let ``Rejects Null, Empty, Or Non Valid Length NMIs``() =
        shouldFail (fun () -> NMI(null) |> ignore)
        shouldFail (fun () -> NMI(String.Empty) |> ignore)
        shouldFail (fun () -> NMI("123456789") |> ignore)
        shouldFail (fun () -> NMI("123456789012") |> ignore)
    
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
        let nmi = NMI(exampleNmi)
        nmi.Checksum |> should equal exampleChecksum
        
    [<Theory>]
    [<InlineData("NMI1234567")>]
    [<InlineData("NMO1234567")>]
    [<InlineData("NM11234567O")>]
    [<InlineData("NM11234567I")>]
    let ``Rejects NMI with I or O as these are ambiguous characters`` nmiWithInvalidCharacter =
        shouldFail (fun() -> NMI(nmiWithInvalidCharacter) |> ignore)

    [<Theory>]
    [<InlineData("12345678900")>]
    [<InlineData("12345678901")>]
    [<InlineData("12345678902")>]
    [<InlineData("12345678903")>]
    [<InlineData("12345678904")>]
    [<InlineData("12345678905")>]
    [<InlineData("12345678906")>]
    // 12345678907 is valid.
    [<InlineData("12345678908")>]
    [<InlineData("12345678909")>]
    let ``Rejects 11 digit NMI with invalid checksum`` nmiWithInvalidChecksum =
        shouldFail (fun() -> NMI(nmiWithInvalidChecksum) |> ignore)

    [<Fact>]
    let ``Can provide NMI inclusive or exclusive of checksum`` () =
        let nmiWithoutChecksum = NMI "2001985732" //Checksum is 8
        nmiWithoutChecksum.Checksum |> should equal 8
        
        nmiWithoutChecksum.Base |> should equal "2001985732"
        nmiWithoutChecksum.Full |> should equal "20019857328"
        
        let nmiWithChecksum = NMI "20019857328"
        nmiWithChecksum.Base |> should equal "2001985732"
        nmiWithChecksum.Full |> should equal "20019857328"

    [<Theory>]
    [<InlineData("2001985732", "2001985732", true)>]
    [<InlineData("2001985732", "20019857328", true)>]
    [<InlineData("20019857328", "2001985732", true)>]
    [<InlineData("QAAAVZZZZZ", "2001985732", false)>]
    [<InlineData("2001985732", "QAAAVZZZZZ", false)>]
    [<InlineData(null, "QAAAVZZZZZ", false)>]
    [<InlineData("QAAAVZZZZZ", null, false)>]
    [<InlineData(null, null, true)>]
    let ``Implements equality`` lhs rhs areEqual =
        let lhsNmi = if isNull lhs then null else NMI(lhs)
        let rhsNmi = if isNull rhs then null else NMI(rhs)
        
        Object.Equals (lhsNmi, rhsNmi) |> should equal areEqual
        
        let lhsHashCode = if isNull lhs then None else Some (lhsNmi.GetHashCode())
        let rhsHashCode = if isNull rhs then None else Some (rhsNmi.GetHashCode())

        (lhsHashCode = rhsHashCode) |> should equal areEqual
