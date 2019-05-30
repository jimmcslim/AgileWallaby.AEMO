namespace AgileWallaby.AEMO

open System

open AgileWallaby.AEMO.FSharp.NMI

[<AllowNullLiteral>]
type NMI(nmiStr: string) =
    
    let (nmi, checksum) =
        let nmi =
            match (FSharp.NMI.createNMI nmiStr) with
            | Error x -> failwith x
            | Ok x -> x

        (nmi,
         FSharp.NMI.calculateChecksum nmiStr)
        
    member this.Checksum = checksum
    
    member this.Base =
        nmi |> baseValue
        
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