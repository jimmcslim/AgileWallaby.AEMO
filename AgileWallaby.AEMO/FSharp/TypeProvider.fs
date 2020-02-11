namespace FSharp.Data.AEMO

open System
open System.Reflection
open ProviderImplementation.ProvidedTypes
open FSharp.Core.CompilerServices
open FSharp.Quotations 

[<TypeProvider>]
type StringTypeProvider(config: TypeProviderConfig) as this = 
    inherit TypeProviderForNamespaces(config)

    let namespaceName = "Samples.StringTypeProvider"
    let thisAssembly = Assembly.GetExecutingAssembly()

    let staticParams = [ProvidedStaticParameter("value", typeof<string>)]

    let t = ProvidedTypeDefinition(thisAssembly, namespaceName, "StringTyped", Some typeof<obj>, hideObjectMethods = true)

    do t.DefineStaticParameters(
        parameters = staticParams,
        instantiationFunction = (fun typeName paramValues ->
            match paramValues with
            | [| :? string as value |] ->
                let ty = ProvidedTypeDefinition(
                            thisAssembly,
                            namespaceName,
                            typeName,
                            Some typeof<obj>
                        )
                
                let getTheLength (s:string) =
                    s.Length

                let lengthProp = ProvidedProperty(
                                    "Length",
                                    typeof<int>,
                                    getterCode = fun args -> <@@ getTheLength value @@>
                                )
                ty.AddMember lengthProp

                let charProps = value
                                    |> Seq.map(fun c -> 
                                            let p = ProvidedProperty(
                                                        c.ToString(),
                                                        typeof<char>,
                                                        getterCode = fun args -> <@@ c @@>
                                                    )
                                            let doc = sprintf "The char %s" (c.ToString())
                                            p.AddXmlDoc doc

                                            p
                                        )
                                    |> Seq.toList
                ty.AddMembersDelayed (fun () -> charProps)

                let sanitized = value.Replace(" ","")
                let valueProp = ProvidedProperty(
                                    sanitized,
                                    typeof<string>,
                                    getterCode = fun args -> <@@ value @@>
                                )
                valueProp.AddXmlDoc "This is the value that you gave me to start with"
                ty.AddMember valueProp

                let ctor = ProvidedConstructor(
                            parameters = [], 
                            invokeCode = fun args -> <@@ value :> obj @@>
                        )

                ctor.AddXmlDoc "Initializes a the awesomes"

                ty.AddMember ctor
                
                let reverser = ProvidedMethod(
                                    methodName = "Reverse",
                                    parameters = [],
                                    returnType = typeof<string>,
                                    invokeCode = (fun args ->
                                                    <@@ 
                                                    value
                                                        |> Seq.map (fun x -> x.ToString())
                                                        |> Seq.toList
                                                        |> List.rev
                                                        |> List.reduce (fun acc el -> acc + el) 
                                                    @@>))

                ty.AddMember reverser

                ty
            | _ -> failwith "No idea what you're doing"
        )
    )

    do this.AddNamespace(namespaceName, [t])
    
[<assembly:TypeProviderAssembly>]
do()    