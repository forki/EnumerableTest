﻿namespace EnumerableTest.Runner.Console

open System
open System.IO
open System.Reactive.Linq
open System.Reactive.Subjects
open System.Reflection
open Argu
open Basis.Core
open Reactive.Bindings
open EnumerableTest.Runner
open EnumerableTest.Runner.UI.Notifications

module Assembly =
  let tryLoadFile (file: FileInfo) =
    try
      let assemblyName = AssemblyName.GetAssemblyName(file.FullName)
      Assembly.Load(assemblyName) |> Some
    with
    | _ -> None

module Program =
  let run isVerbose timeout (assemblyFiles: seq<FileInfo>) =
    use notifier = new Notifier<Success, Info, Warning>(new Subject<_>()) :> INotifier
    let warnings = notifier |> Notifier.collectWarnings
    let printer =
        TestPrinter(Console.Out, Console.BufferWidth - 1, isVerbose)
    let counter = AssertionCounter()
    for assemblyFile in assemblyFiles do
      match OneshotTestAssembly.ofFile(assemblyFile) with
      | Success testAssembly ->
        use testAssembly = testAssembly
        use testClassNotifier =
          new TestClassNotifier(testAssembly.Schema, testAssembly)
        use printerSubscription =
          testClassNotifier.Subscribe(printer)
        use counterSubscription =
          testClassNotifier.Subscribe(counter)
        testAssembly.Start()
        testAssembly.TestCompleted |> Observable.waitTimeout timeout |> ignore<bool>
        testClassNotifier.Complete()
      | Failure e ->
        notifier.NotifyWarning(Warning.CouldNotLoadAssemblyFile (assemblyFile, e))
    printer.PrintWarningsAsync(warnings)
    printer.PrintSummaryAsync(counter.Current)
    printer.JoinAsync() |> Async.RunSynchronously
    counter.IsPassed

  [<EntryPoint>]
  let main _ =
    let thisFile = FileInfo(Assembly.GetExecutingAssembly().Location)
    let assemblyFiles =
      FileSystemInfo.getTestAssemblies thisFile
      |> Seq.append AppArgument.files
      |> Seq.distinctBy (fun file -> file.FullName)
    MarshalValue.Recursion <- AppArgument.recursion
    let isPassed =
      run AppArgument.isVerbose AppArgument.timeout assemblyFiles
    let exitCode =
      if isPassed
        then 0
        else -1
    exitCode
