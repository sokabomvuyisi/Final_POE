using System;

namespace CybersecurityChatbot
{
    /// <summary>
    /// Represents a single cybersecurity task stored in the JSON database.
    /// </summary>
    public class TaskItem
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool IsCompleted { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Optional reminder fields
        public bool HasReminder { get; set; } = false;
        public DateTime? ReminderDate { get; set; } = null;
        public string ReminderText { get; set; } = string.Empty;  // e.g. "in 3 days"
    }
}
