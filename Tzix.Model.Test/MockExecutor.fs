namespace Tzix.Model.Test

open Tzix.Model

type MockExecutor() =
  interface IExecutor with
    member this.Execute(_, _) = ()
