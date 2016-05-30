namespace Tzix.Model

open System
open System.IO
open System.Text.RegularExpressions
open FParsec

module ImportRule =
  type Rule =
    | Include             of string
    | Exclude             of string

  let ofRuleList rules =
    let separate =
      function
      | Include s -> (Some s, None)
      | Exclude s -> (None, Some s)
    let (incs, excs) =
      rules |> List.map separate 
      |> List.unzip
      |> T2.map (List.choose id)
    in
      {
        Roots             = incs |> List.map (fun path -> MyDirectoryInfo(path) :> IDirectory)
        Exclusions        = excs |> List.map (fun pattern -> Regex(pattern))
      }

  let empty =
    {
      Roots                 = []
      Exclusions            = []
    }

  module private Parser =
    open FParsec.CharParsers
    open FParsec.Primitives

    let lineBeginningWith (mark: string) =
      skipString mark >>. spaces >>. restOfLine ((* skipNewLine = *) false)

    let includeLine = lineBeginningWith "+" |>> (Include >> Some)
    let excludeLine = lineBeginningWith "-" |>> (Exclude >> Some)
    let commentLine = lineBeginningWith "#" >>% None
    let emptyLine   = skipMany (skipAnyOf " \t") >>% None

    let line =
      [
        includeLine
        excludeLine
        commentLine
        emptyLine
      ]
      |> Seq.map attempt
      |> choice

    let rules: Parser<_, unit> =
      (sepEndBy line newline .>> eof)
      |>> (List.choose id)

    let run (name: string) (source: string) =
      runParserOnString rules () name source

  let parse name source =
    match Parser.run name source with
    | Success (rules, (), _) ->
        rules |> ofRuleList
    | Failure (msg, err, ()) ->
        raise (Exception(msg))

  let excludes name rule =
    rule.Exclusions |> List.exists (fun regex -> regex.IsMatch(name))
