namespace GUI

type AppWorkspacePage =
    | Items
    | Outfits
    | WaedEnchantments
    | WaedBuilds
    | Skimpify

type IWorkspacePage =
    abstract member SetActivePage: unit -> unit

module Workspace =
    module Items = Data.Items.Database
    module Outfits = Data.Outfit.Database

    let private pageTagsChangeEvent = Event<string array>()

    let changePage page =
        match page with
        | Items ->
            Items.toArrayOfRaw ()
            |> Data.Tags.Get.allTags Items.allItemTags
        | Outfits ->
            Outfits.toArrayOfRaw ()
            |> Data.Tags.Get.allTags Outfits.allOutfitTags
        | WaedEnchantments -> failwith "Not implemented"
        | WaedBuilds -> failwith "Not implemented"
        | Skimpify -> failwith "Not implemented"
        |> pageTagsChangeEvent.Trigger

    let onChangePageTags = pageTagsChangeEvent.Publish
