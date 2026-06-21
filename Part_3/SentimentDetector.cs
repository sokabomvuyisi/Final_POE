using System;
using System.Collections.Generic;

namespace CybersecurityChatbot
{
    /// <summary>
    /// Represents the emotional tone detected in user input.
    /// </summary>
    public enum Sentiment
    {
        Neutral,
        Worried,
        Curious,
        Frustrated,
        Happy
    }

    /// <summary>
    /// Detects the sentiment in user messages and provides empathetic opening responses.
    /// </summary>
    public class SentimentDetector
    {
        private readonly Dictionary<Sentiment, List<string>> _triggerWords;
        private readonly Dictionary<Sentiment, List<string>> _sentimentResponses;
        private readonly Random _random = new();

        public SentimentDetector()
        {
            _triggerWords = new Dictionary<Sentiment, List<string>>
            {
                [Sentiment.Worried] = new List<string>
                {
                    "worried", "scared", "afraid", "anxious", "nervous",
                    "unsafe", "fear", "frightened", "concerned", "panic"
                },
                [Sentiment.Curious] = new List<string>
                {
                    "curious", "wondering", "interested", "want to know",
                    "how does", "what is", "tell me about", "explain", "how do"
                },
                [Sentiment.Frustrated] = new List<string>
                {
                    "frustrated", "annoyed", "confused", "don't understand",
                    "doesn't make sense", "angry", "irritated", "lost", "complicated", "difficult"
                },
                [Sentiment.Happy] = new List<string>
                {
                    "great", "thanks", "helpful", "awesome", "love it",
                    "amazing", "brilliant", "thank you", "excellent", "perfect"
                }
            };

            _sentimentResponses = new Dictionary<Sentiment, List<string>>
            {
                [Sentiment.Worried] = new List<string>
                {
                    "It's completely understandable to feel that way. You're not alone — many people worry about this. Let me help put your mind at ease. ",
                    "I can hear that you're concerned, and that's actually a smart instinct. Here's what you need to know to stay safe. ",
                    "Your concern shows great awareness. Scammers can be very convincing. Let me share some tips to help you stay safe. "
                },
                [Sentiment.Curious] = new List<string>
                {
                    "Great question! Curiosity is the first step to staying safe online. Here's what you should know: ",
                    "Love the curiosity! Understanding this is really important for your digital safety. ",
                    "Excellent — let's dig into that! Here's a clear breakdown for you: "
                },
                [Sentiment.Frustrated] = new List<string>
                {
                    "I totally get it — cybersecurity can feel overwhelming at first. Let me break it down simply. ",
                    "No worries, this stuff can be confusing! Let me explain it in plain language. ",
                    "I understand the frustration. Let's slow down and walk through this step by step. "
                },
                [Sentiment.Happy] = new List<string>
                {
                    "Glad to hear that! Here's some more useful info for you: ",
                    "That's great to know! Let's keep building on that — here's another tip: ",
                    "Wonderful! Stay curious and stay safe. Here's something else worth knowing: "
                }
            };
        }

        /// <summary>
        /// Detects the sentiment in the given input string.
        /// Returns Neutral if no trigger words are found.
        /// </summary>
        public Sentiment Detect(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return Sentiment.Neutral;

            string lowerInput = input.ToLower();

            foreach (var entry in _triggerWords)
            {
                foreach (string word in entry.Value)
                {
                    if (lowerInput.Contains(word))
                        return entry.Key;
                }
            }

            return Sentiment.Neutral;
        }

        /// <summary>
        /// Returns a randomly selected empathetic opener for the given sentiment.
        /// Returns empty string for Neutral.
        /// </summary>
        public string GetSentimentResponse(Sentiment sentiment)
        {
            if (sentiment == Sentiment.Neutral)
                return string.Empty;

            if (_sentimentResponses.TryGetValue(sentiment, out var responses) && responses.Count > 0)
                return responses[_random.Next(responses.Count)];

            return string.Empty;
        }
    }
}