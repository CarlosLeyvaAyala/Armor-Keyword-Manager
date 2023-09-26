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
module internal File =
    let toJson () =
        DB.toArrayOfRaw ()
        |> IO.Common.toJson JsonData.ofRaw

    let ofJson (d: JsonMap) =
        IO.Common.ofJson JsonData.toRaw DB.upsert d
