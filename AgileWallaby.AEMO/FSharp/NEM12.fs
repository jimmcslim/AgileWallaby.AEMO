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
    
type QualityFlag =
    | Actual
    | Estimated
    | Final
    | Null
    | Substituted
    | Variable

let parseQualityFlag str =
    match trimmedOrNone str with
    | Some "A" -> Actual
    | Some "E" -> Estimated
    | Some "F" -> Final
    | Some "N" -> Null
    | Some "S" -> Substituted
    | Some "V" -> Variable
    | Some str -> failwithf "'%s' is not a valid Quality Flag" str
    | None -> failwithf "No Quality Flag specified"
    
type QualityMethod =
    {
        QualityFlag: QualityFlag
        QualityMethod: string option
    }

let parseQualityMethod str =
    match trimmedOrNone str with
    | Some str when str.Length = 1 -> {
            QualityFlag = parseQualityFlag (str.Substring(0, 1))
            QualityMethod = None
        }
    | Some str when str.Length = 3 -> {
            QualityFlag = parseQualityFlag (str.Substring(0, 1))
            QualityMethod = Some (str.Substring(1, 2))
        }    
    | Some str -> failwithf "'%s' is an invalid length quality method" str
    | None -> failwith "No Quality Method specified"

[<Literal>]
let DataDetailsRecordIndicator = "200"
    
type IntervalDataRecord =
    {
        RecordIndicator: int
        IntervalDate: DateTime
        IntervalValues: decimal list
        QualityMethod: QualityMethod
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
        QualityMethod: QualityMethod
        ReasonCode: int option
        ReasonDescription: string
    }
    
[<Literal>]
let IntervalEventRecordIndicator = "400"


type B2BDetailsRecord =
    {
        RecordIndicator: int
    }

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
    | B2BDetails of B2BDetailsRecord
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
    
let parseSomeInt intAsStr =
    match trimmedOrNone intAsStr with
    | None -> None
    | Some intAsStr -> Some (Int32.Parse(intAsStr))
    
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
        failwithf "Expected %d columns but there were %d" expectedNumberOfColumns row.Context.Record.Length
    
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
        QualityMethod = parseQualityMethod row.[afterValuesColumnIndex + 0]
        ReasonCode = parseSomeInt row.[afterValuesColumnIndex + 1]
        ReasonDescription = row.[afterValuesColumnIndex + 2]
        UpdateDateTime = parseDateTime row.[afterValuesColumnIndex + 3]
        MSATSLoadDateTime = parseSomeDateTime row.[afterValuesColumnIndex + 4]
    }

let parseIntervalEventRecord (row:IReaderRow) =
    {
        RecordIndicator = 400
        StartInterval = Int32.Parse(row.[1])
        EndInterval = Int32.Parse(row.[2])
        QualityMethod = parseQualityMethod row.[3]
        ReasonCode = parseSomeInt row.[4]
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
            let context =
                { context with
                    NEM12Context.IntervalLength = record.IntervalLength
                }
            (DataDetails record, context)
        | "300" -> (IntervalData (parseIntervalDataRecord (row, context)), context)
        | "400" -> (IntervalEvent (parseIntervalEventRecord row), context)
        | "500" -> (B2BDetails ({ RecordIndicator = 500 }), context)
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

