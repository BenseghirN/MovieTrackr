// public sealed class TmdbMovieSearchItem
// {
//     public int Id { get; set; }
//     public string Title { get; set; } = "";
//     public string? Original_Title { get; set; } // ou [JsonPropertyName("original_title")]
//     public string? Release_Date { get; set; }   // ex "2016-10-19"
//     public string? Overview { get; set; }
//     public string? Poster_Path { get; set; }
//     public List<int> Genre_Ids { get; set; } = [];
// }

// public sealed class TmdbMovieDetails
// {
//     public int Id { get; set; }
//     public string Title { get; set; } = "";
//     public string? Original_Title { get; set; }
//     public string? Overview { get; set; }
//     public string? Release_Date { get; set; }
//     public int? Runtime { get; set; }
//     public string? Poster_Path { get; set; }
//     public List<TmdbGenre> Genres { get; set; } = [];
//     public TmdbCredits? Credits { get; set; }
// }

// public sealed class TmdbGenre { public int Id { get; set; } public string Name { get; set; } = ""; }

// public class TmdbCredits
// {
//     public List<TmdbCast> Cast { get; set; } = [];
//     public List<TmdbCrew> Crew { get; set; } = [];
// }
// public sealed class TmdbCast { public int Id { get; set; } public string Name { get; set; } = ""; public string? Character { get; set; } public int? Order { get; set; } }
// public sealed class TmdbCrew { public int Id { get; set; } public string Name { get; set; } = ""; public string Job { get; set; } = ""; public string? Department { get; set; } }
