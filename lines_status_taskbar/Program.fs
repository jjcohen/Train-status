module MyProg 

open System.Windows.Forms
open System.Windows
open System.Drawing
open System
open System.Windows.Input
open System.Windows.Threading
open System.Diagnostics

type MainWindowViewModel(w,h) =
    inherit ViewModule.ViewModelBase()

    let mutable width = w
    let mutable height = h
    let mutable journeyTime = 0
    member __.Width with get() = width
    member __.Width with set (s:int) = ()
    member __.Height with get() = height
    member __.Height with set (s:int) = ()
    member __.JourneyTime with get() = journeyTime
    member this.JourneyTime
        with set (s) =
            journeyTime <- s
            this.RaisePropertyChanged("JourneyTime")

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

type Message = Trigger of AsyncReplyChannel<bool option>

/// This actor handles the state of the window, this allows multiple triggers to fire
/// that could show or hide the window without clashing. This is especially important
/// in the case of clicking the taskbar icon which fires both a deactivate and a left
/// mouse click which race with each other.
let windowDebounceActor = MailboxProcessor.Start(fun (inbox:MailboxProcessor<Message>) ->
        let stopwatch = new Stopwatch()
        stopwatch.Start()

        let rec messageLoop (windowState:bool) = async{
            let! (Trigger replyChannel) = inbox.Receive()

            System.Diagnostics.Debug.WriteLine(sprintf "Triggered %i" stopwatch.ElapsedMilliseconds)

            if stopwatch.ElapsedMilliseconds > (200 |> int64) then
                stopwatch.Stop()
                stopwatch.Restart()

                if windowState then
                    replyChannel.Reply (Some false)
                    return! messageLoop false
                    
                else
                    replyChannel.Reply (Some true)
                    return! messageLoop true
            else
                replyChannel.Reply None
                return! messageLoop windowState
            }

        messageLoop false
        )

[<EntryPoint; STAThread>]  
let main(_) =
    let window = Application.LoadComponent(new Uri("Status.xaml", UriKind.Relative)) :?> Window
    let width = 300
    let height = 200

    let mainWindowViewModel = new MainWindowViewModel(width, height)
    window.DataContext <- mainWindowViewModel :> obj

    let update () = async{
       let! fastest = Api.getFastestJourneyTime "SW1A1AA" "SW1A0AA"
       System.Diagnostics.Debug.WriteLine(sprintf "%A" fastest)
       match fastest with Some s -> mainWindowViewModel.JourneyTime <- s | None -> ()
    }

    update () |> Async.StartImmediate

    let dispatcherTimer = new DispatcherTimer()
    dispatcherTimer.Interval <- TimeSpan.FromSeconds(60.0)
    dispatcherTimer.Tick.Add (fun _ -> update () |> Async.StartImmediate)
    dispatcherTimer.Start()

    window.Left <- System.Windows.SystemParameters.WorkArea.Right - (width |> float)
    window.Top <- System.Windows.SystemParameters.WorkArea.Bottom - (height |> float)
    window.Visibility <- Visibility.Hidden

    let icon = new NotifyIcon()
    icon.Icon <- new Icon("train_white.ico")

    let toggleWindowState () = 
        let windowState = windowDebounceActor.PostAndReply (fun (c:AsyncReplyChannel<bool option>) -> Trigger c)
        match windowState with
        | Some s ->
            if s then 
                window.Visibility <- Visibility.Hidden
            else
                window.Visibility <- Visibility.Visible
                window.Activate() |> ignore
        | None -> ()

    window.Deactivated.Add (fun _ -> toggleWindowState () )
    window.Deactivated.Add (fun _ -> System.Diagnostics.Debug.WriteLine("Deactivated"))

    icon.MouseClick.Add (fun e -> if e.Button = MouseButtons.Left then toggleWindowState () )
    icon.MouseClick.Add (fun e -> if e.Button = MouseButtons.Left then System.Diagnostics.Debug.WriteLine("Left mouse click"))

    window.PreviewKeyDown.Add (fun e -> if e.Key = Key.Escape then toggleWindowState () )
    window.PreviewKeyDown.Add (fun e -> if e.Key = Key.Escape then System.Diagnostics.Debug.WriteLine("Esc key pressed"))

    icon.ContextMenu <- contextMenu

    icon.Text <- "Transport status"
    icon.Visible <- true
        
    (new Application()).Run(window)