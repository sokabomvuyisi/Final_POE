using System.Collections.Generic;

namespace CybersecurityChatbot
{
    /// <summary>
    /// Stores and recalls user-specific information such as name and favourite topic.
    /// </summary>
    public class MemoryStore
    {
        public string UserName { get; set; } = string.Empty;
        public string FavouriteTopic { get; set; } = string.Empty;

        private readonly Dictionary<string, string> _store = new();

        /// <summary>Saves any key-value pair to memory.</summary>
        public void Store(string key, string value)
        {
            if (!string.IsNullOrWhiteSpace(key))
                _store[key.ToLower()] = value;
        }

        /// <summary>Retrieves a stored value by key. Returns empty string if not found.</summary>
        public string Recall(string key)
        {
            return _store.TryGetValue(key.ToLower(), out var value) ? value : string.Empty;
        }

        /// <summary>Builds a personalised opener using stored name and topic.</summary>
        public string GetPersonalisedOpener()
        {
            if (!string.IsNullOrWhiteSpace(UserName) && !string.IsNullOrWhiteSpace(FavouriteTopic))
                return $"As someone interested in {FavouriteTopic}, {UserName}, here's something relevant for you: ";

            if (!string.IsNullOrWhiteSpace(UserName))
                return $"{UserName}, ";

            return string.Empty;
        }

        /// <summary>Returns true if the user's name has been captured.</summary>
        public bool HasName() => !string.IsNullOrWhiteSpace(UserName);

        /// <summary>Returns true if a favourite topic has been stored.</summary>
        public bool HasFavouriteTopic() => !string.IsNullOrWhiteSpace(FavouriteTopic);
    }
}