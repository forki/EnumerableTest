﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using EnumerableTest.Sdk;

namespace EnumerableTest
{
    /// <summary>
    /// Represents a result of a unit test.
    /// <para lang="ja">
    /// 単体テストの結果を表す。
    /// </para>
    /// </summary>
    public abstract class Test
    {
        /// <summary>
        /// Gets the name.
        /// <para lang="ja">
        /// テストの名前を取得する。
        /// </para>
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets a value indicating whether the test was passed.
        /// <para lang="ja">
        /// テストが成功したかどうかを取得する。
        /// </para>
        /// </summary>
        public abstract bool IsPassed { get; }

        /// <summary>
        /// Gets the data related to the test.
        /// <para lang="ja">
        /// テストに関連するデータを取得する。
        /// </para>
        /// </summary>
        public TestData Data { get; }

        internal Test(string name, TestData data)
        {
            Name = name;
            Data = data;
        }

        #region Factory
        /// <summary>
        /// Creates a unit test.
        /// <para lang="ja">
        /// 単体テストの結果を生成する。
        /// </para>
        /// </summary>
        /// <param name="name"></param>
        /// <param name="isPassed"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public static Test
            FromResult(
                string name,
                bool isPassed,
                TestData data
            )
        {
            return new AssertionTest(name, isPassed, data);
        }

        /// <summary>
        /// Creates a unit test.
        /// <para lang="ja">
        /// 単体テストの結果を生成する。
        /// </para>
        /// </summary>
        /// <param name="name"></param>
        /// <param name="isPassed"></param>
        /// <returns></returns>
        public static Test FromResult(string name, bool isPassed)
        {
            return new AssertionTest(name, isPassed, TestData.Empty);
        }
        #endregion

        #region Assertions
        /// <summary>
        /// Gets a unit test which is passed.
        /// <para lang="ja">
        /// 「正常」を表す単体テストの結果を取得する。
        /// </para>
        /// </summary>
        public static Test Pass { get; } =
            FromResult(nameof(Pass), true);

        /// <summary>
        /// Tests that two values are equal, using <paramref name="comparer"/>.
        /// <para lang="ja">
        /// <paramref name="comparer"/> で比較して、2つの値が等しいことを検査する。
        /// </para>
        /// </summary>
        /// <typeparam name="X"></typeparam>
        /// <param name="expected"></param>
        /// <param name="actual"></param>
        /// <param name="comparer"></param>
        /// <returns></returns>
        public static Test Equal<X>(X expected, X actual, IEqualityComparer comparer)
        {
            var isPassed = comparer.Equals(actual, expected);
            var data =
                DictionaryTestData.Build()
                .Add("Expected", expected)
                .Add("Actual", actual)
                .MakeReadOnly();
            return FromResult(nameof(Equal), isPassed, data);
        }

        /// <summary>
        /// Equivalent to <see cref="TestExtension.Is{X}(X, X)"/>.
        /// </summary>
        /// <typeparam name="X"></typeparam>
        /// <param name="expected"></param>
        /// <param name="actual"></param>
        /// <returns></returns>
        public static Test Equal<X>(X expected, X actual)
        {
            return Equal(expected, actual, StructuralComparisons.StructuralEqualityComparer);
        }

        /// <summary>
        /// Equivalent to <see cref="TestExtension.TestSatisfy{X}(X, Expression{Func{X, bool}})"/>.
        /// </summary>
        /// <typeparam name="X"></typeparam>
        /// <param name="value"></param>
        /// <param name="predicate"></param>
        /// <returns></returns>
        public static Test Satisfy<X>(X value, Expression<Func<X, bool>> predicate)
        {
            var isPassed = predicate.Compile().Invoke(value);
            var data =
                DictionaryTestData.Build()
                .Add("Value", value)
                .Add("Predicate", predicate)
                .MakeReadOnly();
            return FromResult(nameof(Satisfy), isPassed, data);
        }

        /// <summary>
        /// Tests that an action throws an exception of type <typeparamref name="E"/>.
        /// <para lang="ja">
        /// アクションが型 <typeparamref name="E"/> の例外を送出することを検査する。
        /// </para>
        /// </summary>
        /// <typeparam name="E"></typeparam>
        /// <param name="f"></param>
        /// <returns></returns>
        public static Test Catch<E>(Action f)
            where E : Exception
        {
            var exceptionOrNull = default(Exception);
            try
            {
                f();
            }
            catch (E exception)
            {
                exceptionOrNull = exception;
            }

            var data =
                DictionaryTestData.Build()
                .Add("Type", typeof(E))
                .Add("ExceptionOrNull", exceptionOrNull)
                .MakeReadOnly();
            return FromResult(nameof(Catch), !ReferenceEquals(exceptionOrNull, null), data);
        }

        /// <summary>
        /// Tests that a function throws an exception of type <typeparamref name="E"/>.
        /// <para lang="ja">
        /// 関数が型 <typeparamref name="E"/> の例外を送出することを検査する。
        /// </para>
        /// </summary>
        /// <typeparam name="E"></typeparam>
        /// <param name="f"></param>
        /// <returns></returns>
        public static Test Catch<E>(Func<object> f)
            where E : Exception
        {
            return Catch<E>(() => { f(); });
        }
        #endregion
    }
}
