﻿namespace EnumerableTest.Runner.Console

open System
open Argu
open EnumerableTest.Runner

[<RequireQualifiedAccess>]
type AppArgument =
  | [<MainCommand>]
    Files of list<string>
  | Verbose
  | Timeout of int
  | Recursion
    of int
with
  interface IArgParserTemplate with
    member this.Usage =
      match this with
      | Files _ ->
        "Paths to test assembly"
      | Verbose ->
        "Print debug outputs"
      | Timeout _ ->
        "Timeout [ms] for a test assembly execution"
      | Recursion _ ->
        "Max nesting level for value serialization"
