using System;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xunit;
using Xunit.Sdk;

namespace AwesomeAssertions.Json.Specs
{
    public sealed class JsonParserCustomizationTests : IDisposable
    {
        // restore JsonAssertionConfiguration.ParseFunction
        // after each test
        #region setup/teardown
        private readonly Func<string, JToken> prevParser;

        public JsonParserCustomizationTests()
        {
            prevParser = JsonAssertionConfiguration.ParseFunction;
        }

        public void Dispose()
        {
            JsonAssertionConfiguration.ParseFunction = prevParser;
            GC.SuppressFinalize(this);
        }
        #endregion

        #region Test statements with results depending on 'ParseFunction'
        private void TestSubject_BeEquivalentTo()
        {
            var date = new JValue("2000-01-01T00:00:00Z");

            date.Should().BeEquivalentTo("\"2000-01-01T00:00:00Z\"");
        }

        private void TestSubject_NotBeEquivalentTo()
        {
            var date = new JValue("2000-01-01T00:00:00Z");

            date.Should().NotBeEquivalentTo("\"2000-01-01T00:00:00Z\"");
        }

        private void TestSubject_ContainSubtree()
        {
            var date = new JValue("2000-01-01T00:00:00Z");

            date.Should().ContainSubtree("\"2000-01-01T00:00:00Z\"");
        }

        private void TestSubject_BeValidJson()
        {
            var str = @"
            {
                repeating_key: {},
                repeating_key: {}
            }";

            str.Should().BeValidJson();
        }
        #endregion

        // example of insufficient default behavior
        [Fact]
        public void BeEquivalentTo_of_string_containing_date__with_DEFAULT_parser__fails()
        {
            // assert with no action (default behavior)
            new Action(TestSubject_BeEquivalentTo)
                .Should()
                .Throw<XunitException>(because: "default parser creates a DateTime when meets a date-like string")
                .Which
                .Message.Should().Contain("JSON document has a string instead of a date at $.");
        }

        [Fact]
        public void BeEquivalentTo_of_string_containing_date__with_custom_parser__should_succeed()
        {
            // act
            JsonAssertionConfiguration.ParseFunction = CustomParser;

            // assert
            new Action(TestSubject_BeEquivalentTo)
                .Should()
                .NotThrow();
        }

        [Fact]
        public void ContainSubtree_of_string_containing_date__with_custom_parser__should_succeed()
        {
            // act
            JsonAssertionConfiguration.ParseFunction = CustomParser;

            // assert
            new Action(TestSubject_ContainSubtree)
                .Should()
                .NotThrow();
        }

        // example of insufficient default behavior
        [Fact]
        public void NotBeEquivalentTo_of_string_containing_date__with_DEFAULT_parser__is_false_positive()
        {
            // assert with no action (default behavior)
            new Action(TestSubject_NotBeEquivalentTo)
                .Should()
                .NotThrow();
        }

        [Fact]
        public void NotBeEquivalentTo_of_string_containing_date__with_custom_parser__should_fail()
        {
            // act
            JsonAssertionConfiguration.ParseFunction = CustomParser;

            // assert
            new Action(TestSubject_NotBeEquivalentTo)
                .Should()
                .Throw<XunitException>()
                .Which
                .Message.Should().Contain("Expected JSON document not to be equivalent to");
        }

        // example of insufficient default behavior
        [Fact]
        public void BeValidJson_of_string_containing_repeating_keys__with_DEFAULT_parser__is_false_positive()
        {
            // assert with no action (default behavior)
            new Action(TestSubject_BeValidJson)
                .Should()
                .NotThrow();
        }

        [Fact]
        public void BeValidJson_of_string_containing_repeating_keys__with_custom_parser__should_fail()
        {
            // act
            JsonAssertionConfiguration.ParseFunction = CustomParser;

            // assert
            new Action(TestSubject_BeValidJson)
                .Should()
                .Throw<XunitException>()
                .Which
                .Message.Should().Contain("the name 'repeating_key' already exists");
        }

        private static JToken CustomParser(string json)
        {
            using var jsonReader = new JsonTextReader(new StringReader(json));

            jsonReader.DateParseHandling = DateParseHandling.None;
            var settings = new JsonLoadSettings
            {
                DuplicatePropertyNameHandling = DuplicatePropertyNameHandling.Error,
                LineInfoHandling = LineInfoHandling.Ignore,
                CommentHandling = CommentHandling.Ignore
            };

            var result = JToken.Load(jsonReader, settings);
            while (jsonReader.Read())
            {
            }

            return result;
        }

        [Fact]
        public void Setting_null_ParseFunction__should_throw()
        {
            Action action = () => JsonAssertionConfiguration.ParseFunction = null!;

            action.Should()
                .Throw<ArgumentNullException>();
        }
    }
}
