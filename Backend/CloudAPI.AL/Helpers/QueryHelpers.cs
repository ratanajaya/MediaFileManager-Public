using CloudAPI.AL.Models;
using SharedLibrary.Helpers;
using SharedLibrary.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using c = SharedLibrary.Constants;

namespace CloudAPI.AL.Helpers;

public static class QueryHelpers
{
    private static char[] _cons = { c.ConContain, c.ConEqual, c.ConNot, c.ConGreater, c.ConLesser };

    public static List<QuerySegment> GetQuerySegments(string query) {
        var modifiedQuery = query != null && !ContainsConnector(query) ? $"fulltitle:{query}" : query;
        var segmenStrs = !string.IsNullOrEmpty(modifiedQuery) ? modifiedQuery.Split(',') : new string[] { };

        var segments = segmenStrs.Select(s => {
            var segStr = s.Trim();
            int connectorIndex = segStr.IndexOfAny(_cons);
            var connector = segStr[connectorIndex];
            var key = segStr.Substring(0, connectorIndex);
            var val = segStr.Substring(connectorIndex + 1, segStr.Length - (connectorIndex + 1));

            return new QuerySegment { 
                Con = connector, Key = key, Val = val
            };
        }).ToList();

        return segments;
    }

    public static string CombineQuerySegments(List<QuerySegment> source) {
        return String.Join(',', source.Select(a => a.Text));
    }

    public static bool MatchAllQueries(Album album, List<QuerySegment> querySegs, string[] featuredArtists, string[] featuredCharacters, StringComparison comparer = StringComparison.OrdinalIgnoreCase) {
        bool IsMatch(Album album, QuerySegment querySeg) {
            var connector = querySeg.Con;
            var key = querySeg.Key;
            var val = querySeg.Val;

            if(key.Equals("Tag", comparer)) {
                var isContains = new Func<bool>(() => {
                    if(val.IndexOf('|') != -1) {
                        var vals = val.Split('|');
                        return album.Tags.Any(tag => vals.Contains(tag, comparer));
                    }
                    return album.Tags.Contains(val, comparer);
                })();

                return connector == ':' ? isContains : !isContains;
            }
            else if(key.Equals("Artist", comparer)) {
                return connector == ':' ? album.Artists.Any(a => a.Contains(val, comparer)) :
                        connector == '=' ? album.Artists.Any(a => a.Equals(val, comparer)) :
                        false;
            }
            else if(key.Equals("Character", comparer)) {
                return connector == ':' ? album.Characters.Any(a => a.Contains(val, comparer)) :
                        connector == '=' ? album.Characters.Any(a => a.Equals(val, comparer)) :
                        false;
            }
            else if(key.Equals("FullTitle", comparer)) {
                return album.GetFullTitleDisplay().Contains(val, comparer);
            }
            else if(key.Equals("Title", comparer)) {
                return album.Title.Contains(val, comparer);
            }
            else if(key.Equals("Category", comparer)) {
                return album.Category.Equals(val, comparer);
            }
            else if(key.Equals("Orientation", comparer)) {
                return album.Orientation.Equals(val, comparer);
            }
            else if(key.Equals("Language", comparer)) {
                var isContains = new Func<bool>(() => {
                    if(val.IndexOf('|') != -1) {
                        var vals = val.Split('|');
                        return album.Languages.Any(a => vals.Contains(a, comparer));
                    }
                    return album.Languages.Contains(val, comparer);
                })();

                return connector == ':' ? isContains : !isContains;
            }
            else if(key.Equals("Note", comparer)) {
                return connector == ':' ?
                    (album.Note?.Contains(val, comparer)).GetValueOrDefault() :
                        connector == '=' ?
                    (album.Note?.Equals(val, comparer)).GetValueOrDefault() :
                        false;
            }
            else if(key.Equals("Tier", comparer)) {
                int iVal = int.Parse(val);
                return connector == ':' ? album.Tier == iVal :
                        connector == '>' ? album.Tier > iVal :
                        connector == '<' ? album.Tier < iVal :
                        connector == '!' ? album.Tier != iVal :
                        false;
            }
            else if(key.Equals("IsWip", comparer)) {
                bool bvalue = bool.Parse(val);
                bool isEqual = album.IsWip == bvalue;
                return connector == ':' ? isEqual : !isEqual;
            }
            else if(key.Equals("IsRead", comparer)) {
                bool bvalue = bool.Parse(val);
                bool isEqual = album.IsRead == bvalue;
                return connector == ':' ? isEqual : !isEqual;
            }
            else if(key.Equals("EntryDate", comparer)) {
                DateTime dVal = DateTime.Parse(val);
                return connector == ':' ? album.EntryDate == dVal :
                        connector == '>' ? album.EntryDate > dVal :
                        connector == '<' ? album.EntryDate < dVal :
                        connector == '!' ? album.EntryDate != dVal :
                        false;
            }
            else if(key.Equals("Special", comparer)) {
                if(val.Equals("Tier>0OrNew", comparer)) {
                    return album.Tier > 0 || !album.IsRead;
                }
                if(val.Equals("ToDelete", comparer)) {
                    return ((album.Tier == 0 && string.IsNullOrEmpty(album.Note))
                            || (album.Tier == 1 && album.Note != "HERITAGE"
                                && !album.Artists.Any(artist => featuredArtists.Contains(artist, comparer)))
                        ) && album.IsRead;
                }
                if(val.Equals("Flagged", comparer)) {
                    return !string.IsNullOrEmpty(album.Note)
                        && !(new string[] { "🌟", "HP", "HERITAGE" }).Contains(album.Note);
                }
                if(val.Equals("FeaturedArtist", comparer)) {
                    return connector == ':' ? album.Artists.Any(artist => featuredArtists.Contains(artist, comparer))
                        : connector == '!' ? album.Artists.Any(artist => !featuredArtists.Contains(artist, comparer))
                        : false;
                }
            }
            throw new Exception("Invalid key: " + key);
        }

        foreach(var querySeg in querySegs) {
            if(!IsMatch(album, querySeg)) {
                return false;
            }
        }
        return true;
    }

    public static AlbumVM Get(this List<AlbumVM> albumVMs, string path) {
        return albumVMs.FirstOrDefault(a => a.Path == path);
    }

    public static bool ContainsConnector(string source) {
        return source.IndexOfAny(_cons) > -1;
    }
}

public class QuerySegment
{
    public char Con { get; set; }
    public string Key { get; set; }
    public string Val { get; set; }

    public string Text { 
        get {
            return $"{Key}{Con}{Val}";
        } 
    }
}