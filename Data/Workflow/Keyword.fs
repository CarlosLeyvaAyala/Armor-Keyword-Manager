namespace Data.Keywords

open CommonTypes
open DMLib
open DMLib.String
open DMLib.Types.Skyrim

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
      source: EspFileName
      color: int }

    static member ofRaw(r: Raw) : Data =
        { image = r.image |> Image.ofString
          description = r.description
          originalName = "Added from map"
          source = EspFileName r.source
          color = r.color }

    static member toRaw(r: Data) : Raw =
        { image = r.image.Value
          description = r.description
          source = r.source.Value
          color = r.color }

    member t.toRaw() = Data.toRaw t
    member t.raw = t.toRaw ()

and Raw =
    { image: string
      description: string
      source: string
      color: int }

type KeywordMap = Map<Keyword, Data>

/// Event arguments when database changes
type KeywordsChanged =
    | Added of string array
    | Deleted of string array
    | Edited of string array

module Database =
    let private keywordsChangedEvent = new Event<KeywordsChanged>()

    /// Event triggered when keywords have changed.
    let OnKeywordsChanged = keywordsChangedEvent.Publish

    [<Literal>]
    let UnboundEsp = "__DMUnboundItemManagerKeyword__.esm"

    [<Literal>]
    let DefaultColor = 100000

    let blankKeyword =
        { image = ""
          description = ""
          source = "Skyrim.esm"
          color = DefaultColor }

    let mutable private keywords: KeywordMap = Map.empty

    let private ofRawPair (k, v) =
        Keyword.ofString k, { Data.ofRaw v with originalName = k }

    let private toRawPair (_, v: Data) = v.originalName, v.raw

    let toArrayOfRaw () =
        keywords
        |> Map.toArray
        |> Array.Parallel.map toRawPair

    let ofRaw v =
        keywords <- v |> Array.Parallel.map ofRawPair |> Map.ofArray

        v
        |> Array.Parallel.map fst
        |> Added
        |> keywordsChangedEvent.Trigger

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
                | Some oldVal ->
                    Edited [| key |] |> keywordsChangedEvent.Trigger
                    k, { v' with originalName = oldVal.originalName } // Avoid adding an existing keyword with changed casing
                | None ->
                    Added [| key |] |> keywordsChangedEvent.Trigger
                    k, v'
            )

    let edit key f =
        keywords
        |> Map.find (Keyword.ofString key)
        |> Data.toRaw
        |> f
        |> upsert key

        Edited [| key |] |> keywordsChangedEvent.Trigger

    let delete keys =
        keys
        |> Array.iter (fun key -> keywords <- keywords |> Map.remove (Keyword.ofString key))

        Deleted keys |> keywordsChangedEvent.Trigger
