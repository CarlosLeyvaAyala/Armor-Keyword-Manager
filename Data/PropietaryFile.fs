module IO.PropietaryFile

open DMLib

type PropietaryFile = { itemKeywords: IO.Item.JsonArmorMap }

let Save filename =
    { itemKeywords = Data.Items.toJson () }
    |> Json.writeToFile true filename

let Open filename =
    let d = Json.getFromFile<PropietaryFile> filename
    Data.Items.ofJson d.itemKeywords
    ()
