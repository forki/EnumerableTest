﻿namespace EnumerableTest.Runner.Console

open System
open System.Reflection
open EnumerableTest.Sdk
open EnumerableTest.Runner

[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
module TestClass =
  let create (typ: Type): TestClass =
    let methodInfos = typ |> TestClassType.testMethodInfos
    let instantiate = typ |> TestClassType.instantiate
    let (result, instantiationError) =
      try
        let result =
          methodInfos
          |> Seq.map (fun m -> m |> TestMethod.create (instantiate ()))
          |> Seq.toArray
        (result, None)
      with
      | e ->
        ([||], Some e)
    let testClass =
      {
        TypeFullName                    = (typ: Type).FullName
        InstantiationError              = instantiationError
        Result                          = result
      }
    testClass

  let testMethods (testClass: TestClass): array<TestMethod> =
    match testClass.InstantiationError with
    | Some e ->
      let name = "default constructor"
      let result = new GroupTest(name, [||], e)
      [| TestMethod.ofResult name result None TimeSpan.Zero |]
    | None ->
      testClass.Result

  let assertions (testClass: TestClass) =
    testClass.Result
    |> Seq.collect (fun testMethod -> testMethod.Result.Assertions)

  let isPassed (testClass: TestClass) =
    testClass.InstantiationError.IsNone
    && testClass.Result |> Seq.forall TestMethod.isPassed

[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
module TestSuite =
  let ofAssemblyLazy (assembly: Assembly) =
    assembly.GetTypes()
    |> Seq.filter (fun typ -> typ |> TestClassType.isTestClass)
    |> Seq.map (fun typ -> (typ, fun () -> typ |> TestClass.create))
