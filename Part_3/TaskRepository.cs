using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace CybersecurityChatbot
{
    /// <summary>
    /// Handles loading and saving cybersecurity tasks to/from a local JSON file.
    /// This acts as the database layer (replaces MySQL as per student request).
    /// </summary>
    public class TaskRepository
    {
        private readonly string _filePath;
        private List<TaskItem> _tasks;

        public TaskRepository()
        {
            // Store tasks.json next to the running exe
            _filePath = Path.Combine(AppContext.BaseDirectory, "tasks.json");
            _tasks = Load();
        }

        // ── CRUD ───────────────────────────────────────────────────────────────

        /// <summary>Returns all tasks (live list).</summary>
        public List<TaskItem> GetAll() => _tasks;

        /// <summary>Adds a new task and saves to disk.</summary>
        public TaskItem Add(string title, string description, bool hasReminder = false,
                            DateTime? reminderDate = null, string reminderText = "")
        {
            var task = new TaskItem
            {
                Title = title,
                Description = description,
                HasReminder = hasReminder,
                ReminderDate = reminderDate,
                ReminderText = reminderText
            };
            _tasks.Add(task);
            Save();
            return task;
        }

        /// <summary>Marks a task as completed by index (1-based for user display).</summary>
        public bool MarkComplete(int oneBasedIndex)
        {
            int idx = oneBasedIndex - 1;
            if (idx < 0 || idx >= _tasks.Count) return false;
            _tasks[idx].IsCompleted = true;
            Save();
            return true;
        }

        /// <summary>Deletes a task by index (1-based).</summary>
        public bool Delete(int oneBasedIndex)
        {
            int idx = oneBasedIndex - 1;
            if (idx < 0 || idx >= _tasks.Count) return false;
            _tasks.RemoveAt(idx);
            Save();
            return true;
        }

        /// <summary>Sets a reminder on an existing task by index (1-based).</summary>
        public bool SetReminder(int oneBasedIndex, string reminderText, DateTime? reminderDate = null)
        {
            int idx = oneBasedIndex - 1;
            if (idx < 0 || idx >= _tasks.Count) return false;
            _tasks[idx].HasReminder = true;
            _tasks[idx].ReminderText = reminderText;
            _tasks[idx].ReminderDate = reminderDate;
            Save();
            return true;
        }

        // ── Persistence ────────────────────────────────────────────────────────

        private List<TaskItem> Load()
        {
            try
            {
                if (!File.Exists(_filePath)) return new List<TaskItem>();
                string json = File.ReadAllText(_filePath);
                return JsonSerializer.Deserialize<List<TaskItem>>(json) ?? new List<TaskItem>();
            }
            catch
            {
                return new List<TaskItem>();
            }
        }

        private void Save()
        {
            try
            {
                var options = new JsonSerializerOptions { WriteIndented = true };
                string json = JsonSerializer.Serialize(_tasks, options);
                File.WriteAllText(_filePath, json);
            }
            catch
            {
                // Silently fail — app continues even if disk write fails
            }
        }
    }
}
