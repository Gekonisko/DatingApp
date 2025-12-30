using API.Entities;
using API.Extensions;
using FluentAssertions;

namespace Api.Tests.UnitTests.Extensions;

public class MessageExtensionsTests
{
    [Fact]
    public void ToDto_Should_Map_All_Properties()
    {
        // Arrange
        var sender = new Member
        {
            Id = "sender-id",
            DisplayName = "Sender",
            ImageUrl = "sender.jpg",
            Gender = "male",
            City = "City",
            Country = "Country"
        };

        var recipient = new Member
        {
            Id = "recipient-id",
            DisplayName = "Recipient",
            ImageUrl = "recipient.jpg",
            Gender = "male",
            City = "City",
            Country = "Country"
        };

        var message = new Message
        {
            Id = "msg-id",
            SenderId = sender.Id,
            Sender = sender,
            RecipientId = recipient.Id,
            Recipient = recipient,
            Content = "Hello",
            DateRead = DateTime.UtcNow,
            MessageSent = DateTime.UtcNow
        };

        // Act
        var dto = message.ToDto();

        // Assert
        dto.Id.Should().Be(message.Id);
        dto.SenderId.Should().Be(sender.Id);
        dto.SenderDisplayName.Should().Be(sender.DisplayName);
        dto.SenderImageUrl.Should().Be(sender.ImageUrl);
        dto.RecipientId.Should().Be(recipient.Id);
        dto.RecipientDisplayName.Should().Be(recipient.DisplayName);
        dto.RecipientImageUrl.Should().Be(recipient.ImageUrl);
        dto.Content.Should().Be(message.Content);
        dto.DateRead.Should().Be(message.DateRead);
        dto.MessageSent.Should().Be(message.MessageSent);
    }
}