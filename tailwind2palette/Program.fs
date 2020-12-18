open System.IO
open System

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
  
  let colorDefinitionsFromFile file =
    File.ReadAllText file
    |> TailwindCss.ColorDefinitions.fromString

  let gimpColorsFromColorDefinitions (colorDefinitions : TailwindCss.ColorDefinitions) =
    colorDefinitions
    |> Array.map (fun colorDef -> 
      match colorDef with
      | TailwindCss.NamedColor namedColor -> [|{
          Gimp.RgbColor.Name = namedColor.Name
          Gimp.RgbColor.R = namedColor.Color.R
          Gimp.RgbColor.G = namedColor.Color.G
          Gimp.RgbColor.B = namedColor.Color.B
        } |]
      | TailwindCss.NamedColorGroup group ->
          group.NamedColors
          |> Array.map (fun namedColor -> {
            Gimp.RgbColor.Name = $"{group.Name}-{namedColor.Name}"
            Gimp.RgbColor.R = namedColor.Color.R
            Gimp.RgbColor.G = namedColor.Color.G
            Gimp.RgbColor.B = namedColor.Color.B
          })
      )
    |> Array.concat

  
  let readPalette fileIn paletteName =
    fileIn
    |> colorDefinitionsFromFile
    |> gimpColorsFromColorDefinitions
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