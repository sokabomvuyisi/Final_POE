using System;
using System.Collections.Generic;
using System.Linq;

namespace CybersecurityChatbot
{
    /// <summary>
    /// Records timestamped actions taken by the chatbot during the session.
    /// Provides the Activity Log feature (Task 4).
    /// </summary>
    public class ActivityLogger
    {
        private readonly List<ActivityEntry> _log = new();

        // ── Logging methods ────────────────────────────────────────────────────

        public void LogTaskAdded(string taskTitle, string reminderText = "")
        {
            string detail = string.IsNullOrWhiteSpace(reminderText)
                ? $"Task added: '{taskTitle}' (no reminder set)"
                : $"Task added: '{taskTitle}' (Reminder: {reminderText})";
            Add(detail);
        }

        public void LogTaskCompleted(string taskTitle) =>
            Add($"Task marked as completed: '{taskTitle}'");

        public void LogTaskDeleted(string taskTitle) =>
            Add($"Task deleted: '{taskTitle}'");

        public void LogReminderSet(string taskTitle, string reminderText) =>
            Add($"Reminder set for '{taskTitle}': {reminderText}");

        public void LogQuizStarted() =>
            Add("Quiz started.");

        public void LogQuizCompleted(int score, int total) =>
            Add($"Quiz completed — Score: {score}/{total}");

        public void LogNlpAction(string description) =>
            Add($"NLP action: {description}");

        public void LogKeywordDetected(string keyword) =>
            Add($"Keyword detected and responded to: '{keyword}'");

        // ── Retrieval ──────────────────────────────────────────────────────────

        /// <summary>Returns the last N entries (default 10) for display.</summary>
        public List<ActivityEntry> GetRecent(int count = 10) =>
            _log.TakeLast(count).ToList();

        /// <summary>Returns the full log.</summary>
        public List<ActivityEntry> GetAll() => _log.ToList();

        public int TotalCount => _log.Count;

        // ── Private ────────────────────────────────────────────────────────────

        private void Add(string description)
        {
            _log.Add(new ActivityEntry
            {
                Timestamp = DateTime.Now,
                Description = description
            });
        }
    }

    public class ActivityEntry
    {
        public DateTime Timestamp { get; set; }
        public string Description { get; set; } = string.Empty;

        public override string ToString() =>
            $"[{Timestamp:HH:mm:ss}] {Description}";
    }
}
