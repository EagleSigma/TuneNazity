using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace TuneNazity.Model
{
    public class Song
    {
        // Tag (in iTunes XML):
        public int Id { get; set; }				// TrackID
        public string Album { get; set; }		// Name
        public string Artist { get; set; }		// Album
        public int BitRate { get; set; }		// BitRate
        public string Comments { get; set; }	// Comments
        public string Composer { get; set; }	// Composer
        public string Genre { get; set; }		// Genre
        public string Kind { get; set; }		// Kind
        public string Location { get; set; }	// Location
        public string Name { get; set; }		// Name
        public int PlayCount { get; set; }		// PlayCount
        public int SampleRate { get; set; }		// SampleRate
        public Int64 Size { get; set; }			// Size
        public Int64 TotalTime { get; set; }	// TotalTime
        public int TrackNumber { get; set; }	// TrackNumber
    }

    public static class XExtensions
    {
        public static int ToInt(this XElement xe, int emptyValue)
        {
            return xe == null ? emptyValue : int.Parse(xe.Value);
        }
        public static Int64 ToInt64(this XElement xe, Int64 emptyValue)
        {
            return xe == null ? emptyValue : Int64.Parse(xe.Value);
        }
        public static string ToString(this XElement xe, string emptyValue)
        {
            return xe == null ? emptyValue : xe.Value;
        }
    }

 }
