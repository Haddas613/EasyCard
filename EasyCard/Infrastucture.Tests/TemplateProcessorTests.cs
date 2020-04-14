using Shared.Helpers.Templating;
using System;
using Xunit;

namespace Infrastucture.Tests
{
    public class TemplateProcessorTests
    {
        [Fact(DisplayName = "Substitute: With 1 argument")]
        public void Substitute1()
        {
            var text = "Hello {test}!";
            var sub = new TextSubstitution[] { new TextSubstitution { Substitution = "test", Value = "Customer" } };

            var result = TemplateProcessor.Substitute(text, sub);

            var expected = "Hello Customer!";

            Assert.Equal(expected, result);
        }

        [Fact(DisplayName = "Substitute: With few arguments")]
        public void Substitute2()
        {
            var text = "Hello {test}! {arg1} {arg2}";
            var sub = new TextSubstitution[] { 
                new TextSubstitution { Substitution = "test", Value = "Customer" },
                new TextSubstitution { Substitution = "arg1", Value = "some value 1" }, 
                new TextSubstitution { Substitution = "arg2", Value = "some value 2" }
            };

            var result = TemplateProcessor.Substitute(text, sub);

            var expected = "Hello Customer! some value 1 some value 2";

            Assert.Equal(expected, result);
        }

        [Fact(DisplayName = "Substitute: With extra unused arguments")]
        public void Substitute3()
        {
            var text = "Hello {test}! {arg1}";
            var sub = new TextSubstitution[] {
                new TextSubstitution { Substitution = "test", Value = "Customer" },
                new TextSubstitution { Substitution = "arg1", Value = "some value 1" },
                new TextSubstitution { Substitution = "arg2", Value = "some value 2" }
            };

            var result = TemplateProcessor.Substitute(text, sub);

            var expected = "Hello Customer! some value 1";

            Assert.Equal(expected, result);
        }

        [Fact(DisplayName = "Substitute: With non matching arguments")]
        public void Substitute4()
        {
            var text = "Hello {test}! {arg1} {arg3}";
            var sub = new TextSubstitution[] {
                new TextSubstitution { Substitution = "test", Value = "Customer" },
                new TextSubstitution { Substitution = "arg4", Value = "test4" },
                new TextSubstitution { Substitution = "arg5", Value = "test5" }
            };

            var result = TemplateProcessor.Substitute(text, sub);

            var expected = "Hello Customer! {arg1} {arg3}";

            Assert.Equal(expected, result);
        }
    }
}
