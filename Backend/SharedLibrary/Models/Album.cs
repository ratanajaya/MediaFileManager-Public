using SharedLibrary.Helpers;
using System;
using System.Collections.Generic;

namespace SharedLibrary.Models
{
    //Used for universal _album.json file
    //Should not contains any informations that: 
    //-Can be infered through filesystem (ie. path, page count)
    //-Is volatile (ie. last page opened)
    //Fields that are inferred from other fields and requires logic to output must be exposed as methods
    public class Album
    {
        public string Title { get; set; } = "";
        public string Category { get; set; } = "";
        public string Orientation { get; set; } = "";

        public List<string> Artists { get; set; } = new List<string>();
        public List<string> Tags { get; set; } = new List<string>();
        public List<string> Characters { get; set; } = new List<string>();
        public List<string> Languages { get; set; } = new List<string>();
        public string Note { get; set; } = "";

        public int Tier { get; set; } = 0;

        public bool IsWip { get; set; } = false;
        public bool IsRead { get; set; } = false;

        public DateTime EntryDate { get; set; } = DateTime.Now;

        public Dictionary<string,int> ChapterTier { get; set; } = new Dictionary<string,int>();

        #region Legacy
        [Obsolete]
        public string GetAllArtists() { return string.Join(",", Artists); }
        [Obsolete]
        public string GetAllTags() { return string.Join(",", Tags); }
        [Obsolete]
        public string GetAllLanguages() { return string.Join(",", Languages); }
        [Obsolete]
        public string GetFullTitle() { return "[" + GetAllArtists() + "] " + Title; }
        #endregion

        public string GetArtistsDisplay() { return string.Join(", ", Artists); }
        public string GetTagsDisplay() { return string.Join(", ", Tags); }
        public string GetCharactersDisplay() { return string.Join(", ", Characters); }
        public string GetLanguagesDisplay() { return string.Join(", ", Languages); }

        private string _fullTitleDisplay;
        public string GetFullTitleDisplay() {
            if(_fullTitleDisplay == null) _fullTitleDisplay = "[" + GetArtistsDisplay() + "] " + Title;
            return _fullTitleDisplay; 
        }

        public void ValidateAndCleanup() {
            Artists = Artists.CleanListString();
            Tags = Tags.CleanListString();
            Characters = Characters.CleanListString();
            Languages = Languages.CleanListString();
            Note = !string.IsNullOrWhiteSpace(Note) ? Note : null;
        }
    }
}
