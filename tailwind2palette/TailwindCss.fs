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

module RgbColors =
  let fromString s =
    let r = Regex "(?:['\"](?<Color>#[0-9a-f]{3,6})['\"])"
    let matches = r.Matches s
    matches
    |> Array.ofSeq
    |> Array.map (fun m -> m.Groups.["Color"].Value)
    |> Array.map RgbColor.fromHexString
    |> Array.choose id


type NamedColor = {
  Name: string
  Color: RgbColor
}

type NamedColorGroup = {
  Name: string
  NamedColors: NamedColor array
}

type ColorDefinition = 
  | NamedColor of NamedColor
  | NamedColorGroup of NamedColorGroup

type ColorDefinitions = ColorDefinition array

module ColorDefinitions =
  let fromString s : ColorDefinitions =

    let namedColorsFrom s : NamedColor array =
      let namedColorFrom (m : Match) : NamedColor =
        let color =
          m.Groups.["Value"].Value
          |> RgbColor.fromHexString
          |> Option.defaultWith (fun () -> raise <| ArgumentException (nameof m))
        {
          Name = m.Groups.["Name"].Value
          Color = color
        }
      let rNamedColor = Regex @"(?<NamedColor>(?<Name>[0-9]+): '(?<Value>#[0-9a-f]+)'),"
      rNamedColor.Matches s
      |> Seq.toArray
      |> Array.map namedColorFrom

    let r = Regex @"((?<NamedColorGroup>(?<Name>[a-zA-Z]+): {(?<Content>(.|\n)+?)})|(?<NamedColor>(?<Name>[a-zA-Z]+): '(?<Value>#[0-9a-f]+)'),\n)"
    let matches = r.Matches s
    
    matches
    |> Seq.toArray
    |> Array.map (fun m -> 
      let name = m.Groups.["Name"].Value
      let content = m.Groups.["Content"].Value
      let value = m.Groups.["Value"].Value
      match value with
      | "" -> content
              |> namedColorsFrom
              |> (fun colors -> NamedColorGroup { Name = name; NamedColors = colors })
      | s -> s
             |> RgbColor.fromHexString
             |> Option.get |> (fun color -> NamedColor { Name = name; Color = color } )
      )

