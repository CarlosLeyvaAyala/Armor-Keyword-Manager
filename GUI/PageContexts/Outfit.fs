namespace GUI.PageContexts

open DMLib
open DMLib.Collections
open Data.UI.Outfit

/// Context for working with the outfits page
type OutfitPageCtx() =
    inherit WPFBindable()
    let mutable currentSelection = ""

    member _.Nav = Nav.createFull () |> toObservableCollection

    member t.LoadNav() = t.OnPropertyChanged("Nav")

    member t.SelectOutfit uId =
        currentSelection <- if t.Nav.Count = 0 then "" else uId
        t.OnPropertyChanged("Selected")

    member _.Selected = NavItem(currentSelection)
