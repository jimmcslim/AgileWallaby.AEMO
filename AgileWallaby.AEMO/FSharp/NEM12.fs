module AgileWallaby.AEMO.FSharp.NEM12

open System
open System.Globalization
open System.IO
open CsvHelper
open CsvHelper.Configuration
open Utilities

type HeaderRecord =
    {
        RecordIndicator: int
        VersionHeader: string
        DateTime: DateTime
        FromParticipant: string
        ToParticipant: string
    }
    
[<Literal>]
let HeaderRecordIndicator = "100"
    
type DataDetailsRecord =
    {
        RecordIndicator: int
        NMI: string
        NMIConfiguration: string
        RegisterID: string
        NMISuffix: string
        MDMDataStreamIdentifier: string
        MeterSerialNumber: string
        UOM: string
        IntervalLength: TimeSpan
        NextScheduledReadDate: DateTime option
    }
    
[<Literal>]
let DataDetailsRecordIndicator = "200"
    
type IntervalDataRecord =
    {
        RecordIndicator: int
        IntervalDate: DateTime
        IntervalValues: decimal list
        QualityMethod: string
        ReasonCode: int option
        ReasonDescription: string
        UpdateDateTime: DateTime
        MSATSLoadDateTime: DateTime option
    }
    
[<Literal>]
let IntervalDataRecordIndicator = "300"
    
type IntervalEventRecord =
    {
        RecordIndicator: int
        StartInterval: int
        EndInterval: int
        QualityMethod: string
        ReasonCode: int
        ReasonDescription: string
    }
    
[<Literal>]
let IntervalEventRecordIndicator = "400"

type EndOfDataRecord =
    {
        RecordIndicator: int
    }

[<Literal>]
let EndOfDataRecordIndicator = "900"

type NEM12Record =
    | Header of HeaderRecord
    | DataDetails of DataDetailsRecord
    | IntervalData of IntervalDataRecord
    | IntervalEvent of IntervalEventRecord
    | EndOfData of EndOfDataRecord

let parseDateTime dateAsStr =
    let formats = [|"yyyyMMddHHmm";"yyyyMMddHHmmss"|]
    match DateTime.TryParseExact(dateAsStr, formats, CultureInfo.InvariantCulture, DateTimeStyles.None) with
    | true, d -> d
    | _ -> failwith "Could not parse"
    
let parseDate dateAsStr =
    DateTime.ParseExact(dateAsStr, "yyyyMMdd", null)    
    
let parseSomeDate dateAsStr =
    match trimmedOrNone dateAsStr with
    | None -> None
    | Some dateAsStr -> Some (parseDate dateAsStr)
    
let parseSomeDateTime dateTimeAsStr =
    match trimmedOrNone dateTimeAsStr with
    | None -> None
    | Some dateTimeAsStr -> Some (parseDateTime dateTimeAsStr)
    
let parseHeaderRecord (row:IReaderRow) =
    {
        RecordIndicator = 100
        VersionHeader = row.[1]
        DateTime = parseDateTime row.[2]
        FromParticipant = row.[3]
        ToParticipant = row.[4]
    }

let parseDataDetailsRecord (row:IReaderRow) =
    {
        RecordIndicator = 200
        NMI = row.[1]
        NMIConfiguration = row.[2]
        RegisterID = row.[3]
        NMISuffix = row.[4]
        MDMDataStreamIdentifier = row.[5]
        MeterSerialNumber = row.[6]
        UOM = row.[7]
        IntervalLength =
            TimeSpan.FromMinutes((float) (Int32.Parse(row.[8])))
        NextScheduledReadDate = parseSomeDate (row.[9])
    }
    
type NEM12Context = {
    IntervalLength: TimeSpan
}
    
let parseIntervalDataRecord (row:IReaderRow, context:NEM12Context) =

    let numberOfIntervalsPerDay = (24 * 60) / (int)context.IntervalLength.TotalMinutes
    
    let intervalDate = parseDate row.[1]

    let expectedNumberOfColumns = numberOfIntervalsPerDay + 7

    if row.Context.Record.Length <> numberOfIntervalsPerDay + 7 then
        failwithf "Expected %d columns but there were %d" expectedNumberOfColumns row.Context.ColumnCount
    
    let afterValuesColumnIndex = numberOfIntervalsPerDay + 2
    
    let getIntervalValues =
        seq {
            for i = 1 to numberOfIntervalsPerDay do
                yield row.GetField<decimal>(i + 1)
        } |> Seq.toList

    {
        RecordIndicator = 300
        IntervalDate = intervalDate
        IntervalValues = getIntervalValues
        QualityMethod = row.[afterValuesColumnIndex + 0]
        ReasonCode =
            match trimmedOrNone row.[afterValuesColumnIndex + 1] with
            | Some str -> Some (Int32.Parse(str))
            | _ -> None
        ReasonDescription = row.[afterValuesColumnIndex + 2]
        UpdateDateTime = parseDateTime row.[afterValuesColumnIndex + 3]
        MSATSLoadDateTime = parseSomeDateTime row.[afterValuesColumnIndex + 4]
    }

let parseIntervalEventRecord (row:IReaderRow) =
    {
        RecordIndicator = 400
        StartInterval = Int32.Parse(row.[1])
        EndInterval = Int32.Parse(row.[2])
        QualityMethod = row.[3]
        ReasonCode = Int32.Parse(row.[4])
        ReasonDescription = row.[5]
    }
    
let parseEndOfDataRecord (row:IReaderRow) =
    {
        RecordIndicator = 500
    }
    
let parseNem12File (tr:TextReader) =
    
    let parseNem12Row (row:IReaderRow) context =
        let recordIndicator = row.GetField(0)
        match recordIndicator with
        | "100" -> (Header (parseHeaderRecord row), context)
        | "200" ->
            let record = parseDataDetailsRecord row
            (DataDetails record, context)
        | "300" -> (IntervalData (parseIntervalDataRecord (row, context)), context)
        | "400" -> (IntervalEvent (parseIntervalEventRecord row), context)
        | "900" -> (EndOfData (parseEndOfDataRecord row), context)
        | _ -> failwithf "Unexpected record indicator: %s" recordIndicator
    
    let configuration = new Configuration(HasHeaderRecord = false, IgnoreBlankLines = true)
        
    let parser = new CsvParser(tr, configuration)
    let reader = new CsvReader(parser)

    seq {
        let mutable context = {
            IntervalLength = TimeSpan.FromMinutes 30.
        }
        
        while reader.Read() do
            let rowAndContext = parseNem12Row reader context
            context <- snd rowAndContext
            yield fst rowAndContext
    }
