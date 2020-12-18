module TailwindCss

open System
open System.Text.RegularExpressions

type RgbColor = {
  R: byte
  G: byte
  B: byte
}

module RgbColor =
  let fromHexString s : RgbColor option =

    let fromMatch valueConverter (m : Match) =
      let value (s : string) = m.Groups.[s].Value |> valueConverter
      if m.Success then
        Some {
          R = value "R"
          G = value "G"
          B = value "B"
        }
      else
        None
    
    let fromRegex (r : Regex) valueConverter (s : string) =
      s
      |> r.Match
      |> fromMatch valueConverter
    
    let fromHexTriplets =
      let valueConverter s = Convert.ToByte(s, 16)
      let r = Regex "^#(?<R>[0-9a-f]{2})(?<G>[0-9a-f]{2})(?<B>[0-9a-f]{2})$"
      fromRegex r valueConverter

    let fromHexShortHand =
      let valueConverter s = Convert.ToByte(s + s, 16)
      let r = Regex "^#(?<R>[0-9a-f]{1})(?<G>[0-9a-f]{1})(?<B>[0-9a-f]{1})$"
      fromRegex r valueConverter

    seq {
      fromHexTriplets
      fromHexShortHand
    }
    |> Seq.map (fun f -> f s)
    |> Seq.choose id
    |> Seq.tryHead

module Colors =
  let fromString s =
    let r = Regex "(?:['\"](?<Color>#[0-9a-f]{3,6})['\"])"
    let matches = r.Matches s
    matches
    |> Array.ofSeq
    |> Array.map (fun m -> m.Groups.["Color"].Value)
    |> Array.map RgbColor.fromHexString
    |> Array.choose id
