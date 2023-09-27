module Data.Keywords

open Common
open DMLib
open DMLib.String

type Keyword =
    | Keyword of string
    static member toRaw(Keyword k) = k
    member t.toRaw() = Keyword.toRaw t
    member t.raw = t.toRaw ()
    static member ofString(s: string) = s |> toLower |> Keyword

type Data =
    { image: Image
      description: string
      originalName: string
      color: int }

    static member ofRaw(r: Raw) : Data =
        { image = r.image |> Image.ofString
          description = r.description
          originalName = "Added from map"
          color = r.color }

    static member toRaw(r: Data) : Raw =
        { image = r.image.Value
          description = r.description
          color = r.color }

    member t.toRaw() = Data.toRaw t
    member t.raw = t.toRaw ()

and Raw =
    { image: string
      description: string
      color: int }

type KeywordMap = Map<Keyword, Data>

module Database =
    [<Literal>]
    let DefaultColor = 100000

    let blankKeyword =
        { image = ""
          description = ""
          color = DefaultColor }

    let mutable private keywords: KeywordMap = Map.empty

    let ofRawPair (k, v) =
        Keyword.ofString k, { Data.ofRaw v with originalName = k }

    let toRawPair (_, v: Data) = v.originalName, v.raw

    let toArrayOfRaw () =
        keywords
        |> Map.toArray
        |> Array.Parallel.map toRawPair

    let ofRaw v =
        keywords <- v |> Array.Parallel.map ofRawPair |> Map.ofArray

    let toRaw () = keywords

    let tryFind key =
        keywords |> Map.tryFind (Keyword.ofString key)

    /// Returns a default value if the keyword was not found
    let findDefault key =
        tryFind key
        |> Option.map (fun x -> x.raw)
        |> Option.defaultValue { blankKeyword with description = "*** THIS KEYWORD IS NOT RECOGNIZED ***" } // TODO: Add a proper type so missing keywords can be added by app

    let upsert (key: string) (v: Raw) =
        let (k, v') = (key, v) |> ofRawPair

        keywords <-
            keywords.Add(
                match tryFind key with
                | Some oldVal -> k, { v' with originalName = oldVal.originalName } // Avoid adding an existing keyword with changed casing
                | None -> k, v'
            )

    let edit key f =
        keywords
        |> Map.find (Keyword.ofString key)
        |> Data.toRaw
        |> f
        |> upsert key

    let delete key =
        keywords <- keywords |> Map.remove (Keyword.ofString key)
