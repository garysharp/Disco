using System;
using System.Collections.Generic;
using System.Linq;

namespace Disco
{
    public static class StringExtensions
    {
        /// <summary>
        /// A fuzzy string search algorithm.
        /// 
        /// Based on: ScoreSharp (https://github.com/bltavares/scoresharp)
        /// Based on: string_score from Joshaven Potter (https://github.com/joshaven/string_score)
        /// 
        /// MIT License
        /// </summary>
        public static double Score(this string Source, string Test, double Fuzziness = 0)
        {
            double total_char_score = 0, abbrv_size = Test.Length,
                fuzzies = 1, final_score, abbrv_score;
            int word_size = Source.Length;
            bool start_of_word_bonus = false;

            //If strings are equal, return 1.0
            if (Source == Test) return 1.0;

            int index_in_string,
                index_char_lowercase,
                index_char_uppercase,
                min_index;
            double char_score;
            string c;
            for (int i = 0; i < abbrv_size; i++)
            {
                c = Test[i].ToString();
                index_char_uppercase = Source.IndexOf(c.ToUpper());
                index_char_lowercase = Source.IndexOf(c.ToLower());
                min_index = Math.Min(index_char_lowercase, index_char_uppercase);

                //Finds first valid occurrence
                //In upper or lowercase
                index_in_string = min_index > -1 ?
                    min_index : Math.Max(index_char_lowercase, index_char_uppercase);

                //If no value is found
                //Check if fuzziness is allowed
                if (index_in_string == -1)
                {
                    if (Fuzziness > 0)
                    {
                        fuzzies += 1 - Fuzziness;
                        continue;
                    }
                    else return 0;
                }
                else
                    char_score = 0.1;

                //Check if current char is the same case
                //Then add bonus
                if (Source[index_in_string].ToString() == c) char_score += 0.1;

                //Check if char matches the first letter
                //And add bonus for consecutive letters
                if (index_in_string == 0)
                {
                    char_score += 0.6;

                    //Check if the abbreviation
                    //is in the start of the word
                    start_of_word_bonus = i == 0;
                }
                else
                {
                    // Acronym Bonus
                    // Weighing Logic: Typing the first character of an acronym is as if you
                    // preceded it with two perfect character matches.
                    if (Source.ElementAtOrDefault(index_in_string - 1).ToString() == " ") char_score += 0.8;
                }


                //Remove the start of string, so we don't reprocess it
                Source = Source.Substring(index_in_string + 1);

                //sum chars scores
                total_char_score += char_score;
            }

            abbrv_score = total_char_score / abbrv_size;

            //Reduce penalty for longer words
            final_score = ((abbrv_score * (abbrv_size / word_size)) + abbrv_score) / 2;

            //Reduce using fuzzies;
            final_score = final_score / fuzzies;

            //Process start of string bonus
            if (start_of_word_bonus && final_score <= 0.85)
                final_score += 0.15;

            return final_score;
        }

        /// <summary>
        /// A fuzzy string search algorithm.
        /// 
        /// Based on: ScoreSharp (https://github.com/bltavares/scoresharp)
        /// Based on: string_score from Joshaven Potter (https://github.com/joshaven/string_score)
        /// 
        /// MIT License
        /// </summary>
        public static double Score(this IEnumerable<string> Sources, string Test, double Fuzziness = 0)
        {
            return Sources
                .Where(s => s != null)
                .Select(s => s.Score(Test, Fuzziness))
                .Average();
        }
    }
}
