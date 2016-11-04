﻿namespace EnumerableTest.Runner

open System
open System.Reflection
open Basis.Core
open EnumerableTest.Sdk

/// Represents an instance of a test class.
type TestInstance =
  obj

type TestMethod =
  {
    MethodName                  : string
    Run                         : TestInstance -> GroupTest
  }

type TestClass =
  {
    TypeFullName                : string
    Create                      : unit -> TestInstance
    Methods                     : seq<TestMethod>
  }

type TestSuite =
  seq<TestClass>

/// Denotes where an exception was thrown.
[<RequireQualifiedAccess>]
type TestErrorMethod =
  | Constructor
  | Method
  | Dispose

type TestError =
  {
    Method                      : TestErrorMethod
    Error                       : Exception
  }
with
  static member Create(errorMethod, error) =
    {
      Method                    = errorMethod
      Error                     = error
    }

  static member OfConstructor(error) =
    TestError.Create(TestErrorMethod.Constructor, error)

  static member OfDispose(error) =
    TestError.Create(TestErrorMethod.Dispose, error)

  static member OfMethod(error) =
    TestError.Create(TestErrorMethod.Method, error)

type TestMethodResult =
  TestMethod * Result<GroupTest, TestError>

type TestClassResult =
  TestClass * TestMethodResult []

type TestSuiteResult =
  seq<TestClassResult>
