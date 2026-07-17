Imports Metaphor.Persistence

Friend Module LocationInitializer
    Friend Function Initialize(context As IInitializationContext) As Persistence.LocationInitializer
        Return Sub(location)
                   context.Location = location
                   location.CreateCharacter(CharacterTypes.GWEN, context.ChosenName, $"{context.ChosenName}'s pronouns are they/them. Knowing Finnish won't help here.", InitializeAvatar(context))
               End Sub
    End Function

    Private Function InitializeAvatar(context As IInitializationContext) As CharacterInitializer
        Return Sub(character)
                   character.InitializeCounter(Counters.HEALTH, 100, 0, 100)
                   character.InitializeCounter(Counters.SATIETY, 100, 0, 100)
                   character.InitializeCounter(Counters.STOMACH, 0, 0, 50)
                   character.InitializeCounter(Counters.JOOLS, 0, 0, Integer.MaxValue)
                   character.InitializeCounter(Counters.SNAX, 10, 0, Integer.MaxValue)
                   character.InitializeCounter(Counters.DISTANCE_REMAINING, 2000, 0, Integer.MaxValue)
                   character.InitializeCounter(Counters.PACE, 3, 1, 5)
                   character.SetPronouns(context.ChosenPronouns)
                   character.CreateVerb(VerbTypes.CONTINUE_JOURNEY, "Continue Journey...", "The road goes ever on and on.")
                   character.CreateVerb(VerbTypes.COMPLETE_JOURNEY, "Complete Journey", "It is a privilege to see something end. You then get to either remember it fondly, or be glad that it's over. Sometimes, it's a bit of both. Life is messy that way.")
                   character.CreateVerb(VerbTypes.CHANGE_PACE, "Change Pace...", "New York City? (Get a rope!)")
                   character.World.Avatar = character
               End Sub
    End Function
End Module
