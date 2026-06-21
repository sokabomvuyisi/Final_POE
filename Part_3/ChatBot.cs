using System;
using System.Collections.Generic;
using System.Text;

namespace CybersecurityChatbot
{
    /// <summary>
    /// Central chatbot class — Part 3 enhanced version.
    /// Integrates: keyword recognition, sentiment detection, memory (Parts 1 & 2),
    /// PLUS Task Assistant (JSON DB), Mini-Game Quiz, NLP Simulation, Activity Log.
    /// MainWindow calls only ProcessInput() and GetGreeting().
    /// </summary>
    public class ChatBot
    {
        // ── Part 1 & 2 (unchanged) ─────────────────────────────────────────────
        private readonly KeywordResponder _keywords;
        private readonly SentimentDetector _sentiment;
        private readonly MemoryStore _memory;
        private readonly Random _random = new();
        private bool _awaitingName = true;
        private string _lastTopic = string.Empty;

        // ── Part 3 — new components ────────────────────────────────────────────
        private readonly TaskRepository _taskRepo;
        private readonly QuizEngine _quiz;
        private readonly NlpProcessor _nlp;
        private readonly ActivityLogger _activityLog;

        // Task flow state
        private bool _awaitingReminderYesNo = false;
        private string _pendingTaskTitle = string.Empty;
        private bool _awaitingReminderDetail = false;
        private int _pendingTaskIndexForReminder = -1;

        // Follow-up phrases (carried over from Part 2)
        private readonly List<string> _followUpPhrases = new()
        {
            "tell me more", "explain more", "give me another tip",
            "more info", "elaborate", "continue", "go on", "more details",
            "anything else", "what else"
        };

        // Fallback responses
        private readonly List<string> _fallbackResponses = new()
        {
            "I'm not sure I understand. Could you rephrase?",
            "Hmm, I didn't quite catch that. Try asking about passwords, phishing, malware, or scams!",
            "I'm still learning! Could you ask me something about a cybersecurity topic?",
            "That's outside my expertise right now. Try: 'Tell me about phishing', 'start quiz', or 'show tasks'."
        };

        public ChatBot()
        {
            _keywords    = new KeywordResponder();
            _sentiment   = new SentimentDetector();
            _memory      = new MemoryStore();
            _taskRepo    = new TaskRepository();
            _quiz        = new QuizEngine();
            _nlp         = new NlpProcessor();
            _activityLog = new ActivityLogger();
        }

        // ── Greeting ───────────────────────────────────────────────────────────

        public string GetGreeting() =>
            "Hello! I'm CyberBot, your personal cybersecurity assistant.\n\nBefore we begin, what's your name?";

        // ── Main router ────────────────────────────────────────────────────────

        /// <summary>
        /// All user input flows through here.
        /// Priority: name → quiz (if active) → NLP intents → follow-up →
        ///           sentiment → keyword → special phrases → fallback.
        /// </summary>
        public string ProcessInput(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return "Please type a message so I can help you! :)";

            input = input.Trim();
            string lower = input.ToLower();

            // ── Step 1: Capture name ───────────────────────────────────────────
            if (_awaitingName)
            {
                string name = ExtractName(input);
                _memory.UserName = name;
                _memory.Store("name", name);
                _awaitingName = false;
                _activityLog.LogNlpAction($"Session started for user: {name}");

                return $"Nice to meet you, {name}! 👋\n\n" +
                       $"I'm here to help you stay safe in the digital world.\n\n" +
                       $"Here's what I can do:\n" +
                       $"• 💬 Answer questions about cybersecurity topics\n" +
                       $"• ✅ Manage your cybersecurity tasks — try: 'add task enable 2FA'\n" +
                       $"• 🎮 Quiz you on cybersecurity — try: 'start quiz'\n" +
                       $"• 📋 Show your activity log — try: 'show activity log'\n\n" +
                       $"What would you like to know today, {name}?";
            }

            // ── Step 2: Quiz mode intercept ────────────────────────────────────
            if (_quiz.State == QuizState.InProgress)
            {
                // Allow exit from quiz
                if (lower.Contains("quit quiz") || lower.Contains("stop quiz") || lower.Contains("exit quiz"))
                {
                    _quiz.Quit();
                    _activityLog.LogNlpAction("User exited quiz early.");
                    return "Quiz exited. You can type 'start quiz' to try again anytime!";
                }
                // Route everything else as a quiz answer
                return ProcessQuizAnswer(input);
            }

            // ── Step 3: Reminder yes/no flow ───────────────────────────────────
            if (_awaitingReminderYesNo)
            {
                return HandleReminderYesNo(lower);
            }

            if (_awaitingReminderDetail)
            {
                return HandleReminderDetail(input);
            }

            // ── Step 4: Detect NLP intent ──────────────────────────────────────
            string? intent = _nlp.DetectIntent(lower);
            if (intent != null)
            {
                string? nlpResult = RouteIntent(intent, input, lower);
                if (nlpResult != null) return nlpResult;
            }

            // ── Step 5: Follow-up phrases ──────────────────────────────────────
            if (IsFollowUp(lower))
            {
                if (!string.IsNullOrWhiteSpace(_lastTopic))
                {
                    string? followUpResponse = _keywords.GetResponseForKeyword(_lastTopic);
                    if (followUpResponse != null)
                    {
                        string opener = _memory.HasName() ? $"{_memory.UserName}, here's more on {_lastTopic}: " : $"Here's more on {_lastTopic}: ";
                        return opener + "\n\n" + followUpResponse;
                    }
                }
                return "I don't have a previous topic to continue from. What cybersecurity topic would you like to explore?";
            }

            // ── Step 6: Favourite topic storage ───────────────────────────────
            string? detectedTopic = TryExtractFavouriteTopic(lower);
            if (detectedTopic != null && !_memory.HasFavouriteTopic())
            {
                _memory.FavouriteTopic = detectedTopic;
                _memory.Store("favourite_topic", detectedTopic);
                _lastTopic = detectedTopic;
                string? topicResponse = _keywords.GetResponseForKeyword(detectedTopic);
                string topicReply = topicResponse != null ? "\n\n" + topicResponse : string.Empty;
                return $"Great! I'll remember that you're interested in {detectedTopic}. It's a crucial part of staying safe online.{topicReply}";
            }

            // ── Step 7: Sentiment detection ────────────────────────────────────
            Sentiment detectedSentiment = _sentiment.Detect(lower);
            string sentimentOpener = _sentiment.GetSentimentResponse(detectedSentiment);

            // ── Step 8: Keyword recognition ────────────────────────────────────
            string? keywordResponse = _keywords.GetResponse(lower);
            string? matchedKeyword = _keywords.GetMatchedKeyword(lower);

            if (keywordResponse != null)
            {
                _lastTopic = matchedKeyword ?? string.Empty;
                if (!string.IsNullOrWhiteSpace(matchedKeyword))
                    _activityLog.LogKeywordDetected(matchedKeyword);

                string personalisedOpener = BuildPersonalisedOpener(matchedKeyword);
                string fullResponse = sentimentOpener + personalisedOpener + keywordResponse;
                fullResponse += "\n\nType 'tell me more' for another tip on this topic.";
                return fullResponse;
            }

            // ── Step 9: Special command phrases ───────────────────────────────
            if (lower.Contains("how are you"))
            {
                string name = _memory.HasName() ? $", {_memory.UserName}" : "";
                return $"I'm running at full capacity{name}! Ready to help you stay safe online. What cybersecurity topic can I help with?";
            }

            if (lower.Contains("what can you do") || lower.Contains("purpose"))
            {
                return BuildHelpMessage();
            }

            if (lower.Contains("who are you") || lower.Contains("your name"))
            {
                return "I'm CyberBot — your cybersecurity awareness assistant! I help you understand online threats and stay safe.";
            }

            if (lower.Contains("bye") || lower.Contains("goodbye") || lower.Contains("exit") || lower.Contains("thank you"))
            {
                string name = _memory.HasName() ? $", {_memory.UserName}" : "";
                return $"Stay safe online{name}! Remember: Think before you click. Goodbye! 👋";
            }

            // ── Step 10: Fallback ──────────────────────────────────────────────
            return sentimentOpener + _fallbackResponses[_random.Next(_fallbackResponses.Count)];
        }

        // ══════════════════════════════════════════════════════════════════════
        //  INTENT ROUTING (Task 3 — NLP)
        // ══════════════════════════════════════════════════════════════════════

        private string? RouteIntent(string intent, string original, string lower)
        {
            switch (intent)
            {
                case "ADD_TASK":
                    return HandleAddTask(original);

                case "VIEW_TASKS":
                    return HandleViewTasks();

                case "COMPLETE_TASK":
                    return HandleCompleteTask(lower);

                case "DELETE_TASK":
                    return HandleDeleteTask(lower);

                case "SET_REMINDER":
                    return HandleSetReminderIntent(lower);

                case "START_QUIZ":
                    _activityLog.LogQuizStarted();
                    return _quiz.Start();

                case "QUIT_QUIZ":
                    _quiz.Quit();
                    return "Quiz stopped. Type 'start quiz' whenever you're ready to try again!";

                case "SHOW_LOG":
                    return HandleShowLog(false);

                case "SHOW_FULL_LOG":
                    return HandleShowLog(true);

                case "HELP":
                    return BuildHelpMessage();

                default:
                    return null;
            }
        }

        // ══════════════════════════════════════════════════════════════════════
        //  TASK 1 — TASK ASSISTANT
        // ══════════════════════════════════════════════════════════════════════

        private string HandleAddTask(string input)
        {
            string title = _nlp.ExtractTaskTitle(input);

            // Auto-generate a cybersecurity-relevant description
            string description = GenerateTaskDescription(title);

            // Check for reminder in same message
            var (reminderText, reminderDate) = _nlp.ExtractReminder(input);
            bool hasReminder = !string.IsNullOrWhiteSpace(reminderText);

            var task = _taskRepo.Add(title, description, hasReminder, reminderDate, reminderText);
            _activityLog.LogTaskAdded(title, reminderText);

            string response = $"✅ Task added!\n\n" +
                              $"📌 Title: {task.Title}\n" +
                              $"📝 Description: {task.Description}\n";

            if (hasReminder)
            {
                response += $"⏰ Reminder: {reminderText}\n\n";
                response += "Your task and reminder have been saved!";
            }
            else
            {
                // Ask if they want a reminder
                _pendingTaskTitle = title;
                _pendingTaskIndexForReminder = _taskRepo.GetAll().Count; // 1-based
                _awaitingReminderYesNo = true;
                response += "\nWould you like to set a reminder for this task? (Yes / No)";
            }

            return response;
        }

        private string HandleViewTasks()
        {
            var tasks = _taskRepo.GetAll();
            if (tasks.Count == 0)
                return "You have no tasks yet! Try: 'Add task Enable two-factor authentication'";

            var sb = new StringBuilder();
            sb.AppendLine($"📋 Your Cybersecurity Tasks ({tasks.Count} total):\n");

            for (int i = 0; i < tasks.Count; i++)
            {
                var t = tasks[i];
                string status = t.IsCompleted ? "✅" : "⏳";
                sb.AppendLine($"{status} {i + 1}. {t.Title}");
                sb.AppendLine($"     {t.Description}");
                if (t.HasReminder && !string.IsNullOrWhiteSpace(t.ReminderText))
                    sb.AppendLine($"     ⏰ Reminder: {t.ReminderText}");
                sb.AppendLine();
            }

            sb.AppendLine("Commands:");
            sb.AppendLine("• 'Complete task [number]' — mark as done");
            sb.AppendLine("• 'Delete task [number]' — remove a task");

            return sb.ToString().TrimEnd();
        }

        private string HandleCompleteTask(string lower)
        {
            int num = _nlp.ExtractTaskNumber(lower);
            var tasks = _taskRepo.GetAll();

            if (num == -1)
            {
                // No number given — list tasks and ask
                if (tasks.Count == 0) return "You have no tasks to complete. Add one with 'add task [title]'.";
                return "Which task would you like to mark as complete? Type the task number:\n\n" + ListTasksSummary();
            }

            if (!_taskRepo.MarkComplete(num))
                return $"I couldn't find task {num}. Type 'show tasks' to see your list.";

            string taskTitle = tasks[num - 1].Title;
            _activityLog.LogTaskCompleted(taskTitle);
            return $"✅ Great job! Task {num} '{taskTitle}' has been marked as complete!\n\nKeep building those cybersecurity habits!";
        }

        private string HandleDeleteTask(string lower)
        {
            int num = _nlp.ExtractTaskNumber(lower);
            var tasks = _taskRepo.GetAll();

            if (num == -1)
            {
                if (tasks.Count == 0) return "You have no tasks to delete.";
                return "Which task number would you like to delete?\n\n" + ListTasksSummary();
            }

            if (num < 1 || num > tasks.Count)
                return $"Task {num} doesn't exist. Type 'show tasks' to see your current list.";

            string taskTitle = tasks[num - 1].Title;
            _taskRepo.Delete(num);
            _activityLog.LogTaskDeleted(taskTitle);
            return $"🗑️ Task '{taskTitle}' has been deleted.";
        }

        private string HandleSetReminderIntent(string lower)
        {
            var tasks = _taskRepo.GetAll();
            if (tasks.Count == 0)
                return "You have no tasks yet. Add a task first with 'add task [title]'.";

            int num = _nlp.ExtractTaskNumber(lower);
            if (num == -1)
            {
                _awaitingReminderDetail = true;
                _pendingTaskIndexForReminder = -1;
                return "Which task number would you like to set a reminder for?\n\n" + ListTasksSummary();
            }

            var (text, date) = _nlp.ExtractReminder(lower);
            if (string.IsNullOrWhiteSpace(text))
            {
                _awaitingReminderDetail = true;
                _pendingTaskIndexForReminder = num;
                return $"When would you like to be reminded about task {num}?\n(e.g. 'in 3 days', 'tomorrow', 'next week')";
            }

            if (!_taskRepo.SetReminder(num, text, date))
                return $"Task {num} not found. Type 'show tasks' to see your list.";

            string taskTitle = tasks[num - 1].Title;
            _activityLog.LogReminderSet(taskTitle, text);
            return $"⏰ Reminder set for '{taskTitle}': {text}.";
        }

        // ── Reminder conversation flow ──────────────────────────────────────

        private string HandleReminderYesNo(string lower)
        {
            _awaitingReminderYesNo = false;

            if (lower.Contains("yes") || lower.Contains("y") || lower.Contains("sure") || lower.Contains("ok"))
            {
                _awaitingReminderDetail = true;
                return "When would you like to be reminded?\n(e.g. 'in 3 days', 'tomorrow', 'next week', or a date like '20/07')";
            }
            else
            {
                _pendingTaskTitle = string.Empty;
                _pendingTaskIndexForReminder = -1;
                return $"No problem! Your task has been saved without a reminder.\nType 'show tasks' to see all your tasks.";
            }
        }

        private string HandleReminderDetail(string input)
        {
            _awaitingReminderDetail = false;

            var (text, date) = _nlp.ExtractReminder(input);

            // If no timeframe detected, use the raw input as the reminder text
            if (string.IsNullOrWhiteSpace(text))
                text = input.Trim();

            var tasks = _taskRepo.GetAll();
            int taskIndex = _pendingTaskIndexForReminder > 0
                ? _pendingTaskIndexForReminder
                : tasks.Count; // last added

            if (taskIndex < 1 || taskIndex > tasks.Count)
                return "I couldn't find the task. Type 'show tasks' to see your list.";

            string taskTitle = tasks[taskIndex - 1].Title;
            _taskRepo.SetReminder(taskIndex, text, date);
            _activityLog.LogReminderSet(taskTitle, text);

            _pendingTaskTitle = string.Empty;
            _pendingTaskIndexForReminder = -1;

            return $"⏰ Got it! I'll remind you {text} about '{taskTitle}'.";
        }

        // ══════════════════════════════════════════════════════════════════════
        //  TASK 2 — QUIZ
        // ══════════════════════════════════════════════════════════════════════

        private string ProcessQuizAnswer(string input)
        {
            string result = _quiz.ProcessAnswer(input);

            if (_quiz.State == QuizState.Finished)
                _activityLog.LogQuizCompleted(_quiz.Score, _quiz.Total);

            return result;
        }

        // ══════════════════════════════════════════════════════════════════════
        //  TASK 4 — ACTIVITY LOG
        // ══════════════════════════════════════════════════════════════════════

        private string HandleShowLog(bool showAll)
        {
            var entries = showAll ? _activityLog.GetAll() : _activityLog.GetRecent(10);

            if (entries.Count == 0)
                return "No activity has been recorded yet. Start chatting, add tasks, or take a quiz!";

            var sb = new StringBuilder();
            sb.AppendLine(showAll
                ? $"📋 Full Activity Log ({_activityLog.TotalCount} actions):\n"
                : $"📋 Recent Activity (last {entries.Count} actions):\n");

            for (int i = 0; i < entries.Count; i++)
            {
                sb.AppendLine($"  {i + 1}. {entries[i]}");
            }

            if (!showAll && _activityLog.TotalCount > 10)
                sb.AppendLine($"\n... and {_activityLog.TotalCount - 10} more. Type 'show full log' to see all.");

            return sb.ToString().TrimEnd();
        }

        // ══════════════════════════════════════════════════════════════════════
        //  HELPERS
        // ══════════════════════════════════════════════════════════════════════

        private string BuildHelpMessage()
        {
            var kws = _keywords.GetAllKeywords();
            return "🤖 I'm CyberBot — here's everything I can help with:\n\n" +
                   "💬 CYBERSECURITY TOPICS:\n" +
                   $"   {string.Join(", ", kws)}\n\n" +
                   "✅ TASK ASSISTANT:\n" +
                   "   • 'Add task Enable 2FA'\n" +
                   "   • 'Show tasks' / 'View my tasks'\n" +
                   "   • 'Complete task 1'\n" +
                   "   • 'Delete task 2'\n" +
                   "   • 'Remind me to update my password tomorrow'\n\n" +
                   "🎮 QUIZ:\n" +
                   "   • 'Start quiz' / 'Quiz me'\n\n" +
                   "📋 ACTIVITY LOG:\n" +
                   "   • 'Show activity log'\n" +
                   "   • 'Show full log'\n\n" +
                   "💡 TIP: You can phrase requests naturally — try:\n" +
                   "   'Can you remind me to check my privacy settings?'";
        }

        private string ListTasksSummary()
        {
            var tasks = _taskRepo.GetAll();
            var sb = new StringBuilder();
            for (int i = 0; i < tasks.Count; i++)
            {
                string status = tasks[i].IsCompleted ? "✅" : "⏳";
                sb.AppendLine($"{status} {i + 1}. {tasks[i].Title}");
            }
            return sb.ToString().TrimEnd();
        }

        private string GenerateTaskDescription(string title)
        {
            string lower = title.ToLower();

            if (lower.Contains("2fa") || lower.Contains("two-factor") || lower.Contains("two factor") || lower.Contains("authentication"))
                return "Enable two-factor authentication on your accounts for an extra layer of security.";
            if (lower.Contains("password"))
                return "Review and update your passwords using a password manager to keep accounts secure.";
            if (lower.Contains("privacy") || lower.Contains("settings"))
                return "Review your privacy settings to control what data is shared and with whom.";
            if (lower.Contains("antivirus") || lower.Contains("anti-virus"))
                return "Install or update your antivirus software to protect against malware threats.";
            if (lower.Contains("backup"))
                return "Back up your important files to an external drive or secure cloud storage.";
            if (lower.Contains("update") || lower.Contains("patch"))
                return "Apply the latest security updates and patches to your software and operating system.";
            if (lower.Contains("vpn"))
                return "Set up a VPN to encrypt your internet connection, especially on public Wi-Fi.";
            if (lower.Contains("firewall"))
                return "Enable and configure your firewall to block unauthorised network access.";
            if (lower.Contains("phishing") || lower.Contains("email"))
                return "Learn to identify phishing emails and enable email spam filters on your accounts.";

            // Generic cybersecurity description
            return $"Complete the cybersecurity task: {title}. Stay proactive about your digital safety!";
        }

        // ── Carried over from Part 1 & 2 ──────────────────────────────────────

        private static string ExtractName(string input)
        {
            string lower = input.ToLower();
            foreach (string phrase in new[] { "my name is ", "i am ", "i'm ", "call me ", "it's ", "its " })
            {
                if (lower.Contains(phrase))
                {
                    int idx = lower.IndexOf(phrase) + phrase.Length;
                    string extracted = input.Substring(idx).Trim().Split(' ')[0];
                    return CapitaliseFirst(extracted.TrimEnd('.', ',', '!', '?'));
                }
            }
            return CapitaliseFirst(input.Split(' ')[0].TrimEnd('.', ',', '!', '?'));
        }

        private bool IsFollowUp(string lowerInput)
        {
            foreach (string phrase in _followUpPhrases)
                if (lowerInput.Contains(phrase)) return true;
            return false;
        }

        private string? TryExtractFavouriteTopic(string lowerInput)
        {
            var patterns = new[] { "interested in ", "i like ", "i love ", "passionate about ", "i hate " };
            foreach (string pattern in patterns)
            {
                if (lowerInput.Contains(pattern))
                {
                    int idx = lowerInput.IndexOf(pattern) + pattern.Length;
                    string remainder = lowerInput.Substring(idx).Split(' ')[0].TrimEnd('.', ',', '!', '?');
                    if (_keywords.GetAllKeywords().Contains(remainder))
                        return remainder;
                }
            }
            return null;
        }

        private string BuildPersonalisedOpener(string? keyword)
        {
            if (_memory.HasFavouriteTopic() && keyword == _memory.FavouriteTopic)
                return $"As someone interested in {_memory.FavouriteTopic}, here's something especially relevant: ";
            if (_memory.HasName())
                return $"{_memory.UserName}, ";
            return string.Empty;
        }

        private static string CapitaliseFirst(string s)
        {
            if (string.IsNullOrEmpty(s)) return s;
            return char.ToUpper(s[0]) + s.Substring(1).ToLower();
        }
    }
}
