#nowarn "57"
namespace GUI.PageContexts

open System.Windows.Controls
open DMLib
open DMLib.String
open DMLib.Collections
open DMLib_WPF.Controls
open System.Windows
open System.IO
open GUI.UserControls
open GUI.Filtering
open System
open DMLib_WPF.Contexts
open DMLib_WPF.Dialogs
open GUI
open GUI.PageContexts.Outfit
open Data.Outfit
open DMLib.Objects
open DMLib.Combinators
open Data.SPID

module DB = Data.Outfit.Database
module Paths = IO.AppSettings.Paths.Img.Outfit

/// Context for working with the outfits page
[<Sealed>]
type OutfitPageCtx() as t =
    inherit PageNavigationContext()

    let mutable filter = FilterTagEventArgs.Empty
    let mutable nameFilter = ""
    let mutable useRegexForNameFilter = false
    let mutable nav: NavListItem array = [||]

    do
        DB.OnRuleAdded
        |> Event.add (fun _ ->
            nameof t.RulesList |> t.OnPropertyChanged

            whenIsNotNull (fun lst -> DataGrid.selectLast lst) t.RulesNav)

        DB.OnRuleDeleted
        |> Event.add (fun _ ->
            nameof t.RulesList |> t.OnPropertyChanged

            t.RulesNav
            |> whenIsNotNull (fun lst -> lst.SelectedIndex <- -1))

        DB.OnRuleUpdated
        |> Event.add (fun (_, idx) ->
            t.ReloadNavAndGoToCurrent()
            t.RulesNav.SelectedIndex <- idx)

    member t.Filter
        with get () = filter
        and set v =
            filter <- v
            t.OnPropertyChanged()

    member t.NameFilter
        with get () = nameFilter
        and set v =
            nameFilter <- v
            t.OnPropertyChanged()

    member t.UseRegexForNameFilter
        with get () = useRegexForNameFilter
        and set v =
            useRegexForNameFilter <- v

            if Not isNullOrEmpty nameFilter then
                t.OnPropertyChanged()

    member private _.appyFilter(a: NavListItem array) =
        // TODO: Filter by distribution
        a
        |> Filter.words nameFilter useRegexForNameFilter
        |> Filter.tags filter.TagMode filter.Tags
        |> Filter.pics filter.PicMode

    member val RulesContext = SpidRuleCxt()
    member val RegexBtn = RegexFilterButton(fun v -> t.UseRegexForNameFilter <- v)

    ///////////////////////////////////////////////
    // PageNavigationContext implementation

    override _.RebuildNav() =
        nav <-
            DB.toArrayOfRaw ()
            |> Array.Parallel.map NavListItem
            |> Array.Parallel.sortBy (fun v -> v.Name.ToLower())

    override t.Activate() =
        GUI.Workspace.changePage GUI.AppWorkspacePage.Outfits

    member t.Nav = nav |> t.appyFilter |> toObservableCollection

    member t.UId =
        match t.NavControl.SelectedItem with
        | :? NavListItem as item -> item.UId
        | _ -> ""

    member t.ReloadNavAndGoTo uid =
        t.LoadNav()
        ListBox.selectByUId t.NavControl uid

    override t.ReloadNavAndGoToCurrent() = t.ReloadNavAndGoTo t.UId

    member t.NavSelectedItem = t.NavControl.SelectedItem :?> NavListItem

    member t.NavSelectedItems =
        [| for i in t.NavControl.SelectedItems -> i |]
        |> Seq.cast<NavListItem>

    member t.IsNavSingleSelected = t.NavControl.SelectedItems.Count = 1
    member t.IsNavMultipleSelected = t.NavControl.SelectedItems.Count > 1

    override t.SelectCurrentItem() =
        base.SelectCurrentItem()
        t.RulesContext.UniqueId <- t.UId
        t.RulesContext.RuleIndex <- -1

        [ nameof t.IsNavSingleSelected
          nameof t.IsNavMultipleSelected
          nameof t.UId
          nameof t.RulesList
          nameof t.CanSpidRulesBeActive ]
        |> t.OnPropertyChanged

    /// Current selected outfit context
    member t.SelectedItem = NavSelectedItem(t.UId)

    ///////////////////////////////////////////////
    // Custom implementation

    member t.SetImage filename =
        let setImage uId filename =
            let ext = Paths.copyImg uId filename
            Paths.Thumb.save uId filename
            DB.update uId (fun d -> { d with img = ext })
            Paths.expandImg uId ext

        if Path.Exists filename && t.UId <> "" then
            let uid = t.UId // Needs to send back uid because it gets lost on reloading
            t.NavSelectedItem.Img <- setImage t.UId filename
            t.ReloadNavAndGoToCurrent()
            uid
        else
            ""

    member private _.rename uId newName =
        DB.update uId (fun d -> { d with name = newName })

    /// Renames a single element
    member t.Rename newName =
        let uid = t.UId
        t.rename uid newName
        t.OnPropertyChanged()
        ListBox.selectByUId t.NavControl uid

    /// Renames many elements
    member t.BatchRename(askNames: Func<seq<BatchRename.Item>, seq<BatchRename.Item>>) =
        let r =
            t.NavSelectedItems
            |> Seq.map (fun v -> BatchRename.Item(v.UId, v.Name))
            |> askNames.Invoke

        match r with
        | null -> ()
        | x -> x |> Seq.iter (fun v -> t.rename v.UId v.Name)

        t.ReloadNavAndGoToCurrent()

    /// Deletes all selected outfits
    member t.DeleteSelected owner =
        let delete uid =
            match (DB.find uid).img with
            | IsWhiteSpaceStr -> ()
            | img -> Paths.expandImg uid img |> File.Delete

            DB.delete uid

        MessageBox.Warning(
            owner,
            "Deleting oufits will also delete images associated with them and that can not be undone; your image will be lost.\n\nDo you wish to continue?",
            "Undoable operation",
            (Action (fun () ->
                t.NavSelectedItems
                |> Seq.iter (fun v -> delete v.UId)

                t.ReloadNavAndGoToFirst()))
        )

    ///////////////////////////////////////////////
    // Rules

    member val RulesNav: DataGrid = null with get, set

    member t.CanSpidRulesBeActive =
        t.SelectedItem.IsDistributable
        && t.HasItemsSelected

    member t.RulesList =
        match t.UId with
        | IsEmptyStr -> []
        | uId ->
            DB.getRuleDisplays uId
            |> List.map PageContexts.Outfit.SpidRuleDisplay
        |> toCList

    member t.RuleIndex
        with get () = t.RulesContext.RuleIndex
        and set v =
            t.RulesContext.RuleIndex <- v
            nameof t.RuleIndex |> t.OnPropertyChanged

    member t.NewRule() = DB.addRule t.UId
    member t.DeleteRule() = DB.deleteRule t.UId t.RuleIndex
    member t.CanCopyRule() = t.RulesNav.SelectedIndex >= 0

    member t.CopyRule() =
        DB.getRuleAsStr t.UId t.RuleIndex
        |> TextCopy.ClipboardService.SetText

    member t.CanPasteRule() =
        try
            let clip = TextCopy.ClipboardService.GetText()
            clip |> SpidRule.ofStr |> ignore

            let cantContain chr = clip |> Not(contains chr)
            cantContain "\n" && cantContain "\""
        with
        | _ -> false

    member t.PasteRule() =
        DB.addRule t.UId
        let idx = (DB.find t.UId).spidRules.Length - 1

        TextCopy.ClipboardService.GetText()
        |> SpidRule.ofStr
        |> SpidRule.toRaw
        |> DB.updateRule t.UId idx
