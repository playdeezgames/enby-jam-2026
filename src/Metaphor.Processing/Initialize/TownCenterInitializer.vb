Imports Metaphor.Persistence

Friend Module TownCenterInitializer

    Friend Function Initialize(context As IInitializationContext) As LocationInitializer
        Return Sub(location)
                   location.SetName("Town Center")
                   location.SetFlavor("You find yerself in the town center.")
                   location.CreateCharacter(CharacterTypes.NPC, AddressOf InitializeZooperdan)
                   context.CenterTown = location
               End Sub
    End Function

    Private Sub InitializeZooperdan(character As ICharacter)
        character.SetName("Zooperdan the Town Elder")
        character.SetFlavor("""Hello, my friend! Stay a while, and listen!""")
        character.CreateVerb(VerbTypes.CHECK_BUTTHOLE, AddressOf InitializeCheckButthole)
    End Sub

    Private Sub InitializeCheckButthole(verb As IVerb)
        verb.SetName("Check Butthole")
        verb.SetFlavor("For some reason, Zooperdan will not allow you to check his butthole.")
    End Sub
End Module
