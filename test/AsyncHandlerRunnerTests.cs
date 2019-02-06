using System;
using System.Threading.Tasks;
using Xunit;
using Amazon.Lambda.Core;
using FakeItEasy;
using System.Reflection;
using LambdaNative.Internal;
using Shouldly;

namespace LambdaNative.Tests
{
    public class AsyncHandlerRunnerTests
    {
        public class TestHandler : IHandler, IAsyncHandler<string, string>
        {
            public virtual ILambdaSerializer Serializer { get; }

            public virtual Task<string> Handle(string input, ILambdaContext context)
            {
                throw new NotImplementedException();
            }
        }

        [Fact]
        public void Handle_CallsHandlerHandleWithInputAndContext_AndReturnsOutput()
        {
            // arrange
            var runner = new AsyncHandlerRunner<TestHandler, string, string>(true);
            var handler = A.Fake<TestHandler>();
            var context = A.Fake<ILambdaContext>();

            runner.GetType().GetField("Handler", BindingFlags.NonPublic | BindingFlags.Instance)
                  .SetValue(runner, handler);

            A.CallTo(() => handler.Handle("input", context)).Returns("output");

            // act
            var output = runner.Handle("input", context);

            // assert
            A.CallTo(() => handler.Handle("input", context)).MustHaveHappened();
        }

        [Fact]
        public void Handle_UnwrapsAggregateExceptions_WhenFlagIsTrue()
        {
            // arrange
            var runner = new AsyncHandlerRunner<TestHandler, string, string>(true);
            var handler = A.Fake<TestHandler>();
            var context = A.Fake<ILambdaContext>();

            runner.GetType().GetField("Handler", BindingFlags.NonPublic | BindingFlags.Instance)
                  .SetValue(runner, handler);

            A.CallTo(() => handler.Handle(A<string>.Ignored, A<ILambdaContext>.Ignored))
                .ThrowsAsync(new ArgumentException());

            // act
            var ex = Record.Exception(() => runner.Handle(string.Empty, context));

            // assert
            ex.ShouldBeOfType<ArgumentException>();
        }

        [Fact]
        public void Handle_DoesNotUnwrapAggregateExceptions_WhenFlagIsFalse()
        {
            // arrange
            var runner = new AsyncHandlerRunner<TestHandler, string, string>(false);
            var handler = A.Fake<TestHandler>();
            var context = A.Fake<ILambdaContext>();

            runner.GetType().GetField("Handler", BindingFlags.NonPublic | BindingFlags.Instance)
                  .SetValue(runner, handler);

            A.CallTo(() => handler.Handle(A<string>.Ignored, A<ILambdaContext>.Ignored))
                .ThrowsAsync(new ArgumentException());

            // act
            var ex = Record.Exception(() => runner.Handle(string.Empty, context));

            // assert
            ex.ShouldBeOfType<AggregateException>();
        }
    }
}
