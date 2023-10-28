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
    module internal Page =
        module Items = Data.Items.Database
        module Outfits = Data.Outfit.Database
        let mutable internal currentPage = Unknown

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

        /// Resets the visibility on the tags that can be shown on a page
        let refreshTags () = sendPageTags currentPage

        let change page =
            currentPage <- page
            pageChangeEvent.Trigger page
            refreshTags ()

        let onChangeTags = pageTagsChangeEvent.Publish
        /// Called when the page was changed
        let onChanged = pageChangeEvent.Publish

    module Filter =
        let private filterPaneOpenEvt = Event<_>()
        let internal OnFilterPaneOpened = filterPaneOpenEvt.Publish

        let DrawerWasOpened () =
            filterPaneOpenEvt.Trigger Page.currentPage

    module internal CopyUIds =
        let formatedUId = id

        let nameAndUId namesLen name uid =
            sprintf "%-*s     %s" namesLen name (formatedUId uid)
