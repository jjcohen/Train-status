module MyProg 

open System.Windows.Forms
open System.Windows
open System.Drawing
open System
open System.Windows.Input

type MainWindowViewModel(w,h) =
    let mutable width = w
    let mutable height = h
    member __.Width with get() = width
    member __.Width with set (s:int) = ()
    member __.Height with get() = height
    member __.Height with set (s:int) = ()

let menuItem index text eventHandler = 
    let m = new System.Windows.Forms.MenuItem(Index = index, Text = text)
    m.Click.Add eventHandler
    m

let contextMenu =
    let menu = new System.Windows.Forms.ContextMenu()

    menu.MenuItems.AddRange([|
        menuItem 0 "Settings" (fun _ -> ())
        menuItem 1 "Exit" (fun _ -> Application.Current.Shutdown())
    |])

    menu

[<EntryPoint; STAThread>]  
let main(_) =
    let window = Application.LoadComponent(new Uri("Status.xaml", UriKind.Relative)) :?> Window
    let width = 300
    let height = 600
    window.DataContext <- (new MainWindowViewModel(width, height)) :> obj

    window.Left <- System.Windows.SystemParameters.WorkArea.Right - (width |> float)
    window.Top <- System.Windows.SystemParameters.WorkArea.Bottom - (height |> float)
    window.Visibility <- Visibility.Hidden

    let icon = new NotifyIcon()
    icon.Icon <- new Icon("train_white.ico")

    window.Deactivated.Add (fun _ -> window.Visibility <- Visibility.Hidden)
    window.Deactivated.Add (fun _ -> System.Diagnostics.Debug.WriteLine("Deactivated"))

    window.PreviewKeyDown.Add (fun e -> if e.Key = Key.Escape then window.Visibility <- Visibility.Hidden)

    icon.ContextMenu <- contextMenu

    let toggleDialog (e: System.Windows.Forms.MouseEventArgs) =
        if e.Button = MouseButtons.Left then
            if not window.IsVisible then
                window.Visibility <- Visibility.Visible
            else
                window.Visibility <- Visibility.Hidden

    icon.MouseClick.Add toggleDialog

    icon.Text <- "Transport status"
    icon.Visible <- true
        
    (new Application()).Run(window)