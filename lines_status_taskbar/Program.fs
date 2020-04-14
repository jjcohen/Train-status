module MyProg 

open System.Windows.Forms
open System.Windows
open System.Drawing
open System
open System.Windows.Input
open System.Windows.Threading

type MainWindowViewModel(w,h) =
    inherit ViewModule.ViewModelBase()

    let mutable width = w
    let mutable height = h
    let mutable journeyTime = 0
    member __.Width with get() = width
    member __.Width with set (s:int) = ()
    member __.Height with get() = height
    member __.Height with set (s:int) = ()
    member __.PrintJourneyTime = sprintf "%i mins" journeyTime
    member __.JourneyTime with set s = journeyTime <- s

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

    let mainWindowViewModel = new MainWindowViewModel(width, height)
    window.DataContext <- mainWindowViewModel

    let update () = async{
       let! fastest = Api.getFastestJourneyTime "SW1A1AA" "SW1A0AA"
       System.Diagnostics.Debug.WriteLine(sprintf "%A" fastest)
       match fastest with Some s -> mainWindowViewModel.JourneyTime <- s | None -> ()
       window.DataContext <- mainWindowViewModel :> obj
    }

    let dispatcherTimer = new DispatcherTimer()
    dispatcherTimer.Interval <- TimeSpan.FromSeconds(10.0)
    dispatcherTimer.Tick.Add (fun _ -> update () |> Async.StartImmediate)
    dispatcherTimer.Start()


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
                window.Topmost <- true
            else
                window.Visibility <- Visibility.Hidden

    icon.MouseClick.Add toggleDialog

    icon.Text <- "Transport status"
    icon.Visible <- true
        
    (new Application()).Run(window)