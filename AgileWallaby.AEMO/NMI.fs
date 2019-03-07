namespace AgileWallaby.AEMO

open System

[<AllowNullLiteral>]
type NMI(nmi: string) =
    
    //TODO: Not very functional! Rewrite!
    let calculateChecksum (nmi:string) =
        let mutable v = 0
        let mutable multiply = true
        for i = (nmi.Length) downto 1 do
            let mutable d = (int)nmi.[i - 1]
            if multiply then d <- d * 2
            multiply <- not multiply
            while (d > 0) do
                v <- v + d % 10
                d <- d / 10
            
        (10 - v % 10) % 10
    
    let nmi =
        if String.IsNullOrWhiteSpace nmi then
            raise(ArgumentOutOfRangeException("nmi", "NMI is null."))
        
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