namespace GUI.PageContexts

open DMLib
open DMLib.Collections
open Data.UI.Outfit

/// Context for working with the outfits page
type OutfitPageCtx() =
    inherit WPFBindable()
    let mutable currentSelection = ""
    let mutable selectionCount = 0

    member _.UId = currentSelection
    member _.Nav = Nav.createFull () |> toObservableCollection

    member t.LoadNav() = t.OnPropertyChanged("Nav")

    member t.SelectOutfit uId =
        currentSelection <- if t.Nav.Count = 0 then "" else uId
        t.OnPropertyChanged("Selected")
        t.OnPropertyChanged("UId")

    member _.Selected = NavItem(currentSelection)

    member t.Rename newName =
        Edit.Rename currentSelection newName
        t.Selected.Name <- newName
        t.OnPropertyChanged("")

    member _.IsSingleSelected = selectionCount = 1
    member _.IsMultipleSelected = selectionCount > 1

    member t.SelectionCount
        with set v =
            selectionCount <- v
            t.OnPropertyChanged("IsSingleSelected")
            t.OnPropertyChanged("IsMultipleSelected")
