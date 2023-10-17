[<RequireQualifiedAccess>]
module IO.WAED.File

open System.IO
open IO.WAED
open DMLib
open DMLib.Combinators
open DMLib.String
open IO.AppSettings

module MGEF = Data.WAED.MGEF_Db
module ENCH = Data.WAED.ENCH_Db

let private openEnchEvt = Event<_>()
let OnEnchOpened = openEnchEvt.Publish

let private saveRawArrayToJson toArray toJson getFilename =
    toArray ()
    |> Array.Parallel.map (Tuple.mapSnd toJson)
    |> Map.ofArray
    |> Json.writeToFile true (getFilename ())

let saveMgef () =
    saveRawArrayToJson MGEF.toArrayOfRaw MGEFJson.ofRaw Paths.MagicEffectsFile

let saveEnch () =
    saveRawArrayToJson ENCH.toArrayOfRaw ObjectEffectJson.ofRaw Paths.ObjectEffectsFile

let openMgef filename =
    IO.Common.openGlobalJson MGEFJson.toRaw MGEF.upsert filename

let openEnch filename =
    IO.Common.openGlobalJson ObjectEffectJson.toRaw ENCH.upsert filename
    openEnchEvt.Trigger()

let importxEdit filename =
    File.ReadAllLines filename
    |> Array.filter (Not isNullOrWhiteSpace)
    |> ENCH.ofxEdit
