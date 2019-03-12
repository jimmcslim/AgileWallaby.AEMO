module AgileWallaby.AEMO.FSharp.NMI

type T

val createNMI : (T -> 'a) -> (string -> 'a) -> string -> 'a
val createNMIOrNone : (string -> T option)
val createNMIOrError : (string -> T)

val calculateChecksum: string -> int
val baseValue : T -> string
val fullValue : T -> string
val checksum : T -> int



