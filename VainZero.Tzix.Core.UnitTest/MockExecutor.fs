﻿namespace VainZero.Tzix.Core.UnitTest

open VainZero.Tzix.Core

type MockExecutor() =
  interface IExecutor with
    member this.Execute(fsys, path) =
      let dir         = fsys.DirectoryInfo(path)
      let file        = fsys.FileInfo(path)
      if not (dir.Exists || file.Exists) then 
        failwith "File not found"
