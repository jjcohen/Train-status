module Api

open System
open FSharp.Control.Tasks
open FSharp.Data

//def station_arrivals(times):
//    arrivals_string = ', '.join(map(str, times))
//    if len(times) > 0:
//        return arrivals_string
//    else:
//        return 'No trains'

//def fastest_journey_time(from_post_code, to_post_code):
//    req = requests.get( 'https://api.tfl.gov.uk/journey/journeyresults/' + from_post_code + '/to/' + to_post_code).json()
//    journeys = req['journeys']
//    times = sorted(list(map(lambda x: x['duration'],  journeys)))
//    if len(times) > 0:
//        return str(times[0])
//    else:
//        return str(-1)
//        # todo: what to do about this case

//let retrieveLineStatuses () =
//    try
//        let details =
//            Document.Load("http://cloud.tfl.gov.uk/TrackerNet/LineStatus").LineStatuses
//            |> Array.map (fun lineStatus -> (lineStatus.Line.Name, lineStatus.Status.Description))

//        let (namePadding, descriptionPadding) =
//            let names, descriptions = details |> Array.unzip
//            let max (arr:string []) = arr |> Array.maxBy (fun x -> x.Length)
//            (max names).Length, (max descriptions).Length

//        let applyPadding ((str1:String),(str2:String)) =
//            (str1.PadRight(namePadding, '.'),
//             str2.PadRight(descriptionPadding, '.'))
    

//while true do
//    let status = () |> retrieveLineStatuses
    
//    printToScreen 10 0 status
//    |> ignore
//    |> millisecondsUntilNextMinute
//    |> Thread.Sleep


type LineStatus = XmlProvider<"LineStatus.xml">
type ArrivalTimes = JsonProvider<"arrivals.json">

//def arrival_times(from_stn, to_stn):
//    from_arrivals = requests.get( 'https://api.tfl.gov.uk/StopPoint/' + from_stn + '/arrivals').json()
//    filtered_arrivals = list(filter(lambda x: x['destinationNaptanId'] == to_stn, from_arrivals))
//    prettified_strings = list(map(lambda x: prettify_datetime(x['expectedArrival']),  filtered_arrivals))
//    return list(filter(lambda x: x != "", prettified_strings))

let millisecondsUntilNextMinute () =
    ((60 - System.DateTime.Now.Second) * 1000) + System.DateTime.Now.Millisecond

let liverpool_street_code = "910GLIVST"
let liverpool_street_name = "Liverpool street"

let forest_gate_code = "910GFRSTGT"
let forest_gate_name = "Forest gate"
 
let gospel_oak_code = "910GGOSPLOK"
let gospel_oak_name = "Gospel oak"
 
let wanstead_park_code = "910GWNSTDPK"
let wanstead_park_name = "Wantead park"

let getData fromStation toStation =
    ArrivalTimes.Load(sprintf "https://api.tfl.gov.uk/StopPoint/%s/arrivals" fromStation)
    |> Array.filter (fun x -> x.DestinationNaptanId = toStation)
    |> Array.map (fun x ->
        let f = DateTime.Now - x.ExpectedArrival.LocalDateTime
        f.TotalMinutes |> int )