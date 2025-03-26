﻿using MessageBus.IntegrationEventLog.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MessageBus.IntegrationEventLog.EF.Models;

public class FailedMessageChain : IFailedMessageChain<FailedMessage>
{
    [Key]
    public int Id { get; set; }
    [Required]
    public DateTime CreationTime { get; set; } = DateTime.UtcNow;
    [Required]
    public bool ShouldRepublish { get; set; } = false;
    [Required]
    public required string EntityId { get; set; }
    [NotMapped]
    public ICollection<FailedMessage>? FailedMessages { get; set; }
}
