namespace ArmorPreviewer

open System
open System.IO
open System.Collections.Generic
open Elmish
open Avalonia
open Avalonia.Controls
open Avalonia.Controls.ApplicationLifetimes
open Avalonia.FuncUI
open Avalonia.FuncUI.DSL
open Avalonia.FuncUI.Elmish
open Avalonia.FuncUI.Hosts
open Avalonia.Layout
open Avalonia.Media
open Avalonia.Media.Imaging
open Avalonia.Themes.Fluent

type ArmorItem =
    { FormKeyStr: string
      EditorId: string
      Name: string
      PluginName: string
      CustomNotes: string
      PiecePngPath: string
      SetPngPath: string
      ExtraPiecePngPaths: string list }

type State =
    { Mo2RootDir: string
      AvailableProfiles: string list
      SelectedProfile: string option
      CurrentEnv: SkyrimEnvironment option
      SearchQuery: string
      AllArmors: ArmorItem list
      SelectedArmor: ArmorItem option
      StatusMessage: string }

type Msg =
    | SetMo2Root of string
    | ScanMo2Profiles
    | SelectProfile of string
    | SetSearchQuery of string
    | SelectArmor of ArmorItem option
    | ArmorsLoaded of ArmorItem list

module BitmapCache =
    let private cache = Dictionary<string, Bitmap>(StringComparer.OrdinalIgnoreCase)

    let tryGet (path: string) : Bitmap option =
        if String.IsNullOrWhiteSpace(path) || not (File.Exists(path)) then
            None
        else
            match cache.TryGetValue(path) with
            | true, bmp -> Some bmp
            | false, _ ->
                let bmp = new Bitmap(path)
                cache[path] <- bmp
                Some bmp

    let clear () =
        for bmp in cache.Values do
            bmp.Dispose()

        cache.Clear()

module App =
    let init () : State * Cmd<Msg> =
        { Mo2RootDir = @"C:\Modding\MO2"
          AvailableProfiles = []
          SelectedProfile = None
          CurrentEnv = None
          SearchQuery = ""
          AllArmors = []
          SelectedArmor = None
          StatusMessage = "Select an MO2 directory or Vanilla Data path to begin." },
        Cmd.none

    let update (msg: Msg) (state: State) : State * Cmd<Msg> =
        match msg with
        | SetMo2Root dir -> { state with Mo2RootDir = dir }, Cmd.none

        | ScanMo2Profiles ->
            let profiles = Mo2Profile.listProfiles state.Mo2RootDir

            let status =
                if List.isEmpty profiles then
                    "No MO2 profiles found in that folder."
                else
                    sprintf "Found %d MO2 profile(s)." profiles.Length

            { state with
                AvailableProfiles = profiles
                StatusMessage = status },
            Cmd.none

        | SelectProfile profile ->
            let env = Mo2Profile.loadFromMo2 state.Mo2RootDir profile

            { state with
                SelectedProfile = Some profile
                CurrentEnv = Some env
                StatusMessage =
                    sprintf
                        "Loaded profile '%s' (%d active plugins, %d BSAs)."
                        profile
                        env.ActivePluginPaths.Length
                        env.OrderedBsaPaths.Length },
            Cmd.none

        | SetSearchQuery query -> { state with SearchQuery = query }, Cmd.none

        | SelectArmor armor -> { state with SelectedArmor = armor }, Cmd.none

        | ArmorsLoaded armors ->
            { state with
                AllArmors = armors
                SelectedArmor = List.tryHead armors
                StatusMessage = sprintf "Loaded %d armor records." armors.Length },
            Cmd.none

    let private renderTooltipImage (pngPath: string) (label: string) (size: float) =
        match BitmapCache.tryGet pngPath with
        | Some bmp ->
            StackPanel.create
                [ StackPanel.orientation Orientation.Vertical
                  StackPanel.spacing 4.0
                  StackPanel.children
                      [ Border.create
                            [ Border.background (SolidColorBrush(Color.FromArgb(40uy, 255uy, 255uy, 255uy)))
                              Border.cornerRadius 6.0
                              Border.padding 4.0
                              Border.child (
                                  Image.create
                                      [ Image.source bmp
                                        Image.width size
                                        Image.height size
                                        Image.stretch Stretch.Uniform ]
                              ) ]
                        TextBlock.create
                            [ TextBlock.text label
                              TextBlock.fontSize 11.0
                              TextBlock.horizontalAlignment HorizontalAlignment.Center ] ] ]
            :> Types.IView
        | None ->
            Border.create
                [ Border.width size
                  Border.height size
                  Border.background (SolidColorBrush(Color.FromArgb(30uy, 128uy, 128uy, 128uy)))
                  Border.cornerRadius 6.0
                  Border.child (
                      TextBlock.create
                          [ TextBlock.text (sprintf "No render\n(%s)" label)
                            TextBlock.fontSize 11.0
                            TextBlock.horizontalAlignment HorizontalAlignment.Center
                            TextBlock.verticalAlignment VerticalAlignment.Center ]
                  ) ]
            :> Types.IView

    let private armorListItemView (item: ArmorItem) =
        Border.create
            [ Border.padding (8.0, 6.0)
              Border.background Brushes.Transparent
              ToolTip.showDelay 150
              ToolTip.betweenShowDelay 50
              ToolTip.tip (
                  StackPanel.create
                      [ StackPanel.orientation Orientation.Horizontal
                        StackPanel.spacing 12.0
                        StackPanel.children
                            [ renderTooltipImage item.PiecePngPath "Selected Piece" 180.0
                              renderTooltipImage item.SetPngPath "Full Armor Set" 220.0
                              for extraPath in item.ExtraPiecePngPaths do
                                  renderTooltipImage extraPath (Path.GetFileNameWithoutExtension(extraPath)) 140.0 ] ]
              )
              Border.child (
                  Grid.create
                      [ Grid.columnDefinitions "*, Auto"
                        Grid.children
                            [ StackPanel.create
                                  [ Grid.column 0
                                    StackPanel.children
                                        [ TextBlock.create
                                              [ TextBlock.text (
                                                    if String.IsNullOrWhiteSpace(item.Name) then
                                                        item.EditorId
                                                    else
                                                        item.Name
                                                )
                                                TextBlock.fontWeight FontWeight.SemiBold ]
                                          TextBlock.create
                                              [ TextBlock.text (sprintf "%s  •  %s" item.EditorId item.PluginName)
                                                TextBlock.fontSize 11.0
                                                TextBlock.opacity 0.7 ] ] ]
                              if not (String.IsNullOrWhiteSpace(item.CustomNotes)) then
                                  TextBlock.create
                                      [ Grid.column 1
                                        TextBlock.text item.CustomNotes
                                        TextBlock.fontSize 11.0
                                        TextBlock.verticalAlignment VerticalAlignment.Center
                                        TextBlock.opacity 0.85 ] ] ]
              ) ]

    let view (state: State) (dispatch: Msg -> unit) =
        let filteredArmors =
            if String.IsNullOrWhiteSpace(state.SearchQuery) then
                state.AllArmors
            else
                let q = state.SearchQuery.Trim()

                state.AllArmors
                |> List.filter (fun a ->
                    a.Name.Contains(q, StringComparison.OrdinalIgnoreCase)
                    || a.EditorId.Contains(q, StringComparison.OrdinalIgnoreCase)
                    || a.CustomNotes.Contains(q, StringComparison.OrdinalIgnoreCase))

        DockPanel.create
            [ DockPanel.margin 12.0
              DockPanel.children
                  [ StackPanel.create
                        [ DockPanel.dock Dock.Top
                          StackPanel.orientation Orientation.Horizontal
                          StackPanel.spacing 8.0
                          StackPanel.margin (0.0, 0.0, 0.0, 10.0)
                          StackPanel.children
                              [ TextBox.create
                                    [ TextBox.width 360.0
                                      TextBox.watermark "Mod Organizer 2 Root Folder..."
                                      TextBox.text state.Mo2RootDir
                                      TextBox.onTextChanged (SetMo2Root >> dispatch) ]
                                Button.create
                                    [ Button.content "Scan MO2 Profiles"
                                      Button.onClick (fun _ -> dispatch ScanMo2Profiles) ]
                                ComboBox.create
                                    [ ComboBox.width 200.0
                                      ComboBox.placeholderText "Select Profile..."
                                      ComboBox.dataItems state.AvailableProfiles
                                      ComboBox.selectedItem (state.SelectedProfile |> Option.map box |> Option.toObj)
                                      ComboBox.onSelectedItemChanged (fun obj ->
                                          match obj with
                                          | :? string as p -> dispatch (SelectProfile p)
                                          | _ -> ()) ] ] ]

                    TextBlock.create
                        [ DockPanel.dock Dock.Bottom
                          TextBlock.margin (0.0, 8.0, 0.0, 0.0)
                          TextBlock.fontSize 12.0
                          TextBlock.opacity 0.8
                          TextBlock.text state.StatusMessage ]

                    Grid.create
                        [ Grid.columnDefinitions "420, 12, *"
                          Grid.children
                              [ DockPanel.create
                                    [ Grid.column 0
                                      DockPanel.children
                                          [ TextBox.create
                                                [ DockPanel.dock Dock.Top
                                                  TextBox.margin (0.0, 0.0, 0.0, 8.0)
                                                  TextBox.watermark "Filter armors by name, EditorID, or notes..."
                                                  TextBox.text state.SearchQuery
                                                  TextBox.onTextChanged (SetSearchQuery >> dispatch) ]
                                            ListBox.create
                                                [ ListBox.dataItems filteredArmors
                                                  ListBox.itemTemplate (
                                                      DataTemplateView<ArmorItem>.create armorListItemView
                                                  )
                                                  ListBox.onSelectedItemChanged (fun obj ->
                                                      match obj with
                                                      | :? ArmorItem as item -> dispatch (SelectArmor(Some item))
                                                      | _ -> dispatch (SelectArmor None)) ] ] ]

                                Border.create
                                    [ Grid.column 2
                                      Border.borderThickness 1.0
                                      Border.borderBrush (SolidColorBrush(Color.FromArgb(50uy, 255uy, 255uy, 255uy)))
                                      Border.cornerRadius 8.0
                                      Border.padding 16.0
                                      Border.child (
                                          match state.SelectedArmor with
                                          | Some armor ->
                                              StackPanel.create
                                                  [ StackPanel.spacing 16.0
                                                    StackPanel.children
                                                        [ TextBlock.create
                                                              [ TextBlock.text (
                                                                    if String.IsNullOrWhiteSpace(armor.Name) then
                                                                        armor.EditorId
                                                                    else
                                                                        armor.Name
                                                                )
                                                                TextBlock.fontSize 20.0
                                                                TextBlock.fontWeight FontWeight.Bold ]
                                                          StackPanel.create
                                                              [ StackPanel.orientation Orientation.Horizontal
                                                                StackPanel.spacing 24.0
                                                                StackPanel.children
                                                                    [ renderTooltipImage
                                                                          armor.PiecePngPath
                                                                          "Selected Piece"
                                                                          300.0
                                                                      renderTooltipImage
                                                                          armor.SetPngPath
                                                                          "Full Set"
                                                                          360.0 ] ] ] ]
                                              :> Types.IView
                                          | None ->
                                              TextBlock.create
                                                  [ TextBlock.text
                                                        "Select an armor from the list (or hover over any row for quick multi-image preview)."
                                                    TextBlock.horizontalAlignment HorizontalAlignment.Center
                                                    TextBlock.verticalAlignment VerticalAlignment.Center
                                                    TextBlock.opacity 0.6 ]
                                              :> Types.IView
                                      ) ] ] ] ] ]

type MainWindow() as this =
    inherit HostWindow()

    do
        base.Title <- "Skyrim Armor & Set Previewer"
        base.Width <- 1180.0
        base.Height <- 720.0

        Program.mkProgram App.init App.update App.view
        |> Program.withHost this
        |> Program.run

type App() =
    inherit Application()

    override this.Initialize() = this.Styles.Add(FluentTheme())

    override this.OnFrameworkInitializationCompleted() =
        match this.ApplicationLifetime with
        | :? IClassicDesktopStyleApplicationLifetime as desktop ->
            desktop.MainWindow <- MainWindow()
            desktop.Exit.Add(fun _ -> BitmapCache.clear ())
        | _ -> ()

module Program =
    [<EntryPoint; STAThread>]
    let main args =
        AppBuilder.Configure<App>().UsePlatformDetect().LogToTrace().StartWithClassicDesktopLifetime(args)
