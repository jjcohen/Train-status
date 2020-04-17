module Api

open System
open FSharp.Data

type LineStatus = XmlProvider<"LineStatus.xml">
type ArrivalTimes = JsonProvider<"arrivals.json">
type Journeys = JsonProvider<"journey.json">

type Stations = 
    | Liverpool_street
    | Forest_gate
    | Wantead_park
    | Gospel_Oak
    with
    member this.ToStationCode =
        match this with
        | Liverpool_street -> "910GLIVST"
        | Forest_gate      -> "910GFRSTGT"
        | Wantead_park     -> "910GGOSPLOK"
        | Gospel_Oak       -> "910GWNSTDPK"
    override this.ToString() =
        match this with
        | Liverpool_street -> "Liverpool street"
        | Forest_gate      -> "Forest gate"
        | Wantead_park     -> "Gospel oak"
        | Gospel_Oak       -> "Wanstead park"
        

let getData fromStation toStation =
    ArrivalTimes.Load(sprintf "https://api.tfl.gov.uk/StopPoint/%s/arrivals" fromStation)
    |> Array.choose (fun x ->
        if x.DestinationNaptanId = toStation
        then 
            let f = DateTime.Now - x.ExpectedArrival.LocalDateTime
            f.TotalMinutes |> int |> Some
        else None)
    |> Array.sort

let getFastestJourneyTime fromPostCode toPostCode = async {
        let! journeys = Journeys.AsyncLoad(sprintf "https://api.tfl.gov.uk/journey/journeyresults/%s/to/%s" fromPostCode toPostCode)

        let fastest = 
            journeys.Journeys
            |> Array.map (fun x -> x.Duration)
            |> Array.sort
            |> Array.tryHead

        return fastest
    }