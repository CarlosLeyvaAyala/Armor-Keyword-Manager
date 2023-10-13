namespace GUI

open DMLib
open DMLib.String

module Items = Data.Items.Database
module Outfits = Data.Outfit.Database

[<Sealed>]
type AppCtxData() =
    member _.EspWithNoOutfits =

        let getEsp a =
            a
            |> Array.Parallel.map (fst >> Types.Skyrim.UniqueId.Split >> fst)
            |> Array.distinct

        Items.toArrayOfRaw ()
        |> getEsp
        |> Array.except (Outfits.toArrayOfRaw () |> getEsp)
        |> Array.Parallel.sortWith compareICase
        |> Array.fold smartNl ""

    member _.CanGetEspWithNoOutfits = Data.Items.Database.isEmpty ()
