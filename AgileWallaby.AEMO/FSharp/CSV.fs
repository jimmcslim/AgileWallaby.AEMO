module AgileWallaby.AEMO.FSharp.CSV

open CsvHelper
open CsvHelper.Configuration

type ReportKey = {
    reportType: string
    reportSubtype: string
    reportVersion: int
}

[<CLIMutable>]
type Header = {
    recordIdentifier: char
    system: string
    reportId: string
    from: string
    toValue: string
    publishDate: string
    publishTime: string
}

[<CLIMutable>]
type Footer = {
    recordIdentifier: char
    text: string
    recordCount: int
}

[<CLIMutable>]
type ReportHeader = {
    recordIdentifier: char
    reportType: string
    reportSubtype: string
    reportVersion: int
    fields: string list
} with
    member t.reportKey = {
        reportType = t.reportType
        reportSubtype = t.reportSubtype
        reportVersion = t.reportVersion
    }

[<CLIMutable>]
type DataRecord = {
    recordIdentifier: char
    reportType: string
    reportSubtype: string
    reportVersion: int
    fields: string list
} with
    member t.reportKey = {
        reportType = t.reportType
        reportSubtype = t.reportSubtype
        reportVersion = t.reportVersion
    }
    
type CsvRecord = 
    | Comment of Header
    | Information of ReportHeader
    | Data of DataRecord
    | EndOfReport of Footer
    
type HeaderRecordMap() =
    inherit ClassMap<Header>()
    do
        base.Map(fun x -> x.recordIdentifier).Index(0) |> ignore
        base.Map(fun x -> x.system).Index(1) |> ignore
        base.Map(fun x -> x.reportId).Index(2) |> ignore
        base.Map(fun x -> x.from).Index(3) |> ignore
        base.Map(fun x -> x.toValue).Index(4) |> ignore
        base.Map(fun x -> x.publishDate).Index(5) |> ignore
        base.Map(fun x -> x.publishTime).Index(6) |> ignore

type FooterRecordMap() =
    inherit ClassMap<Footer>()
    do
        base.Map(fun x -> x.recordIdentifier).Index(0) |> ignore
        base.Map(fun x -> x.text).Index(1) |> ignore
        base.Map(fun x -> x.recordCount).Index(2) |> ignore

let getFields (row:IReaderRow) =
    seq { 4 .. row.Context.Record.Length - 1 }
    |> Seq.map (fun i -> row.GetField(i))
    |> Seq.toList
    
type InformationRecordMap() =
    inherit ClassMap<ReportHeader>()
    do
        base.Map(fun x -> x.recordIdentifier).Index(0) |> ignore
        base.Map(fun x -> x.reportType).Index(1) |> ignore
        base.Map(fun x -> x.reportSubtype).Index(2) |> ignore
        base.Map(fun x -> x.reportVersion).Index(3) |> ignore
        base.Map(fun x -> x.fields).ConvertUsing(getFields) |> ignore

type DataRecordMap() =
    inherit ClassMap<DataRecord>()
    do
        base.Map(fun x -> x.recordIdentifier).Index(0) |> ignore
        base.Map(fun x -> x.reportType).Index(1) |> ignore
        base.Map(fun x -> x.reportSubtype).Index(2) |> ignore
        base.Map(fun x -> x.reportVersion).Index(3) |> ignore
        base.Map(fun x -> x.fields).ConvertUsing(getFields) |> ignore
        
type ProcessingState =
    | NoCurrentReport
    | Report of ReportHeader
    | Completed
    
type ReportData = Map<string, string>

type Report = {
    Header: ReportHeader
    Data: ReportData list
}

type FileData = {
    ProcessingState: ProcessingState
    Reports: Map<ReportKey, Report>
}

let initializeData =
    {
        ProcessingState = NoCurrentReport
        Reports = Map.empty
    }
    
let createConfiguration() =
    let config = Configuration()
    config.IgnoreBlankLines <- true
    config.HasHeaderRecord <- false
    config.RegisterClassMap<HeaderRecordMap>() |> ignore
    config.RegisterClassMap<InformationRecordMap>() |> ignore
    config.RegisterClassMap<DataRecordMap>() |> ignore
    config.RegisterClassMap<FooterRecordMap>() |> ignore
    config

let getRecord fieldValue (csv:CsvReader) =
    match fieldValue with
    | 'C' when csv.Context.Row = 1 -> Comment (csv.GetRecord<Header>())
    | 'C' when csv.Context.Row > 1 -> EndOfReport (csv.GetRecord<Footer>()) 
    | 'I' -> Information (csv.GetRecord<ReportHeader>())
    | 'D' -> Data (csv.GetRecord<DataRecord>())
    | recordIdentifier -> failwithf "Unknown record identifier: %c" recordIdentifier

let updateState record state =
    match record, state with
    | Comment _, { ProcessingState = NoCurrentReport } ->
        state
    | Comment _, _ ->
        failwithf "Unexpected header"
    | Information i, _ when state.Reports.ContainsKey(i.reportKey) ->
        failwith "Re-adding the report key"
    | Information i, _ ->
        { state with
            ProcessingState = Report i
            Reports = state.Reports.Add(i.reportKey, { Header = i; Data = list.Empty }) }
    | Data _, { ProcessingState = NoCurrentReport} ->
        failwith "Processing data without a report"
    | Data d, { ProcessingState = Report header } when header.reportKey <> d.reportKey ->
        failwith "Unexpected report type"
    | Data { fields = v }, { ProcessingState = Report header } when header.fields.Length <> v.Length ->
        failwith "Expecting same length of fields"
    | Data d, { ProcessingState = Report header } ->
        let kv =
            List.zip header.fields d.fields
            |> Map.ofList
        let rk = d.reportKey
        let report = state.Reports.[rk]
        let reports = state.Reports.Add(rk, { report with Data = report.Data @ [kv] })
        { state with Reports = reports }
    | EndOfReport _, _  ->
        { state with ProcessingState = Completed }
    | _, { ProcessingState = Completed } ->
        failwith "Should not be processing more records!"
        
let parseCSVFile reader =
    let csv = new CsvReader(reader, createConfiguration())
        
    let processRecord state _ =
        let fieldValue = csv.GetField<char> 0
        let record = getRecord fieldValue csv
        updateState record state
        
    let readLines _ = csv.Read()
    let linesAvailable = (<>) false

    Seq.initInfinite readLines
    |> Seq.takeWhile linesAvailable
    |> Seq.fold processRecord initializeData