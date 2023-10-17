﻿namespace GUI

open DMLib_WPF.Contexts
open IO.AppSettingsTypes
open IO
open GUI.PageContexts
open System.IO
open DMLib.IO.Path
open DMLib.IO
open DMLib.String
open DMLib.Combinators
open System

module TagManager = Data.Tags.Manager

[<Sealed>]
type AppCtx() =
    inherit ApplicationContext()

    let addReservedTags () =
        TagManager.addReservedTags Data.SPID.SpidRule.allAutoTags Data.Tags.AutoOutfit
        TagManager.addReservedTags Data.Items.ArmorType.allAutoTags Data.Tags.AutoItem

    let loadKeywords () =
        Keywords.File.Open(AppSettings.Paths.KeywordsFile())

    do
        addReservedTags ()

        PropietaryFile.OnFileOpen
        |> Event.add TagManager.rebuildCache

        Data.WAED.ENCH_Db.OnxEditImported
        |> Event.add IO.WAED.File.saveMgef

        Data.WAED.ENCH_Db.OnxEditImported
        |> Event.add IO.WAED.File.saveEnch

        // onAppPathChanged
        AppSettings.Paths.onAppPathChanged
        |> Event.choose (fun e ->
            match e with
            | ApplicationPath _ -> Some()
            | DummyOption -> None)
        |> Event.add (fun _ ->
            Keywords.File.Open(AppSettings.Paths.KeywordsFile())
            SpidAutocompletion.init ())

    let mutable _xEditPath = ""
    let mutable bgWorkCaption = ""

    member val FileWatchers = FileWatchers()
    // TODO: Get from reading the script
    member val ScriptHasUpdates = true

    member t.xEditPath
        with get () = _xEditPath
        and set v =
            _xEditPath <- v

            t.FileWatchers.path <-
                match v with
                | FileExists fn -> fn |> getDir |> combine2' "Edit Scripts"
                | _ -> ""

            [ nameof t.xEditPath
              nameof t.xEditDirExists ]
            |> t.OnPropertyChanged

    member t.xEditDirExists = t.xEditPath |> Path.Exists
    member _.ReloadKeywords() = loadKeywords ()

    member t.BackgroundWorkCaption
        with get () = bgWorkCaption
        and set v =
            bgWorkCaption <- v

            [ nameof t.BackgroundWorkCaption
              nameof t.IsWorkingInBackground ]
            |> t.OnPropertyChanged

    member _.IsWorkingInBackground = Not isNullOrEmpty bgWorkCaption
    member val Data = AppCtxData()
