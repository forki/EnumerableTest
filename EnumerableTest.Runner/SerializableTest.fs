﻿namespace EnumerableTest.Runner

open System
open System.Collections.Generic
open EnumerableTest
open EnumerableTest.Sdk

[<Serializable>]
[<AbstractClass>]
type SerializableTest(name) =
  abstract IsPassed: bool

  member this.Name: string = name

[<Serializable>]
[<Sealed>]
type SerializableAssertionTest(name, isPassed, message, data) =
  inherit SerializableTest(name)

  member this.Message =
    (message: option<string>)

  member this.MessageOrNull =
    this.Message |> Option.toObj

  member this.Data: SerializableTestData =
    data

  override this.IsPassed =
    isPassed

[<Serializable>]
[<Sealed>]
type SerializableGroupTest(name, tests, error) =
  inherit SerializableTest(name)

  member this.Tests =
    (tests: array<SerializableTest>)

  member this.Exception =
    (error: option<exn>)

  member this.ExceptionOrNull =
    this.Exception |> Option.toObj

  override val IsPassed =
    tests |> Array.forall (fun test -> (test: SerializableTest).IsPassed)

[<AutoOpen>]
module SerializableTestExtension =
  let (|AssertionTest|GroupTest|) (test: SerializableTest) =
    match test with
    | :? SerializableAssertionTest as test ->
      AssertionTest test
    | :? SerializableGroupTest as test ->
      GroupTest test
    | _ ->
      ArgumentException() |> raise

[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
module SerializableTest =
  let rec ofAssertionTest (test: AssertionTest) =
    let message =
      test.MessageOrNull |> Option.ofObj
    let data =
      SerializableTestData.ofTestData test.IsPassed test.Data
    SerializableAssertionTest(test.Name, test.IsPassed, message, data)

  let rec ofGroupTest (test: GroupTest) =
    let tests = test.Tests |> Seq.map ofTest |> Seq.toArray
    let e = test.ExceptionOrNull |> Option.ofObj
    SerializableGroupTest(test.Name, tests, e)

  and ofTest (test: Test): SerializableTest =
    match test with
    | :? AssertionTest as test ->
      test |> ofAssertionTest :> _
    | :? GroupTest as test ->
      test |> ofGroupTest :> _
    | _ ->
      ArgumentException() |> raise

  let rec assertions =
    function
    | AssertionTest test ->
      test |> Seq.singleton
    | GroupTest test ->
      test.Tests |> Seq.collect assertions
