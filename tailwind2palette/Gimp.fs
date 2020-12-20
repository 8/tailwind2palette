module Gimp

open System.IO

type RgbColor = {
  Name: string
  R: byte
  G: byte
  B: byte
}
module RgbColor =
  let from name (r, g, b) =
    {
      Name = name
      R = r
      G = g
      B = b
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

