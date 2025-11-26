using MovieTrackR.Domain.Enums;

namespace MovieTrackR.Domain.Helpers;

public static class CrewHelpers
{
    private static readonly HashSet<CrewJob> ImportantJobs = new()
    {
        CrewJob.Director,
        CrewJob.Writer,
        CrewJob.Screenplay,
        CrewJob.Producer,
        CrewJob.OriginalMusicComposer,
        CrewJob.DirectorOfPhotography,
        CrewJob.Editor
    };

    public static bool IsImportantJob(string? job)
    {
        CrewJob crewJob = ParseJob(job);
        return ImportantJobs.Contains(crewJob);
    }

    public static int GetJobPriority(string? job)
    {
        CrewJob crewJob = ParseJob(job);
        return crewJob switch
        {
            CrewJob.Director => 0,
            CrewJob.Writer => 1,
            CrewJob.Screenplay => 1,
            CrewJob.Producer => 2,
            CrewJob.DirectorOfPhotography => 3,
            CrewJob.OriginalMusicComposer => 4,
            CrewJob.Editor => 5,
            _ => 99
        };
    }

    public static CrewJob ParseJob(string? job)
    {
        if (string.IsNullOrWhiteSpace(job))
            return CrewJob.Other;

        return job.ToLowerInvariant().Replace(" ", "") switch
        {
            "director" => CrewJob.Director,
            "assistantdirector" => CrewJob.AssistantDirector,
            "writer" => CrewJob.Writer,
            "screenplay" => CrewJob.Screenplay,
            "story" => CrewJob.Story,
            "producer" => CrewJob.Producer,
            "executiveproducer" => CrewJob.ExecutiveProducer,
            "coproducer" => CrewJob.CoProducer,
            "associateproducer" => CrewJob.AssociateProducer,
            "lineproducer" => CrewJob.LineProducer,
            "directorofphotography" => CrewJob.DirectorOfPhotography,
            "cinematography" => CrewJob.Cinematography,
            "editor" => CrewJob.Editor,
            "originalmusiccomposer" => CrewJob.OriginalMusicComposer,
            "musiccomposer" => CrewJob.MusicComposer,
            "sounddesigner" => CrewJob.SoundDesigner,
            "productiondesign" => CrewJob.ProductionDesign,
            "artdirection" => CrewJob.ArtDirection,
            "setdecoration" => CrewJob.SetDecoration,
            "decorator" => CrewJob.Decorator,
            "costumedesigner" => CrewJob.CostumeDesigner,
            "makeupartist" => CrewJob.MakeupArtist,
            "visualeffects" => CrewJob.VisualEffects,
            "animation" => CrewJob.Animation,
            "stuntcoordinator" => CrewJob.StuntCoordinator,
            "colorgrading" => CrewJob.ColorGrading,
            _ => CrewJob.Other
        };
    }

    public static CrewDepartment ParseDepartment(string? department)
    {
        if (string.IsNullOrWhiteSpace(department))
            return CrewDepartment.Other;

        return department.ToLowerInvariant().Replace(" ", "").Replace("&", "and") switch
        {
            "directing" => CrewDepartment.Directing,
            "writing" => CrewDepartment.Writing,
            "production" => CrewDepartment.Production,
            "camera" => CrewDepartment.Camera,
            "editing" => CrewDepartment.Editing,
            "sound" => CrewDepartment.Sound,
            "art" => CrewDepartment.Art,
            "costumeandmakeup" or "costumeandmake-up" => CrewDepartment.CostumeAndMakeUp,
            "visualeffects" => CrewDepartment.VisualEffects,
            "crew" => CrewDepartment.Crew,
            _ => CrewDepartment.Other
        };
    }

    public static string GetJobDisplayName(CrewJob job)
    {
        return job switch
        {
            CrewJob.Director => "Director",
            CrewJob.AssistantDirector => "Assistant Director",
            CrewJob.Writer => "Writer",
            CrewJob.Producer => "Producer",
            CrewJob.DirectorOfPhotography => "Director of Photography",
            CrewJob.Editor => "Editor",
            CrewJob.OriginalMusicComposer => "Original Music Composer",
            CrewJob.CostumeDesigner => "Costume Designer",
            CrewJob.StuntCoordinator => "Stunt Coordinator",
            CrewJob.ColorGrading => "Color Grading",
            _ => job.ToString()
        };
    }
}