module Data.WAED.ENCH_Db

open DMLib.Types.Skyrim
open DMLib

let private addEvt = Event<_>()
let private importxEditEvt = Event<_>()

let OnAdded = addEvt.Publish
let OnxEditImported = importxEditEvt.Publish

let mutable db: ObjectEffectDb = Map.empty

let tryFind uId =
    db
    |> Map.tryFind (UniqueId uId)
    |> Option.map ObjectEffect.toRaw

/// Given a batch of new effects, preserves the values of the existing ones
let private preserveOldEffects oldFx newFx =
    let oldValues =
        oldFx
        |> Array.map (Tuple.dupMapFst (fun (v: EffectRaw) -> v.mgef))
        |> Map.ofArray

    newFx
    |> Array.map (fun (v: EffectRaw) ->
        match oldValues |> Map.tryFind v.mgef with
        | None -> v
        | Some old -> old)

let upsert uId (v: ObjectEffectRaw) =
    let id = UniqueId uId

    let value =
        match tryFind uId with
        | Some old -> { old with effects = v.effects |> preserveOldEffects old.effects }
        | None -> v
        |> ObjectEffect.ofRaw

    db <- db |> Map.add id value
    addEvt.Trigger()

let ofxEdit a =
    let importLine s =
        let r = xEdit.parse s
        MGEF_Db.upsert r.magicEffects
        upsert r.objFxId r.objFx

    a |> Array.iter importLine
    importxEditEvt.Trigger()

let toArrayOfRaw () =
    db
    |> Map.toArray
    |> Array.Parallel.map (fun (id, v) -> id.Value, v.asRaw)
