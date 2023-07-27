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
      InputFile = "../../../../node_modules/tailwindcss/src/public/colors.js"
      OutputFile = "tailwindcss.gpl"
      PaletteName = "Tailwind CSS"
    }
  
  let colorDefinitionsFromFile file =
    File.ReadAllText file
    |> TailwindCss.ColorDefinitions.fromString

  let gimpColorsFromColorDefinitions (colorDefinitions : TailwindCss.ColorDefinitions) =
    
    colorDefinitions
    |> Array.map (fun colorDef -> 
      match colorDef with
      | TailwindCss.NamedColor namedColor -> [|
          Gimp.RgbColor.from namedColor.Name (namedColor.Color.R, namedColor.Color.G, namedColor.Color.B)
         |]
      | TailwindCss.NamedColorGroup group ->
          group.NamedColors
          |> Array.map (fun namedColor ->
            Gimp.RgbColor.from $"{group.Name}-{namedColor.Name}" (namedColor.Color.R, namedColor.Color.G, namedColor.Color.B)
          )
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