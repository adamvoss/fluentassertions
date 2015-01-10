using System;
using System.Linq.Expressions;
using FluentAssertions.Common;
using FluentAssertions.Execution;

namespace FluentAssertions.Equivalency
{
    /// <summary>
    /// An equivalency step used to support the Fluent API.
    /// It takes an Action against an IAssertionContext to determine success and allows filtering based on an ISubjectInfo
    /// </summary>
    internal class AssertionContextEquivalencyStep<TSubject> : IEquivalencyStep
    {
        private readonly Func<ISubjectInfo, bool> predicate;
        private readonly Action<IAssertionContext<TSubject>> action;
        private readonly string description;

        public AssertionContextEquivalencyStep(Expression<Func<ISubjectInfo, bool>> predicate, Action<IAssertionContext<TSubject>> action)
        {
            this.predicate = predicate.Compile();
            this.action = action;
            description = "Invoke Action<" + typeof(TSubject).Name + "> when " + predicate.Body;
        }

        public bool CanHandle(IEquivalencyValidationContext context, IEquivalencyAssertionOptions config)
        {
            return ((context.SelectedMemberInfo != null) && predicate(context));
        }

        public bool Handle(IEquivalencyValidationContext context, IEquivalencyValidator parent, IEquivalencyAssertionOptions config)
        {
            bool expectationisNull = ReferenceEquals(context.Expectation, null);

            bool succeeded =
                AssertionScope.Current
                    .ForCondition(expectationisNull || context.Expectation.GetType().IsSameOrInherits(typeof(TSubject)))
                    .FailWith("Expected " + context.SelectedMemberDescription + " to be a {0}{reason}, but found a {1}",
                        !expectationisNull ? context.Expectation.GetType() : null, context.SelectedMemberInfo.MemberType);

            if (succeeded)
            {
                action(AssertionContext<TSubject>.CreateFromEquivalencyValidationContext(context));
            }

            return true;
        }

        public override string ToString()
        {
            return description;
        }
    }
}