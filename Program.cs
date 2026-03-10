namespace NameGenerator
{
    class NameGenerator
    {
        public interface IRule
        {
            bool IsAllowed(string currentWord, string nextLetter, int length);
        }

        public class NoTripleVowelsRule : IRule
        {
            public bool IsAllowed(string word, string nextLetter, int length)
            {
                if (word.Length < 2) return true;
                return !(IsVowel(word[^1].ToString()) && IsVowel(word[^2].ToString()) && IsVowel(nextLetter));
            }
        }

        public class NoTripleConsonantsRule : IRule
        {
            public bool IsAllowed(string word, string nextLetter, int length)
            {
                if (word.Length < 2) return true;
                return !( !IsVowel(word[^1].ToString()) && !IsVowel(word[^2].ToString()) && !IsVowel(nextLetter));
            }
        }

        public class AllowedClusterRule : IRule
        {
            private HashSet<string> startAllowed = new HashSet<string>()
            {
                "TR", "CR", "KR", "BL", "SL", "DR", "BR", "PL", "WR", "GN", "KN", "SK", "CH", "SH", "PH", "TH", "FL"
            };

            private HashSet<string> endAllowed = new HashSet<string>()
            {
                "SK", "CH", "SH", "PH", "TH", "NG", "CK", "RN", "NT"
            };

            public bool IsAllowed(string word, string nextLetter, int length)
            {
                if (word.Length < 1) return true;

                string cluster = word[^1].ToString() + nextLetter;
                if (IsVowel(word[^1].ToString()) || IsVowel(nextLetter)) return true;

                if (word.Length == 1)
                    return startAllowed.Contains(cluster);

                if (word.Length + 1 == length)
                    return endAllowed.Contains(cluster);

                return startAllowed.Contains(cluster) || endAllowed.Contains(cluster);

            }
        }

        public class RestrictVowelClusterRule : IRule
        {
            private HashSet<string> restricted = new HashSet<string>()
            {
                "AA", "II", "UO", "YY"
            };

            public bool IsAllowed(string word, string nextLetter, int length)
            {
                if (word.Length < 1) return true;

                if (restricted.Contains(word[^1].ToString() + nextLetter)) return false;
                return true;
            }
        }

        public class NoRepeatRule : IRule
        {
            private HashSet<string> notRepeatable = new HashSet<string>() { "W", "Q", "H", "J", "K", "X", "V" };

            public bool IsAllowed(string word, string nextLetter, int length)
            {
                if (word.Length < 1) return true;
                return !(word[^1].ToString() == nextLetter && notRepeatable.Contains(nextLetter));
            }
        }

        public class NoStartLettersRule : IRule
        {
            private readonly HashSet<string> notStarting = new HashSet<string>();

            public NoStartLettersRule(string NotStarting)
            {
                foreach (char letter in NotStarting)
                    notStarting.Add(letter.ToString());
            }
            
            public bool IsAllowed(string word, string nextLetter, int length)
            {
                if (word.Length == 0 && notStarting.Contains(nextLetter)) return false;
                return true;
            }
        }

        public class NoEndLettersRule : IRule
        {
            private HashSet<string> notEnding = new HashSet<string>();
            
            public NoEndLettersRule(string NotEnding)
            {
                foreach (char letter in NotEnding)
                    notEnding.Add(letter.ToString());
            }

            public bool IsAllowed(string word, string nextLetter, int length)
            {
                if (word.Length + 1 == length && notEnding.Contains(nextLetter)) return false;
                return true;
            }
        }

        public class FollowLetterRule : IRule
        {
            private readonly string firstLetter;
            private readonly string followLetter;

            public FollowLetterRule(string FirstLetter, string FollowLetter)
            {
                firstLetter = FirstLetter;
                followLetter = FollowLetter;
            }

            public bool IsAllowed(string word, string nextLetter, int length)
            {
                if (word.Length < 1) return true;
                if (word[^1].ToString() == firstLetter && nextLetter != followLetter)
                    return false;
                return true;
            }
        }

        public class MandatoryStartClusterRule : IRule
        {
            private readonly string cluster;

            public MandatoryStartClusterRule(string Cluster)
            {
                cluster = Cluster;
            }

            public bool IsAllowed(string word, string nextLetter, int length)
            {
                for (int i = 0; i < cluster.Length; i++)
                    if (word.Length == i && nextLetter != cluster[i].ToString()) return false;
                return true;
            }
        }

        public class MandatoryDoubleVowelsRule : IRule
        {
            public bool IsAllowed(string word, string nextLetter, int length)
            {
                if (word.Length == 0) return true;

                if (word.Length == 1 && IsVowel(word[^1].ToString()) && word[^1].ToString() != nextLetter) return false;

                if (word.Length >= 2)
                {
                    if (IsVowel(word[^2].ToString()) && IsVowel(word[^1].ToString()) && IsVowel(nextLetter))
                        return false;

                    if (!IsVowel(word[^2].ToString()) && IsVowel(word[^1].ToString()) && !IsVowel(nextLetter))
                        return false;
                }

                return true;
            }
        }

        public class NoRepeatLetters : IRule
        {
            public bool IsAllowed(string word, string nextLetter, int length)
            {
                foreach (char letter in word)
                    if (letter.ToString() == nextLetter)
                        return false;

                return true;
            }
        }

        public class LanguageProfile
        {
            public List<IRule> Rules { get; } = new List<IRule>();

            public LanguageProfile(IEnumerable<IRule> rules)
            {
                Rules.AddRange(rules);
            }

            public bool IsAllowed(string word, string nextLetter, int length)
            {
                foreach (var rule in Rules)
                {
                    if (!rule.IsAllowed(word, nextLetter, length))
                        return false;
                }
                return true;
            }
        }

        // Dictionary of all letters in the alphabet with weights assigned to each
        private static Dictionary<string, int> alphabet = new Dictionary<string, int>()
        {
            { "A", 8000 },
            { "B", 1600 },
            { "C", 3000 },
            { "D", 4400 },
            { "E", 12000 },
            { "F", 2500 },
            { "G", 1700 },
            { "H", 6400 },
            { "I", 8000 },
            { "J", 400 },
            { "K", 800 },
            { "L", 4000 },
            { "M", 3000 },
            { "N", 8000 },
            { "O", 8000 },
            { "P", 1700 },
            { "Q", 500 },
            { "R", 6200 },
            { "S", 8000 },
            { "T", 9000 },
            { "U", 3400 },
            { "V", 1200 },
            { "W", 2000 },
            { "X", 400 },
            { "Y", 2000 },
            { "Z", 200 }
        };
        private static Random rand = new Random();

        // Checks if a single letter is a vowel
        static bool IsVowel(string letter)
        {
            return "AEIOUY".IndexOf(letter) >= 0;
        }

        // Randomly picks a letter from the alphabet
        static string All()
        {
            int totalWeight = 0;

            foreach (var weight in alphabet.Values)
                totalWeight += weight;

            int choice = rand.Next(totalWeight);

            int cumulative = 0;
            foreach (var letter in alphabet)
            {
                cumulative += letter.Value;
                if (choice < cumulative)
                    return letter.Key;
            }

            throw new Exception("It don't workey man");
        }

        // Randomly picks a consonants from the alphabet
        static string Con()
        {
            while (true)
            {
                string con = All();
                if (!IsVowel(con))
                {
                    return con;
                }
            }
        }

        // Randomly picks a vowel from the alphabet
        static string Vow()
        {
            while (true)
            {
                string vow = All();
                if (IsVowel(vow))
                {
                    return vow;
                }
            }
        }

        public static string GenerateName(LanguageProfile profile, int maxLength = 10 ,int minLength = 3)
        {
            int length = rand.Next(minLength, maxLength + 1);
            string name = "";
            while (name.Length < length)
            {
                string letter = All();

                if (profile.IsAllowed(name, letter, length))
                    name += letter;
            }
            return char.ToUpper(name[0]) + name.Substring(1).ToLower();
        }

        static void Main()
        {
            var humanLike = new LanguageProfile(new IRule[]
            {
                new NoTripleConsonantsRule(),
                new NoTripleVowelsRule(),
                new AllowedClusterRule(),
                new RestrictVowelClusterRule(),
                new NoRepeatRule(),
                new NoStartLettersRule("XQZJV"),
                new NoEndLettersRule("HQVWJ"),
                new FollowLetterRule("Q", "U"),
                //new MandatoryStartClusterRule("F"),
                //new MandatoryDoubleVowelsRule(),
                //new NoRepeatLetters()
            });

            while (true)
            {
                string testName = GenerateName(humanLike, 10, 3);
                Console.WriteLine(testName);
                Console.ReadLine();
            }
        }
    }
}