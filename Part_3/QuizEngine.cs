using System;
using System.Collections.Generic;
using System.Linq;

namespace CybersecurityChatbot
{
    public enum QuizState { Idle, InProgress, Finished }

    /// <summary>
    /// Manages the cybersecurity mini-game quiz (Task 2).
    /// 12 questions — mix of multiple choice and true/false.
    /// One question at a time, immediate feedback, final score.
    /// </summary>
    public class QuizEngine
    {
        private readonly List<QuizQuestion> _questions;
        private List<QuizQuestion> _shuffled = new();
        private int _currentIndex = 0;
        private int _score = 0;

        // Tracks whether user typed something non-quiz last turn
        // so we can warn them once before pausing
        private bool _awaitingResumeConfirm = false;

        public QuizState State { get; private set; } = QuizState.Idle;
        public int Score => _score;
        public int Total => _shuffled.Count;
        public int CurrentQuestionNumber => _currentIndex + 1;

        // ── Valid answer tokens ────────────────────────────────────────────────
        // Anything that looks like a quiz answer
        private static readonly HashSet<string> ValidAnswers = new(StringComparer.OrdinalIgnoreCase)
        {
            "A", "B", "C", "D",
            "TRUE", "FALSE", "T", "F",
            "TRUE.", "FALSE.",          // trailing punctuation tolerance
            "A.", "B.", "C.", "D."
        };

        // ── Quit / pause phrases ───────────────────────────────────────────────
        private static readonly string[] QuitPhrases =
        {
            "quit quiz", "stop quiz", "exit quiz", "end quiz",
            "leave quiz", "cancel quiz", "pause quiz",
            "i want to stop", "stop the quiz", "i'm done",
            "nevermind", "never mind", "forget it"
        };

        public QuizEngine()
        {
            _questions = BuildQuestions();
        }

        // ── Public API ─────────────────────────────────────────────────────────

        /// <summary>Starts a new quiz session. Shuffles questions.</summary>
        public string Start()
        {
            _shuffled = _questions.OrderBy(_ => Guid.NewGuid()).Take(12).ToList();
            _currentIndex = 0;
            _score = 0;
            _awaitingResumeConfirm = false;
            State = QuizState.InProgress;

            return "Welcome to the CyberBot Quiz!\n\n" +
                   $"You'll answer {_shuffled.Count} cybersecurity questions.\n" +
                   "Type the letter (A / B / C / D) or True / False to answer.\n" +
                   "Type 'quit quiz' at any time to stop and do something else.\n\n" +
                   GetCurrentQuestion();
        }

        /// <summary>
        /// Main answer processor. Handles:
        ///   - Explicit quit commands  → exits quiz cleanly
        ///   - Valid answers (A/B/C/D, True/False) → marks right/wrong, next question
        ///   - Off-topic input → warns user once, offers to pause or continue
        ///   - Resume confirm ("yes"/"continue") after off-topic warning
        /// </summary>
        public string ProcessAnswer(string input)
        {
            if (State != QuizState.InProgress)
                return "No quiz is currently running. Type 'start quiz' to begin!";

            string trimmed = input.Trim();
            string lower = trimmed.ToLower();
            string upper = trimmed.ToUpper().TrimEnd('.'); // normalise "A." → "A"

            // ── 1. Explicit quit command ───────────────────────────────────────
            if (IsQuitCommand(lower))
            {
                return QuitQuiz();
            }

            // ── 2. User is confirming they want to resume after off-topic input ─
            if (_awaitingResumeConfirm)
            {
                if (lower.Contains("yes") || lower.Contains("continue") ||
                    lower.Contains("resume") || lower.Contains("keep going") ||
                    lower.Contains("ok") || lower.Contains("sure"))
                {
                    _awaitingResumeConfirm = false;
                    return "Great! Let's continue. \n\n" + GetCurrentQuestion();
                }
                else if (lower.Contains("no") || lower.Contains("stop") ||
                         lower.Contains("quit") || lower.Contains("exit") ||
                         lower.Contains("pause") || lower.Contains("leave"))
                {
                    return QuitQuiz();
                }
                else
                {
                    // They typed something else entirely — exit quiz, hand back to chatbot
                    State = QuizState.Idle;
                    _awaitingResumeConfirm = false;
                    return "Quiz paused. I'll pass your message to the chatbot now!\n\n" +
                           "Type 'start quiz' whenever you want to try again.";
                }
            }

            // ── 3. Valid quiz answer ───────────────────────────────────────────
            if (ValidAnswers.Contains(upper) ||
                upper == "TRUE" || upper == "FALSE" ||
                upper == "T" || upper == "F")
            {
                return EvaluateAnswer(upper);
            }

            // ── 4. Off-topic / unrecognised input ─────────────────────────────
            // The user typed something that isn't A/B/C/D or True/False.
            // Warn them once and ask if they want to pause or keep going.
            _awaitingResumeConfirm = true;
            return $"It looks like \"{trimmed}\" isn't a quiz answer.\n\n" +
                   "Would you like to:\n" +
                   "  • Type 'yes' or 'continue' to keep going with the quiz\n" +
                   "  • Type 'no' or 'quit quiz' to stop and chat normally\n\n" +
                   $"(You're on question {_currentIndex + 1} of {_shuffled.Count}, " +
                   $"score so far: {_score})";
        }

        /// <summary>Returns the current question text (already formatted).</summary>
        public string GetCurrentQuestion()
        {
            if (_currentIndex >= _shuffled.Count) return string.Empty;
            return _shuffled[_currentIndex].Format(_currentIndex + 1, _shuffled.Count);
        }

        /// <summary>Force-quits the quiz from outside (e.g. MainWindow toolbar button).</summary>
        public void Quit() => State = QuizState.Idle;

        // ── Private helpers ────────────────────────────────────────────────────

        private string EvaluateAnswer(string upper)
        {
            // Normalise T/F to TRUE/FALSE
            if (upper == "T") upper = "TRUE";
            if (upper == "F") upper = "FALSE";

            var q = _shuffled[_currentIndex];
            bool correct = upper == q.CorrectAnswer.ToUpper();
            if (correct) _score++;

            string feedback = correct
                ? $"Correct! {q.Explanation}"
                : $"Incorrect. The correct answer was {q.CorrectAnswer}. {q.Explanation}";

            _currentIndex++;

            if (_currentIndex >= _shuffled.Count)
            {
                State = QuizState.Finished;
                return feedback + "\n\n" + BuildFinalScore();
            }

            return feedback +
                   $"\n\n── Question {_currentIndex + 1} of {_shuffled.Count} ──\n\n" +
                   GetCurrentQuestion();
        }

        private string QuitQuiz()
        {
            State = QuizState.Idle;
            _awaitingResumeConfirm = false;
            int remaining = _shuffled.Count - _currentIndex;
            return $"Quiz stopped. \n\n" +
                   $"You answered {_currentIndex} of {_shuffled.Count} questions.\n" +
                   $"Score so far: {_score}/{_currentIndex}\n\n" +
                   "You can now ask me anything — or type 'start quiz' to try again from the beginning!";
        }

        private static bool IsQuitCommand(string lower)
        {
            foreach (string phrase in QuitPhrases)
                if (lower.Contains(phrase)) return true;
            return false;
        }

        private string BuildFinalScore()
        {
            double pct = (double)_score / _shuffled.Count * 100;
            string badge = pct >= 80
                ? "🏆 Great job! You're a cybersecurity pro!"
                : pct >= 50
                    ? "Good effort! Keep learning to stay safe online!"
                    : "Keep learning — cybersecurity knowledge saves you from real threats!";

            return $"━━━━━━━━━━━━━━━━━━━━━\n" +
                   $"  QUIZ COMPLETE!\n" +
                   $"  Score: {_score}/{_shuffled.Count}  ({pct:F0}%)\n" +
                   $"━━━━━━━━━━━━━━━━━━━━━\n\n" +
                   badge + "\n\n" +
                   "Type 'start quiz' to play again, or ask me about any cybersecurity topic!";
        }

        // ── Question bank ──────────────────────────────────────────────────────

        private static List<QuizQuestion> BuildQuestions()
        {
            return new List<QuizQuestion>
            {
                // ── Multiple Choice ────────────────────────────────────────────
                new QuizQuestion
                {
                    Text = "What should you do if you receive an email asking for your password?",
                    Type = QuestionType.MultipleChoice,
                    Options = new() { "A) Reply with your password", "B) Delete the email",
                                      "C) Report the email as phishing", "D) Ignore it" },
                    CorrectAnswer = "C",
                    Explanation = "Reporting phishing emails helps protect others and alerts your email provider."
                },
                new QuizQuestion
                {
                    Text = "Which of the following is the strongest password?",
                    Type = QuestionType.MultipleChoice,
                    Options = new() { "A) password123", "B) MyDog2020",
                                      "C) Tr0ub4dor&3!", "D) qwerty" },
                    CorrectAnswer = "C",
                    Explanation = "A strong password uses uppercase, lowercase, numbers, and symbols — and is at least 12 characters long."
                },
                new QuizQuestion
                {
                    Text = "What does HTTPS in a website URL indicate?",
                    Type = QuestionType.MultipleChoice,
                    Options = new() { "A) The site is government-owned", "B) The connection is encrypted",
                                      "C) The site is fast", "D) You are logged in" },
                    CorrectAnswer = "B",
                    Explanation = "HTTPS means your connection to the website is encrypted using SSL/TLS, protecting your data in transit."
                },
                new QuizQuestion
                {
                    Text = "What is ransomware?",
                    Type = QuestionType.MultipleChoice,
                    Options = new() { "A) Software that speeds up your PC", "B) A type of antivirus",
                                      "C) Malware that locks your files and demands payment", "D) A firewall program" },
                    CorrectAnswer = "C",
                    Explanation = "Ransomware encrypts your files and demands a ransom to restore access. Regular backups are your best defence."
                },
                new QuizQuestion
                {
                    Text = "What is two-factor authentication (2FA)?",
                    Type = QuestionType.MultipleChoice,
                    Options = new() { "A) Logging in with two passwords", "B) Using two devices",
                                      "C) Verifying identity with a second factor beyond a password", "D) A type of firewall" },
                    CorrectAnswer = "C",
                    Explanation = "2FA adds a second verification step (like an SMS code or app notification), making it much harder for attackers to access your account."
                },
                new QuizQuestion
                {
                    Text = "Which of the following is a sign of a phishing email?",
                    Type = QuestionType.MultipleChoice,
                    Options = new() { "A) Your name is correctly spelled", "B) The sender's domain matches the company exactly",
                                      "C) The email creates urgency ('Act now!')", "D) There are no attachments" },
                    CorrectAnswer = "C",
                    Explanation = "Phishing emails often create a false sense of urgency to pressure you into acting without thinking."
                },
                new QuizQuestion
                {
                    Text = "What is a VPN primarily used for?",
                    Type = QuestionType.MultipleChoice,
                    Options = new() { "A) Speeding up your internet", "B) Hiding your IP and encrypting your connection",
                                      "C) Blocking viruses", "D) Storing passwords" },
                    CorrectAnswer = "B",
                    Explanation = "A VPN (Virtual Private Network) encrypts your internet traffic and hides your IP address, protecting your privacy online."
                },
                new QuizQuestion
                {
                    Text = "What should you do before clicking a link in an email?",
                    Type = QuestionType.MultipleChoice,
                    Options = new() { "A) Click it immediately", "B) Hover over it to check the actual URL",
                                      "C) Forward it to friends", "D) Reply and ask if it is real" },
                    CorrectAnswer = "B",
                    Explanation = "Hovering over a link reveals the actual destination URL, helping you spot fake or malicious links before clicking."
                },

                // ── True / False ───────────────────────────────────────────────
                new QuizQuestion
                {
                    Text = "True or False: Using the same password for multiple accounts is safe as long as the password is strong.",
                    Type = QuestionType.TrueFalse,
                    Options = new() { "True", "False" },
                    CorrectAnswer = "FALSE",
                    Explanation = "Reusing passwords is dangerous. If one account is breached, all accounts with the same password are at risk."
                },
                new QuizQuestion
                {
                    Text = "True or False: A firewall can help block unauthorised access to your device or network.",
                    Type = QuestionType.TrueFalse,
                    Options = new() { "True", "False" },
                    CorrectAnswer = "TRUE",
                    Explanation = "Firewalls monitor and control incoming and outgoing network traffic based on security rules, blocking suspicious connections."
                },
                new QuizQuestion
                {
                    Text = "True or False: WhatsApp messages are end-to-end encrypted by default.",
                    Type = QuestionType.TrueFalse,
                    Options = new() { "True", "False" },
                    CorrectAnswer = "TRUE",
                    Explanation = "WhatsApp uses end-to-end encryption, meaning only you and the recipient can read your messages — not even WhatsApp."
                },
                new QuizQuestion
                {
                    Text = "True or False: It is safe to use public Wi-Fi for online banking without a VPN.",
                    Type = QuestionType.TrueFalse,
                    Options = new() { "True", "False" },
                    CorrectAnswer = "FALSE",
                    Explanation = "Public Wi-Fi is unsecured and can be monitored. Always use a VPN when doing sensitive activities on public networks."
                },
            };
        }
    }

    public enum QuestionType { MultipleChoice, TrueFalse }

    public class QuizQuestion
    {
        public string Text { get; set; } = string.Empty;
        public QuestionType Type { get; set; }
        public List<string> Options { get; set; } = new();
        public string CorrectAnswer { get; set; } = string.Empty;
        public string Explanation { get; set; } = string.Empty;

        public string Format(int current, int total)
        {
            string header = Type == QuestionType.TrueFalse
                ? $"Question {current}/{total}  [True / False]\n\n"
                : $"Question {current}/{total}  [Multiple Choice]\n\n";

            string optionText = string.Join("\n", Options);
            return header + Text + "\n\n" + optionText;
        }
    }
}
