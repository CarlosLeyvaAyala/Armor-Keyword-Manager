module Data.WAED.MGEF_Db

open DMLib.Types.Skyrim

let private addEvt = Event<_>()
let OnAdded = addEvt.Publish

let mutable db: MgefDatabase = Map.empty

let toArrayOfRaw () =
    db
    |> Map.toArray
    |> Array.Parallel.map (fun (id, v) -> id.Value, v.asRaw)

let upsertArray mgef =
    db <-
        mgef
        |> Array.map (fun (id, v) -> UniqueId id, MGEF.ofRaw v)
        |> Array.fold (fun (acc: MgefDatabase) (id, v) -> acc |> Map.add id v) db

    addEvt.Trigger()

let upsert uId v =
    db <- Map.add <||| (UniqueId uId, MGEF.ofRaw v, db)
