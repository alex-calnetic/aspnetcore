// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Testing;
using Microsoft.Extensions.Localization;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.Mvc.DataAnnotations.Internal
{
    public class StringLengthAttributeAdapterTest
    {
        [Fact]
        [ReplaceCulture]
        public void AddValidation_WithMaxLength_AddsAttributes_Localize()
        {
            // Arrange
            var provider = TestModelMetadataProvider.CreateDefaultProvider();
            var metadata = provider.GetMetadataForProperty(typeof(string), "Length");

            var attribute = new StringLengthAttribute(8);
            attribute.ErrorMessage = "Property must not be longer than '{1}' characters.";

            var expectedMessage = "Property must not be longer than '8' characters.";

            var stringLocalizer = new Mock<IStringLocalizer>();
            var expectedProperties = new object[] { "Length", 8, 0 };

            stringLocalizer.Setup(s => s[attribute.ErrorMessage, expectedProperties])
                .Returns(new LocalizedString(attribute.ErrorMessage, expectedMessage));

            var adapter = new StringLengthAttributeAdapter(attribute, stringLocalizer: stringLocalizer.Object);

            var actionContext = new ActionContext();
            var context = new ClientModelValidationContext(actionContext, metadata, provider, new AttributeDictionary());

            // Act
            adapter.AddValidation(context);

            // Assert
            Assert.Collection(
                context.Attributes,
                kvp => { Assert.Equal("data-val", kvp.Key); Assert.Equal("true", kvp.Value); },
                kvp => { Assert.Equal("data-val-length", kvp.Key); Assert.Equal(expectedMessage, kvp.Value); },
                kvp => { Assert.Equal("data-val-length-max", kvp.Key); Assert.Equal("8", kvp.Value); });

        }

        [Fact]
        [ReplaceCulture]
        public void AddValidation_WithMaxLength_AddsAttributes()
        {
            // Arrange
            var provider = TestModelMetadataProvider.CreateDefaultProvider();
            var metadata = provider.GetMetadataForProperty(typeof(string), "Length");

            var attribute = new StringLengthAttribute(8);
            var adapter = new StringLengthAttributeAdapter(attribute, stringLocalizer: null);

            var expectedMessage = attribute.FormatErrorMessage("Length");

            var actionContext = new ActionContext();
            var context = new ClientModelValidationContext(actionContext, metadata, provider, new AttributeDictionary());

            // Act
            adapter.AddValidation(context);

            // Assert
            Assert.Collection(
                context.Attributes,
                kvp => { Assert.Equal("data-val", kvp.Key); Assert.Equal("true", kvp.Value); },
                kvp => { Assert.Equal("data-val-length", kvp.Key); Assert.Equal(expectedMessage, kvp.Value); },
                kvp => { Assert.Equal("data-val-length-max", kvp.Key); Assert.Equal("8", kvp.Value); });
        }

        [Fact]
        [ReplaceCulture]
        public void AddValidation_WithMinAndMaxLength_AddsAttributes()
        {
            // Arrange
            var provider = TestModelMetadataProvider.CreateDefaultProvider();
            var metadata = provider.GetMetadataForProperty(typeof(string), "Length");

            var attribute = new StringLengthAttribute(10) { MinimumLength = 3 };
            var adapter = new StringLengthAttributeAdapter(attribute, stringLocalizer: null);

            var expectedMessage = attribute.FormatErrorMessage("Length");

            var actionContext = new ActionContext();
            var context = new ClientModelValidationContext(actionContext, metadata, provider, new AttributeDictionary());

            // Act
            adapter.AddValidation(context);

            // Assert
            Assert.Collection(
                context.Attributes,
                kvp => { Assert.Equal("data-val", kvp.Key); Assert.Equal("true", kvp.Value); },
                kvp => { Assert.Equal("data-val-length", kvp.Key); Assert.Equal(expectedMessage, kvp.Value); },
                kvp => { Assert.Equal("data-val-length-max", kvp.Key); Assert.Equal("10", kvp.Value); },
                kvp => { Assert.Equal("data-val-length-min", kvp.Key); Assert.Equal("3", kvp.Value); });
        }

        [Fact]
        [ReplaceCulture]
        public void AddValidation_WithMaxLength_AtIntMaxValue_AddsAttributes()
        {
            // Arrange
            var provider = TestModelMetadataProvider.CreateDefaultProvider();
            var metadata = provider.GetMetadataForProperty(typeof(string), "Length");

            var attribute = new StringLengthAttribute(int.MaxValue);
            var adapter = new StringLengthAttributeAdapter(attribute, stringLocalizer: null);

            var expectedMessage = attribute.FormatErrorMessage("Length");

            var actionContext = new ActionContext();
            var context = new ClientModelValidationContext(actionContext, metadata, provider, new AttributeDictionary());

            // Act
            adapter.AddValidation(context);

            // Assert
            Assert.Collection(
                context.Attributes,
                kvp => { Assert.Equal("data-val", kvp.Key); Assert.Equal("true", kvp.Value); },
                kvp => { Assert.Equal("data-val-length", kvp.Key); Assert.Equal(expectedMessage, kvp.Value); });
        }

        [Fact]
        [ReplaceCulture]
        public void AddValidation_DoesNotTrounceExistingAttributes()
        {
            // Arrange
            var provider = TestModelMetadataProvider.CreateDefaultProvider();
            var metadata = provider.GetMetadataForProperty(typeof(string), "Length");

            var attribute = new StringLengthAttribute(10) { MinimumLength = 3 };
            var adapter = new StringLengthAttributeAdapter(attribute, stringLocalizer: null);

            var expectedMessage = attribute.FormatErrorMessage("Length");

            var actionContext = new ActionContext();
            var context = new ClientModelValidationContext(actionContext, metadata, provider, new AttributeDictionary());

            context.Attributes.Add("data-val", "original");
            context.Attributes.Add("data-val-length", "original");
            context.Attributes.Add("data-val-length-max", "original");
            context.Attributes.Add("data-val-length-min", "original");

            // Act
            adapter.AddValidation(context);

            // Assert
            Assert.Collection(
                context.Attributes,
                kvp => { Assert.Equal("data-val", kvp.Key); Assert.Equal("original", kvp.Value); },
                kvp => { Assert.Equal("data-val-length", kvp.Key); Assert.Equal("original", kvp.Value); },
                kvp => { Assert.Equal("data-val-length-max", kvp.Key); Assert.Equal("original", kvp.Value); },
                kvp => { Assert.Equal("data-val-length-min", kvp.Key); Assert.Equal("original", kvp.Value); });
        }
    }
}
