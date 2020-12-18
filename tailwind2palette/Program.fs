open System.IO

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
  
  let colorsFromFile file =
    File.ReadAllText file |> TailwindCss.Colors.fromString

  let mapColor (color : TailwindCss.RgbColor) : Gimp.RgbColor =
    let formatHex (color : TailwindCss.RgbColor)
      = $"#{color.R:x2}{color.G:x2}{color.B:x2}"

    {
      Name = formatHex color
      R = color.R
      G = color.G
      B = color.B
    }
  
  let readPalette fileIn paletteName =
    fileIn
    |> colorsFromFile
    |> Array.map mapColor
    |> Gimp.Palette.create paletteName

  let printPalette (p : Gimp.Palette) =
    printf $"Palette '{p.Name}', Colors: {p.Colors.Length}"
    p

  let run () =

    let args = Arguments.def

    readPalette args.InputFile args.PaletteName
    |> printPalette
    |> Gimp.Palette.writeToFile args.OutputFile
  
    ()

Program.run ()