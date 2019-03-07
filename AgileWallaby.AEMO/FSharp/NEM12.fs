module AgileWallaby.AEMO.FSharp.NEM12

open System

type HeaderRecord =
    {
        RecordIndicator: int
        VersionHeader: string
        DateTime: DateTime
        FromParticipant: string
        ToParticipant: string
    }
    
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
        NextScheduledReadDate: DateTime
    }
    
type IntervalDataRecord =
    {
        RecordIndicator: int
        IntervalDate: DateTime
        IntervalValues: decimal list
        QualityMethod: string
        ReasonCode: int
        ReasonDescription: string
        UpdateDateTime: DateTime
        MSATSLoadDateTime: DateTime
    }
    
type IntervalEventRecord =
    {
        RecordIndicator: int
        StartInterval: int
        EndInterval: int
        QualityMethod: string
        ReasonCode: int
        ReasonDescription: string
    }

type EndOfDataRecord =
    {
        RecordIndicator: int
    }

type NEM12Record =
    | Header of HeaderRecord
    | DataDetails of DataDetailsRecord
    | IntervalData of IntervalDataRecord
    | IntervalEvent of IntervalEventRecord
    | EndOfData of EndOfDataRecord