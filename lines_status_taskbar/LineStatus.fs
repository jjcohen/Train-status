module LineStatus

open FSharp.Data

type Document = XmlProvider<"LineStatus.xml">

let details =
    Document.Load("http://cloud.tfl.gov.uk/TrackerNet/LineStatus").LineStatuses
    |> Array.map (fun lineStatus -> (lineStatus.Line.Name, lineStatus.Status.Description))