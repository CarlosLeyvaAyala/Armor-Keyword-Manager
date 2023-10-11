namespace GUI

type AppWorkspacePage =
    | Unknown
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
    let mutable private currentPage = Unknown

    let private pageTagsChangeEvent = Event<string array>()
    let private pageChangeEvent = Event<AppWorkspacePage>()

    let private sendPageTags page =
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
        | Unknown -> [||]
        |> pageTagsChangeEvent.Trigger

    let refreshPageTags () = sendPageTags currentPage

    let changePage page =
        currentPage <- page
        pageChangeEvent.Trigger page
        refreshPageTags ()

    let onChangePageTags = pageTagsChangeEvent.Publish
    let onPageChanged = pageChangeEvent.Publish
