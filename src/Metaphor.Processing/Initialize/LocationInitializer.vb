Imports Metaphor.Persistence

Friend Module LocationInitializer
    Friend Function Initialize(context As IInitializationContext) As Persistence.LocationInitializer
        Return Sub(location)
                   context.Location = location
                   location.CreateCharacter(CharacterTypes.GWEN, context.ChosenName, "Yer pronouns are they/them. Knowing Finnish won't help here.", InitializeAvatar(context))
               End Sub
    End Function

    Private Function InitializeAvatar(context As IInitializationContext) As CharacterInitializer
        Return Sub(character)
                   character.InitializeCounter(Counters.HEALTH, 100, 0, 100)
                   character.InitializeCounter(Counters.SATIETY, 100, 0, 100)
                   character.InitializeCounter(Counters.STOMACH, 0, 0, 50)
                   character.InitializeCounter(Counters.JOOLS, 0, 0, Integer.MaxValue)
                   character.SetCounter(Counters.DISTANCE_REMAINING, 2000)
                   character.InitializeCounter(Counters.PACE, 3, 1, 5)
                   character.World.Avatar = character
               End Sub
    End Function
End Module
