open System
open System.IO
open System.Text.RegularExpressions

type RgbColor = {
  R: byte
  G: byte
  B: byte
}

type Palette = {
  Name: string
  Colors: RgbColor array
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

module Palette =
  let fromColors name colors =
    {
      Colors = colors
      Name = name
    }

  let write (stream : Stream) (palette : Palette) =
    
    let formatColor (color : RgbColor): string =
      let formatPart p = p.ToString().PadLeft(3)
      let formatHex color = $"{color.R:x2}{color.G:x2}{color.B:x2}"
      $"{formatPart color.R} {formatPart color.G} {formatPart color.B} #{formatHex color}"
    
    use sw = new StreamWriter(stream, NewLine = "\n")
    sw.WriteLine "GIMP Palette"
    sw.WriteLine $"Name: {palette.Name}"
    sw.WriteLine "#"
    for color in palette.Colors do
      formatColor color |> sw.WriteLine
    ()
    sw.WriteLine ""

  let writeToFile file palette =
    use fs = File.OpenWrite file
    write fs palette


module Program =

  type Arguments = {
    InputFile : string
    OutputFile : string
    PaletteName : string
  }
  module Arguments =
    let def = {
      InputFile = "../../../../node_modules/tailwindcss/colors.js"
      OutputFile = "tailwindcss.gpl"
      PaletteName = "TailwindCss"
    }
    //let fromArray a =
    //  {
    //    PaletteName = "TailwindCss"
    //  }
    //let fromStdin () =
    //  Environment.GetCommandLineArgs()
    //  |> fromArray
  
  let colorsFromFile file =
    File.ReadAllText file |> Colors.fromString
  
  let palette fileIn paletteName =
    fileIn
    |> colorsFromFile
    |> Palette.fromColors paletteName

  let printPalette p =
    printf $"Palette '{p.Name}', Colors: {p.Colors.Length}"
    p

  let run () =

    let args = Arguments.def

    palette args.InputFile args.PaletteName
    |> printPalette
    |> Palette.writeToFile args.OutputFile
  
    ()

Program.run ()

//[<EntryPoint>]
//let main argv =
//  let message = from "F#" // Call the function
//  printfn "Hello world %s" message
//  0 // return an integer exit code