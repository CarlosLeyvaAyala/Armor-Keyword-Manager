namespace IO.Keywords

module DB = Data.Keywords.Database

type JsonData =
    { image: string
      description: string
      color: int }

    static member toRaw(r: JsonData) : Data.Keywords.Raw =
        { color = r.color
          description = r.description
          image = r.image }

    static member ofRaw(r: Data.Keywords.Raw) =
        { color = r.color
          description = r.description
          image = r.image }

type JsonMap = Map<string, JsonData>

[<RequireQualifiedAccess>]
module File =
    open DMLib
    open Data.Keywords

    let private toJson () =
        DB.toArrayOfRaw ()
        |> IO.Common.toJson JsonData.ofRaw

    let private ofJson (d: JsonMap) =
        IO.Common.ofJson JsonData.toRaw DB.upsert d

    let private fileOpenedEvt = Event<string>()
    let OnFileOpened = fileOpenedEvt.Publish

    /// Opens keyword database from json file
    let Open filename =
        filename
        |> Json.getFromFile<JsonMap>
        |> Map.toArray
        |> Array.Parallel.map (fun (k, v) -> k, JsonData.toRaw v)
        |> DB.ofRaw

        fileOpenedEvt.Trigger filename

    /// Saves keyword database to json file
    let Save filename =
        toJson () |> Json.writeToFile true filename
