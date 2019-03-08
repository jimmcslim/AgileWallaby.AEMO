namespace AgileWallaby.AEMO

open System

open AgileWallaby.AEMO.FSharp.NMI
open AgileWallaby.AEMO.FSharp.Utilities

[<AllowNullLiteral>]
type NMI(nmi: string) =
    
    let nmi =
        
        match trimmedOrNone nmi with
        | None -> raise(ArgumentOutOfRangeException("nmi", "NMI is null."))
        | Some nmi ->
            if nmi.ToUpper().IndexOfAny([|'I'; 'O'|]) >= 0 then
                raise(ArgumentOutOfRangeException("nmi", "NMI cannot contain I or O, ambiguous with 1 and 0."))

            nmi.ToUpper()
    
    let checksum =
        match nmi.Length with
        | 10 -> calculateChecksum nmi
        | 11 ->
            let nmiWithoutChecksum = nmi.Substring(0, 10)
            let expectedChecksum = calculateChecksum nmiWithoutChecksum
            let actualChecksum = Int32.Parse(nmi.[10].ToString())
            if expectedChecksum <> actualChecksum then
                raise(ArgumentException("nmi", sprintf "Checksum is incorrect, expected %d, was %d" expectedChecksum actualChecksum))
            expectedChecksum
        | _ -> raise (ArgumentOutOfRangeException("nmi", "NMI should be 10 or 11 characters in length."))
        
    member this.Value = nmi
    
    member this.Checksum = checksum
    
    member this.Base =
        nmi.Substring(0, 10)
    
    member this.Full =
        sprintf "%s%d" this.Base this.Checksum
        
    member private this.Equals(other:NMI) =
        String.Equals(this.Base, other.Base, StringComparison.InvariantCultureIgnoreCase)
        
    override this.Equals (obj:Object) =
        if Object.ReferenceEquals(null, obj) then false
        else if Object.ReferenceEquals(this, obj) then true
        else
            obj.GetType() = this.GetType() && this.Equals(obj :?> NMI)

    override this.GetHashCode() =
        StringComparer.InvariantCultureIgnoreCase.GetHashCode(this.Base)