module AgileWallaby.AEMO.FSharp.NMI

type T

val createNMI : string -> Result<T, string>

val calculateChecksum: string -> int
val baseValue : T -> string
val fullValue : T -> string
val checksum : T -> int



