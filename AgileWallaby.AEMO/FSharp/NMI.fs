module AgileWallaby.AEMO.FSharp.NMI


//TODO: Not very functional - rewrite!
let calculateChecksum(nmi:string) =
    
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