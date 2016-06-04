namespace Tzix.Model

open System.Diagnostics

type IExecutor =
  abstract member Execute: IFileSystem * string -> unit

type DotNetExecutor private() =
  static member val private LazyInstance = lazy DotNetExecutor()
  static member Instance = DotNetExecutor.LazyInstance.Value

  interface IExecutor with
    member this.Execute(fsys, path) =
      Process.Start(path) |> ignore
