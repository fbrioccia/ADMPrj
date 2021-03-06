﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ADMPrj.Models
{
    public static class Constants
    {
        public static class PartOfSpeech
        {
            public static class Adjective
            {
                public static String JJ = "JJ";
                public static String JJR = "JJR";
                public static String JJS = "JJS";
            }
            
            public static class Noun
            {
                public static String NN = "NN";
                public static String NNS = "NNS"; 
                public static String NNP="NNP"; 
                public static String NNPS = "NNPS";
            }

            public static class Adverb
            {
                public static String RB = "RB";
                public static String RBR = "RBR";
                public static String RBS = "RBS";
            }

            public static class Verb
            {
                public static String VB = "VB";
                public static String VBD = "VBD";
                public static String VBG = "VBG";
                public static String VBN = "VBN";
                public static String VBP = "VBP";
                public static String VBZ = "VBZ";
            }

            public static bool IsAdjective(string token)
            {
                var upperToken = token.ToUpper().Trim();
                return upperToken == Adjective.JJ || upperToken == Adjective.JJR || upperToken == Adjective.JJS;
            }

        }

        //public static List<(string Code, string Description)> PartOfSpeechDictionary = new List<(string Code, string Description)>()
        //{
        //    ("CC", "Coordinating conjunction"),
        //    ("CD", "Cardinal number"),
        //    ("DT", "Determiner"),
        //    ("EX", "Existential there"),
        //    ("FW", "Foreign word"),
        //    ("IN", "Preposition or subordinating conjunction"),
        //    ("JJ", "Adjective"),
        //    ("JJR", "Adjective, comparative"),
        //    ("JJS", "Adjective, superlative"),
        //    ("LS", "List item marker"),
        //    ("MD", "Modal"),
        //    ("NN", "Noun, singular or mass"),
        //    ("NNS", "Noun, plural"),
        //    ("NNP", "Proper noun, singular"),
        //    ("NNPS", "Proper noun, plural"),
        //    ("PDT", "Predeterminer"),
        //    ("POS", "Possessive ending"),
        //    ("PRP", "Personal pronoun"),
        //    ("PRP$", "Possessive pronoun"),
        //    ("RB", "Adverb"),
        //    ("RBR", "Adverb, comparative"),
        //    ("RBS", "Adverb, superlative"),
        //    ("RP", "Particle"),
        //    ("SYM", "Symbol"),
        //    ("TO", "to"),
        //    ("UH", "Interjection"),
        //    ("VB", "Verb, base form"),
        //    ("VBD", "Verb, past tense"),
        //    ("VBG", "Verb, gerund or present participle"),
        //    ("VBN", "Verb, past participle"),
        //    ("VBP", "Verb, non­3rd person singular present"),
        //    ("VBZ", "Verb, 3rd person singular present"),
        //    ("WDT", "Wh­determiner"),
        //    ("WP", "Wh­pronoun"),
        //    ("WP$", "Possessive wh­pronoun"),
        //    ("WRB", "Wh­adverb")
        //};

        
    }
    //CC Coordinating conjunction
    //CD Cardinal number
    //DT Determiner
    //EX Existential there
    //FW Foreign word
    //IN Preposition or subordinating conjunction
    //JJ Adjective
    //JJR Adjective, comparative
    //JJS Adjective, superlative
    //LS List item marker
    //MD Modal
    //NN Noun, singular or mass
    //NNS Noun, plural
    //NNP Proper noun, singular
    //NNPS Proper noun, plural
    //PDT Predeterminer
    //POS Possessive ending
    //PRP Personal pronoun
    //PRP$ Possessive pronoun
    //RB Adverb
    //RBR Adverb, comparative
    //RBS Adverb, superlative
    //RP Particle
    //SYM Symbol
    //TO to
    //UH Interjection
    //VB Verb, base form
    //VBD Verb, past tense
    //VBG Verb, gerund or present participle
    //VBN Verb, past participle
    //VBP Verb, non­3rd person singular present
    //VBZ Verb, 3rd person singular present
    //WDT Wh­determiner
    //WP Wh­pronoun
    //WP$ Possessive wh­pronoun
    //WRB Wh­adverb
}
