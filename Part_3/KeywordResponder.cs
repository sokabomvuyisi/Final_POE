using System;
using System.Collections.Generic;
using System.Linq;

namespace CybersecurityChatbot
{
    public class KeywordResponder
    {
        private readonly Dictionary<string, List<string>> _responses;
        private readonly Dictionary<string, List<FollowUp>> _followUps;
        private readonly Random _random = new();

        public KeywordResponder()
        {
            _responses = new Dictionary<string, List<string>>
            {
                ["password"] = new List<string>
                {
                    "Passwords are your first line of defence online!\n\n" +
                    "Here are the key rules:\n" +
                    "• Use at least 12 characters\n" +
                    "• Mix uppercase, lowercase, numbers and symbols\n" +
                    "• Never use personal info like your name or birthday\n" +
                    "• Never reuse the same password across different sites\n\n" +
                    "Would you like to know more? You can ask me:\n" +
                    "> 'How do I remember my passwords?'\n" +
                    "> 'How do I update my passwords?'\n" +
                    "> 'What is a password manager?'"
                },
                ["phishing"] = new List<string>
                {
                    "Phishing is when scammers pretend to be trusted organisations to steal your info!\n\n" +
                    "Warning signs to watch for:\n" +
                    "• Emails creating urgency ('Act now or lose your account!')\n" +
                    "• Sender email doesn't match the official domain\n" +
                    "• Links that look slightly wrong (e.g. amaz0n.com)\n" +
                    "• Unexpected attachments\n\n" +
                    "Would you like to know more? You can ask me:\n" +
                    "> 'How do I report a phishing email?'\n" +
                    "> 'What do I do if I clicked a phishing link?'\n" +
                    "> 'How do I check if an email is real?'"
                },
                ["privacy"] = new List<string>
                {
                    "Your online privacy is extremely important!\n\n" +
                    "Here's how to protect yourself:\n" +
                    "• Review social media privacy settings regularly\n" +
                    "• Limit what personal info you share publicly\n" +
                    "• Use a VPN on public Wi-Fi\n" +
                    "• Read app permissions before installing\n\n" +
                    "Would you like to know more? You can ask me:\n" +
                    "> 'What is a VPN?'\n" +
                    "> 'How do I check my social media privacy settings?'\n" +
                    "> 'What data do apps collect about me?'"
                },
                ["scam"] = new List<string>
                {
                    "Online scams are becoming more sophisticated every day!\n\n" +
                    "The most common scams in South Africa:\n" +
                    "• Fake job offers asking for upfront payments\n" +
                    "• Romance scams on dating apps\n" +
                    "• Fake bank SMS alerts\n" +
                    "• Prize/lottery scams ('You have won!')\n\n" +
                    "Would you like to know more? You can ask me:\n" +
                    "> 'How do I report a scam in South Africa?'\n" +
                    "> 'How do I know if a job offer is fake?'\n" +
                    "> 'What do I do if I was scammed?'"
                },
                ["malware"] = new List<string>
                {
                    "Malware is malicious software designed to damage or gain access to your device!\n\n" +
                    "Types of malware you should know:\n" +
                    "• Virus — spreads by attaching to files\n" +
                    "• Ransomware — locks your files and demands payment\n" +
                    "• Spyware — secretly monitors your activity\n" +
                    "• Trojan — disguises itself as legitimate software\n\n" +
                    "Would you like to know more? You can ask me:\n" +
                    "> 'How do I remove malware from my device?'\n" +
                    "> 'What is the best antivirus software?'\n" +
                    "> 'How do I know if my device has malware?'"
                },
                ["firewall"] = new List<string>
                {
                    "A firewall is your device's security guard — it monitors and controls network traffic!\n\n" +
                    "Key facts:\n" +
                    "• Blocks unauthorised access to your device\n" +
                    "• Both your router and PC should have one enabled\n" +
                    "• Windows Firewall is built into Windows 10/11\n" +
                    "• Can block suspicious outgoing traffic too\n\n" +
                    "Would you like to know more? You can ask me:\n" +
                    "> 'How do I turn on Windows Firewall?'\n" +
                    "> 'Do I need a firewall if I have antivirus?'\n" +
                    "> 'What is the difference between a hardware and software firewall?'"
                },
                ["encryption"] = new List<string>
                {
                    "Encryption scrambles your data so only authorised people can read it!\n\n" +
                    "Where encryption protects you:\n" +
                    "• HTTPS websites (look for the padlock in your browser)\n" +
                    "• WhatsApp messages (end-to-end encrypted)\n" +
                    "• Full disk encryption on your laptop (BitLocker on Windows)\n" +
                    "• Encrypted USB drives for sensitive files\n\n" +
                    "Would you like to know more? You can ask me:\n" +
                    "> 'How do I enable BitLocker on Windows?'\n" +
                    "> 'What does end-to-end encryption mean?'\n" +
                    "> 'How do I know if a website is secure?'"
                },
                ["vpn"] = new List<string>
                {
                    "A VPN (Virtual Private Network) hides your internet activity and protects your data!\n\n" +
                    "Why you need a VPN:\n" +
                    "• Hides your IP address from websites and hackers\n" +
                    "• Encrypts your connection on public Wi-Fi\n" +
                    "• Prevents your ISP from tracking your browsing\n" +
                    "• Lets you access geo-restricted content safely\n\n" +
                    "Would you like to know more? You can ask me:\n" +
                    "> 'What is the best VPN to use?'\n" +
                    "> 'Is a free VPN safe to use?'\n" +
                    "> 'How do I set up a VPN on my phone?'"
                }
            };

            // Follow-up responses for each keyword
            _followUps = new Dictionary<string, List<FollowUp>>
            {
                ["password"] = new List<FollowUp>
                {
                    new FollowUp("remember", "how to remember",
                        "How to Remember Your Passwords:\n\n" +
                        "• Use a passphrase — e.g. 'Coffee@Sunrise!2024' is strong AND memorable\n" +
                        "• Use a password manager like Bitwarden (free) or LastPass\n" +
                        "• Never write passwords on sticky notes near your computer\n" +
                        "• If you must write it down, store it in a locked safe\n\n" +
                        "> Ask me 'What is a password manager?' to learn more!"),

                    new FollowUp("update", "change", "how to update",
                        "How to Update Your Passwords:\n\n" +
                        "• Change passwords immediately if you suspect a breach\n" +
                        "• Use HaveIBeenPwned.com to check if your email was leaked\n" +
                        "• Update passwords every 6-12 months for important accounts\n" +
                        "• Always update default passwords on new devices and routers\n\n" +
                        "> Ask me 'How do I remember my passwords?' for memory tips!"),

                    new FollowUp("manager", "password manager",
                        "What is a Password Manager?\n\n" +
                        "A password manager is an app that:\n" +
                        "• Stores all your passwords securely in one place\n" +
                        "• Generates strong random passwords for you\n" +
                        "• Auto-fills login forms safely\n" +
                        "• Only requires you to remember ONE master password\n\n" +
                        "Recommended options:\n" +
                        "• Bitwarden — free and open source \n" +
                        "• 1Password — great for families\n" +
                        "• KeePass — offline option\n\n" +
                        "> Ask me 'How do I update my passwords?' for more tips!")
                },
                ["phishing"] = new List<FollowUp>
                {
                    new FollowUp("report", "how to report",
                        "How to Report a Phishing Email:\n\n" +
                        "• In Gmail: click the 3 dots → 'Report phishing'\n" +
                        "• In Outlook: click 'Report' → 'Report Phishing'\n" +
                        "• Forward it to: report@phishing.gov.za (South Africa)\n" +
                        "• Report to your bank directly if it impersonates them\n" +
                        "• Delete the email after reporting — do not click any links\n\n" +
                        "> Ask me 'What do I do if I clicked a phishing link?' if you already clicked!"),

                    new FollowUp("clicked", "i clicked", "what do i do",
                        "You Clicked a Phishing Link — Act Fast!\n\n" +
                        "Step 1: Disconnect from the internet immediately\n" +
                        "Step 2: Change passwords for any accounts you accessed\n" +
                        "Step 3: Run a full antivirus scan on your device\n" +
                        "Step 4: Enable 2FA on your important accounts\n" +
                        "Step 5: Contact your bank if any financial info was entered\n" +
                        "Step 6: Monitor your accounts for suspicious activity\n\n" +
                        "> Ask me 'How do I check if an email is real?' to prevent future clicks!"),

                    new FollowUp("check", "real", "verify", "how do i check",
                        "How to Check if an Email is Real:\n\n" +
                        "• Hover over links before clicking — check the actual URL\n" +
                        "• Check the sender's full email address (not just the display name)\n" +
                        "• Look for spelling mistakes in the email body\n" +
                        "• Call the company directly using the number on their OFFICIAL website\n" +
                        "• Legitimate companies never ask for passwords via email\n\n" +
                        "> Ask me 'How do I report a phishing email?' to take action!")
                },
                ["scam"] = new List<FollowUp>
                {
                    new FollowUp("report", "how to report", "south africa",
                        "How to Report a Scam in South Africa:\n\n" +
                        "• South African Police Service (SAPS): 10111\n" +
                        "• South African Banking Risk Centre (SABRIC): 011 847 3000\n" +
                        "• Financial Sector Conduct Authority (FSCA): 0800 20 37 22\n" +
                        "• Report online fraud at: www.cybercrime.org.za\n" +
                        "• Consumer Protection: 012 428 7000\n\n" +
                        "> Ask me 'What do I do if I was scammed?' for immediate steps!"),

                    new FollowUp("scammed", "i was scammed", "what do i do",
                        "If You Were Scammed — Take These Steps Now:\n\n" +
                        "Step 1: Contact your bank immediately to freeze your account\n" +
                        "Step 2: Change passwords on all affected accounts\n" +
                        "Step 3: Report it to SAPS (10111) and SABRIC (011 847 3000)\n" +
                        "Step 4: Keep all evidence — screenshots, emails, messages\n" +
                        "Step 5: Warn your contacts if the scammer had access to your accounts\n\n" +
                        "> Ask me 'How do I report a scam in South Africa?' for contact details!")
                }
            };
        }

        public string? GetResponse(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return null;
            string lower = input.ToLower();

            // Check follow-ups first
            foreach (var keyword in _followUps.Keys)
            {
                if (lower.Contains(keyword))
                {
                    foreach (var followUp in _followUps[keyword])
                    {
                        if (followUp.Matches(lower))
                            return followUp.Response;
                    }
                }
            }

            // Then check main keywords
            foreach (var entry in _responses)
            {
                if (lower.Contains(entry.Key))
                {
                    var list = entry.Value;
                    return list[_random.Next(list.Count)];
                }
            }

            return null;
        }

        public string? GetMatchedKeyword(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return null;
            string lower = input.ToLower();
            foreach (var key in _responses.Keys)
                if (lower.Contains(key)) return key;
            return null;
        }

        public string? GetResponseForKeyword(string keyword)
        {
            if (_responses.TryGetValue(keyword.ToLower(), out var list))
                return list[_random.Next(list.Count)];
            return null;
        }

        public List<string> GetAllKeywords() => _responses.Keys.ToList();
    }

    /// <summary>
    /// Represents a follow-up topic with one or more trigger phrases.
    /// </summary>
    public class FollowUp
    {
        private readonly List<string> _triggers;
        public string Response { get; }

        public FollowUp(string response, params string[] triggers)
        {
            Response = response;
            _triggers = triggers.ToList();
        }

        // Constructor that takes triggers first then response
        public FollowUp(string trigger1, string trigger2, string response)
        {
            _triggers = new List<string> { trigger1, trigger2 };
            Response = response;
        }

        public FollowUp(string trigger1, string trigger2, string trigger3, string response)
        {
            _triggers = new List<string> { trigger1, trigger2, trigger3 };
            Response = response;
        }

        public bool Matches(string input) =>
            _triggers.Any(t => input.Contains(t));
    }
}