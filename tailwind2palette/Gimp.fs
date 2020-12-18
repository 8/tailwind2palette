module Gimp

open System.IO

type RgbColor = {
  Name: string
  R: byte
  G: byte
  B: byte
}

type Palette = {
  Name: string
  Colors: RgbColor array
}

module Palette =
  let create name colors =
    {
      Colors = colors
      Name = name
    }

  let write (stream : Stream) (palette : Palette) =
    
    let formatColor (color : RgbColor): string =
      let formatPart p = p.ToString().PadLeft(3)
      //let formatHex color = $"{color.R:x2}{color.G:x2}{color.B:x2}"
      //$"{formatPart color.R} {formatPart color.G} {formatPart color.B} #{formatHex color}"
      $"{formatPart color.R} {formatPart color.G} {formatPart color.B} {color.Name}"
    
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

